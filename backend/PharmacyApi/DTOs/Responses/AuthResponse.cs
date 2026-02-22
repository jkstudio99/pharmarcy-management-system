namespace PharmacyApi.DTOs.Responses;

public class AuthResponse
{
    public int EId { get; set; }
    public string EmpName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
