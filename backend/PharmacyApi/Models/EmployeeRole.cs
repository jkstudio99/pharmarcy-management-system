using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models;

[Table("EMPLOYEE_ROLE")]
public class EmployeeRole
{
    [Column("EID")]
    public int EId { get; set; }

    [Column("RoleID")]
    public int RoleId { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("EId")]
    public Employee Employee { get; set; } = null!;

    [ForeignKey("RoleId")]
    public Role Role { get; set; } = null!;
}
