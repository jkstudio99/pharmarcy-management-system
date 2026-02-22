using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models;

[Table("SUPPLIER")]
public class Supplier
{
    [Key]
    [Column("SupplierID")]
    public int SupplierId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("SupplierName")]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("Contact")]
    public string? Contact { get; set; }

    [MaxLength(500)]
    [Column("Address")]
    public string? Address { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<InventoryBatch> InventoryBatches { get; set; } = new List<InventoryBatch>();
}
