using Microsoft.Extensions.DependencyInjection;
using STX.Sdk;
using STX.Sdk.Auth;

namespace StxDemo
{
    /// <summary>
    /// Registers the SDK with whichever credentials the environment supplies.
    /// </summary>
    /// <remarks>
    /// Two ways to authenticate, and the samples support both so you can compare them:
    ///
    /// <b>API key (preferred).</b> Set STX_API_KEY_ID and STX_API_KEY_PEM_PATH. Requests are
    /// signed per call with Ed25519. There is no login, no token to expire and no refresh, so
    /// a restart or an API deployment cannot leave you without a session. Create a key under
    /// Account, API Keys.
    ///
    /// <b>Email and password.</b> Set EMAIL and PASSWORD. Still fully supported.
    ///
    /// Keep the private key out of source control and off the command line. Pass a path and
    /// read the file, as here, rather than pasting the key into an environment variable.
    /// </remarks>
    public static class StxAuth
    {
        public static bool UsesApiKey =>
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("STX_API_KEY_ID"));

        public static IServiceCollection AddStx(this IServiceCollection services)
        {
            Func<IServiceProvider, string> graphQl = _ => Environment.GetEnvironmentVariable("GRAPHQL_URI");
            Func<IServiceProvider, string> channels = _ => Environment.GetEnvironmentVariable("CHANNELS_URI");

            if (!UsesApiKey)
            {
                return services.ConfigureSTXServices(graphQl, channels);
            }

            var keyId = Environment.GetEnvironmentVariable("STX_API_KEY_ID");
            var pemPath = Environment.GetEnvironmentVariable("STX_API_KEY_PEM_PATH")
                ?? throw new InvalidOperationException(
                    "STX_API_KEY_ID is set but STX_API_KEY_PEM_PATH is not. Point it at the "
                    + "PEM file holding the private key for that key id.");

            if (!File.Exists(pemPath))
                throw new FileNotFoundException($"Private key not found at '{pemPath}'.", pemPath);

            return services.ConfigureSTXServices(
                graphQl, channels, new STXApiKeyCredentials(keyId, File.ReadAllText(pemPath)));
        }
    }
}
