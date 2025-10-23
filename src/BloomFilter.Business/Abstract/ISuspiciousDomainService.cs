using BloomFilter.Dto.CommonDTOs;
using BloomFilter.Dto.RequestDTOs;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Shared.Responses.Concrete;

namespace BloomFilter.Business.Abstract;

public interface ISuspiciousDomainService
{
    Task<Response<CheckResultResponseDto>> CheckDomainAsync(CheckDomainRequestDto request);
    Task<Response<SuspiciousDomainResponseDto>> AddSuspiciousDomainAsync(string domainName, string? description = null);
    Task<Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>> GetSuspiciousDomainsAsync(PaginationRequestDto request);
    Task<Response<List<SuspiciousDomainResponseDto>>> GetMostReportedDomainsAsync(int count = 10);
    Task<Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>> SearchDomainsAsync(string searchTerm, PaginationRequestDto request);
    Task<Response<BulkOperationResponseDto>> AddBulkDomainsAsync(BulkOperationRequestDto request);
}
