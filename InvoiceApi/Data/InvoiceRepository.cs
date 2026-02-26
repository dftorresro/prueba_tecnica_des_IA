using Microsoft.Data.SqlClient;
using InvoiceApi.Dtos;

namespace InvoiceApi.Data;

public class InvoiceRepository
{
    private readonly DbConnectionFactory _factory;

    public InvoiceRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<InvoiceResponse> CreateAsync(Guid id, CreateInvoiceRequest req, CancellationToken ct)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand("dbo.sp_Invoice_Create", conn);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@ClientName", req.ClientName);
        cmd.Parameters.AddWithValue("@InvoiceNumber", req.InvoiceNumber);
        cmd.Parameters.AddWithValue("@IssueDate", req.IssueDate.Date);
        cmd.Parameters.AddWithValue("@TotalAmount", req.TotalAmount);
        cmd.Parameters.AddWithValue("@Currency", req.Currency);
        cmd.Parameters.AddWithValue("@Description", (object?)req.Description ?? DBNull.Value);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            throw new InvalidOperationException("Invoice insert returned no rows.");

        return Map(reader);
    }

    public async Task<InvoiceResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand("dbo.sp_Invoice_GetById", conn);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return Map(reader);
    }

    public async Task<List<InvoiceResponse>> SearchByClientAsync(string clientName, CancellationToken ct)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand("dbo.sp_Invoice_SearchByClient", conn);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ClientName", clientName);

        using var reader = await cmd.ExecuteReaderAsync(ct);

        var list = new List<InvoiceResponse>();
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));

        return list;
    }

    private static InvoiceResponse Map(SqlDataReader r) => new()
    {
        Id = r.GetGuid(r.GetOrdinal("Id")),
        ClientName = r.GetString(r.GetOrdinal("ClientName")),
        InvoiceNumber = r.GetString(r.GetOrdinal("InvoiceNumber")),
        IssueDate = r.GetDateTime(r.GetOrdinal("IssueDate")),
        TotalAmount = r.GetDecimal(r.GetOrdinal("TotalAmount")),
        Currency = r.GetString(r.GetOrdinal("Currency")),
        Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
    };
}