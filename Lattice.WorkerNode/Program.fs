module Lattice.WorkerNode.Program

open Elmish
open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Contracts
open Microsoft.Extensions.Configuration
open System
open System.IO
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open Thoth.Json.Net

[<RequireQualifiedAccess>]
type ExitCode =
    | Success = 0

[<EntryPoint>]
let run _ =
    use cts = new CancellationTokenSource()

    // Get configuration from appsettings.json
    let config =
        new ConfigurationBuilder()
        :> IConfigurationBuilder
        |> _.SetBasePath(Directory.GetCurrentDirectory())
        |> _.AddJsonFile("appsettings.json", optional = false)
        |> _.Build()

    let orchestratorAddress =
        config.GetValue<string | null>("OrchestratorAddress")
        |> Option.ofNull
        |> Option.defaultWith (fun _ -> failwith "OrchestratorAddress not provided in appsettings.json")

    let nodeId =
        config.GetValue<string | null>("NodeId")
        |> Option.ofNull
        |> Option.bind (fun v -> Guid.TryParse v |> function | true, id -> Some id | _ -> None)
        |> Option.teeNone (fun _ -> printfn "No NodeId provided in appsettings.json or provided ID not a valid Guid, generating random...")
        |> Option.defaultValue (Guid.NewGuid())

    // Negotiate web pubsub uri from orchestrator
    let uri =
        asyncResult {
            use client = new HttpClient()
            let! res = client.PostAsync(Uri(orchestratorAddress + "/api/negotiate"), null)
            let! content = res.Content.ReadAsStringAsync()
            let! negotiate = Decode.fromString NegotiateResponse.decoder content
            return negotiate.Url
        }
        |> AsyncResult.defaultWith (fun _ -> failwith "Failed to negotiate with orchestrator")
        |> Async.RunSynchronously
        |> Uri

    // Run program
    printfn "Starting node %A..." nodeId

    Program.mkProgram Node.init Node.update (fun _ _ -> ())
    |> Program.withSubscription Node.subscribe
    |> Program.withTermination Node.terminate (fun _ -> cts.Cancel())
    |> Program.withConsoleTrace
    |> Program.runWith (nodeId, uri)

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
