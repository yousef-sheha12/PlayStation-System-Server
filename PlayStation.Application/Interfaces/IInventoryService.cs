using PlayStation.Application.DTOs.Product;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Interfaces;

public interface IInventoryService
{
    Task<Result> IncreaseQuantityAsync(int productId, int quantity);
    Task<Result> DecreaseQuantityAsync(int productId, int quantity);
    Task<Result<List<ProductStockAlertDto>>> GetLowStockAlertsAsync();
    Task<Result<List<ProductStockAlertDto>>> GetOutOfStockAlertsAsync();
}
