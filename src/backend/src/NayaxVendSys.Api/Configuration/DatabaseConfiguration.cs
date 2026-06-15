using NayaxVendSys.Application.Abstractions.Persistence;

namespace NayaxVendSys.Api.Configuration;

public static class DatabaseConfiguration
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await initializer.InitializeAsync(app.Lifetime.ApplicationStopping);
    }
}
