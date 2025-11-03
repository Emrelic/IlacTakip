namespace MedulaOtomasyon;

/// <summary>
/// Wizard Adım 2: Element Seçimi
/// </summary>
public partial class Step2_ElementSelection : UserControl, IWizardStep
{
    private ConditionInfo _conditionInfo;
    private UIElementInfo? _selectedElement;
    private List<UIElementInfo> _availableElements = new();

    public Step2_ElementSelection(ConditionInfo conditionInfo)
    {
        InitializeComponent();
        _conditionInfo = conditionInfo;
    }

    public void OnStepEnter()
    {
        lblPageInfo.Text = $"📄 Sayfa: {_conditionInfo.PageIdentifier ?? "Belirtilmedi"}";
    }

    public bool OnStepExit()
    {
        if (_selectedElement == null)
        {
            MessageBox.Show("Lütfen bir UI elementi seçin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // İlk condition için element olarak kaydet (geçici)
        if (_conditionInfo.Conditions.Count == 0)
        {
            _conditionInfo.Conditions.Add(new UICondition { Element = _selectedElement });
        }
        else
        {
            _conditionInfo.Conditions[0].Element = _selectedElement;
        }

        return true;
    }

    public bool CanProceed()
    {
        return _selectedElement != null;
    }

    /// <summary>
    /// Mouse ile element seç
    /// </summary>
    private async void BtnSelectWithMouse_Click(object? sender, EventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                "Tamam'a bastıktan sonra 2 saniye içinde\nUI elementine tıklayın!",
                "Element Seç",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (result != DialogResult.OK)
                return;

            var parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.Opacity = 0.3;
            }

            await Task.Delay(2000);

            var selectedElement = await UIElementPicker.CaptureElementAtMousePositionAsync();

            if (selectedElement != null)
            {
                _selectedElement = selectedElement;
                _availableElements.Add(selectedElement);

                lblSelectedElement.Text = $"✅ {selectedElement.ControlType ?? "Element"}: {selectedElement.Name ?? selectedElement.AutomationId ?? "İsimsiz"}";
                lblSelectedElement.ForeColor = System.Drawing.Color.DarkGreen;

                // Parent formun butonlarını güncelle
                if (parentForm is ConditionalBranchWizard wizardForm)
                {
                    wizardForm.RefreshButtons();
                }
            }

            if (parentForm != null)
            {
                parentForm.Opacity = 1.0;
                parentForm.BringToFront();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Element seçim hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            var parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.Opacity = 1.0;
            }
        }
    }
}
