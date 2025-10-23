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

public class SuspiciousEmailService : ISuspiciousEmailService
{
    private readonly ISuspiciousEmailDal _suspiciousEmailDal;
    private readonly ISuspiciousDomainService _suspiciousDomainService;
    private readonly IBloomFilterService _bloomFilterService;
    private readonly ILogger<SuspiciousEmailService> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SuspiciousEmailService(
        ISuspiciousEmailDal suspiciousEmailDal,
        ISuspiciousDomainService suspiciousDomainService,
        IBloomFilterService bloomFilterService,
        ILogger<SuspiciousEmailService> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _suspiciousEmailDal = suspiciousEmailDal;
        _suspiciousDomainService = suspiciousDomainService;
        _bloomFilterService = bloomFilterService;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Response<CheckResultResponseDto>> CheckEmailAsync(CheckSuspiciousRequestDto request)
    {
        try
        {
            var emailAddress = request.EmailAddress.ToLower().Trim();
            var domainName = ExtractDomainFromEmail(emailAddress);

            var existingEmail = await _suspiciousEmailDal.GetByEmailAddressAsync(emailAddress);

            if (existingEmail != null)
            {
                var exactMatchResult = new CheckResultResponseDto
                {
                    CheckedValue = emailAddress,
                    IsSuspicious = true,
                    CheckType = "Email",
                    IsExactMatch = true,
                    AdditionalInfo = _localizer["EmailFoundInDb", existingEmail.ReportCount]
                };

                return new Response<CheckResultResponseDto>(ResponseCode.Success, exactMatchResult);
            }

            var isEmailInBloomFilterResponse = await _bloomFilterService.CheckEmailInFilterAsync(emailAddress);
            var isDomainInBloomFilterResponse = await _bloomFilterService.CheckDomainInFilterAsync(domainName);

            var isEmailInBloomFilter = isEmailInBloomFilterResponse.Data;
            var isDomainInBloomFilter = isDomainInBloomFilterResponse.Data;

            var isSuspicious = isEmailInBloomFilter || isDomainInBloomFilter;
            var additionalInfo = "";

            if (isEmailInBloomFilter)
                additionalInfo = _localizer["EmailFlaggedByBloomFilter"];
            else if (isDomainInBloomFilter)
                additionalInfo = _localizer["DomainFlaggedByBloomFilter"];
            else
                additionalInfo = _localizer["EmailAndDomainNotFoundInList"];

            var bloomFilterResult = new CheckResultResponseDto
            {
                CheckedValue = emailAddress,
                IsSuspicious = isSuspicious,
                CheckType = "Email",
                IsExactMatch = false,
                AdditionalInfo = additionalInfo
            };

            return new Response<CheckResultResponseDto>(ResponseCode.Success, bloomFilterResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email: {Email}", request.EmailAddress);
            return new Response<CheckResultResponseDto>(ResponseCode.Fail, _localizer["EmailCheckError"]);
        }
    }

    public async Task<Response<SuspiciousEmailResponseDto>> AddSuspiciousEmailAsync(string emailAddress, string? description = null)
    {
        try
        {
            emailAddress = emailAddress.ToLower().Trim();
            var domainName = ExtractDomainFromEmail(emailAddress);

            var existingEmail = await _suspiciousEmailDal.GetByEmailAddressAsync(emailAddress);

            if (existingEmail != null)
            {
                await _suspiciousEmailDal.IncrementReportCountAsync(existingEmail.Id);

                var response = new SuspiciousEmailResponseDto
                {
                    Id = existingEmail.Id,
                    EmailAddress = existingEmail.EmailAddress,
                    DomainName = existingEmail.DomainName,
                    Description = existingEmail.Description,
                    ReportCount = existingEmail.ReportCount + 1,
                    LastReportedDate = DateTime.UtcNow,
                    CreatedDate = existingEmail.CreatedDate,
                    IsActive = existingEmail.IsActive
                };

                return new Response<SuspiciousEmailResponseDto>(ResponseCode.Success, response, _localizer["EmailReportCountUpdated"]);
            }

            var newEmail = new SuspiciousEmail
            {
                EmailAddress = emailAddress,
                DomainName = domainName,
                Description = description,
                ReportCount = 1,
                LastReportedDate = DateTime.UtcNow
            };

            await _suspiciousEmailDal.AddAsync(newEmail);
            await _suspiciousEmailDal.SaveChangesAsync();

            await _bloomFilterService.AddEmailToFilterAsync(emailAddress);

            await _suspiciousDomainService.AddSuspiciousDomainAsync(domainName, _localizer["DomainExtractedFromEmail"]);

            var newEmailResponse = new SuspiciousEmailResponseDto
            {
                Id = newEmail.Id,
                EmailAddress = newEmail.EmailAddress,
                DomainName = newEmail.DomainName,
                Description = newEmail.Description,
                ReportCount = newEmail.ReportCount,
                LastReportedDate = newEmail.LastReportedDate,
                CreatedDate = newEmail.CreatedDate,
                IsActive = newEmail.IsActive
            };

            return new Response<SuspiciousEmailResponseDto>(ResponseCode.Success, newEmailResponse, _localizer["SuspiciousEmailAdded"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding suspicious email: {Email}", emailAddress);
            return new Response<SuspiciousEmailResponseDto>(ResponseCode.Fail, _localizer["AddEmailError"]);
        }
    }

    public async Task<Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>> GetSuspiciousEmailsAsync(PaginationRequestDto request)
    {
        try
        {
            var emails = await _suspiciousEmailDal.GetPagedListAsync(
                request.PageNumber,
                request.PageSize,
                orderBy: x => x.CreatedDate,
                orderByDescending: request.SortDescending);

            var totalCount = await _suspiciousEmailDal.GetCountAsync();

            var emailResponses = emails.Select(e => new SuspiciousEmailResponseDto
            {
                Id = e.Id,
                EmailAddress = e.EmailAddress,
                DomainName = e.DomainName,
                Description = e.Description,
                ReportCount = e.ReportCount,
                LastReportedDate = e.LastReportedDate,
                CreatedDate = e.CreatedDate,
                IsActive = e.IsActive
            }).ToList();

            var paginatedResponse = new PaginatedResponseDto<SuspiciousEmailResponseDto>
            {
                Data = emailResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            };

            return new Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>(ResponseCode.Success, paginatedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suspicious emails");
            return new Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>(ResponseCode.Fail, _localizer["GetEmailsError"]);
        }
    }

    public async Task<Response<List<SuspiciousEmailResponseDto>>> GetMostReportedEmailsAsync(int count = 10)
    {
        try
        {
            var emails = await _suspiciousEmailDal.GetMostReportedEmailsAsync(count);

            var emailResponses = emails.Select(e => new SuspiciousEmailResponseDto
            {
                Id = e.Id,
                EmailAddress = e.EmailAddress,
                DomainName = e.DomainName,
                Description = e.Description,
                ReportCount = e.ReportCount,
                LastReportedDate = e.LastReportedDate,
                CreatedDate = e.CreatedDate,
                IsActive = e.IsActive
            }).ToList();

            return new Response<List<SuspiciousEmailResponseDto>>(ResponseCode.Success, emailResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most reported emails");
            return new Response<List<SuspiciousEmailResponseDto>>(ResponseCode.Fail, _localizer["GetMostReportedEmailsError"]);
        }
    }

    public async Task<Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>> SearchEmailsAsync(string searchTerm, PaginationRequestDto request)
    {
        try
        {
            var emails = await _suspiciousEmailDal.SearchEmailsAsync(searchTerm, request.PageNumber, request.PageSize);
            var totalCount = await _suspiciousEmailDal.GetCountAsync(e =>
                e.EmailAddress.Contains(searchTerm) ||
                e.DomainName.Contains(searchTerm) ||
                (e.Description != null && e.Description.Contains(searchTerm)));

            var emailResponses = emails.Select(e => new SuspiciousEmailResponseDto
            {
                Id = e.Id,
                EmailAddress = e.EmailAddress,
                DomainName = e.DomainName,
                Description = e.Description,
                ReportCount = e.ReportCount,
                LastReportedDate = e.LastReportedDate,
                CreatedDate = e.CreatedDate,
                IsActive = e.IsActive
            }).ToList();

            var paginatedResponse = new PaginatedResponseDto<SuspiciousEmailResponseDto>
            {
                Data = emailResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            };

            return new Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>(ResponseCode.Success, paginatedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching emails with term: {SearchTerm}", searchTerm);
            return new Response<PaginatedResponseDto<SuspiciousEmailResponseDto>>(ResponseCode.Fail, _localizer["SearchEmailsError"]);
        }
    }

    public async Task<Response<BulkOperationResponseDto>> AddBulkEmailsAsync(BulkOperationRequestDto request)
    {
        try
        {
            var successItems = new List<string>();
            var failedItems = new List<string>();

            foreach (var email in request.Values)
            {
                try
                {
                    var result = await AddSuspiciousEmailAsync(email, request.Description);
                    if (result.ResponseCode == ResponseCode.Success)
                    {
                        successItems.Add(email);
                    }
                    else
                    {
                        failedItems.Add(email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding email in bulk operation: {Email}", email);
                    failedItems.Add(email);
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
            _logger.LogError(ex, "Error in bulk email operation");
            return new Response<BulkOperationResponseDto>(ResponseCode.Fail, _localizer["BulkOperationError"]);
        }
    }

    public async Task<Response<List<SuspiciousEmailResponseDto>>> GetEmailsByDomainAsync(string domainName)
    {
        try
        {
            var emails = await _suspiciousEmailDal.GetEmailsByDomainAsync(domainName);

            var emailResponses = emails.Select(e => new SuspiciousEmailResponseDto
            {
                Id = e.Id,
                EmailAddress = e.EmailAddress,
                DomainName = e.DomainName,
                Description = e.Description,
                ReportCount = e.ReportCount,
                LastReportedDate = e.LastReportedDate,
                CreatedDate = e.CreatedDate,
                IsActive = e.IsActive
            }).ToList();

            return new Response<List<SuspiciousEmailResponseDto>>(ResponseCode.Success, emailResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emails by domain: {Domain}", domainName);
            return new Response<List<SuspiciousEmailResponseDto>>(ResponseCode.Fail, _localizer["GetEmailsByDomainError"]);
        }
    }

    private string ExtractDomainFromEmail(string email)
    {
        var atIndex = email.LastIndexOf('@');
        return atIndex > 0 && atIndex < email.Length - 1 ? email.Substring(atIndex + 1) : string.Empty;
    }
}