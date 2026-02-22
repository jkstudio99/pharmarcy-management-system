using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Requests;
using PharmacyApi.DTOs.Responses;
using PharmacyApi.Models;

namespace PharmacyApi.Controllers;

/// <summary>
/// CRUD operations for medicine categories.
/// Read access: AllStaff · Write access: PharmacistUp · Delete: AdminOnly.
/// </summary>
[Route("api/[controller]")]
public class CategoriesController(PharmacyDbContext db) : ApiControllerBase
{
    /// <summary>
    /// List all active categories, ordered alphabetically.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AllStaff")]
    [ProducesResponseType(typeof(ApiResponse<List<Category>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<Category>>>> GetAll(CancellationToken ct)
    {
        var categories = await db.Categories
            .OrderBy(c => c.CategoryName)
            .ToListAsync(ct);

        return Ok(ApiResponse<List<Category>>.Ok(categories));
    }

    /// <summary>
    /// Get a single category by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "AllStaff")]
    [ProducesResponseType(typeof(ApiResponse<Category>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Category>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Category>>> GetById(int id, CancellationToken ct)
    {
        var cat = await db.Categories.FindAsync([id], ct);
        if (cat is null)
            return NotFound(ApiResponse<Category>.Fail("Category not found."));

        return Ok(ApiResponse<Category>.Ok(cat));
    }

    /// <summary>
    /// Create a new category. Requires Pharmacist or Admin role.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "PharmacistUp")]
    [ProducesResponseType(typeof(ApiResponse<Category>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Category>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Category>>> Create(
        [FromBody] CategoryRequest req,
        CancellationToken ct)
    {
        var duplicate = await db.Categories
            .AnyAsync(c => c.CategoryName == req.CategoryName, ct);

        if (duplicate)
            return BadRequest(ApiResponse<Category>.Fail($"Category '{req.CategoryName}' already exists."));

        var cat = new Category
        {
            CategoryName = req.CategoryName,
            Description = req.Description,
            CreatedAt = DateTime.UtcNow
        };

        db.Categories.Add(cat);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = cat.CategoryId },
            ApiResponse<Category>.Ok(cat, "Category created."));
    }

    /// <summary>
    /// Update an existing category. Requires Pharmacist or Admin role.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "PharmacistUp")]
    [ProducesResponseType(typeof(ApiResponse<Category>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Category>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Category>>> Update(
        int id,
        [FromBody] CategoryRequest req,
        CancellationToken ct)
    {
        var cat = await db.Categories.FindAsync([id], ct);
        if (cat is null)
            return NotFound(ApiResponse<Category>.Fail("Category not found."));

        cat.CategoryName = req.CategoryName;
        cat.Description = req.Description;
        cat.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<Category>.Ok(cat, "Category updated."));
    }

    /// <summary>
    /// Soft-delete a category (sets IsActive = false). Requires Admin role.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
    {
        var cat = await db.Categories.FindAsync([id], ct);
        if (cat is null)
            return NotFound(ApiResponse<string>.Fail("Category not found."));

        cat.IsActive = false;
        cat.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<string>.Ok("Category deactivated."));
    }
}
