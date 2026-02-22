namespace PharmacyApi.DTOs.Requests;

public class UpdateEmployeeRequest
{
    public string EmpName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? NewPassword { get; set; }
    public List<int>? RoleIds { get; set; }
}
