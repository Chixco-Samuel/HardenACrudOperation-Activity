using HardenACrud.Exceptions;
using HardenACrud.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HardenACrud.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Keyboard", Description = "Mechanical keyboard", Price = 59.99m },
        new Product { Id = 2, Name = "Mouse",    Description = "Wireless mouse",      Price = 24.50m },
        new Product { Id = 3, Name = "Monitor",  Description = "27-inch 4K monitor",  Price = 329.00m }
    };

    private static int _nextId = 4;

    private static Product FindOrThrow(int id) =>
        _products.FirstOrDefault(p => p.Id == id)
        ?? throw new NotFoundException(nameof(Product), id);

    private static void EnsureNameIsUnique(string name, int? ignoreId = null)
    {
        if (_products.Any(p => p.Id != ignoreId &&
                               string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"A product named '{name}' already exists.");
        }
    }

    [HttpGet]
    public ActionResult<PagedResponse<Product>> GetAll(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        var totalCount = _products.Count;

        var data = _products
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new PagedResponse<Product>
        {
            Data = data,
            Meta = new PageMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                PageCount = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id) => Ok(FindOrThrow(id));

    [HttpPost]
    public ActionResult<Product> Create(CreateProductRequest request)
    {
        EnsureNameIsUnique(request.Name);

        var product = new Product
        {
            Id = _nextId++,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            SalePrice = request.SalePrice
        };
        _products.Add(product);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public ActionResult<Product> Update(int id, UpdateProductRequest request)
    {
        var existing = FindOrThrow(id);
        EnsureNameIsUnique(request.Name, id);
        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Price = request.Price;
        existing.SalePrice = request.SalePrice;
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existing = FindOrThrow(id);
        _products.Remove(existing);
        return NoContent();
    }
}