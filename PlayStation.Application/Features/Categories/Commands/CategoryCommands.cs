using MediatR;
using PlayStation.Application.DTOs.Category;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Categories.Commands;

public record CreateCategoryCommand(CreateCategoryDto Category) : IRequest<Result<int>>;
public record UpdateCategoryCommand(int Id, UpdateCategoryDto Category) : IRequest<Result>;
public record DeleteCategoryCommand(int Id) : IRequest<Result>;
