using AutoMapper;
using MediatR;
using PlayStation.Application.DTOs.Customer;
using PlayStation.Application.Features.Customers.Queries;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;

namespace PlayStation.Application.Features.Customers.Handlers;

public class GetAllCustomersHandler : IRequestHandler<GetAllCustomersQuery, Result<List<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCustomersHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _unitOfWork.Repository<Customer>().FindAsync(c => !c.IsDeleted);
        var customerDtos = _mapper.Map<List<CustomerDto>>(customers);
        return Result<List<CustomerDto>>.Success(customerDtos);
    }
}

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(request.Id);
        if (customer == null || customer.IsDeleted)
            return Result<CustomerDto>.Failure("Customer not found");

        var customerDto = _mapper.Map<CustomerDto>(customer);
        return Result<CustomerDto>.Success(customerDto);
    }
}

public class SearchCustomersHandler : IRequestHandler<SearchCustomersQuery, Result<List<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchCustomersHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<CustomerDto>>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _unitOfWork.Repository<Customer>().FindAsync(c =>
            !c.IsDeleted &&
            (c.Name.Contains(request.SearchTerm) ||
             (c.Email != null && c.Email.Contains(request.SearchTerm)) ||
             (c.PhoneNumber != null && c.PhoneNumber.Contains(request.SearchTerm))));

        var customerDtos = _mapper.Map<List<CustomerDto>>(customers);
        return Result<List<CustomerDto>>.Success(customerDtos);
    }
}

public class GetCustomersPaginatedHandler : IRequestHandler<GetCustomersPaginatedQuery, Result<PagedResult<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomersPaginatedHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CustomerDto>>> Handle(GetCustomersPaginatedQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Customer>().Query()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.Where(c => c.Name.Contains(request.SearchTerm) ||
                (c.Email != null && c.Email.Contains(request.SearchTerm)) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(request.SearchTerm)));

        var totalCount = query.Count();
        var items = query
            .OrderBy(c => c.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var customerDtos = _mapper.Map<List<CustomerDto>>(items);

        var pagedResult = new PagedResult<CustomerDto>
        {
            Items = customerDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<CustomerDto>>.Success(pagedResult);
    }
}
