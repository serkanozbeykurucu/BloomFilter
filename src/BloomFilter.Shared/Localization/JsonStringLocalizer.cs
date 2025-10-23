using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace BloomFilter.Shared.Localization;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly ConcurrentDictionary<string, string> _localizations;

    public JsonStringLocalizer(string json)
    {
        _localizations = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(json) ?? new ConcurrentDictionary<string, string>();
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = _localizations.GetValueOrDefault(name);
            return new LocalizedString(name, value ?? name, value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var localizedString = this[name];
            if (!localizedString.ResourceNotFound)
            {
                return new LocalizedString(name, string.Format(localizedString.Value, arguments), false);
            }
            return localizedString;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return _localizations.Select(l => new LocalizedString(l.Key, l.Value, false));
    }
}
