using MediatR;
using PlayStation.Application.DTOs.Product;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Products.Queries;

public record GetAllProductsQuery() : IRequest<Result<List<ProductDto>>>;
public record GetProductByIdQuery(int Id) : IRequest<Result<ProductDto>>;
public record SearchProductsQuery(string SearchTerm) : IRequest<Result<List<ProductDto>>>;
public record GetProductsPaginatedQuery(int PageNumber, int PageSize, string? SearchTerm, int? CategoryId, bool? IsLowStock) : IRequest<Result<PagedResult<ProductDto>>>;
