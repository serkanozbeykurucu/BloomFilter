using BloomFilter.Business.Abstract;
using BloomFilter.DataAccess.Abstract;
using BloomFilter.Dto.CommonDTOs;
using BloomFilter.Dto.RequestDTOs;
using BloomFilter.Dto.ResponseDTOs;
using BloomFilter.Entity.Concrete;
using BloomFilter.Entity.Enums;
using BloomFilter.Shared.Responses.ComplexTypes;
using BloomFilter.Shared.Responses.Concrete;
using BloomFilter.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BloomFilter.Business.Concrete;

public class UserReportService : IUserReportService
{
    private readonly IUserReportDal _userReportDal;
    private readonly ISuspiciousDomainService _suspiciousDomainService;
    private readonly ISuspiciousEmailService _suspiciousEmailService;
    private readonly ISuspiciousDomainDal _suspiciousDomainDal;
    private readonly ISuspiciousEmailDal _suspiciousEmailDal;
    private readonly IBloomFilterDataDal _bloomFilterDataDal;
    private readonly ILogger<UserReportService> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UserReportService(
        IUserReportDal userReportDal,
        ISuspiciousDomainService suspiciousDomainService,
        ISuspiciousEmailService suspiciousEmailService,
        ISuspiciousDomainDal suspiciousDomainDal,
        ISuspiciousEmailDal suspiciousEmailDal,
        IBloomFilterDataDal bloomFilterDataDal,
        ILogger<UserReportService> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _userReportDal = userReportDal;
        _suspiciousDomainService = suspiciousDomainService;
        _suspiciousEmailService = suspiciousEmailService;
        _suspiciousDomainDal = suspiciousDomainDal;
        _suspiciousEmailDal = suspiciousEmailDal;
        _bloomFilterDataDal = bloomFilterDataDal;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Response<UserReportResponseDto>> CreateReportAsync(CreateUserReportRequestDto request, string? ipAddress = null)
    {
        try
        {
            var reportedValue = request.ReportedValue.ToLower().Trim();

            var newReport = new UserReport
            {
                ReporterName = request.ReporterName,
                ReporterEmail = request.ReporterEmail,
                ReportType = request.ReportType,
                ReportedValue = reportedValue,
                Description = request.Description,
                ReporterIpAddress = ipAddress,
                Status = ReportStatus.Pending
            };

            await _userReportDal.AddAsync(newReport);
            await _userReportDal.SaveChangesAsync();

            await ProcessReportAsync(newReport);

            var reportResponse = new UserReportResponseDto
            {
                Id = newReport.Id,
                ReporterName = newReport.ReporterName,
                ReporterEmail = newReport.ReporterEmail,
                ReportType = newReport.ReportType,
                ReportedValue = newReport.ReportedValue,
                Description = newReport.Description,
                Status = newReport.Status,
                CreatedDate = newReport.CreatedDate,
                ReviewedDate = newReport.ReviewedDate,
                ReviewedBy = newReport.ReviewedBy
            };

            return new Response<UserReportResponseDto>(ResponseCode.Success, reportResponse, _localizer["ReportCreated"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user report for: {ReportedValue}", request.ReportedValue);
            return new Response<UserReportResponseDto>(ResponseCode.Fail, _localizer["CreateReportError"]);
        }
    }

    private async Task ProcessReportAsync(UserReport report)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(report.ReportedValue))
            {
                report.Status = ReportStatus.Rejected;
                report.ReviewedBy = "System";
                report.ReviewedDate = DateTime.UtcNow;
                await _userReportDal.SaveChangesAsync();
                return;
            }

            if (report.ReportType == ReportType.SuspiciousDomain)
            {
                if (IsValidDomain(report.ReportedValue))
                {
                    var result = await _suspiciousDomainService.AddSuspiciousDomainAsync(
                        report.ReportedValue,
                        _localizer["ReportedByUser", report.ReporterName]);

                    if (result.ResponseCode == ResponseCode.Success)
                    {
                        report.Status = ReportStatus.Approved;

                        var domain = await _suspiciousDomainDal.GetByDomainNameAsync(report.ReportedValue);
                        if (domain != null)
                        {
                            report.SuspiciousDomainId = domain.Id;
                        }
                    }
                }
                else
                {
                    report.Status = ReportStatus.Rejected;
                }
            }
            else if (report.ReportType == ReportType.SuspiciousEmail)
            {
                if (IsValidEmail(report.ReportedValue))
                {
                    var result = await _suspiciousEmailService.AddSuspiciousEmailAsync(
                        report.ReportedValue,
                        _localizer["ReportedByUser", report.ReporterName]);

                    if (result.ResponseCode == ResponseCode.Success)
                    {
                        report.Status = ReportStatus.Approved;

                        var email = await _suspiciousEmailDal.GetByEmailAddressAsync(report.ReportedValue);
                        if (email != null)
                        {
                            report.SuspiciousEmailId = email.Id;
                        }
                    }
                }
                else
                {
                    report.Status = ReportStatus.Rejected;
                }
            }

            report.ReviewedBy = "System";
            report.ReviewedDate = DateTime.UtcNow;
            await _userReportDal.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing report: {ReportId}", report.Id);
        }
    }

