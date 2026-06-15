namespace NayaxVendSys.Application.Contracts.Dex;

public sealed record DexMeterDto(
    int Id,
    string MachineId,
    DateTime DexDateTime,
    string MachineSerialNumber,
    decimal ValueOfPaidVends,
    IReadOnlyCollection<DexLaneMeterDto> Lanes);
