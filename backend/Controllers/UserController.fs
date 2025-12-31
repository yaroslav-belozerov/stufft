namespace backend.Controllers

open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open backend.Services

[<ApiController>]
[<Route("[controller]")>]
// [<Microsoft.AspNetCore.Authorization.Authorize>]
type UsersController(userService: IUserService) as this =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        task {
            let! result = userService.GetAllUsers()
            return this.Ok(result)
        }