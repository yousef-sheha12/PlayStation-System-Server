using PlayStation.Application.DTOs.Session;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;
using PlayStation.Domain.Enums;

namespace PlayStation.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;

    public SessionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> StartSessionAsync(int deviceId, int? customerId)
    {
        var device = await _unitOfWork.Repository<Device>().GetByIdAsync(deviceId);
        if (device == null || device.IsDeleted)
            return Result<int>.Failure("Device not found");

        if (device.Status != DeviceStatus.Available)
            return Result<int>.Failure("Device is not available");

        if (customerId.HasValue)
        {
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId.Value);
            if (customer == null || customer.IsDeleted)
                return Result<int>.Failure("Customer not found");
        }

        var session = new Session
        {
            DeviceId = deviceId,
            CustomerId = customerId,
            StartTime = DateTime.UtcNow,
            Status = SessionStatus.Active
        };

        device.Status = DeviceStatus.Occupied;
        await _unitOfWork.Repository<Device>().UpdateAsync(device);
        await _unitOfWork.Repository<Session>().AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return Result<int>.Success(session.Id, "Session started successfully");
    }

    public async Task<Result> PauseSessionAsync(int sessionId)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionId);
        if (session == null || session.IsDeleted)
            return Result.Failure("Session not found");

        if (session.Status != SessionStatus.Active)
            return Result.Failure("Session is not active");

        session.Status = SessionStatus.Paused;
        session.PauseTime = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Session>().UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Session paused successfully");
    }

    public async Task<Result> ResumeSessionAsync(int sessionId)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionId);
        if (session == null || session.IsDeleted)
            return Result.Failure("Session not found");

        if (session.Status != SessionStatus.Paused)
            return Result.Failure("Session is not paused");

        if (session.PauseTime.HasValue)
        {
            var pauseDuration = DateTime.UtcNow - session.PauseTime.Value;
            session.TotalPauseDuration = (session.TotalPauseDuration ?? TimeSpan.Zero) + pauseDuration;
        }

        session.Status = SessionStatus.Active;
        session.PauseTime = null;
        session.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Session>().UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Session resumed successfully");
    }

    public async Task<Result<int>> EndSessionAsync(int sessionId)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionId);
        if (session == null || session.IsDeleted)
            return Result<int>.Failure("Session not found");

        if (session.Status == SessionStatus.Ended)
            return Result<int>.Failure("Session already ended");

        session.EndTime = DateTime.UtcNow;
        session.Status = SessionStatus.Ended;
        session.UpdatedAt = DateTime.UtcNow;

        if (session.PauseTime.HasValue)
        {
            var pauseDuration = DateTime.UtcNow - session.PauseTime.Value;
            session.TotalPauseDuration = (session.TotalPauseDuration ?? TimeSpan.Zero) + pauseDuration;
        }

        var totalDuration = session.EndTime.Value - session.StartTime;
        if (session.TotalPauseDuration.HasValue)
            totalDuration -= session.TotalPauseDuration.Value;

        session.TotalHours = (decimal)totalDuration.TotalHours;

        var device = await _unitOfWork.Repository<Device>().GetByIdAsync(session.DeviceId);
        if (device != null)
        {
            session.DeviceCost = session.TotalHours * session.HourlyRate;
            device.Status = DeviceStatus.Available;
            await _unitOfWork.Repository<Device>().UpdateAsync(device);
        }

        var sessionProducts = await _unitOfWork.Repository<SessionProduct>().FindAsync(sp => sp.SessionId == sessionId);
        session.ProductsCost = sessionProducts.Sum(sp => sp.TotalPrice);

        session.TotalCost = session.DeviceCost + session.ProductsCost - session.Discount;

        await _unitOfWork.Repository<Session>().UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return Result<int>.Success(session.Id, "Session ended successfully");
    }

    public async Task<Result> AddProductToSessionAsync(int sessionId, int productId, int quantity)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionId);
        if (session == null || session.IsDeleted)
            return Result.Failure("Session not found");

        if (session.Status != SessionStatus.Active && session.Status != SessionStatus.Paused)
            return Result.Failure("Session is not active or paused");

        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
        if (product == null || product.IsDeleted)
            return Result.Failure("Product not found");

        if (product.Quantity < quantity)
            return Result.Failure("Insufficient product quantity");

        var existingProduct = (await _unitOfWork.Repository<SessionProduct>().FindAsync(sp =>
            sp.SessionId == sessionId && sp.ProductId == productId)).FirstOrDefault();

        if (existingProduct != null)
        {
            existingProduct.Quantity += quantity;
            existingProduct.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<SessionProduct>().UpdateAsync(existingProduct);
        }
        else
        {
            var sessionProduct = new SessionProduct
            {
                SessionId = sessionId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };
            await _unitOfWork.Repository<SessionProduct>().AddAsync(sessionProduct);
        }

        product.Quantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Product>().UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Product added to session successfully");
    }

    public async Task<Result> RemoveProductFromSessionAsync(int sessionId, int productId)
    {
        var sessionProduct = (await _unitOfWork.Repository<SessionProduct>().FindAsync(sp =>
            sp.SessionId == sessionId && sp.ProductId == productId)).FirstOrDefault();

        if (sessionProduct == null)
            return Result.Failure("Product not found in session");

        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
        if (product != null)
        {
            product.Quantity += sessionProduct.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Product>().UpdateAsync(product);
        }

        await _unitOfWork.Repository<SessionProduct>().DeleteAsync(sessionProduct);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Product removed from session successfully");
    }
}
