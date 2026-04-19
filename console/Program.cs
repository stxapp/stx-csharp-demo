using Microsoft.Extensions.Hosting;
using STX.Sdk.Console;
using STX.Sdk;
using Microsoft.Extensions.DependencyInjection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.ConfigureSTXServices(
            (s) => Environment.GetEnvironmentVariable("GRAPHQL_URI"),
            (s) => Environment.GetEnvironmentVariable("CHANNELS_URI"));

        services.AddSingleton<STXWorker>();
    })
    .Build();

var _ = host.RunAsync(); //this is needed in order to run background services

var stx = host.Services.GetService<STXWorker>();

await stx.RunAsync();