using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace MedulaOtomasyon;

/// <summary>
/// MSHTML (Internet Explorer) elementlerinden özellik çıkarır
/// </summary>
public static class MSHTMLExtractor
{
    /// <summary>
    /// IE elementi ise MSHTML ile ek özellikleri toplar
    /// </summary>
    public static void EnrichWithMSHTML(AutomationElement element, UIElementInfo info)
    {
        try
        {
            // Sadece IE/Edge elementleri için
            if (info.FrameworkId != "InternetExplorer" && info.FrameworkId != "Edge")
            {
                return;
            }

            // NativeWindowHandle al
            var hwnd = new IntPtr(element.Current.NativeWindowHandle);
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            // IHTMLElement'e erişmeye çalış
            dynamic? htmlElement = TryGetHTMLElement(element);
            if (htmlElement == null)
            {
                return;
            }

            // HTML özellikleri
            try
            {
                info.HtmlId = htmlElement.id ?? "";
                info.Tag = htmlElement.tagName ?? "";
                info.TagName = htmlElement.tagName ?? "";
                info.InnerText = htmlElement.innerText ?? "";
                info.InnerHtml = htmlElement.innerHTML ?? "";
                info.Title = htmlElement.title ?? "";
                info.ClassName = htmlElement.className ?? "";
            }
            catch { }

            // Attributes
            try
            {
                dynamic? attributes = htmlElement.attributes;
                if (attributes != null)
                {
                    info.DataAttributes = new Dictionary<string, string>();
                    info.OtherAttributes = new Dictionary<string, string>();

                    int length = attributes.length;
                    for (int i = 0; i < length; i++)
                    {
                        try
                        {
                            dynamic attr = attributes[i];
                            string attrName = attr.nodeName ?? "";
                            string attrValue = attr.nodeValue?.ToString() ?? "";

                            // ARIA attributes
                            if (attrName.StartsWith("aria-"))
                            {
                                switch (attrName)
                                {
                                    case "aria-label":
                                        info.AriaLabel = attrValue;
                                        break;
                                    case "aria-labelledby":
                                        info.AriaLabelledBy = attrValue;
                                        break;
                                    case "aria-describedby":
                                        info.AriaDescribedBy = attrValue;
                                        break;
                                    case "aria-role":
                                        info.AriaRole = attrValue;
                                        break;
                                    case "aria-required":
                                        info.AriaRequired = attrValue;
                                        break;
                                    case "aria-expanded":
                                        info.AriaExpanded = attrValue;
                                        break;
                                    case "aria-checked":
                                        info.AriaChecked = attrValue;
                                        break;
                                    case "aria-hidden":
                                        info.AriaHidden = attrValue;
                                        break;
                                }
                            }
                            // data-* attributes
                            else if (attrName.StartsWith("data-"))
                            {
                                info.DataAttributes[attrName] = attrValue;
                            }
                            // Standart HTML attributes
                            else
                            {
                                switch (attrName)
                                {
                                    case "name":
                                        info.HtmlName = attrValue;
                                        break;
                                    case "type":
                                        info.Type = attrValue;
                                        break;
                                    case "value":
                                        info.Value = attrValue;
                                        break;
                                    case "href":
                                        info.Href = attrValue;
                                        break;
                                    case "src":
                                        info.Src = attrValue;
                                        break;
                                    case "alt":
                                        info.Alt = attrValue;
                                        break;
                                    case "placeholder":
                                        info.Placeholder = attrValue;
                                        break;
                                    default:
                                        info.OtherAttributes[attrName] = attrValue;
                                        break;
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // CSS Selector oluştur
            info.CssSelector = GenerateCssSelector(info);

            // XPath oluştur
            info.XPath = GenerateXPath(info);
        }
        catch
        {
            // MSHTML hatası - sessizce devam et
        }
    }

    /// <summary>
    /// HTMLElement'e erişmeyi dener (COM interop)
    /// </summary>
    private static dynamic? TryGetHTMLElement(AutomationElement element)
    {
        try
        {
            // ValuePattern'den değer al
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern)
                && valuePattern is ValuePattern vp)
            {
                // Bu kısım basitleştirilmiş - gerçek MSHTML erişimi daha karmaşık
                // Şimdilik null döndür, ileride COM interop ile geliştirilecek
                return null;
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Element için CSS Selector oluşturur
    /// </summary>
    private static string? GenerateCssSelector(UIElementInfo info)
    {
        var parts = new List<string>();

        // ID varsa en spesifik
        if (!string.IsNullOrEmpty(info.HtmlId))
        {
            return $"#{info.HtmlId}";
        }

        // Tag
        if (!string.IsNullOrEmpty(info.Tag))
        {
            parts.Add(info.Tag.ToLower());
        }

        // ClassName
        if (!string.IsNullOrEmpty(info.ClassName))
        {
            var classes = info.ClassName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var cls in classes)
            {
                parts.Add($".{cls}");
            }
        }

        // Name attribute
        if (!string.IsNullOrEmpty(info.HtmlName))
        {
            parts.Add($"[name='{info.HtmlName}']");
        }

        // Type attribute
        if (!string.IsNullOrEmpty(info.Type))
        {
            parts.Add($"[type='{info.Type}']");
        }

        return parts.Count > 0 ? string.Join("", parts) : null;
    }

    /// <summary>
    /// Element için XPath oluşturur
    /// </summary>
    private static string? GenerateXPath(UIElementInfo info)
    {
        if (string.IsNullOrEmpty(info.Tag))
        {
            return null;
        }

        var conditions = new List<string>();

        // ID varsa
        if (!string.IsNullOrEmpty(info.HtmlId))
        {
            return $"//{info.Tag.ToLower()}[@id='{info.HtmlId}']";
        }

        // Name
        if (!string.IsNullOrEmpty(info.HtmlName))
        {
            conditions.Add($"@name='{info.HtmlName}'");
        }

        // Text
        if (!string.IsNullOrEmpty(info.InnerText) && info.InnerText.Length < 50)
        {
            conditions.Add($"text()='{info.InnerText}'");
        }

        // ClassName
        if (!string.IsNullOrEmpty(info.ClassName))
        {
            conditions.Add($"@class='{info.ClassName}'");
        }

        if (conditions.Count > 0)
        {
            return $"//{info.Tag.ToLower()}[{string.Join(" and ", conditions)}]";
        }

        return $"//{info.Tag.ToLower()}";
    }
}
