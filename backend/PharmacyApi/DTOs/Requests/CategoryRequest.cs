namespace PharmacyApi.DTOs.Requests;

public class CategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
