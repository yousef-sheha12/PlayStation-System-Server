using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayStation.Application.DTOs.Device;
using PlayStation.Application.Features.Devices.Commands;
using PlayStation.Application.Features.Devices.Queries;
using PlayStation.Domain.Enums;

namespace PlayStation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DevicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllDevicesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetDeviceByIdQuery(id));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(DeviceStatus status)
    {
        var result = await _mediator.Send(new GetDevicesByStatusQuery(status));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeviceDto device)
    {
        var result = await _mediator.Send(new CreateDeviceCommand(device));
        if (!result.IsSuccess) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceDto device)
    {
        var result = await _mediator.Send(new UpdateDeviceCommand(id, device));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteDeviceCommand(id));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
