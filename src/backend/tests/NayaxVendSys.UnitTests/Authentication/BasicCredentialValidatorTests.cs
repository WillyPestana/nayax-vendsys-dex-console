using FluentAssertions;
using Microsoft.Extensions.Options;
using NayaxVendSys.Infrastructure.Authentication;

namespace NayaxVendSys.UnitTests.Authentication;

public sealed class BasicCredentialValidatorTests
{
    [Fact]
    public void Validate_ReturnsTrue_ForConfiguredCredentials()
    {
        var validator = new BasicCredentialValidator(Options.Create(new BasicAuthOptions()));

        var result = validator.Validate("vendsys", "NFsZGmHAGWJSZ#RuvdiV");

        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsFalse_ForInvalidPassword()
    {
        var validator = new BasicCredentialValidator(Options.Create(new BasicAuthOptions()));

        var result = validator.Validate("vendsys", "wrong");

        result.Should().BeFalse();
    }
}
