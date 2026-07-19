using AutoMapper;
using MediatR;
using PlayStation.Application.DTOs.Customer;
using PlayStation.Application.Features.Customers.Commands;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;

namespace PlayStation.Application.Features.Customers.Handlers;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCustomerHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = _mapper.Map<Customer>(request.Customer);
        await _unitOfWork.Repository<Customer>().AddAsync(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(customer.Id, "Customer created successfully");
    }
}

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCustomerHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(request.Id);
        if (customer == null)
            return Result.Failure("Customer not found");

        _mapper.Map(request.Customer, customer);
        customer.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Customer>().UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Customer updated successfully");
    }
}

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(request.Id);
        if (customer == null)
            return Result.Failure("Customer not found");

        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Customer>().UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Customer deleted successfully");
    }
}
