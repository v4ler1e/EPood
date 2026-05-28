using EPood.Domain.Entities;

namespace EPood.Application.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAll();

    Task<Order?> GetById(int id);

    Task Add(Order order);

    void Delete(Order order);

    Task SaveChanges();
}
