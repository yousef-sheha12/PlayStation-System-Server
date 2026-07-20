using PlayStation.Domain.Entities;
using PlayStation.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace PlayStation.Infrastructure.Data;

public class PlayStationDbContext : DbContext
{
    public PlayStationDbContext(DbContextOptions<PlayStationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SessionProduct> SessionProducts => Set<SessionProduct>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

            modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalHours).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DeviceCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ProductsCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Device).WithMany(d => d.Sessions).HasForeignKey(e => e.DeviceId);
            entity.HasOne(e => e.Customer).WithMany(c => c.Sessions).HasForeignKey(e => e.CustomerId).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Category).WithMany(c => c.Products).HasForeignKey(e => e.CategoryId).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<SessionProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Session).WithMany(s => s.SessionProducts).HasForeignKey(e => e.SessionId);
            entity.HasOne(e => e.Product).WithMany(p => p.SessionProducts).HasForeignKey(e => e.ProductId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Session).WithOne(s => s.Invoice).HasForeignKey<Invoice>(e => e.SessionId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Invoice).WithMany(i => i.InvoiceItems).HasForeignKey(e => e.InvoiceId);
            entity.HasOne(e => e.Product).WithMany(p => p.InvoiceItems).HasForeignKey(e => e.ProductId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Category).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "System Administrator", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 2, Name = "Worker", Description = "PlayStation Worker", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "admin@playstation.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                RoleId = 1,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Device>().HasData(
            new Device { Id = 1, Name = "PS1", Description = "PlayStation 1", HourlyRate = 5.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Device { Id = 2, Name = "PS2", Description = "PlayStation 2", HourlyRate = 6.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Device { Id = 3, Name = "PS3", Description = "PlayStation 3", HourlyRate = 7.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Device { Id = 4, Name = "PS4", Description = "PlayStation 4", HourlyRate = 8.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Device { Id = 5, Name = "PS5", Description = "PlayStation 5", HourlyRate = 10.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Device { Id = 6, Name = "PS6", Description = "PlayStation 6", HourlyRate = 12.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Device { Id = 7, Name = "PS7", Description = "PlayStation 7", HourlyRate = 14.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Device { Id = 8, Name = "PS8", Description = "PlayStation 8", HourlyRate = 16.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Device { Id = 9, Name = "PS9", Description = "PlayStation 9", HourlyRate = 18.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Device { Id = 10, Name = "PS10", Description = "PlayStation 10", HourlyRate = 20.0m, Status = Domain.Enums.DeviceStatus.Available, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
