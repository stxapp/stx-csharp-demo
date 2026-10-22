// A long-running worker on STX.Sdk: it streams the order book of a few markets and keeps a live
// copy of your account, and with --quote it rests a 1-cent bid protected by cancel-on-disconnect.
//
//   dotnet run                          # stream until Ctrl+C
//   dotnet run -- --quote               # also rest a 1-cent bid while running
//   dotnet run -- --once --quote        # one pass: place, check, cancel, exit
//
// The API key comes from ~/.stx/credentials or STX_* environment variables; see README.md.

using StxConsole;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton(new WorkerOptions(Quote: args.Contains("--quote"), Once: args.Contains("--once"), Profile: ProfileArg(args)));
builder.Services.AddHostedService<Worker>();
await builder.Build().RunAsync();

static string? ProfileArg(string[] args)
{
    var i = Array.IndexOf(args, "--profile");
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
