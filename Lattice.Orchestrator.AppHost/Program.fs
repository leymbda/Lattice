open Azure.Messaging.WebPubSub
open Lattice.Orchestrator.AppHost
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Pool
open Lattice.Orchestrator.Presentation
open Microsoft.Azure.Cosmos
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.DurableTask.Client

let (!) f = f |> ignore

HostBuilder()
    .ConfigureFunctionsWorkerDefaults(fun (builder: IFunctionsWorkerApplicationBuilder) ->
        !builder.UseMiddleware<ExceptionMiddleware>()
    )
    .ConfigureServices(fun ctx services ->
        !services.AddHttpClient()
        !services.AddLogging()
        !services.AddApplicationInsightsTelemetryWorkerService()
        !services.ConfigureFunctionsApplicationInsights()

        !services.AddSingleton<CosmosClient>(fun _ -> new CosmosClient(ctx.Configuration.GetValue<string>("CosmosDb")))
        !services.AddSingleton<WebPubSubServiceClient>(fun _ -> new WebPubSubServiceClient(ctx.Configuration.GetValue<string>("WebPubSub"), PoolHandler.HUB_NAME))
        !services.AddDurableTaskClient(fun builder -> !builder.UseGrpc())

        !services.AddSingleton<IEnv, Env>()
        !services.AddSingleton<IDiscord>(fun sp -> sp.GetRequiredService<IEnv>() :> IDiscord)
        !services.AddSingleton<IPool>(fun sp -> sp.GetRequiredService<IEnv>() :> IPool)
        !services.AddSingleton<IPersistence>(fun sp -> sp.GetRequiredService<IEnv>() :> IPersistence)
        !services.AddSingleton<ISecrets>(fun sp -> sp.GetRequiredService<IEnv>() :> ISecrets)
    )
    .Build()
    .Run()
