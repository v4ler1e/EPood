using EPood.Application.Common;
using EPood.Application.Products.Commands;
using EPood.Application.Repositories;

namespace EPood.Application.Products.Handlers;

public class DeleteProductCommandHandler
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<OperationResult<bool>> Handle(DeleteProductCommand request)
    {
        if (request == null || request.Id <= 0)
        {
            return OperationResult<bool>.Fail("Invalid product id");
        }

        var product = await _productRepository.GetById(request.Id);

        if (product == null)
        {
            return OperationResult<bool>.Fail("Product not found");
        }

        _productRepository.Delete(product);
        await _productRepository.SaveChanges();

        return OperationResult<bool>.Ok(true);
    }
}