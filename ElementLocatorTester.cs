using System.Diagnostics;
using System.Windows.Automation;

namespace MedulaOtomasyon;

/// <summary>
/// Element bulma stratejilerini oluşturur ve test eder
/// </summary>
public class ElementLocatorTester
{
    /// <summary>
    /// UIElementInfo'dan olası tüm stratejileri oluşturur
    /// </summary>
    public static List<ElementLocatorStrategy> GenerateStrategies(UIElementInfo elementInfo)
    {
        var strategies = new List<ElementLocatorStrategy>();

        // 1. AutomationId (en stabil)
        if (!string.IsNullOrEmpty(elementInfo.AutomationId))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "AutomationId",
                Description = $"AutomationId: '{elementInfo.AutomationId}'",
                Type = LocatorType.AutomationId,
                Properties = new Dictionary<string, string>
                {
                    ["AutomationId"] = elementInfo.AutomationId
                }
            });

            // AutomationId + ControlType
            if (!string.IsNullOrEmpty(elementInfo.ControlType))
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "AutomationId + ControlType",
                    Description = $"AutomationId: '{elementInfo.AutomationId}' + ControlType: '{elementInfo.ControlType}'",
                    Type = LocatorType.AutomationIdAndControlType,
                    Properties = new Dictionary<string, string>
                    {
                        ["AutomationId"] = elementInfo.AutomationId,
                        ["ControlType"] = elementInfo.ControlType
                    }
                });
            }
        }

        // 2. Name
        if (!string.IsNullOrEmpty(elementInfo.Name))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "Name",
                Description = $"Name: '{elementInfo.Name}'",
                Type = LocatorType.Name,
                Properties = new Dictionary<string, string>
                {
                    ["Name"] = elementInfo.Name
                }
            });

            // Name + ControlType
            if (!string.IsNullOrEmpty(elementInfo.ControlType))
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "Name + ControlType",
                    Description = $"Name: '{elementInfo.Name}' + ControlType: '{elementInfo.ControlType}'",
                    Type = LocatorType.NameAndControlType,
                    Properties = new Dictionary<string, string>
                    {
                        ["Name"] = elementInfo.Name,
                        ["ControlType"] = elementInfo.ControlType
                    }
                });

                // Name + ControlType + IndexInParent (duplicate name çözümü)
                if (elementInfo.IndexInParent.HasValue)
                {
                    strategies.Add(new ElementLocatorStrategy
                    {
                        Name = "Name + ControlType + Index",
                        Description = $"Name: '{elementInfo.Name}' + ControlType: '{elementInfo.ControlType}' + Index: {elementInfo.IndexInParent}",
                        Type = LocatorType.NameAndControlTypeAndIndex,
                        Properties = new Dictionary<string, string>
                        {
                            ["Name"] = elementInfo.Name,
                            ["ControlType"] = elementInfo.ControlType,
                            ["Index"] = elementInfo.IndexInParent.Value.ToString()
                        }
                    });
                }
            }

            // Name + Parent.Name
            if (!string.IsNullOrEmpty(elementInfo.ParentName))
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "Name + Parent.Name",
                    Description = $"Name: '{elementInfo.Name}' + Parent.Name: '{elementInfo.ParentName}'",
                    Type = LocatorType.NameAndParent,
                    Properties = new Dictionary<string, string>
                    {
                        ["Name"] = elementInfo.Name,
                        ["ParentName"] = elementInfo.ParentName
                    }
                });

                // Name + Parent + IndexInParent (duplicate name çözümü)
                if (elementInfo.IndexInParent.HasValue)
                {
                    strategies.Add(new ElementLocatorStrategy
                    {
                        Name = "Name + Parent + Index",
                        Description = $"Name: '{elementInfo.Name}' + Parent: '{elementInfo.ParentName}' + Index: {elementInfo.IndexInParent}",
                        Type = LocatorType.NameAndParentAndIndex,
                        Properties = new Dictionary<string, string>
                        {
                            ["Name"] = elementInfo.Name,
                            ["ParentName"] = elementInfo.ParentName,
                            ["Index"] = elementInfo.IndexInParent.Value.ToString()
                        }
                    });
                }
            }
        }

        // 3. ClassName
        if (!string.IsNullOrEmpty(elementInfo.ClassName))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "ClassName",
                Description = $"ClassName: '{elementInfo.ClassName}'",
                Type = LocatorType.ClassName,
                Properties = new Dictionary<string, string>
                {
                    ["ClassName"] = elementInfo.ClassName
                }
            });

            // ClassName + Index
            if (elementInfo.IndexInParent.HasValue)
            {
                strategies.Add(new ElementLocatorStrategy
                {
                    Name = "ClassName + Index",
                    Description = $"ClassName: '{elementInfo.ClassName}' + Index: {elementInfo.IndexInParent}",
                    Type = LocatorType.ClassNameAndIndex,
                    Properties = new Dictionary<string, string>
                    {
                        ["ClassName"] = elementInfo.ClassName,
                        ["Index"] = elementInfo.IndexInParent.Value.ToString()
                    }
                });
            }
        }

        // 4. ElementPath
        if (!string.IsNullOrEmpty(elementInfo.ElementPath))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "ElementPath",
                Description = $"ElementPath: '{elementInfo.ElementPath}'",
                Type = LocatorType.ElementPath,
                Properties = new Dictionary<string, string>
                {
                    ["ElementPath"] = elementInfo.ElementPath
                }
            });
        }

        // 5. TreePath
        if (!string.IsNullOrEmpty(elementInfo.TreePath))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "TreePath",
                Description = $"TreePath: '{elementInfo.TreePath}'",
                Type = LocatorType.TreePath,
                Properties = new Dictionary<string, string>
                {
                    ["TreePath"] = elementInfo.TreePath
                }
            });
        }

        // 6. XPath (Web için)
        if (!string.IsNullOrEmpty(elementInfo.XPath))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "XPath",
                Description = $"XPath: '{elementInfo.XPath}'",
                Type = LocatorType.XPath,
                Properties = new Dictionary<string, string>
                {
                    ["XPath"] = elementInfo.XPath
                }
            });
        }

        // 7. CSS Selector (Web için)
        if (!string.IsNullOrEmpty(elementInfo.CssSelector))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "CSS Selector",
                Description = $"CSS: '{elementInfo.CssSelector}'",
                Type = LocatorType.CssSelector,
                Properties = new Dictionary<string, string>
                {
                    ["CssSelector"] = elementInfo.CssSelector
                }
            });
        }

        // 8. HTML ID (Web için)
        if (!string.IsNullOrEmpty(elementInfo.HtmlId))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "HTML Id",
                Description = $"HTML Id: '{elementInfo.HtmlId}'",
                Type = LocatorType.HtmlId,
                Properties = new Dictionary<string, string>
                {
                    ["HtmlId"] = elementInfo.HtmlId
                }
            });
        }

        // 9. Playwright Selectors (JSON format - Smart Element Recorder)
        if (elementInfo.OtherAttributes != null &&
            elementInfo.OtherAttributes.TryGetValue("PlaywrightSelectorsJson", out var selectorsJson) &&
            !string.IsNullOrEmpty(selectorsJson))
        {
            try
            {
                var selectors = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(selectorsJson);
                if (selectors != null)
                {
                    // RowIndex bilgisi al
                    int? rowIndex = null;
                    if (elementInfo.OtherAttributes.TryGetValue("PlaywrightRowIndex", out var rowIndexStr) &&
                        int.TryParse(rowIndexStr, out var rowIndexValue))
                    {
                        rowIndex = rowIndexValue;
                    }

                    // TableSelector bilgisi al
                    string? tableSelector = null;
                    elementInfo.OtherAttributes.TryGetValue("PlaywrightTableSelector", out tableSelector);

                    // Her selector için bir strateji oluştur
                    foreach (var kvp in selectors)
                    {
                        if (string.IsNullOrWhiteSpace(kvp.Value))
                            continue;

                        var properties = new Dictionary<string, string>
                        {
                            ["SelectorKind"] = kvp.Key,
                            ["Selector"] = kvp.Value
                        };

                        if (rowIndex.HasValue && rowIndex.Value >= 0)
                        {
                            properties["RowIndex"] = rowIndex.Value.ToString();
                        }

                        if (!string.IsNullOrEmpty(tableSelector))
                        {
                            properties["TableSelector"] = tableSelector;
                        }

                        strategies.Add(new ElementLocatorStrategy
                        {
                            Name = $"Playwright {kvp.Key}",
                            Description = $"Playwright selector ({kvp.Key}): {kvp.Value}",
                            Type = LocatorType.PlaywrightSelector,
                            Properties = properties
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[GenerateStrategies] PlaywrightSelectorsJson parse hatası: {ex.Message}");
            }
        }
        // Fallback: Eski format PlaywrightSelector (backward compatibility)
        else if (!string.IsNullOrEmpty(elementInfo.PlaywrightSelector))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "Playwright Selector",
                Description = $"Playwright: '{elementInfo.PlaywrightSelector}'",
                Type = LocatorType.PlaywrightSelector,
                Properties = new Dictionary<string, string>
                {
                    ["Selector"] = elementInfo.PlaywrightSelector
                }
            });
        }

        // 10. Koordinat (son çare)
        if (elementInfo.X.HasValue && elementInfo.Y.HasValue)
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "Koordinat (Son Çare)",
                Description = $"X: {elementInfo.X}, Y: {elementInfo.Y}",
                Type = LocatorType.Coordinates,
                Properties = new Dictionary<string, string>
                {
                    ["X"] = elementInfo.X.Value.ToString(),
                    ["Y"] = elementInfo.Y.Value.ToString()
                }
            });
        }

        return strategies;
    }

    /// <summary>
    /// Bir stratejiyi test eder (elementi bulabilir mi + ne kadar sürede)
    /// </summary>
    public static async Task<ElementLocatorStrategy> TestStrategy(ElementLocatorStrategy strategy, UIElementInfo? elementInfo = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var element = await Task.Run(() => FindElementByStrategy(strategy, elementInfo));

            stopwatch.Stop();
            strategy.TestDurationMs = (int)stopwatch.ElapsedMilliseconds;
            strategy.IsSuccessful = element != null;

            if (!strategy.IsSuccessful)
            {
                strategy.ErrorMessage = "Element bulunamadı";

                // Debug bilgisi ekle
                if (elementInfo != null)
                {
                    strategy.ErrorMessage += $" | Window: {elementInfo.WindowTitle ?? "N/A"}";
                }
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            strategy.TestDurationMs = (int)stopwatch.ElapsedMilliseconds;
            strategy.IsSuccessful = false;
            strategy.ErrorMessage = ex.Message;
        }

        return strategy;
    }

    /// <summary>
    /// Strateji kullanarak elementi bulmayı dener
    /// MULTI-LEVEL FALLBACK: Container → Window → RootElement
    /// </summary>
    public static AutomationElement? FindElementByStrategy(ElementLocatorStrategy strategy, UIElementInfo? elementInfo = null)
    {
        DebugLogger.LogBlankLine();
        DebugLogger.Log("[ElementLocator] ===== ARAMA BAŞLIYOR =====");
        DebugLogger.Log($"[ElementLocator] Strateji: {strategy.Name}");

        // Container bilgisi varsa önce container'ı bul
        AutomationElement? targetContainer = null;
        AutomationElement? targetWindow = null;

        if (elementInfo != null)
        {
            DebugLogger.Log("[ElementLocator] ElementInfo mevcut:");
            DebugLogger.Log($"  - WindowTitle: {elementInfo.WindowTitle ?? "N/A"}");
            DebugLogger.Log($"  - WindowProcessId: {elementInfo.WindowProcessId?.ToString() ?? "N/A"}");
            DebugLogger.Log($"  - ContainerControlType: {elementInfo.ContainerControlType ?? "N/A"}");
            DebugLogger.Log($"  - ContainerName: {elementInfo.ContainerName ?? "N/A"}");

            // 1. ÖNCE WINDOW BUL
            targetWindow = FindTargetWindow(elementInfo);
            if (targetWindow != null)
            {
                DebugLogger.Log($"[ElementLocator] ✓ Hedef Window bulundu: {targetWindow.Current.Name}");

                // 2. CONTAINER BİLGİSİ VARSA CONTAINER'I BUL (Window içinde ara)
                targetContainer = FindTargetContainer(elementInfo, targetWindow);
                if (targetContainer != null)
                {
                    DebugLogger.Log($"[ElementLocator] ✓ Hedef Container bulundu: {targetContainer.Current.ControlType.ProgrammaticName} - {targetContainer.Current.Name}");
                }
                else if (!string.IsNullOrEmpty(elementInfo.ContainerControlType))
                {
                    DebugLogger.Log($"[ElementLocator] ⚠ Container bilgisi var ama bulunamadı: {elementInfo.ContainerControlType}");
                }
            }
            else if (!string.IsNullOrEmpty(elementInfo.WindowTitle))
            {
                DebugLogger.Log($"[ElementLocator] ⚠ Hedef Window bulunamadı: {elementInfo.WindowTitle}");
            }
        }

        // 3. ARAMA STRATEJİSİ: Container → Window → RootElement (Multi-level fallback)
        AutomationElement? foundElement = null;

        // Level 1: Container içinde ara (en dar kapsam - en hızlı)
        if (targetContainer != null)
        {
            DebugLogger.Log("[ElementLocator] [Level 1] Container içinde aranıyor...");
            foundElement = FindByStrategyInScope(strategy, targetContainer);
            if (foundElement != null)
            {
                DebugLogger.Log("[ElementLocator] ✅ Container içinde BULUNDU!");
                return foundElement;
            }
            DebugLogger.Log("[ElementLocator] ⚠ Container içinde bulunamadı, Window'da aranacak...");
        }

        // Level 2: Window içinde ara (orta kapsam)
        if (targetWindow != null)
        {
            DebugLogger.Log("[ElementLocator] [Level 2] Window içinde aranıyor...");
            foundElement = FindByStrategyInScope(strategy, targetWindow);
            if (foundElement != null)
            {
                DebugLogger.Log("[ElementLocator] ✅ Window içinde BULUNDU!");
                return foundElement;
            }
            DebugLogger.Log("[ElementLocator] ⚠ Window içinde bulunamadı, RootElement'te aranacak...");
        }

        // Level 3: RootElement'te ara (fallback - tüm desktop)
        DebugLogger.Log("[ElementLocator] [Level 3] RootElement (Desktop) içinde aranıyor...");
        foundElement = FindByStrategyInScope(strategy, AutomationElement.RootElement);
        if (foundElement != null)
        {
            DebugLogger.Log("[ElementLocator] ✅ RootElement içinde BULUNDU!");
            return foundElement;
        }

        DebugLogger.Log("[ElementLocator] ❌ Hiçbir yerde bulunamadı!");
        return null;
    }

    /// <summary>
    /// Belirtilen scope içinde stratejiyi uygular
    /// </summary>
    private static AutomationElement? FindByStrategyInScope(ElementLocatorStrategy strategy, AutomationElement searchRoot)
    {
        // Debug: Hangi değer aranıyor?
        string searchValue = "";
        if (strategy.Properties.Count > 0)
        {
            searchValue = string.Join(", ", strategy.Properties.Select(p => $"{p.Key}={p.Value}"));
            DebugLogger.Log($"  → Aranan değer: {searchValue}");
        }

        switch (strategy.Type)
        {
            case LocatorType.AutomationId:
                return FindByAutomationId(strategy.Properties["AutomationId"], searchRoot);

            case LocatorType.Name:
                return FindByName(strategy.Properties["Name"], searchRoot);

            case LocatorType.ClassName:
                return FindByClassName(strategy.Properties["ClassName"], searchRoot);

            case LocatorType.AutomationIdAndControlType:
                return FindByAutomationIdAndControlType(
                    strategy.Properties["AutomationId"],
                    strategy.Properties["ControlType"], searchRoot);

            case LocatorType.NameAndControlType:
                return FindByNameAndControlType(
                    strategy.Properties["Name"],
                    strategy.Properties["ControlType"], searchRoot);

            case LocatorType.NameAndParent:
                return FindByNameAndParent(
                    strategy.Properties["Name"],
                    strategy.Properties["ParentName"], searchRoot);

            case LocatorType.NameAndControlTypeAndIndex:
                return FindByNameAndControlTypeAndIndex(
                    strategy.Properties["Name"],
                    strategy.Properties["ControlType"],
                    int.Parse(strategy.Properties["Index"]), searchRoot);

            case LocatorType.NameAndParentAndIndex:
                return FindByNameAndParentAndIndex(
                    strategy.Properties["Name"],
                    strategy.Properties["ParentName"],
                    int.Parse(strategy.Properties["Index"]), searchRoot);

            case LocatorType.ClassNameAndIndex:
                return FindByClassNameAndIndex(
                    strategy.Properties["ClassName"],
                    int.Parse(strategy.Properties["Index"]), searchRoot);

            case LocatorType.ElementPath:
                return FindByElementPath(strategy.Properties["ElementPath"], searchRoot);

            case LocatorType.TreePath:
                return FindByTreePath(strategy.Properties["TreePath"], searchRoot);

            case LocatorType.XPath:
                return FindByXPath(strategy.Properties["XPath"], searchRoot);

            case LocatorType.CssSelector:
                return FindByCssSelector(strategy.Properties["CssSelector"], searchRoot);

            case LocatorType.HtmlId:
                return FindByHtmlId(strategy.Properties["HtmlId"], searchRoot);

            case LocatorType.PlaywrightSelector:
                return FindByPlaywrightSelector(strategy, searchRoot);

            case LocatorType.Coordinates:
                // Koordinat araması searchRoot'tan bağımsızdır
                return FindByCoordinates(
                    int.Parse(strategy.Properties["X"]),
                    int.Parse(strategy.Properties["Y"]));

            default:
                return null;
        }
    }

    /// <summary>
    /// UIElementInfo'dan hedef container'ı bulur (Window içinde)
    /// </summary>
    private static AutomationElement? FindTargetContainer(UIElementInfo elementInfo, AutomationElement targetWindow)
    {
        try
        {
            // Container bilgisi yoksa null dön
            if (string.IsNullOrEmpty(elementInfo.ContainerControlType))
            {
                return null;
            }

            DebugLogger.Log($"[FindTargetContainer] Container aranıyor: {elementInfo.ContainerControlType}");

            // Container ControlType'ı parse et
            var containerControlType = ParseControlType(elementInfo.ContainerControlType);
            if (containerControlType == null)
            {
                DebugLogger.Log($"[FindTargetContainer] ControlType parse edilemedi: {elementInfo.ContainerControlType}");
                return null;
            }

            // *** YENİ STRATEJI 1: INDEX BAZLI HIZLI ARAMA (Direct Children Only) ***
            // Yapı analizinde container'ın Window'un direct child'ı olduğunu gördük
            // TreeScope.Descendants yerine TreeScope.Children kullanarak çok daha hızlı arama yapabiliriz
            if (!string.IsNullOrEmpty(elementInfo.ContainerAutomationId) || !string.IsNullOrEmpty(elementInfo.ContainerName))
            {
                DebugLogger.Log("[FindTargetContainer] 🚀 INDEX bazlı hızlı arama yapılıyor (Direct Children Only)...");
                try
                {
                    var children = FindAllWithRawView(targetWindow, TreeScope.Children, Condition.TrueCondition);
                    DebugLogger.Log($"[FindTargetContainer] Window'un {(children?.Count ?? 0)} direct child'ı bulundu");
                    if (children == null) return targetWindow;

                    int index = 0;
                    foreach (AutomationElement child in children)
                    {
                        try
                        {
                            // AutomationId match?
                            if (!string.IsNullOrEmpty(elementInfo.ContainerAutomationId) &&
                                child.Current.AutomationId == elementInfo.ContainerAutomationId &&
                                child.Current.ControlType == containerControlType)
                            {
                                DebugLogger.Log($"[FindTargetContainer] ✅ INDEX bazlı arama ile BULUNDU! (Index: {index}, AutomationId: {elementInfo.ContainerAutomationId})");
                                return child;
                            }

                            // Name match?
                            if (!string.IsNullOrEmpty(elementInfo.ContainerName) &&
                                child.Current.Name == elementInfo.ContainerName &&
                                child.Current.ControlType == containerControlType)
                            {
                                DebugLogger.Log($"[FindTargetContainer] ✅ INDEX bazlı arama ile BULUNDU! (Index: {index}, Name: {elementInfo.ContainerName})");
                                return child;
                            }
                        }
                        catch (Exception ex)
                        {
                            DebugLogger.Log($"[FindTargetContainer] ⚠ Child {index} okunamadı: {ex.Message}");
                        }
                        index++;
                    }

                    DebugLogger.Log("[FindTargetContainer] ⚠ INDEX bazlı aramada bulunamadı, yavaş aramaya geçiliyor...");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[FindTargetContainer] INDEX arama hatası: {ex.Message}");
                }
            }

            // *** ESKİ STRATEJI: YAVAŞ RECURSIVE ARAMA (Fallback) ***
            // AutomationId ile ara (en güvenilir)
            if (!string.IsNullOrEmpty(elementInfo.ContainerAutomationId))
            {
                DebugLogger.Log($"[FindTargetContainer] ContainerAutomationId ile aranıyor: {elementInfo.ContainerAutomationId}");
                var condition = new AndCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, elementInfo.ContainerAutomationId),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, containerControlType)
                );
                var container = FindWithTimeout(targetWindow, TreeScope.Descendants, condition, 5000);
                if (container != null)
                {
                    DebugLogger.Log("[FindTargetContainer] ✓ AutomationId ile bulundu");
                    return container;
                }
                else
                {
                    DebugLogger.Log("[FindTargetContainer] ✗ AutomationId ile bulunamadı");
                }
            }

            // Name ile ara
            if (!string.IsNullOrEmpty(elementInfo.ContainerName))
            {
                DebugLogger.Log($"[FindTargetContainer] ContainerName ile aranıyor: {elementInfo.ContainerName}");
                var condition = new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, elementInfo.ContainerName),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, containerControlType)
                );
                var container = FindWithTimeout(targetWindow, TreeScope.Descendants, condition, 5000);
                if (container != null)
                {
                    DebugLogger.Log("[FindTargetContainer] ✓ Name ile bulundu");
                    return container;
                }
                else
                {
                    DebugLogger.Log("[FindTargetContainer] ✗ Name ile bulunamadı");
                }
            }

            // ClassName ile ara (SON ÇARE - ancak güvenilir değil!)
            // WinForms'da ClassName çok generic olabilir, yanlış container bulabilir
            if (!string.IsNullOrEmpty(elementInfo.ContainerClassName))
            {
                DebugLogger.Log($"[FindTargetContainer] ⚠ ClassName ile arama GÜVENİLİR DEĞİL (çok generic)");
                DebugLogger.Log($"[FindTargetContainer] ClassName: {elementInfo.ContainerClassName}");

                // WinForms generic class name'leri atla
                if (elementInfo.ContainerClassName.StartsWith("WindowsForms10."))
                {
                    DebugLogger.Log("[FindTargetContainer] ✗ WinForms generic ClassName - GÜVENİLİR DEĞİL, atlanıyor");
                }
                else
                {
                    // Sadece unique ClassName'ler için ara
                    var condition = new AndCondition(
                        new PropertyCondition(AutomationElement.ClassNameProperty, elementInfo.ContainerClassName),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, containerControlType)
                    );
                    var container = FindWithTimeout(targetWindow, TreeScope.Descendants, condition, 2000);
                    if (container != null)
                    {
                        DebugLogger.Log("[FindTargetContainer] ✓ ClassName ile bulundu (unique ClassName)");
                        return container;
                    }
                }
            }

            DebugLogger.Log("[FindTargetContainer] ✗ Container bulunamadı (doğrudan Window'da aranacak)");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[FindTargetContainer] Exception: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// UIElementInfo'dan hedef pencereyi bulur
    /// </summary>
    /// <summary>
    /// Element'in kendi programımıza ait olup olmadığını kontrol eder
    /// </summary>
    private static bool IsSelfProgramElement(AutomationElement element)
    {
        try
        {
            var currentProcessId = Process.GetCurrentProcess().Id;
            var elementProcessId = element.Current.ProcessId;

            if (elementProcessId == currentProcessId)
            {
                DebugLogger.Log($"[FILTER] ⚠ Kendi programımızın elementi tespit edildi, filtreleniyor: {element.Current.Name}");
                return true;
            }

            // Window kontrolü - pencere adı kontrolü
            var windowName = element.Current.Name ?? string.Empty;
            if (windowName.Contains("Görev Zinciri", StringComparison.OrdinalIgnoreCase) ||
                windowName.Contains("Görev Kaydedici", StringComparison.OrdinalIgnoreCase) ||
                windowName.Contains("TaskChain", StringComparison.OrdinalIgnoreCase))
            {
                DebugLogger.Log($"[FILTER] ⚠ Kendi programımızın penceresi tespit edildi (isim), filtreleniyor: {windowName}");
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static AutomationElement? FindTargetWindow(UIElementInfo elementInfo)
    {
        try
        {
            var currentProcessId = Process.GetCurrentProcess().Id;

            if (!string.IsNullOrEmpty(elementInfo.WindowTitle))
            {
                DebugLogger.Log($"[FindTargetWindow] WindowTitle ile arama: {elementInfo.WindowTitle}");
                var condition = new PropertyCondition(AutomationElement.NameProperty, elementInfo.WindowTitle);
                var window = AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);

                if (window != null)
                {
                    // Kendi programımızı filtrele
                    if (IsSelfProgramElement(window))
                    {
                        DebugLogger.Log($"[FindTargetWindow] ✗ Kendi programımız, filtrelendi!");
                        return null;
                    }

                    DebugLogger.Log($"[FindTargetWindow] ✓ Pencere bulundu: {window.Current.Name}");
                }
                else
                {
                    DebugLogger.Log($"[FindTargetWindow] ✗ Pencere bulunamadı: {elementInfo.WindowTitle}");
                }

                return window;
            }
            else if (elementInfo.WindowProcessId.HasValue)
            {
                DebugLogger.Log($"[FindTargetWindow] ProcessId ile arama: {elementInfo.WindowProcessId.Value}");

                // Kendi processimizi filtrele
                if (elementInfo.WindowProcessId.Value == currentProcessId)
                {
                    DebugLogger.Log($"[FindTargetWindow] ✗ Kendi programımızın ProcessId'si, filtrelendi!");
                    return null;
                }

                var windows = FindAllWithRawView(AutomationElement.RootElement, TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
                if (windows == null) return null;

                foreach (AutomationElement window in windows)
                {
                    try
                    {
                        if (window.Current.ProcessId == elementInfo.WindowProcessId.Value)
                        {
                            DebugLogger.Log($"[FindTargetWindow] ✓ Pencere bulundu (ProcessId): {window.Current.Name}");
                            return window;
                        }
                    }
                    catch { }
                }
                DebugLogger.Log("[FindTargetWindow] ✗ ProcessId ile pencere bulunamadı");
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[FindTargetWindow] Exception: {ex.Message}");
        }

        return null;
    }

    #region Find Methods

    /// <summary>
    /// Timeout ile element arama (3 saniye) - Kendi programımızın elementlerini filtreler
    /// </summary>
    private static AutomationElement? FindWithTimeout(AutomationElement root, TreeScope scope, Condition condition, int timeoutMs = 3000)
    {
        var stopwatch = Stopwatch.StartNew();
        AutomationElement? result = null;

        try
        {
            var task = Task.Run(() =>
            {
                try
                {
                    // RawView kullan - IsContentElement: false elementleri de bul
                    // TitleBar butonları gibi UI elementleri için gerekli
                    var cacheRequest = new CacheRequest();
                    cacheRequest.TreeFilter = Automation.RawViewCondition;
                    cacheRequest.Add(AutomationElement.NameProperty);
                    cacheRequest.Add(AutomationElement.AutomationIdProperty);
                    cacheRequest.Add(AutomationElement.ControlTypeProperty);

                    using (cacheRequest.Activate())
                    {
                        return root.FindFirst(scope, condition);
                    }
                }
                catch
                {
                    return null;
                }
            });

            if (task.Wait(timeoutMs))
            {
                result = task.Result;

                // Kendi programımızın elementlerini filtrele
                if (result != null && IsSelfProgramElement(result))
                {
                    DebugLogger.Log($"    [FindWithTimeout] ⚠ Element kendi programımıza ait, filtrelendi: {result.Current.Name}");
                    result = null;
                }
            }
        }
        catch { }

        stopwatch.Stop();
        return result;
    }

    /// <summary>
    /// RawView ile FindAll - IsContentElement: false elementleri de bulur
    /// </summary>
    private static AutomationElementCollection? FindAllWithRawView(AutomationElement root, TreeScope scope, Condition condition)
    {
        try
        {
            var cacheRequest = new CacheRequest();
            cacheRequest.TreeFilter = Automation.RawViewCondition;
            cacheRequest.Add(AutomationElement.NameProperty);
            cacheRequest.Add(AutomationElement.AutomationIdProperty);
            cacheRequest.Add(AutomationElement.ControlTypeProperty);

            using (cacheRequest.Activate())
            {
                return root.FindAll(scope, condition);
            }
        }
        catch
        {
            return null;
        }
    }

    private static AutomationElement? FindByAutomationId(string automationId, AutomationElement searchRoot)
    {
        DebugLogger.Log($"    [FindByAutomationId] Aranıyor: '{automationId}'");
        var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
        var result = FindWithTimeout(searchRoot, TreeScope.Descendants, condition);
        DebugLogger.Log($"    [FindByAutomationId] Sonuç: {(result != null ? "BULUNDU ✓" : "BULUNAMADI ✗")}");
        return result;
    }

    private static AutomationElement? FindByName(string name, AutomationElement searchRoot)
    {
        DebugLogger.Log($"    [FindByName] Aranıyor: '{name}'");
        var condition = new PropertyCondition(AutomationElement.NameProperty, name);
        var result = FindWithTimeout(searchRoot, TreeScope.Descendants, condition);
        DebugLogger.Log($"    [FindByName] Sonuç: {(result != null ? "BULUNDU ✓" : "BULUNAMADI ✗")}");
        return result;
    }

    private static AutomationElement? FindByClassName(string className, AutomationElement searchRoot)
    {
        var condition = new PropertyCondition(AutomationElement.ClassNameProperty, className);
        return FindWithTimeout(searchRoot, TreeScope.Descendants, condition);
    }

    private static AutomationElement? FindByAutomationIdAndControlType(string automationId, string controlTypeStr, AutomationElement searchRoot)
    {
        var controlType = ParseControlType(controlTypeStr);
        if (controlType == null) return null;

        var condition = new AndCondition(
            new PropertyCondition(AutomationElement.AutomationIdProperty, automationId),
            new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)
        );
        return FindWithTimeout(searchRoot, TreeScope.Descendants, condition);
    }

    private static AutomationElement? FindByNameAndControlType(string name, string controlTypeStr, AutomationElement searchRoot)
    {
        var controlType = ParseControlType(controlTypeStr);
        if (controlType == null) return null;

        var condition = new AndCondition(
            new PropertyCondition(AutomationElement.NameProperty, name),
            new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)
        );
        return FindWithTimeout(searchRoot, TreeScope.Descendants, condition);
    }

    private static AutomationElement? FindByNameAndParent(string name, string parentName, AutomationElement searchRoot)
    {
        // Önce parent'ı bul
        var parentCondition = new PropertyCondition(AutomationElement.NameProperty, parentName);
        var parent = FindWithTimeout(searchRoot, TreeScope.Descendants, parentCondition);

        if (parent == null) return null;

        // Parent içinde child'ı bul
        var childCondition = new PropertyCondition(AutomationElement.NameProperty, name);
        return FindWithTimeout(parent, TreeScope.Descendants, childCondition);
    }

    private static AutomationElement? FindByNameAndControlTypeAndIndex(string name, string controlTypeStr, int index, AutomationElement searchRoot)
    {
        // Name + ControlType + IndexInParent ile bul
        // Bu duplicate name problemini çözer (örn: birden fazla "1" button varsa)
        try
        {
            var controlType = ParseControlType(controlTypeStr);
            if (controlType == null) return null;

            var condition = new AndCondition(
                new PropertyCondition(AutomationElement.NameProperty, name),
                new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)
            );

            // Tüm matching elementleri bul - RawView kullan
            var task = Task.Run(() =>
            {
                try
                {
                    return FindAllWithRawView(searchRoot, TreeScope.Descendants, condition);
                }
                catch
                {
                    return null;
                }
            });

            if (task.Wait(3000) && task.Result != null)
            {
                var elements = task.Result;

                // Her element için IndexInParent kontrol et
                foreach (AutomationElement element in elements)
                {
                    try
                    {
                        var parent = TreeWalker.RawViewWalker.GetParent(element);
                        if (parent != null)
                        {
                            var siblings = FindAllWithRawView(parent, TreeScope.Children, Condition.TrueCondition);
                            if (siblings == null) continue;
                            for (int i = 0; i < siblings.Count; i++)
                            {
                                if (Automation.Compare(siblings[i], element))
                                {
                                    if (i == index)
                                    {
                                        return element; // Index eşleşti!
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }

        return null;
    }

    private static AutomationElement? FindByNameAndParentAndIndex(string name, string parentName, int index, AutomationElement searchRoot)
    {
        // Name + Parent + IndexInParent ile bul
        // Bu duplicate name problemini çözer (örn: birden fazla "1" varsa ama farklı parent'larda)
        try
        {
            // Önce parent'ı bul
            var parentCondition = new PropertyCondition(AutomationElement.NameProperty, parentName);
            var parent = FindWithTimeout(searchRoot, TreeScope.Descendants, parentCondition);

            if (parent == null) return null;

            // Parent'ın children'ını al - RawView kullan
            var children = FindAllWithRawView(parent, TreeScope.Children, Condition.TrueCondition);
            if (children == null) return null;

            // Index'teki child'ı bul ve name kontrolü yap
            foreach (AutomationElement child in children)
            {
                try
                {
                    // Child'ın indexini bul
                    var childParent = TreeWalker.RawViewWalker.GetParent(child);
                    if (childParent != null && Automation.Compare(childParent, parent))
                    {
                        var siblings = FindAllWithRawView(childParent, TreeScope.Children, Condition.TrueCondition);
                        if (siblings == null) continue;
                        for (int i = 0; i < siblings.Count; i++)
                        {
                            if (Automation.Compare(siblings[i], child))
                            {
                                if (i == index && child.Current.Name == name)
                                {
                                    return child; // Index ve Name eşleşti!
                                }
                                break;
                            }
                        }
                    }
                }
                catch { }
            }
        }
        catch { }

        return null;
    }

    private static ControlType? ParseControlType(string controlTypeStr)
    {
        // "ControlType.Button" formatından "Button" çıkar
        var typeName = controlTypeStr.Replace("ControlType.", "");

        return typeName switch
        {
            "Button" => ControlType.Button,
            "Text" => ControlType.Text,
            "Edit" => ControlType.Edit,
            "ComboBox" => ControlType.ComboBox,
            "ListItem" => ControlType.ListItem,
            "List" => ControlType.List,
            "CheckBox" => ControlType.CheckBox,
            "RadioButton" => ControlType.RadioButton,
            "Window" => ControlType.Window,
            "Pane" => ControlType.Pane,
            "MenuItem" => ControlType.MenuItem,
            "DataItem" => ControlType.DataItem,
            _ => null
        };
    }

    private static AutomationElement? FindByClassNameAndIndex(string className, int index, AutomationElement searchRoot)
    {
        try
        {
            var condition = new PropertyCondition(AutomationElement.ClassNameProperty, className);
            var task = Task.Run(() =>
            {
                try
                {
                    return FindAllWithRawView(searchRoot, TreeScope.Descendants, condition);
                }
                catch
                {
                    return null;
                }
            });

            if (task.Wait(3000) && task.Result != null)
            {
                var elements = task.Result;
                if (index < elements.Count)
                {
                    return elements[index];
                }
            }
        }
        catch { }

        return null;
    }

    private static AutomationElement? FindByElementPath(string elementPath, AutomationElement searchRoot)
    {
        // ElementPath formatı: "Window[Title]/Pane/Button[Name]"
        // Basit implementasyon: Name ile bul
        try
        {
            var parts = elementPath.Split('/');
            if (parts.Length > 0)
            {
                var lastPart = parts[^1];
                var match = System.Text.RegularExpressions.Regex.Match(lastPart, @"\[([^\]]+)\]");
                if (match.Success)
                {
                    return FindByName(match.Groups[1].Value, searchRoot);
                }
            }
        }
        catch { }

        return null;
    }

    private static AutomationElement? FindByTreePath(string treePath, AutomationElement searchRoot)
    {
        // TreePath formatı: "0/2/5/1" (index bazlı)
        // Yeni format: "0/2/?[Pane]/1" (index bulunamazsa ControlType ile fallback)
        // TreePath RootElement'ten başlar, searchRoot parametresi ignore edilir
        try
        {
            if (string.IsNullOrWhiteSpace(treePath))
                return null;

            var pathParts = treePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length == 0)
            {
                return null;
            }

            // RootElement'ten başla (TreePath her zaman RootElement'ten oluşturulur)
            var current = AutomationElement.RootElement;

            // Her path part ile child navigate et
            for (int i = 0; i < pathParts.Length; i++)
            {
                var part = pathParts[i];

                // "?" ile başlıyorsa - ControlType fallback
                if (part.StartsWith("?"))
                {
                    // Format: "?[ControlType]"
                    var controlTypeMatch = System.Text.RegularExpressions.Regex.Match(part, @"\?\[([^\]]+)\]");
                    if (controlTypeMatch.Success)
                    {
                        var controlType = controlTypeMatch.Groups[1].Value;

                        // Bu seviyeyi ControlType ile bul (ilk eşleşeni al) - RawView kullan
                        var children = FindAllWithRawView(current, TreeScope.Children, Condition.TrueCondition);
                        if (children == null) return null;
                        bool found = false;

                        foreach (AutomationElement child in children)
                        {
                            try
                            {
                                var childControlType = child.Current.ControlType.ProgrammaticName
                                    .Replace("ControlType.", "");

                                if (string.Equals(childControlType, controlType, StringComparison.OrdinalIgnoreCase))
                                {
                                    current = child;
                                    found = true;
                                    break;
                                }
                            }
                            catch { }
                        }

                        if (!found)
                        {
                            // ControlType eşleşmedi - navigation başarısız
                            return null;
                        }
                    }
                    else
                    {
                        // "?" tek başına - navigation yapılamaz
                        return null;
                    }
                }
                else
                {
                    // Sayısal index
                    if (!int.TryParse(part, out int index))
                    {
                        return null;
                    }

                    var children = FindAllWithRawView(current, TreeScope.Children, Condition.TrueCondition);
                    if (children == null) return null;

                    if (index >= children.Count || index < 0)
                    {
                        // Index out of range - element bulunamadı
                        return null;
                    }

                    current = children[index];
                }
            }

            return current;
        }
        catch
        {
            return null;
        }
    }

    private static AutomationElement? FindByXPath(string xpath, AutomationElement searchRoot)
    {
        // XPath'ten HTML ID, name veya text çıkar
        try
        {
            // ID ile arama: //button[@id='myButton']
            var idMatch = System.Text.RegularExpressions.Regex.Match(xpath, @"@id='([^']+)'");
            if (idMatch.Success)
            {
                var id = idMatch.Groups[1].Value;
                return FindByAutomationId(id, searchRoot) ?? FindByHtmlId(id, searchRoot);
            }

            // Name ile arama: //input[@name='username']
            var nameMatch = System.Text.RegularExpressions.Regex.Match(xpath, @"@name='([^']+)'");
            if (nameMatch.Success)
            {
                return FindByName(nameMatch.Groups[1].Value, searchRoot);
            }

            // Text ile arama: //button[text()='Submit']
            var textMatch = System.Text.RegularExpressions.Regex.Match(xpath, @"text\(\)='([^']+)'");
            if (textMatch.Success)
            {
                return FindByName(textMatch.Groups[1].Value, searchRoot);
            }
        }
        catch { }

        return null;
    }

    private static AutomationElement? FindByCssSelector(string cssSelector, AutomationElement searchRoot)
    {
        // CSS Selector'den HTML ID, class veya attribute çıkar
        try
        {
            // ID ile arama: #myButton
            if (cssSelector.StartsWith("#"))
            {
                var id = cssSelector.Substring(1).Split('[', '.')[0];
                return FindByAutomationId(id, searchRoot) ?? FindByHtmlId(id, searchRoot);
            }

            // Class ile arama: .btn-primary
            if (cssSelector.StartsWith("."))
            {
                var className = cssSelector.Substring(1).Split('[', '.')[0];
                return FindByClassName(className, searchRoot);
            }

            // Attribute ile arama: [name='username']
            var attrMatch = System.Text.RegularExpressions.Regex.Match(cssSelector, @"\[name='([^']+)'\]");
            if (attrMatch.Success)
            {
                return FindByName(attrMatch.Groups[1].Value, searchRoot);
            }
        }
        catch { }

        return null;
    }

    private static AutomationElement? FindByHtmlId(string htmlId, AutomationElement searchRoot)
    {
        // Önce AutomationId ile dene
        var element = FindByAutomationId(htmlId, searchRoot);
        if (element != null) return element;

        // Name ile dene (bazı elementlerde ID Name olarak görünür)
        return FindByName(htmlId, searchRoot);
    }

    private static AutomationElement? FindByPlaywrightSelector(ElementLocatorStrategy strategy, AutomationElement searchRoot)
    {
        try
        {
            // SelectorKind kontrolü yap
            if (!strategy.Properties.TryGetValue("SelectorKind", out var selectorKind))
            {
                // SelectorKind yoksa basit selector string'i kullan
                if (strategy.Properties.TryGetValue("Selector", out var selector))
                {
                    return FindBySimplePlaywrightSelector(selector, searchRoot);
                }
                return null;
            }

            var kind = selectorKind?.Trim().ToLowerInvariant() ?? string.Empty;
            DebugLogger.Log($"[PlaywrightSelector] SelectorKind: {kind}");

            // SelectorKind'a göre işlem yap
            switch (kind)
            {
                case "table-row":
                case "row-index":
                    // Table row için özel işlem - RowIndex bilgisini kullan
                    if (strategy.Properties.TryGetValue("RowIndex", out var rowIndexStr) &&
                        int.TryParse(rowIndexStr, out var rowIndex))
                    {
                        DebugLogger.Log($"[PlaywrightSelector] Table row aranıyor, RowIndex: {rowIndex}");
                        return FindByTableRowIndex(rowIndex, searchRoot);
                    }
                    break;

                case "css":
                    if (strategy.Properties.TryGetValue("Selector", out var cssSelector))
                    {
                        DebugLogger.Log($"[PlaywrightSelector] CSS selector: {cssSelector}");
                        return FindByCssSelector(cssSelector, searchRoot);
                    }
                    break;

                case "xpath":
                    if (strategy.Properties.TryGetValue("Selector", out var xpathSelector))
                    {
                        DebugLogger.Log($"[PlaywrightSelector] XPath selector: {xpathSelector}");
                        return FindByXPath(xpathSelector, searchRoot);
                    }
                    break;

                case "text":
                    if (strategy.Properties.TryGetValue("Selector", out var textContent))
                    {
                        DebugLogger.Log($"[PlaywrightSelector] Text content: {textContent}");
                        return FindByName(textContent, searchRoot);
                    }
                    break;

                default:
                    DebugLogger.Log($"[PlaywrightSelector] Bilinmeyen SelectorKind: {kind}");
                    break;
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[PlaywrightSelector] Hata: {ex.Message}");
        }

        return null;
    }

    private static AutomationElement? FindBySimplePlaywrightSelector(string selector, AutomationElement searchRoot)
    {
        try
        {
            // ID: #myButton
            if (selector.StartsWith("#"))
            {
                var id = selector.Substring(1).Split('[')[0];
                return FindByAutomationId(id, searchRoot) ?? FindByHtmlId(id, searchRoot);
            }

            // Text-based: button:has-text('Submit')
            var textMatch = System.Text.RegularExpressions.Regex.Match(selector, @":has-text\('([^']+)'\)");
            if (textMatch.Success)
            {
                return FindByName(textMatch.Groups[1].Value, searchRoot);
            }

            // Attribute: [data-testid='mybutton']
            var attrMatch = System.Text.RegularExpressions.Regex.Match(selector, @"\[[\w-]+='([^']+)'\]");
            if (attrMatch.Success)
            {
                return FindByName(attrMatch.Groups[1].Value, searchRoot) ?? FindByAutomationId(attrMatch.Groups[1].Value, searchRoot);
            }
        }
        catch { }

        return null;
    }

    private static AutomationElement? FindByTableRowIndex(int rowIndex, AutomationElement searchRoot)
    {
        try
        {
            DebugLogger.Log($"[TableRow] Satır {rowIndex} aranıyor...");

            // Önce TABLE elementini bul (eğer TableSelector bilgisi varsa)
            // Şimdilik basit yaklaşım: Table ControlType ile tüm tabloları bul
            var tableCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Table);
            var tables = FindAllWithRawView(searchRoot, TreeScope.Descendants, tableCondition);

            DebugLogger.Log($"[TableRow] {(tables?.Count ?? 0)} tablo bulundu");
            if (tables == null || tables.Count == 0)
            {
                DebugLogger.Log("[TableRow] ⚠ Tablo bulunamadı, fallback'e geçiliyor...");
            }
            else
            {
                // Her tabloda satır ara
                foreach (AutomationElement table in tables)
            {
                try
                {
                    DebugLogger.Log($"[TableRow] Tablo aranıyor: {table.Current.Name ?? "N/A"}");

                    // Tablonun child elementlerini al (satırlar)
                    var walker = TreeWalker.RawViewWalker;
                    var row = walker.GetFirstChild(table);
                    int currentIndex = 0;

                    while (row != null)
                    {
                        if (currentIndex == rowIndex)
                        {
                            DebugLogger.Log($"[TableRow] ✓ Satır bulundu (index {rowIndex}): {row.Current.Name ?? "N/A"}");
                            return row;
                        }

                        currentIndex++;
                        row = walker.GetNextSibling(row);
                    }

                    DebugLogger.Log($"[TableRow] Bu tabloda {currentIndex} satır bulundu, index {rowIndex} yok");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[TableRow] Tablo tarama hatası: {ex.Message}");
                }
            }
            }

            // FALLBACK: Table bulunamadıysa, tüm Custom elementlerden index'e göre al
            DebugLogger.Log("[TableRow] ⚠ Table bulunamadı, Custom elementlerden aranıyor (fallback)...");
            var customCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom);
            var customs = FindAllWithRawView(searchRoot, TreeScope.Descendants, customCondition);

            DebugLogger.Log($"[TableRow] {(customs?.Count ?? 0)} custom element bulundu");
            if (customs == null) return null;

            if (rowIndex >= 0 && rowIndex < customs.Count)
            {
                var element = customs[rowIndex];
                DebugLogger.Log($"[TableRow] ⚠ Fallback: Custom element bulundu: {element.Current.Name ?? "N/A"}");
                return element;
            }

            DebugLogger.Log($"[TableRow] ✗ RowIndex {rowIndex} hiçbir yerde bulunamadı");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[TableRow] Hata: {ex.Message}");
        }

        return null;
    }

    private static AutomationElement? FindByCoordinates(int x, int y)
    {
        try
        {
            return AutomationElement.FromPoint(new System.Windows.Point(x, y));
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
