using EPood.Application.Common;
using EPood.Application.Orders.Commands;
using EPood.Application.Repositories;

namespace EPood.Application.Orders.Handlers;

public class DeleteOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OperationResult<bool>> Handle(DeleteOrderCommand request)
    {
        if (request == null || request.Id <= 0)
        {
            return OperationResult<bool>.Fail("Invalid order id");
        }

        var order = await _orderRepository.GetById(request.Id);

        if (order == null)
        {
            return OperationResult<bool>.Fail("Order not found");
        }

        _orderRepository.Delete(order);
        await _orderRepository.SaveChanges();

        return OperationResult<bool>.Ok(true);
    }
}