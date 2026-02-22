namespace PharmacyApi.DTOs.Responses;

public class EmployeeDto
{
    public int EId { get; set; }
    public string EmpName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}
