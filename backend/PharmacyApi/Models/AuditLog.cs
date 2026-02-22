using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models;

[Table("AUDIT_LOG")]
public class AuditLog
{
    [Key]
    [Column("LogID")]
    public int LogId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("TableName")]
    public string TableName { get; set; } = string.Empty;

    [Column("RecordID")]
    public int RecordId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("Action")]
    public string Action { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

    [Column("OldValues", TypeName = "jsonb")]
    public string? OldValues { get; set; }

    [Column("NewValues", TypeName = "jsonb")]
    public string? NewValues { get; set; }

    [Column("ActionBy")]
    public int? ActionBy { get; set; }

    [Column("ActionDate")]
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("ActionBy")]
    public Employee? Employee { get; set; }
}
