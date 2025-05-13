module Lattice.WorkerNode.Program

open Elmish
open System
open System.Threading.Tasks

[<RequireQualifiedAccess>]
type ExitCode =
    | Success = 0

[<EntryPoint>]
let run _ =

    let options = Options.read() |> Option.defaultWith (fun _ -> failwith "Invalid configuration")
    let negotiateUri = Uri(options.OrchestratorAddress + $"/api/negotiate?userId={options.NodeId}")

    printfn "Starting node %A..." options.NodeId
    let tcs = TaskCompletionSource<Node.Model>()

    Program.mkProgram Node.init Node.update (fun _ _ -> ())
    |> Program.withSubscription Node.subscribe
    |> Program.withTermination Node.terminate tcs.SetResult
    |> Program.withConsoleTrace
    |> Program.runWith (options.NodeId, negotiateUri)

    tcs.Task :> Task
    |> Async.AwaitTask
    |> Async.RunSynchronously

    printfn "Program finished."
    int ExitCode.Success
    
    // TODO: On disconnect, should the node attempt to reconnect? Maybe try before shutting down shards? TBD
