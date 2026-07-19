using MediatR;
using PlayStation.Application.DTOs.Customer;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Customers.Queries;

public record GetAllCustomersQuery() : IRequest<Result<List<CustomerDto>>>;
public record GetCustomerByIdQuery(int Id) : IRequest<Result<CustomerDto>>;
public record SearchCustomersQuery(string SearchTerm) : IRequest<Result<List<CustomerDto>>>;
public record GetCustomersPaginatedQuery(int PageNumber, int PageSize, string? SearchTerm) : IRequest<Result<PagedResult<CustomerDto>>>;
