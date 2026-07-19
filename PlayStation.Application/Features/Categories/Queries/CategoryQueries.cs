using MediatR;
using PlayStation.Application.DTOs.Category;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Categories.Queries;

public record GetAllCategoriesQuery() : IRequest<Result<List<CategoryDto>>>;
public record GetCategoryByIdQuery(int Id) : IRequest<Result<CategoryDto>>;
