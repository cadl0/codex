using Microsoft.EntityFrameworkCore;
using AFKproject.Api.Models;

namespace AFKproject.Api.Data
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
        {
        }

        public DbSet<InventoryItem> Items => Set<InventoryItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryItem>().HasData(
                new InventoryItem { Id = 1, Code = "DM-1001", Description = "Double Medical Screw 4.5mm", Quantity = 150 },
                new InventoryItem { Id = 2, Code = "DM-2001", Description = "Double Medical Plate 6 holes", Quantity = 75 }
            );
        }
    }
}
