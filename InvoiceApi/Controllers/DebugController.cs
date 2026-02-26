using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace InvoiceApi.Controllers;

[ApiController]
[Route("debug")]
public class DebugController : ControllerBase
{
    private readonly IConfiguration _config;
    public DebugController(IConfiguration config) => _config = config;

    [HttpGet("db")]
    public async Task<IActionResult> Db(CancellationToken ct)
    {
        var cs = _config.GetConnectionString("SqlServer");
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(@"
SELECT @@SERVERNAME AS ServerName, DB_NAME() AS CurrentDb;
SELECT OBJECT_ID('dbo.sp_Invoice_Create','P') AS HasCreate;
", conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        string serverName = "", currentDb = "";
        object? hasCreate = null;

        if (await reader.ReadAsync(ct))
        {
            serverName = reader.GetString(0);
            currentDb = reader.GetString(1);
        }

        await reader.NextResultAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            hasCreate = reader.GetValue(0);
        }

        return Ok(new { serverName, currentDb, hasCreate });
    }
}