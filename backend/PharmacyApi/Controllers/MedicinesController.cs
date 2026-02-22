using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Requests;
using PharmacyApi.DTOs.Responses;
using PharmacyApi.Models;

namespace PharmacyApi.Controllers;

/// <summary>
/// Medicine master-data management.
/// Read access: AllStaff · Write access: PharmacistUp.
/// </summary>
[Route("api/[controller]")]
public class MedicinesController(PharmacyDbContext db) : ApiControllerBase
{
    /// <summary>
    /// Search and list medicines with pagination. Supports filtering by name, barcode, and category.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AllStaff")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MedicineDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<MedicineDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.Medicines
            .AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.InventoryBatches.Where(b => b.IsActive))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m =>
                m.DrugName.Contains(search)
                || (m.GenericName != null && m.GenericName.Contains(search))
                || (m.Barcode != null && m.Barcode.Contains(search)));

        if (categoryId.HasValue)
            query = query.Where(m => m.CategoryId == categoryId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(m => m.DrugName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MedicineDto
            {
                DrugId = m.DrugId,
                Barcode = m.Barcode,
                DrugName = m.DrugName,
                GenericName = m.GenericName,
                Unit = m.Unit,
                CategoryId = m.CategoryId,
                CategoryName = m.Category != null ? m.Category.CategoryName : null,
                ReorderLevel = m.ReorderLevel,
                ImageUrl = m.ImageUrl,
                TotalStock = m.InventoryBatches.Sum(b => b.QuantityInStock),
                IsLowStock = m.InventoryBatches.Sum(b => b.QuantityInStock) <= m.ReorderLevel,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(ct);

        var result = new PagedResult<MedicineDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PagedResult<MedicineDto>>.Ok(result));
    }

    /// <summary>
    /// Get detailed medicine information including active inventory batches (FEFO ordered).
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "AllStaff")]
    [ProducesResponseType(typeof(ApiResponse<MedicineDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MedicineDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MedicineDetailDto>>> GetById(int id, CancellationToken ct)
    {
        var m = await db.Medicines
            .AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.InventoryBatches.Where(b => b.IsActive))
                .ThenInclude(b => b.Supplier)
            .FirstOrDefaultAsync(m => m.DrugId == id, ct);

        if (m is null)
            return NotFound(ApiResponse<MedicineDetailDto>.Fail("Medicine not found."));

        var totalStock = m.InventoryBatches.Sum(b => b.QuantityInStock);

        var dto = new MedicineDetailDto
        {
            DrugId = m.DrugId,
            Barcode = m.Barcode,
            DrugName = m.DrugName,
            GenericName = m.GenericName,
            Unit = m.Unit,
            CategoryId = m.CategoryId,
            CategoryName = m.Category?.CategoryName,
            ReorderLevel = m.ReorderLevel,
            ImageUrl = m.ImageUrl,
            TotalStock = totalStock,
            IsLowStock = totalStock <= m.ReorderLevel,
            CreatedAt = m.CreatedAt,
            Batches = m.InventoryBatches
                .OrderBy(b => b.ExpDate)
                .Select(b => new BatchSummaryDto
                {
                    BatchId = b.BatchId,
                    BatchNumber = b.BatchNumber,
                    QuantityInStock = b.QuantityInStock,
                    CostPrice = b.CostPrice,
                    SellingPrice = b.SellingPrice,
                    MfgDate = b.MfgDate,
                    ExpDate = b.ExpDate,
                    SupplierName = b.Supplier?.SupplierName
                }).ToList()
        };

        return Ok(ApiResponse<MedicineDetailDto>.Ok(dto));
    }

    /// <summary>
    /// Create a new medicine record. Requires Pharmacist or Admin role.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "PharmacistUp")]
    [ProducesResponseType(typeof(ApiResponse<MedicineDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<MedicineDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MedicineDto>>> Create(
        [FromBody] MedicineRequest req,
        CancellationToken ct)
    {
        if (req.CategoryId.HasValue
            && !await db.Categories.AnyAsync(c => c.CategoryId == req.CategoryId, ct))
            return BadRequest(ApiResponse<MedicineDto>.Fail("Category not found."));

        if (!string.IsNullOrEmpty(req.Barcode)
            && await db.Medicines.AnyAsync(m => m.Barcode == req.Barcode, ct))
            return BadRequest(ApiResponse<MedicineDto>.Fail($"Barcode '{req.Barcode}' is already in use."));

        var medicine = new Medicine
        {
            Barcode = req.Barcode,
            DrugName = req.DrugName,
            GenericName = req.GenericName,
            Unit = req.Unit,
            CategoryId = req.CategoryId,
            ReorderLevel = req.ReorderLevel,
            ImageUrl = req.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        db.Medicines.Add(medicine);
        await db.SaveChangesAsync(ct);

        var dto = new MedicineDto
        {
            DrugId = medicine.DrugId,
            Barcode = medicine.Barcode,
            DrugName = medicine.DrugName,
            GenericName = medicine.GenericName,
            Unit = medicine.Unit,
            CategoryId = medicine.CategoryId,
            ReorderLevel = medicine.ReorderLevel,
            ImageUrl = medicine.ImageUrl,
            TotalStock = 0,
            IsLowStock = true,
            CreatedAt = medicine.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = medicine.DrugId },
            ApiResponse<MedicineDto>.Ok(dto, "Medicine created."));
    }

    /// <summary>
    /// Update an existing medicine record. Requires Pharmacist or Admin role.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "PharmacistUp")]
    [ProducesResponseType(typeof(ApiResponse<MedicineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MedicineDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MedicineDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MedicineDto>>> Update(
        int id,
        [FromBody] MedicineRequest req,
        CancellationToken ct)
    {
        var medicine = await db.Medicines.FindAsync([id], ct);
        if (medicine is null)
            return NotFound(ApiResponse<MedicineDto>.Fail("Medicine not found."));

        if (req.CategoryId.HasValue
            && !await db.Categories.AnyAsync(c => c.CategoryId == req.CategoryId, ct))
            return BadRequest(ApiResponse<MedicineDto>.Fail("Category not found."));

        if (!string.IsNullOrEmpty(req.Barcode)
            && await db.Medicines.AnyAsync(m => m.Barcode == req.Barcode && m.DrugId != id, ct))
            return BadRequest(ApiResponse<MedicineDto>.Fail($"Barcode '{req.Barcode}' is already in use."));

        medicine.Barcode = req.Barcode;
        medicine.DrugName = req.DrugName;
        medicine.GenericName = req.GenericName;
        medicine.Unit = req.Unit;
        medicine.CategoryId = req.CategoryId;
        medicine.ReorderLevel = req.ReorderLevel;
        medicine.ImageUrl = req.ImageUrl;
        medicine.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var totalStock = await db.InventoryBatches
            .Where(b => b.DrugId == id)
            .SumAsync(b => b.QuantityInStock, ct);

        var dto = new MedicineDto
        {
            DrugId = medicine.DrugId,
            Barcode = medicine.Barcode,
            DrugName = medicine.DrugName,
            GenericName = medicine.GenericName,
            Unit = medicine.Unit,
            CategoryId = medicine.CategoryId,
            ReorderLevel = medicine.ReorderLevel,
            ImageUrl = medicine.ImageUrl,
            TotalStock = totalStock,
            IsLowStock = totalStock <= medicine.ReorderLevel,
            CreatedAt = medicine.CreatedAt
        };

        return Ok(ApiResponse<MedicineDto>.Ok(dto, "Medicine updated."));
    }

    /// <summary>
    /// Upload or replace the packaging image for a medicine. Accepts JPEG/PNG/WEBP up to 5 MB.
    /// File is saved under wwwroot/images/medicines/. Requires Pharmacist or Admin role.
    /// </summary>
    [HttpPost("{id:int}/image")]
    [Authorize(Policy = "PharmacistUp")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(
        int id,
        IFormFile file,
        CancellationToken ct)
    {
        var medicine = await db.Medicines.FindAsync([id], ct);
        if (medicine is null)
            return NotFound(ApiResponse<string>.Fail("Medicine not found."));

        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file provided."));

        const long maxBytes = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxBytes)
            return BadRequest(ApiResponse<string>.Fail("File size must not exceed 5 MB."));

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(ApiResponse<string>.Fail("Only JPEG, PNG, and WEBP images are allowed."));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"medicine_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "medicines");

        Directory.CreateDirectory(uploadDir);

        var filePath = Path.Combine(uploadDir, fileName);
        await using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream, ct);

        medicine.ImageUrl = $"/images/medicines/{fileName}";
        medicine.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<string>.Ok(medicine.ImageUrl, "Image uploaded successfully."));
    }

    /// <summary>
    /// Soft-delete a medicine (sets IsActive = false). Requires Pharmacist or Admin role.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "PharmacistUp")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
    {
        var medicine = await db.Medicines.FindAsync([id], ct);
        if (medicine is null)
            return NotFound(ApiResponse<string>.Fail("Medicine not found."));

        medicine.IsActive = false;
        medicine.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<string>.Ok("Medicine deactivated."));
    }
}
