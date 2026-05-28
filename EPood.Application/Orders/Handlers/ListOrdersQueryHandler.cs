using EPood.Application.Common;
using EPood.Application.Orders.Dtos;
using EPood.Application.Orders.Queries;
using EPood.Application.Repositories;

namespace EPood.Application.Orders.Handlers;

public class ListOrdersQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public ListOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OperationResult<PagedResult<OrderDto>>> Handle(ListOrdersQuery request)
    {
        if (request == null)
        {
            return OperationResult<PagedResult<OrderDto>>.Fail("Request is null");
        }

        if (request.Page <= 0)
        {
            return OperationResult<PagedResult<OrderDto>>.Fail("Page must be greater than zero");
        }

        if (request.PageSize <= 0)
        {
            return OperationResult<PagedResult<OrderDto>>.Fail("Page size must be greater than zero");
        }

        var orders = await _orderRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            orders = orders
                .Where(x =>
                    x.CustomerName.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                    (x.Product != null &&
                     x.Product.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var totalCount = orders.Count;

        var items = orders
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new OrderDto
            {
                Id = x.Id,
                CustomerName = x.CustomerName,
                CreatedAt = x.CreatedAt,
                ProductId = x.ProductId,
                ProductName = x.Product?.Name,
                Quantity = x.Quantity,
                TotalPrice = x.TotalPrice
            })
            .ToList();

        return OperationResult<PagedResult<OrderDto>>.Ok(new PagedResult<OrderDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}