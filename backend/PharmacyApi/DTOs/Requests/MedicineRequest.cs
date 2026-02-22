namespace PharmacyApi.DTOs.Requests;

public class MedicineRequest
{
    public string? Barcode { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Unit { get; set; }
    public int? CategoryId { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public string? ImageUrl { get; set; }
}
