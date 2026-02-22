namespace PharmacyApi.DTOs.Responses;

public class InventoryBatchDto
{
    public int BatchId { get; set; }
    public int DrugId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public DateOnly? MfgDate { get; set; }
    public DateOnly? ExpDate { get; set; }
    public bool IsExpiringSoon { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AlertsDto
{
    public List<LowStockAlert> LowStockAlerts { get; set; } = new();
    public List<ExpiryAlert> ExpiryAlerts { get; set; } = new();
}

public class LowStockAlert
{
    public int DrugId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public int ReorderLevel { get; set; }
    public int CurrentStock { get; set; }
}

public class ExpiryAlert
{
    public int BatchId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly ExpDate { get; set; }
    public int QuantityInStock { get; set; }
    public int DaysUntilExpiry { get; set; }
}
