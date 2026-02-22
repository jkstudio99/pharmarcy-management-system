using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Responses;

namespace PharmacyApi.Controllers;

/// <summary>
/// Read-only stock transaction history with filtering and pagination.
/// Accessible by all authenticated staff.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class TransactionsController(PharmacyDbContext db) : ApiControllerBase
{
    /// <summary>
    /// List stock transactions. Supports filtering by batch, type (IN/OUT/ADJUST/EXPIRED),
    /// and date range. Ordered newest-first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<TransactionDto>>>> GetAll(
        [FromQuery] int? batchId,
        [FromQuery] string? transType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.StockTransactions
            .AsNoTracking()
            .Include(t => t.InventoryBatch)
                .ThenInclude(b => b.Medicine)
            .Include(t => t.Employee)
            .AsQueryable();

        if (batchId.HasValue)
            query = query.Where(t => t.BatchId == batchId);
        if (!string.IsNullOrEmpty(transType))
            query = query.Where(t => t.TransType == transType);
        if (from.HasValue)
            query = query.Where(t => t.CreatedAt >= from);
        if (to.HasValue)
            query = query.Where(t => t.CreatedAt <= to);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto
            {
                TransactionId = t.TransactionId,
                BatchId = t.BatchId,
                BatchNumber = t.InventoryBatch.BatchNumber,
                DrugName = t.InventoryBatch.Medicine.DrugName,
                EmployeeName = t.Employee.EmpName,
                TransType = t.TransType,
                ReferenceNo = t.ReferenceNo,
                Quantity = t.Quantity,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<PagedResult<TransactionDto>>.Ok(new PagedResult<TransactionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        }));
    }
}
