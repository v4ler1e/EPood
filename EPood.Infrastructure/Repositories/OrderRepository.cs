using EPood.Application.Repositories;
using EPood.Domain.Entities;
using EPood.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EPood.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Order>> GetAll()
    {
        return await _dbContext.Orders
            .Include(x => x.Product)
            .ToListAsync();
    }

    public async Task<Order?> GetById(int id)
    {
        return await _dbContext.Orders
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task Add(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
    }

    public void Delete(Order order)
    {
        _dbContext.Orders.Remove(order);
    }

    public async Task SaveChanges()
    {
        await _dbContext.SaveChangesAsync();
    }
}