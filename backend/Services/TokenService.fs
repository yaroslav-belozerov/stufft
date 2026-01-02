namespace backend.Services

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open Microsoft.IdentityModel.Tokens
open Microsoft.Extensions.Configuration
open backend.Models

type TokenService(config: IConfiguration) =
    member _.GenerateToken(user: FullUser) =
        let claims = [|
            Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
            Claim(JwtRegisteredClaimNames.Name, user.Username)
            Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        |]

        let key = SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.["Jwt:Key"]))
        let creds = SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        
        let token = JwtSecurityToken(
            issuer = config.["Jwt:Issuer"],
            audience = config.["Jwt:Audience"],
            claims = claims,
            expires = DateTime.Now.AddMinutes(15.0),
            signingCredentials = creds
        )

        JwtSecurityTokenHandler().WriteToken(token)
        
    member _.GenerateRefreshToken() =
        let randomNumber = Array.zeroCreate<byte> 32
        using (System.Security.Cryptography.RandomNumberGenerator.Create()) (fun rng ->
            rng.GetBytes(randomNumber)
            Convert.ToBase64String(randomNumber)
        )