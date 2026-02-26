using Microsoft.AspNetCore.Mvc;
using InvoiceApi.Dtos;
using InvoiceApi.Services;
using Microsoft.Data.SqlClient;

namespace InvoiceApi.Controllers;

[ApiController]
[Route("invoice")]
public class InvoiceController : ControllerBase
{
    private readonly InvoiceService _service;

    public InvoiceController(InvoiceService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var invoice = await _service.GetByIdAsync(id, ct);
        if (invoice is null)
            return NotFound(Problem(title: "Invoice not found", detail: $"No invoice with id {id}."));

        return Ok(invoice);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(List<InvoiceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search([FromQuery] string client, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(client) || client.Trim().Length < 2)
            return BadRequest(Problem(title: "Invalid query", detail: "Query param 'client' must have at least 2 characters."));

        var results = await _service.SearchByClientAsync(client.Trim(), ct);
        return Ok(results);
    }
}