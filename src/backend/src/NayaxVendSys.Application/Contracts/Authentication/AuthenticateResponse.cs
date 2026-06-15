namespace NayaxVendSys.Application.Contracts.Authentication;

public sealed record AuthenticateResponse(
    bool Authenticated,
    string AuthorizationHeader,
    string Username);
