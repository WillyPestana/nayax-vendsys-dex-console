using Microsoft.AspNetCore.Authentication;
using NayaxVendSys.Api.Authentication;
using NayaxVendSys.Api.Middleware;
using NayaxVendSys.Api.Realtime;
using NayaxVendSys.Application;
using NayaxVendSys.Application.Abstractions.Realtime;
using NayaxVendSys.Infrastructure.Configuration;

namespace NayaxVendSys.Api.Configuration;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration);

        services.AddSingleton<IDexProcessingNotifier, SignalRDexProcessingNotifier>();

        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSignalR();
        services.AddHealthChecks();

        services
            .AddAuthentication(BasicAuthenticationDefaults.Scheme)
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                BasicAuthenticationDefaults.Scheme,
                options => { });

        services.AddAuthorization();

        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? ["http://localhost:3000", "http://localhost:5173"];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        var swaggerEnabled = app.Environment.IsDevelopment()
            || app.Configuration.GetValue("Swagger:Enabled", false);

        if (swaggerEnabled)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
