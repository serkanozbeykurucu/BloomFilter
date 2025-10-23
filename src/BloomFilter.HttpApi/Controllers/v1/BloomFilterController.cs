using BloomFilter.Business.Abstract;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Shared.Responses.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace BloomFilter.HttpApi.Controllers.v1;

[ApiController]
[Route("api/[controller]")]
public class BloomFilterController : ControllerBase
{
    private readonly IBloomFilterService _bloomFilterService;

    public BloomFilterController(IBloomFilterService bloomFilterService)
    {
        _bloomFilterService = bloomFilterService;
    }

    [HttpPost]
    [Route("Initialize")]
    public async Task<Response> InitializeFilters()
    {
        return await _bloomFilterService.InitializeFiltersAsync();
    }

    [HttpPost]
    [Route("Rebuild")]
    public async Task<Response> RebuildFilters()
    {
        return await _bloomFilterService.RebuildFiltersAsync();
    }

    [HttpGet]
    [Route("GetFilterStats")]
    public async Task<Response<BloomFilterStatsResponseDto>> GetFilterStats(string filterName)
    {
        return await _bloomFilterService.GetFilterStatsAsync(filterName);
    }

    [HttpPost("AddDomain")]
    public async Task<Response<bool>> AddDomainToFilter([FromBody] string domain)
    {
        return await _bloomFilterService.AddDomainToFilterAsync(domain);
    }

    [HttpPost]
    [Route("AddEmail")]
    public async Task<Response<bool>> AddEmailToFilter([FromBody] string email)
    {
        return await _bloomFilterService.AddEmailToFilterAsync(email);
    }

    [HttpPost]
    [Route("CheckDomain")]
    public async Task<Response<bool>> CheckDomainInFilter([FromBody] string domain)
    {
        return await _bloomFilterService.CheckDomainInFilterAsync(domain);
    }

    [HttpPost]
    [Route("CheckEmail")]
    public async Task<Response<bool>> CheckEmailInFilter([FromBody] string email)
    {
        return await _bloomFilterService.CheckEmailInFilterAsync(email);
    }
}