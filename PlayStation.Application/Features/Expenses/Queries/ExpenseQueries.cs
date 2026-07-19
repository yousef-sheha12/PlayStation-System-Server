using MediatR;
using PlayStation.Application.DTOs.Expense;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Expenses.Queries;

public record GetAllExpensesQuery() : IRequest<Result<List<ExpenseDto>>>;
public record GetExpenseByIdQuery(int Id) : IRequest<Result<ExpenseDto>>;
public record GetExpensesByDateRangeQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<List<ExpenseDto>>>;
public record GetExpensesPaginatedQuery(int PageNumber, int PageSize, string? SearchTerm, DateTime? StartDate, DateTime? EndDate) : IRequest<Result<PagedResult<ExpenseDto>>>;
