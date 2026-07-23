using MediatR;
using PlayStation.Application.DTOs.Invoice;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Invoices.Commands;

public record GenerateInvoiceCommand(GenerateInvoiceDto Request) : IRequest<Result<InvoiceDto>>;
public record UpdateInvoicePaymentCommand(int InvoiceId, bool IsPaid) : IRequest<Result>;
