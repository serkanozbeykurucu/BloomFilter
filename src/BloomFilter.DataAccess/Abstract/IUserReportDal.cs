using BloomFilter.Entity.Concrete;
using BloomFilter.Entity.Enums;

namespace BloomFilter.DataAccess.Abstract;

public interface IUserReportDal : IGenericRepositoryDal<UserReport>
{
    Task<List<UserReport>> GetReportsByStatusAsync(ReportStatus status);
    Task<List<UserReport>> GetReportsByTypeAsync(ReportType reportType);
    Task<List<UserReport>> GetRecentReportsAsync(int count = 50);
    Task<int> GetTodayReportCountAsync();
    Task<int> GetPendingReportCountAsync();
    Task<List<UserReport>> GetReportsByReporterEmailAsync(string reporterEmail);
    Task<bool> UpdateReportStatusAsync(int reportId, ReportStatus status, string reviewedBy);
}