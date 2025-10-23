using BloomFilter.DataAccess.Abstract;
using BloomFilter.DataAccess.Concrete.Context;
using BloomFilter.Entity.Concrete;
using BloomFilter.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace BloomFilter.DataAccess.Concrete.EntityFrameworkCore;

public class EfUserReportDal : GenericRepository<UserReport>, IUserReportDal
{
    public EfUserReportDal(BloomFilterDbContext context) : base(context)
    {
    }

    public async Task<List<UserReport>> GetReportsByStatusAsync(ReportStatus status)
    {
        return await _dbSet
            .Where(x => x.Status == status && x.IsActive)
            .Include(x => x.SuspiciousDomain)
            .Include(x => x.SuspiciousEmail)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<UserReport>> GetReportsByTypeAsync(ReportType reportType)
    {
        return await _dbSet
            .Where(x => x.ReportType == reportType && x.IsActive)
            .Include(x => x.SuspiciousDomain)
            .Include(x => x.SuspiciousEmail)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<UserReport>> GetRecentReportsAsync(int count = 50)
    {
        return await _dbSet
            .Where(x => x.IsActive)
            .Include(x => x.SuspiciousDomain)
            .Include(x => x.SuspiciousEmail)
            .OrderByDescending(x => x.CreatedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetTodayReportCountAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _dbSet
            .CountAsync(x => x.CreatedDate >= today && x.CreatedDate < tomorrow && x.IsActive);
    }

    public async Task<int> GetPendingReportCountAsync()
    {
        return await _dbSet
            .CountAsync(x => x.Status == ReportStatus.Pending && x.IsActive);
    }

    public async Task<List<UserReport>> GetReportsByReporterEmailAsync(string reporterEmail)
    {
        return await _dbSet
            .Where(x => x.ReporterEmail != null &&
                       x.ReporterEmail.ToLower() == reporterEmail.ToLower() &&
                       x.IsActive)
            .Include(x => x.SuspiciousDomain)
            .Include(x => x.SuspiciousEmail)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> UpdateReportStatusAsync(int reportId, ReportStatus status, string reviewedBy)
    {
        var report = await GetByIdAsync(reportId);
        if (report == null) return false;

        report.Status = status;
        report.ReviewedBy = reviewedBy;
        report.ReviewedDate = DateTime.UtcNow;
        report.UpdatedDate = DateTime.UtcNow;

        await SaveChangesAsync();
        return true;
    }
}