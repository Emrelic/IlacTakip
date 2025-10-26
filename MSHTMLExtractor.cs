using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace MedulaOtomasyon;

#region COM Interfaces for MSHTML

/// <summary>
/// IHTMLDocument2 COM interface (mshtml.dll)
/// </summary>
[ComImport]
[Guid("332C4425-26CB-11D0-B483-00C04FD90119")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IHTMLDocument2
{
    [return: MarshalAs(UnmanagedType.IDispatch)]
    object GetScript();

    IHTMLElementCollection GetAll();

    [return: MarshalAs(UnmanagedType.Interface)]
    object GetBody();

    [return: MarshalAs(UnmanagedType.Interface)]
    object GetActiveElement();

    IHTMLElementCollection GetImages();
    IHTMLElementCollection GetApplets();
    IHTMLElementCollection GetLinks();
    IHTMLElementCollection GetForms();
    IHTMLElementCollection GetAnchors();

    void SetTitle([MarshalAs(UnmanagedType.BStr)] string p);

    [return: MarshalAs(UnmanagedType.BStr)]
    string GetTitle();

    IHTMLElementCollection GetElementsByName([MarshalAs(UnmanagedType.BStr)] string v);

    [return: MarshalAs(UnmanagedType.Interface)]
    IHTMLElement GetElementById([MarshalAs(UnmanagedType.BStr)] string v);
}

/// <summary>
/// IHTMLElement COM interface
/// </summary>
[ComImport]
[Guid("3050F1FF-98B5-11CF-BB82-00AA00BDCE0B")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IHTMLElement
{
    void SetAttribute([MarshalAs(UnmanagedType.BStr)] string strAttributeName, object AttributeValue, int lFlags);
    object GetAttribute([MarshalAs(UnmanagedType.BStr)] string strAttributeName, int lFlags);
    bool RemoveAttribute([MarshalAs(UnmanagedType.BStr)] string strAttributeName, int lFlags);

    void SetClassName([MarshalAs(UnmanagedType.BStr)] string p);
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetClassName();

    void SetId([MarshalAs(UnmanagedType.BStr)] string p);
    [return: MarshalAs(UnmanagedType.BStr)]
    string GetId();

    [return: MarshalAs(UnmanagedType.BStr)]
    string GetTagName();

    [return: MarshalAs(UnmanagedType.Interface)]
    IHTMLElement GetParentElement();

    [return: MarshalAs(UnmanagedType.BStr)]
    string GetInnerHTML();
    void SetInnerHTML([MarshalAs(UnmanagedType.BStr)] string p);

    [return: MarshalAs(UnmanagedType.BStr)]
    string GetInnerText();
    void SetInnerText([MarshalAs(UnmanagedType.BStr)] string p);

    [return: MarshalAs(UnmanagedType.BStr)]
    string GetOuterHTML();
    void SetOuterHTML([MarshalAs(UnmanagedType.BStr)] string p);
}

/// <summary>
/// IHTMLElementCollection COM interface
/// </summary>
[ComImport]
[Guid("3050F21F-98B5-11CF-BB82-00AA00BDCE0B")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IHTMLElementCollection
{
    [return: MarshalAs(UnmanagedType.BStr)]
    string ToString();

    void SetLength(int p);
    int GetLength();

    [return: MarshalAs(UnmanagedType.Interface)]
    object Item(object name, object index);
}

#endregion

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

    #region Native Methods

    private const int OBJID_CLIENT = unchecked((int)0xFFFFFFFC);
    private const uint WM_HTML_GETOBJECT = 0x41D;

    [DllImport("user32.dll", EntryPoint = "SendMessageTimeout")]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam,
        uint flags,
        uint timeout,
        out IntPtr result);

    [DllImport("oleacc.dll")]
    private static extern int ObjectFromLresult(
        IntPtr lResult,
        [In] ref Guid riid,
        IntPtr wParam,
        [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

    #endregion

    /// <summary>
    /// HTMLElement'e erişmeyi dener (COM interop)
    /// </summary>
    private static dynamic? TryGetHTMLElement(AutomationElement element)
    {
        try
        {
            // 1. IHTMLDocument2'yi al
            var doc = TryGetHTMLDocument(element);
            if (doc == null)
            {
                return null;
            }

            // 2. Element'i document'te bul
            // AutomationId genellikle HTML id ile eşleşir
            var automationId = element.Current.AutomationId;
            if (!string.IsNullOrEmpty(automationId))
            {
                try
                {
                    var htmlElement = doc.GetElementById(automationId);
                    if (htmlElement != null)
                    {
                        return htmlElement;
                    }
                }
                catch { }
            }

            // Name ile bul
            var name = element.Current.Name;
            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    var elements = doc.GetElementsByName(name);
                    if (elements != null && elements.GetLength() > 0)
                    {
                        return elements.Item(0, 0);
                    }
                }
                catch { }
            }

            // Element bulunamadı ama document var - temel bilgiler için document kullanabiliriz
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// IHTMLDocument2 nesnesini almaya çalışır
    /// </summary>
    private static IHTMLDocument2? TryGetHTMLDocument(AutomationElement element)
    {
        try
        {
            // Window handle'ını al
            var hwnd = new IntPtr(element.Current.NativeWindowHandle);
            if (hwnd == IntPtr.Zero)
            {
                // Parent window'dan bul
                var window = GetParentWindow(element);
                if (window != null)
                {
                    hwnd = new IntPtr(window.Current.NativeWindowHandle);
                }
            }

            if (hwnd == IntPtr.Zero)
            {
                return null;
            }

            // IE_Server child window'unu bul
            var ieServerHwnd = FindIEServerWindow(hwnd);
            if (ieServerHwnd == IntPtr.Zero)
            {
                ieServerHwnd = hwnd; // Kendi HWND'si IE_Server olabilir
            }

            // WM_HTML_GETOBJECT message gönder
            IntPtr lResult;
            var sendResult = SendMessageTimeout(
                ieServerHwnd,
                WM_HTML_GETOBJECT,
                IntPtr.Zero,
                IntPtr.Zero,
                2, // SMTO_ABORTIFHUNG
                1000,
                out lResult);

            if (lResult == IntPtr.Zero)
            {
                return null;
            }

            // IHTMLDocument2 GUID
            var iid = typeof(IHTMLDocument2).GUID;

            // ObjectFromLresult ile IHTMLDocument2 al
            int hr = ObjectFromLresult(lResult, ref iid, IntPtr.Zero, out object doc);
            if (hr == 0 && doc is IHTMLDocument2 htmlDoc)
            {
                return htmlDoc;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// IE_Server child window'unu bulur
    /// </summary>
    private static IntPtr FindIEServerWindow(IntPtr hwndParent)
    {
        try
        {
            // Doğrudan IE_Server ara
            var ieServer = FindWindowEx(hwndParent, IntPtr.Zero, "Internet Explorer_Server", null);
            if (ieServer != IntPtr.Zero)
            {
                return ieServer;
            }

            // Nested aramaya çalış (TabWindowClass > Shell DocObject View > Internet Explorer_Server)
            var tabWindow = FindWindowEx(hwndParent, IntPtr.Zero, "TabWindowClass", null);
            if (tabWindow != IntPtr.Zero)
            {
                var shellDocView = FindWindowEx(tabWindow, IntPtr.Zero, "Shell DocObject View", null);
                if (shellDocView != IntPtr.Zero)
                {
                    ieServer = FindWindowEx(shellDocView, IntPtr.Zero, "Internet Explorer_Server", null);
                    if (ieServer != IntPtr.Zero)
                    {
                        return ieServer;
                    }
                }
            }

            return IntPtr.Zero;
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    private static AutomationElement? GetParentWindow(AutomationElement element)
    {
        var current = element;
        while (current != null)
        {
            try
            {
                if (current.Current.ControlType == ControlType.Window)
                {
                    return current;
                }
                current = TreeWalker.RawViewWalker.GetParent(current);
            }
            catch
            {
                break;
            }
        }
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
