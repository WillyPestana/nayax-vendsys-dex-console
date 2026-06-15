namespace NayaxVendSys.Application.Contracts.Dex;

public sealed record UploadDexResponse(
    int DexMeterId,
    string MachineId,
    DateTime DexDateTime,
    int LaneCount,
    string Message);
