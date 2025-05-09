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
    |> Program.withTermination (function | Node.Msg.OnDisconnect -> true | _ -> false) (fun _ -> ())
    |> Program.runWith nodeId

    // TODO: Test to ensure termination works as expected
    // TODO: On disconnect, should the node attempt to reconnect? Maybe try before shutting down shards? TBD

    printfn "Program finished."
    int ExitCode.Success
    