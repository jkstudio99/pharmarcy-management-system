namespace PharmacyApi.DTOs.Responses;

public class TransactionDto
{
    public int TransactionId { get; set; }
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string TransType { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
