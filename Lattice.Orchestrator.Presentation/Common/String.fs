module String

open System

let defaultValue (value: string) str = 
    match String.IsNullOrWhiteSpace str with
    | true -> value
    | false -> str

// TODO: Handle proper validation on payloads and return 400 if invalid
