using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models;

[Table("STOCK_TRANSACTION")]
public class StockTransaction
{
    [Key]
    [Column("TransactionID")]
    public int TransactionId { get; set; }

    [Column("BatchID")]
    public int BatchId { get; set; }

    [Column("EID")]
    public int EId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("TransType")]
    public string TransType { get; set; } = string.Empty; // IN, OUT, ADJUST, EXPIRED

    [MaxLength(100)]
    [Column("ReferenceNo")]
    public string? ReferenceNo { get; set; }

    [Column("Quantity")]
    public int Quantity { get; set; }

    [MaxLength(500)]
    [Column("Notes")]
    public string? Notes { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("BatchId")]
    public InventoryBatch InventoryBatch { get; set; } = null!;

    [ForeignKey("EId")]
    public Employee Employee { get; set; } = null!;
}
