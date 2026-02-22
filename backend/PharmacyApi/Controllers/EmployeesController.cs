using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Requests;
using PharmacyApi.DTOs.Responses;
using PharmacyApi.Models;
using PharmacyApi.Services;

namespace PharmacyApi.Controllers;

/// <summary>
/// Employee management (CRUD + role assignment). Requires Admin role.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class EmployeesController(
    PharmacyDbContext db,
    PasswordService passwordService) : ApiControllerBase
{
    /// <summary>
    /// List all employees with their assigned roles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EmployeeDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetAll(CancellationToken ct)
    {
        var employees = await db.Employees
            .AsNoTracking()
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
            .OrderBy(e => e.EmpName)
            .Select(e => MapToDto(e))
            .ToListAsync(ct);

        return Ok(ApiResponse<List<EmployeeDto>>.Ok(employees));
    }

    /// <summary>
    /// Get a single employee by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(int id, CancellationToken ct)
    {
        var dto = await db.Employees
            .AsNoTracking()
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
            .Where(e => e.EId == id)
            .Select(e => MapToDto(e))
            .FirstOrDefaultAsync(ct);

        if (dto is null)
            return NotFound(ApiResponse<EmployeeDto>.Fail("Employee not found."));

        return Ok(ApiResponse<EmployeeDto>.Ok(dto));
    }

    /// <summary>
    /// Update employee profile, password, and/or role assignments.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Update(
        int id,
        [FromBody] UpdateEmployeeRequest req,
        CancellationToken ct)
    {
        var employee = await db.Employees
            .Include(e => e.EmployeeRoles)
            .FirstOrDefaultAsync(e => e.EId == id, ct);

        if (employee is null)
            return NotFound(ApiResponse<EmployeeDto>.Fail("Employee not found."));

        // Check email uniqueness if changed
        if (!string.Equals(employee.Email, req.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailTaken = await db.Employees
                .IgnoreQueryFilters()
                .AnyAsync(e => e.Email == req.Email && e.EId != id, ct);

            if (emailTaken)
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Email is already in use by another account."));
        }

        employee.EmpName = req.EmpName;
        employee.Email = req.Email;
        employee.IsActive = req.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(req.NewPassword))
            employee.PasswordHash = passwordService.HashPassword(req.NewPassword);

        // Replace roles if provided
        if (req.RoleIds is not null)
        {
            db.EmployeeRoles.RemoveRange(employee.EmployeeRoles);

            var validRoleIds = await db.Roles
                .Where(r => req.RoleIds.Contains(r.RoleId))
                .Select(r => r.RoleId)
                .ToListAsync(ct);

            foreach (var roleId in validRoleIds)
                db.EmployeeRoles.Add(new EmployeeRole { EId = id, RoleId = roleId });
        }

        await db.SaveChangesAsync(ct);

        // Reload roles for response
        var roles = await db.EmployeeRoles
            .Where(er => er.EId == id)
            .Include(er => er.Role)
            .Select(er => er.Role.RoleName)
            .ToListAsync(ct);

        return Ok(ApiResponse<EmployeeDto>.Ok(new EmployeeDto
        {
            EId = employee.EId,
            EmpName = employee.EmpName,
            Email = employee.Email,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt,
            Roles = roles
        }, "Employee updated."));
    }

    /// <summary>
    /// Soft-delete an employee (sets IsActive = false).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct)
    {
        var employee = await db.Employees.FindAsync([id], ct);
        if (employee is null)
            return NotFound(ApiResponse<string>.Fail("Employee not found."));

        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<string>.Ok("Employee deactivated."));
    }

    // ── Private helpers ──────────────────────────────────────────

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        EId = e.EId,
        EmpName = e.EmpName,
        Email = e.Email,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        Roles = e.EmployeeRoles.Select(er => er.Role.RoleName).ToList()
    };
}
