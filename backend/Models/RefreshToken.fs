namespace backend.Models

open System

[<CLIMutable>]
type RefreshToken = {
    Id: int
    Token: string
    Username: string
    ExpiresAt: DateTime
    IsRevoked: bool
}