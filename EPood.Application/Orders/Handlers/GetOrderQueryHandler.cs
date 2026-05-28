using EPood.Application.Common;
using EPood.Application.Orders.Dtos;
using EPood.Application.Orders.Queries;
using EPood.Application.Repositories;

namespace EPood.Application.Orders.Handlers;

public class GetOrderQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OperationResult<OrderDto>> Handle(GetOrderQuery request)
    {
        if (request == null || request.Id <= 0)
        {
            return OperationResult<OrderDto>.Fail("Invalid order id");
        }

        var order = await _orderRepository.GetById(request.Id);

        if (order == null)
        {
            return OperationResult<OrderDto>.Fail("Order not found");
        }

        return OperationResult<OrderDto>.Ok(new OrderDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CreatedAt = order.CreatedAt,
            ProductId = order.ProductId,
            ProductName = order.Product?.Name,
            Quantity = order.Quantity,
            TotalPrice = order.TotalPrice
        });
    }
}