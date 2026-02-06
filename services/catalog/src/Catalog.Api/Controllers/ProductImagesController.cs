using Catalog.Api.Data.Repositories;
using Catalog.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/products/{productId:int}/images")]
public sealed class ProductImagesController : ControllerBase
{
    private readonly ProductImagesRepository _repo;

    public ProductImagesController(ProductImagesRepository repo)
    {
        _repo = repo;
    }

    // GET /api/catalog/products/{productId}/images
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var images = await _repo.GetByProductIdAsync(productId);
        return Ok(images);
    }

    // POST /api/catalog/products/{productId}/images (Admin)
    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Add(int productId, [FromBody] AddProductImageRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Url))
            return BadRequest(new { message = "url is required" });

        var created = await _repo.AddAsync(
            productId,
            req.Url.Trim(),
            req.IsPrimary
        );

        if (created == null)
            return BadRequest(new { message = "Invalid productId (product not found)" });

        return Created(
            $"/api/catalog/products/{productId}/images/{((dynamic)created).Id}",
            created
        );
    }

    // DELETE /api/catalog/products/{productId}/images/{id}
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int productId, int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok)
            return NotFound(new { message = "Image not found" });

        return NoContent();
    }
}
