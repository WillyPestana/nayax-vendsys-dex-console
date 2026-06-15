using Microsoft.AspNetCore.Mvc;
using NayaxVendSys.Application.Contracts.Dex;
using NayaxVendSys.Application.Abstractions.Persistence;
using NayaxVendSys.Application.Features.Dex.GetDexMeters;
using NayaxVendSys.Application.Features.Dex.UploadDex;

namespace NayaxVendSys.Api.Endpoints;

public static class DexEndpoints
{
    private const long MaxUploadRequestSizeBytes = UploadDexCommandValidator.MaxFileSizeBytes + 64 * 1024;

    public static IEndpointRouteBuilder MapDexEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/dex", async (
                IFormFile file,
                UploadDexCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                await using var stream = file.OpenReadStream();
                var response = await handler.HandleAsync(
                    new UploadDexCommand(stream, file.FileName, file.Length),
                    cancellationToken);

                return Results.Created($"/dex/{response.DexMeterId}", response);
            })
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxUploadRequestSizeBytes))
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<UploadDexResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        app.MapGet("/dex", async (
                GetDexMetersQueryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var meters = await handler.HandleAsync(cancellationToken);
                return Results.Ok(meters);
            })
            .RequireAuthorization()
            .Produces<IReadOnlyCollection<DexMeterDto>>()
            .Produces(StatusCodes.Status401Unauthorized);

        app.MapDelete("/dex", async (
                IDexMeterRepository repository,
                CancellationToken cancellationToken) =>
            {
                await repository.ClearAsync(cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
