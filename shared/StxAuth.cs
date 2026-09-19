using Microsoft.Extensions.DependencyInjection;
using STX.Sdk;
using STX.Sdk.Auth;
using STX.Sdk.Settings;

namespace StxDemo
{
    /// <summary>
    /// Registers the SDK with whichever environment and credentials the environment supplies.
    /// </summary>
    /// <remarks>
    /// These samples authenticate with an API key, which is the recommended path for
    /// programmatic access. Set STX_API_KEY_ID and STX_API_KEY_PEM_PATH. Requests are signed
    /// per call with Ed25519: there is no login, no token to expire and no refresh, so a
    /// restart or an API deployment cannot leave you without a session. Create a key under
    /// Account, API Keys.
    ///
    /// Email and password authentication still works in the SDK and is supported for
    /// integrations already using it, but it is deliberately not shown here. A sample should
    /// teach one way to do something. See the SDK docs if you are maintaining such an
    /// integration.
    ///
    /// Keep the private key out of source control and off the command line. Pass a path, as
    /// here, rather than putting the key itself in an environment variable.
    /// </remarks>
    public static class StxAuth
    {
        public static IServiceCollection AddStx(this IServiceCollection services)
        {
            var environment = ResolveEnvironment();

            var keyId = Environment.GetEnvironmentVariable("STX_API_KEY_ID")
                ?? throw new InvalidOperationException(
                    "STX_API_KEY_ID is not set. These samples authenticate with an API key: "
                    + "create one under Account, API Keys, then set STX_API_KEY_ID and "
                    + "STX_API_KEY_PEM_PATH. See the README.");
            var pemPath = Environment.GetEnvironmentVariable("STX_API_KEY_PEM_PATH")
                ?? throw new InvalidOperationException(
                    "STX_API_KEY_ID is set but STX_API_KEY_PEM_PATH is not. Point it at the "
                    + "PEM file holding the private key for that key id.");

            return services.ConfigureSTXServices(
                environment, STXApiKeyCredentials.FromPemFile(keyId, pemPath));
        }

        /// <summary>
        /// STX_ENV picks a published environment by name. The SDK carries the hosts, so
        /// there are no URLs to get right.
        /// </summary>
        /// <remarks>
        /// GRAPHQL_URI and CHANNELS_URI still work and win when set, which is how you point a
        /// sample at an environment this SDK version does not name.
        /// </remarks>
        private static STXEnvironment ResolveEnvironment()
        {
            var graphQl = Environment.GetEnvironmentVariable("GRAPHQL_URI");
            var channels = Environment.GetEnvironmentVariable("CHANNELS_URI");

            if (!string.IsNullOrWhiteSpace(graphQl) && !string.IsNullOrWhiteSpace(channels))
                return STXEnvironment.Custom(graphQl, channels);

            return (Environment.GetEnvironmentVariable("STX_ENV") ?? "ontario-demo").ToLowerInvariant() switch
            {
                "ontario-demo"       => STXEnvironment.OntarioDemo,
                "ontario-production" => STXEnvironment.OntarioProduction,
                "us-demo"            => STXEnvironment.USDemo,
                var other => throw new InvalidOperationException(
                    $"Unknown STX_ENV '{other}'. Use ontario-demo, ontario-production or "
                    + "us-demo, or set GRAPHQL_URI and CHANNELS_URI for anything else.")
            };
        }
    }
}
