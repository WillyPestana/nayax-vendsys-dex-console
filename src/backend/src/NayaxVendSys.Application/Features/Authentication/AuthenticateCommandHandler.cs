using System.Text;
using FluentValidation;
using NayaxVendSys.Application.Abstractions.Authentication;
using NayaxVendSys.Application.Contracts.Authentication;

namespace NayaxVendSys.Application.Features.Authentication;

public sealed class AuthenticateCommandHandler(
    IValidator<AuthenticateCommand> validator,
    ICredentialValidator credentialValidator)
{
    public async Task<AuthenticateResponse?> HandleAsync(
        AuthenticateCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        if (!credentialValidator.Validate(command.Username, command.Password))
        {
            return null;
        }

        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{command.Username}:{command.Password}"));
        return new AuthenticateResponse(true, $"Basic {token}", command.Username);
    }
}
