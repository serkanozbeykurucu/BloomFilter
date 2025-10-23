using BloomFilter.DataAccess.Abstract;
using BloomFilter.DataAccess.Concrete.Context;
using BloomFilter.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace BloomFilter.DataAccess.Concrete.EntityFrameworkCore;

public class EfSuspiciousEmailDal : GenericRepository<SuspiciousEmail>, ISuspiciousEmailDal
{
    public EfSuspiciousEmailDal(BloomFilterDbContext context) : base(context)
    {
    }

    public async Task<SuspiciousEmail?> GetByEmailAddressAsync(string emailAddress)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.EmailAddress.ToLower() == emailAddress.ToLower() && x.IsActive);
    }

    public async Task<List<SuspiciousEmail>> GetEmailsByDomainAsync(string domainName)
    {
        return await _dbSet
            .Where(x => x.DomainName.ToLower() == domainName.ToLower() && x.IsActive)
            .OrderByDescending(x => x.ReportCount)
            .ToListAsync();
    }

    public async Task<List<SuspiciousEmail>> GetMostReportedEmailsAsync(int count = 10)
    {
        return await _dbSet
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.ReportCount)
            .ThenByDescending(x => x.LastReportedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> IsEmailExistsAsync(string emailAddress)
    {
        return await _dbSet
            .AnyAsync(x => x.EmailAddress.ToLower() == emailAddress.ToLower() && x.IsActive);
    }

    public async Task<int> IncrementReportCountAsync(int emailId)
    {
        var email = await GetByIdAsync(emailId);
        if (email == null) return 0;

        email.ReportCount++;
        email.LastReportedDate = DateTime.UtcNow;
        email.UpdatedDate = DateTime.UtcNow;

        return await SaveChangesAsync();
    }

    public async Task<List<SuspiciousEmail>> SearchEmailsAsync(string searchTerm, int pageNumber, int pageSize)
    {
        var query = _dbSet.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.EmailAddress.Contains(searchTerm) ||
                                   x.DomainName.Contains(searchTerm) ||
                                   (x.Description != null && x.Description.Contains(searchTerm)));
        }

        return await query
            .OrderByDescending(x => x.ReportCount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}