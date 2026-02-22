namespace PharmacyApi.DTOs.Requests;

public class SupplierRequest
{
    public string SupplierName { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public string? Address { get; set; }
}
