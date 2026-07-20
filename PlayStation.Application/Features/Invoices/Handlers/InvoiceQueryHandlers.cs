using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayStation.Application.DTOs.Invoice;
using PlayStation.Application.Features.Invoices.Queries;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;

namespace PlayStation.Application.Features.Invoices.Handlers;

public class GetAllInvoicesHandler : IRequestHandler<GetAllInvoicesQuery, Result<List<InvoiceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllInvoicesHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<InvoiceDto>>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _unitOfWork.Repository<Invoice>().Query()
            .Include(i => i.Session).ThenInclude(s => s.Device)
            .Include(i => i.Session).ThenInclude(s => s.Customer)
            .Include(i => i.InvoiceItems).ThenInclude(ii => ii.Product)
            .Where(i => !i.IsDeleted)
            .ToListAsync();
        var invoiceDtos = _mapper.Map<List<InvoiceDto>>(invoices);
        return Result<List<InvoiceDto>>.Success(invoiceDtos);
    }
}

public class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetInvoiceByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Repository<Invoice>().Query()
            .Include(i => i.Session).ThenInclude(s => s.Device)
            .Include(i => i.Session).ThenInclude(s => s.Customer)
            .Include(i => i.InvoiceItems).ThenInclude(ii => ii.Product)
            .FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDeleted);
        if (invoice == null)
            return Result<InvoiceDto>.Failure("Invoice not found");

        var invoiceDto = _mapper.Map<InvoiceDto>(invoice);
        return Result<InvoiceDto>.Success(invoiceDto);
    }
}

public class GetInvoiceBySessionHandler : IRequestHandler<GetInvoiceBySessionQuery, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetInvoiceBySessionHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceBySessionQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Repository<Invoice>().Query()
            .Include(i => i.Session).ThenInclude(s => s.Device)
            .Include(i => i.Session).ThenInclude(s => s.Customer)
            .Include(i => i.InvoiceItems).ThenInclude(ii => ii.Product)
            .FirstOrDefaultAsync(i => i.SessionId == request.SessionId && !i.IsDeleted);

        if (invoice == null)
            return Result<InvoiceDto>.Failure("Invoice not found for this session");

        var invoiceDto = _mapper.Map<InvoiceDto>(invoice);
        return Result<InvoiceDto>.Success(invoiceDto);
    }
}
