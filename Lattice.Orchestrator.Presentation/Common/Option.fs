module Option

open System

let ofString str =
    match String.IsNullOrWhiteSpace str with
    | true -> None
    | false -> Some str
