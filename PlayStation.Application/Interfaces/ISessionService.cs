using PlayStation.Domain.Common;

namespace PlayStation.Application.Interfaces;

public interface ISessionService
{
    Task<Result<int>> StartSessionAsync(int deviceId, int? customerId);
    Task<Result> PauseSessionAsync(int sessionId);
    Task<Result> ResumeSessionAsync(int sessionId);
    Task<Result<int>> EndSessionAsync(int sessionId);
    Task<Result> AddProductToSessionAsync(int sessionId, int productId, int quantity);
    Task<Result> RemoveProductFromSessionAsync(int sessionId, int productId);
}
