using BloomFilter.Business.Abstract;
using BloomFilter.Dto.CommonDTOs;
using BloomFilter.Dto.RequestDTOs;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Shared.Responses.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace BloomFilter.HttpApi.Controllers.v1;

[ApiController]
[Route("api/[controller]")]
public class SuspiciousDomainController : ControllerBase
{
    private readonly ISuspiciousDomainService _suspiciousDomainService;

    public SuspiciousDomainController(ISuspiciousDomainService suspiciousDomainService)
    {
        _suspiciousDomainService = suspiciousDomainService;
    }

    [HttpPost]
    [Route("Check")]
    public async Task<Response<CheckResultResponseDto>> CheckDomain([FromBody] CheckDomainRequestDto request)
    {
        return await _suspiciousDomainService.CheckDomainAsync(request);
    }

    [HttpGet]
    [Route("GetDomains")]
    public async Task<Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>> GetDomains([FromQuery] PaginationRequestDto request)
    {
        return await _suspiciousDomainService.GetSuspiciousDomainsAsync(request);
    }

    [HttpGet]
    [Route("GetMostReportedDomains")]
    public async Task<Response<List<SuspiciousDomainResponseDto>>> GetMostReportedDomains([FromQuery] int count = 10)
    {
        return await _suspiciousDomainService.GetMostReportedDomainsAsync(count);
    }

    [HttpGet]
    [Route("SearchDomains")]
    public async Task<Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>> SearchDomains([FromQuery] string searchTerm, [FromQuery] PaginationRequestDto request)
    {
        return await _suspiciousDomainService.SearchDomainsAsync(searchTerm, request);
    }

    [HttpPost("bulk")]
    public async Task<Response<BulkOperationResponseDto>> AddBulkDomains([FromBody] BulkOperationRequestDto request)
    {
        return await _suspiciousDomainService.AddBulkDomainsAsync(request);
    }
}