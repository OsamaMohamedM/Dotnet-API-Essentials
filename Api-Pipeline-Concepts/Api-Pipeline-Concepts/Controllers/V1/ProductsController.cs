using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Api_Pipeline_Concepts.DTOs;
using Api_Pipeline_Concepts.Exceptions;

namespace Api_Pipeline_Concepts.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    private static readonly List<(int Id, string Name, decimal Price, string Category)> _products =
    [
        (1, "Laptop", 999.99m, "Electronics"),
        (2, "Notebook", 2.99m, "Stationery"),
    ];

    [HttpGet]
    public IActionResult GetAll()
    {
        var names = _products.Select(p => p.Name).ToList();
        return Ok(names);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == default)
            throw new NotFoundException("Product", id);

        return Ok(new { product.Id, product.Name, product.Price, product.Category });
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductDto dto)
    {
        var newId = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
        _products.Add((newId, dto.Name, dto.Price, dto.Category));
        return CreatedAtAction(nameof(GetById), new { id = newId, version = "1" },
            new { Id = newId, dto.Name, dto.Price, dto.Category });
    }

    [HttpGet("error")]
    public IActionResult TriggerError()
    {
        throw new InvalidOperationException("This is a deliberate unhandled exception for demo purposes.");
    }
}
