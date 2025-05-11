module Lattice.WorkerNode.Program

open Elmish
open System
open System.Threading
open System.Threading.Tasks

[<RequireQualifiedAccess>]
type ExitCode =
    | Success = 0

[<EntryPoint>]
let run _ =
    use cts = new CancellationTokenSource()

    let options = Options.read() |> Option.defaultWith (fun _ -> failwith "Invalid configuration")
    let negotiateUri = Uri(options.OrchestratorAddress + $"/api/negotiate?userId={options.NodeId}")

    printfn "Starting node %A..." options.NodeId

    Program.mkProgram Node.init Node.update (fun _ _ -> ())
    |> Program.withSubscription Node.subscribe
    |> Program.withTermination Node.terminate (fun _ -> cts.Cancel())
    |> Program.withConsoleTrace
    |> Program.runWith (options.NodeId, negotiateUri)

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
