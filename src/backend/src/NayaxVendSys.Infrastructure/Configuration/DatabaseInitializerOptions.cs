namespace NayaxVendSys.Infrastructure.Configuration;

public sealed class DatabaseInitializerOptions
{
    public const string SectionName = "DatabaseInitializer";

    public bool RunOnStartup { get; init; }

    public string ScriptsPath { get; init; } = "database/scripts";
}
