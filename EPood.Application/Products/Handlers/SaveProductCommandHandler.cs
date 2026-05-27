using EPood.Application.Common;
using EPood.Application.Products.Commands;
using EPood.Application.Repositories;
using EPood.Domain.Entities;

namespace EPood.Application.Products.Handlers;

public class SaveProductCommandHandler
{
    private readonly IProductRepository _productRepository;

    public SaveProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<OperationResult<int>> Handle(SaveProductCommand request)
    {
        if (request == null)
        {
            return OperationResult<int>.Fail("Request is null");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return OperationResult<int>.Fail("Product name is required");
        }

        if (request.Price <= 0)
        {
            return OperationResult<int>.Fail("Price must be greater than zero");
        }

        if (request.Stock < 0)
        {
            return OperationResult<int>.Fail("Stock cannot be negative");
        }

        var categoryExists = await _productRepository.CategoryExists(request.CategoryId);

        if (!categoryExists)
        {
            return OperationResult<int>.Fail("Category not found");
        }

        Product product;

        if (request.Id == 0)
        {
            product = new Product();
            await _productRepository.Add(product);
        }
        else
        {
            product = await _productRepository.GetById(request.Id);

            if (product == null)
            {
                return OperationResult<int>.Fail("Product not found");
            }
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;

        await _productRepository.SaveChanges();

        return OperationResult<int>.Ok(product.Id);
    }
}