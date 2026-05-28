using EPood.Application.Common;
using EPood.Application.Orders.Commands;
using EPood.Application.Repositories;
using EPood.Domain.Entities;

namespace EPood.Application.Orders.Handlers;

public class SaveOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public SaveOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<OperationResult<int>> Handle(SaveOrderCommand request)
    {
        if (request == null)
        {
            return OperationResult<int>.Fail("Request is null");
        }

        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return OperationResult<int>.Fail("Customer name is required");
        }

        if (request.ProductId <= 0)
        {
            return OperationResult<int>.Fail("Product is required");
        }

        if (request.Quantity <= 0)
        {
            return OperationResult<int>.Fail("Quantity must be greater than zero");
        }

        var product = await _productRepository.GetById(request.ProductId);

        if (product == null)
        {
            return OperationResult<int>.Fail("Product not found");
        }

        Order order;

        if (request.Id == 0)
        {
            order = new Order
            {
                CreatedAt = DateTime.Now
            };

            await _orderRepository.Add(order);
        }
        else
        {
            order = await _orderRepository.GetById(request.Id);

            if (order == null)
            {
                return OperationResult<int>.Fail("Order not found");
            }
        }

        order.CustomerName = request.CustomerName;
        order.ProductId = request.ProductId;
        order.Quantity = request.Quantity;
        order.TotalPrice = product.Price * request.Quantity;

        await _orderRepository.SaveChanges();

        return OperationResult<int>.Ok(order.Id);
    }
}