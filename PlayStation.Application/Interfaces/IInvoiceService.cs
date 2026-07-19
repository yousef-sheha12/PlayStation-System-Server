using PlayStation.Domain.Common;

namespace PlayStation.Application.Interfaces;

public interface IInvoiceService
{
    Task<Result<int>> GenerateInvoiceAsync(int sessionId);
}
