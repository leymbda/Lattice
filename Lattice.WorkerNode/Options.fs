namespace Lattice.WorkerNode

open FsToolkit.ErrorHandling
open Microsoft.Extensions.Configuration
open System
open System.IO

type Options = {
    NodeId: Guid
    OrchestratorAddress: string
}

module Options =
    let read () =
        let config =
            new ConfigurationBuilder()
            :> IConfigurationBuilder
            |> _.SetBasePath(Directory.GetCurrentDirectory())
            |> _.AddJsonFile("appsettings.json", optional = false)
            |> _.Build()
            
        let nodeId =
            config.GetValue<string | null>("NodeId")
            |> Option.ofNull
            |> Option.bind (fun v -> Guid.TryParse v |> function | true, id -> Some id | _ -> None)
            |> Option.teeNone (fun _ -> printfn "No NodeId provided in appsettings.json or provided ID not a valid Guid, generating random...")
            |> Option.defaultValue (Guid.NewGuid())

        let orchestratorAddress =
            config.GetValue<string | null>("OrchestratorAddress")
            |> Option.ofNull

        match orchestratorAddress with
        | None -> None
        | Some orchestratorAddress ->
            Some {
                NodeId = nodeId
                OrchestratorAddress = orchestratorAddress
            }
