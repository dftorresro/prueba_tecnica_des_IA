using System.ComponentModel.DataAnnotations;

namespace InvoiceApi.Dtos;

public class CreateInvoiceRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string ClientName { get; set; } = default!;

    [Required, StringLength(50, MinimumLength = 1)]
    public string InvoiceNumber { get; set; } = default!;

    [Required]
    public DateTime IssueDate { get; set; }

    [Range(0.01, 999999999)]
    public decimal TotalAmount { get; set; }

    [Required, StringLength(10, MinimumLength = 1)]
    public string Currency { get; set; } = "USD";

    [StringLength(500)]
    public string? Description { get; set; }
}