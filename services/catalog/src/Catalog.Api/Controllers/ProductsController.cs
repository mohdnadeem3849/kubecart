using Catalog.Api.Data.Repositories;
using Catalog.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductsRepository _repo;

    public ProductsController(ProductsRepository repo)
    {
        _repo = repo;
    }

    // GET /api/catalog/products
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAllAsync();
        return Ok(items);
    }

    // GET /api/catalog/products/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Product not found" });
        return Ok(item);
    }

    // GET /api/catalog/products/by-category/{categoryId}
    [HttpGet("by-category/{categoryId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCategory([FromRoute] int categoryId)
    {
        var items = await _repo.GetByCategoryIdAsync(categoryId);
        return Ok(items);
    }

    // POST /api/catalog/products (Admin only)
    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
    {
        if (req == null) return BadRequest(new { message = "body is required" });

        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "name is required" });

        if (req.Price <= 0)
            return BadRequest(new { message = "price must be > 0" });

        if (req.CategoryId <= 0)
            return BadRequest(new { message = "categoryId is required" });

        if (req.Stock < 0)
            return BadRequest(new { message = "stock must be >= 0" });

        var created = await _repo.CreateAsync(
            req.Name.Trim(),
            req.Description?.Trim() ?? "",
            req.Price,
            req.CategoryId,
            req.ImageUrl?.Trim(),
            req.Stock
        );

        if (created == null)
            return BadRequest(new { message = "Invalid categoryId (category not found)" });

        return Created($"/api/catalog/products/{((dynamic)created).Id}", created);
    }

    // PUT /api/catalog/products/{id} (Admin only)
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateProductRequest req)
    {
        if (req == null) return BadRequest(new { message = "Body is required" });

        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "name is required" });

        if (req.Price <= 0)
            return BadRequest(new { message = "price must be > 0" });

        if (req.CategoryId <= 0)
            return BadRequest(new { message = "categoryId is required" });

        if (req.Stock.HasValue && req.Stock.Value < 0)
            return BadRequest(new { message = "stock must be >= 0" });

        // ✅ First check product exists
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Product not found" });

        // ✅ If stock not sent, keep old stock from DB
        int stockToUse = req.Stock ?? (int)((dynamic)existing).Stock;

        var updated = await _repo.UpdateAsync(
            id,
            req.Name.Trim(),
            req.Description?.Trim(),
            req.Price,
            req.CategoryId,
            req.ImageUrl?.Trim(),
            stockToUse
        );

        if (updated == null)
            return BadRequest(new { message = "Invalid categoryId (category not found)" });

        return Ok(updated);
    }


    // DELETE /api/catalog/products/{id} (Admin only)
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return NotFound(new { message = "Product not found" });
        return NoContent();
    }
}
