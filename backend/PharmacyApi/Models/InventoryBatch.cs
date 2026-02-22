using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models;

[Table("INVENTORY_BATCH")]
public class InventoryBatch
{
    [Key]
    [Column("BatchID")]
    public int BatchId { get; set; }

    [Column("DrugID")]
    public int DrugId { get; set; }

    [Column("SupplierID")]
    public int? SupplierId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("BatchNumber")]
    public string BatchNumber { get; set; } = string.Empty;

    [Column("QuantityInStock")]
    public int QuantityInStock { get; set; } = 0;

    [Column("CostPrice", TypeName = "decimal(18,2)")]
    public decimal? CostPrice { get; set; }

    [Column("SellingPrice", TypeName = "decimal(18,2)")]
    public decimal? SellingPrice { get; set; }

    [Column("MfgDate")]
    public DateOnly? MfgDate { get; set; }

    [Column("ExpDate")]
    public DateOnly? ExpDate { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    [ForeignKey("DrugId")]
    public Medicine Medicine { get; set; } = null!;

    [ForeignKey("SupplierId")]
    public Supplier? Supplier { get; set; }

    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
}
