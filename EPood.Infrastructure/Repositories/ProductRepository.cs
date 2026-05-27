using EPood.Application.Repositories;
using EPood.Domain.Entities;
using EPood.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EPood.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Product>> GetAll()
    {
        return await _dbContext.Products
            .Include(x => x.Category)
            .ToListAsync();
    }

    public async Task<Product?> GetById(int id)
    {
        return await _dbContext.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> CategoryExists(int categoryId)
    {
        return await _dbContext.Categories
            .AnyAsync(x => x.Id == categoryId);
    }

    public async Task Add(Product product)
    {
        await _dbContext.Products.AddAsync(product);
    }

    public void Delete(Product product)
    {
        _dbContext.Products.Remove(product);
    }

    public async Task SaveChanges()
    {
        await _dbContext.SaveChangesAsync();
    }
}