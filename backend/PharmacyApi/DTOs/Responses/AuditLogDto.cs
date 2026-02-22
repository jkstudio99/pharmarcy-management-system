namespace PharmacyApi.DTOs.Responses;

public class AuditLogDto
{
    public int LogId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public int? ActionBy { get; set; }
    public string? ActionByName { get; set; }
    public DateTime ActionDate { get; set; }
}
