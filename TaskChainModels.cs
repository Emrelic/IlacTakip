using System.Text.Json.Serialization;

namespace MedulaOtomasyon;

/// <summary>
/// Görev adımı tipleri
/// </summary>
public enum StepType
{
    /// <summary>
    /// Tip 1: Hedef program/pencere seçimi ve yapılacak işlem
    /// </summary>
    TargetSelection = 1,

    /// <summary>
    /// Tip 2: UI element tıklama/tuşlama görevleri
    /// </summary>
    UIElementAction = 2,

    /// <summary>
    /// Tip 3: Sayfa durum kontrolü (koşullu dallanma)
    /// </summary>
    ConditionalBranch = 3,

    /// <summary>
    /// Tip 4: Döngü veya görev bitiş koşulu
    /// </summary>
    LoopOrEnd = 4
}

/// <summary>
/// UI element üzerinde yapılacak eylem tipleri
/// </summary>
public enum ActionType
{
    None = 0,
    LeftClick = 1,
    RightClick = 2,
    DoubleClick = 3,
    MouseWheel = 4,
    KeyPress = 5,
    TypeText = 6,
    CheckCondition = 7 // Durum kontrolü için
}

/// <summary>
/// Hedef program/pencere bilgileri
/// </summary>
public class TargetInfo
{
    public string? ProgramPath { get; set; }
    public string? WindowTitle { get; set; }
    public string? WindowClassName { get; set; }
    public int? ProcessId { get; set; }
    public bool IsDesktop { get; set; }
    public string? InitialAction { get; set; } // "DoubleClick", "Enter", vb
}

/// <summary>
/// UI Element özellikleri (3 teknoloji ile toplanan veriler)
/// </summary>
public class UIElementInfo
{
    // === PENCERE BİLGİLERİ ===
    public string? WindowId { get; set; }
    public string? WindowName { get; set; }
    public string? WindowTitle { get; set; }
    public string? WindowClassName { get; set; }
    public string? WindowProcessName { get; set; }
    public int? WindowProcessId { get; set; }

    // === CONTAINER BİLGİLERİ (Element'in immediate parent container'ı - Pane, Group, vb) ===
    public string? ContainerAutomationId { get; set; }
    public string? ContainerName { get; set; }
    public string? ContainerClassName { get; set; }
    public string? ContainerControlType { get; set; }
    public string? ContainerRuntimeId { get; set; }

    // === UI AUTOMATION ÖZELLİKLERİ ===
    public string? AutomationId { get; set; }
    public string? RuntimeId { get; set; } // Sistem tarafından atanmış geçici ID
    public string? Name { get; set; }
    public string? ClassName { get; set; }
    public string? ControlType { get; set; }
    public string? FrameworkId { get; set; } // "Win32", "WPF", "InternetExplorer", vb
    public string? LocalizedControlType { get; set; }
    public string? HelpText { get; set; }
    public string? AcceleratorKey { get; set; }
    public string? AccessKey { get; set; }
    public string? Culture { get; set; }
    public string? ItemType { get; set; }
    public string? ItemStatus { get; set; }

    // Durum özellikleri
    public bool? IsEnabled { get; set; }
    public bool? IsOffscreen { get; set; }
    public bool? IsVisible { get; set; }
    public bool? IsKeyboardFocusable { get; set; }
    public bool? HasKeyboardFocus { get; set; }
    public bool? IsPassword { get; set; }
    public bool? IsContentElement { get; set; }
    public bool? IsControlElement { get; set; }

    // Etiket ve ilişkiler
    public string? LabeledByElement { get; set; } // Bağlı label elementi
    public string? DescribedByElement { get; set; }

    // === HİYERARŞİ VE PATH BİLGİLERİ ===
    public string? ElementPath { get; set; } // UIA Path: "Window/Pane/Button[Name]"
    public string? TreePath { get; set; } // Tree yolu: "0/2/5/1" (index bazlı)
    public string? ParentChain { get; set; } // "Window > Pane > Button"
    public string? ParentAutomationId { get; set; }
    public string? ParentName { get; set; }
    public string? ParentClassName { get; set; }

    // === WEB/HTML ÖZELLİKLERİ (MSHTML/Playwright) ===
    public string? HtmlId { get; set; } // HTML id attribute
    public string? Tag { get; set; } // HTML tag name: "button", "input", vb
    public string? TagName { get; set; } // Aynı (tagName ve tag)
    public string? Title { get; set; } // HTML title attribute
    public string? InnerText { get; set; }
    public string? InnerHtml { get; set; }
    public string? OuterHtml { get; set; }
    public string? Value { get; set; }
    public string? Type { get; set; } // input type: "text", "button", vb
    public string? Href { get; set; } // link href
    public string? Src { get; set; } // img/iframe src
    public string? Alt { get; set; } // img alt
    public string? Placeholder { get; set; }
    public string? HtmlName { get; set; } // HTML name attribute

