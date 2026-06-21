using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace _3_Multithreading
{
    public class AppDbContext : DbContext
    {
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Phone> Phones { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=phones.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(m => m.Country)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.HasMany(m => m.Phones)
                      .WithOne(p => p.Manufacturer)
                      .HasForeignKey(p => p.ManufacturerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Phone>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Model)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");
            });
        }
    }
}