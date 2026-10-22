using Microsoft.EntityFrameworkCore;
using samsa_asist_proyect.En.Asistencia;
using System;
using System.Collections.Generic;
using System.Text;

namespace samsa_asist_proyect.Dt.AppBdContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aquí mapeamos la tabla
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones específicas para MySQL
            modelBuilder.Entity<AttendanceRecord>(entity =>
            {
                entity.ToTable("AttendanceRecords");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.GuardName)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(e => e.Branch)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.ImageUrl)
                      .HasMaxLength(500);
            });
        }
    }
    }
