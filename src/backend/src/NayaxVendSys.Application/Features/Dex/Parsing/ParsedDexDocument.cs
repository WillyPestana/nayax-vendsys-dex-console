namespace NayaxVendSys.Application.Features.Dex.Parsing;

public sealed record ParsedDexDocument(
    string MachineId,
    DateTime DexDateTime,
    string MachineSerialNumber,
    decimal ValueOfPaidVends,
    IReadOnlyCollection<ParsedDexLane> Lanes);
