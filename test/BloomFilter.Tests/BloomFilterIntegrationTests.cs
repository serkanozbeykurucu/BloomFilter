using System.Diagnostics;
using BloomFilter.Business.Concrete;
using BloomFilter.DataAccess.Abstract;
using BloomFilter.DataAccess.Concrete.Context;
using BloomFilter.DataAccess.Concrete.EntityFrameworkCore;
using BloomFilter.Shared.Localization;
using BloomFilter.Shared.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace BloomFilter.Tests;

public class BloomFilterIntegrationTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    private static JsonStringLocalizer LoadLocalizer(string culture)
    {
        var resourcePath = GetResourcePath(culture);
        if (!File.Exists(resourcePath))
        {
            resourcePath = GetResourcePath("en");
        }
        return new JsonStringLocalizer(File.ReadAllText(resourcePath));
    }

    private static string GetResourcePath(string culture) =>
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "BloomFilter.Shared", "Resources", $"SharedResource.{culture}.json");

    private async Task RunTestAsync(string culture, Func<BloomFilterService, IStringLocalizer, ISuspiciousDomainDal, ISuspiciousEmailDal, Task> testAction)
    {
        var localizer = LoadLocalizer(culture);
        _output.WriteLine($"--- {localizer["Test_RunningForLanguage", culture].Value} ---");

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        var options = new DbContextOptionsBuilder<BloomFilterDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var dbContext = new BloomFilterDbContext(options);
        var domainDal = new EfSuspiciousDomainDal(dbContext);
        var emailDal = new EfSuspiciousEmailDal(dbContext);
        var bloomFilterDal = new EfBloomFilterDataDal(dbContext);

        var service = new BloomFilterService(
            bloomFilterDal,
            domainDal,
            emailDal,
            new Mock<ILogger<BloomFilterService>>().Object,
            new Mock<IStringLocalizer<SharedResource>>().Object);

        await service.InitializeFiltersAsync();

        await testAction(service, localizer, domainDal, emailDal);

        _output.WriteLine($"--- {localizer["Test_CompletedForLanguage", culture].Value} ---\n");
    }

    [Theory]
    [InlineData("en")]
    [InlineData("tr")]
    [InlineData("pl")]
    public async Task ComprehensiveReportTest(string culture)
    {
        await RunTestAsync(culture, async (service, loc, domainDal, emailDal) =>
        {
            _output.WriteLine(loc["Test_ComprehensiveReport"].Value);

            var domains = await domainDal.GetAllAsync();
            var domainList = domains.Select(d => d.DomainName).ToList();
            var emails = await emailDal.GetAllAsync();
            var emailList = emails.Select(e => e.EmailAddress).ToList();

            _output.WriteLine($"\n{loc["Test_DataDescription"].Value}");
            _output.WriteLine($"{loc["Test_DomainCount", domainList.Count].Value}");
            _output.WriteLine($"{loc["Test_EmailCount", emailList.Count].Value}");

            _output.WriteLine($"\n{loc["Test_BloomFilterPerformance"].Value}");

            var swBfAdd = Stopwatch.StartNew();
            await service.AddBulkDomainsToFilterAsync(domainList);
            
            swBfAdd.Stop();
            
            _output.WriteLine($"{loc["Test_DomainAddPerformance"].Value}: {loc["Test_TimeMs", swBfAdd.ElapsedMilliseconds].Value}");

            await service.AddBulkEmailsToFilterAsync(emailList);
            
            var swBfCheck = Stopwatch.StartNew();
            var found = 0;
            
            foreach (var email in emailList)
            {
                if ((await service.CheckEmailInFilterAsync(email)).Data) found++;
            }
            
            swBfCheck.Stop();

            _output.WriteLine($"{loc["Test_EmailCheckPerformance"].Value}: {loc["Test_TimeMs", swBfCheck.ElapsedMilliseconds].Value}");

            var testDomains = new List<string>();
           
            for (int i = 0; i < 10000; i++)
                testDomains.Add($"legitimate-domain-{i}-{Guid.NewGuid()}.com");

            var fp = 0;

            foreach (var domain in testDomains)
            {
                if ((await service.CheckDomainInFilterAsync(domain)).Data) fp++;
            }

            _output.WriteLine($"{loc["Test_FalsePositives", fp, testDomains.Count].Value}");

            _output.WriteLine($"\n{loc["Test_HashSetComparison"].Value}");

            var hashSet = new HashSet<string>();
            var swHsAdd = Stopwatch.StartNew();

            foreach (var domain in domainList)
            {
                hashSet.Add(domain);
            }

            swHsAdd.Stop();

            _output.WriteLine($"{loc["Test_AddPerformance"].Value}");
            _output.WriteLine($"{loc["Test_BloomFilterTime", swBfAdd.ElapsedMilliseconds].Value}");
            _output.WriteLine($"{loc["Test_HashSetTime", swHsAdd.ElapsedMilliseconds].Value}");

            var swHsCheck = Stopwatch.StartNew();
            var hsFound = 0;

            foreach (var domain in testDomains)
            {
                if (hashSet.Contains(domain)) hsFound++;
            }

            swHsCheck.Stop();

            _output.WriteLine($"{loc["Test_CheckPerformance"].Value}");
            _output.WriteLine($"{loc["Test_BloomFilterTime", "N/A (Email check)"]}");
            _output.WriteLine($"{loc["Test_HashSetTime", swHsCheck.ElapsedMilliseconds].Value}");

            var domainStats = await service.GetFilterStatsAsync("DomainFilter");
            long bfMemoryBytes = (domainStats.Data?.FilterSize ?? 0) / 8;
            long hsMemoryBytes = domainList.Sum(d => d.Length * 2 + 24);

            _output.WriteLine($"{loc["Test_MemoryUsage"].Value}");
            _output.WriteLine($"{loc["Test_BloomFilterMemory", bfMemoryBytes / 1024.0].Value}");
            _output.WriteLine($"{loc["Test_HashSetMemory", hsMemoryBytes / 1024.0].Value}");

            if(bfMemoryBytes > 0) _output.WriteLine($"{loc["Test_MemorySavings", (double)hsMemoryBytes / bfMemoryBytes].Value}");

            _output.WriteLine($"{loc["Test_Accuracy"].Value}");
            _output.WriteLine($"{loc["Test_BloomFilterAccuracy", (1 - (double)fp / testDomains.Count) * 100].Value}");
            _output.WriteLine($"{loc["Test_HashSetAccuracy", 100.0].Value}");
        });
    }
}
