namespace PharmacyApi.DTOs.Responses;

public class MedicineDto
{
    public int DrugId { get; set; }
    public string? Barcode { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Unit { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int ReorderLevel { get; set; }
    public string? ImageUrl { get; set; }
    public int TotalStock { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MedicineDetailDto : MedicineDto
{
    public List<BatchSummaryDto> Batches { get; set; } = new();
}

public class BatchSummaryDto
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public DateOnly? MfgDate { get; set; }
    public DateOnly? ExpDate { get; set; }
    public string? SupplierName { get; set; }
}
