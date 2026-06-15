namespace NayaxVendSys.Domain.Exceptions;

public sealed class DexParsingException : Exception
{
    public DexParsingException(string message)
        : base(message)
    {
    }

    public DexParsingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
