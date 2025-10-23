namespace BloomFilter.Dto.ResponseDTOs;

public class DashboardStatsResponseDto
{
    public int TotalSuspiciousDomains { get; set; }
    public int TotalSuspiciousEmails { get; set; }
    public int TotalUserReports { get; set; }
    public int PendingReports { get; set; }
    public int TodayReports { get; set; }
    public List<BloomFilterStatsResponseDto> BloomFilterStats { get; set; } = new();
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
}