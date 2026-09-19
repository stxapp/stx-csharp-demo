using StxDemo;
using Microsoft.Extensions.Hosting;
using STX.Sdk.Console;
using STX.Sdk;
using Microsoft.Extensions.DependencyInjection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Picks API-key or email/password based on the environment. See shared/StxAuth.cs.
        services.AddStx();

        services.AddSingleton<STXWorker>();
    })
    .Build();

var _ = host.RunAsync(); //this is needed in order to run background services

var stx = host.Services.GetService<STXWorker>();

await stx.RunAsync();