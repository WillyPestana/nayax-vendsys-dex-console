using NayaxVendSys.Api.Endpoints;
using NayaxVendSys.Api.Hubs;

namespace NayaxVendSys.Api.Configuration;

public static class EndpointConfiguration
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => Results.Redirect("/swagger/"));
        app.MapHealthChecks("/health");
        app.MapAuthenticationEndpoints();
        app.MapDexEndpoints();
        app.MapHub<DexProcessingHub>("/hubs/dex-processing");

        return app;
    }
}
