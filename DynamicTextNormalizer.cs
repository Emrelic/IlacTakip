using System.Text.RegularExpressions;

namespace MedulaOtomasyon;

/// <summary>
/// Dinamik (hasta adı, tarih, sayısal değer vb.) içeren metinleri normalize etmek ve karşılaştırmak için yardımcı sınıf.
/// </summary>
public static class DynamicTextNormalizer
{
    private static readonly Regex DateRegex = new(@"(\d{2}[./-]\d{2}[./-]\d{2,4})|(\d{4}[./-]\d{2}[./-]\d{2})", RegexOptions.Compiled);
    private static readonly Regex LongNumberRegex = new(@"\b\d{4,}\b", RegexOptions.Compiled);
    private static readonly Regex NumberRegex = new(@"\b\d+\b", RegexOptions.Compiled);
    private static readonly Regex PersonNameRegex = new(@"\b[A-ZÇĞİÖŞÜ][a-zçğıöşü']+(?:\s+[A-ZÇĞİÖŞÜ][a-zçğıöşü']+)+\b", RegexOptions.Compiled);
    private static readonly Regex GuidRegex = new(@"\b[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// Dinamik kısımları {NAME}, {NUM}, {DATE} gibi yer tutuculara çevirir.
    /// </summary>
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value?.Trim();
        }

        var normalized = value.Trim();

        normalized = GuidRegex.Replace(normalized, "{GUID}");
        normalized = DateRegex.Replace(normalized, "{DATE}");
        normalized = LongNumberRegex.Replace(normalized, "{NUM}");
        normalized = NumberRegex.Replace(normalized, "{NUM}");
        normalized = PersonNameRegex.Replace(normalized, "{NAME}");

        normalized = WhitespaceRegex.Replace(normalized, " ").Trim();

        return normalized;
    }

    /// <summary>
    /// Metnin yüksek olasılıkla dinamik (hasta adı, sayı, tarih) içerdiğini tespit eder.
    /// </summary>
    public static bool IsLikelyDynamic(string? rawValue, string? normalizedValue = null)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        normalizedValue ??= Normalize(rawValue);
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            return false;
        }

        if (normalizedValue.Contains("{GUID}", StringComparison.Ordinal) ||
            normalizedValue.Contains("{NAME}", StringComparison.Ordinal) ||
            normalizedValue.Contains("{NUM}", StringComparison.Ordinal) ||
            normalizedValue.Contains("{DATE}", StringComparison.Ordinal))
        {
            return true;
        }

        // Ham değerde tarih veya büyük sayı olması
        if (DateRegex.IsMatch(rawValue) || LongNumberRegex.IsMatch(rawValue))
        {
            return true;
        }

        // Tamamen büyük harf + boşluk kombinasyonlu isimler (hasta adı vb.)
        if (PersonNameRegex.IsMatch(rawValue))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// İki metni normalize ederek karşılaştırır.
    /// </summary>
    public static bool AreEquivalent(string? value1, string? value2)
    {
        var norm1 = Normalize(value1);
        var norm2 = Normalize(value2);

        if (norm1 == null && norm2 == null) return true;
        if (norm1 == null || norm2 == null) return false;

        return string.Equals(norm1, norm2, StringComparison.OrdinalIgnoreCase);
    }
}
