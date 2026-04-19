# Contributing

Thanks for taking the time to improve this demo. These are reference applications for the [STX C# SDK](https://www.nuget.org/packages/STX.Sdk); the goal is clear, minimal, *working* examples that a newcomer can clone and run.

## Where to file things

- **Bug report / example doesn't work** → open an [Issue](https://github.com/stxapp/stx-csharp-demo/issues/new/choose). The form walks you through the details that matter.
- **New example request / improvement** → also an Issue, using the "Feature request / example request" template.
- **Question about the SDK itself** (as opposed to these demos) → start with [the docs](https://docs.stxapp.io/sdks/csharp/quickstart). For bugs in the SDK (not this demo), file against the SDK package, not here.

## Filing a good issue

- One problem per issue.
- Include the exact command you ran + the exact output you got. Copy-paste beats paraphrasing.
- Mention your OS, `.dotnet --version`, and the `STX.Sdk` version you're on.
- If you already know the root cause, say so — but don't wait until you do to file.

## Pull requests

Happy to take them. Before you start a large change, open an issue first so we can discuss scope.

Before opening a PR:

```bash
# Both projects should build clean.
dotnet build console/
dotnet build webapi/

# If your change affects a sample, run it against staging once:
export EMAIL=... PASSWORD=... \
       GRAPHQL_URI=https://api-staging.on.sportsxapp.com/api/graphql \
       CHANNELS_URI=wss://api-staging.on.sportsxapp.com/socket/websocket
cd console && dotnet run    # or: cd webapi && dotnet run
```

Keep diffs small and focused. One concern per PR makes review cheap.

## What *not* to submit

- Breaking changes to the shape of these demos without prior agreement (they're public reference; stability matters).
- Examples that require unreleased or private SDK versions — everything here must work against the latest published `STX.Sdk` on NuGet.
- Credentials, tokens, or any real account data in example configs.

## Code of conduct

Be kind. Assume the person on the other end is trying to do good work. Reviews are a conversation, not a gate.
