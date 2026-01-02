namespace backend.Services

open System
open System.Collections.Generic
open System.Threading.Tasks
open Dapper
open Npgsql
open backend.Models

type IUserService =
    abstract member GetAllUsers: unit -> Task<IEnumerable<User>>
    abstract member GetByUsername: string -> Task<FullUser option>
    abstract member CreateUser: string * string * string -> Task<Result<string, int>>
    abstract member SaveRefreshToken: string * string * int -> Task<unit>
    abstract member GetRefreshToken: string -> Task<RefreshToken option>
    abstract member RevokeRefreshToken : token:string -> Task<unit>

type UserService(connectionString: string) =
    interface IUserService with
        member _.GetAllUsers() =
            task {
                use connection = new NpgsqlConnection(connectionString)
                return! connection.QueryAsync<User>("SELECT username, display_name, created_at as CreatedAt FROM users")
            }

        member _.GetByUsername(username: string) =
            task {
                use connection = new NpgsqlConnection(connectionString)

                let user =
                    connection.QueryFirstOrDefault<FullUser>(
                        "SELECT username, display_name, created_at as CreatedAt, password_hash as PasswordHash FROM users WHERE username = @username",
                        {| username = username |}
                    )

                return if isNull (box user) then None else Some user
            }

        member _.CreateUser(username: string, displayName: string, passwordHash: string) =
            task {
                try
                    use conn = new NpgsqlConnection(connectionString)

                    let sql =
                        "
                        INSERT INTO users (username, display_name, password_hash) 
                        VALUES (@username, @displayName, @passwordHash) 
                        RETURNING username"

                    let parameters =
                        {| username = username
                           displayName = displayName
                           passwordHash = passwordHash |}

                    let! newId = conn.ExecuteScalarAsync<string>(sql, parameters)
                    return Ok newId

                with
                | :? PostgresException as ex when ex.SqlState = "23505" -> return Error 1
                | ex -> return Error -1
            }

        member _.SaveRefreshToken(username: string, token: string, days: int) =
            task {
                use conn = new NpgsqlConnection(connectionString)

                let sql =
                    "INSERT INTO refresh_tokens (username, token, expires_at) VALUES (:username, :token, :expiresAt)"

                let! _ =
                    conn.ExecuteAsync(
                        sql,
                        {| Username = username 
                           token = token
                           expiresAt = DateTime.UtcNow.AddDays(float days) |}
                    )

                return ()
            }

        member _.GetRefreshToken(token: string) =
            task {
                use conn = new NpgsqlConnection(connectionString)

                let sql =
                    "SELECT * FROM refresh_tokens WHERE token = :token AND is_revoked = false AND expires_at > :now"

                let! result =
                    conn.QueryFirstOrDefaultAsync<RefreshToken>(
                        sql,
                        {| token = token
                           now = DateTime.UtcNow |}
                    )

                return if isNull (box result) then None else Some result
            }
        member _.RevokeRefreshToken(token: string) =
            task {
                use conn = new NpgsqlConnection(connectionString)
                
                let sql = "
                    UPDATE refresh_tokens 
                    SET is_revoked = true 
                    WHERE token = :token"
                
                let! _ = conn.ExecuteAsync(sql, {| token = token |})
                return ()
            }
