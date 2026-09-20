using Microsoft.EntityFrameworkCore;
using OrderService.Entities;

namespace OrderService.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(
        DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");

            entity.HasKey(x => x.OrderId);

            entity.Property(x => x.OrderId)
                .HasColumnName("order_id");

            entity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(x => x.OrderStatus)
                .HasColumnName("order_status")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone")
                .IsRequired();

            entity.HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");

            entity.HasKey(x => x.OrderItemId);

            entity.Property(x => x.OrderItemId)
                .HasColumnName("order_item_id");

            entity.Property(x => x.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            entity.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            entity.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();
        });
    }
}