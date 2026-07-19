using AutoMapper;
using MediatR;
using PlayStation.Application.DTOs.Category;
using PlayStation.Application.Features.Categories.Commands;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;

namespace PlayStation.Application.Features.Categories.Handlers;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = _mapper.Map<Category>(request.Category);
        await _unitOfWork.Repository<Category>().AddAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(category.Id, "Category created successfully");
    }
}

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.Id);
        if (category == null)
            return Result.Failure("Category not found");

        _mapper.Map(request.Category, category);
        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Category>().UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Category updated successfully");
    }
}

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.Id);
        if (category == null)
            return Result.Failure("Category not found");

        var hasProducts = (await _unitOfWork.Repository<Product>().FindAsync(p => p.CategoryId == request.Id && !p.IsDeleted)).Any();
        if (hasProducts)
            return Result.Failure("Cannot delete category with active products");

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<Category>().UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Category deleted successfully");
    }
}
