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
/// Handles authentication: login, registration (admin-only), and current-user profile.
/// </summary>
[Route("api/[controller]")]
public class AuthController(
    PharmacyDbContext db,
    PasswordService passwordService,
    TokenService tokenService) : ApiControllerBase
{
    /// <summary>
    /// Authenticate with email and password. Returns a JWT bearer token on success.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var employee = await db.Employees
            .IgnoreQueryFilters()
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(e => e.Email == request.Email, ct);

        if (employee is null || !employee.IsActive)
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid email or password."));

        if (!passwordService.VerifyPassword(request.Password, employee.PasswordHash))
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid email or password."));

        var response = BuildAuthResponse(employee, includeToken: true);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Login successful."));
    }

    /// <summary>
    /// Register a new employee account. Requires Admin role.
    /// </summary>
    [HttpPost("register")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var emailTaken = await db.Employees
            .IgnoreQueryFilters()
            .AnyAsync(e => e.Email == request.Email, ct);

        if (emailTaken)
            return BadRequest(ApiResponse<AuthResponse>.Fail("An account with this email already exists."));

        // Validate that all requested roles exist
        var validRoleIds = await db.Roles
            .Where(r => request.RoleIds.Contains(r.RoleId))
            .Select(r => r.RoleId)
            .ToListAsync(ct);

        if (validRoleIds.Count != request.RoleIds.Count)
            return BadRequest(ApiResponse<AuthResponse>.Fail("One or more role IDs are invalid."));

        var employee = new Employee
        {
            EmpName = request.EmpName,
            Email = request.Email,
            PasswordHash = passwordService.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);

        // Assign roles in a single batch
        var employeeRoles = validRoleIds.Select(roleId => new EmployeeRole
        {
            EId = employee.EId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        });

        db.EmployeeRoles.AddRange(employeeRoles);
        await db.SaveChangesAsync(ct);

        // Reload with roles for response
        await db.Entry(employee)
            .Collection(e => e.EmployeeRoles)
            .Query()
            .Include(er => er.Role)
            .LoadAsync(ct);

        var response = BuildAuthResponse(employee, includeToken: true);
        return CreatedAtAction(nameof(Me), ApiResponse<AuthResponse>.Ok(response, "Employee registered successfully."));
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Me(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid token."));

        var employee = await db.Employees
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(e => e.EId == userId, ct);

        if (employee is null)
            return NotFound(ApiResponse<AuthResponse>.Fail("User not found."));

        var response = BuildAuthResponse(employee, includeToken: false);
        return Ok(ApiResponse<AuthResponse>.Ok(response));
    }

    /// <summary>
    /// Change the current user's own password. Requires valid current password.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword(
        [FromBody] ChangePasswordRequest req,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<string>.Fail("Invalid token."));

        var employee = await db.Employees.FindAsync([userId], ct);
        if (employee is null)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        if (!passwordService.VerifyPassword(req.CurrentPassword, employee.PasswordHash))
            return BadRequest(ApiResponse<string>.Fail("Current password is incorrect."));

        employee.PasswordHash = passwordService.HashPassword(req.NewPassword);
        employee.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<string>.Ok("Password changed successfully."));
    }

    // ── Private helpers ────────────────────────────────────────────

    private AuthResponse BuildAuthResponse(Employee employee, bool includeToken)
    {
        var roles = employee.EmployeeRoles
            .Where(er => er.Role.IsActive)
            .Select(er => er.Role.RoleName)
            .ToList();

        return new AuthResponse
        {
            EId = employee.EId,
            EmpName = employee.EmpName,
            Email = employee.Email,
            Token = includeToken ? tokenService.GenerateToken(employee, roles) : string.Empty,
            Roles = roles
        };
    }
}
