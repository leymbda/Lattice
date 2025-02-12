module Lattice.Web.Api

open System.Net.Http
open System.Text
open System.Text.Json
open System.Text.Json.Nodes

// TODO: Can types be auto-generated from the api project?

module Json =
    let add (key: string) (value: 'a) (json: JsonObject) =
        json.Add(key, JsonValue.Create value)
        json

    let get<'a> (key: string) (json: JsonNode) =
        json[key].GetValue<'a>()

let [<Literal>] BASE_URL = "http://localhost:7071/api/" // TODO: Configure in environment

let login (code: string) (redirectUri: string) (client: HttpClient) = task {
    let json =
        JsonObject()
        |> Json.add "code" code
        |> Json.add "redirectUri" redirectUri

    use content = new StringContent(JsonSerializer.Serialize json, Encoding.UTF8, "application/json")

    match! client.PostAsync(BASE_URL + "auth/login", content) with
    | res when not res.IsSuccessStatusCode -> return Error res.StatusCode
    | res ->
        let! body = res.Content.ReadAsStringAsync()
        let json = JsonObject.Parse body

        return Ok {|
            Token = json |> Json.get<string> "token"
        |}
}
