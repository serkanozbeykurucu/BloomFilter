using System.ComponentModel.DataAnnotations;

namespace BloomFilter.Entity.Concrete;

public class SuspiciousEmail : BaseEntity
{
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;

    [MaxLength(255)]
    public string DomainName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int ReportCount { get; set; } = 1;

    public DateTime LastReportedDate { get; set; } = DateTime.UtcNow;

    // associations
    public ICollection<UserReport> UserReports { get; set; } = new List<UserReport>();
}