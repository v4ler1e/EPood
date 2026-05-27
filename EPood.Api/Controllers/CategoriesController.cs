using EPood.Domain.Entities;
using EPood.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPood.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public CategoriesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var categories = await _dbContext.Categories
            .Select(x => new
            {
                x.Id,
                x.Name
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest("Category name is required");
        }

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return Ok(category.Id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Category category)
    {
        var existingCategory = await _dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existingCategory == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest("Category name is required");
        }

        existingCategory.Name = category.Name;

        await _dbContext.SaveChangesAsync();

        return Ok(existingCategory.Id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _dbContext.Categories
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Products.Any())
        {
            return BadRequest("Cannot delete category with products");
        }

        _dbContext.Categories.Remove(category);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}