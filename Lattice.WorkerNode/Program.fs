open Lattice.WorkerNode
open FSharp.Discord.Gateway
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System
open System.IO

let (!) f = f |> ignore

Host
    .CreateDefaultBuilder(Environment.GetCommandLineArgs())
    .ConfigureAppConfiguration(fun builder ->
        // Add environment variables to configuration
        !builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .AddEnvironmentVariables()
    )
    .ConfigureServices(fun services ->
        // Register services
        !services.AddHttpClient()
        !services.AddTransient<IServiceBusClientFactory, ServiceBusClientFactory>()
        !services.AddTransient<IWebsocketFactory, WebsocketFactory>()
        !services.AddTransient<IGatewayClientFactory, GatewayClientFactory>()
    )
    .Build()
    .Services.GetRequiredService<Node>()
    .StartAsync()
|> Async.AwaitTask
|> Async.RunSynchronously
