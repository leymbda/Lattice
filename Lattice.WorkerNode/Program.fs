module Lattice.WorkerNode.Program

open Elmish
open FsToolkit.ErrorHandling
open System
open System.Threading
open System.Threading.Tasks

[<RequireQualifiedAccess>]
type ExitCode =
    | Success = 0

[<EntryPoint>]
let run args =
    use cts = new CancellationTokenSource()

    let nodeId =
        args
        |> Array.tryExactlyOne
        |> Option.bind (fun v -> Guid.TryParse v |> function | true, id -> Some id | _ -> None)
        |> Option.teeNone (fun _ -> printfn "No ID provided or provided ID not a valid Guid, generating random...")
        |> Option.defaultValue (Guid.NewGuid())

    printfn "Starting node %A..." nodeId

    Program.mkProgram Node.init Node.update (fun _ _ -> ())
    |> Program.withSubscription Node.subscribe
    |> Program.withTermination Node.terminate (fun _ -> cts.Cancel())
    |> Program.withConsoleTrace
    |> Program.runWith nodeId

    task {
        while not cts.Token.IsCancellationRequested do
            do! Task.Delay 1000
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore

    printfn "Program finished."
    int ExitCode.Success
    
    // TODO: This cancellation token termination is messy, how should this be done properly?
    // TODO: On disconnect, should the node attempt to reconnect? Maybe try before shutting down shards? TBD
