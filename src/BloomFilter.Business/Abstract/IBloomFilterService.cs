
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Shared.Responses.Concrete;

namespace BloomFilter.Business.Abstract;

public interface IBloomFilterService
{
    Task<Response> InitializeFiltersAsync();
    Task<Response<bool>> AddDomainToFilterAsync(string domain);
    Task<Response<bool>> AddEmailToFilterAsync(string email);
    Task<Response<bool>> CheckDomainInFilterAsync(string domain);
    Task<Response<bool>> CheckEmailInFilterAsync(string email);
    Task<bool> AddBulkDomainsToFilterAsync(List<string> domains);
    Task<bool> AddBulkEmailsToFilterAsync(List<string> emails);
    Task<Response> RebuildFiltersAsync();
    Task<Response<BloomFilterStatsResponseDto>> GetFilterStatsAsync(string filterName);
}