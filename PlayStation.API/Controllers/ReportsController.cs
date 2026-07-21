using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayStation.Application.Features.Reports.Queries;

namespace PlayStation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("daily-revenue")]
    public async Task<IActionResult> GetDailyRevenue([FromQuery] DateTime date)
    {
        var result = await _mediator.Send(new GetDailyRevenueQuery(date));
        return Ok(result);
    }

    [HttpGet("monthly-revenue")]
    public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _mediator.Send(new GetMonthlyRevenueQuery(year, month));
        return Ok(result);
    }

    [HttpGet("yearly-revenue")]
    public async Task<IActionResult> GetYearlyRevenue([FromQuery] int year)
    {
        var result = await _mediator.Send(new GetYearlyRevenueQuery(year));
        return Ok(result);
    }

    [HttpGet("most-used-devices")]
    public async Task<IActionResult> GetMostUsedDevices([FromQuery] int count = 5)
    {
        var result = await _mediator.Send(new GetMostUsedDevicesQuery(count));
        return Ok(result);
    }

    [HttpGet("most-sold-products")]
    public async Task<IActionResult> GetMostSoldProducts([FromQuery] int count = 5)
    {
        var result = await _mediator.Send(new GetMostSoldProductsQuery(count));
        return Ok(result);
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpensesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _mediator.Send(new GetExpensesReportQuery(startDate, endDate));
        return Ok(result);
    }

    [HttpGet("profit")]
    public async Task<IActionResult> GetProfitReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _mediator.Send(new GetProfitReportQuery(startDate, endDate));
        return Ok(result);
    }
}
