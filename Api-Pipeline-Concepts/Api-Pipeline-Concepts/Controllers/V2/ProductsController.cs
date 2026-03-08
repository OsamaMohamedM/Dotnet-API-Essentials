using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Api_Pipeline_Concepts.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
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
        var products = _products.Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            p.Category
        }).ToList();

        return Ok(new
        {
            ApiVersion = "2.0",
            Count = products.Count,
            Products = products
        });
    }
}
