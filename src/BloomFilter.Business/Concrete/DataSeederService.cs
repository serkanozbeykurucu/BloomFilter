using BloomFilter.Business.Abstract;
using BloomFilter.DataAccess.Abstract;
using BloomFilter.Entity.Concrete;
using BloomFilter.Shared.Responses.ComplexTypes;
using BloomFilter.Shared.Responses.Concrete;
using Microsoft.Extensions.Logging;

namespace BloomFilter.Business.Concrete;

public class DataSeederService : IDataSeederService
{
    private readonly ISuspiciousDomainDal _suspiciousDomainDal;
    private readonly ISuspiciousEmailDal _suspiciousEmailDal;
    private readonly IBloomFilterService _bloomFilterService;
    private readonly ILogger<DataSeederService> _logger;
    private readonly string _domainsFilePath;
    private readonly string _emailsFilePath;

    public DataSeederService(
        ISuspiciousDomainDal suspiciousDomainDal,
        ISuspiciousEmailDal suspiciousEmailDal,
        IBloomFilterService bloomFilterService,
        ILogger<DataSeederService> logger)
    {
        _suspiciousDomainDal = suspiciousDomainDal;
        _suspiciousEmailDal = suspiciousEmailDal;
        _bloomFilterService = bloomFilterService;
        _logger = logger;

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _domainsFilePath = Path.Combine(baseDirectory, "SeedData", "suspicious.txt");
        _emailsFilePath = Path.Combine(baseDirectory, "SeedData", "emails.txt");
    }

    public async Task<Response> SeedDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding process...");

            var domainCount = await _suspiciousDomainDal.GetCountAsync();
            var emailCount = await _suspiciousEmailDal.GetCountAsync();

            if (domainCount > 0 || emailCount > 0)
            {
                _logger.LogInformation("Data already seeded. Skipping seeding process. Domains: {DomainCount}, Emails: {EmailCount}", domainCount, emailCount);
                return new Response(ResponseCode.Success, "Data already seeded");
            }

            await SeedDomainsAsync();

            await SeedEmailsAsync();

            _logger.LogInformation("Data seeding completed successfully");
            return new Response(ResponseCode.Success, "Data seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data seeding");
            return new Response(ResponseCode.Fail, "An error occurred during data seeding");
        }
    }

    private async Task SeedDomainsAsync()
    {
        try
        {
            if (!File.Exists(_domainsFilePath))
            {
                _logger.LogWarning("Domains file not found at: {FilePath}", _domainsFilePath);
                return;
            }

            _logger.LogInformation("Reading domains from file: {FilePath}", _domainsFilePath);

            var domains = await File.ReadAllLinesAsync(_domainsFilePath);
            var domainsToAdd = new List<SuspiciousDomain>();
            var processedCount = 0;

            foreach (var line in domains)
            {
                var domain = line.Trim().ToLower();

                if (string.IsNullOrWhiteSpace(domain))
                    continue;

                var existingDomain = await _suspiciousDomainDal.GetByDomainNameAsync(domain);
                if (existingDomain == null)
                {
                    domainsToAdd.Add(new SuspiciousDomain
                    {
                        DomainName = domain,
                        Description = "Seeded from suspicious.txt",
                        ReportCount = 1,
                        LastReportedDate = DateTime.UtcNow,
                        IsActive = true
                    });

                    processedCount++;

                    if (domainsToAdd.Count >= 100)
                    {
                        await _suspiciousDomainDal.AddRangeAsync(domainsToAdd);
                        await _suspiciousDomainDal.SaveChangesAsync();

                        foreach (var d in domainsToAdd)
                        {
                            await _bloomFilterService.AddDomainToFilterAsync(d.DomainName);
                        }

                        _logger.LogInformation("Seeded {Count} domains", domainsToAdd.Count);
                        domainsToAdd.Clear();
                    }
                }
            }

            if (domainsToAdd.Count > 0)
            {
                await _suspiciousDomainDal.AddRangeAsync(domainsToAdd);
                await _suspiciousDomainDal.SaveChangesAsync();

                foreach (var d in domainsToAdd)
                {
                    await _bloomFilterService.AddDomainToFilterAsync(d.DomainName);
                }

                _logger.LogInformation("Seeded {Count} domains", domainsToAdd.Count);
            }

            _logger.LogInformation("Total domains seeded: {Count}", processedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding domains");
            throw;
        }
    }

    private async Task SeedEmailsAsync()
    {
        try
        {
            if (!File.Exists(_emailsFilePath))
            {
                _logger.LogWarning("Emails file not found at: {FilePath}", _emailsFilePath);
                return;
            }

            _logger.LogInformation("Reading emails from file: {FilePath}", _emailsFilePath);

            var emails = await File.ReadAllLinesAsync(_emailsFilePath);
            var emailsToAdd = new List<SuspiciousEmail>();
            var processedCount = 0;

            foreach (var line in emails)
            {
                var email = line.Trim().ToLower();

                if (string.IsNullOrWhiteSpace(email))
                    continue;

                var existingEmail = await _suspiciousEmailDal.GetByEmailAddressAsync(email);
                if (existingEmail == null)
                {
                    emailsToAdd.Add(new SuspiciousEmail
                    {
                        EmailAddress = email,
                        Description = "Seeded from emails.txt",
                        ReportCount = 1,
                        LastReportedDate = DateTime.UtcNow,
                        IsActive = true
                    });

                    processedCount++;

                    if (emailsToAdd.Count >= 100)
                    {
                        await _suspiciousEmailDal.AddRangeAsync(emailsToAdd);
                        await _suspiciousEmailDal.SaveChangesAsync();

                        foreach (var e in emailsToAdd)
                        {
                            await _bloomFilterService.AddEmailToFilterAsync(e.EmailAddress);
                        }

                        _logger.LogInformation("Seeded {Count} emails", emailsToAdd.Count);
                        emailsToAdd.Clear();
                    }
                }
            }

            if (emailsToAdd.Count > 0)
            {
                await _suspiciousEmailDal.AddRangeAsync(emailsToAdd);
                await _suspiciousEmailDal.SaveChangesAsync();

                foreach (var e in emailsToAdd)
                {
                    await _bloomFilterService.AddEmailToFilterAsync(e.EmailAddress);
                }

                _logger.LogInformation("Seeded {Count} emails", emailsToAdd.Count);
            }

            _logger.LogInformation("Total emails seeded: {Count}", processedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding emails");
            throw;
        }
    }
}