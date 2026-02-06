using Catalog.Api.Data.Repositories;
using Catalog.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoriesRepository _repo;

    public CategoriesController(CategoriesRepository repo)
    {
        _repo = repo;
    }

    // GET /api/catalog/categories
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAllAsync();
        return Ok(items);
    }

    // GET /api/catalog/categories/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Category not found" });
        return Ok(item);
    }

    // POST /api/catalog/categories  (Admin only)
    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "name is required" });

        var created = await _repo.CreateAsync(req.Name.Trim());

        // keeping your existing dynamic approach
        return Created($"/api/catalog/categories/{((dynamic)created).Id}", created);
    }

    // PUT /api/catalog/categories/{id}  (Admin only)
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCategoryRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "name is required" });

        var updated = await _repo.UpdateAsync(id, req.Name.Trim());
        if (updated == null) return NotFound(new { message = "Category not found" });

        return Ok(updated);
    }

    // DELETE /api/catalog/categories/{id}  (Admin only)
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        // ✅ NEW: block delete when products exist -> return 409 instead of 500
        if (await _repo.HasProductsAsync(id))
        {
            return Conflict(new
            {
                message = "Cannot delete this category because products are using it. Move/delete those products first."
            });
        }

        var ok = await _repo.DeleteAsync(id);
        if (!ok) return NotFound(new { message = "Category not found" });

        return NoContent();
    }
}
