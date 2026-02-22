using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models;

[Table("SALES_ORDER")]
public class SalesOrder
{
    [Key]
    [Column("OrderID")]
    public int OrderId { get; set; }

    [Column("EID")]
    public int EId { get; set; }

    [MaxLength(200)]
    [Column("CustomerInfo")]
    public string? CustomerInfo { get; set; }

    [Column("TotalAmount", TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(50)]
    [Column("PaymentMethod")]
    public string? PaymentMethod { get; set; } // Cash, QR PromptPay, Credit Card

    [MaxLength(20)]
    [Column("Status")]
    public string Status { get; set; } = "Pending"; // Completed, Pending, Cancelled

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    [ForeignKey("EId")]
    public Employee Employee { get; set; } = null!;

    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
}
