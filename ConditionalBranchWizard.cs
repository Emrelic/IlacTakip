using System.ComponentModel;

namespace MedulaOtomasyon;

/// <summary>
/// Koşullu Dallanma Sihirbazı - Adım adım koşul tanımlama
/// </summary>
public partial class ConditionalBranchWizard : Form
{
    private int _currentStep = 0;
    private List<UserControl> _steps;
    private ConditionInfo _conditionInfo;

    // Wizard sonucu
    public ConditionInfo? Result { get; private set; }

    // Wizard adımları
    private Step1_PageSelection? _step1;
    private Step2_ElementSelection? _step2;
    private Step3_ConditionSetup? _step3;
    private Step4_BranchPaths? _step4;
    private Step5_Summary? _step5;

    public ConditionalBranchWizard(int previousStepNumber = 0)
    {
        InitializeComponent();
        _conditionInfo = new ConditionInfo
        {
            PreviousStepNumber = previousStepNumber
        };
        _steps = new List<UserControl>();

        InitializeWizardSteps();
        ShowStep(0);
    }

    /// <summary>
    /// Wizard adımlarını başlat
    /// </summary>
    private void InitializeWizardSteps()
    {
        // Adım 1: Hedef Sayfa Seçimi
        _step1 = new Step1_PageSelection(_conditionInfo);
        _steps.Add(_step1);

        // Adım 2: Element Seçimi
        _step2 = new Step2_ElementSelection(_conditionInfo);
        _steps.Add(_step2);

        // Adım 3: Koşul Tanımlama
        _step3 = new Step3_ConditionSetup(_conditionInfo);
        _steps.Add(_step3);

        // Adım 4: Dallanma Yolları
        _step4 = new Step4_BranchPaths(_conditionInfo);
        _steps.Add(_step4);

        // Adım 5: Özet ve Kaydet
        _step5 = new Step5_Summary(_conditionInfo);
        _steps.Add(_step5);

        // Her adımı panele ekle
        foreach (var step in _steps)
        {
            step.Dock = DockStyle.Fill;
            step.Visible = false;
            pnlStepContainer.Controls.Add(step);
        }
    }

    /// <summary>
    /// Belirtilen adımı göster
    /// </summary>
    private void ShowStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= _steps.Count)
            return;

        // Önceki adımı gizle
        if (_currentStep >= 0 && _currentStep < _steps.Count)
        {
            _steps[_currentStep].Visible = false;
        }

        // Yeni adımı göster
        _currentStep = stepIndex;
        _steps[_currentStep].Visible = true;

        // Başlık güncelle
        lblStepTitle.Text = $"Adım {_currentStep + 1} / {_steps.Count}";
        lblStepDescription.Text = GetStepDescription(_currentStep);

        // Buton durumlarını güncelle
        UpdateButtons();

        // Adıma giriş yap
        if (_steps[_currentStep] is IWizardStep wizardStep)
        {
            wizardStep.OnStepEnter();
        }
    }

    /// <summary>
    /// Adım açıklamasını al
    /// </summary>
    private string GetStepDescription(int step)
    {
        return step switch
        {
            0 => "📄 Hedef Sayfa Seçimi",
            1 => "🎯 UI Element Seçimi",
            2 => "⚙️ Koşul Tanımlama",
            3 => "🔀 Dallanma Yolları",
            4 => "✅ Özet ve Kaydet",
            _ => ""
        };
    }

    /// <summary>
    /// Buton durumlarını güncelle
    /// </summary>
    private void UpdateButtons()
    {
        btnBack.Enabled = _currentStep > 0;
        btnNext.Text = _currentStep == _steps.Count - 1 ? "💾 Kaydet" : "İleri →";
        btnNext.Enabled = CanProceedToNextStep();
    }

    /// <summary>
    /// Butonları yenile (public - adımlardan çağrılabilir)
    /// </summary>
    public void RefreshButtons()
    {
        UpdateButtons();
    }

    /// <summary>
    /// Bir sonraki adıma geçilebilir mi?
    /// </summary>
    private bool CanProceedToNextStep()
    {
        if (_steps[_currentStep] is IWizardStep wizardStep)
        {
            return wizardStep.CanProceed();
        }
        return true;
    }

    /// <summary>
    /// İleri butonu
    /// </summary>
    private void BtnNext_Click(object? sender, EventArgs e)
    {
        // Adımdan çıkış yap
        if (_steps[_currentStep] is IWizardStep wizardStep)
        {
            if (!wizardStep.OnStepExit())
            {
                MessageBox.Show("Lütfen gerekli alanları doldurun!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        // Son adımda mıyız?
        if (_currentStep == _steps.Count - 1)
        {
            // Kaydet ve kapat
            Result = _conditionInfo;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            // Bir sonraki adıma geç
            ShowStep(_currentStep + 1);
        }
    }

    /// <summary>
    /// Geri butonu
    /// </summary>
    private void BtnBack_Click(object? sender, EventArgs e)
    {
        if (_currentStep > 0)
        {
            ShowStep(_currentStep - 1);
        }
    }

    /// <summary>
    /// İptal butonu
    /// </summary>
    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Sihirbazdan çıkmak istediğinize emin misiniz?\nYaptığınız değişiklikler kaybolacak.",
            "Çıkış Onayı",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            Result = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

/// <summary>
/// Wizard adımı için interface
/// </summary>
public interface IWizardStep
{
    /// <summary>
    /// Adıma girildiğinde çağrılır
    /// </summary>
    void OnStepEnter();

    /// <summary>
    /// Adımdan çıkarken çağrılır
    /// </summary>
    /// <returns>Çıkılabilirse true</returns>
    bool OnStepExit();

    /// <summary>
    /// Bir sonraki adıma geçilebilir mi?
    /// </summary>
    bool CanProceed();
}
