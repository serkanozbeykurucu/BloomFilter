using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace BloomFilter.Shared.Localization;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly ConcurrentDictionary<string, JsonStringLocalizer> _localizers = new();
    private readonly string _resourcesPath;

    public JsonStringLocalizerFactory()
    {
        var assembly = typeof(Resources.SharedResource).Assembly;
        var assemblyPath = Path.GetDirectoryName(assembly.Location) ?? throw new InvalidOperationException("Could not get assembly directory");
        _resourcesPath = Path.Combine(assemblyPath, "Resources");
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var culture = Thread.CurrentThread.CurrentUICulture;
        var cultureName = culture.Name;
        var resourceName = resourceSource.Name;

        var cacheKey = $"{resourceName}_{cultureName}";

        return _localizers.GetOrAdd(cacheKey, _ =>
        {
            var jsonFilePath = Path.Combine(_resourcesPath, $"{resourceName}.{cultureName}.json");

            if (!File.Exists(jsonFilePath) && culture.Parent != CultureInfo.InvariantCulture)
            {
                var parentCultureName = culture.Parent.Name;
                jsonFilePath = Path.Combine(_resourcesPath, $"{resourceName}.{parentCultureName}.json");
            }

            if (!File.Exists(jsonFilePath))
            {
                jsonFilePath = Path.Combine(_resourcesPath, $"{resourceName}.en.json");
            }
            
            if (!File.Exists(jsonFilePath))
            {
                 jsonFilePath = Path.Combine(_resourcesPath, $"{resourceName}.json");
            }

            if (File.Exists(jsonFilePath))
            {
                var json = File.ReadAllText(jsonFilePath);
                return new JsonStringLocalizer(json);
            }

            return new JsonStringLocalizer("{}");
        });
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        throw new NotImplementedException();
    }
}
