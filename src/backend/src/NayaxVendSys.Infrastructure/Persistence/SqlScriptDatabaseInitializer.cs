using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NayaxVendSys.Application.Abstractions.Persistence;
using NayaxVendSys.Infrastructure.Configuration;
using NayaxVendSys.Infrastructure.Persistence.Connection;

namespace NayaxVendSys.Infrastructure.Persistence;

public sealed partial class SqlScriptDatabaseInitializer(
    ISqlConnectionFactory connectionFactory,
    IOptions<DatabaseInitializerOptions> options,
    ILogger<SqlScriptDatabaseInitializer> logger) : IDatabaseInitializer
{
    private readonly DatabaseInitializerOptions _options = options.Value;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (!_options.RunOnStartup)
        {
            logger.LogInformation("Database script initializer is disabled.");
            return;
        }

        var scriptsPath = Path.GetFullPath(_options.ScriptsPath);
        if (!Directory.Exists(scriptsPath))
        {
            throw new DirectoryNotFoundException($"Database scripts directory was not found: {scriptsPath}");
        }

        await using var connection = connectionFactory.CreateMasterConnection();
        await OpenWithRetryAsync(connection, cancellationToken);

        foreach (var scriptPath in Directory.EnumerateFiles(scriptsPath, "*.sql").Order(StringComparer.Ordinal))
        {
            logger.LogInformation("Applying database script {ScriptPath}", scriptPath);
            var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
            foreach (var batch in SplitBatches(script))
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                await using var command = connection.CreateCommand();
                command.CommandText = batch;
                command.CommandTimeout = 60;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }

    private static async Task OpenWithRetryAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const int maxAttempts = 30;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await connection.OpenAsync(cancellationToken);
                return;
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    private static IEnumerable<string> SplitBatches(string script)
    {
        return GoRegex().Split(script);
    }

    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoRegex();
}
