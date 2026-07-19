using MediatR;
using PlayStation.Application.DTOs.Invoice;
using PlayStation.Application.Features.Invoices.Commands;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;
using PlayStation.Domain.Enums;

namespace PlayStation.Application.Features.Invoices.Handlers;

public class GenerateInvoiceHandler : IRequestHandler<GenerateInvoiceCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GenerateInvoiceHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(request.Request.SessionId);
        if (session == null || session.IsDeleted)
            return Result<int>.Failure("Session not found");

        if (session.Status != SessionStatus.Ended)
            return Result<int>.Failure("Session must be ended before generating invoice");

        var existingInvoice = (await _unitOfWork.Repository<Invoice>().FindAsync(i =>
            i.SessionId == request.Request.SessionId && !i.IsDeleted)).FirstOrDefault();

        if (existingInvoice != null)
            return Result<int>.Failure("Invoice already exists for this session");

        var sessionProducts = await _unitOfWork.Repository<SessionProduct>().FindAsync(sp => sp.SessionId == request.Request.SessionId);

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks.ToString()[^6..]}";

        var invoice = new Invoice
        {
            SessionId = request.Request.SessionId,
            InvoiceNumber = invoiceNumber,
            SubTotal = session.TotalCost,
            Discount = request.Request.Discount,
            TaxRate = request.Request.TaxRate,
            TaxAmount = (session.TotalCost - request.Request.Discount) * request.Request.TaxRate / 100,
            TotalAmount = (session.TotalCost - request.Request.Discount) + ((session.TotalCost - request.Request.Discount) * request.Request.TaxRate / 100),
            PaymentMethod = request.Request.PaymentMethod,
            IsPaid = true,
            PaidAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Invoice>().AddAsync(invoice);

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

        session.Discount = request.Request.Discount;
        session.TotalCost = session.DeviceCost + session.ProductsCost - request.Request.Discount;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Session>().UpdateAsync(session);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(invoice.Id, "Invoice generated successfully");
    }
}

public class UpdateInvoicePaymentHandler : IRequestHandler<UpdateInvoicePaymentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInvoicePaymentHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateInvoicePaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(request.InvoiceId);
        if (invoice == null || invoice.IsDeleted)
            return Result.Failure("Invoice not found");

        invoice.IsPaid = request.IsPaid;
        if (request.IsPaid)
            invoice.PaidAt = DateTime.UtcNow;
        else
            invoice.PaidAt = null;

        invoice.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Invoice>().UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Invoice payment updated successfully");
    }
}
