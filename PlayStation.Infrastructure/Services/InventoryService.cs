using PlayStation.Application.DTOs.Product;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;
using PlayStation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PlayStation.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> IncreaseQuantityAsync(int productId, int quantity)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
        if (product == null || product.IsDeleted)
            return Result.Failure("Product not found");

        if (quantity <= 0)
            return Result.Failure("Quantity must be greater than 0");

        product.Quantity += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Product>().UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Quantity increased successfully");
    }

    public async Task<Result> DecreaseQuantityAsync(int productId, int quantity)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
        if (product == null || product.IsDeleted)
            return Result.Failure("Product not found");

        if (quantity <= 0)
            return Result.Failure("Quantity must be greater than 0");

        if (product.Quantity < quantity)
            return Result.Failure("Insufficient quantity");

        product.Quantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Product>().UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Quantity decreased successfully");
    }

    public async Task<Result<List<ProductStockAlertDto>>> GetLowStockAlertsAsync()
    {
        var products = await _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted && p.Quantity <= p.LowStockThreshold && p.Quantity > 0)
            .ToListAsync();

        var dtos = products.Select(p => new ProductStockAlertDto
        {
            Id = p.Id,
            Name = p.Name,
            Quantity = p.Quantity,
            LowStockThreshold = p.LowStockThreshold,
            CategoryName = p.Category?.Name ?? string.Empty
        }).ToList();

        return Result<List<ProductStockAlertDto>>.Success(dtos);
    }

    public async Task<Result<List<ProductStockAlertDto>>> GetOutOfStockAlertsAsync()
    {
        var products = await _unitOfWork.Repository<Product>().Query()
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted && p.Quantity == 0)
            .ToListAsync();

        var dtos = products.Select(p => new ProductStockAlertDto
        {
            Id = p.Id,
            Name = p.Name,
            Quantity = p.Quantity,
            LowStockThreshold = p.LowStockThreshold,
            CategoryName = p.Category?.Name ?? string.Empty
        }).ToList();

        return Result<List<ProductStockAlertDto>>.Success(dtos);
    }
}
