using BloomFilter.Dto.CommonDTOs;
using BloomFilter.Dto.RequestDTOs;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Shared.Responses.Concrete;

namespace BloomFilter.Business.Abstract;

public interface ISuspiciousEmailService
{
    Task<Response<CheckResultResponseDto>> CheckEmailAsync(CheckSuspiciousRequestDto request);
    Task<Response<SuspiciousEmailResponseDto>> AddSuspiciousEmailAsync(string emailAddress, string? description = null);
    Task<Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>> GetSuspiciousEmailsAsync(PaginationRequestDto request);
    Task<Response<List<SuspiciousEmailResponseDto>>> GetMostReportedEmailsAsync(int count = 10);
    Task<Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>> SearchEmailsAsync(string searchTerm, PaginationRequestDto request);
    Task<Response<BulkOperationResponseDto>> AddBulkEmailsAsync(BulkOperationRequestDto request);
    Task<Response<List<SuspiciousEmailResponseDto>>> GetEmailsByDomainAsync(string domainName);
}
