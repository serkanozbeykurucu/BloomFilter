namespace BloomFilter.Dto.ResponseDTOs;

public class SuspiciousEmailResponseDto
{
    public int Id { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ReportCount { get; set; }
    public DateTime LastReportedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}