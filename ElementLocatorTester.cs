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
    public static async Task<ElementLocatorStrategy> TestStrategy(ElementLocatorStrategy strategy)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var element = await Task.Run(() => FindElementByStrategy(strategy));

            stopwatch.Stop();
            strategy.TestDurationMs = (int)stopwatch.ElapsedMilliseconds;
            strategy.IsSuccessful = element != null;

            if (!strategy.IsSuccessful)
            {
                strategy.ErrorMessage = "Element bulunamadı";
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
    private static AutomationElement? FindElementByStrategy(ElementLocatorStrategy strategy)
    {
        switch (strategy.Type)
        {
            case LocatorType.AutomationId:
                return FindByAutomationId(strategy.Properties["AutomationId"]);

            case LocatorType.Name:
                return FindByName(strategy.Properties["Name"]);

            case LocatorType.ClassName:
                return FindByClassName(strategy.Properties["ClassName"]);

            case LocatorType.AutomationIdAndControlType:
                return FindByAutomationIdAndControlType(
                    strategy.Properties["AutomationId"],
                    strategy.Properties["ControlType"]);

            case LocatorType.NameAndControlType:
                return FindByNameAndControlType(
                    strategy.Properties["Name"],
                    strategy.Properties["ControlType"]);

            case LocatorType.NameAndParent:
                return FindByNameAndParent(
                    strategy.Properties["Name"],
                    strategy.Properties["ParentName"]);

            // Diğer stratejiler için placeholder
            case LocatorType.ElementPath:
            case LocatorType.TreePath:
            case LocatorType.XPath:
            case LocatorType.CssSelector:
            case LocatorType.HtmlId:
            case LocatorType.PlaywrightSelector:
            case LocatorType.Coordinates:
                // Şimdilik desteklenmiyor
                return null;

            default:
                return null;
        }
    }

    #region Find Methods

    private static AutomationElement? FindByAutomationId(string automationId)
    {
        var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
        return AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
    }

    private static AutomationElement? FindByName(string name)
    {
        var condition = new PropertyCondition(AutomationElement.NameProperty, name);
        return AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
    }

    private static AutomationElement? FindByClassName(string className)
    {
        var condition = new PropertyCondition(AutomationElement.ClassNameProperty, className);
        return AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
    }

    private static AutomationElement? FindByAutomationIdAndControlType(string automationId, string controlTypeStr)
    {
        var controlType = ParseControlType(controlTypeStr);
        if (controlType == null) return null;

        var condition = new AndCondition(
            new PropertyCondition(AutomationElement.AutomationIdProperty, automationId),
            new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)
        );
        return AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
    }

    private static AutomationElement? FindByNameAndControlType(string name, string controlTypeStr)
    {
        var controlType = ParseControlType(controlTypeStr);
        if (controlType == null) return null;

        var condition = new AndCondition(
            new PropertyCondition(AutomationElement.NameProperty, name),
            new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)
        );
        return AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
    }

    private static AutomationElement? FindByNameAndParent(string name, string parentName)
    {
        // Önce parent'ı bul
        var parentCondition = new PropertyCondition(AutomationElement.NameProperty, parentName);
        var parent = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, parentCondition);

        if (parent == null) return null;

        // Parent içinde child'ı bul
        var childCondition = new PropertyCondition(AutomationElement.NameProperty, name);
        return parent.FindFirst(TreeScope.Descendants, childCondition);
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

    #endregion
}
