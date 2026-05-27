using EPood.Domain.Entities;

namespace EPood.Application.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAll();

    Task<Product?> GetById(int id);

    Task<bool> CategoryExists(int categoryId);

    Task Add(Product product);

    void Delete(Product product);

    Task SaveChanges();
}