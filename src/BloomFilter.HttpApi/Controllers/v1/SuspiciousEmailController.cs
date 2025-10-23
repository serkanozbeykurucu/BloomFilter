using BloomFilter.Business.Abstract;
using BloomFilter.Dto.CommonDTOs;
using BloomFilter.Dto.RequestDTOs;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Shared.Responses.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace BloomFilter.HttpApi.Controllers.v1;

[ApiController]
[Route("api/[controller]")]
public class SuspiciousEmailController : ControllerBase
{
    private readonly ISuspiciousEmailService _suspiciousEmailService;

    public SuspiciousEmailController(ISuspiciousEmailService suspiciousEmailService)
    {
        _suspiciousEmailService = suspiciousEmailService;
    }

    [HttpPost]
    [Route("Check")]
    public async Task<Response<CheckResultResponseDto>> CheckEmail([FromBody] CheckSuspiciousRequestDto request)
    {
        return await _suspiciousEmailService.CheckEmailAsync(request);
    }

    [HttpGet]
    [Route("GetSuspiciousEmails")]
    public async Task<Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>> GetEmails([FromQuery] PaginationRequestDto request)
    {
        return await _suspiciousEmailService.GetSuspiciousEmailsAsync(request);
    }

    [HttpGet]
    [Route("GetMostReportedEmails")]
    public async Task<Response<List<SuspiciousEmailResponseDto>>> GetMostReportedEmails([FromQuery] int count = 10)
    {
        return await _suspiciousEmailService.GetMostReportedEmailsAsync(count);

    }

    [HttpGet]
    [Route("SearchEmails")]
    public async Task<Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>> SearchEmails([FromQuery] string searchTerm, [FromQuery] PaginationRequestDto request)
    {
        return await _suspiciousEmailService.SearchEmailsAsync(searchTerm, request);
    }

    [HttpGet]
    [Route("GetEmailsByDomain")]
    public async Task<Response<List<SuspiciousEmailResponseDto>>> GetEmailsByDomain(string domainName)
    {
        return await _suspiciousEmailService.GetEmailsByDomainAsync(domainName);
    }

    [HttpPost]
    [Route("AddBulkEmails")]
    public async Task<Response<BulkOperationResponseDto>> AddBulkEmails([FromBody] BulkOperationRequestDto request)
    {
        return await _suspiciousEmailService.AddBulkEmailsAsync(request);
    }
}