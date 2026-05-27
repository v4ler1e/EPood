using EPood.Application.Common;
using EPood.Application.Products.Dtos;
using EPood.Application.Products.Queries;
using EPood.Application.Repositories;

namespace EPood.Application.Products.Handlers;

public class GetProductQueryHandler
{
    private readonly IProductRepository _productRepository;

    public GetProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<OperationResult<ProductDto>> Handle(GetProductQuery request)
    {
        if (request == null || request.Id <= 0)
        {
            return OperationResult<ProductDto>.Fail("Invalid product id");
        }

        var product = await _productRepository.GetById(request.Id);

        if (product == null)
        {
            return OperationResult<ProductDto>.Fail("Product not found");
        }

        return OperationResult<ProductDto>.Ok(new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name
        });
    }
}