    // ARIA (Accessibility) özellikleri
    public string? AriaLabel { get; set; }
    public string? AriaLabelledBy { get; set; }
    public string? AriaDescribedBy { get; set; }
    public string? AriaRole { get; set; }
    public string? AriaRequired { get; set; }
    public string? AriaExpanded { get; set; }
    public string? AriaChecked { get; set; }
    public string? AriaHidden { get; set; }

    // data-* özellikleri (dinamik olarak toplanacak)
    public Dictionary<string, string>? DataAttributes { get; set; }

    // Diğer HTML attributes
    public Dictionary<string, string>? OtherAttributes { get; set; }

    // === XPATH VE CSS SELECTORS ===
    public string? XPath { get; set; } // XPath selector
    public string? CssSelector { get; set; } // CSS Selector
    public string? PlaywrightSelector { get; set; } // Playwright selector

    // === KONUM VE BOYUT ===
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? BoundingRectangle { get; set; } // "X,Y,Width,Height" formatında

    // === TEKNOLOJİ BİLGİSİ ===
    public string? DetectionMethod { get; set; } // "UIAutomation", "MSHTML", "Playwright"
    public DateTime CapturedAt { get; set; } = DateTime.Now;

    // === INDEX BİLGİLERİ (Aynı tipten çok varsa) ===
    public int? IndexInParent { get; set; } // Parent içindeki index
    public int? SiblingIndex { get; set; } // Aynı türdeki kardeşler arasında index

    // === EK TANIMLAYICILAR (Name "1" gibi generic isimler için) ===
    public string? Role { get; set; } // HTML role attribute
    public string? TextContent { get; set; } // textContent (innerText'ten farklı, gizli metni de içerir)
    public string? SiblingContext { get; set; } // Sibling elementlerin özeti (Name "1" için context)
    public string? GrandParentName { get; set; } // 2. seviye parent (daha fazla context)
    public string? GrandParentAutomationId { get; set; } // GrandParent'ın AutomationId'si
    public string? ComputedCssPath { get; set; } // Hesaplanmış tam CSS path (body > div > span gibi)
    public int? SiblingCount { get; set; } // Parent altındaki toplam sibling sayısı
}

/// <summary>
/// Koşullu dallanma bilgileri
/// </summary>
public class ConditionInfo
{
    public string? PropertyName { get; set; } // "IsChecked", "Text", "IsVisible", vb
    public string? Operator { get; set; } // "Equals", "Contains", "IsTrue", "IsFalse", vb
    public string? ExpectedValue { get; set; }
    public string? LogicalOperator { get; set; } // "AND", "OR"
    public int? TrueStepNumber { get; set; } // True ise gidilecek adım
    public int? FalseStepNumber { get; set; } // False ise gidilecek adım
}

/// <summary>
/// Element bulma stratejisi
/// </summary>
public class ElementLocatorStrategy
{
    public string Name { get; set; } = ""; // Örn: "AutomationId", "Name + ControlType"
    public string Description { get; set; } = "";
    public LocatorType Type { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new(); // Kullanılan özellikler
    public bool IsSuccessful { get; set; } // Test başarılı mı?
    public int TestDurationMs { get; set; } // Test süresi (ms)
    public string? ErrorMessage { get; set; } // Hata mesajı varsa
}

/// <summary>
/// Locator tipleri
/// </summary>
public enum LocatorType
{
    AutomationId,
    Name,
    ClassName,
    AutomationIdAndControlType,
    NameAndControlType,
    ElementPath,
    TreePath,
    XPath,
    CssSelector,
    NameAndParent,
    ClassNameAndIndex,
    Coordinates,
    HtmlId,
    PlaywrightSelector,
    NameAndControlTypeAndIndex,     // Name + ControlType + IndexInParent (duplicate name çözümü)
    NameAndParentAndIndex           // Name + Parent + IndexInParent (duplicate name çözümü)
}

/// <summary>
/// Bir görev adımı
/// </summary>
public class TaskStep
{
    public int StepNumber { get; set; }
    public StepType StepType { get; set; }
    public string? Description { get; set; }

    // Tip 1 için
    public TargetInfo? Target { get; set; }

    // Tip 2 için
    public UIElementInfo? UIElement { get; set; }
    public ActionType Action { get; set; }
    public string? KeysToPress { get; set; } // Klavye tuşları
    public string? TextToType { get; set; } // Yazılacak metin
    public int? WaitMilliseconds { get; set; } // Bekleme süresi

    // Element bulma stratejisi (kullanıcının seçtiği)
    public ElementLocatorStrategy? SelectedStrategy { get; set; }

    // Tip 3 için
    public ConditionInfo? Condition { get; set; }

    // Tip 4 için
    public bool IsLoopEnd { get; set; }
    public int? LoopBackToStep { get; set; }
    public bool IsChainEnd { get; set; }

    // Genel
    public int? NextStepNumber { get; set; } // Varsayılan sonraki adım
}

/// <summary>
/// Görev zinciri
/// </summary>
public class TaskChain
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public List<TaskStep> Steps { get; set; } = new();
}
