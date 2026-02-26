namespace InvoiceApi.Dtos;

public class InvoiceResponse
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = default!;
    public string InvoiceNumber { get; set; } = default!;
    public DateTime IssueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}