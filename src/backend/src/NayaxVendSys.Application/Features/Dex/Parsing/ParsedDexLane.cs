namespace NayaxVendSys.Application.Features.Dex.Parsing;

public sealed record ParsedDexLane(
    string ProductIdentifier,
    decimal Price,
    int NumberOfVends,
    decimal ValueOfPaidSales);
