namespace backend.Controllers

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open backend.Models
open backend.Services

[<ApiController>]
[<Route("[controller]")>]
[<Microsoft.AspNetCore.Authorization.Authorize>]
type UserController(userService: IUserService) as this =
    inherit ControllerBase()

    [<HttpGet>]
    member _.GetMyProfile() =
        task {
            let username = this.User.Identity.Name

            if String.IsNullOrEmpty(username) then
                return this.Unauthorized() :> IActionResult
            else
                let! userOpt = userService.GetByUsername(username)

                return
                    match userOpt with
                    | Some user -> this.Ok(UserMapping.fromFullUser user) :> IActionResult
                    | None -> this.NotFound() :> IActionResult
        }