using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PharmacyApi.Controllers;

/// <summary>
/// Shared base controller providing common helpers for all API controllers.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Extracts the authenticated user's ID from the JWT claims.
    /// Returns <c>null</c> when the token is missing or the claim cannot be parsed.
    /// </summary>
    protected int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub");

        return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
    }

    /// <summary>
    /// Convenience wrapper that returns the user ID or short-circuits with 401.
    /// </summary>
    protected bool TryGetUserId(out int userId)
    {
        var id = GetCurrentUserId();
        userId = id ?? 0;
        return id.HasValue;
    }
}
