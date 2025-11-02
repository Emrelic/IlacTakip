namespace MedulaOtomasyon;

public partial class Step3_ConditionSetup : UserControl, IWizardStep
{
    private ConditionInfo _conditionInfo;
    private UIElementInfo? _currentElement;

    public Step3_ConditionSetup(ConditionInfo conditionInfo)
    {
        InitializeComponent();
        _conditionInfo = conditionInfo;
    }

    public void OnStepEnter()
    {
        _currentElement = _conditionInfo.Conditions.FirstOrDefault()?.Element;

        if (_currentElement != null)
        {
            lblElementInfo.Text = $"📌 Element: {_currentElement.ControlType ?? "?"} - {_currentElement.Name ?? _currentElement.AutomationId ?? "İsimsiz"}";
            PopulateProperties();
        }
        else
        {
            lblElementInfo.Text = "❌ Element seçilmedi!";
        }

        RefreshConditionsList();
    }

    public bool OnStepExit()
    {
        // En az 1 koşul eklenmiş olmalı
        if (_conditionInfo.Conditions.Count == 0 || _conditionInfo.Conditions[0].PropertyName == "")
        {
            MessageBox.Show("Lütfen en az bir koşul tanımlayın!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    public bool CanProceed()
    {
        return _conditionInfo.Conditions.Count > 0 && !string.IsNullOrEmpty(_conditionInfo.Conditions[0].PropertyName);
    }

    /// <summary>
    /// Element özelliklerini doldur
    /// </summary>
    private void PopulateProperties()
    {
        if (_currentElement == null) return;

        cmbProperty.Items.Clear();

        // Boolean özellikler
        AddPropertyIfNotEmpty("IsEnabled", _currentElement.IsEnabled?.ToString());
        AddPropertyIfNotEmpty("IsVisible", _currentElement.IsVisible?.ToString());
        AddPropertyIfNotEmpty("IsOffscreen", _currentElement.IsOffscreen?.ToString());
        AddPropertyIfNotEmpty("HasKeyboardFocus", _currentElement.HasKeyboardFocus?.ToString());
        AddPropertyIfNotEmpty("IsKeyboardFocusable", _currentElement.IsKeyboardFocusable?.ToString());
        AddPropertyIfNotEmpty("IsPassword", _currentElement.IsPassword?.ToString());

        // Text özellikleri
        AddPropertyIfNotEmpty("Name", _currentElement.Name);
        AddPropertyIfNotEmpty("AutomationId", _currentElement.AutomationId);
        AddPropertyIfNotEmpty("ClassName", _currentElement.ClassName);
        AddPropertyIfNotEmpty("ControlType", _currentElement.ControlType);
        AddPropertyIfNotEmpty("InnerText", _currentElement.InnerText);
        AddPropertyIfNotEmpty("Value", _currentElement.Value);
        AddPropertyIfNotEmpty("HelpText", _currentElement.HelpText);

        // Web özellikleri
        AddPropertyIfNotEmpty("HtmlId", _currentElement.HtmlId);
        AddPropertyIfNotEmpty("Tag", _currentElement.Tag);
        AddPropertyIfNotEmpty("Placeholder", _currentElement.Placeholder);

        // Operatörleri doldur
        cmbOperator.Items.Clear();
        cmbOperator.Items.Add("== (Eşittir)");
        cmbOperator.Items.Add("!= (Eşit Değil)");
        cmbOperator.Items.Add("İçerir");
        cmbOperator.Items.Add("İçermez");
        cmbOperator.Items.Add("Başlar");
        cmbOperator.Items.Add("Biter");
        cmbOperator.Items.Add("Boş mu?");
        cmbOperator.Items.Add("Boş değil mi?");
        cmbOperator.SelectedIndex = 0;

        // İlk özelliği seç
        if (cmbProperty.Items.Count > 0)
        {
            cmbProperty.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Özellik ekle (boş değilse)
    /// </summary>
    private void AddPropertyIfNotEmpty(string propertyName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            cmbProperty.Items.Add($"{propertyName} = {value}");
        }
    }

    /// <summary>
    /// Koşul ekle butonu
    /// </summary>
    private void BtnAddCondition_Click(object? sender, EventArgs e)
    {
        if (cmbProperty.SelectedItem == null)
        {
            MessageBox.Show("Lütfen bir özellik seçin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtValue.Text))
        {
            MessageBox.Show("Lütfen bir değer girin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Özellik adını ayıkla (örn: "IsEnabled = True" -> "IsEnabled")
        var propertyText = cmbProperty.SelectedItem.ToString() ?? "";
        var propertyName = propertyText.Split('=')[0].Trim();

        // Operatörü dönüştür
        var operatorText = cmbOperator.SelectedItem?.ToString() ?? "";
        var conditionOperator = ConvertToOperator(operatorText);

        // Yeni koşul oluştur
        var condition = new UICondition
        {
            Element = _currentElement,
            PropertyName = propertyName,
            Operator = conditionOperator,
            ExpectedValue = txtValue.Text.Trim(),
            LogicalOperator = LogicalOperator.None
        };

        // İlk koşul mu?
        if (_conditionInfo.Conditions.Count == 0)
        {
            _conditionInfo.Conditions.Add(condition);
        }
        else
        {
            // AND/OR seçeneği sun
            var result = MessageBox.Show(
                "Yeni koşul mevcut koşullarla nasıl birleştirilsin?\n\n" +
                "YES = AND (Tüm koşullar sağlanmalı)\n" +
                "NO = OR (Herhangi biri sağlanmalı)",
                "Koşul Bağlantısı",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
                return;

            // Önceki koşulun LogicalOperator'ünü güncelle
            _conditionInfo.Conditions[_conditionInfo.Conditions.Count - 1].LogicalOperator =
                result == DialogResult.Yes ? LogicalOperator.AND : LogicalOperator.OR;

            _conditionInfo.Conditions.Add(condition);
        }

        RefreshConditionsList();
        txtValue.Clear();

        // Parent formun butonlarını güncelle
        var parentForm = this.FindForm() as ConditionalBranchWizard;
        parentForm?.RefreshButtons();
    }

    /// <summary>
    /// Operatör string'ini enum'a çevir
    /// </summary>
    private ConditionOperator ConvertToOperator(string operatorText)
    {
        return operatorText switch
        {
            "== (Eşittir)" => ConditionOperator.Equals,
            "!= (Eşit Değil)" => ConditionOperator.NotEquals,
            "İçerir" => ConditionOperator.Contains,
            "İçermez" => ConditionOperator.NotContains,
            "Başlar" => ConditionOperator.StartsWith,
            "Biter" => ConditionOperator.EndsWith,
            "Boş mu?" => ConditionOperator.IsEmpty,
            "Boş değil mi?" => ConditionOperator.IsNotEmpty,
            _ => ConditionOperator.Equals
        };
    }

    /// <summary>
    /// Koşullar listesini yenile
    /// </summary>
    private void RefreshConditionsList()
    {
        lstConditions.Items.Clear();

        for (int i = 0; i < _conditionInfo.Conditions.Count; i++)
        {
            var condition = _conditionInfo.Conditions[i];
            var displayText = $"{condition.PropertyName} {GetOperatorSymbol(condition.Operator)} {condition.ExpectedValue}";

            if (i < _conditionInfo.Conditions.Count - 1)
            {
                displayText += condition.LogicalOperator == LogicalOperator.AND ? " AND" : " OR";
            }

            lstConditions.Items.Add(displayText);
        }
    }

    /// <summary>
    /// Operatör sembolünü al
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
            ConditionOperator.IsEmpty => "boş mu?",
            ConditionOperator.IsNotEmpty => "boş değil mi?",
            _ => "=="
        };
    }

    /// <summary>
    /// Seçili koşulu sil
    /// </summary>
    private void BtnRemoveCondition_Click(object? sender, EventArgs e)
    {
        if (lstConditions.SelectedIndex >= 0)
        {
            _conditionInfo.Conditions.RemoveAt(lstConditions.SelectedIndex);
            RefreshConditionsList();

            var parentForm = this.FindForm() as ConditionalBranchWizard;
            parentForm?.RefreshButtons();
        }
    }
}
