using System.Diagnostics;
using BloomFilter.Business.Concrete;
using BloomFilter.DataAccess.Concrete.Context;
using BloomFilter.DataAccess.Concrete.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace BloomFilter.Tests;

public class PerformanceBenchmarkTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public async Task ComprehensivePerformanceBenchmark()
    {
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

        var domains = await domainDal.GetAllAsync();
        var domainList = domains.Select(d => d.DomainName).ToList();
        var emails = await emailDal.GetAllAsync();
        var emailList = emails.Select(e => e.EmailAddress).ToList();

        var testData = domainList.Concat(emailList).ToList();
        var falsePositiveTestData = GenerateFalsePositiveTestData(10000);

        var bloomFilter = new BloomFilterImplementation(
            size: 100000,
            hashFunctionCount: 7
        );

        var hashSet = new HashSet<string>();

        var bfWriteStopwatch = Stopwatch.StartNew();
        foreach (var item in testData)
        {
            bloomFilter.Add(item);
        }
        bfWriteStopwatch.Stop();
        var bloomFilterWriteTime = bfWriteStopwatch.ElapsedMilliseconds;

        var hsWriteStopwatch = Stopwatch.StartNew();
        foreach (var item in testData)
        {
            hashSet.Add(item);
        }
        hsWriteStopwatch.Stop();
        var hashSetWriteTime = hsWriteStopwatch.ElapsedMilliseconds;

        var bfReadStopwatch = Stopwatch.StartNew();
        var bfFoundCount = 0;
        foreach (var item in testData)
        {
            if (bloomFilter.Contains(item)) bfFoundCount++;
        }
        bfReadStopwatch.Stop();
        var bloomFilterReadTime = bfReadStopwatch.ElapsedMilliseconds;

        var hsReadStopwatch = Stopwatch.StartNew();
        var hsFoundCount = 0;
        foreach (var item in testData)
        {
            if (hashSet.Contains(item)) hsFoundCount++;
        }
        hsReadStopwatch.Stop();
        var hashSetReadTime = hsReadStopwatch.ElapsedMilliseconds;

        var bloomFilterMemoryBytes = bloomFilter.size / 8;
        var bloomFilterMemoryKB = bloomFilterMemoryBytes / 1024.0;

        var hashSetMemoryBytes = testData.Sum(s => 8 + 24 + (s.Length * 2));
        var hashSetMemoryKB = hashSetMemoryBytes / 1024.0;

        var bfFalsePositives = 0;
        foreach (var item in falsePositiveTestData)
        {
            if (bloomFilter.Contains(item)) bfFalsePositives++;
        }

        var hsFalsePositives = 0;
        foreach (var item in falsePositiveTestData)
        {
            if (hashSet.Contains(item)) hsFalsePositives++;
        }

        var bloomFilterAccuracy = (1 - (double)bfFalsePositives / falsePositiveTestData.Count) * 100;
        var hashSetAccuracy = (1 - (double)hsFalsePositives / falsePositiveTestData.Count) * 100;

        _output.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
        _output.WriteLine("║                    PERFORMANCE COMPARISON                         ║");
        _output.WriteLine("╠═══════════════════════════════════════════════════════════════════╣");
        _output.WriteLine($"║ Test Data           │ {domainList.Count} domains + {emailList.Count} emails = {testData.Count} total items ║");
        _output.WriteLine("╠═══════════════════════════════════════════════════════════════════╣");
        _output.WriteLine("║ Metric              │ Bloom Filter    │ HashSet                   ║");
        _output.WriteLine("╟─────────────────────┼─────────────────┼───────────────────────────╢");
        _output.WriteLine($"║ Write Speed         │ {bloomFilterWriteTime,7} ms      │ {hashSetWriteTime,7} ms            ║");
        _output.WriteLine("╟─────────────────────┼─────────────────┼───────────────────────────╢");
        _output.WriteLine($"║ Read Speed          │ {bloomFilterReadTime,7} ms      │ {hashSetReadTime,7} ms            ║");
        _output.WriteLine("╟─────────────────────┼─────────────────┼───────────────────────────╢");
        _output.WriteLine($"║ Memory Usage        │ {bloomFilterMemoryKB,8:F2} KB     │ {hashSetMemoryKB,8:F2} KB         ║");
        _output.WriteLine("╟─────────────────────┼─────────────────┼───────────────────────────╢");
        _output.WriteLine($"║ Accuracy            │ {bloomFilterAccuracy,7:F2}%      │ {hashSetAccuracy,7:F2}%            ║");
        _output.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");

        Assert.True(bloomFilterWriteTime >= 0);
        Assert.True(hashSetWriteTime >= 0);
        Assert.Equal(testData.Count, bfFoundCount);
        Assert.Equal(testData.Count, hsFoundCount);
        Assert.True(bloomFilterAccuracy > 95);
        Assert.Equal(100, hashSetAccuracy);
        Assert.True(bloomFilterMemoryKB < hashSetMemoryKB);
    }

    private static List<string> GenerateFalsePositiveTestData(int count)
    {
        var data = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            data.Add($"fp-test-{i}-{Guid.NewGuid()}.com");
        }
        return data;
    }
}
