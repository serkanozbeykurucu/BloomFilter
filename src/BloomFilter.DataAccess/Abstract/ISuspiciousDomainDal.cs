using BloomFilter.Entity.Concrete;

namespace BloomFilter.DataAccess.Abstract;

public interface ISuspiciousDomainDal : IGenericRepositoryDal<SuspiciousDomain>
{
    Task<SuspiciousDomain?> GetByDomainNameAsync(string domainName);
    Task<List<SuspiciousDomain>> GetMostReportedDomainsAsync(int count = 10);
    Task<bool> IsDomainExistsAsync(string domainName);
    Task<int> IncrementReportCountAsync(int domainId);
    Task<List<SuspiciousDomain>> SearchDomainsAsync(string searchTerm, int pageNumber, int pageSize);
}