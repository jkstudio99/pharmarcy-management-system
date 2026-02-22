using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models;

[Table("MEDICINE")]
public class Medicine
{
    [Key]
    [Column("DrugID")]
    public int DrugId { get; set; }

    [MaxLength(100)]
    [Column("Barcode")]
    public string? Barcode { get; set; }

    [Required]
    [MaxLength(300)]
    [Column("DrugName")]
    public string DrugName { get; set; } = string.Empty;

    [MaxLength(300)]
    [Column("GenericName")]
    public string? GenericName { get; set; }

    [MaxLength(50)]
    [Column("Unit")]
    public string? Unit { get; set; }

    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    [Column("ReorderLevel")]
    public int ReorderLevel { get; set; } = 10;

    [MaxLength(500)]
    [Column("ImageUrl")]
    public string? ImageUrl { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    public ICollection<InventoryBatch> InventoryBatches { get; set; } = new List<InventoryBatch>();
}
