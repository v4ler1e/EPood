using EPood.Application.Products.Commands;
using EPood.Application.Products.Handlers;
using EPood.Application.Products.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EPood.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly GetProductQueryHandler _getHandler;
    private readonly ListProductsQueryHandler _listHandler;
    private readonly SaveProductCommandHandler _saveHandler;
    private readonly DeleteProductCommandHandler _deleteHandler;

    public ProductsController(
        GetProductQueryHandler getHandler,
        ListProductsQueryHandler listHandler,
        SaveProductCommandHandler saveHandler,
        DeleteProductCommandHandler deleteHandler)
    {
        _getHandler = getHandler;
        _listHandler = listHandler;
        _saveHandler = saveHandler;
        _deleteHandler = deleteHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ListProductsQuery query)
    {
        var result = await _listHandler.Handle(query);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var query = new GetProductQuery
        {
            Id = id
        };

        var result = await _getHandler.Handle(query);

        if (!result.Success)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Save(
        [FromBody] SaveProductCommand command)
    {
        var result = await _saveHandler.Handle(command);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
    int id,
    [FromBody] SaveProductCommand command)
    {
        command.Id = id;

        var result = await _saveHandler.Handle(command);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteProductCommand
        {
            Id = id
        };

        var result = await _deleteHandler.Handle(command);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}