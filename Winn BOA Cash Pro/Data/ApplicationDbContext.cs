using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Winn_BOA_Cash_Pro.Models;

namespace Winn_BOA_Cash_Pro.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public virtual DbSet<FedWire> FedWires { get; set; } = null!;
        public virtual DbSet<VwHrExport> VwHrExports { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("name=DefaultConnection");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityRole>().HasData(new List<IdentityRole>
            {
                new IdentityRole()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "User",
                    NormalizedName = "USER"
                }
            });

            modelBuilder.Entity<FedWire>(entity =>
            {
                entity.ToTable("FedWire");

                entity.Property(e => e.CreatedBy).HasDefaultValueSql("(N'')");

                entity.Property(e => e.CreatedDate).HasDefaultValueSql("('0001-01-01T00:00:00.0000000')");

                entity.Property(e => e.FromAbanumber)
                    .HasColumnName("FromABANumber")
                    .HasDefaultValueSql("(N'')");

                entity.Property(e => e.FromAccountName).HasDefaultValueSql("(N'')");

                entity.Property(e => e.FromAccountNumber).HasDefaultValueSql("(N'')");

                entity.Property(e => e.FromBankName).HasDefaultValueSql("(N'Bank of America')");

                entity.Property(e => e.ToAbanumber)
                    .HasColumnName("ToABANumber")
                    .HasDefaultValueSql("(N'')");

                entity.Property(e => e.ToAccountName).HasDefaultValueSql("(N'')");

                entity.Property(e => e.ToAccountNumber).HasDefaultValueSql("(N'')");

                entity.Property(e => e.ToBankCity).HasDefaultValueSql("(N'Bank of America')");

                entity.Property(e => e.ToBankName).HasDefaultValueSql("(N'Bank of America')");

                entity.Property(e => e.TransactionStatus).HasDefaultValueSql("(N'')");

                entity.Property(e => e.TransferAmount).HasColumnType("numeric(11, 2)");
            });

            modelBuilder.Entity<VwHrExport>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vwHrExport");

                entity.Property(e => e.BusinessEmail)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.BusinessPhone)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Department)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeGroup)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeId)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("EmployeeID");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.FullName)
                    .HasMaxLength(501)
                    .IsUnicode(false);

                entity.Property(e => e.JobTitle)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ManagerId)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("ManagerID");

                entity.Property(e => e.MiddleName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Office)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Ssologin)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("SSOLogin");

                entity.Property(e => e.Status)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
