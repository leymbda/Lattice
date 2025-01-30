namespace Lattice.Orchestrator.Application

open System.Security.Cryptography
open System
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open System.Text.RegularExpressions

type JwtHeader = {
    [<JsonPropertyName "alg">] Algorithm: string
    [<JsonPropertyName "typ">] Type: string
}

module JwtHeaader =
    let create algorithm = {
        Algorithm = algorithm
        Type = "JWT"
    }

type Jwt<'a> = {
    Claims: 'a
}

module Jwt =
    let [<Literal>] ALGORITHM = "HS256"

    let create claims = {
        Claims = claims
    }

    let encode (key: string) jwt =
        use hmac = new HMACSHA256(Encoding.UTF8.GetBytes key)

        let header =
            JwtHeaader.create ALGORITHM
            |> JsonSerializer.Serialize
            |> Encoding.UTF8.GetBytes
            |> Convert.ToBase64String

        let payload =
            jwt.Claims
            |> JsonSerializer.Serialize
            |> Encoding.UTF8.GetBytes
            |> Convert.ToBase64String

        let signature =
            header + "." + payload
            |> Encoding.UTF8.GetBytes
            |> hmac.ComputeHash
            |> Convert.ToBase64String

        $"{header}.{payload}.{signature}"

    let decode<'a> (key: string) jwt =
        use hmac = new HMACSHA256(Encoding.UTF8.GetBytes key)

        let matches = Regex.Match(jwt, "^(?<header>[a-zA-Z0-9+\/=]+)\.(?<payload>[a-zA-Z0-9+\/=]+)\.(?<signature>[a-zA-Z0-9+\/=]+)$")

        match matches.Success with
        | false -> None
        | true ->
            let header = matches.Groups["header"].Value
            let payload = matches.Groups["payload"].Value
            let signature = matches.Groups["signature"].Value

            let actualSignature = 
                header + "." + payload
                |> Encoding.UTF8.GetBytes
                |> hmac.ComputeHash
                |> Convert.ToBase64String

            match signature = actualSignature with
            | false -> None
            | true -> Some (JsonSerializer.Deserialize<'a> payload)

    let verify (key: string) (jwt: string) =
        match decode key jwt with
        | None -> false
        | Some _ -> true
