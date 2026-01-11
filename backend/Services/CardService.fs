namespace backend.Services

open System.Collections.Generic
open System.Threading.Tasks
open Dapper
open Npgsql
open backend.Models


type ICardService =
    abstract member Create: CreateCardRequest * string -> Task<Card list>
    abstract member DeleteById: int -> Task<int>
    abstract member GetAllForUser: string -> Task<Card list>
    abstract member AddLinks: CreateLinkRequest list * int -> Task<unit>
    abstract member UpdateAll: UpdateCardsRequest * string -> Task<Card list>
    
type CardService(connectionString: string) =
    let getAllForUser(username: string) =
        task {
            use connection = new NpgsqlConnection(connectionString)
    
            let sql = "
                SELECT 
                    c.id, c.title, c.text_content AS TextContent, c.created_at AS CreatedAt, c.parent_username AS ParentUsername,
                    l.id, l.type, l.parent_card AS ParentId,
                    t.id, t.content
                FROM cards c
                LEFT JOIN links l ON c.id = l.parent_card
                LEFT JOIN card_tags ct ON c.id = ct.card_id
                LEFT JOIN tags t ON ct.tag_id = t.id
                WHERE c.parent_username = @username"

            let cardDict = Dictionary<int, Card>()

            let! _ = 
                connection.QueryAsync<Card, Link, Tag, Card>(
                    sql, 
                    (fun card link tag ->
                        let currentCard = 
                            match cardDict.TryGetValue(card.Id) with
                            | true, existing -> existing
                            | _ -> 
                                let newCard = { card with Links = ResizeArray(); Tags = ResizeArray() }
                                cardDict.[card.Id] <- newCard
                                newCard
                        
                        if box link <> null && not (currentCard.Links |> Seq.exists (fun l -> l.Id = link.Id)) then
                            currentCard.Links.Add(link)
                        
                        if box tag <> null && not (currentCard.Tags |> Seq.exists (fun t -> t.Id = tag.Id)) then
                            currentCard.Tags.Add(tag)
                        
                        currentCard
                    ),
                    {| username = username |},
                    splitOn = "id,id"
                )

            return cardDict.Values |> Seq.toList
        }
    
    interface ICardService with
        member _.DeleteById(id: int) =
            task {
                use connection = new NpgsqlConnection(connectionString)
                
                let sql = "DELETE FROM cards WHERE id = @id RETURNING id"
                let! id = connection.ExecuteScalarAsync<int>(sql, {|id = id|}) 
                return id
            }
        
        member _.GetAllForUser(username: string) = getAllForUser(username)
            
        member _.Create(req: CreateCardRequest, username: string) =
            task {
                use connection = new NpgsqlConnection(connectionString)
                
                let sql = "INSERT INTO cards (title, text_content, parent_username) VALUES (@title, @textContent, @username) RETURNING id"
                let! _ = connection.ExecuteScalarAsync<int>(sql, {|title = req.Title; textContent = req.TextContent; username = username|})
                return! getAllForUser(username) 
            }
           
        member _.UpdateAll(cards: UpdateCardsRequest, username: string) =
            task {
                use connection = new NpgsqlConnection(connectionString)
                use transaction = connection.BeginTransaction()
                let sql = "UPDATE links SET (title, text_content) VALUES (@title, @textContent) WHERE id = @id"
                let cardsToUpdate = cards.Cards |> Seq.toArray
                if cardsToUpdate.Length > 0 then
                    let! _ = connection.ExecuteAsync(sql, cardsToUpdate, transaction)
                    ()
                transaction.Commit()
                return! getAllForUser(username) 
            }
        
        member _.AddLinks(links: CreateLinkRequest list, parentCardId: int) =
            task {
                use connection = new NpgsqlConnection(connectionString)
                use transaction = connection.BeginTransaction()
                let sql = "INSERT INTO links (type, content, parent_card) VALUES (@type, @content, @parentCardId)"
                let linksToInsert = 
                    links
                        |> List.map (fun l -> {| Type = l.Type; Content = l.Content; ParentCardId = parentCardId |})
                        |> Seq.toArray

                if linksToInsert.Length > 0 then
                    let! _ = connection.ExecuteAsync(sql, linksToInsert, transaction)
                    ()
                transaction.Commit()
            }
