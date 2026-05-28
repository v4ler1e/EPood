using EPood.Application.Orders.Handlers;
using EPood.Application.Products.Handlers;
using EPood.Application.Repositories;
using EPood.Infrastructure.Data;
using EPood.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<GetProductQueryHandler>();
builder.Services.AddScoped<ListProductsQueryHandler>();
builder.Services.AddScoped<SaveProductCommandHandler>();
builder.Services.AddScoped<DeleteProductCommandHandler>();

builder.Services.AddScoped<GetOrderQueryHandler>();
builder.Services.AddScoped<ListOrdersQueryHandler>();
builder.Services.AddScoped<SaveOrderCommandHandler>();
builder.Services.AddScoped<DeleteOrderCommandHandler>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    dbContext.Database.Migrate();

    SeedData.Generate(dbContext);
}

app.Run();

public partial class Program
{
}