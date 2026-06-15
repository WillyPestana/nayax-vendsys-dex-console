using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NayaxVendSys.Application.Abstractions.Persistence;
using NayaxVendSys.Application.Contracts.Dex;
using NayaxVendSys.Application.Features.Dex.Parsing;

namespace NayaxVendSys.IntegrationTests.Api;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDatabaseInitializer>();
            services.RemoveAll<IDexMeterRepository>();
            services.AddSingleton<IDatabaseInitializer, NoOpDatabaseInitializer>();
            services.AddSingleton<IDexMeterRepository, FakeDexMeterRepository>();
        });
    }

    private sealed class NoOpDatabaseInitializer : IDatabaseInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDexMeterRepository : IDexMeterRepository
    {
        public Task<int> SaveAsync(ParsedDexDocument document, CancellationToken cancellationToken)
        {
            return Task.FromResult(42);
        }

        public Task<IReadOnlyCollection<DexMeterDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<DexMeterDto> meters =
            [
                new DexMeterDto(
                    42,
                    "100077238",
                    new DateTime(2023, 12, 10, 23, 10, 53),
                    "100077238",
                    344.50m,
                    [
                        new DexLaneMeterDto(1, "101", 3.25m, 4, 13.00m)
                    ])
            ];

            return Task.FromResult(meters);
        }

        public Task ClearAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
