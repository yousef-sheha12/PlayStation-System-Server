using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayStation.Application.Interfaces;

namespace PlayStation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockAlerts()
    {
        var result = await _inventoryService.GetLowStockAlertsAsync();
        return Ok(result);
    }

    [HttpGet("out-of-stock")]
    public async Task<IActionResult> GetOutOfStockAlerts()
    {
        var result = await _inventoryService.GetOutOfStockAlertsAsync();
        return Ok(result);
    }

    [HttpPost("{productId}/increase")]
    public async Task<IActionResult> IncreaseQuantity(int productId, [FromBody] QuantityRequest request)
    {
        var result = await _inventoryService.IncreaseQuantityAsync(productId, request.Quantity);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{productId}/decrease")]
    public async Task<IActionResult> DecreaseQuantity(int productId, [FromBody] QuantityRequest request)
    {
        var result = await _inventoryService.DecreaseQuantityAsync(productId, request.Quantity);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}

public class QuantityRequest
{
    public int Quantity { get; set; }
}
