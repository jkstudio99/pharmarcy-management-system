using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Requests;
using PharmacyApi.DTOs.Responses;
using PharmacyApi.Models;

namespace PharmacyApi.Controllers;

/// <summary>
/// Supplier management.
/// Read access: AllStaff · Write/Delete: AdminOnly.
/// </summary>
[Route("api/[controller]")]
public class SuppliersController(PharmacyDbContext db) : ApiControllerBase
{
    /// <summary>
    /// List all active suppliers. Supports search by name or contact.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AllStaff")]
    [ProducesResponseType(typeof(ApiResponse<List<Supplier>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<Supplier>>>> GetAll(
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = db.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s =>
                s.SupplierName.Contains(search)
                || (s.Contact != null && s.Contact.Contains(search)));

        var suppliers = await query
            .OrderBy(s => s.SupplierName)
            .ToListAsync(ct);

        return Ok(ApiResponse<List<Supplier>>.Ok(suppliers));
    }

    /// <summary>
    /// Get a single supplier by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "AllStaff")]
    [ProducesResponseType(typeof(ApiResponse<Supplier>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Supplier>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Supplier>>> GetById(int id, CancellationToken ct)
    {
        var sup = await db.Suppliers.FindAsync([id], ct);
        if (sup is null)
            return NotFound(ApiResponse<Supplier>.Fail("Supplier not found."));

        return Ok(ApiResponse<Supplier>.Ok(sup));
    }

    /// <summary>
    /// Create a new supplier. Requires Admin role.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<Supplier>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Supplier>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Supplier>>> Create(
        [FromBody] SupplierRequest req,
        CancellationToken ct)
    {
        var duplicate = await db.Suppliers
            .AnyAsync(s => s.SupplierName == req.SupplierName, ct);

        if (duplicate)
            return BadRequest(ApiResponse<Supplier>.Fail($"Supplier '{req.SupplierName}' already exists."));

        var sup = new Supplier
        {
            SupplierName = req.SupplierName,
            Contact = req.Contact,
            Address = req.Address,
            CreatedAt = DateTime.UtcNow
        };

        db.Suppliers.Add(sup);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = sup.SupplierId },
            ApiResponse<Supplier>.Ok(sup, "Supplier created."));
    }

    /// <summary>
    /// Update an existing supplier. Requires Admin role.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<Supplier>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Supplier>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Supplier>>> Update(
        int id,
        [FromBody] SupplierRequest req,
        CancellationToken ct)
    {
        var sup = await db.Suppliers.FindAsync([id], ct);
        if (sup is null)
            return NotFound(ApiResponse<Supplier>.Fail("Supplier not found."));

        sup.SupplierName = req.SupplierName;
        sup.Contact = req.Contact;
        sup.Address = req.Address;
        sup.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<Supplier>.Ok(sup, "Supplier updated."));
    }

    /// <summary>
    /// Soft-delete a supplier (sets IsActive = false). Requires Admin role.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
    {
        var sup = await db.Suppliers.FindAsync([id], ct);
        if (sup is null)
            return NotFound(ApiResponse<string>.Fail("Supplier not found."));

        sup.IsActive = false;
        sup.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<string>.Ok("Supplier deactivated."));
    }
}
