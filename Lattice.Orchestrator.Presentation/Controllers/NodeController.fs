namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums
open System.Net

type NodeController (env: IEnv) =
    [<Function "RegisterNode">]
    [<OpenApiOperation(operationId = "RegisterNode", Summary = "Register a new node", Description = "Registers a new node ready to accept shards to open", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof<NodeResponse>, Description = "Node successfully registered")>]
    member _.RegisterNode (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "nodes")>] req: HttpRequestData
    ) = task {
        let! res = RegisterNodeCommand.run env

        match res with
        | Error RegisterNodeCommandError.RegistrationFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok node ->
            let res = req.CreateResponse HttpStatusCode.Created
            do! res.WriteAsJsonAsync (NodeResponse.fromDomain node)
            return res
    }
