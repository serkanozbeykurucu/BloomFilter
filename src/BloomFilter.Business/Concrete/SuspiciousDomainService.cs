using BloomFilter.Business.Abstract;
using BloomFilter.DataAccess.Abstract;
using BloomFilter.Dto.CommonDTOs;
using BloomFilter.Dto.RequestDTOs;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Entity.Concrete;
using BloomFilter.Shared.Responses.ComplexTypes;
using BloomFilter.Shared.Responses.Concrete;
using BloomFilter.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BloomFilter.Business.Concrete;

public class SuspiciousDomainService : ISuspiciousDomainService
{
    private readonly ISuspiciousDomainDal _suspiciousDomainDal;
    private readonly IBloomFilterService _bloomFilterService;
    private readonly ILogger<SuspiciousDomainService> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SuspiciousDomainService(
        ISuspiciousDomainDal suspiciousDomainDal,
        IBloomFilterService bloomFilterService,
        ILogger<SuspiciousDomainService> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _suspiciousDomainDal = suspiciousDomainDal;
        _bloomFilterService = bloomFilterService;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Response<CheckResultResponseDto>> CheckDomainAsync(CheckDomainRequestDto request)
    {
        try
        {
            var domainName = request.DomainName.ToLower().Trim();

            var existingDomain = await _suspiciousDomainDal.GetByDomainNameAsync(domainName);

            if (existingDomain != null)
            {
                var exactMatchResult = new CheckResultResponseDto
                {
                    CheckedValue = domainName,
                    IsSuspicious = true,
                    CheckType = "Domain",
                    IsExactMatch = true,
                    AdditionalInfo = _localizer["DomainFoundInDb", existingDomain.ReportCount]
                };

                return new Response<CheckResultResponseDto>(ResponseCode.Success, exactMatchResult);
            }

            var isInBloomFilterResponse = await _bloomFilterService.CheckDomainInFilterAsync(domainName);
            var isInBloomFilter = isInBloomFilterResponse.Data;

            var bloomFilterResult = new CheckResultResponseDto
            {
                CheckedValue = domainName,
                IsSuspicious = isInBloomFilter,
                CheckType = "Domain",
                IsExactMatch = false,
                AdditionalInfo = isInBloomFilter ?
                    _localizer["DomainFlaggedByBloomFilter"] :
                    _localizer["DomainNotFoundInList"]
            };

            return new Response<CheckResultResponseDto>(ResponseCode.Success, bloomFilterResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking domain: {Domain}", request.DomainName);
            return new Response<CheckResultResponseDto>(ResponseCode.Fail, _localizer["DomainCheckError"]);
        }
    }

    public async Task<Response<SuspiciousDomainResponseDto>> AddSuspiciousDomainAsync(string domainName, string? description = null)
    {
        try
        {
            domainName = domainName.ToLower().Trim();

            var existingDomain = await _suspiciousDomainDal.GetByDomainNameAsync(domainName);

            if (existingDomain != null)
            {
                await _suspiciousDomainDal.IncrementReportCountAsync(existingDomain.Id);

                var response = new SuspiciousDomainResponseDto
                {
                    Id = existingDomain.Id,
                    DomainName = existingDomain.DomainName,
                    Description = existingDomain.Description,
                    ReportCount = existingDomain.ReportCount + 1,
                    LastReportedDate = DateTime.UtcNow,
                    CreatedDate = existingDomain.CreatedDate,
                    IsActive = existingDomain.IsActive
                };

                return new Response<SuspiciousDomainResponseDto>(ResponseCode.Success, response, _localizer["DomainReportCountUpdated"]);
            }

            var newDomain = new SuspiciousDomain
            {
                DomainName = domainName,
                Description = description,
                ReportCount = 1,
                LastReportedDate = DateTime.UtcNow
            };

            await _suspiciousDomainDal.AddAsync(newDomain);
            await _suspiciousDomainDal.SaveChangesAsync();

            await _bloomFilterService.AddDomainToFilterAsync(domainName);

            var newDomainResponse = new SuspiciousDomainResponseDto
            {
                Id = newDomain.Id,
                DomainName = newDomain.DomainName,
                Description = newDomain.Description,
                ReportCount = newDomain.ReportCount,
                LastReportedDate = newDomain.LastReportedDate,
                CreatedDate = newDomain.CreatedDate,
                IsActive = newDomain.IsActive
            };

            return new Response<SuspiciousDomainResponseDto>(ResponseCode.Success, newDomainResponse, _localizer["SuspiciousDomainAdded"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding suspicious domain: {Domain}", domainName);
            return new Response<SuspiciousDomainResponseDto>(ResponseCode.Fail, _localizer["AddDomainError"]);
        }
    }

    public async Task<Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>> GetSuspiciousDomainsAsync(PaginationRequestDto request)
    {
        try
        {
            var domains = await _suspiciousDomainDal.GetPagedListAsync(
                request.PageNumber,
                request.PageSize,
                orderBy: x => x.CreatedDate,
                orderByDescending: request.SortDescending);

            var totalCount = await _suspiciousDomainDal.GetCountAsync();

            var domainResponses = domains.Select(d => new SuspiciousDomainResponseDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                Description = d.Description,
                ReportCount = d.ReportCount,
                LastReportedDate = d.LastReportedDate,
                CreatedDate = d.CreatedDate,
                IsActive = d.IsActive
            }).ToList();

            var paginatedResponse = new PaginatedResponseDto<SuspiciousDomainResponseDto>
            {
                Data = domainResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            };

            return new Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>(ResponseCode.Success, paginatedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suspicious domains");
            return new Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>(ResponseCode.Fail, _localizer["GetDomainsError"]);
        }
    }

    public async Task<Response<List<SuspiciousDomainResponseDto>>> GetMostReportedDomainsAsync(int count = 10)
    {
        try
        {
            var domains = await _suspiciousDomainDal.GetMostReportedDomainsAsync(count);

            var domainResponses = domains.Select(d => new SuspiciousDomainResponseDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                Description = d.Description,
                ReportCount = d.ReportCount,
                LastReportedDate = d.LastReportedDate,
                CreatedDate = d.CreatedDate,
                IsActive = d.IsActive
            }).ToList();

            return new Response<List<SuspiciousDomainResponseDto>>(ResponseCode.Success, domainResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most reported domains");
            return new Response<List<SuspiciousDomainResponseDto>>(ResponseCode.Fail, _localizer["GetMostReportedDomainsError"]);
        }
    }

    public async Task<Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>> SearchDomainsAsync(string searchTerm, PaginationRequestDto request)
    {
        try
        {
            var domains = await _suspiciousDomainDal.SearchDomainsAsync(searchTerm, request.PageNumber, request.PageSize);
            var totalCount = await _suspiciousDomainDal.GetCountAsync(d =>
                d.DomainName.Contains(searchTerm) ||
                (d.Description != null && d.Description.Contains(searchTerm)));

            var domainResponses = domains.Select(d => new SuspiciousDomainResponseDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                Description = d.Description,
                ReportCount = d.ReportCount,
                LastReportedDate = d.LastReportedDate,
                CreatedDate = d.CreatedDate,
                IsActive = d.IsActive
            }).ToList();

            var paginatedResponse = new PaginatedResponseDto<SuspiciousDomainResponseDto>
            {
                Data = domainResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            };

            return new Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>(ResponseCode.Success, paginatedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching domains with term: {SearchTerm}", searchTerm);
            return new Response<PaginatedResponseDto<SuspiciousDomainResponseDto>>(ResponseCode.Fail, _localizer["SearchDomainsError"]);
        }
    }

    public async Task<Response<BulkOperationResponseDto>> AddBulkDomainsAsync(BulkOperationRequestDto request)
    {
        try
        {
            var successItems = new List<string>();
            var failedItems = new List<string>();

            foreach (var domain in request.Values)
            {
                try
                {
                    var result = await AddSuspiciousDomainAsync(domain, request.Description);
                    if (result.ResponseCode == ResponseCode.Success)
                    {
                        successItems.Add(domain);
                    }
                    else
                    {
                        failedItems.Add(domain);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding domain in bulk operation: {Domain}", domain);
                    failedItems.Add(domain);
                }
            }

            var bulkResponse = new BulkOperationResponseDto
            {
                TotalCount = request.Values.Count,
                SuccessCount = successItems.Count,
                FailedCount = failedItems.Count,
                SuccessItems = successItems,
                FailedItems = failedItems
            };

            return new Response<BulkOperationResponseDto>(ResponseCode.Success, bulkResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk domain operation");
            return new Response<BulkOperationResponseDto>(ResponseCode.Fail, _localizer["BulkOperationError"]);
        }
    }
}