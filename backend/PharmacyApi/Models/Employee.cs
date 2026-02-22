using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models;

[Table("EMPLOYEE")]
public class Employee
{
    [Key]
    [Column("EID")]
    public int EId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("EmpName")]
    public string EmpName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
