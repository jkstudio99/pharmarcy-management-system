using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Responses;

namespace PharmacyApi.Controllers;

/// <summary>
/// Dashboard summary metrics. Calls the sp_get_dashboard_summary stored procedure.
/// Accessible by all authenticated staff.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = "AllStaff")]
public class DashboardController(PharmacyDbContext db) : ApiControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Get dashboard summary: total medicines, low-stock count, expiring-soon count,
    /// today's transactions, monthly sales, and top-selling medicines.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummary>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DashboardSummary>>> GetSummary(CancellationToken ct)
    {
        try
        {
            await using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sp_get_dashboard_summary()";

            var result = await cmd.ExecuteScalarAsync(ct);
            var json = result?.ToString() ?? "{}";

            var summary = JsonSerializer.Deserialize<DashboardSummary>(json, JsonOptions)
                          ?? new DashboardSummary();

            return Ok(ApiResponse<DashboardSummary>.Ok(summary, "Dashboard data retrieved."));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<DashboardSummary>.Fail(ex.InnerException?.Message ?? ex.Message));
        }
    }
}
