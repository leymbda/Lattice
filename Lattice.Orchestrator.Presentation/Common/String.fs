module String

open System

let defaultValue (value: string) str = 
    match String.IsNullOrWhiteSpace str with
    | true -> value
    | false -> str
