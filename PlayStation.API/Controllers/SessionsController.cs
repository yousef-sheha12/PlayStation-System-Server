using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayStation.Application.DTOs.Session;
using PlayStation.Application.Features.Sessions.Commands;
using PlayStation.Application.Features.Sessions.Queries;
using PlayStation.Domain.Enums;

namespace PlayStation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllSessionsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetSessionByIdQuery(id));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _mediator.Send(new GetActiveSessionsQuery());
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(SessionStatus status)
    {
        var result = await _mediator.Send(new GetSessionsByStatusQuery(status));
        return Ok(result);
    }

    [HttpGet("device/{deviceId}")]
    public async Task<IActionResult> GetByDevice(int deviceId)
    {
        var result = await _mediator.Send(new GetSessionsByDeviceQuery(deviceId));
        return Ok(result);
    }

    [HttpGet("date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _mediator.Send(new GetSessionsByDateRangeQuery(startDate, endDate));
        return Ok(result);
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionDto request)
    {
        var result = await _mediator.Send(new StartSessionCommand(request));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{sessionId}/pause")]
    public async Task<IActionResult> PauseSession(int sessionId)
    {
        var result = await _mediator.Send(new PauseSessionCommand(sessionId));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{sessionId}/resume")]
    public async Task<IActionResult> ResumeSession(int sessionId)
    {
        var result = await _mediator.Send(new ResumeSessionCommand(sessionId));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{sessionId}/end")]
    public async Task<IActionResult> EndSession(int sessionId, [FromBody] EndSessionDto? request = null)
    {
        var result = await _mediator.Send(new EndSessionCommand(sessionId, request?.Discount ?? 0));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{sessionId}/products")]
    public async Task<IActionResult> AddProduct(int sessionId, [FromBody] AddProductToSessionDto request)
    {
        var result = await _mediator.Send(new AddProductToSessionCommand(sessionId, request));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{sessionId}/products/{productId}")]
    public async Task<IActionResult> RemoveProduct(int sessionId, int productId)
    {
        var result = await _mediator.Send(new RemoveProductFromSessionCommand(sessionId, productId));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
