using Microsoft.EntityFrameworkCore;
using QuickBiteMenuAPI.Models;

namespace QuickBiteMenuAPI.Data
{
    public class MenuDbContext : DbContext
    {
        public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure MenuItem entity
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasPrecision(6, 2);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DietaryTag).HasMaxLength(50).HasDefaultValue("None");
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                // Add indexes for better performance
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.DietaryTag);
            });

            // Seed data
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem
                {
                    Id = 1,
                    Name = "Margherita Pizza",
                    Description = "Classic pizza with tomato sauce, mozzarella, and fresh basil",
                    Price = 12.99m,
                    Category = "Main Course",
                    DietaryTag = "Vegetarian",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Id = 2,
                    Name = "Caesar Salad",
                    Description = "Fresh romaine lettuce with Caesar dressing, croutons, and parmesan",
                    Price = 8.99m,
                    Category = "Appetizer",
                    DietaryTag = "Vegetarian",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new MenuItem
                {
                    Id = 3,
                    Name = "Grilled Chicken Breast",
                    Description = "Tender grilled chicken breast with herbs and spices",
                    Price = 16.99m,
                    Category = "Main Course",
                    DietaryTag = "Gluten-Free",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is MenuItem && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (MenuItem)entry.Entity;
                
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}