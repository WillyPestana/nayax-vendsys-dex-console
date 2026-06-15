namespace NayaxVendSys.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string ConnectionString { get; init; } =
        "Server=localhost,1433;Database=VendSysDex;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=False";
}
