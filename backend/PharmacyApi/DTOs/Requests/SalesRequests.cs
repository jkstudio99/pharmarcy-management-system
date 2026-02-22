namespace PharmacyApi.DTOs.Requests;

public class CreateSalesOrderRequest
{
    public string? CustomerInfo { get; set; }
    public string? PaymentMethod { get; set; }
    public List<SalesOrderItemRequest> Items { get; set; } = new();
}

public class SalesOrderItemRequest
{
    public int DrugId { get; set; }
    public int Quantity { get; set; }
}
