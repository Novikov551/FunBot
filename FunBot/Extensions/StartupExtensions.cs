using Microsoft.AspNetCore.HttpOverrides;

namespace FunBot.Extensions
{
    public static class StartupExtensions
    {
        public static void UseForwarderHeaders(this IApplicationBuilder app)
        {
            var forwardedOpts = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedOpts.KnownNetworks.Clear();
            forwardedOpts.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedOpts);
        }
    }
}