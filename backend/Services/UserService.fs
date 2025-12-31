namespace backend.Services

open System.Collections.Generic
open System.Threading.Tasks
open Dapper
open Npgsql
open backend.Models

type IUserService =
    abstract member GetAllUsers : unit -> Task<IEnumerable<User>>
    abstract member GetByUsername: string -> Task<FullUser option>
    abstract member CreateUser : string * string * string -> Task<Result<string, int>> 

type UserService(connectionString: string) =
    interface IUserService with
        member _.GetAllUsers() =
            task {
                use connection = new NpgsqlConnection(connectionString)
                return! connection.QueryAsync<User>("SELECT username, email, created_at as CreatedAt FROM users")
            }
            
        member _.GetByUsername(username: string) =
            task {
                use connection = new NpgsqlConnection(connectionString)
                let user = connection.QueryFirstOrDefault<FullUser>("SELECT username, email, created_at as CreatedAt, password_hash as PasswordHash FROM users WHERE username = @username", {|username = username|})
                return if isNull (box user) then None else Some user
            }
            
        member _.CreateUser(username: string, email: string, passwordHash: string) =
            task {
                try
                    use conn = new NpgsqlConnection(connectionString)
                    
                    let sql = "
                        INSERT INTO users (username, email, password_hash) 
                        VALUES (@username, @email, @passwordHash) 
                        RETURNING username"
                    
                    let parameters = {| 
                        username = username
                        email = email
                        passwordHash = passwordHash 
                    |}

                    let! newId = conn.ExecuteScalarAsync<string>(sql, parameters)
                    return Ok newId

                with
                | :? PostgresException as ex when ex.SqlState = "23505" -> 
                    return Error 1 
                | ex ->
                    return Error -1
            }

