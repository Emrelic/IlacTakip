using System.Windows.Automation;
using System.Text.RegularExpressions;

namespace MedulaOtomasyon;

/// <summary>
/// Tip 3 görevler için koşul değerlendirme motoru
/// UI element durumlarını kontrol edip boolean sonuç döner
/// </summary>
public class ConditionEvaluator
{
    private readonly ElementLocatorTester _locatorTester;

    public ConditionEvaluator()
    {
        _locatorTester = new ElementLocatorTester();
    }

    /// <summary>
    /// Bir ConditionInfo'daki tüm koşulları değerlendir
    /// </summary>
    /// <param name="conditionInfo">Değerlendirilecek koşul bilgisi</param>
    /// <returns>Koşul sonucu (true/false) veya switch-case için özel değer</returns>
    public string EvaluateConditions(ConditionInfo conditionInfo)
    {
        if (conditionInfo.Conditions == null || conditionInfo.Conditions.Count == 0)
        {
            return "false";
        }

        try
        {
            bool finalResult = true;
            bool isFirstCondition = true;
            LogicalOperator previousLogicalOp = LogicalOperator.None;

            foreach (var condition in conditionInfo.Conditions)
            {
                // Koşulu değerlendir
                bool conditionResult = EvaluateSingleCondition(condition);

                // İlk koşul ise direkt ata
                if (isFirstCondition)
                {
                    finalResult = conditionResult;
                    isFirstCondition = false;
                }
                else
                {
                    // Önceki mantıksal operatöre göre birleştir
                    finalResult = previousLogicalOp switch
                    {
                        LogicalOperator.AND => finalResult && conditionResult,
                        LogicalOperator.OR => finalResult || conditionResult,
                        _ => conditionResult
                    };
                }

                // Bir sonraki koşul için mantıksal operatörü sakla
                previousLogicalOp = condition.LogicalOperator;
            }

            return finalResult.ToString().ToLower();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Koşul değerlendirme hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tek bir UI koşulunu değerlendir
    /// </summary>
    private bool EvaluateSingleCondition(UICondition condition)
    {
        if (condition.Element == null)
        {
            throw new ArgumentException("Koşul elementi null olamaz!");
        }

        try
        {
            // Elementi bul
            AutomationElement? element = FindElement(condition.Element, condition.LocatorStrategy);

            if (element == null)
            {
                // Element bulunamadıysa, özelliğe göre karar ver
                // IsVisible için false dön (görünmüyor)
                if (condition.PropertyName.Equals("IsVisible", StringComparison.OrdinalIgnoreCase))
                {
                    return EvaluateOperator(false, condition.Operator, condition.ExpectedValue);
                }

                throw new InvalidOperationException($"Element bulunamadı: {condition.Element.Name ?? condition.Element.AutomationId}");
            }

            // Özellik değerini al
            object? propertyValue = GetPropertyValue(element, condition.Element, condition.PropertyName);

            // Operatörü değerlendir
            return EvaluateOperator(propertyValue, condition.Operator, condition.ExpectedValue);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Koşul değerlendirme hatası: {condition.PropertyName} @ {condition.Element.Name ?? condition.Element.AutomationId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// UI elementi bul
    /// </summary>
    private AutomationElement? FindElement(UIElementInfo elementInfo, ElementLocatorStrategy? strategy)
    {
        try
        {
            // Strateji belirtilmişse kullan
            if (strategy != null)
            {
                var result = ElementLocatorTester.TestStrategy(strategy, elementInfo).GetAwaiter().GetResult();
                return result.IsSuccessful ? AutomationElement.RootElement.FindFirst(
                    TreeScope.Descendants,
                    CreateConditionFromStrategy(strategy)) : null;
            }

            // Yoksa AutomationId veya Name ile bul
            var desktop = AutomationElement.RootElement;

            Condition condition;

            if (!string.IsNullOrEmpty(elementInfo.AutomationId))
            {
                condition = new PropertyCondition(AutomationElement.AutomationIdProperty, elementInfo.AutomationId);
            }
            else if (!string.IsNullOrEmpty(elementInfo.Name))
            {
                condition = new PropertyCondition(AutomationElement.NameProperty, elementInfo.Name);
            }
            else
            {
                throw new InvalidOperationException("Element tanımlayıcı bilgisi eksik!");
            }

            return desktop.FindFirst(TreeScope.Descendants, condition);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Element bulunamadı: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Strategy'den Condition oluştur
    /// </summary>
    private Condition CreateConditionFromStrategy(ElementLocatorStrategy strategy)
    {
        var conditions = new List<Condition>();

        if (strategy.Properties.TryGetValue("AutomationId", out var automationId))
        {
            conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));
        }

        if (strategy.Properties.TryGetValue("Name", out var name))
        {
            conditions.Add(new PropertyCondition(AutomationElement.NameProperty, name));
        }

        if (conditions.Count == 0)
            return Condition.TrueCondition;

        if (conditions.Count == 1)
            return conditions[0];

        return new AndCondition(conditions.ToArray());
    }

    /// <summary>
    /// Element özellik değerini al
    /// </summary>
    private object? GetPropertyValue(AutomationElement element, UIElementInfo elementInfo, string propertyName)
    {
        try
        {
            return propertyName.ToLower() switch
            {
                "isenabled" => element.Current.IsEnabled,
                "isvisible" => !element.Current.IsOffscreen,
                "isoffscreen" => element.Current.IsOffscreen,
                "haskeyboardfocus" => element.Current.HasKeyboardFocus,
                "name" => element.Current.Name,
                "classname" => element.Current.ClassName,
                "controltype" => element.Current.ControlType.ProgrammaticName,
                "text" => GetTextValue(element),
                "value" => GetValuePattern(element),
                "ischecked" => GetToggleState(element),
                "innertext" => elementInfo.InnerText,
                "innerhtml" => elementInfo.InnerHtml,
                _ => throw new ArgumentException($"Desteklenmeyen özellik: {propertyName}")
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Özellik değeri alınamadı: {propertyName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Text değerini al (ValuePattern veya Name kullanarak)
    /// </summary>
    private string? GetTextValue(AutomationElement element)
    {
        try
        {
            // Önce ValuePattern dene
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
            {
                return ((ValuePattern)valuePattern).Current.Value;
            }

            // Yoksa Name kullan
            return element.Current.Name;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// ValuePattern ile değer al
    /// </summary>
    private string? GetValuePattern(AutomationElement element)
    {
        try
        {
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? pattern))
            {
                return ((ValuePattern)pattern).Current.Value;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// TogglePattern ile checkbox durumunu al
    /// </summary>
    private bool? GetToggleState(AutomationElement element)
    {
        try
        {
            if (element.TryGetCurrentPattern(TogglePattern.Pattern, out object? pattern))
            {
                var state = ((TogglePattern)pattern).Current.ToggleState;
                return state == ToggleState.On;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Operatör değerlendirmesi yap
    /// </summary>
    private bool EvaluateOperator(object? actualValue, ConditionOperator op, string expectedValue)
    {
        // Null kontrolleri
        if (actualValue == null)
        {
            return op switch
            {
                ConditionOperator.IsEmpty => true,
                ConditionOperator.IsNotEmpty => false,
                ConditionOperator.IsFalse => true,
                _ => false
            };
        }

        string actualStr = actualValue.ToString() ?? "";
        string expectedStr = expectedValue ?? "";

        return op switch
        {
            ConditionOperator.Equals => string.Equals(actualStr, expectedStr, StringComparison.OrdinalIgnoreCase),

            ConditionOperator.NotEquals => !string.Equals(actualStr, expectedStr, StringComparison.OrdinalIgnoreCase),

            ConditionOperator.Contains => actualStr.Contains(expectedStr, StringComparison.OrdinalIgnoreCase),

            ConditionOperator.NotContains => !actualStr.Contains(expectedStr, StringComparison.OrdinalIgnoreCase),

            ConditionOperator.StartsWith => actualStr.StartsWith(expectedStr, StringComparison.OrdinalIgnoreCase),

            ConditionOperator.EndsWith => actualStr.EndsWith(expectedStr, StringComparison.OrdinalIgnoreCase),

            ConditionOperator.GreaterThan => CompareNumeric(actualStr, expectedStr) > 0,

            ConditionOperator.LessThan => CompareNumeric(actualStr, expectedStr) < 0,

            ConditionOperator.GreaterOrEqual => CompareNumeric(actualStr, expectedStr) >= 0,

            ConditionOperator.LessOrEqual => CompareNumeric(actualStr, expectedStr) <= 0,

            ConditionOperator.IsTrue => ParseBool(actualStr) == true,

            ConditionOperator.IsFalse => ParseBool(actualStr) == false,

            ConditionOperator.IsEmpty => string.IsNullOrWhiteSpace(actualStr),

            ConditionOperator.IsNotEmpty => !string.IsNullOrWhiteSpace(actualStr),

            _ => false
        };
    }

    /// <summary>
    /// Sayısal karşılaştırma yap
    /// </summary>
    private int CompareNumeric(string actual, string expected)
    {
        if (double.TryParse(actual, out double actualNum) && double.TryParse(expected, out double expectedNum))
        {
            return actualNum.CompareTo(expectedNum);
        }

        throw new ArgumentException($"Sayısal karşılaştırma yapılamadı: '{actual}' ve '{expected}'");
    }

    /// <summary>
    /// Boolean parse et
    /// </summary>
    private bool? ParseBool(string value)
    {
        if (bool.TryParse(value, out bool result))
            return result;

        // "1", "0", "yes", "no" gibi değerleri de destekle
        return value.ToLower() switch
        {
            "1" or "yes" or "true" or "on" or "checked" => true,
            "0" or "no" or "false" or "off" or "unchecked" => false,
            _ => null
        };
    }

    /// <summary>
    /// Koşul sonucuna göre gidilecek dalı bul
    /// </summary>
    public string GetTargetBranch(ConditionInfo conditionInfo, string conditionResult)
    {
        // Boolean dallanma
        if (conditionInfo.BranchType == "Boolean")
        {
            var branch = conditionInfo.Branches.FirstOrDefault(b =>
                b.ConditionValue.Equals(conditionResult, StringComparison.OrdinalIgnoreCase));

            if (branch != null)
                return branch.TargetStepId;
        }
        // Switch-case dallanma
        else
        {
            foreach (var branch in conditionInfo.Branches)
            {
                // Özel değer karşılaştırması yapılabilir
                if (branch.ConditionValue.Equals(conditionResult, StringComparison.OrdinalIgnoreCase))
                {
                    return branch.TargetStepId;
                }
            }
        }

        // Eşleşme yoksa varsayılan dalı dön
        return conditionInfo.DefaultBranchStepId ?? "";
    }
}
