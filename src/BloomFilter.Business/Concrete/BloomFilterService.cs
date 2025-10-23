using BloomFilter.Business.Abstract;
using BloomFilter.DataAccess.Abstract;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Entity.Concrete;
using BloomFilter.Shared.Responses.ComplexTypes;
using BloomFilter.Shared.Responses.Concrete;
using BloomFilter.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BloomFilter.Business.Concrete;

public class BloomFilterService : IBloomFilterService
{
    private readonly IBloomFilterDataDal _bloomFilterDataDal;
    private readonly ISuspiciousDomainDal _suspiciousDomainDal;
    private readonly ISuspiciousEmailDal _suspiciousEmailDal;
    private readonly ILogger<BloomFilterService> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    private BloomFilterImplementation? _domainFilter;
    private BloomFilterImplementation? _emailFilter;

    private const int DefaultFilterSize = 1000000;
    private const int DefaultHashFunctionCount = 7;
    private const string DomainFilterName = "DomainFilter";
    private const string EmailFilterName = "EmailFilter";

    public BloomFilterService(
        IBloomFilterDataDal bloomFilterDataDal,
        ISuspiciousDomainDal suspiciousDomainDal,
        ISuspiciousEmailDal suspiciousEmailDal,
        ILogger<BloomFilterService> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _bloomFilterDataDal = bloomFilterDataDal;
        _suspiciousDomainDal = suspiciousDomainDal;
        _suspiciousEmailDal = suspiciousEmailDal;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Response> InitializeFiltersAsync()
    {
        try
        {
            await InitializeDomainFilterAsync();
            await InitializeEmailFilterAsync();
            _logger.LogInformation("Bloom filters initialized successfully");
            return new Response(ResponseCode.Success, _localizer["BloomFiltersInitialized"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing bloom filters");
            return new Response(ResponseCode.Fail, _localizer["ErrorInitializingBloomFilter"]);
        }
    }

    private async Task InitializeDomainFilterAsync()
    {
        var filterData = await _bloomFilterDataDal.GetByFilterNameAsync(DomainFilterName);

        if (filterData != null)
        {
            _domainFilter = new BloomFilterImplementation(
                filterData.FilterSize,
                filterData.HashFunctionCount,
                filterData.BitArray,
                filterData.ElementCount);
        }
        else
        {
            _domainFilter = new BloomFilterImplementation(DefaultFilterSize, DefaultHashFunctionCount);

            var newFilterData = new BloomFilterData
            {
                FilterName = DomainFilterName,
                FilterSize = DefaultFilterSize,
                HashFunctionCount = DefaultHashFunctionCount,
                BitArray = _domainFilter.SerializeBitArray(),
                ElementCount = 0,
                Description = "Bloom filter for suspicious domains"
            };

            await _bloomFilterDataDal.AddAsync(newFilterData);
            await _bloomFilterDataDal.SaveChangesAsync();
        }
    }

    private async Task InitializeEmailFilterAsync()
    {
        var filterData = await _bloomFilterDataDal.GetByFilterNameAsync(EmailFilterName);

        if (filterData != null)
        {
            _emailFilter = new BloomFilterImplementation(
                filterData.FilterSize,
                filterData.HashFunctionCount,
                filterData.BitArray,
                filterData.ElementCount);
        }
        else
        {
            _emailFilter = new BloomFilterImplementation(DefaultFilterSize, DefaultHashFunctionCount);

            var newFilterData = new BloomFilterData
            {
                FilterName = EmailFilterName,
                FilterSize = DefaultFilterSize,
                HashFunctionCount = DefaultHashFunctionCount,
                BitArray = _emailFilter.SerializeBitArray(),
                ElementCount = 0,
                Description = "Bloom filter for suspicious emails"
            };

            await _bloomFilterDataDal.AddAsync(newFilterData);
            await _bloomFilterDataDal.SaveChangesAsync();
        }
    }

    public async Task<Response<bool>> AddDomainToFilterAsync(string domain)
    {
        if (_domainFilter == null)
            return new Response<bool>(ResponseCode.BadRequest, false, _localizer["DomainFilterNotInitialized"]);

        try
        {
            _domainFilter.Add(domain.ToLower());
            await _bloomFilterDataDal.UpdateBitArrayAsync(
                DomainFilterName,
                _domainFilter.SerializeBitArray(),
                _domainFilter.elementCount);

            return new Response<bool>(ResponseCode.Success, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding domain {Domain} to filter", domain);
            return new Response<bool>(ResponseCode.BadRequest, false, _localizer["AddDomainError"]);
        }
    }

    public async Task<Response<bool>> AddEmailToFilterAsync(string email)
    {
        if (_emailFilter == null) 
            return new Response<bool>(ResponseCode.BadRequest, false, _localizer["EmailFilterNotInitialized"]);

        try
        {
            _emailFilter.Add(email.ToLower());
            await _bloomFilterDataDal.UpdateBitArrayAsync(
                EmailFilterName,
                _emailFilter.SerializeBitArray(),
                _emailFilter.elementCount);

            return new Response<bool>(ResponseCode.Success, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding email {Email} to filter", email);
            return new Response<bool>(ResponseCode.BadRequest, false, _localizer["AddEmailError"]);
        }
    }

    public async Task<Response<bool>> CheckDomainInFilterAsync(string domain)
    {
        if (_domainFilter == null)
        {
            await InitializeFiltersAsync();
        }

        var contains = _domainFilter?.Contains(domain.ToLower());
        if (contains.HasValue)
        {
            var message = contains.Value
                ? _localizer["DomainFoundInFilter", domain]
                : _localizer["DomainNotFoundInFilter", domain];

            return new Response<bool>(ResponseCode.Success, contains.Value, message);
        }
        return new Response<bool>(ResponseCode.BadRequest, false, _localizer["DomainCheckFailed"]);
    }

    public async Task<Response<bool>> CheckEmailInFilterAsync(string email)
    {
        if (_emailFilter == null)
        {
            await InitializeFiltersAsync();
        }

        var emailExists = _emailFilter?.Contains(email.ToLower()) ?? false;
        var message = emailExists
            ? _localizer["EmailFoundInFilter", email]
            : _localizer["EmailNotFoundInFilter", email];

        return new Response<bool>(ResponseCode.Success, emailExists, message);
    }

    public async Task<bool> AddBulkDomainsToFilterAsync(List<string> domains)
    {
        if (_domainFilter == null) return false;

        try
        {
            foreach (var domain in domains)
            {
                _domainFilter.Add(domain.ToLower());
            }

            await _bloomFilterDataDal.UpdateBitArrayAsync(
                DomainFilterName,
                _domainFilter.SerializeBitArray(),
                _domainFilter.elementCount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding bulk domains to filter");
            return false;
        }
    }

    public async Task<bool> AddBulkEmailsToFilterAsync(List<string> emails)
    {
        if (_emailFilter == null) return false;

        try
        {
            foreach (var email in emails)
            {
                _emailFilter.Add(email.ToLower());
            }

            await _bloomFilterDataDal.UpdateBitArrayAsync(
                EmailFilterName,
                _emailFilter.SerializeBitArray(),
                _emailFilter.elementCount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding bulk emails to filter");
            return false;
        }
    }

    public async Task<Response> RebuildFiltersAsync()
    {
        try
        {
            _domainFilter = new BloomFilterImplementation(DefaultFilterSize, DefaultHashFunctionCount);
            var domains = await _suspiciousDomainDal.GetAllAsync();

            foreach (var domain in domains)
            {
                _domainFilter.Add(domain.DomainName.ToLower());
            }

            await _bloomFilterDataDal.UpdateBitArrayAsync(
                DomainFilterName,
                _domainFilter.SerializeBitArray(),
                _domainFilter.elementCount);

            _emailFilter = new BloomFilterImplementation(DefaultFilterSize, DefaultHashFunctionCount);
            var emails = await _suspiciousEmailDal.GetAllAsync();

            foreach (var email in emails)
            {
                _emailFilter.Add(email.EmailAddress.ToLower());
            }

            await _bloomFilterDataDal.UpdateBitArrayAsync(
                EmailFilterName,
                _emailFilter.SerializeBitArray(),
                _emailFilter.elementCount);

            _logger.LogInformation("Bloom filters rebuilt successfully");
            return new Response(ResponseCode.Success, _localizer["BloomFilterRebuilt"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding bloom filters");
            return new Response(ResponseCode.Fail, _localizer["ErrorRebuildingBloomFilter"]);
        }
    }

    public async Task<Response<BloomFilterStatsResponseDto>> GetFilterStatsAsync(string filterName)
    {
        try
        {
            var filterData = await _bloomFilterDataDal.GetByFilterNameAsync(filterName);

            if (filterData == null)
            {
                return new Response<BloomFilterStatsResponseDto>(
                    ResponseCode.NotFound,
                    null,
                    _localizer["FilterNotFound", filterName]);
            }

            BloomFilterImplementation? filter = filterName.ToLower() switch
            {
                "domainfilter" => _domainFilter,
                "emailfilter" => _emailFilter,
                _ => null
            };

            var currentFalsePositiveRate = filter?.GetCurrentFalsePositiveRate() ?? 0;
            var currentLoadFactor = filterData.FilterSize > 0
                ? (double)filterData.ElementCount / filterData.FilterSize
                : 0;

            var statsDto = new BloomFilterStatsResponseDto
            {
                FilterName = filterData.FilterName,
                FilterSize = filterData.FilterSize,
                HashFunctionCount = filterData.HashFunctionCount,
                ElementCount = filterData.ElementCount,
                ExpectedFalsePositiveRate = currentFalsePositiveRate,
                CurrentLoadFactor = currentLoadFactor,
                LastUpdatedDate = filterData.UpdatedDate ?? filterData.CreatedDate,
                Description = filterData.Description
            };

            return new Response<BloomFilterStatsResponseDto>(
                ResponseCode.Success,
                statsDto,
                _localizer["FilterStatsSuccess"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter stats for {FilterName}", filterName);
            return new Response<BloomFilterStatsResponseDto>(
                ResponseCode.Fail,
                null,
                _localizer["FilterStatsError"]);
        }
    }
}