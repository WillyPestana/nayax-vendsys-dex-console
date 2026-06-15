using Microsoft.Data.SqlClient;

namespace NayaxVendSys.Infrastructure.Persistence.Connection;

public interface ISqlConnectionFactory
{
    SqlConnection CreateApplicationConnection();

    SqlConnection CreateMasterConnection();
}
