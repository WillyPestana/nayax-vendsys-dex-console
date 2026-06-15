using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NayaxVendSys.Application.Abstractions.Authentication;
using NayaxVendSys.Application.Abstractions.Parsing;
using NayaxVendSys.Application.Abstractions.Persistence;
using NayaxVendSys.Infrastructure.Authentication;
using NayaxVendSys.Infrastructure.Parsing;
using NayaxVendSys.Infrastructure.Persistence;
using NayaxVendSys.Infrastructure.Persistence.Connection;

namespace NayaxVendSys.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BasicAuthOptions>(configuration.GetSection(BasicAuthOptions.SectionName));
        services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));
        services.Configure<DatabaseInitializerOptions>(configuration.GetSection(DatabaseInitializerOptions.SectionName));

        services.AddSingleton<ICredentialValidator, BasicCredentialValidator>();
        services.AddSingleton<IDexFileParser, DexFileParser>();
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddSingleton<IDatabaseInitializer, SqlScriptDatabaseInitializer>();
        services.AddScoped<IDexMeterRepository, DexMeterRepository>();

        return services;
    }
}
