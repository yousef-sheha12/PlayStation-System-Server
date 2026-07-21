using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var session = await _unitOfWork.Repository<Session>().Query()
            .Include(s => s.Device)
            .FirstOrDefaultAsync(s => s.Id == request.Request.SessionId && !s.IsDeleted, cancellationToken);

        if (session == null)
            return Result<int>.Failure("Session not found");

        if (session.Status != SessionStatus.Ended)
            return Result<int>.Failure("Session must be ended before generating invoice");

        var existingInvoice = (await _unitOfWork.Repository<Invoice>().FindAsync(i =>
            i.SessionId == request.Request.SessionId && !i.IsDeleted)).FirstOrDefault();

        if (existingInvoice != null)
            return Result<int>.Failure("Invoice already exists for this session");

        var sessionProducts = (await _unitOfWork.Repository<SessionProduct>().FindAsync(sp => sp.SessionId == request.Request.SessionId)).ToList();

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks.ToString()[^6..]}";

        var discount = request.Request.Discount;
        var taxRate = request.Request.TaxRate;
        var subTotal = session.TotalCost;
        var taxableAmount = subTotal - discount;
        var taxAmount = taxableAmount * taxRate / 100;
        var totalAmount = taxableAmount + taxAmount;

        var invoice = new Invoice
        {
            SessionId = request.Request.SessionId,
            InvoiceNumber = invoiceNumber,
            SubTotal = subTotal,
            Discount = discount,
            TaxRate = taxRate,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            PaymentMethod = request.Request.PaymentMethod,
            IsPaid = true,
            PaidAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
