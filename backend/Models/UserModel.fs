namespace backend.Models

[<CLIMutable>]
type FullUser = {
    Username: string
    DisplayName: string
    CreatedAt: System.DateTime
    PasswordHash: string
}

[<CLIMutable>]
type User = {
    Username: string
    DisplayName: string
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
    DisplayName: string
    Password: string
}
