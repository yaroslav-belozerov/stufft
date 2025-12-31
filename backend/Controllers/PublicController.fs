namespace backend.Controllers

open System
open System.IO
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration

[<ApiController>]
[<Route("[controller]")>]
// [<Microsoft.AspNetCore.Authorization.Authorize>]
type PublicController(configuration: IConfiguration, environment: IWebHostEnvironment) as this =
    inherit ControllerBase()

    [<HttpPost("upload")>]
    member _.Upload(file: IFormFile) =
        async {
            if not (file = null) then
                let fileName = $"{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}"
                let filePath = Path.Combine(environment.WebRootPath, "images", fileName)
                let res = using (File.Create(filePath)) (fun stream -> file.CopyToAsync(stream))
                res.Wait()
                return this.Ok(Path.Combine("images", fileName)) :> IActionResult
            else
                return this.BadRequest() :> IActionResult
        }