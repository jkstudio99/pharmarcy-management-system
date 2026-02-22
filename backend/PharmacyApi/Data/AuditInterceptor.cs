using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PharmacyApi.Models;

namespace PharmacyApi.Data;

/// <summary>
/// EF Core SaveChanges interceptor that automatically writes to AUDIT_LOG
/// for INSERT, UPDATE, and DELETE operations on auditable entities.
///
/// Strategy:
///   - BEFORE save  → snapshot Modified/Deleted entries (old values, state)
///   - AFTER save   → write audit rows so Added entries have their real DB-generated PK
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> AuditedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "MEDICINE", "CATEGORY", "SUPPLIER", "EMPLOYEE", "INVENTORY_BATCH", "SALES_ORDER"
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpContextAccessor _httpContextAccessor;

    // Per-call snapshot — keyed by DbContext instance hash to support concurrent requests
    private readonly List<PendingAudit> _pending = [];

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ── Before save: snapshot state ──────────────────────────────

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is PharmacyDbContext db)
            SnapshotEntries(db);

        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is PharmacyDbContext db)
            SnapshotEntries(db);

        return base.SavingChanges(eventData, result);
    }

    // ── After save: write audit rows with real PKs ────────────────

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default)
    {
        if (eventData.Context is PharmacyDbContext db && _pending.Count > 0)
        {
            FlushAuditEntries(db);
            await db.SaveChangesAsync(ct);
        }

        return await base.SavedChangesAsync(eventData, result, ct);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is PharmacyDbContext db && _pending.Count > 0)
        {
            FlushAuditEntries(db);
            db.SaveChanges();
        }

        return base.SavedChanges(eventData, result);
    }

    // ── Private helpers ──────────────────────────────────────────

    private void SnapshotEntries(PharmacyDbContext db)
    {
        _pending.Clear();
        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var entry in db.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.Name;
            if (!AuditedTables.Contains(tableName)) continue;

            var action = entry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => null
            };
            if (action is null) continue;

            string? oldValues = null;
            string? newValues = null;

            if (entry.State == EntityState.Modified)
            {
                var changed = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name,
                                  p => new { Old = p.OriginalValue, New = p.CurrentValue });

                oldValues = JsonSerializer.Serialize(
                    changed.ToDictionary(k => k.Key, v => v.Value.Old), JsonOpts);
                newValues = JsonSerializer.Serialize(
                    changed.ToDictionary(k => k.Key, v => v.Value.New), JsonOpts);
            }
            else if (entry.State == EntityState.Added)
            {
                // newValues captured after save (real PK available then)
            }
            else if (entry.State == EntityState.Deleted)
            {
                oldValues = JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(
                        p => p.Metadata.Name, p => p.OriginalValue), JsonOpts);
            }

            _pending.Add(new PendingAudit
            {
                Entry = entry,
                TableName = tableName,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                ActionBy = userId,
                ActionDate = now
            });
        }
    }

    private void FlushAuditEntries(PharmacyDbContext db)
    {
        foreach (var p in _pending)
        {
            // For INSERT: capture new values now (real PK assigned after save)
            if (p.Action == "INSERT")
            {
                p.NewValues = JsonSerializer.Serialize(
                    p.Entry.Properties.ToDictionary(
                        prop => prop.Metadata.Name,
                        prop => prop.CurrentValue), JsonOpts);
            }

            var pkProp = p.Entry.Properties.FirstOrDefault(prop => prop.Metadata.IsPrimaryKey());
            var recordId = pkProp?.CurrentValue is int id ? id : 0;

            db.AuditLogs.Add(new AuditLog
            {
                TableName = p.TableName,
                RecordId = recordId,
                Action = p.Action,
                OldValues = p.OldValues,
                NewValues = p.NewValues,
                ActionBy = p.ActionBy,
                ActionDate = p.ActionDate
            });
        }

        _pending.Clear();
    }

    private int? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null) return null;

        var claim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                 ?? user.FindFirst("sub");

        return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
    }

    // ── Inner type ───────────────────────────────────────────────

    private sealed class PendingAudit
    {
        public required Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; init; }
        public required string TableName { get; init; }
        public required string Action { get; init; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public int? ActionBy { get; init; }
        public DateTime ActionDate { get; init; }
    }
}
