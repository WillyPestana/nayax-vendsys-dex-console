namespace NayaxVendSys.Application.Features.Dex.UploadDex;

public sealed record UploadDexCommand(Stream FileStream, string FileName, long FileSizeBytes);
