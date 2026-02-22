using System.ComponentModel.DataAnnotations;

namespace PharmacyApi.DTOs.Requests;

public class RegisterRequest
{
    [Required]
    [MaxLength(200)]
    public string EmpName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public List<int> RoleIds { get; set; } = new();
}
