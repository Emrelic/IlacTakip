namespace MedulaOtomasyon;

public partial class Step5_Summary : UserControl, IWizardStep
{
    private ConditionInfo _conditionInfo;

    public Step5_Summary(ConditionInfo conditionInfo)
    {
        InitializeComponent();
        _conditionInfo = conditionInfo;
    }

    public void OnStepEnter()
    {
        PopulateSummary();
    }

    public bool OnStepExit() { return true; }
    public bool CanProceed() { return true; }

    /// <summary>
    /// Tüm özet bilgilerini doldur
    /// </summary>
    private void PopulateSummary()
    {
        // 1. Sayfa Bilgisi
        lblPageValue.Text = _conditionInfo.PageIdentifier ?? "Belirtilmedi";

        // 2. Element Bilgisi
        var element = _conditionInfo.Conditions.FirstOrDefault()?.Element;
        if (element != null)
        {
            var elementInfo = "";
            if (!string.IsNullOrEmpty(element.ControlType))
                elementInfo += $"Type: {element.ControlType}  ";
            if (!string.IsNullOrEmpty(element.Name))
                elementInfo += $"Name: {element.Name}  ";
            if (!string.IsNullOrEmpty(element.AutomationId))
                elementInfo += $"AutomationId: {element.AutomationId}  ";
            if (!string.IsNullOrEmpty(element.ClassName))
                elementInfo += $"ClassName: {element.ClassName}";

            txtElementInfo.Text = string.IsNullOrEmpty(elementInfo) ? "Element bilgisi yok" : elementInfo;
        }
        else
        {
            txtElementInfo.Text = "❌ Element seçilmedi";
        }

        // 3. Koşullar
        if (_conditionInfo.Conditions.Count > 0)
        {
            var conditionsText = "";
            for (int i = 0; i < _conditionInfo.Conditions.Count; i++)
            {
                var condition = _conditionInfo.Conditions[i];
                conditionsText += $"{i + 1}. {condition.PropertyName} {GetOperatorSymbol(condition.Operator)} \"{condition.ExpectedValue}\"";

                if (i < _conditionInfo.Conditions.Count - 1)
                {
                    conditionsText += condition.LogicalOperator == LogicalOperator.AND ? "\r\n   AND\r\n" : "\r\n   OR\r\n";
                }
                else
                {
                    conditionsText += "\r\n";
                }
            }
            txtConditions.Text = conditionsText;
        }
        else
        {
            txtConditions.Text = "❌ Koşul tanımlanmadı";
        }

        // 4. Dallanma Yolları
        if (_conditionInfo.Branches.Count > 0)
        {
            var branchesText = "";

            var trueBranch = _conditionInfo.Branches.FirstOrDefault(b => b.ConditionValue.ToLower() == "true");
            if (trueBranch != null)
            {
                branchesText += $"✅ TRUE → {trueBranch.TargetStepId}";
                if (!string.IsNullOrEmpty(trueBranch.Description))
                    branchesText += $"\r\n   ({trueBranch.Description})";
                branchesText += "\r\n\r\n";
            }

            var falseBranch = _conditionInfo.Branches.FirstOrDefault(b => b.ConditionValue.ToLower() == "false");
            if (falseBranch != null)
            {
                branchesText += $"❌ FALSE → {falseBranch.TargetStepId}";
                if (!string.IsNullOrEmpty(falseBranch.Description))
                    branchesText += $"\r\n   ({falseBranch.Description})";
                branchesText += "\r\n\r\n";
            }

            if (!string.IsNullOrEmpty(_conditionInfo.DefaultBranchStepId))
            {
                branchesText += $"🔧 DEFAULT (Hata) → {_conditionInfo.DefaultBranchStepId}";
            }

            txtBranches.Text = branchesText.Trim();
        }
        else
        {
            txtBranches.Text = "❌ Dallanma yolu tanımlanmadı";
        }
    }

    /// <summary>
    /// Operatör sembolü
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
    /// Kaydet butonu - Wizard'ı kapat ve sonucu döndür
    /// </summary>
    private void BtnSave_Click(object? sender, EventArgs e)
    {
        // Parent wizard formunu bul ve kapat
        var parentForm = this.FindForm();
        if (parentForm != null)
        {
            parentForm.DialogResult = DialogResult.OK;
            parentForm.Close();
        }
    }

    /// <summary>
    /// İptal butonu - Wizard'ı kapat
    /// </summary>
    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Koşullu dallanma tanımını iptal etmek istediğinizden emin misiniz?\nTüm değişiklikler kaybedilecek!",
            "İptal Onayı",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            var parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.DialogResult = DialogResult.Cancel;
                parentForm.Close();
            }
        }
    }
}
