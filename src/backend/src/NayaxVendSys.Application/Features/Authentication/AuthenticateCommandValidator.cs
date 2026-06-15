using FluentValidation;

namespace NayaxVendSys.Application.Features.Authentication;

public sealed class AuthenticateCommandValidator : AbstractValidator<AuthenticateCommand>
{
    public AuthenticateCommandValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MaximumLength(200);
    }
}
