namespace PharmacyApi.DTOs.Requests;

public class StockInRequest
{
    public int DrugId { get; set; }
    public int? SupplierId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public DateOnly? MfgDate { get; set; }
    public DateOnly? ExpDate { get; set; }
    public string? ReferenceNo { get; set; }
}

public class StockOutFefoRequest
{
    public int DrugId { get; set; }
    public int Quantity { get; set; }
    public string? ReferenceNo { get; set; }
}

public class StockAdjustRequest
{
    public int BatchId { get; set; }
    public int NewQuantity { get; set; }
    public string? Reason { get; set; }
}
