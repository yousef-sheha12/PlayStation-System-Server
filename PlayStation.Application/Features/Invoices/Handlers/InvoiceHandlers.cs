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

public class GenerateInvoiceHandler : IRequestHandler<GenerateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GenerateInvoiceHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDto>> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<Session>().Query()
            .Include(s => s.Device)
            .FirstOrDefaultAsync(s => s.Id == request.Request.SessionId && !s.IsDeleted, cancellationToken);

        if (session == null)
            return Result<InvoiceDto>.Failure("Session not found");

        if (session.Status != SessionStatus.Ended)
            return Result<InvoiceDto>.Failure("Session must be ended before generating invoice");

        var existingInvoice = (await _unitOfWork.Repository<Invoice>().FindAsync(i =>
            i.SessionId == request.Request.SessionId && !i.IsDeleted)).FirstOrDefault();

        if (existingInvoice != null)
            return Result<InvoiceDto>.Failure("Invoice already exists for this session");

        var sessionProducts = (await _unitOfWork.Repository<SessionProduct>().FindAsync(sp => sp.SessionId == request.Request.SessionId)).ToList();

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks.ToString()[^6..]}";

        var invoice = new Invoice
        {
            SessionId = request.Request.SessionId,
            InvoiceNumber = invoiceNumber,
            SubTotal = session.TotalCost + session.Discount,
            Discount = session.Discount,
            TaxRate = request.Request.TaxRate,
            TaxAmount = session.TotalCost * request.Request.TaxRate / 100,
            TotalAmount = session.TotalCost + (session.TotalCost * request.Request.TaxRate / 100),
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

        if (sessionProducts.Any())
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdInvoice = await _unitOfWork.Repository<Invoice>().Query()
            .Include(i => i.Session).ThenInclude(s => s.Device)
            .Include(i => i.Session).ThenInclude(s => s.Customer)
            .Include(i => i.InvoiceItems).ThenInclude(ii => ii.Product)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

        var invoiceDto = _mapper.Map<InvoiceDto>(createdInvoice!);
        return Result<InvoiceDto>.Success(invoiceDto, "Invoice generated successfully");
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
