using MediatR;
using PlayStation.Application.DTOs.Invoice;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Invoices.Queries;

public record GetAllInvoicesQuery() : IRequest<Result<List<InvoiceDto>>>;
public record GetInvoiceByIdQuery(int Id) : IRequest<Result<InvoiceDto>>;
public record GetInvoiceBySessionQuery(int SessionId) : IRequest<Result<InvoiceDto>>;
