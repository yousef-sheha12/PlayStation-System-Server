using MediatR;
using PlayStation.Application.DTOs.Expense;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Expenses.Commands;

public record CreateExpenseCommand(CreateExpenseDto Expense) : IRequest<Result<int>>;
public record UpdateExpenseCommand(int Id, UpdateExpenseDto Expense) : IRequest<Result>;
public record DeleteExpenseCommand(int Id) : IRequest<Result>;
