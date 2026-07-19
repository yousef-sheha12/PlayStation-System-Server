using AutoMapper;
using MediatR;
using PlayStation.Application.DTOs.Expense;
using PlayStation.Application.Features.Expenses.Queries;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;

namespace PlayStation.Application.Features.Expenses.Handlers;

public class GetAllExpensesHandler : IRequestHandler<GetAllExpensesQuery, Result<List<ExpenseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllExpensesHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ExpenseDto>>> Handle(GetAllExpensesQuery request, CancellationToken cancellationToken)
    {
        var expenses = await _unitOfWork.Repository<Expense>().FindAsync(e => !e.IsDeleted);
        var expenseDtos = _mapper.Map<List<ExpenseDto>>(expenses);
        return Result<List<ExpenseDto>>.Success(expenseDtos);
    }
}

public class GetExpenseByIdHandler : IRequestHandler<GetExpenseByIdQuery, Result<ExpenseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExpenseByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ExpenseDto>> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await _unitOfWork.Repository<Expense>().GetByIdAsync(request.Id);
        if (expense == null || expense.IsDeleted)
            return Result<ExpenseDto>.Failure("Expense not found");

        var expenseDto = _mapper.Map<ExpenseDto>(expense);
        return Result<ExpenseDto>.Success(expenseDto);
    }
}

public class GetExpensesByDateRangeHandler : IRequestHandler<GetExpensesByDateRangeQuery, Result<List<ExpenseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExpensesByDateRangeHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ExpenseDto>>> Handle(GetExpensesByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var expenses = await _unitOfWork.Repository<Expense>().FindAsync(e =>
            !e.IsDeleted &&
            e.ExpenseDate >= request.StartDate &&
            e.ExpenseDate <= request.EndDate.AddDays(1));

        var expenseDtos = _mapper.Map<List<ExpenseDto>>(expenses);
        return Result<List<ExpenseDto>>.Success(expenseDtos);
    }
}

public class GetExpensesPaginatedHandler : IRequestHandler<GetExpensesPaginatedQuery, Result<PagedResult<ExpenseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExpensesPaginatedHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ExpenseDto>>> Handle(GetExpensesPaginatedQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Expense>().Query()
            .Where(e => !e.IsDeleted);

        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.Where(e => e.Description.Contains(request.SearchTerm));

        if (request.StartDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= request.EndDate.Value.AddDays(1));

        var totalCount = query.Count();
        var items = query
            .OrderByDescending(e => e.ExpenseDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var expenseDtos = _mapper.Map<List<ExpenseDto>>(items);

        var pagedResult = new PagedResult<ExpenseDto>
        {
            Items = expenseDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<ExpenseDto>>.Success(pagedResult);
    }
}
