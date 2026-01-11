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
            .WithColumn("username").AsString(255).NotNullable()
            .WithColumn("expires_at").AsDateTime().NotNullable()
            .WithColumn("is_revoked").AsBoolean().NotNullable().WithDefaultValue(false) |> ignore
        this.Create.ForeignKey("FK_refresh_tokens_username_users_username")
               .FromTable("refresh_tokens").ForeignColumn("username")
               .ToTable("users").PrimaryColumn("username")
               .OnDeleteOrUpdate(System.Data.Rule.Cascade);

    override this.Down() =
        this.Delete.Table("refresh_tokens") |> ignore


[<Migration(2026140123000L)>]
type CreateCardAndLinksTable() as this =
    inherit Migration()

    override _.Up() =
        this.Create.Table("links")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("type").AsByte().NotNullable()
            .WithColumn("content").AsString(255).NotNullable()
            .WithColumn("parent_card").AsInt32().NotNullable() |> ignore
        this.Create.Table("cards")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("text_content").AsString(255).NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("parent_username").AsString(255).NotNullable() |> ignore
        this.Create.ForeignKey("FK_links_parent_card_cards_id")
               .FromTable("links").ForeignColumn("parent_card")
               .ToTable("cards").PrimaryColumn("id")
               .OnDeleteOrUpdate(System.Data.Rule.Cascade)
               
        this.Create.Table("tags")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("content").AsString(255).Unique().NotNullable() |> ignore
        this.Create.Table("card_tags")
            .WithColumn("card_id").AsInt32().PrimaryKey()
            .WithColumn("tag_id").AsInt32().PrimaryKey() |> ignore
        this.Create.ForeignKey("FK_card_tags_card")
            .FromTable("card_tags").ForeignColumn("card_id")
            .ToTable("cards").PrimaryColumn("id").OnDelete(System.Data.Rule.Cascade) |> ignore
        this.Create.ForeignKey("FK_card_tags_tag")
            .FromTable("card_tags").ForeignColumn("tag_id")
            .ToTable("tags").PrimaryColumn("id").OnDelete(System.Data.Rule.Cascade) |> ignore
            
        this.Create.ForeignKey("FK_cards_parent_username_users_username")
               .FromTable("cards").ForeignColumn("parent_username")
               .ToTable("users").PrimaryColumn("username")
               .OnDeleteOrUpdate(System.Data.Rule.Cascade);


    override _.Down() =
        this.Delete.Table("cards") |> ignore
