using EPood.Domain.Entities;
using EPood.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPood.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public OrdersController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
    {
        var query = _dbContext.Orders
            .Include(x => x.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.CustomerName.Contains(search) ||
                x.Product!.Name.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var orders = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.CustomerName,
                x.CreatedAt,
                x.ProductId,
                ProductName = x.Product != null ? x.Product.Name : null,
                x.Quantity,
                x.TotalPrice
            })
            .ToListAsync();

        return Ok(new
        {
            Items = orders,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var order = await _dbContext.Orders
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            order.Id,
            order.CustomerName,
            order.CreatedAt,
            order.ProductId,
            ProductName = order.Product?.Name,
            order.Quantity,
            order.TotalPrice
        });
    }

    [HttpPost]
    public async Task<IActionResult> Add(Order order)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == order.ProductId);

        if (product == null)
        {
            return BadRequest("Product not found");
        }

        if (string.IsNullOrWhiteSpace(order.CustomerName))
        {
            return BadRequest("Customer name is required");
        }

        if (order.Quantity <= 0)
        {
            return BadRequest("Quantity must be greater than zero");
        }

        order.CreatedAt = DateTime.Now;
        order.TotalPrice = product.Price * order.Quantity;

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        return Ok(order.Id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Order order)
    {
        var existingOrder = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existingOrder == null)
        {
            return NotFound();
        }

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == order.ProductId);

        if (product == null)
        {
            return BadRequest("Product not found");
        }

        if (string.IsNullOrWhiteSpace(order.CustomerName))
        {
            return BadRequest("Customer name is required");
        }

        if (order.Quantity <= 0)
        {
            return BadRequest("Quantity must be greater than zero");
        }

        existingOrder.CustomerName = order.CustomerName;
        existingOrder.ProductId = order.ProductId;
        existingOrder.Quantity = order.Quantity;
        existingOrder.TotalPrice = product.Price * order.Quantity;

        await _dbContext.SaveChangesAsync();

        return Ok(existingOrder.Id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}