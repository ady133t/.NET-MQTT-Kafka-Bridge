using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;

namespace My_Dashboard.Models.DB
{
    public class MachineUtilizationContext : DbContext
    {
        public MachineUtilizationContext()
        {
        }

        public MachineUtilizationContext(DbContextOptions<MachineUtilizationContext> options)
            : base(options)
        {
        }

        public DbSet<Machine> Machines { get; set; } = null!;
        public DbSet<MachineType> MachineTypes { get; set; } = null!;

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Machine>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.DateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.MachineType)
                      .WithMany(mt => mt.Machines)
                      .HasForeignKey(e => e.MachineTypeId);
            });

            modelBuilder.Entity<MachineType>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.DateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
           
        }

        
    }
}
