using BloomFilter.Business.Abstract;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BloomFilter.Business.Extensions;
using BloomFilter.DataAccess.Concrete.Context;
using BloomFilter.HttpApi.Middlewares;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddCustomJsonLocalization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "BloomFilter API",
        Version = "v1",
        Description = "API for managing suspicious emails and domains using Bloom Filter"
    });
});

builder.Services.AddBloomFilterServices(builder.Configuration);
builder.Services.AddBloomFilterCors();
builder.Services.AddHealthChecks();

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("tr-TR"),
    new CultureInfo("pl-PL"),
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("BloomFilterPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<BloomFilterDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migration completed successfully");

        var bloomFilterService = scope.ServiceProvider.GetRequiredService<IBloomFilterService>();
        await bloomFilterService.InitializeFiltersAsync();
        Log.Information("Bloom filters initialized successfully");

        var dataSeederService = scope.ServiceProvider.GetRequiredService<IDataSeederService>();
        await dataSeederService.SeedDataAsync();
        Log.Information("Data seeding completed successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while starting the application");
        return;
    }
}

Log.Information("BloomFilter API started successfully");

app.Run();