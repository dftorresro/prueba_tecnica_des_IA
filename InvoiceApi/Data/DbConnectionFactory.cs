using Microsoft.Data.SqlClient;

namespace InvoiceApi.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Missing connection string 'SqlServer'.");
    }

    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
}