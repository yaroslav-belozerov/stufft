namespace backend.Controllers

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open BCrypt.Net
open Microsoft.AspNetCore.Http.HttpResults
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.IdentityModel.Tokens
open backend.Models
open backend.Services

[<ApiController>]
[<Route("[controller]")>]
type AuthController(userService: IUserService, tokenService: TokenService) as this =
    inherit ControllerBase()
    
    [<HttpPost("login")>]
    member _.Login([<FromBody>] req: LoginRequest) =
        task {
            let! userOpt = userService.GetByUsername(req.Username)
            
            match userOpt with
            | Some user ->
                if BCrypt.Verify(req.Password, user.PasswordHash) then
                    let token = tokenService.GenerateToken(user)
                    return this.Ok({| token = token |}) :> IActionResult
                else
                    return this.NotFound("Invalid credentials") :> IActionResult
            | None -> 
                return this.NotFound("User not found") :> IActionResult
        }
       
    [<HttpPost("reg")>]
    member _.Register([<FromBody>] req: RegisterRequest) =
        task {
            let hashed = BCrypt.HashPassword(req.Password)
            let! result = userService.CreateUser(req.Username, req.Email, hashed)

            return 
                match result with
                | Ok username -> this.Ok({| Username = username |}) :> IActionResult
                | Error msg -> match msg with | 1 -> this.Conflict(msg) :> IActionResult | _ -> this.BadRequest(msg) :> IActionResult
        }
