using AutoMapper;
using MediatR;
using PlayStation.Application.DTOs.Expense;
using PlayStation.Application.Features.Expenses.Commands;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;

namespace PlayStation.Application.Features.Expenses.Handlers;

public class CreateExpenseHandler : IRequestHandler<CreateExpenseCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateExpenseHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = _mapper.Map<Expense>(request.Expense);
        await _unitOfWork.Repository<Expense>().AddAsync(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(expense.Id, "Expense created successfully");
    }
}

public class UpdateExpenseHandler : IRequestHandler<UpdateExpenseCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateExpenseHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _unitOfWork.Repository<Expense>().GetByIdAsync(request.Id);
        if (expense == null)
            return Result.Failure("Expense not found");

        _mapper.Map(request.Expense, expense);
        expense.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Expense>().UpdateAsync(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Expense updated successfully");
    }
}

public class DeleteExpenseHandler : IRequestHandler<DeleteExpenseCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteExpenseHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _unitOfWork.Repository<Expense>().GetByIdAsync(request.Id);
        if (expense == null)
            return Result.Failure("Expense not found");

        expense.IsDeleted = true;
        expense.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Expense>().UpdateAsync(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Expense deleted successfully");
    }
}
