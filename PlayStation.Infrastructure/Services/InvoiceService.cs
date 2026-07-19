using PlayStation.Application.DTOs.Invoice;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;
using PlayStation.Domain.Enums;

namespace PlayStation.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> GenerateInvoiceAsync(int sessionId)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionId);
        if (session == null || session.IsDeleted)
            return Result<int>.Failure("Session not found");

        if (session.Status != SessionStatus.Ended)
            return Result<int>.Failure("Session must be ended before generating invoice");

        var existingInvoice = (await _unitOfWork.Repository<Invoice>().FindAsync(i =>
            i.SessionId == sessionId && !i.IsDeleted)).FirstOrDefault();

        if (existingInvoice != null)
            return Result<int>.Failure("Invoice already exists for this session");

        var sessionProducts = await _unitOfWork.Repository<SessionProduct>().FindAsync(sp => sp.SessionId == sessionId);

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks.ToString()[^6..]}";

        var invoice = new Invoice
        {
            SessionId = sessionId,
            InvoiceNumber = invoiceNumber,
            SubTotal = session.TotalCost,
            Discount = session.Discount,
            TaxRate = 0,
            TaxAmount = 0,
            TotalAmount = session.TotalCost,
            PaymentMethod = PaymentMethod.Cash,
            IsPaid = true,
            PaidAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        foreach (var sessionProduct in sessionProducts)
        {
            var invoiceItem = new InvoiceItem
            {
                InvoiceId = invoice.Id,
                ProductId = sessionProduct.ProductId,
                Quantity = sessionProduct.Quantity,
                UnitPrice = sessionProduct.UnitPrice
            };
            await _unitOfWork.Repository<InvoiceItem>().AddAsync(invoiceItem);
        }

        await _unitOfWork.SaveChangesAsync();

        return Result<int>.Success(invoice.Id, "Invoice generated successfully");
    }
}
