using AutoMapper;
using MediatR;
using PlayStation.Application.DTOs.Product;
using PlayStation.Application.Features.Products.Queries;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;
using PlayStation.Domain.Entities;

namespace PlayStation.Application.Features.Products.Handlers;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProductsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Repository<Product>().FindAsync(p => !p.IsDeleted);
        var productDtos = _mapper.Map<List<ProductDto>>(products);
        return Result<List<ProductDto>>.Success(productDtos);
    }
}

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.Id);
        if (product == null || product.IsDeleted)
            return Result<ProductDto>.Failure("Product not found");

        var productDto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(productDto);
    }
}

public class SearchProductsHandler : IRequestHandler<SearchProductsQuery, Result<List<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchProductsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ProductDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Repository<Product>().FindAsync(p =>
            !p.IsDeleted &&
            (p.Name.Contains(request.SearchTerm) ||
             (p.Description != null && p.Description.Contains(request.SearchTerm))));

        var productDtos = _mapper.Map<List<ProductDto>>(products);
        return Result<List<ProductDto>>.Success(productDtos);
    }
}

public class GetProductsPaginatedHandler : IRequestHandler<GetProductsPaginatedQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsPaginatedHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Product>().Query()
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.Where(p => p.Name.Contains(request.SearchTerm) ||
                (p.Description != null && p.Description.Contains(request.SearchTerm)));

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.IsLowStock.HasValue && request.IsLowStock.Value)
            query = query.Where(p => p.Quantity <= p.LowStockThreshold && p.Quantity > 0);

        var totalCount = query.Count();
        var items = query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var productDtos = _mapper.Map<List<ProductDto>>(items);

        var pagedResult = new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<ProductDto>>.Success(pagedResult);
    }
}
