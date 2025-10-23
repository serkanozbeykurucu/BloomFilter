using BloomFilter.Entity.Enums;

namespace BloomFilter.Dto.ResponseDTOs;

public class UserReportResponseDto
{
    public int Id { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string? ReporterEmail { get; set; }
    public ReportType ReportType { get; set; }
    public string ReportedValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewedBy { get; set; }
}