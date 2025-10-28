using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace MedulaOtomasyon;

/// <summary>
/// Playwright kullanarak HTML tablo satırlarından ek seçim bilgileri üretir ve test eder.
/// </summary>
public static class PlaywrightRowAnalyzer
{
    static PlaywrightRowAnalyzer()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static readonly SemaphoreSlim _initLock = new(1, 1);
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;

    private static async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_browser != null) return;

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_browser != null) return;

            _playwright = await Microsoft.Playwright.Playwright.CreateAsync().ConfigureAwait(false);
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            }).ConfigureAwait(false);
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static async Task<string> ReadHtmlContentAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("HTML kaynak dosyası bulunamadı", path);
        }

        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var utf8Content = Encoding.UTF8.GetString(bytes);

        if (utf8Content.Contains("charset=ISO-8859-9", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.GetEncoding("iso-8859-9").GetString(bytes);
        }

        return utf8Content;
    }

    public static async Task<PlaywrightSelectorInfo> AnalyzeAsync(RecordedElement recordedElement, string htmlFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recordedElement);
        ArgumentException.ThrowIfNullOrEmpty(htmlFilePath);

        try
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new PlaywrightSelectorInfo
            {
                ErrorMessage = $"Playwright başlatılamadı: {ex.Message}"
            };
        }

        IPage? page = null;
        try
        {
            var htmlContent = await ReadHtmlContentAsync(htmlFilePath, cancellationToken).ConfigureAwait(false);

            page = await _browser!.NewPageAsync().ConfigureAwait(false);
            await page.SetContentAsync(htmlContent).ConfigureAwait(false);

            return await ExtractRowInfoAsync(page, recordedElement, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new PlaywrightSelectorInfo
            {
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            if (page != null)
            {
                await page.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    public static async Task<bool> TestStrategyAsync(ElementLocatorStrategy strategy, string htmlFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentException.ThrowIfNullOrEmpty(htmlFilePath);

        try
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }

        IPage? page = null;
        try
        {
            var htmlContent = await ReadHtmlContentAsync(htmlFilePath, cancellationToken).ConfigureAwait(false);

            page = await _browser!.NewPageAsync().ConfigureAwait(false);
            await page.SetContentAsync(htmlContent).ConfigureAwait(false);

            return await TryExecuteLocatorAsync(page, strategy, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
        finally
        {
            if (page != null)
            {
                await page.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    private static async Task<PlaywrightSelectorInfo> ExtractRowInfoAsync(IPage page, RecordedElement recordedElement, CancellationToken cancellationToken)
    {
        IElementHandle? rowHandle = null;
        string? tableSelector = null;

        var tableId = recordedElement.TableInfo?.HtmlTableId ?? recordedElement.TableInfo?.TableId;
        var targetRowIndex = recordedElement.TableInfo?.HtmlRowIndex ?? recordedElement.TableInfo?.RowIndex ?? -1;

        if (!string.IsNullOrEmpty(tableId))
        {
            tableSelector = $"table#{EscapeCssIdentifier(tableId)}";
            rowHandle = await FindRowByIndexAsync(page, tableSelector, targetRowIndex, cancellationToken).ConfigureAwait(false);
        }

        if (rowHandle == null)
        {
            rowHandle = await FindRowByTextAsync(page, recordedElement, cancellationToken).ConfigureAwait(false);
            if (rowHandle != null)
            {
                tableSelector = await ComputeTableSelectorAsync(rowHandle).ConfigureAwait(false);
            }
        }

        if (rowHandle == null)
        {
            rowHandle = await page.QuerySelectorAsync("table tr").ConfigureAwait(false);
            tableSelector ??= "table";
        }

        if (rowHandle == null)
        {
            return new PlaywrightSelectorInfo
            {
                ErrorMessage = "Tablo satırı bulunamadı."
            };
        }

        return await BuildSelectorInfoAsync(rowHandle, tableSelector, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IElementHandle?> FindRowByIndexAsync(IPage page, string tableSelector, int rowIndex, CancellationToken cancellationToken)
    {
        if (rowIndex < 0) return null;

        var table = await page.QuerySelectorAsync(tableSelector).ConfigureAwait(false);
        if (table == null) return null;

        var rows = await table.QuerySelectorAllAsync("tr").ConfigureAwait(false);
        if (rowIndex < rows.Count)
        {
            return rows[rowIndex];
        }

        return null;
    }

    private static async Task<IElementHandle?> FindRowByTextAsync(IPage page, RecordedElement recordedElement, CancellationToken cancellationToken)
    {
        var textCandidates = recordedElement.HtmlInfo?.TextContent ??
                             recordedElement.TableInfo?.CellTexts ??
                             new List<string>();

        foreach (var candidate in textCandidates.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var rows = await page.QuerySelectorAllAsync("table tr").ConfigureAwait(false);
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var text = await row.InnerTextAsync().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(text) &&
                    text.Contains(candidate.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }
        }

        return null;
    }

    private static async Task<string?> ComputeTableSelectorAsync(IElementHandle rowHandle)
    {
        return await rowHandle.EvaluateAsync<string?>(@"(el) => {
            const table = el.closest('table');
            if (!table) return null;
            if (table.id) return `table#${table.id}`;
            const className = (table.className || '').trim();
            if (className.length > 0) {
                const parts = className.split(/\s+/).filter(Boolean);
                if (parts.length > 0) {
                    return 'table.' + parts.join('.');
                }
            }
            return 'table';
        }").ConfigureAwait(false);
    }

    private static async Task<PlaywrightSelectorInfo> BuildSelectorInfoAsync(IElementHandle rowHandle, string? tableSelector, CancellationToken cancellationToken)
    {
        var info = new PlaywrightSelectorInfo
        {
            TableSelector = tableSelector
        };

        info.DomId = await rowHandle.GetAttributeAsync("id").ConfigureAwait(false);
        info.DomClass = await rowHandle.GetAttributeAsync("class").ConfigureAwait(false);
        info.InnerText = (await rowHandle.InnerTextAsync().ConfigureAwait(false))?.Trim();

        info.RowIndex = await rowHandle.EvaluateAsync<int?>(@"(el) => {
            if (!el) return -1;
            const parent = el.parentElement;
            if (!parent) return -1;
            const siblings = Array.from(parent.children).filter(child => child.tagName === el.tagName);
            return siblings.indexOf(el);
        }").ConfigureAwait(false) ?? -1;

        var cells = await rowHandle.QuerySelectorAllAsync("th,td").ConfigureAwait(false);
        foreach (var cell in cells)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = (await cell.InnerTextAsync().ConfigureAwait(false))?.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                info.CellTexts.Add(text);
            }
        }

        if (!string.IsNullOrEmpty(info.DomId))
        {
            info.DomSelector = $"#{EscapeCssIdentifier(info.DomId)}";
            info.Selectors["dom-id"] = info.DomSelector;
        }

        if (!string.IsNullOrEmpty(tableSelector) && info.RowIndex >= 0)
        {
            info.Selectors["table-row"] = $"{tableSelector} tr:nth-of-type({info.RowIndex + 1})";
            info.Selectors["row-index"] = info.RowIndex.ToString();
        }

        var cssPath = await ComputeCssPathAsync(rowHandle).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(cssPath))
        {
            info.Selectors["css"] = cssPath;
            info.CssPath = cssPath;
        }

        var xpath = await ComputeXPathAsync(rowHandle).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(xpath))
        {
            info.Selectors["xpath"] = xpath;
            info.XPath = xpath;
        }

        if (!string.IsNullOrEmpty(info.InnerText))
        {
            var textForSelector = info.InnerText.Length > 120
                ? info.InnerText[..120]
                : info.InnerText;
            info.Selectors["text"] = textForSelector;
        }

        return info;
    }

    private static async Task<bool> TryExecuteLocatorAsync(IPage page, ElementLocatorStrategy strategy, CancellationToken cancellationToken)
    {
        if (!strategy.Properties.TryGetValue("Selector", out var selector) ||
            string.IsNullOrWhiteSpace(selector))
        {
            return false;
        }

        strategy.Properties.TryGetValue("SelectorKind", out var kind);
        kind ??= strategy.Name;

        ILocator locator = kind switch
        {
            "xpath" or "XPath" => page.Locator($"xpath={selector}"),
            "dom-id" => page.Locator(selector.StartsWith("#", StringComparison.Ordinal) ? selector : $"#{EscapeCssIdentifier(selector)}"),
            "text" => page.GetByText(selector),
            "table-row" or "row-index" when strategy.Properties.TryGetValue("TableSelector", out var tableSel) &&
                                           int.TryParse(strategy.Properties.GetValueOrDefault("RowIndex"), out var rowIdx) =>
                page.Locator($"{tableSel} tr").Nth(rowIdx),
            _ => page.Locator(selector)
        };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var count = await locator.CountAsync().ConfigureAwait(false);
            if (count == 0) return false;

            await locator.First.ScrollIntoViewIfNeededAsync().ConfigureAwait(false);
            await locator.First.ClickAsync(new LocatorClickOptions { Trial = true }).ConfigureAwait(false);
            return true;
        }
        catch
        {
            try
            {
                await locator.First.ClickAsync().ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    private static async Task<string?> ComputeCssPathAsync(IElementHandle element)
    {
        return await element.EvaluateAsync<string?>(@"(el) => {
            if (!el) return null;
            if (el.id) return `#${el.id}`;
            const parts = [];
            while (el && el.nodeType === Node.ELEMENT_NODE) {
                let selector = el.nodeName.toLowerCase();
                if (el.className) {
                    const classList = el.className.trim().split(/\s+/).filter(Boolean);
                    if (classList.length) {
                        selector += '.' + classList.join('.');
                    }
                }
                let sibling = el;
                let index = 1;
                while (sibling = sibling.previousElementSibling) {
                    if (sibling.nodeName === el.nodeName) {
                        index++;
                    }
                }
                selector += `:nth-of-type(${index})`;
                parts.unshift(selector);
                el = el.parentElement;
            }
            return parts.join(' > ');
        }").ConfigureAwait(false);
    }

    private static async Task<string?> ComputeXPathAsync(IElementHandle element)
    {
        return await element.EvaluateAsync<string?>(@"(el) => {
            if (!el) return null;
            const getIndex = (node) => {
                let index = 1;
                let sibling = node.previousSibling;
                while (sibling) {
                    if (sibling.nodeType === Node.ELEMENT_NODE && sibling.nodeName === node.nodeName) {
                        index++;
                    }
                    sibling = sibling.previousSibling;
                }
                return index;
            };
            const buildPath = (node) => {
                if (node === document.body) {
                    return '/html/body';
                }
                const parent = node.parentNode;
                if (!parent || parent.nodeType !== Node.ELEMENT_NODE) {
                    return '';
                }
                const index = getIndex(node);
                return `${buildPath(parent)}/${node.nodeName.toLowerCase()}[${index}]`;
            };
            return buildPath(el);
        }").ConfigureAwait(false);
    }

    private static string EscapeCssIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var builder = new StringBuilder();
        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch) || ch is '_' or '-' or ':')
            {
                builder.Append(ch);
            }
            else
            {
                builder.Append('\\');
                builder.Append(((int)ch).ToString("X2"));
                builder.Append(' ');
            }
        }

        return builder.ToString();
    }
}

public class PlaywrightSelectorInfo
{
    public string? DomId { get; set; }
    public string? DomClass { get; set; }
    public string? DomSelector { get; set; }
    public string? InnerText { get; set; }
    public string? CssPath { get; set; }
    public string? XPath { get; set; }
    public string? TableSelector { get; set; }
    public int RowIndex { get; set; } = -1;
    public List<string> CellTexts { get; } = new();
    public Dictionary<string, string> Selectors { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string? ErrorMessage { get; set; }
}
