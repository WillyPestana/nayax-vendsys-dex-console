using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NayaxVendSys.Application.Features.Authentication;
using NayaxVendSys.Application.Features.Dex.GetDexMeters;
using NayaxVendSys.Application.Features.Dex.UploadDex;

namespace NayaxVendSys.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<AuthenticateCommand>, AuthenticateCommandValidator>();
        services.AddSingleton<IValidator<UploadDexCommand>, UploadDexCommandValidator>();

        services.AddScoped<AuthenticateCommandHandler>();
        services.AddScoped<UploadDexCommandHandler>();
        services.AddScoped<GetDexMetersQueryHandler>();

        return services;
    }
}
