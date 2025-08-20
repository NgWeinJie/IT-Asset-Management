using Microsoft.EntityFrameworkCore;
using ITAssetManagement.API.Models;

namespace ITAssetManagement.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Asset> Assets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Configure Asset entity
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.AssetTag).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Brand).HasMaxLength(50);
                entity.Property(e => e.Model).HasMaxLength(50);
                entity.Property(e => e.SerialNumber).HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.UpdatedBy).HasMaxLength(50);
                entity.HasIndex(e => e.AssetTag).IsUnique();
            });

            // Seed some sample assets
            modelBuilder.Entity<Asset>().HasData(
                new Asset
                {
                    Id = 1,
                    Name = "Dell Laptop OptiPlex 1",
                    AssetTag = "LT001",
                    Category = "Laptop",
                    Brand = "Dell",
                    Model = "OptiPlex 7490",
                    SerialNumber = "DL2024001",
                    Status = "Assigned",
                    AssignedTo = "John Doe",
                    Location = "Office Floor 1",
                    PurchasePrice = 1200.00m,
                    PurchaseDate = new DateTime(2024, 1, 15),
                    WarrantyExpiry = new DateTime(2027, 1, 15),
                    CreatedBy = "admin",
                    CreatedAt = DateTime.Now
                },
                new Asset
                {
                    Id = 2,
                    Name = "HP Desktop Pro 1",
                    AssetTag = "DT001",
                    Category = "Desktop",
                    Brand = "HP",
                    Model = "Pro 3500",
                    SerialNumber = "HP2024001",
                    Status = "Available",
                    Location = "IT Storage",
                    PurchasePrice = 800.00m,
                    PurchaseDate = new DateTime(2024, 2, 10),
                    WarrantyExpiry = new DateTime(2027, 2, 10),
                    CreatedBy = "admin",
                    CreatedAt = DateTime.Now
                },
                new Asset
                {
                    Id = 3,
                    Name = "Samsung Monitor 24 inch",
                    AssetTag = "MN001",
                    Category = "Monitor",
                    Brand = "Samsung",
                    Model = "S24E450",
                    SerialNumber = "SM2024001",
                    Status = "Assigned",
                    AssignedTo = "Jane Smith",
                    Location = "Office Floor 2",
                    PurchasePrice = 250.00m,
                    PurchaseDate = new DateTime(2024, 1, 20),
                    WarrantyExpiry = new DateTime(2026, 1, 20),
                    CreatedBy = "admin",
                    CreatedAt = DateTime.Now
                },
                new Asset
                {
                    Id = 4,
                    Name = "Lenovo ThinkPad X1",
                    AssetTag = "LT002",
                    Category = "Laptop",
                    Brand = "Lenovo",
                    Model = "ThinkPad X1 Carbon",
                    SerialNumber = "LV2024001",
                    Status = "Maintenance",
                    Location = "IT Department",
                    PurchasePrice = 1800.00m,
                    PurchaseDate = new DateTime(2023, 12, 5),
                    WarrantyExpiry = new DateTime(2026, 12, 5),
                    Notes = "Screen flickering issue - under repair",
                    CreatedBy = "admin",
                    CreatedAt = DateTime.Now
                }
            );
        }
    }
}