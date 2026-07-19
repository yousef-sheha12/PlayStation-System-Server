using MediatR;
using PlayStation.Application.DTOs.Customer;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Customers.Commands;

public record CreateCustomerCommand(CreateCustomerDto Customer) : IRequest<Result<int>>;
public record UpdateCustomerCommand(int Id, UpdateCustomerDto Customer) : IRequest<Result>;
public record DeleteCustomerCommand(int Id) : IRequest<Result>;
