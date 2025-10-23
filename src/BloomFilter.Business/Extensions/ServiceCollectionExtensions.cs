using BloomFilter.Business.Abstract;
using BloomFilter.Business.Concrete;
using BloomFilter.DataAccess.Abstract;
using BloomFilter.DataAccess.Concrete.Context;
using BloomFilter.DataAccess.Concrete.EntityFrameworkCore;
using BloomFilter.Shared.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;


namespace BloomFilter.Business.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBloomFilterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BloomFilterDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISuspiciousDomainDal, EfSuspiciousDomainDal>();
        services.AddScoped<ISuspiciousEmailDal, EfSuspiciousEmailDal>();
        services.AddScoped<IUserReportDal, EfUserReportDal>();
        services.AddScoped<IBloomFilterDataDal, EfBloomFilterDataDal>();

        services.AddScoped<IBloomFilterService, BloomFilterService>();
        services.AddScoped<ISuspiciousDomainService, SuspiciousDomainService>();
        services.AddScoped<ISuspiciousEmailService, SuspiciousEmailService>();
        services.AddScoped<IUserReportService, UserReportService>();
        services.AddScoped<IDataSeederService, DataSeederService>();

        return services;
    }

    public static IServiceCollection AddBloomFilterCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("BloomFilterPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }

    public static IServiceCollection AddCustomJsonLocalization(this IServiceCollection services)
    {
        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        services.AddLocalization();
        return services;
    }
}
