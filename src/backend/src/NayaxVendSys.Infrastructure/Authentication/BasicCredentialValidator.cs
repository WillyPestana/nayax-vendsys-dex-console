using Microsoft.Extensions.Options;
using NayaxVendSys.Application.Abstractions.Authentication;

namespace NayaxVendSys.Infrastructure.Authentication;

public sealed class BasicCredentialValidator(IOptions<BasicAuthOptions> options) : ICredentialValidator
{
    private readonly BasicAuthOptions _options = options.Value;

    public bool Validate(string username, string password)
    {
        return string.Equals(username, _options.Username, StringComparison.Ordinal)
            && string.Equals(password, _options.Password, StringComparison.Ordinal);
    }
}
