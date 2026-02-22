using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Responses;

namespace PharmacyApi.Controllers;

/// <summary>
/// Read-only audit trail. Viewable by Admin only (FR-09.3).
/// </summary>
[Route("api/audit-logs")]
[Authorize(Policy = "AdminOnly")]
public class AuditLogsController(PharmacyDbContext db) : ApiControllerBase
{
    /// <summary>
    /// List audit log entries with optional filters and pagination.
    /// Supports filtering by table name, action (INSERT/UPDATE/DELETE), actor, and date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> GetAll(
        [FromQuery] string? tableName,
        [FromQuery] string? action,
        [FromQuery] int? actionBy,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.AuditLogs
            .AsNoTracking()
            .Include(a => a.Employee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tableName))
            query = query.Where(a => a.TableName == tableName);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action.ToUpper());
        if (actionBy.HasValue)
            query = query.Where(a => a.ActionBy == actionBy);
        if (from.HasValue)
            query = query.Where(a => a.ActionDate >= from);
        if (to.HasValue)
            query = query.Where(a => a.ActionDate <= to);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.ActionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                LogId        = a.LogId,
                TableName    = a.TableName,
                RecordId     = a.RecordId,
                Action       = a.Action,
                OldValues    = a.OldValues,
                NewValues    = a.NewValues,
                ActionBy     = a.ActionBy,
                ActionByName = a.Employee != null ? a.Employee.EmpName : null,
                ActionDate   = a.ActionDate
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(new PagedResult<AuditLogDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        }));
    }

    /// <summary>
    /// Get all audit entries for a specific record in a specific table.
    /// Useful for viewing the full change history of a single entity.
    /// </summary>
    [HttpGet("{tableName}/{recordId:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetByRecord(
        string tableName,
        int recordId,
        CancellationToken ct)
    {
        var items = await db.AuditLogs
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.TableName == tableName && a.RecordId == recordId)
            .OrderByDescending(a => a.ActionDate)
            .Select(a => new AuditLogDto
            {
                LogId        = a.LogId,
                TableName    = a.TableName,
                RecordId     = a.RecordId,
                Action       = a.Action,
                OldValues    = a.OldValues,
                NewValues    = a.NewValues,
                ActionBy     = a.ActionBy,
                ActionByName = a.Employee != null ? a.Employee.EmpName : null,
                ActionDate   = a.ActionDate
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<AuditLogDto>>.Ok(items));
    }
}
