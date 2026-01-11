namespace backend.Controllers

open Microsoft.AspNetCore.Mvc
open backend.Models
open backend.Services

[<ApiController>]
[<Route("[controller]")>]
[<Microsoft.AspNetCore.Authorization.Authorize>]
type CardsController(cardService: ICardService) as this =
    inherit ControllerBase()

    [<HttpGet>]
    member _.GetMyCards() =
        task {
            let username = this.User.Identity.Name
            return! cardService.GetAllForUser username
        }
    
    [<HttpPost>]
    member _.CreateCard([<FromBody>] req: CreateCardRequest) =
        task {
            return! cardService.Create(req, this.User.Identity.Name)
        }
    
    [<HttpPost("/update_all")>]
    member _.UpdateAllCards([<FromBody>] req: UpdateCardsRequest) =
        task {
            return cardService.UpdateAll(req, this.User.Identity.Name)
        }
    
    [<HttpPost("{id:int}/links")>]
    member _.AddLink([<FromBody>] link: CreateLinkRequest, [<FromRoute>] id: int) =
        task {
            return! cardService.AddLinks([link], id)
        }
        
    [<HttpPost("{id:int}/tag")>]
    member _.AddTag([<FromBody>] link: CreateLinkRequest, [<FromRoute>] id: int) =
        task {
            return! cardService.AddLinks([link], id)
        }
