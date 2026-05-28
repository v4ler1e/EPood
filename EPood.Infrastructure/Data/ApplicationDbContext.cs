using EPood.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EPood.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
}