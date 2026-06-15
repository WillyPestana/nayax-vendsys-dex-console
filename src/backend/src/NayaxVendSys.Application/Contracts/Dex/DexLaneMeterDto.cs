namespace NayaxVendSys.Application.Contracts.Dex;

public sealed record DexLaneMeterDto(
    int Id,
    string ProductIdentifier,
    decimal Price,
    int NumberOfVends,
    decimal ValueOfPaidSales);
