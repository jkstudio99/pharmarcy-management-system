using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Requests;
using PharmacyApi.DTOs.Responses;
using PharmacyApi.Models;

namespace PharmacyApi.Controllers;

/// <summary>
/// Sales order management: list, detail, and create with automatic FEFO stock deduction.
/// Requires Pharmacist or Admin role.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = "PharmacistUp")]
public class SalesController(PharmacyDbContext db) : ApiControllerBase
{
    /// <summary>
    /// List sales orders with optional filters and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SalesOrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SalesOrderDto>>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Employee)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);
        if (from.HasValue)
            query = query.Where(o => o.CreatedAt >= from);
        if (to.HasValue)
            query = query.Where(o => o.CreatedAt <= to);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new SalesOrderDto
            {
                OrderId = o.OrderId,
                EmployeeName = o.Employee.EmpName,
                CustomerInfo = o.CustomerInfo,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<PagedResult<SalesOrderDto>>.Ok(new PagedResult<SalesOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        }));
    }

    /// <summary>
    /// Get detailed sales order including line items with drug and batch information.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SalesOrderDetailDto>>> GetById(int id, CancellationToken ct)
    {
        var order = await db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Employee)
            .Include(o => o.Items)
                .ThenInclude(i => i.InventoryBatch)
                    .ThenInclude(b => b.Medicine)
            .FirstOrDefaultAsync(o => o.OrderId == id, ct);

        if (order is null)
            return NotFound(ApiResponse<SalesOrderDetailDto>.Fail("Order not found."));

        var dto = new SalesOrderDetailDto
        {
            OrderId = order.OrderId,
            EmployeeName = order.Employee.EmpName,
            CustomerInfo = order.CustomerInfo,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new SalesItemDto
            {
                OrderItemId = i.OrderItemId,
                DrugName = i.InventoryBatch.Medicine.DrugName,
                BatchNumber = i.InventoryBatch.BatchNumber,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.Quantity * i.UnitPrice
            }).ToList()
        };

        return Ok(ApiResponse<SalesOrderDetailDto>.Ok(dto));
    }

    /// <summary>
    /// Create a sales order. Automatically deducts stock using FEFO for each line item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SalesOrderDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SalesOrderDetailDto>>> Create(
        [FromBody] CreateSalesOrderRequest req,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<SalesOrderDetailDto>.Fail("Invalid token."));

        if (req.Items.Count == 0)
            return BadRequest(ApiResponse<SalesOrderDetailDto>.Fail("Order must contain at least one item."));

        var referenceNo = $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var order = new SalesOrder
        {
            EId = userId,
            CustomerInfo = req.CustomerInfo,
            PaymentMethod = req.PaymentMethod,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        };

        decimal total = 0;
        var orderItems = new List<SalesOrderItem>();

        foreach (var item in req.Items)
        {
            if (item.Quantity <= 0)
                return BadRequest(ApiResponse<SalesOrderDetailDto>.Fail(
                    $"Quantity must be greater than zero for DrugID {item.DrugId}."));

            // FEFO deduction via stored procedure
            try
            {
                await db.Database.ExecuteSqlRawAsync(
                    "SELECT sp_deduct_stock_fefo({0}, {1}, {2}, {3})",
                    [item.DrugId, item.Quantity, userId, referenceNo], ct);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SalesOrderDetailDto>.Fail(
                    $"Failed to deduct stock for DrugID {item.DrugId}: {ex.InnerException?.Message ?? ex.Message}"));
            }

            // Find the FEFO batch for pricing
            var batch = await db.InventoryBatches
                .Where(b => b.DrugId == item.DrugId && b.IsActive && b.QuantityInStock >= 0)
                .OrderBy(b => b.ExpDate)
                .FirstOrDefaultAsync(ct);

            if (batch is not null)
            {
                var unitPrice = batch.SellingPrice ?? 0;
                orderItems.Add(new SalesOrderItem
                {
                    BatchId = batch.BatchId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });
                total += item.Quantity * unitPrice;
            }
        }

        order.TotalAmount = total;
        db.SalesOrders.Add(order);
        await db.SaveChangesAsync(ct);

        foreach (var oi in orderItems)
        {
            oi.OrderId = order.OrderId;
            db.SalesOrderItems.Add(oi);
        }
        await db.SaveChangesAsync(ct);

        // Return full detail DTO
        var dto = await db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Employee)
            .Include(o => o.Items)
                .ThenInclude(i => i.InventoryBatch)
                    .ThenInclude(b => b.Medicine)
            .Where(o => o.OrderId == order.OrderId)
            .Select(o => new SalesOrderDetailDto
            {
                OrderId = o.OrderId,
                EmployeeName = o.Employee.EmpName,
                CustomerInfo = o.CustomerInfo,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new SalesItemDto
                {
                    OrderItemId = i.OrderItemId,
                    DrugName = i.InventoryBatch.Medicine.DrugName,
                    BatchNumber = i.InventoryBatch.BatchNumber,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                }).ToList()
            })
            .FirstAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = order.OrderId },
            ApiResponse<SalesOrderDetailDto>.Ok(dto, "Sales order created."));
    }
}
