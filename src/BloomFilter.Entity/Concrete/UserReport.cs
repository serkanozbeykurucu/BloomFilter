using BloomFilter.Entity.Enums;
using System.ComponentModel.DataAnnotations;

namespace BloomFilter.Entity.Concrete;

public class UserReport : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string ReporterName { get; set; } = string.Empty;

    [MaxLength(255)]
    [EmailAddress]
    public string? ReporterEmail { get; set; }

    [Required]
    public ReportType ReportType { get; set; }

    [Required]
    [MaxLength(255)]
    public string ReportedValue { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? ReporterIpAddress { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public DateTime? ReviewedDate { get; set; }

    [MaxLength(100)]
    public string? ReviewedBy { get; set; }

    // associations

    public int? SuspiciousDomainId { get; set; }
    public SuspiciousDomain? SuspiciousDomain { get; set; }
    public SuspiciousEmail? SuspiciousEmail { get; set; }
    public int? SuspiciousEmailId { get; set; }
}