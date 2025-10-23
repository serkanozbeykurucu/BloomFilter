using System.ComponentModel.DataAnnotations;

namespace BloomFilter.Entity.Concrete;

public class SuspiciousDomain : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string DomainName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int ReportCount { get; set; } = 1;

    public DateTime LastReportedDate { get; set; } = DateTime.UtcNow;

    // associations
    public ICollection<UserReport> UserReports { get; set; } = new List<UserReport>();
}