namespace PharmacyApi.DTOs.Responses;

public class SalesOrderDto
{
    public int OrderId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? CustomerInfo { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

public class SalesOrderDetailDto : SalesOrderDto
{
    public List<SalesItemDto> Items { get; set; } = new();
}

public class SalesItemDto
{
    public int OrderItemId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
