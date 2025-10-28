using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace MedulaOtomasyon;

/// <summary>
/// Mouse ile UI element yakalama yardımcı sınıfı
/// </summary>
public class UIElementPicker
{
    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT Point);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// Mouse pozisyonundaki UI elementini UI Automation ile yakalar
    /// </summary>
    public static async Task<UIElementInfo?> CaptureElementAtMousePositionAsync()
    {
        try
        {
            // Mouse pozisyonunu al
            if (!GetCursorPos(out POINT point))
            {
                return null;
            }

            // UI Automation ile element bul
            var element = AutomationElement.FromPoint(new System.Windows.Point(point.X, point.Y));
            if (element == null)
            {
                return null;
            }

            return await ExtractUIAutomationInfoAsync(element);
        }
        catch (Exception ex)
        {
            throw new Exception($"Element yakalama hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Mouse pozisyonundaki UI elementini UI Automation ile yakalar (synchronous wrapper)
    /// </summary>
    public static UIElementInfo? CaptureElementAtMousePosition()
    {
        return CaptureElementAtMousePositionAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// UI Automation elementinden tüm özellikleri çıkarır (async)
    /// 3 teknoloji ile zenginleştirme: UI Automation → Playwright → MSHTML
    /// </summary>
    public static async Task<UIElementInfo> ExtractUIAutomationInfoAsync(AutomationElement element)
    {
        var info = await ExtractUIAutomationInfoInternalAsync(element);
        return info;
    }

    /// <summary>
    /// UI Automation elementinden tüm özellikleri çıkarır (synchronous wrapper)
    /// </summary>
    public static UIElementInfo ExtractUIAutomationInfo(AutomationElement element)
    {
        return ExtractUIAutomationInfoAsync(element).GetAwaiter().GetResult();
    }

    /// <summary>
    /// UI Automation elementinden tüm özellikleri çıkarır (internal async implementation)
    /// </summary>
    private static async Task<UIElementInfo> ExtractUIAutomationInfoInternalAsync(AutomationElement element)
    {
        var info = new UIElementInfo
        {
            DetectionMethod = "UIAutomation",
            CapturedAt = DateTime.Now
        };

        try
        {
            // === TEMEL ÖZELLİKLER ===
            info.AutomationId = element.Current.AutomationId;
            info.Name = element.Current.Name;
            info.ClassName = element.Current.ClassName;
            info.ControlType = element.Current.ControlType.ProgrammaticName;
            info.FrameworkId = element.Current.FrameworkId;
            info.LocalizedControlType = element.Current.LocalizedControlType;
            info.HelpText = element.Current.HelpText;
            info.AcceleratorKey = element.Current.AcceleratorKey;
            info.AccessKey = element.Current.AccessKey;
            info.ItemType = element.Current.ItemType;
            info.ItemStatus = element.Current.ItemStatus;

            // RuntimeId (geçici ID)
            try
            {
                var runtimeId = element.GetRuntimeId();
                if (runtimeId != null)
                {
                    info.RuntimeId = string.Join(",", runtimeId);
                }
            }
            catch { }

            // === DURUM ÖZELLİKLERİ ===
            info.IsEnabled = element.Current.IsEnabled;
            info.IsOffscreen = element.Current.IsOffscreen;
            info.IsKeyboardFocusable = element.Current.IsKeyboardFocusable;
            info.HasKeyboardFocus = element.Current.HasKeyboardFocus;
            info.IsPassword = element.Current.IsPassword;
            info.IsContentElement = element.Current.IsContentElement;
            info.IsControlElement = element.Current.IsControlElement;

            // IsVisible hesapla (IsOffscreen'in tersi gibi ama farklı)
            info.IsVisible = !element.Current.IsOffscreen && element.Current.IsEnabled;

            // === KONUM VE BOYUT ===
            var rect = element.Current.BoundingRectangle;
            info.X = (int)rect.X;
            info.Y = (int)rect.Y;
            info.Width = (int)rect.Width;
            info.Height = (int)rect.Height;
            info.BoundingRectangle = $"{rect.X},{rect.Y},{rect.Width},{rect.Height}";

            // === PENCERE BİLGİLERİ ===
            try
            {
                var window = GetParentWindow(element);
                if (window != null)
                {
                    info.WindowTitle = window.Current.Name;
                    info.WindowName = window.Current.Name;
                    info.WindowClassName = window.Current.ClassName;
                    info.WindowId = window.Current.NativeWindowHandle.ToString();
                    info.WindowProcessId = window.Current.ProcessId;

                    // Process adını al
                    try
                    {
                        var process = System.Diagnostics.Process.GetProcessById(window.Current.ProcessId);
                        info.WindowProcessName = process.ProcessName;
                    }
                    catch { }
                }
            }
            catch { }

            // === CONTAINER BİLGİLERİ (Element'in immediate parent container'ı - Pane, Group, vb) ===
            try
            {
                var container = GetImmediateContainer(element);
                if (container != null)
                {
                    info.ContainerAutomationId = container.Current.AutomationId;
                    info.ContainerName = container.Current.Name;
                    info.ContainerClassName = container.Current.ClassName;
                    info.ContainerControlType = container.Current.ControlType.ProgrammaticName;

                    // Container RuntimeId
                    try
                    {
                        var containerRuntimeId = container.GetRuntimeId();
                        if (containerRuntimeId != null)
                        {
                            info.ContainerRuntimeId = string.Join(",", containerRuntimeId);
                        }
                    }
                    catch { }
                }
            }
            catch { }

            // === HİYERARŞİ VE PATH ===
            info.ElementPath = BuildElementPath(element);
            info.TreePath = BuildTreePath(element);
            info.ParentChain = BuildParentChain(element);

            // Parent özellikleri
            try
            {
                var parent = TreeWalker.RawViewWalker.GetParent(element);
                if (parent != null && parent != AutomationElement.RootElement)
                {
                    info.ParentAutomationId = parent.Current.AutomationId;
                    info.ParentName = parent.Current.Name;
                    info.ParentClassName = parent.Current.ClassName;
                }
            }
            catch { }

            // === INDEX BİLGİLERİ ===
            info.IndexInParent = GetIndexInParent(element);

            // === EK TANIMLAYICILAR (Generic Name için) ===
            // GrandParent bilgileri
            try
            {
                var parent = TreeWalker.RawViewWalker.GetParent(element);
                if (parent != null && parent != AutomationElement.RootElement)
                {
                    var grandParent = TreeWalker.RawViewWalker.GetParent(parent);
                    if (grandParent != null && grandParent != AutomationElement.RootElement)
                    {
                        info.GrandParentName = grandParent.Current.Name;
                        info.GrandParentAutomationId = grandParent.Current.AutomationId;
                    }
                }
            }
            catch { }

            // Sibling context ve count
            try
            {
                var parent = TreeWalker.RawViewWalker.GetParent(element);
                if (parent != null)
                {
                    var siblings = parent.FindAll(TreeScope.Children, Condition.TrueCondition);
                    info.SiblingCount = siblings.Count;

                    // Sibling context: İlk 5 sibling'in Name'lerini topla
                    var siblingNames = new List<string>();
                    for (int i = 0; i < Math.Min(5, siblings.Count); i++)
                    {
                        try
                        {
                            var sibling = siblings[i];
                            var sibName = sibling.Current.Name;
                            var sibCtrlType = sibling.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");

                            if (!string.IsNullOrEmpty(sibName))
                            {
                                siblingNames.Add($"{sibCtrlType}[{sibName}]");
                            }
                            else
                            {
                                siblingNames.Add(sibCtrlType);
                            }
                        }
                        catch { }
                    }

                    if (siblingNames.Count > 0)
                    {
                        info.SiblingContext = string.Join(", ", siblingNames);
                    }
                }
            }
            catch { }

            // === ETİKET VE İLİŞKİLER ===
            try
            {
                var labeledBy = element.Current.LabeledBy;
                if (labeledBy != null)
                {
                    info.LabeledByElement = $"{labeledBy.Current.Name} (AutomationId: {labeledBy.Current.AutomationId})";
                }
            }
            catch { }

            // DescribedBy UIA'da yok, MSHTML'den alınacak

            // === 3 TEKNOLOJİ İLE ZENGİNLEŞTİRME ===
            // Teknoloji 1: UI Automation (yukarıda tamamlandı)

            // Teknoloji 2: Playwright (Chrome, Edge, Firefox için)
            if (info.FrameworkId == "Chrome" || info.FrameworkId == "Edge" || info.FrameworkId == "Firefox")
            {
                ExtractWebProperties(element, info);

                try
                {
                    await PlaywrightExtractor.EnrichWithPlaywrightAsync(element, info);

                    // Detection method güncelle
                    if (!string.IsNullOrEmpty(info.PlaywrightSelector))
                    {
                        info.DetectionMethod = "UIAutomation+Playwright";
                    }
                }
                catch
                {
                    // Playwright hatası - devam et
                }
            }

            // Teknoloji 3: MSHTML (Internet Explorer + WebBrowser Control için)
            // WinForms/Win32 WebBrowser control da MSHTML kullanır
            bool isWebBrowserElement = IsWebBrowserElement(element, info);
            if (info.FrameworkId == "InternetExplorer" || isWebBrowserElement)
            {
                ExtractWebProperties(element, info);

                try
                {
                    MSHTMLExtractor.EnrichWithMSHTML(element, info);

                    // Detection method güncelle
                    if (!string.IsNullOrEmpty(info.CssSelector) || !string.IsNullOrEmpty(info.XPath))
                    {
                        info.DetectionMethod = isWebBrowserElement ? "UIAutomation+MSHTML(WebBrowser)" : "UIAutomation+MSHTML";
                    }
                }
                catch
                {
                    // MSHTML hatası - devam et
                }
            }

        }
        catch (Exception ex)
        {
            throw new Exception($"Element özellikleri çıkarılırken hata: {ex.Message}", ex);
        }

        return info;
    }

    /// <summary>
    /// Web elementleri için ek özellikleri çıkarır
    /// </summary>
    private static void ExtractWebProperties(AutomationElement element, UIElementInfo info)
    {
        try
        {
            // ValuePattern ile value özelliği
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern)
                && valuePattern is ValuePattern vp)
            {
                info.Value = vp.Current.Value;
                info.InnerText = vp.Current.Value; // ValuePattern'den inner text olarak da kullan
            }
        }
        catch { }
    }

    /// <summary>
    /// Tree path oluşturur - Geliştirilmiş versiyon
    /// Format: "0/2/5/1" (index bazlı)
    /// Index bulunamazsa "?" kullanır, böylece path yine de oluşur
    /// </summary>
    private static string BuildTreePath(AutomationElement element)
    {
        var pathParts = new List<string>();
        var current = element;
        int depth = 0;
        const int maxDepth = 15; // Daha derin hierarchy'ler için artırıldı

        try
        {
            while (current != null && current != AutomationElement.RootElement && depth < maxDepth)
            {
                try
                {
                    var index = GetIndexInParent(current);

                    if (index.HasValue)
                    {
                        // Index başarıyla bulundu
                        pathParts.Insert(0, index.Value.ToString());
                    }
                    else
                    {
                        // Index bulunamadı - ControlType bilgisi ile fallback
                        var controlType = current.Current.ControlType.ProgrammaticName
                            .Replace("ControlType.", "");
                        pathParts.Insert(0, $"?[{controlType}]");
                    }

                    // Parent'a git
                    current = TreeWalker.RawViewWalker.GetParent(current);
                    depth++;
                }
                catch
                {
                    // Bu seviyede hata - bir sonraki parent'a geç
                    try
                    {
                        current = TreeWalker.RawViewWalker.GetParent(current);
                        depth++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }
        catch { }

        // Eğer hiç path oluşturulamadıysa
        if (pathParts.Count == 0)
        {
            try
            {
                var controlType = element.Current.ControlType.ProgrammaticName
                    .Replace("ControlType.", "");
                return $"?[{controlType}]";
            }
            catch
            {
                return "?";
            }
        }

        return string.Join("/", pathParts);
    }

    /// <summary>
    /// Parent içindeki index'i bulur - Geliştirilmiş versiyon
    /// Birden fazla yöntemle index bulmayı dener
    /// </summary>
    private static int? GetIndexInParent(AutomationElement element)
    {
        try
        {
            var parent = TreeWalker.RawViewWalker.GetParent(element);
            if (parent == null || parent == AutomationElement.RootElement)
                return null;

            // Yöntem 1: FindAll ile tüm children'ı bul ve Automation.Compare ile karşılaştır
            try
            {
                var siblings = parent.FindAll(TreeScope.Children, Condition.TrueCondition);
                if (siblings != null && siblings.Count > 0)
                {
                    for (int i = 0; i < siblings.Count; i++)
                    {
                        try
                        {
                            if (Automation.Compare(siblings[i], element))
                            {
                                return i;
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // Yöntem 2: TreeWalker ile manuel olarak iterate et
            try
            {
                var walker = TreeWalker.RawViewWalker;
                var currentChild = walker.GetFirstChild(parent);
                int index = 0;

                while (currentChild != null && index < 1000) // Max 1000 child'a kadar bak
                {
                    try
                    {
                        if (Automation.Compare(currentChild, element))
                        {
                            return index;
                        }
                    }
                    catch { }

                    try
                    {
                        currentChild = walker.GetNextSibling(currentChild);
                        index++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch { }

            // Yöntem 3: RuntimeId karşılaştırması (son çare)
            try
            {
                var targetRuntimeId = element.GetRuntimeId();
                if (targetRuntimeId != null)
                {
                    var siblings = parent.FindAll(TreeScope.Children, Condition.TrueCondition);
                    if (siblings != null)
                    {
                        for (int i = 0; i < siblings.Count; i++)
                        {
                            try
                            {
                                var siblingRuntimeId = siblings[i].GetRuntimeId();
                                if (siblingRuntimeId != null &&
                                    targetRuntimeId.SequenceEqual(siblingRuntimeId))
                                {
                                    return i;
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }
        catch { }

        return null;
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
    /// Element'in immediate parent container'ını bulur (Pane, Group, Document vb.)
    /// Bu container, botanik overlay gibi farklı yapıların tespiti için kritik
    /// </summary>
    private static AutomationElement? GetImmediateContainer(AutomationElement element)
    {
        try
        {
            var parent = TreeWalker.RawViewWalker.GetParent(element);
            if (parent == null || parent == AutomationElement.RootElement)
            {
                return null;
            }

            // Parent doğrudan Window ise, Container olarak kullanma (zaten window bilgisi var)
            // Bunun yerine parent'ın parent'ına bak
            if (parent.Current.ControlType == ControlType.Window)
            {
                return null; // Window container değil
            }

            // Pane, Group, Document gibi container tipler
            var containerTypes = new[]
            {
                ControlType.Pane,
                ControlType.Group,
                ControlType.Document,
                ControlType.Custom,
                ControlType.Tab,
                ControlType.TabItem
            };

            // Parent'ı container olarak kabul et
            if (containerTypes.Contains(parent.Current.ControlType))
            {
                return parent;
            }

            // Değilse, parent'ın parent'ına bak (max 3 seviye yukarı)
            for (int i = 0; i < 3; i++)
            {
                parent = TreeWalker.RawViewWalker.GetParent(parent);
                if (parent == null || parent == AutomationElement.RootElement)
                {
                    break;
                }

                if (parent.Current.ControlType == ControlType.Window)
                {
                    break; // Window'a ulaştık, container yok
                }

                if (containerTypes.Contains(parent.Current.ControlType))
                {
                    return parent;
                }
            }
        }
        catch { }

        return null;
    }

    private static string BuildElementPath(AutomationElement element)
    {
        var parts = new List<string>();
        var current = element;

        try
        {
            while (current != null && current != AutomationElement.RootElement)
            {
                var controlType = current.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");
                var name = current.Current.Name;

                if (!string.IsNullOrEmpty(name))
                {
                    parts.Insert(0, $"{controlType}[{name}]");
                }
                else
                {
                    parts.Insert(0, controlType);
                }

                current = TreeWalker.RawViewWalker.GetParent(current);

                // Maksimum derinlik
                if (parts.Count > 10)
                    break;
            }
        }
        catch { }

        return string.Join("/", parts);
    }

    private static string BuildParentChain(AutomationElement element)
    {
        var chain = new List<string>();
        var current = element;

        try
        {
            while (current != null && current != AutomationElement.RootElement)
            {
                var name = current.Current.Name;
                var controlType = current.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");

                if (!string.IsNullOrEmpty(name))
                {
                    chain.Insert(0, $"{controlType}:{name}");
                }
                else
                {
                    chain.Insert(0, controlType);
                }

                current = TreeWalker.RawViewWalker.GetParent(current);

                // Maksimum derinlik
                if (chain.Count > 5)
                    break;
            }
        }
        catch { }

        return string.Join(" > ", chain);
    }

    /// <summary>
    /// WinForms/Win32 WebBrowser control içindeki element mi kontrol et
    /// Botanik gibi overlay uygulamalar WebBrowser control kullanır
    /// </summary>
    private static bool IsWebBrowserElement(AutomationElement element, UIElementInfo info)
    {
        try
        {
            // WinForms veya Win32 framework'ü kontrolü
            if (info.FrameworkId != "Win32" && info.FrameworkId != "WinForms")
            {
                return false;
            }

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
                return false;
            }

            // IE_Server window varlığını kontrol et
            // MSHTMLExtractor'daki FindIEServerWindow metodu hierarchy'de IE_Server arar
            var ieServerHwnd = MSHTMLExtractor.FindIEServerWindow(hwnd);
            return ieServerHwnd != IntPtr.Zero;
        }
        catch
        {
            return false;
        }
    }
}
