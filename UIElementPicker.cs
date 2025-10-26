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

            // Teknoloji 3: MSHTML (Internet Explorer için)
            else if (info.FrameworkId == "InternetExplorer")
            {
                ExtractWebProperties(element, info);

                try
                {
                    MSHTMLExtractor.EnrichWithMSHTML(element, info);

                    // Detection method güncelle
                    if (!string.IsNullOrEmpty(info.CssSelector) || !string.IsNullOrEmpty(info.XPath))
                    {
                        info.DetectionMethod = "UIAutomation+MSHTML";
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
    /// Tree path oluşturur (index bazlı: "0/2/5/1")
    /// </summary>
    private static string BuildTreePath(AutomationElement element)
    {
        var indices = new List<int>();
        var current = element;

        try
        {
            while (current != null && current != AutomationElement.RootElement)
            {
                var index = GetIndexInParent(current);
                if (index.HasValue)
                {
                    indices.Insert(0, index.Value);
                }

                current = TreeWalker.RawViewWalker.GetParent(current);

                if (indices.Count > 10) break; // Maksimum derinlik
            }
        }
        catch { }

        return string.Join("/", indices);
    }

    /// <summary>
    /// Parent içindeki index'i bulur
    /// </summary>
    private static int? GetIndexInParent(AutomationElement element)
    {
        try
        {
            var parent = TreeWalker.RawViewWalker.GetParent(element);
            if (parent == null) return null;

            var siblings = parent.FindAll(TreeScope.Children, Condition.TrueCondition);
            for (int i = 0; i < siblings.Count; i++)
            {
                if (Automation.Compare(siblings[i], element))
                {
                    return i;
                }
            }
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
}
