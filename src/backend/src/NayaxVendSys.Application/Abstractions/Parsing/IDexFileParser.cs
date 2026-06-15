using NayaxVendSys.Application.Features.Dex.Parsing;

namespace NayaxVendSys.Application.Abstractions.Parsing;

public interface IDexFileParser
{
    ParsedDexDocument Parse(string content);
}
