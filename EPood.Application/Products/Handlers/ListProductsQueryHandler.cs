using EPood.Application.Common;
using EPood.Application.Products.Dtos;
using EPood.Application.Products.Queries;
using EPood.Application.Repositories;

namespace EPood.Application.Products.Handlers;

public class ListProductsQueryHandler
{
    private readonly IProductRepository _productRepository;

    public ListProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<OperationResult<PagedResult<ProductDto>>> Handle(ListProductsQuery request)
    {
        if (request == null)
        {
            return OperationResult<PagedResult<ProductDto>>.Fail("Request is null");
        }

        if (request.Page <= 0)
        {
            return OperationResult<PagedResult<ProductDto>>.Fail("Page must be greater than zero");
        }

        if (request.PageSize <= 0)
        {
            return OperationResult<PagedResult<ProductDto>>.Fail("Page size must be greater than zero");
        }

        var products = await _productRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            products = products
                .Where(x =>
                    x.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                    (x.Description != null &&
                     x.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Category != null &&
                     x.Category.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        products = request.SortBy?.ToLower() switch
        {
            "name" => request.Descending
                ? products.OrderByDescending(x => x.Name).ToList()
                : products.OrderBy(x => x.Name).ToList(),

            "price" => request.Descending
                ? products.OrderByDescending(x => x.Price).ToList()
                : products.OrderBy(x => x.Price).ToList(),

            "stock" => request.Descending
                ? products.OrderByDescending(x => x.Stock).ToList()
                : products.OrderBy(x => x.Stock).ToList(),

            "category" => request.Descending
                ? products.OrderByDescending(x => x.Category != null ? x.Category.Name : "").ToList()
                : products.OrderBy(x => x.Category != null ? x.Category.Name : "").ToList(),

            _ => products.OrderBy(x => x.Id).ToList()
        };

        var totalCount = products.Count;

        var items = products
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Stock = x.Stock,
                CategoryId = x.CategoryId,
                CategoryName = x.Category?.Name
            })
            .ToList();

        return OperationResult<PagedResult<ProductDto>>.Ok(new PagedResult<ProductDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}