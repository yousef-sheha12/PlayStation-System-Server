using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayStation.Application.DTOs.Invoice;
using PlayStation.Application.Features.Invoices.Commands;
using PlayStation.Application.Features.Invoices.Queries;

namespace PlayStation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllInvoicesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(id));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetBySession(int sessionId)
    {
        var result = await _mediator.Send(new GetInvoiceBySessionQuery(sessionId));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateInvoiceDto request)
    {
        var result = await _mediator.Send(new GenerateInvoiceCommand(request));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{invoiceId}/payment")]
    public async Task<IActionResult> UpdatePayment(int invoiceId, [FromQuery] bool isPaid)
    {
        var result = await _mediator.Send(new UpdateInvoicePaymentCommand(invoiceId, isPaid));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
