using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayStation.Application.DTOs.Invoice;
using PlayStation.Application.DTOs.Session;
using PlayStation.Application.Features.Sessions.Commands;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;
using PlayStation.Domain.Enums;

namespace PlayStation.Application.Features.Sessions.Handlers;

public class StartSessionHandler : IRequestHandler<StartSessionCommand, Result<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StartSessionHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SessionDto>> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Repository<Device>().GetByIdAsync(request.Request.DeviceId);
        if (device == null || device.IsDeleted)
            return Result<SessionDto>.Failure("Device not found");

        if (device.Status != DeviceStatus.Available)
            return Result<SessionDto>.Failure("Device is not available");

        var session = new Session
        {
            DeviceId = request.Request.DeviceId,
            CustomerName = request.Request.CustomerName,
            HourlyRate = request.Request.HourlyRate,
            StartTime = DateTime.UtcNow,
            Status = SessionStatus.Active
        };

        device.Status = DeviceStatus.Occupied;
        await _unitOfWork.Repository<Device>().UpdateAsync(device);
        await _unitOfWork.Repository<Session>().AddAsync(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sessionDto = _mapper.Map<SessionDto>(session);
        sessionDto.DeviceName = device.Name;
        sessionDto.CustomerName = request.Request.CustomerName;
        return Result<SessionDto>.Success(sessionDto, "Session started successfully");
    }
}

public class PauseSessionHandler : IRequestHandler<PauseSessionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public PauseSessionHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(PauseSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(request.SessionId);
        if (session == null || session.IsDeleted)
            return Result.Failure("Session not found");

        if (session.Status != SessionStatus.Active)
            return Result.Failure("Session is not active");

        session.Status = SessionStatus.Paused;
        session.PauseTime = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Session>().UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Session paused successfully");
    }
}

public class ResumeSessionHandler : IRequestHandler<ResumeSessionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public ResumeSessionHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResumeSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(request.SessionId);
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Session resumed successfully");
    }
}

public class EndSessionHandler : IRequestHandler<EndSessionCommand, Result<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EndSessionHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SessionDto>> Handle(EndSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(request.SessionId);
        if (session == null || session.IsDeleted)
            return Result<SessionDto>.Failure("Session not found");

        if (session.Status == SessionStatus.Ended)
            return Result<SessionDto>.Failure("Session already ended");

        session.EndTime = DateTime.UtcNow;
        session.Status = SessionStatus.Ended;
        session.UpdatedAt = DateTime.UtcNow;
        session.Discount = request.Discount;

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

        var sessionProducts = await _unitOfWork.Repository<SessionProduct>().FindAsync(sp => sp.SessionId == request.SessionId);
        session.ProductsCost = sessionProducts.Sum(sp => sp.TotalPrice);

        session.TotalCost = session.DeviceCost + session.ProductsCost - session.Discount;

        await _unitOfWork.Repository<Session>().UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sessionDto = _mapper.Map<SessionDto>(session);
        return Result<SessionDto>.Success(sessionDto, "Session ended successfully");
    }
}

public class AddProductToSessionHandler : IRequestHandler<AddProductToSessionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddProductToSessionHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddProductToSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(request.SessionId);
        if (session == null || session.IsDeleted)
            return Result.Failure("Session not found");

        if (session.Status != SessionStatus.Active && session.Status != SessionStatus.Paused)
            return Result.Failure("Session is not active or paused");

        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.Request.ProductId);
        if (product == null || product.IsDeleted)
            return Result.Failure("Product not found");

        if (product.Quantity < request.Request.Quantity)
            return Result.Failure("Insufficient product quantity");

        var existingProduct = (await _unitOfWork.Repository<SessionProduct>().FindAsync(sp =>
            sp.SessionId == request.SessionId && sp.ProductId == request.Request.ProductId)).FirstOrDefault();

        if (existingProduct != null)
        {
            existingProduct.Quantity += request.Request.Quantity;
            existingProduct.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<SessionProduct>().UpdateAsync(existingProduct);
        }
        else
        {
            var sessionProduct = new SessionProduct
            {
                SessionId = request.SessionId,
                ProductId = request.Request.ProductId,
                Quantity = request.Request.Quantity,
                UnitPrice = product.Price
            };
            await _unitOfWork.Repository<SessionProduct>().AddAsync(sessionProduct);
        }

        product.Quantity -= request.Request.Quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Product>().UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Product added to session successfully");
    }
}

