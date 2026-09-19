using StxDemo;
using STX.Sdk.Services;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Learns who the API key belongs to, once, before anything user-scoped starts.
    /// </summary>
    /// <remarks>
    /// The user-scoped channel wrappers are keyed on the user id. With email and password
    /// that id arrives in the login reply, but an API key has no login step, so nothing
    /// populates it and the first request to /active-orders (or any other user-scoped
    /// endpoint) fails with "the user id is not known yet".
    ///
    /// GetMeAsync is the one call that answers it, so make it at startup rather than
    /// leaving each controller to remember.
    /// </remarks>
    public class StxIdentityWarmup : IHostedService
    {
        private readonly STXViewerService m_ViewerService;
        private readonly ILogger<StxIdentityWarmup> m_Logger;

        public StxIdentityWarmup(STXViewerService viewerService, ILogger<StxIdentityWarmup> logger)
        {
            m_ViewerService = viewerService;
            m_Logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!StxAuth.UsesApiKey)
            {
                return;
            }

            // A throw here would take the whole host down, which is a poor trade for a
            // warmup: the API being briefly unavailable at boot should not stop the app
            // from starting. The user-scoped endpoints fail with their own clear message
            // until a later call succeeds.
            try
            {
                var me = await m_ViewerService.GetMeAsync();
                m_Logger.LogInformation(
                    "Authenticated with API key as {UserId} (scope {Scope})", me.UserId, me.Scope);
            }
            catch (Exception ex)
            {
                m_Logger.LogWarning(ex,
                    "Could not resolve the API key's user id at startup. User-scoped "
                    + "endpoints will fail until a call to /me succeeds.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
