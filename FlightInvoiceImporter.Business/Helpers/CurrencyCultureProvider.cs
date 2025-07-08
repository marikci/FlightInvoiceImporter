using System.Globalization;

namespace FlightInvoiceImporter.Business.Helpers;

public static class CurrencyCultureProvider
{
    private static readonly IReadOnlyDictionary<string, string> Map =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TRY"] = "tr-TR",
            ["EUR"] = "de-DE",
            ["USD"] = "en-US"
        };

    public static CultureInfo GetCulture(string? currencyCode)
    {
        if (currencyCode != null && Map.TryGetValue(currencyCode.ToUpperInvariant(), out var cultureName))
        {
            return new CultureInfo(cultureName);
        }
        return CultureInfo.InvariantCulture;
    }
}