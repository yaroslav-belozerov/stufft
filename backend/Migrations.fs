namespace backend.Migrations

open FluentMigrator

[<Migration(2025123120000L)>]
type CreateUserTable() as this =
    inherit Migration()

    override _.Up() =
        this.Create.Table("users")
            .WithColumn("username").AsString(255).PrimaryKey()
            .WithColumn("email").AsString(255).NotNullable()
            .WithColumn("password_hash").AsString(255).NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime) |> ignore

    override _.Down() =
        this.Delete.Table("users") |> ignore

