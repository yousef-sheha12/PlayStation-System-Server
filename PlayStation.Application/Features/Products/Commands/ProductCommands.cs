using MediatR;
using PlayStation.Application.DTOs.Product;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Products.Commands;

public record CreateProductCommand(CreateProductDto Product) : IRequest<Result<int>>;
public record UpdateProductCommand(int Id, UpdateProductDto Product) : IRequest<Result>;
public record DeleteProductCommand(int Id) : IRequest<Result>;
