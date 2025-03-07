open Azure.Identity
open Azure.Messaging.EventGrid
open Azure.Messaging.ServiceBus
open FSharp.Discord.Rest
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Discord
open Lattice.Orchestrator.Presentation
open Microsoft.Azure.Cosmos
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System
open System.IO
open Microsoft.DurableTask.Client

let (!) f = f |> ignore

HostBuilder()
    .ConfigureFunctionsWorkerDefaults(fun (builder: IFunctionsWorkerApplicationBuilder) ->
        !builder.UseMiddleware<ExceptionMiddleware>()
    )
    .ConfigureAppConfiguration(fun builder ->
        // Add environment variables to configuration
        !builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", true)
            .AddJsonFile("appsettings.json", true)
            .AddEnvironmentVariables()
    )
    .ConfigureServices(fun ctx services ->
        !services.Configure<SecretsOptions>(ctx.Configuration.GetSection(nameof SecretsOptions))

        !services.AddHttpClient()
        !services.AddLogging()
        !services.AddApplicationInsightsTelemetryWorkerService()
        !services.ConfigureFunctionsApplicationInsights()

        !services.AddSingleton<IDiscordClientFactory, DiscordClientFactory>()
        !services.AddSingleton<CosmosClient>(fun _ -> new CosmosClient(ctx.Configuration.GetValue<string>("CosmosDb")))
        !services.AddSingleton<EventGridPublisherClient>(fun sp -> new EventGridPublisherClient(Uri (ctx.Configuration.GetValue<string>("EventGridEndpoint")), DefaultAzureCredential()))
        !services.AddSingleton<ServiceBusClient>(fun _ -> new ServiceBusClient(ctx.Configuration.GetValue<string>("ServiceBus")))
        !services.AddDurableTaskClient(fun builder -> !builder.UseGrpc())

        !services.AddSingleton<IEnv, Env>()
        !services.AddSingleton<IDiscord>(fun sp -> sp.GetRequiredService<IEnv>() :> IDiscord)
        !services.AddSingleton<IEvents>(fun sp -> sp.GetRequiredService<IEnv>() :> IEvents)
        !services.AddSingleton<IPersistence>(fun sp -> sp.GetRequiredService<IEnv>() :> IPersistence)
    )
    .Build()
    .Run()
