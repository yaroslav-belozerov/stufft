namespace backend.Migrations

open FluentMigrator

[<Migration(2025123120000L)>]
type CreateUserTable() as this =
    inherit Migration()

    override _.Up() =
        this.Create.Table("users")
            .WithColumn("username").AsString(255).PrimaryKey()
            .WithColumn("display_name").AsString(255).NotNullable()
            .WithColumn("password_hash").AsString(255).NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime) |> ignore

    override _.Down() =
        this.Delete.Table("users") |> ignore

[<Migration(2025123120003L)>]
type AddRefreshTokensTable() =
    inherit Migration()
    override this.Up() =
        this.Create.Table("refresh_tokens")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("token").AsString().NotNullable().Unique()
            .WithColumn("username").AsString(255).NotNullable().ForeignKey("users", "username")
            .WithColumn("expires_at").AsDateTime().NotNullable()
            .WithColumn("is_revoked").AsBoolean().NotNullable().WithDefaultValue(false) |> ignore

    override this.Down() =
        this.Delete.Table("refresh_tokens") |> ignore