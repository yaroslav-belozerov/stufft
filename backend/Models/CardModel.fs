namespace backend.Models

type LinkType =
    | Url = 'U'
    | Image = 'I'

[<CLIMutable>]
type Tag = {
    Id: int
    Content: string
}

[<CLIMutable>]
type CreateTagRequest = {
    Content: string
}

[<CLIMutable>]
type Link = {
    Id: int
    Type: LinkType
    Content: string
    ParentId: int
}

[<CLIMutable>]
type CreateLinkRequest = {
    Type: string 
    Content: string
}

[<CLIMutable>]
type Card = {
    Id: int
    Title: string
    TextContent: string
    Links: ResizeArray<Link>
    Tags: ResizeArray<Tag>
    CreatedAt: string
    ParentUsername: string
}

[<CLIMutable>]
type CreateCardRequest = {
    Title: string
    TextContent: string
}

type UpdateCardRequest = {
    Id: int
    Title: string
    TextContent: string
}

[<CLIMutable>]
type UpdateCardsRequest = {
    Cards: UpdateCardRequest[]
}