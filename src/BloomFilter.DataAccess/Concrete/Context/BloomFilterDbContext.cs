using BloomFilter.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace BloomFilter.DataAccess.Concrete.Context;

public class BloomFilterDbContext : DbContext
{
    public BloomFilterDbContext(DbContextOptions<BloomFilterDbContext> options) : base(options)
    {
    }

    public DbSet<SuspiciousDomain> SuspiciousDomains { get; set; }
    public DbSet<SuspiciousEmail> SuspiciousEmails { get; set; }
    public DbSet<UserReport> UserReports { get; set; }
    public DbSet<BloomFilterData> BloomFilterDatas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SuspiciousDomain>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DomainName).IsUnique();
            entity.Property(e => e.DomainName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<SuspiciousEmail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EmailAddress).IsUnique();
            entity.HasIndex(e => e.DomainName);
            entity.Property(e => e.EmailAddress).IsRequired().HasMaxLength(255);
            entity.Property(e => e.DomainName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReportedValue);
            entity.HasIndex(e => e.ReportType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedDate);

            entity.Property(e => e.ReporterName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ReporterEmail).HasMaxLength(255);
            entity.Property(e => e.ReportedValue).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ReporterIpAddress).HasMaxLength(50);
            entity.Property(e => e.ReviewedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.SuspiciousDomain)
                  .WithMany(d => d.UserReports)
                  .HasForeignKey(e => e.SuspiciousDomainId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SuspiciousEmail)
                  .WithMany(e => e.UserReports)
                  .HasForeignKey(e => e.SuspiciousEmailId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BloomFilterData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FilterName).IsUnique();
            entity.Property(e => e.FilterName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BitArray).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}