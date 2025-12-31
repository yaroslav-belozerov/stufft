namespace backend
#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open FluentMigrator.Runner
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text
open backend.Migrations
open backend.Services

module Program =
    let upgradeDatabase (services: IServiceProvider) =
        let runner = services.GetRequiredService<IMigrationRunner>()
        runner.MigrateUp()
    
    let exitCode = 0

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)
        
        let connString = builder.Configuration.["Database:Url"]
        if String.IsNullOrEmpty connString then
            failwith "Database connection string is missing!"
            
        builder.Services.AddSingleton<TokenService>() |> ignore

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun options ->
                options.TokenValidationParameters <- TokenValidationParameters(
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration.["Jwt:Issuer"],
                    ValidAudience = builder.Configuration.["Jwt:Audience"],
                    IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.["Jwt:Key"]))
                )
            ) |> ignore
            
        builder.Services
            .AddFluentMigratorCore()
            .ConfigureRunner(fun rb ->
                rb.AddPostgres()
                  .WithGlobalConnectionString(connString)
                  .ScanIn(typeof<CreateUserTable>.Assembly).For.Migrations() |> ignore)
            .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore)
            |> ignore
        
        builder.Services.AddScoped<IUserService, UserService>(fun sp -> 
            UserService(connString)) |> ignore

        builder.Services.AddControllers()
        
        let app = builder.Build()
        
        using (app.Services.CreateScope()) (fun scope ->
            upgradeDatabase scope.ServiceProvider
        )

        app.UseHttpsRedirection()

        app.UseAuthentication()
        app.UseAuthorization()
        app.MapControllers()
        app.UseStaticFiles()

        app.Run()

        exitCode