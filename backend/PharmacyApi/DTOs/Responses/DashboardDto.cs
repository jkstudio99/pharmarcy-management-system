namespace PharmacyApi.DTOs.Responses;

public class DashboardSummary
{
    public int TotalMedicines { get; set; }
    public int LowStockCount { get; set; }
    public int ExpiringSoonCount { get; set; }
    public int TodayTransactions { get; set; }
    public List<MonthlySales> MonthlySales { get; set; } = new();
}

public class MonthlySales
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public int OrderCount { get; set; }
}

public class FefoDeductionResult
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int DeductedQty { get; set; }
    public int RemainingQty { get; set; }
    public DateOnly? ExpDate { get; set; }
}
