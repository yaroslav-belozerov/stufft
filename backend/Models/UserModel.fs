namespace backend.Models

[<CLIMutable>]
type FullUser = {
    Username: string
    Email: string
    CreatedAt: System.DateTime
    PasswordHash: string
}

[<CLIMutable>]
type User = {
    Username: string
    Email: string
    CreatedAt: System.DateTime
}

[<CLIMutable>]
type LoginRequest = {
    Username: string
    Password: string
}

[<CLIMutable>]
type RegisterRequest = {
    Username: string
    Email: string
    Password: string
}
