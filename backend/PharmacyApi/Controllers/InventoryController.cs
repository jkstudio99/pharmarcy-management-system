using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Requests;
using PharmacyApi.DTOs.Responses;
using PharmacyApi.Models;

namespace PharmacyApi.Controllers;

/// <summary>
/// Inventory batch management: stock-in, FEFO stock-out, adjustments, and alerts.
/// Read/Stock-In/Stock-Out: AllStaff · Adjustment: AdminOnly.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class InventoryController(PharmacyDbContext db) : ApiControllerBase
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// List inventory batches with optional filters and pagination. Ordered by expiry date (FEFO).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryBatchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<InventoryBatchDto>>>> GetAll(
        [FromQuery] int? drugId,
        [FromQuery] int? supplierId,
        [FromQuery] bool? expiringOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.InventoryBatches
            .AsNoTracking()
            .Include(b => b.Medicine)
            .Include(b => b.Supplier)
            .AsQueryable();

        if (drugId.HasValue)
            query = query.Where(b => b.DrugId == drugId);
        if (supplierId.HasValue)
            query = query.Where(b => b.SupplierId == supplierId);
        if (expiringOnly == true)
        {
            var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
            query = query.Where(b => b.ExpDate != null
                && b.ExpDate <= threshold
                && b.QuantityInStock > 0);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(b => b.ExpDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapToBatchDto(b))
            .ToListAsync(ct);

        return Ok(ApiResponse<PagedResult<InventoryBatchDto>>.Ok(new PagedResult<InventoryBatchDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        }));
    }

    /// <summary>
    /// Get a single inventory batch by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InventoryBatchDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InventoryBatchDto>>> GetById(int id, CancellationToken ct)
    {
        var dto = await db.InventoryBatches
            .AsNoTracking()
            .Include(b => b.Medicine)
            .Include(b => b.Supplier)
            .Where(b => b.BatchId == id)
            .Select(b => MapToBatchDto(b))
            .FirstOrDefaultAsync(ct);

        if (dto is null)
            return NotFound(ApiResponse<InventoryBatchDto>.Fail("Batch not found."));

        return Ok(ApiResponse<InventoryBatchDto>.Ok(dto));
    }

    /// <summary>
    /// Receive stock: creates a new inventory batch and records an IN transaction.
    /// </summary>
    [HttpPost("stock-in")]
    [ProducesResponseType(typeof(ApiResponse<InventoryBatchDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<InventoryBatchDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<InventoryBatchDto>>> StockIn(
        [FromBody] StockInRequest req,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<InventoryBatchDto>.Fail("Invalid token."));

        if (req.Quantity <= 0)
            return BadRequest(ApiResponse<InventoryBatchDto>.Fail("Quantity must be greater than zero."));

        if (!await db.Medicines.AnyAsync(m => m.DrugId == req.DrugId, ct))
            return BadRequest(ApiResponse<InventoryBatchDto>.Fail("Medicine not found."));

        if (req.SupplierId.HasValue
            && !await db.Suppliers.AnyAsync(s => s.SupplierId == req.SupplierId, ct))
            return BadRequest(ApiResponse<InventoryBatchDto>.Fail("Supplier not found."));

        var batch = new InventoryBatch
        {
            DrugId = req.DrugId,
            SupplierId = req.SupplierId,
            BatchNumber = req.BatchNumber,
            QuantityInStock = req.Quantity,
            CostPrice = req.CostPrice,
            SellingPrice = req.SellingPrice,
            MfgDate = req.MfgDate,
            ExpDate = req.ExpDate,
            CreatedAt = DateTime.UtcNow
        };

        db.InventoryBatches.Add(batch);
        await db.SaveChangesAsync(ct);

        db.StockTransactions.Add(new StockTransaction
        {
            BatchId = batch.BatchId,
            EId = userId,
            TransType = "IN",
            ReferenceNo = req.ReferenceNo,
            Quantity = req.Quantity,
            Notes = $"Stock-in: {req.Quantity} units, Batch {req.BatchNumber}",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        // Reload with navigation properties for DTO mapping
        var dto = await db.InventoryBatches
            .AsNoTracking()
            .Include(b => b.Medicine)
            .Include(b => b.Supplier)
            .Where(b => b.BatchId == batch.BatchId)
            .Select(b => MapToBatchDto(b))
            .FirstAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = batch.BatchId },
            ApiResponse<InventoryBatchDto>.Ok(dto, "Stock received."));
    }

    /// <summary>
    /// Deduct stock using FEFO (First Expire, First Out) via stored procedure.
    /// </summary>
    [HttpPost("stock-out-fefo")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<string>>> StockOutFefo(
        [FromBody] StockOutFefoRequest req,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<string>.Fail("Invalid token."));

        if (req.Quantity <= 0)
            return BadRequest(ApiResponse<string>.Fail("Quantity must be greater than zero."));

        try
        {
            await db.Database.ExecuteSqlRawAsync(
                "SELECT sp_deduct_stock_fefo({0}, {1}, {2}, {3})",
                [req.DrugId, req.Quantity, userId, req.ReferenceNo ?? ""], ct);

            return Ok(ApiResponse<string>.Ok("FEFO stock deduction completed."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.InnerException?.Message ?? ex.Message));
        }
    }

    /// <summary>
    /// Manually adjust batch quantity with a reason. Requires Admin role.
    /// Records an ADJUST transaction with the delta.
    /// </summary>
    [HttpPost("adjust")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<string>>> Adjust(
        [FromBody] StockAdjustRequest req,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<string>.Fail("Invalid token."));

        if (req.NewQuantity < 0)
            return BadRequest(ApiResponse<string>.Fail("New quantity cannot be negative."));

        var batch = await db.InventoryBatches.FindAsync([req.BatchId], ct);
        if (batch is null)
            return NotFound(ApiResponse<string>.Fail("Batch not found."));

        var oldQty = batch.QuantityInStock;
        batch.QuantityInStock = req.NewQuantity;
        batch.UpdatedAt = DateTime.UtcNow;

        db.StockTransactions.Add(new StockTransaction
        {
            BatchId = req.BatchId,
            EId = userId,
            TransType = "ADJUST",
            Quantity = req.NewQuantity - oldQty,
            Notes = req.Reason,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<string>.Ok($"Stock adjusted from {oldQty} to {req.NewQuantity}."));
    }

    /// <summary>
    /// Get low-stock and expiry alerts (batches expiring within 30 days).
    /// </summary>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(ApiResponse<AlertsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AlertsDto>>> GetAlerts(CancellationToken ct)
    {
        var lowStock = await db.Medicines
            .AsNoTracking()
            .Include(m => m.InventoryBatches.Where(b => b.IsActive))
            .Where(m => m.InventoryBatches.Sum(b => b.QuantityInStock) <= m.ReorderLevel)
            .Select(m => new LowStockAlert
            {
                DrugId = m.DrugId,
                DrugName = m.DrugName,
                ReorderLevel = m.ReorderLevel,
                CurrentStock = m.InventoryBatches.Sum(b => b.QuantityInStock)
            })
            .ToListAsync(ct);

        var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expiring = await db.InventoryBatches
            .AsNoTracking()
            .Include(b => b.Medicine)
            .Where(b => b.ExpDate != null
                && b.ExpDate <= threshold
                && b.QuantityInStock > 0)
            .OrderBy(b => b.ExpDate)
            .Select(b => new ExpiryAlert
            {
                BatchId = b.BatchId,
                DrugName = b.Medicine.DrugName,
                BatchNumber = b.BatchNumber,
                ExpDate = b.ExpDate!.Value,
                QuantityInStock = b.QuantityInStock,
                DaysUntilExpiry = b.ExpDate!.Value.DayNumber - today.DayNumber
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<AlertsDto>.Ok(new AlertsDto
        {
            LowStockAlerts = lowStock,
            ExpiryAlerts = expiring
        }));
    }

    // ── Private helpers ──────────────────────────────────────────

    private static InventoryBatchDto MapToBatchDto(InventoryBatch b) => new()
    {
        BatchId = b.BatchId,
        DrugId = b.DrugId,
        DrugName = b.Medicine.DrugName,
        SupplierId = b.SupplierId,
        SupplierName = b.Supplier?.SupplierName,
        BatchNumber = b.BatchNumber,
        QuantityInStock = b.QuantityInStock,
        CostPrice = b.CostPrice,
        SellingPrice = b.SellingPrice,
        MfgDate = b.MfgDate,
        ExpDate = b.ExpDate,
        IsExpiringSoon = b.ExpDate != null
            && b.ExpDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
        CreatedAt = b.CreatedAt
    };
}