public class RemoveProductFromSessionHandler : IRequestHandler<RemoveProductFromSessionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProductFromSessionHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveProductFromSessionCommand request, CancellationToken cancellationToken)
    {
        var sessionProduct = (await _unitOfWork.Repository<SessionProduct>().FindAsync(sp =>
            sp.SessionId == request.SessionId && sp.ProductId == request.ProductId)).FirstOrDefault();

        if (sessionProduct == null)
            return Result.Failure("Product not found in session");

        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.ProductId);
        if (product != null)
        {
            product.Quantity += sessionProduct.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Product>().UpdateAsync(product);
        }

        await _unitOfWork.Repository<SessionProduct>().DeleteAsync(sessionProduct);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Product removed from session successfully");
    }
}

public class EndSessionAndGenerateInvoiceHandler : IRequestHandler<EndSessionAndGenerateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EndSessionAndGenerateInvoiceHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDto>> Handle(EndSessionAndGenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var sessionId = request.SessionId;
        var req = request.Request;

        var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionId);
        if (session == null || session.IsDeleted)
            return Result<InvoiceDto>.Failure("Session not found");

        if (session.Status == SessionStatus.Ended)
        {
            var existingForEnded = await _unitOfWork.Repository<Invoice>().Query()
                .AsNoTracking()
                .Include(i => i.Session).ThenInclude(s => s.Device)
                .Include(i => i.Session).ThenInclude(s => s.Customer)
                .Include(i => i.InvoiceItems).ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync(i => i.SessionId == sessionId && !i.IsDeleted, cancellationToken);

            if (existingForEnded != null)
                return Result<InvoiceDto>.Success(_mapper.Map<InvoiceDto>(existingForEnded), "Invoice already exists");

            return Result<InvoiceDto>.Failure("Session already ended");
        }

        session.EndTime = DateTime.UtcNow;
        session.Status = SessionStatus.Ended;
        session.UpdatedAt = DateTime.UtcNow;
        session.Discount = req.Discount;

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

        var sessionProducts = (await _unitOfWork.Repository<SessionProduct>().FindAsync(sp => sp.SessionId == sessionId)).ToList();
        session.ProductsCost = sessionProducts.Sum(sp => sp.TotalPrice);
        session.TotalCost = session.DeviceCost + session.ProductsCost - session.Discount;

        await _unitOfWork.Repository<Session>().UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var existingInvoice = await _unitOfWork.Repository<Invoice>().Query()
            .AsNoTracking()
            .Include(i => i.Session).ThenInclude(s => s.Device)
            .Include(i => i.Session).ThenInclude(s => s.Customer)
            .Include(i => i.InvoiceItems).ThenInclude(ii => ii.Product)
            .FirstOrDefaultAsync(i => i.SessionId == sessionId && !i.IsDeleted, cancellationToken);

        if (existingInvoice != null)
            return Result<InvoiceDto>.Success(_mapper.Map<InvoiceDto>(existingInvoice), "Invoice already exists");

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks.ToString()[^6..]}";

        var invoice = new Invoice
        {
            SessionId = sessionId,
            InvoiceNumber = invoiceNumber,
            SubTotal = session.TotalCost + session.Discount,
            Discount = session.Discount,
            TaxRate = req.TaxRate,
            TaxAmount = session.TotalCost * req.TaxRate / 100,
            TotalAmount = session.TotalCost + (session.TotalCost * req.TaxRate / 100),
            PaymentMethod = req.PaymentMethod,
            IsPaid = true,
            PaidAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Invoice>().AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var sp in sessionProducts)
        {
            var invoiceItem = new InvoiceItem
            {
                InvoiceId = invoice.Id,
                ProductId = sp.ProductId,
                Quantity = sp.Quantity,
                UnitPrice = sp.UnitPrice
            };
            await _unitOfWork.Repository<InvoiceItem>().AddAsync(invoiceItem);
        }

        if (sessionProducts.Any())
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdInvoice = await _unitOfWork.Repository<Invoice>().Query()
            .AsNoTracking()
            .Include(i => i.Session).ThenInclude(s => s.Device)
            .Include(i => i.Session).ThenInclude(s => s.Customer)
            .Include(i => i.InvoiceItems).ThenInclude(ii => ii.Product)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id, cancellationToken);

        if (createdInvoice == null)
            return Result<InvoiceDto>.Failure("Invoice was created but could not be retrieved");

        var invoiceDto = _mapper.Map<InvoiceDto>(createdInvoice);
        return Result<InvoiceDto>.Success(invoiceDto, "Session ended and invoice generated successfully");
    }
}
