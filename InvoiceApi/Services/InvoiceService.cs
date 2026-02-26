using InvoiceApi.Data;
using InvoiceApi.Dtos;

namespace InvoiceApi.Services;

public class InvoiceService
{
    private readonly InvoiceRepository _repo;

    public InvoiceService(InvoiceRepository repo)
    {
        _repo = repo;
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest req, CancellationToken ct)
    {
        // Validación adicional (además de DataAnnotations)
        if (req.IssueDate.Date > DateTime.UtcNow.Date.AddDays(1))
            throw new ArgumentException("IssueDate cannot be in the far future.");

        var id = Guid.NewGuid();
        return await _repo.CreateAsync(id, req, ct);
    }

    public Task<InvoiceResponse?> GetByIdAsync(Guid id, CancellationToken ct)
        => _repo.GetByIdAsync(id, ct);

    public Task<List<InvoiceResponse>> SearchByClientAsync(string client, CancellationToken ct)
        => _repo.SearchByClientAsync(client, ct);
}