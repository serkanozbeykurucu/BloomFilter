using BloomFilter.Business.Abstract;
using BloomFilter.Dto.CommonDTOs;
using BloomFilter.Dto.RequestDTOs;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Shared.Responses.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BloomFilter.HttpApi.Controllers.v1;

[ApiController]
[Route("api/[controller]")]
public class UserReportController : ControllerBase
{
    private readonly IUserReportService _userReportService;

    public UserReportController(IUserReportService userReportService)
    {
        _userReportService = userReportService;
    }

    [HttpPost]
    [Route("Create")]
    public async Task<Response<UserReportResponseDto>> CreateReport([FromBody] CreateUserReportRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        return await _userReportService.CreateReportAsync(request, ipAddress);
    }

    [HttpGet]
    [Route("GetReportsByStatus")]
    public async Task<Response<PaginatedResponseDto<UserReportResponseDto>>> GetReportsByStatus(int status, [FromBody] PaginationRequestDto request)
    {
        return await _userReportService.GetReportsByStatusAsync(status, request);
    }

    [HttpGet]
    [Route("GetRecentReports")]
    public async Task<Response<List<UserReportResponseDto>>> GetRecentReports([FromQuery] int count = 50)
    {
        return await _userReportService.GetRecentReportsAsync(count);
    }

    [HttpPut]
    [Route("UpdateReportStatus")]
    public async Task<Response> UpdateReportStatus([FromBody] UpdateReportStatusRequestDto request)
    {
        return await _userReportService.UpdateReportStatusAsync(request);
    }

    [HttpGet]
    [Route("GetDashboardStats")]
    public async Task<Response<DashboardStatsResponseDto>> GetDashboardStats()
    {
        return await _userReportService.GetDashboardStatsAsync();
    }
}