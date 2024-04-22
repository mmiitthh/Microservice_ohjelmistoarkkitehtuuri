using DatabaseManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseManagementService.Context
{
    public class ElectricityDbContext : DbContext
    {
        public ElectricityDbContext(DbContextOptions<ElectricityDbContext> options) : base(options) { }

        public DbSet<ElectricityPrice> ElectricityPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
