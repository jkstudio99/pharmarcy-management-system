using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models;

[Table("SALES_ORDER_ITEM")]
public class SalesOrderItem
{
    [Key]
    [Column("OrderItemID")]
    public int OrderItemId { get; set; }

    [Column("OrderID")]
    public int OrderId { get; set; }

    [Column("BatchID")]
    public int BatchId { get; set; }

    [Column("Quantity")]
    public int Quantity { get; set; }

    [Column("UnitPrice", TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("OrderId")]
    public SalesOrder SalesOrder { get; set; } = null!;

    [ForeignKey("BatchId")]
    public InventoryBatch InventoryBatch { get; set; } = null!;
}
