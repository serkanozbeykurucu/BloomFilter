using BloomFilter.Dto.CommonDTOs;
using BloomFilter.Dto.RequestDTOs;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Shared.Responses.Concrete;

namespace BloomFilter.Business.Abstract;

public interface IUserReportService
{
    Task<Response<UserReportResponseDto>> CreateReportAsync(CreateUserReportRequestDto request, string? ipAddress = null);
    Task<Response<PaginatedResponseDto<UserReportResponseDto>>> GetReportsByStatusAsync(int status, PaginationRequestDto request);
    Task<Response<List<UserReportResponseDto>>> GetRecentReportsAsync(int count = 50);
    Task<Response> UpdateReportStatusAsync(UpdateReportStatusRequestDto request);
    Task<Response<DashboardStatsResponseDto>> GetDashboardStatsAsync();
}