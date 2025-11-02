namespace MedulaOtomasyon;

public partial class Step4_BranchPaths : UserControl, IWizardStep
{
    private ConditionInfo _conditionInfo;

    public Step4_BranchPaths(ConditionInfo conditionInfo)
    {
        InitializeComponent();
        _conditionInfo = conditionInfo;
    }

    public void OnStepEnter()
    {
        // Koşul özetini göster
        RefreshConditionSummary();

        // Dallanma türünü ayarla
        cmbBranchType.Items.Clear();
        cmbBranchType.Items.Add("Boolean (TRUE/FALSE)");
        cmbBranchType.Items.Add("Switch-Case (Çoklu Dal)");
        cmbBranchType.SelectedIndex = 0;

        // Mevcut branch bilgilerini yükle
        LoadExistingBranches();
    }

    public bool OnStepExit()
    {
        // En az TRUE ve FALSE dalları tanımlanmış olmalı
        var hasTrueBranch = _conditionInfo.Branches.Any(b => b.ConditionValue.ToLower() == "true");
        var hasFalseBranch = _conditionInfo.Branches.Any(b => b.ConditionValue.ToLower() == "false");

        if (!hasTrueBranch || !hasFalseBranch)
        {
            MessageBox.Show("Lütfen hem TRUE hem FALSE dallarını tanımlayın!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // BranchType'ı kaydet
        _conditionInfo.BranchType = cmbBranchType.SelectedIndex == 0 ? "Boolean" : "SwitchCase";

        return true;
    }

    public bool CanProceed()
    {
        var hasTrueBranch = _conditionInfo.Branches.Any(b => b.ConditionValue.ToLower() == "true");
        var hasFalseBranch = _conditionInfo.Branches.Any(b => b.ConditionValue.ToLower() == "false");
        return hasTrueBranch && hasFalseBranch;
    }

    /// <summary>
    /// Koşul özetini göster
    /// </summary>
    private void RefreshConditionSummary()
    {
        var summary = "Tanımlı Koşullar:\n";
        for (int i = 0; i < _conditionInfo.Conditions.Count; i++)
        {
            var condition = _conditionInfo.Conditions[i];
            summary += $"  {i + 1}. {condition.PropertyName} {GetOperatorSymbol(condition.Operator)} {condition.ExpectedValue}";

            if (i < _conditionInfo.Conditions.Count - 1)
            {
                summary += condition.LogicalOperator == LogicalOperator.AND ? " AND\n" : " OR\n";
            }
            else
            {
                summary += "\n";
            }
        }

        lblConditionSummary.Text = summary;
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
            _ => "=="
        };
    }

    /// <summary>
    /// Mevcut branch'leri yükle
    /// </summary>
    private void LoadExistingBranches()
    {
        // TRUE dalı
        var trueBranch = _conditionInfo.Branches.FirstOrDefault(b => b.ConditionValue.ToLower() == "true");
        if (trueBranch != null)
        {
            txtTrueTargetStep.Text = trueBranch.TargetStepId;
            txtTrueDescription.Text = trueBranch.Description ?? "";
        }

        // FALSE dalı
        var falseBranch = _conditionInfo.Branches.FirstOrDefault(b => b.ConditionValue.ToLower() == "false");
        if (falseBranch != null)
        {
            txtFalseTargetStep.Text = falseBranch.TargetStepId;
            txtFalseDescription.Text = falseBranch.Description ?? "";
        }

        // Default dal
        if (!string.IsNullOrEmpty(_conditionInfo.DefaultBranchStepId))
        {
            txtDefaultStep.Text = _conditionInfo.DefaultBranchStepId;
        }
    }

    /// <summary>
    /// Kaydet butonu
    /// </summary>
    private void BtnSave_Click(object? sender, EventArgs e)
    {
        // Validasyon
        if (string.IsNullOrWhiteSpace(txtTrueTargetStep.Text))
        {
            MessageBox.Show("Lütfen TRUE dalı için hedef adım girin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtFalseTargetStep.Text))
        {
            MessageBox.Show("Lütfen FALSE dalı için hedef adım girin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Branch'leri temizle ve yeniden ekle
        _conditionInfo.Branches.Clear();

        // TRUE dalı
        _conditionInfo.Branches.Add(new BranchTarget
        {
            BranchName = "TRUE",
            ConditionValue = "true",
            TargetStepId = txtTrueTargetStep.Text.Trim(),
            Description = txtTrueDescription.Text.Trim()
        });

        // FALSE dalı
        _conditionInfo.Branches.Add(new BranchTarget
        {
            BranchName = "FALSE",
            ConditionValue = "false",
            TargetStepId = txtFalseTargetStep.Text.Trim(),
            Description = txtFalseDescription.Text.Trim()
        });

        // Default dal
        _conditionInfo.DefaultBranchStepId = txtDefaultStep.Text.Trim();

        MessageBox.Show("Dallanma yolları kaydedildi!", "Başarılı",
            MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Parent formun butonlarını güncelle
        var parentForm = this.FindForm() as ConditionalBranchWizard;
        parentForm?.RefreshButtons();
    }

    /// <summary>
    /// Otomatik adım öneri - TRUE dalı
    /// </summary>
    private void BtnSuggestTrue_Click(object? sender, EventArgs e)
    {
        // Mevcut adım numarasını ikiye böl - A dalı
        int currentStepNumber = _conditionInfo.PreviousStepNumber;
        txtTrueTargetStep.Text = $"{currentStepNumber}A";
        txtTrueDescription.Text = "Koşul sağlandı - A yolu";
    }

    /// <summary>
    /// Otomatik adım öneri - FALSE dalı
    /// </summary>
    private void BtnSuggestFalse_Click(object? sender, EventArgs e)
    {
        // Mevcut adım numarasını ikiye böl - B dalı
        int currentStepNumber = _conditionInfo.PreviousStepNumber;
        txtFalseTargetStep.Text = $"{currentStepNumber}B";
        txtFalseDescription.Text = "Koşul sağlanmadı - B yolu";
    }
}
