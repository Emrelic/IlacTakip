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
/// Koşul operatörleri (Tip 3 için)
/// </summary>
public enum ConditionOperator
{
    Equals,           // ==
    NotEquals,        // !=
    Contains,         // Text içinde geçiyor mu
    NotContains,      // Text içinde geçmiyor mu
    StartsWith,       // Text ile başlıyor mu
    EndsWith,         // Text ile bitiyor mu
    GreaterThan,      // > (sayısal)
    LessThan,         // < (sayısal)
    GreaterOrEqual,   // >=
    LessOrEqual,      // <=
    IsTrue,           // Boolean true mu
    IsFalse,          // Boolean false mu
    IsEmpty,          // Boş mu (string/text için)
    IsNotEmpty        // Boş değil mi
}

/// <summary>
/// Mantıksal operatörler (çoklu koşullar için)
/// </summary>
public enum LogicalOperator
{
    None,  // Tek koşul varsa
    AND,   // Ve
    OR     // Veya
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
/// Tek bir UI element koşulu (Tip 3 için)
/// </summary>
public class UICondition
{
    /// <summary>
    /// Kontrol edilecek UI elementi (tam element bilgisi)
    /// </summary>
    public UIElementInfo? Element { get; set; }

    /// <summary>
    /// Kontrol edilecek özellik adı (IsChecked, Text, IsEnabled, IsVisible, Value, vb)
    /// </summary>
    public string PropertyName { get; set; } = "";

    /// <summary>
    /// Karşılaştırma operatörü
    /// </summary>
    public ConditionOperator Operator { get; set; }

    /// <summary>
    /// Beklenen değer (karşılaştırılacak değer)
    /// </summary>
    public string ExpectedValue { get; set; } = "";

    /// <summary>
    /// Bir sonraki koşulla bağlantı (AND/OR/None)
    /// </summary>
    public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.None;

    /// <summary>
    /// Element bulma stratejisi
    /// </summary>
    public ElementLocatorStrategy? LocatorStrategy { get; set; }
}

/// <summary>
/// Dallanma hedefi (Tip 3 için)
/// </summary>
public class BranchTarget
{
    /// <summary>
    /// Dal adı (A, B, C, vb)
    /// </summary>
    public string BranchName { get; set; } = "";

    /// <summary>
    /// Hedef adım ID'si (örn: "6A", "7B")
    /// </summary>
    public string TargetStepId { get; set; } = "";

    /// <summary>
    /// Bu dala gitmek için gereken koşul sonucu
    /// true, false, veya switch-case için özel değer
    /// </summary>
    public string ConditionValue { get; set; } = "";

    /// <summary>
    /// Dal açıklaması
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Koşullu dallanma bilgileri (Tip 3 için - yenilendi)
/// </summary>
public class ConditionInfo
{
    /// <summary>
    /// Hedef sayfa URL'i veya tanımlayıcı bilgisi
    /// </summary>
    public string? PageIdentifier { get; set; }

    /// <summary>
    /// Kontrol edilecek koşullar listesi
    /// </summary>
    public List<UICondition> Conditions { get; set; } = new();

    /// <summary>
    /// Dallanma hedefleri
    /// </summary>
    public List<BranchTarget> Branches { get; set; } = new();

    /// <summary>
    /// Hiçbir koşul tutmazsa varsayılan hedef
    /// </summary>
    public string? DefaultBranchStepId { get; set; }

    /// <summary>
    /// Dallanma tipi: "Boolean" (true/false) veya "SwitchCase" (çoklu dal)
    /// </summary>
    public string BranchType { get; set; } = "Boolean";

    /// <summary>
    /// Döngü sonlanma modu aktif mi?
    /// True: Koşul true ise program sonlanır, false ise döngü devam eder (belirtilen adıma gider)
    /// False: Normal koşullu dallanma davranışı
    /// </summary>
    public bool IsLoopTerminationMode { get; set; } = false;

    /// <summary>
    /// Bir önceki adım numarası (dallanma önerisi için)
    /// </summary>
    public int PreviousStepNumber { get; set; } = 0;
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

    // Smart Element Recorder için
    public RecordedElement? RecordedElement { get; set; } // Kaydedilmiş element bilgisi
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
    NameAndParentAndIndex,          // Name + Parent + IndexInParent (duplicate name çözümü)

    // Smart Element Recorder için yeni tipler
    TableRowIndex,                  // Tablo ID + Satır Index
    TextContent,                    // Hücre text içeriği
    ClassAndName,                   // ClassName + Name kombinasyonu
    ParentNameAndControlType,       // Parent.Name + ControlType kombinasyonu
    ParentAutomationIdAndControlType // Parent.AutomationId + ControlType kombinasyonu
}

/// <summary>
/// Bir görev adımı
/// </summary>
public class TaskStep
{
    /// <summary>
    /// Adım numarası (eski sistem için uyumluluk)
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Adım ID'si - dallanmalar için: "1", "2", "5", "6A", "6B", "7A", "7B", "8"
    /// </summary>
    public string StepId { get; set; } = "";

    public StepType StepType { get; set; }
    public string? Description { get; set; }

    // Tip 1 için
    public TargetInfo? Target { get; set; }

    // Tip 2 için
    public UIElementInfo? UIElement { get; set; }
    public ActionType Action { get; set; }
    public string? KeysToPress { get; set; } // Klavye tuşları
    public string? TextToType { get; set; } // Yazılacak metin
    public int? MouseWheelDelta { get; set; } // Mouse tekerlek delta (120 = yukarı, -120 = aşağı)
    public int? WaitMilliseconds { get; set; } // Bekleme süresi

    // Element bulma stratejisi (kullanıcının seçtiği)
    public ElementLocatorStrategy? SelectedStrategy { get; set; }

    // Tip 3 için
    public ConditionInfo? Condition { get; set; }

    // Tip 4 için
    public bool IsLoopEnd { get; set; }
    public int? LoopBackToStep { get; set; }
    public string? LoopBackToStepId { get; set; } // Dallanma için
    public bool IsChainEnd { get; set; }

    // Genel - Sonraki adım (dallanma yoksa)
    public int? NextStepNumber { get; set; } // Eski sistem için
    public string? NextStepId { get; set; } // Yeni sistem için - dallanma desteği
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

    // Döngüsel görev özellikleri
    public bool IsLooped { get; set; } = false;
    public int LoopStartIndex { get; set; } = 0; // Döngünün başlayacağı adım indexi (0-based)
    public int LoopEndIndex { get; set; } = -1; // Döngünün biteceği adım indexi (0-based)
    public int MaxLoopCount { get; set; } = 100; // Maksimum döngü sayısı (varsayılan: 100)
    public TaskStep? LoopConditionStep { get; set; } // Döngü sonlanma kontrolü adımı
}
