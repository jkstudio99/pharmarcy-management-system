using Microsoft.EntityFrameworkCore;
using PharmacyApi.Models;

namespace PharmacyApi.Data;

public class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) { }

    // ── DbSets ──────────────────────────────────────────────
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<InventoryBatch> InventoryBatches => Set<InventoryBatch>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── EMPLOYEE ────────────────────────────────────────
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.HasQueryFilter(e => e.IsActive);
        });

        // ── ROLE ────────────────────────────────────────────
        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.HasQueryFilter(e => e.IsActive);

            // Seed default roles (static dates to avoid EF PendingModelChangesWarning)
            entity.HasData(
                new Role { RoleId = 1, RoleName = "Admin", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { RoleId = 2, RoleName = "Pharmacist", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { RoleId = 3, RoleName = "StockEmployee", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        // ── EMPLOYEE_ROLE (Composite PK) ────────────────────
        modelBuilder.Entity<EmployeeRole>(entity =>
        {
            entity.HasKey(er => new { er.EId, er.RoleId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(er => er.Employee)
                  .WithMany(e => e.EmployeeRoles)
                  .HasForeignKey(er => er.EId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(er => er.Role)
                  .WithMany(r => r.EmployeeRoles)
                  .HasForeignKey(er => er.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── SUPPLIER ────────────────────────────────────────
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.HasQueryFilter(e => e.IsActive);
        });

        // ── CATEGORY ────────────────────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.HasQueryFilter(e => e.IsActive);
        });

        // ── MEDICINE ────────────────────────────────────────
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasIndex(e => e.Barcode);
            entity.HasIndex(e => e.DrugName);
            entity.HasIndex(e => e.CategoryId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.HasQueryFilter(e => e.IsActive);

            entity.HasOne(m => m.Category)
                  .WithMany(c => c.Medicines)
                  .HasForeignKey(m => m.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── INVENTORY_BATCH ─────────────────────────────────
        modelBuilder.Entity<InventoryBatch>(entity =>
        {
            entity.HasIndex(e => e.DrugId);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.ExpDate);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.HasQueryFilter(e => e.IsActive);

            entity.HasOne(b => b.Medicine)
                  .WithMany(m => m.InventoryBatches)
                  .HasForeignKey(b => b.DrugId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Supplier)
                  .WithMany(s => s.InventoryBatches)
                  .HasForeignKey(b => b.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── STOCK_TRANSACTION (append-only, no soft-delete) ─
        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasIndex(e => e.BatchId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.EId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(t => t.InventoryBatch)
                  .WithMany(b => b.StockTransactions)
                  .HasForeignKey(t => t.BatchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Employee)
                  .WithMany(e => e.StockTransactions)
                  .HasForeignKey(t => t.EId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SALES_ORDER ─────────────────────────────────────
        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasIndex(e => e.EId);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.HasQueryFilter(e => e.IsActive);

            entity.HasOne(o => o.Employee)
                  .WithMany(e => e.SalesOrders)
                  .HasForeignKey(o => o.EId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SALES_ORDER_ITEM ────────────────────────────────
        modelBuilder.Entity<SalesOrderItem>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(i => i.SalesOrder)
                  .WithMany(o => o.Items)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.InventoryBatch)
                  .WithMany(b => b.SalesOrderItems)
                  .HasForeignKey(i => i.BatchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── AUDIT_LOG (append-only, no soft-delete) ─────────
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => new { e.TableName, e.RecordId });
            entity.HasIndex(e => e.ActionDate);
            entity.Property(e => e.ActionDate).HasDefaultValueSql("NOW()");

            entity.HasOne(a => a.Employee)
                  .WithMany(e => e.AuditLogs)
                  .HasForeignKey(a => a.ActionBy)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
