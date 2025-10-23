using BloomFilter.DataAccess.Abstract;
using BloomFilter.DataAccess.Concrete.Context;
using BloomFilter.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace BloomFilter.DataAccess.Concrete.EntityFrameworkCore;

public class EfSuspiciousDomainDal : GenericRepository<SuspiciousDomain>, ISuspiciousDomainDal
{
    public EfSuspiciousDomainDal(BloomFilterDbContext context) : base(context)
    {
    }

    public async Task<SuspiciousDomain?> GetByDomainNameAsync(string domainName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.DomainName.ToLower() == domainName.ToLower() && x.IsActive);
    }

    public async Task<List<SuspiciousDomain>> GetMostReportedDomainsAsync(int count = 10)
    {
        return await _dbSet
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.ReportCount)
            .ThenByDescending(x => x.LastReportedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> IsDomainExistsAsync(string domainName)
    {
        return await _dbSet
            .AnyAsync(x => x.DomainName.ToLower() == domainName.ToLower() && x.IsActive);
    }

    public async Task<int> IncrementReportCountAsync(int domainId)
    {
        var domain = await GetByIdAsync(domainId);
        if (domain == null) return 0;

        domain.ReportCount++;
        domain.LastReportedDate = DateTime.UtcNow;
        domain.UpdatedDate = DateTime.UtcNow;

        return await SaveChangesAsync();
    }

    public async Task<List<SuspiciousDomain>> SearchDomainsAsync(string searchTerm, int pageNumber, int pageSize)
    {
        var query = _dbSet.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.DomainName.Contains(searchTerm) ||
                                   (x.Description != null && x.Description.Contains(searchTerm)));
        }

        return await query
            .OrderByDescending(x => x.ReportCount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}