using Microsoft.EntityFrameworkCore;
using FirstErpSystem.Api.Models;

namespace FirstErpSystem.Api.Data;

/*
================================================================
AppDbContext — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- DbContext = EF Core এর main class
- প্রতিটা DbSet = একটা database table
- OnModelCreating = table rules define করা
  (unique index, required fields, decimal precision)
- Phase 2 তে ছিল শুধু Employees
- Phase 4 তে add হলো: Products, StockTransactions,
  PurchaseOrders, PurchaseOrderLines,
  SalesOrders, SalesOrderLines
================================================================
*/
public class AppDbContext : DbContext
{
    //Added By Himel Sarkar 09-06-2026
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Phase 2 — Employee table
    public DbSet<Employee> Employees { get; set; }

    // Phase 4 — Inventory tables
    public DbSet<Product> Products { get; set; }
    public DbSet<StockTransaction> StockTransactions { get; set; }

    // Phase 4 — Purchase Order tables
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }

    // Phase 4 — Sales Order tables
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /*
        LEARNING: OnModelCreating = Fluent API
        এখানে table rules define করি
        Alternative: Data Annotations ([Required], [MaxLength])
        Fluent API বেশি flexible তাই ERP-এ prefer করি
        */

        // ── Employee ──────────────────────────────────────
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
        });

        // ── Product ───────────────────────────────────────
        /*
        LEARNING: Product Code unique হবে
        e.g. PRD-001, PRD-002 — duplicate allowed না
        */
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        });

        // ── StockTransaction ──────────────────────────────
        /*
        LEARNING: HasOne/WithMany = relationship define করা
        একটা Product এর অনেক StockTransaction থাকতপারে
        OnDelete Restrict = Product delete করলে
        transaction গুলো delete হবে না
        */
        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PurchaseOrder ─────────────────────────────────
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNo).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.OrderNo).IsUnique();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
        });

        // ── PurchaseOrderLine ─────────────────────────────
        /*
        LEARNING: PurchaseOrderLine belongs to PurchaseOrder
        Cascade delete = Order delete হলে Lines ও delete হবে
        */
        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.PurchaseOrder)
                  .WithMany(o => o.Lines)
                  .HasForeignKey(e => e.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            // Ignore calculated property — DB-তে store হবে না
            entity.Ignore(e => e.LineTotal);
        });

        // ── SalesOrder ────────────────────────────────────
        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNo).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.OrderNo).IsUnique();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
        });

        // ── SalesOrderLine ────────────────────────────────
        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.SalesOrder)
                  .WithMany(o => o.Lines)
                  .HasForeignKey(e => e.SalesOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Ignore(e => e.LineTotal);
        });
    }
    //End By Himel Sarkar 09-06-2026
}
