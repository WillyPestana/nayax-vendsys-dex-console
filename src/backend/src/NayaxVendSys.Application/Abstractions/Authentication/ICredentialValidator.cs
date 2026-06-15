namespace NayaxVendSys.Application.Abstractions.Authentication;

public interface ICredentialValidator
{
    bool Validate(string username, string password);
}
