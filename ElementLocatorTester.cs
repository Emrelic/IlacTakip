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

        // 9. Playwright Selector
        if (!string.IsNullOrEmpty(elementInfo.PlaywrightSelector))
        {
            strategies.Add(new ElementLocatorStrategy
            {
                Name = "Playwright Selector",
                Description = $"Playwright: '{elementInfo.PlaywrightSelector}'",
                Type = LocatorType.PlaywrightSelector,
                Properties = new Dictionary<string, string>
                {
                    ["PlaywrightSelector"] = elementInfo.PlaywrightSelector
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
    /// </summary>
    public static AutomationElement? FindElementByStrategy(ElementLocatorStrategy strategy, UIElementInfo? elementInfo = null)
    {
        // Hedef pencereyi bul (varsa)
        AutomationElement? targetWindow = null;
        if (elementInfo != null)
        {
            targetWindow = FindTargetWindow(elementInfo);

            // Debug: Hedef pencere bulundu mu?
            if (targetWindow == null && !string.IsNullOrEmpty(elementInfo.WindowTitle))
            {
                // Hedef pencere bulunamadı - bu strateji başarısız olacak
                System.Diagnostics.Debug.WriteLine($"[ElementLocator] UYARI: Hedef pencere bulunamadı: {elementInfo.WindowTitle}");
            }
        }

        // Arama kapsam rootunu belirle
        var searchRoot = targetWindow ?? AutomationElement.RootElement;

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
                // Playwright selector - UI Automation ile yaklaşık eşleşme dene
                return FindByPlaywrightSelector(strategy.Properties["PlaywrightSelector"], searchRoot);

            case LocatorType.Coordinates:
                return FindByCoordinates(
                    int.Parse(strategy.Properties["X"]),
                    int.Parse(strategy.Properties["Y"]));

            default:
                return null;
        }
    }

    /// <summary>
    /// UIElementInfo'dan hedef pencereyi bulur
    /// </summary>
    private static AutomationElement? FindTargetWindow(UIElementInfo elementInfo)
    {
        try
        {
            if (!string.IsNullOrEmpty(elementInfo.WindowTitle))
            {
                System.Diagnostics.Debug.WriteLine($"[FindTargetWindow] WindowTitle ile arama: {elementInfo.WindowTitle}");
                var condition = new PropertyCondition(AutomationElement.NameProperty, elementInfo.WindowTitle);
                var window = AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);

                if (window != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[FindTargetWindow] ✓ Pencere bulundu: {window.Current.Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[FindTargetWindow] ✗ Pencere bulunamadı: {elementInfo.WindowTitle}");
                }

                return window;
            }
            else if (elementInfo.WindowProcessId.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($"[FindTargetWindow] ProcessId ile arama: {elementInfo.WindowProcessId.Value}");
                var windows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

                foreach (AutomationElement window in windows)
                {
                    try
                    {
                        if (window.Current.ProcessId == elementInfo.WindowProcessId.Value)
                        {
                            System.Diagnostics.Debug.WriteLine($"[FindTargetWindow] ✓ Pencere bulundu (ProcessId): {window.Current.Name}");
                            return window;
                        }
                    }
                    catch { }
                }
                System.Diagnostics.Debug.WriteLine($"[FindTargetWindow] ✗ ProcessId ile pencere bulunamadı");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FindTargetWindow] Exception: {ex.Message}");
        }

        return null;
    }

    #region Find Methods

    /// <summary>
    /// Timeout ile element arama (3 saniye)
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
                    return root.FindFirst(scope, condition);
                }
                catch
                {
                    return null;
                }
            });

            if (task.Wait(timeoutMs))
            {
                result = task.Result;
            }
        }
        catch { }

        stopwatch.Stop();
        return result;
    }

    private static AutomationElement? FindByAutomationId(string automationId, AutomationElement searchRoot)
    {
        var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
        return FindWithTimeout(searchRoot, TreeScope.Descendants, condition);
    }

    private static AutomationElement? FindByName(string name, AutomationElement searchRoot)
    {
        var condition = new PropertyCondition(AutomationElement.NameProperty, name);
        return FindWithTimeout(searchRoot, TreeScope.Descendants, condition);
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
                    return searchRoot.FindAll(TreeScope.Descendants, condition);
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
        // Zorlu implementasyon - şimdilik null
        return null;
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

    private static AutomationElement? FindByPlaywrightSelector(string selector, AutomationElement searchRoot)
    {
        // Playwright selector'den bilgi çıkar
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
