module Lattice.WorkerNode.Program

open Elmish
open FsToolkit.ErrorHandling
open System

[<RequireQualifiedAccess>]
type ExitCode =
    | Success = 0

[<EntryPoint>]
let run args =
    let nodeId =
        args
        |> Array.tryExactlyOne
        |> Option.bind (fun v -> Guid.TryParse v |> function | true, id -> Some id | _ -> None)
        |> Option.teeNone (fun _ -> printfn "No ID provided or provided ID not a valid Guid, generating random...")
        |> Option.defaultValue (Guid.NewGuid())

    printfn "Starting node %A..." nodeId

    Program.mkProgram Node.init Node.update (fun _ _ -> ())
    |> Program.withSubscription Node.subscribe
    |> Program.runWith nodeId

    printfn "Program finished."
    int ExitCode.Success
    