using System.Windows.Automation;
using Microsoft.Playwright;

namespace MedulaOtomasyon;

/// <summary>
/// Playwright ile web elementlerinden özellik çıkarır
/// Modern web tarayıcıları (Chrome, Edge, Firefox) için destek sağlar
/// </summary>
public static class PlaywrightExtractor
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static bool _isInitialized = false;

    /// <summary>
    /// Playwright'i başlatır (lazy initialization)
    /// </summary>
    private static async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            _playwright = await Playwright.CreateAsync();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Playwright başlatma hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Web elementi ise Playwright ile ek özellikleri toplar
    /// </summary>
    public static async Task EnrichWithPlaywrightAsync(AutomationElement element, UIElementInfo info)
    {
        try
        {
            // Sadece web elementleri için (Chrome, Edge, Firefox)
            if (info.FrameworkId != "Chrome" &&
                info.FrameworkId != "Edge" &&
                info.FrameworkId != "Firefox")
            {
                return;
            }

            // Playwright'i başlat
            await InitializeAsync();

            // CDP ile mevcut tarayıcıya bağlanmayı dene
            // Not: Bu özellik gelişmiş bir kullanım gerektirir
            // Şimdilik basic selector generation yapıyoruz

            // Playwright Selector oluştur (info'dan mevcut bilgilerle)
            info.PlaywrightSelector = GeneratePlaywrightSelector(info);

            // Eğer element'in tüm özellikleri UI Automation'dan geldiyse
            // ve web teknolojisi kullanılıyorsa, ek web özellikleri ekle
            await EnrichWebPropertiesAsync(element, info);
        }
        catch
        {
            // Playwright hatası - sessizce devam et
        }
    }

    /// <summary>
    /// Web elementleri için ek özellikleri çıkarır
    /// </summary>
    private static async Task EnrichWebPropertiesAsync(AutomationElement element, UIElementInfo info)
    {
        await Task.Run(() =>
        {
            try
            {
                // ValuePattern ile value özelliği (eğer henüz yoksa)
                if (string.IsNullOrEmpty(info.Value) &&
                    element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern) &&
                    valuePattern is ValuePattern vp)
                {
                    info.Value = vp.Current.Value;
                }

                // RangeValuePattern ile range bilgileri
                if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out object? rangePattern) &&
                    rangePattern is RangeValuePattern rvp)
                {
                    info.OtherAttributes ??= new Dictionary<string, string>();
                    info.OtherAttributes["min"] = rvp.Current.Minimum.ToString();
                    info.OtherAttributes["max"] = rvp.Current.Maximum.ToString();
                    info.OtherAttributes["step"] = rvp.Current.SmallChange.ToString();
                }

                // TogglePattern ile checkbox/radio durumu
                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out object? togglePattern) &&
                    togglePattern is TogglePattern tp)
                {
                    info.AriaChecked = tp.Current.ToggleState.ToString();
                }

                // ExpandCollapsePattern ile açılır/kapanır durumu
                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandPattern) &&
                    expandPattern is ExpandCollapsePattern ecp)
                {
                    info.AriaExpanded = (ecp.Current.ExpandCollapseState == ExpandCollapseState.Expanded).ToString().ToLower();
                }
            }
            catch { }
        });
    }

    /// <summary>
    /// Element için Playwright Selector oluşturur
    /// https://playwright.dev/docs/selectors
    /// </summary>
    private static string? GeneratePlaywrightSelector(UIElementInfo info)
    {
        var selectors = new List<string>();

        // 1. data-testid (en güvenilir)
        if (info.DataAttributes != null && info.DataAttributes.TryGetValue("data-testid", out string? testId))
        {
            return $"[data-testid='{testId}']";
        }

        // 2. ID (en spesifik)
        if (!string.IsNullOrEmpty(info.HtmlId))
        {
            return $"#{info.HtmlId}";
        }

        // 3. Text-based selector (button, link için ideal)
        if (!string.IsNullOrEmpty(info.InnerText) && info.InnerText.Length < 50)
        {
            var tag = info.Tag?.ToLower() ?? "";
            switch (tag)
            {
                case "button":
                    return $"button:has-text('{EscapeSelector(info.InnerText)}')";
                case "a":
                    return $"a:has-text('{EscapeSelector(info.InnerText)}')";
                case "input" when info.Type == "submit":
                    return $"input[type='submit'][value='{EscapeSelector(info.InnerText)}']";
            }
        }

        // 4. Role-based selector (ARIA)
        if (!string.IsNullOrEmpty(info.AriaRole))
        {
            var roleSelector = $"[role='{info.AriaRole}']";
            if (!string.IsNullOrEmpty(info.AriaLabel))
            {
                roleSelector += $"[aria-label='{EscapeSelector(info.AriaLabel)}']";
            }
            selectors.Add(roleSelector);
        }

        // 5. Name attribute (form elementleri için)
        if (!string.IsNullOrEmpty(info.HtmlName))
        {
            selectors.Add($"[name='{info.HtmlName}']");
        }

        // 6. Placeholder (input elementleri için)
        if (!string.IsNullOrEmpty(info.Placeholder))
        {
            selectors.Add($"[placeholder='{EscapeSelector(info.Placeholder)}']");
        }

        // 7. CSS Class kombinasyonu
        if (!string.IsNullOrEmpty(info.ClassName))
        {
            var classes = info.ClassName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (classes.Length > 0)
            {
                var tag = !string.IsNullOrEmpty(info.Tag) ? info.Tag.ToLower() : "";
                var classSelector = tag + "." + string.Join(".", classes);
                selectors.Add(classSelector);
            }
        }

        // 8. Type attribute (input elementleri için)
        if (!string.IsNullOrEmpty(info.Tag) && !string.IsNullOrEmpty(info.Type))
        {
            selectors.Add($"{info.Tag.ToLower()}[type='{info.Type}']");
        }

        // En iyi selector'ı döndür
        return selectors.Count > 0 ? selectors[0] : null;
    }

    /// <summary>
    /// Selector string'ini escape eder (tek tırnak için)
    /// </summary>
    private static string EscapeSelector(string text)
    {
        return text.Replace("'", "\\'").Replace("\n", " ").Trim();
    }

    /// <summary>
    /// Playwright'in gelişmiş selector'larını oluşturur (kombinasyon)
    /// </summary>
    public static string? GenerateAdvancedPlaywrightSelector(UIElementInfo info)
    {
        // Kombinasyon selector'ları
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(info.Tag))
        {
            parts.Add(info.Tag.ToLower());
        }

        // Attribute selectors
        if (!string.IsNullOrEmpty(info.HtmlId))
        {
            parts.Add($"#{info.HtmlId}");
        }
        else if (!string.IsNullOrEmpty(info.HtmlName))
        {
            parts.Add($"[name='{info.HtmlName}']");
        }

        // Text selector (kısa metinler için)
        if (!string.IsNullOrEmpty(info.InnerText) && info.InnerText.Length < 30)
        {
            return $"{string.Join("", parts)}:has-text('{EscapeSelector(info.InnerText)}')";
        }

        return parts.Count > 0 ? string.Join("", parts) : null;
    }

    /// <summary>
    /// CDP (Chrome DevTools Protocol) kullanarak mevcut tarayıcıya bağlan
    /// Not: Gelişmiş kullanım - Chrome'un --remote-debugging-port ile başlatılması gerekir
    /// </summary>
    public static async Task<IBrowser?> ConnectToBrowserAsync(string cdpEndpoint = "http://localhost:9222")
    {
        try
        {
            await InitializeAsync();
            if (_playwright == null) return null;

            _browser = await _playwright.Chromium.ConnectOverCDPAsync(cdpEndpoint);
            return _browser;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Kaynakları temizle
    /// </summary>
    public static async Task DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        if (_playwright != null)
        {
            _playwright.Dispose();
            _playwright = null;
        }

        _isInitialized = false;
    }
}
