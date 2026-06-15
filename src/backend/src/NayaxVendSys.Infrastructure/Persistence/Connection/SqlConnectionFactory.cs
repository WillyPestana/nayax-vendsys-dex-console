using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace NayaxVendSys.Infrastructure.Persistence.Connection;

public sealed class SqlConnectionFactory(IOptions<PersistenceOptions> options) : ISqlConnectionFactory
{
    private readonly PersistenceOptions _options = options.Value;

    public SqlConnection CreateApplicationConnection()
    {
        return new SqlConnection(_options.ConnectionString);
    }

    public SqlConnection CreateMasterConnection()
    {
        var builder = new SqlConnectionStringBuilder(_options.ConnectionString)
        {
            InitialCatalog = "master"
        };

        return new SqlConnection(builder.ConnectionString);
    }
}
