namespace backend.Controllers

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open BCrypt.Net
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
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
                    let accessToken = tokenService.GenerateToken(user)

                    let refreshToken = tokenService.GenerateRefreshToken()
                    do! userService.SaveRefreshToken(user.Username, refreshToken, 7)

                    this.Response.Cookies.Append(
                        "refreshToken",
                        refreshToken,
                        CookieOptions(
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddDays(7.0)
                        )
                    )

                    return this.Ok({| accessToken = accessToken |}) :> IActionResult
                else
                    return this.NotFound("Invalid credentials") :> IActionResult
            | None -> return this.NotFound("User not found") :> IActionResult
        }

    [<HttpPost("reg")>]
    member _.Register([<FromBody>] req: RegisterRequest) =
        task {
            let hashed = BCrypt.HashPassword(req.Password)
            let! result = userService.CreateUser(req.Username, req.DisplayName, hashed)

            return
                match result with
                | Ok username -> this.Ok({| Username = username |}) :> IActionResult
                | Error msg ->
                    match msg with
                    | 1 -> this.Conflict(msg) :> IActionResult
                    | _ -> this.BadRequest(msg) :> IActionResult
        }

    [<HttpPost("refresh")>]
    member _.Refresh() =
        task {
            let refreshToken = this.Request.Cookies.["refreshToken"]

            if String.IsNullOrEmpty(refreshToken) then
                return this.Unauthorized() :> IActionResult
            else
                let! tokenOpt = userService.GetRefreshToken(refreshToken)

                match tokenOpt with
                | None -> return this.Unauthorized() :> IActionResult
                | Some token ->
                    let! userOpt = userService.GetByUsername(token.Username)

                    match userOpt with
                    | None -> return this.Unauthorized() :> IActionResult
                    | Some user ->
                        let newAccessToken = tokenService.GenerateToken(user)
                        let newRefreshToken = tokenService.GenerateRefreshToken()
                        let! _ = userService.SaveRefreshToken(user.Username, newRefreshToken, 7)

                        this.Response.Cookies.Append(
                            "refreshToken",
                            newRefreshToken,
                            CookieOptions(HttpOnly = true, Secure = true)
                        )

                        return this.Ok({| accessToken = newAccessToken |}) :> IActionResult
        }

    [<HttpPost("logout")>]
    [<Authorize>]
    member this.Logout() =
        task {
            let refreshToken = this.Request.Cookies.["refreshToken"]
            
            if not (String.IsNullOrEmpty(refreshToken)) then
                do! userService.RevokeRefreshToken(refreshToken)
                this.Response.Cookies.Delete("refreshToken")

            return this.NoContent() :> IActionResult
        }