    public async Task<Response<PaginatedResponseDto<UserReportResponseDto>>> GetReportsByStatusAsync(int status, PaginationRequestDto request)
    {
        try
        {
            var reportStatus = (ReportStatus)status;
            var reports = await _userReportDal.GetReportsByStatusAsync(reportStatus);

            var reportResponses = reports.Select(r => new UserReportResponseDto
            {
                Id = r.Id,
                ReporterName = r.ReporterName,
                ReporterEmail = r.ReporterEmail,
                ReportType = r.ReportType,
                ReportedValue = r.ReportedValue,
                Description = r.Description,
                Status = r.Status,
                CreatedDate = r.CreatedDate,
                ReviewedDate = r.ReviewedDate,
                ReviewedBy = r.ReviewedBy
            }).ToList();

            var totalCount = reportResponses.Count;
            var pagedReports = reportResponses
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var paginatedResponse = new PaginatedResponseDto<UserReportResponseDto>
            {
                Data = pagedReports,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            };

            return new Response<PaginatedResponseDto<UserReportResponseDto>>(ResponseCode.Success, paginatedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reports by status: {Status}", status);
            return new Response<PaginatedResponseDto<UserReportResponseDto>>(ResponseCode.Fail, _localizer["GetReportsError"]);
        }
    }

    public async Task<Response<List<UserReportResponseDto>>> GetRecentReportsAsync(int count = 50)
    {
        try
        {
            var reports = await _userReportDal.GetRecentReportsAsync(count);

            var reportResponses = reports.Select(r => new UserReportResponseDto
            {
                Id = r.Id,
                ReporterName = r.ReporterName,
                ReporterEmail = r.ReporterEmail,
                ReportType = r.ReportType,
                ReportedValue = r.ReportedValue,
                Description = r.Description,
                Status = r.Status,
                CreatedDate = r.CreatedDate,
                ReviewedDate = r.ReviewedDate,
                ReviewedBy = r.ReviewedBy
            }).ToList();

            return new Response<List<UserReportResponseDto>>(ResponseCode.Success, reportResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent reports");
            return new Response<List<UserReportResponseDto>>(ResponseCode.Fail, _localizer["GetRecentReportsError"]);
        }
    }

    public async Task<Response> UpdateReportStatusAsync(UpdateReportStatusRequestDto request)
    {
        try
        {
            var success = await _userReportDal.UpdateReportStatusAsync(
                request.ReportId,
                request.Status,
                request.ReviewedBy);

            if (!success)
            {
                return new Response(ResponseCode.NotFound, _localizer["ReportNotFound"]);
            }

            return new Response(ResponseCode.Success, _localizer["ReportStatusUpdated"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report status: {ReportId}", request.ReportId);
            return new Response(ResponseCode.Fail, _localizer["UpdateReportStatusError"]);
        }
    }

    public async Task<Response<DashboardStatsResponseDto>> GetDashboardStatsAsync()
    {
        try
        {
            var totalDomains = await _suspiciousDomainDal.GetCountAsync();
            var totalEmails = await _suspiciousEmailDal.GetCountAsync();
            var totalReports = await _userReportDal.GetCountAsync();
            var pendingReports = await _userReportDal.GetPendingReportCountAsync();
            var todayReports = await _userReportDal.GetTodayReportCountAsync();

            var bloomFilterStats = new List<BloomFilterStatsResponseDto>();
            var allFilters = await _bloomFilterDataDal.GetAllActiveFiltersAsync();

            foreach (var filter in allFilters)
            {
                var loadFactor = filter.ElementCount > 0 ? (double)filter.ElementCount / filter.FilterSize : 0;

                bloomFilterStats.Add(new BloomFilterStatsResponseDto
                {
                    FilterName = filter.FilterName,
                    ElementCount = filter.ElementCount,
                    FilterSize = filter.FilterSize,
                    CurrentLoadFactor = loadFactor
                });
            }
            var dashboardStats = new DashboardStatsResponseDto
            {
                TotalSuspiciousDomains = totalDomains,
                TotalSuspiciousEmails = totalEmails,
                TotalUserReports = totalReports,
                PendingReports = pendingReports,
                TodayReports = todayReports,
                BloomFilterStats = bloomFilterStats
            };
            return new Response<DashboardStatsResponseDto>(ResponseCode.Success, dashboardStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return new Response<DashboardStatsResponseDto>(ResponseCode.Fail, _localizer["GetDashboardStatsError"]);
        }
    }

    private bool IsValidDomain(string domain)
    {
        try
        {
            var parts = domain.Split('.');
            if (parts.Length < 2) return false;
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part) || part.Length > 63 || !Regex.IsMatch(part, @"^[a-zA-Z0-9-]+$") || part.StartsWith("-") || part.EndsWith("-"))
                {
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}