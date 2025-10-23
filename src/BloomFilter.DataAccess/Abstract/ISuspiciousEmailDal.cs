using BloomFilter.Entity.Concrete;

namespace BloomFilter.DataAccess.Abstract;

public interface ISuspiciousEmailDal : IGenericRepositoryDal<SuspiciousEmail>
{
    Task<SuspiciousEmail?> GetByEmailAddressAsync(string emailAddress);
    Task<List<SuspiciousEmail>> GetEmailsByDomainAsync(string domainName);
    Task<List<SuspiciousEmail>> GetMostReportedEmailsAsync(int count = 10);
    Task<bool> IsEmailExistsAsync(string emailAddress);
    Task<int> IncrementReportCountAsync(int emailId);
    Task<List<SuspiciousEmail>> SearchEmailsAsync(string searchTerm, int pageNumber, int pageSize);
}