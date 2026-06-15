using NayaxVendSys.Api.Contracts.Requests;
using NayaxVendSys.Application.Contracts.Authentication;
using NayaxVendSys.Application.Features.Authentication;

namespace NayaxVendSys.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/authenticate", async (
                AuthenticateRequest request,
                AuthenticateCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(
                    new AuthenticateCommand(request.Username, request.Password),
                    cancellationToken);

                return response is null
                    ? Results.Unauthorized()
                    : Results.Ok(response);
            })
            .AllowAnonymous()
            .Produces<AuthenticateResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
