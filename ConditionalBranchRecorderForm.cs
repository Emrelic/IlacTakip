using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace MedulaOtomasyon;

/// <summary>
/// Tip 3 görev kaydedici: Koşullu Dallanma
/// Kullanıcı UI elementlerinin durumlarına göre görev zincirini dallandırabilir
/// </summary>
public partial class ConditionalBranchRecorderForm : Form
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private ConditionInfo _conditionInfo;
    private List<UIElementInfo> _availableElements;
    private bool _isTopmost = false;

    public ConditionInfo? Result { get; private set; }

    public ConditionalBranchRecorderForm()
    {
        InitializeComponent();
        _conditionInfo = new ConditionInfo();
        _availableElements = new List<UIElementInfo>();
        InitializeOperators();
        InitializeLogicalOperators();
        InitializeProperties();
    }

    /// <summary>
    /// Operatör combobox'ını doldur
    /// </summary>
    private void InitializeOperators()
    {
        cmbOperator.Items.Clear();
        cmbOperator.Items.Add("Eşittir (==)");
        cmbOperator.Items.Add("Eşit Değil (!=)");
        cmbOperator.Items.Add("İçerir (Contains)");
        cmbOperator.Items.Add("İçermez (NotContains)");
        cmbOperator.Items.Add("İle Başlar (StartsWith)");
        cmbOperator.Items.Add("İle Biter (EndsWith)");
        cmbOperator.Items.Add("Büyüktür (>)");
        cmbOperator.Items.Add("Küçüktür (<)");
        cmbOperator.Items.Add("Büyük veya Eşit (>=)");
        cmbOperator.Items.Add("Küçük veya Eşit (<=)");
        cmbOperator.Items.Add("True mu?");
        cmbOperator.Items.Add("False mu?");
        cmbOperator.Items.Add("Boş mu?");
        cmbOperator.Items.Add("Boş Değil mi?");
        cmbOperator.SelectedIndex = 0;
    }

    /// <summary>
    /// Mantıksal operatör combobox'ını doldur
    /// </summary>
    private void InitializeLogicalOperators()
    {
        cmbLogicalOp.Items.Clear();
        cmbLogicalOp.Items.Add("(Son koşul)");
        cmbLogicalOp.Items.Add("VE (AND)");
        cmbLogicalOp.Items.Add("VEYA (OR)");
        cmbLogicalOp.SelectedIndex = 0;
    }

    /// <summary>
    /// Property combobox'ını doldur
    /// </summary>
    private void InitializeProperties()
    {
        cmbProperty.Items.Clear();
        cmbProperty.Items.Add("Text");
        cmbProperty.Items.Add("Name");
        cmbProperty.Items.Add("Value");
        cmbProperty.Items.Add("IsEnabled");
        cmbProperty.Items.Add("IsVisible");
        cmbProperty.Items.Add("IsChecked");
        cmbProperty.Items.Add("IsOffscreen");
        cmbProperty.Items.Add("HasKeyboardFocus");
        cmbProperty.Items.Add("InnerText");
        cmbProperty.Items.Add("InnerHtml");
        cmbProperty.Items.Add("ClassName");
        cmbProperty.Items.Add("ControlType");
        cmbProperty.SelectedIndex = 0;
    }

    private void BtnTopmost_Click(object? sender, EventArgs e)
    {
        _isTopmost = !_isTopmost;
        this.TopMost = _isTopmost;
        btnTopmost.Text = _isTopmost ? "📌 Üstte (Aktif)" : "📌 Üstte Tut";
    }

    /// <summary>
    /// Sayfadaki UI elementlerini listele
    /// </summary>
    private async void BtnRefreshElements_Click(object? sender, EventArgs e)
    {
        try
        {
            btnRefreshElements.Enabled = false;
            btnRefreshElements.Text = "⏳ Taranıyor...";

            // Aktif pencereyi al
            var foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
            {
                MessageBox.Show("Aktif pencere bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // UI Automation ile elementleri topla
            var rootElement = AutomationElement.FromHandle(foregroundWindow);
            _availableElements.Clear();

            // Tüm elementleri recursive olarak topla
            await Task.Run(() => CollectElements(rootElement));

            // ComboBox'ı güncelle
            UpdateElementComboBox();

            MessageBox.Show($"{_availableElements.Count} element bulundu!", "Başarılı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Element tarama hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnRefreshElements.Enabled = true;
            btnRefreshElements.Text = "🔄 Elementleri Listele";
        }
    }

    /// <summary>
    /// UI elementlerini recursive olarak topla
    /// </summary>
    private void CollectElements(AutomationElement element, int depth = 0)
    {
        if (element == null || depth > 10) return;

        try
        {
            var elementInfo = new UIElementInfo
            {
                AutomationId = element.Current.AutomationId,
                Name = element.Current.Name,
                ClassName = element.Current.ClassName,
                ControlType = element.Current.ControlType.ProgrammaticName,
                IsEnabled = element.Current.IsEnabled,
                IsOffscreen = element.Current.IsOffscreen,
                LocalizedControlType = element.Current.LocalizedControlType,
                DetectionMethod = "UIAutomation"
            };

            // Sadece anlamlı elementleri ekle (Name veya AutomationId olan)
            if (!string.IsNullOrWhiteSpace(elementInfo.Name) ||
                !string.IsNullOrWhiteSpace(elementInfo.AutomationId))
            {
                _availableElements.Add(elementInfo);
            }

            // Çocuk elementleri tara
            var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
            foreach (AutomationElement child in children)
            {
                CollectElements(child, depth + 1);
            }
        }
        catch
        {
            // Element erişim hatalarını yoksay
        }
    }

    /// <summary>
    /// Element combobox'ını güncelle
    /// </summary>
    private void UpdateElementComboBox()
    {
        cmbElement.Items.Clear();
        foreach (var elem in _availableElements)
        {
            var displayText = $"{elem.ControlType ?? "?"} - {elem.Name ?? elem.AutomationId ?? "??"}";
            cmbElement.Items.Add(displayText);
        }
    }

    /// <summary>
    /// Element picker ile element seç
    /// </summary>
    private async void BtnPickElement_Click(object? sender, EventArgs e)
    {
        try
        {
            this.Hide();
            await Task.Delay(500);

            MessageBox.Show("Mouse ile seçmek istediğiniz elemente tıklayın!", "Element Seç",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await Task.Delay(500);

            // Mouse pozisyonundaki elementi yakala
            var selectedElement = await UIElementPicker.CaptureElementAtMousePositionAsync();

            if (selectedElement != null)
            {
                // Seçilen elementi listeye ekle
                _availableElements.Add(selectedElement);
                UpdateElementComboBox();

                // Yeni eklenen elementi seç
                cmbElement.SelectedIndex = _availableElements.Count - 1;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Element seçim hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.Show();
            this.BringToFront();
        }
    }

    /// <summary>
    /// Element seçildiğinde özellik combobox'ını güncelle
    /// </summary>
    private void CmbElement_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbElement.SelectedIndex < 0 || cmbElement.SelectedIndex >= _availableElements.Count)
            return;

        var selectedElement = _availableElements[cmbElement.SelectedIndex];

        // Özellik listesini element tipine göre güncelle
        UpdatePropertyComboBox(selectedElement);
    }

    /// <summary>
    /// Element tipine göre özellik listesini güncelle
    /// </summary>
    private void UpdatePropertyComboBox(UIElementInfo element)
    {
        cmbProperty.Items.Clear();

        // Her element için ortak özellikler
        cmbProperty.Items.Add("IsEnabled");
        cmbProperty.Items.Add("IsVisible");

        if (!string.IsNullOrEmpty(element.Name))
            cmbProperty.Items.Add("Name");

        // Control type'a göre özel özellikler
        if (element.ControlType?.Contains("CheckBox") == true ||
            element.ControlType?.Contains("RadioButton") == true)
        {
            cmbProperty.Items.Add("IsChecked");
        }

        if (element.ControlType?.Contains("Text") == true ||
            element.ControlType?.Contains("Edit") == true)
        {
            cmbProperty.Items.Add("Text");
            cmbProperty.Items.Add("Value");
        }

        // Web elementleri için
        if (!string.IsNullOrEmpty(element.InnerText))
            cmbProperty.Items.Add("InnerText");

        if (!string.IsNullOrEmpty(element.Value))
            cmbProperty.Items.Add("Value");

        cmbProperty.Items.Add("ClassName");
        cmbProperty.Items.Add("ControlType");

        if (cmbProperty.Items.Count > 0)
            cmbProperty.SelectedIndex = 0;
    }

    /// <summary>
    /// Koşul ekle
    /// </summary>
    private void BtnAddCondition_Click(object? sender, EventArgs e)
    {
        try
        {
            // Validasyon
            if (cmbElement.SelectedIndex < 0)
            {
                MessageBox.Show("Lütfen bir element seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbProperty.Text))
            {
                MessageBox.Show("Lütfen bir özellik seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var condition = new UICondition
            {
                Element = _availableElements[cmbElement.SelectedIndex],
                PropertyName = cmbProperty.Text,
                Operator = GetOperatorFromComboBox(),
                ExpectedValue = txtValue.Text,
                LogicalOperator = GetLogicalOperatorFromComboBox()
            };

            _conditionInfo.Conditions.Add(condition);
            UpdateConditionsList();

            // Formu temizle
            txtValue.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Koşul ekleme hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Seçili koşulu sil
    /// </summary>
    private void BtnRemoveCondition_Click(object? sender, EventArgs e)
    {
        if (lstConditions.SelectedIndex >= 0 && lstConditions.SelectedIndex < _conditionInfo.Conditions.Count)
        {
            _conditionInfo.Conditions.RemoveAt(lstConditions.SelectedIndex);
            UpdateConditionsList();
        }
    }

    /// <summary>
    /// Koşullar listesini güncelle
    /// </summary>
    private void UpdateConditionsList()
    {
        lstConditions.Items.Clear();
        for (int i = 0; i < _conditionInfo.Conditions.Count; i++)
        {
            var cond = _conditionInfo.Conditions[i];
            var elementName = cond.Element?.Name ?? cond.Element?.AutomationId ?? "?";
            var logicalOp = cond.LogicalOperator == LogicalOperator.None ? "" : $" {cond.LogicalOperator}";

            lstConditions.Items.Add(
                $"{i + 1}. {elementName}.{cond.PropertyName} {GetOperatorSymbol(cond.Operator)} \"{cond.ExpectedValue}\"{logicalOp}");
        }
    }

    /// <summary>
    /// Dal ekle
    /// </summary>
    private void BtnAddBranch_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtBranchName.Text))
            {
                MessageBox.Show("Lütfen dal adı girin (A, B, C...)!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTargetStepId.Text))
            {
                MessageBox.Show("Lütfen hedef adım ID girin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var branch = new BranchTarget
            {
                BranchName = txtBranchName.Text.Trim().ToUpper(),
                TargetStepId = txtTargetStepId.Text.Trim(),
                ConditionValue = txtConditionValue.Text.Trim(),
                Description = txtBranchDesc.Text.Trim()
            };

            _conditionInfo.Branches.Add(branch);
            UpdateBranchesList();

            // Formu temizle
            txtBranchName.Clear();
            txtTargetStepId.Clear();
            txtConditionValue.Clear();
            txtBranchDesc.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Dal ekleme hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Seçili dalı sil
    /// </summary>
    private void BtnRemoveBranch_Click(object? sender, EventArgs e)
    {
        if (lstBranches.SelectedIndex >= 0 && lstBranches.SelectedIndex < _conditionInfo.Branches.Count)
        {
            _conditionInfo.Branches.RemoveAt(lstBranches.SelectedIndex);
            UpdateBranchesList();
        }
    }

    /// <summary>
    /// Dallar listesini güncelle
    /// </summary>
    private void UpdateBranchesList()
    {
        lstBranches.Items.Clear();
        foreach (var branch in _conditionInfo.Branches)
        {
            lstBranches.Items.Add(
                $"Dal {branch.BranchName} -> Adım {branch.TargetStepId} (Değer: {branch.ConditionValue}) - {branch.Description}");
        }
    }

    private void LstConditions_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Seçili koşulu düzenleme için form alanlarına yükle (opsiyonel)
    }

    private void LstBranches_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Seçili dalı düzenleme için form alanlarına yükle (opsiyonel)
    }

    private void CmbBranchType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _conditionInfo.BranchType = cmbBranchType.SelectedIndex == 0 ? "Boolean" : "SwitchCase";
    }

    /// <summary>
    /// Kaydet ve kapat
    /// </summary>
    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            // Validasyon
            if (_conditionInfo.Conditions.Count == 0)
            {
                MessageBox.Show("En az bir koşul tanımlamalısınız!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_conditionInfo.Branches.Count == 0)
            {
                MessageBox.Show("En az bir dal tanımlamalısınız!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _conditionInfo.PageIdentifier = txtPageIdentifier.Text.Trim();
            _conditionInfo.DefaultBranchStepId = txtDefaultBranch.Text.Trim();

            Result = _conditionInfo;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kaydetme hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        Result = null;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    #region Helper Methods

    /// <summary>
    /// ComboBox'tan operatör enum'ı al
    /// </summary>
    private ConditionOperator GetOperatorFromComboBox()
    {
        return cmbOperator.SelectedIndex switch
        {
            0 => ConditionOperator.Equals,
            1 => ConditionOperator.NotEquals,
            2 => ConditionOperator.Contains,
            3 => ConditionOperator.NotContains,
            4 => ConditionOperator.StartsWith,
            5 => ConditionOperator.EndsWith,
            6 => ConditionOperator.GreaterThan,
            7 => ConditionOperator.LessThan,
            8 => ConditionOperator.GreaterOrEqual,
            9 => ConditionOperator.LessOrEqual,
            10 => ConditionOperator.IsTrue,
            11 => ConditionOperator.IsFalse,
            12 => ConditionOperator.IsEmpty,
            13 => ConditionOperator.IsNotEmpty,
            _ => ConditionOperator.Equals
        };
    }

    /// <summary>
    /// ComboBox'tan mantıksal operatör enum'ı al
    /// </summary>
    private LogicalOperator GetLogicalOperatorFromComboBox()
    {
        return cmbLogicalOp.SelectedIndex switch
        {
            0 => LogicalOperator.None,
            1 => LogicalOperator.AND,
            2 => LogicalOperator.OR,
            _ => LogicalOperator.None
        };
    }

    /// <summary>
    /// Operatör enum'ından sembol al
    /// </summary>
    private string GetOperatorSymbol(ConditionOperator op)
    {
        return op switch
        {
            ConditionOperator.Equals => "==",
            ConditionOperator.NotEquals => "!=",
            ConditionOperator.Contains => "içerir",
            ConditionOperator.NotContains => "içermez",
            ConditionOperator.StartsWith => "başlar",
            ConditionOperator.EndsWith => "biter",
            ConditionOperator.GreaterThan => ">",
            ConditionOperator.LessThan => "<",
            ConditionOperator.GreaterOrEqual => ">=",
            ConditionOperator.LessOrEqual => "<=",
            ConditionOperator.IsTrue => "true mu?",
            ConditionOperator.IsFalse => "false mu?",
            ConditionOperator.IsEmpty => "boş mu?",
            ConditionOperator.IsNotEmpty => "boş değil mi?",
            _ => "?"
        };
    }

    #endregion
}
