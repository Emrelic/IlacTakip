using System.Diagnostics;
using System.Windows.Automation;

namespace MedulaOtomasyon;

public partial class TaskChainRecorderForm : Form
{
    private TaskChain _currentChain;
    private TaskChainDatabase _database;
    private int _currentStepNumber = 1;
    private TaskStep _currentStep;
    private List<ElementLocatorStrategy> _availableStrategies = new();
    private ElementLocatorStrategy? _selectedStrategy = null;

    public TaskChainRecorderForm()
    {
        InitializeComponent();
        _database = new TaskChainDatabase();
        _currentChain = new TaskChain
        {
            CreatedDate = DateTime.Now,
            Steps = new List<TaskStep>()
        };

        _currentStep = new TaskStep
        {
            StepNumber = _currentStepNumber,
            StepType = StepType.TargetSelection
        };

        UpdateStepNumberLabel();
        LogMessage("Görev kaydedici başlatıldı. İlk adım için hedef seçin.");
    }

    private void UpdateStepNumberLabel()
    {
        lblCurrentStep.Text = $"Adım: {_currentStepNumber}";
    }

    private void LogMessage(string message)
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
    }

    private void cmbStepType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Tip seçimine göre ilgili GroupBox'ları göster/gizle
        grpTargetSelection.Visible = false;
        grpUIElementAction.Visible = false;

        switch (cmbStepType.SelectedIndex)
        {
            case 0: // Tip 1: Hedef Program/Pencere Seçimi
                grpTargetSelection.Visible = true;
                _currentStep.StepType = StepType.TargetSelection;
                LogMessage("Tip 1 seçildi: Hedef Program/Pencere Seçimi");
                break;

            case 1: // Tip 2: UI Element Tıklama/Tuşlama
                grpUIElementAction.Visible = true;
                _currentStep.StepType = StepType.UIElementAction;
                LogMessage("Tip 2 seçildi: UI Element Tıklama/Tuşlama");
                break;

            case 2: // Tip 3: Sayfa Durum Kontrolü
                MessageBox.Show("Tip 3: Sayfa Durum Kontrolü henüz uygulanmadı.\nYakında eklenecek.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbStepType.SelectedIndex = 0;
                break;

            case 3: // Tip 4: Döngü veya Bitiş Koşulu
                MessageBox.Show("Tip 4: Döngü veya Bitiş Koşulu henüz uygulanmadı.\nYakında eklenecek.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbStepType.SelectedIndex = 0;
                break;
        }
    }

    private void btnBrowse_Click(object? sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Title = "Hedef Program Seçin",
            Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
            FilterIndex = 1
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            txtProgramPath.Text = openFileDialog.FileName;
            _currentStep.Target = new TargetInfo
            {
                ProgramPath = openFileDialog.FileName,
                IsDesktop = false
            };
            LogMessage($"Program seçildi: {openFileDialog.FileName}");
        }
    }

    private void btnSelectWindow_Click(object? sender, EventArgs e)
    {
        LogMessage("Aktif pencere seçimi başlatılıyor...");
        LogMessage("5 saniye içinde hedef pencereyi aktif hale getirin...");

        // 5 saniye bekle
        Task.Delay(5000).ContinueWith(_ =>
        {
            if (InvokeRequired)
            {
                Invoke(() => CaptureActiveWindow());
            }
            else
            {
                CaptureActiveWindow();
            }
        });
    }

    private void CaptureActiveWindow()
    {
        try
        {
            var currentProcessId = Process.GetCurrentProcess().Id;
            var windows = AutomationElement.RootElement.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
            );

            AutomationElement? activeWindow = null;
            foreach (AutomationElement window in windows)
            {
                try
                {
                    var processId = window.Current.ProcessId;
                    if (processId != currentProcessId && !window.Current.IsOffscreen)
                    {
                        // En üstteki pencereyi bul (basit implementasyon)
                        activeWindow = window;
                        break;
                    }
                }
                catch { }
            }

            if (activeWindow != null)
            {
                var windowName = activeWindow.Current.Name;
                var windowClassName = activeWindow.Current.ClassName;
                var processId = activeWindow.Current.ProcessId;

                txtProgramPath.Text = $"Pencere: {windowName} (Class: {windowClassName})";
                _currentStep.Target = new TargetInfo
                {
                    WindowTitle = windowName,
                    WindowClassName = windowClassName,
                    ProcessId = processId,
                    IsDesktop = false
                };

                LogMessage($"Pencere yakalandı: {windowName}");
            }
            else
            {
                LogMessage("HATA: Aktif pencere bulunamadı!");
            }
        }
        catch (Exception ex)
        {
            LogMessage($"HATA: {ex.Message}");
        }
    }

    private void btnDesktop_Click(object? sender, EventArgs e)
    {
        txtProgramPath.Text = "Hedef: Masaüstü";
        _currentStep.Target = new TargetInfo
        {
            IsDesktop = true
        };
        LogMessage("Masaüstü hedef olarak seçildi.");
    }

    private void btnSaveStep_Click(object? sender, EventArgs e)
    {
        // Tip kontrolü
        if (_currentStep.StepType == StepType.TargetSelection)
        {
            if (_currentStep.Target == null)
            {
                MessageBox.Show("Lütfen önce bir hedef seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _currentStep.Description = $"Adım {_currentStepNumber}: Hedef - {txtProgramPath.Text}";
        }
        else if (_currentStep.StepType == StepType.UIElementAction)
        {
            if (_currentStep.UIElement == null)
            {
                MessageBox.Show("Lütfen önce bir UI element seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Strateji seçilmeli
            if (_selectedStrategy == null)
            {
                MessageBox.Show("Lütfen bir element bulma stratejisi seçin!\n\n" +
                              "1. Element Seç butonuna tıklayın\n" +
                              "2. Tüm Stratejileri Test Et'e tıklayın\n" +
                              "3. Listeden bir strateji seçin",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Başarısız strateji uyarısı
            if (!_selectedStrategy.IsSuccessful)
            {
                var result = MessageBox.Show(
                    $"Seçtiğiniz strateji test sırasında BAŞARISIZ oldu!\n\n" +
                    $"Strateji: {_selectedStrategy.Name}\n" +
                    $"Hata: {_selectedStrategy.ErrorMessage}\n\n" +
                    $"Yine de kaydetmek istiyor musunuz?",
                    "Başarısız Strateji Uyarısı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // Action tipini kaydet
            _currentStep.Action = cmbActionType.SelectedIndex switch
            {
                0 => ActionType.LeftClick,
                1 => ActionType.RightClick,
                2 => ActionType.DoubleClick,
                3 => ActionType.MouseWheel,
                4 => ActionType.KeyPress,
                5 => ActionType.TypeText,
                _ => ActionType.None
            };

            // Klavye tuşları veya metin
            if (_currentStep.Action == ActionType.KeyPress)
            {
                _currentStep.KeysToPress = txtKeysToPress.Text;
            }
            else if (_currentStep.Action == ActionType.TypeText)
            {
                _currentStep.TextToType = txtKeysToPress.Text;
            }

            // SEÇİLEN STRATEJİYİ KAYDET
            _currentStep.SelectedStrategy = _selectedStrategy;

            _currentStep.Description = $"Adım {_currentStepNumber}: {_currentStep.Action} - {_currentStep.UIElement.Name} [{_selectedStrategy.Name}]";
        }

        _currentChain.Steps.Add(_currentStep);
        LogMessage($"✓ Adım {_currentStepNumber} kaydedildi: {_currentStep.Description}");

        MessageBox.Show($"Adım {_currentStepNumber} başarıyla kaydedildi!", "Başarılı",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnTestStep_Click(object? sender, EventArgs e)
    {
        LogMessage("Test başlatılıyor...");

        try
        {
            if (_currentStep.StepType == StepType.TargetSelection)
            {
                TestTargetSelection();
            }
            else if (_currentStep.StepType == StepType.UIElementAction)
            {
                TestUIElementAction();
            }
        }
        catch (Exception ex)
        {
            LogMessage($"HATA: {ex.Message}");
            MessageBox.Show($"Test başarısız: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TestTargetSelection()
    {
        if (_currentStep.Target == null)
        {
            MessageBox.Show("Lütfen önce bir hedef seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_currentStep.Target.IsDesktop)
        {
            LogMessage("✓ Masaüstü hedefi - test başarılı.");
            MessageBox.Show("Masaüstü hedefi doğrulandı!", "Test Başarılı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else if (!string.IsNullOrEmpty(_currentStep.Target.ProgramPath))
        {
            // Program çalıştırmayı dene
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = _currentStep.Target.ProgramPath,
                UseShellExecute = true
            });

            if (process != null)
            {
                LogMessage($"✓ Program başlatıldı: {_currentStep.Target.ProgramPath}");
                MessageBox.Show("Program başarıyla başlatıldı!", "Test Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else if (!string.IsNullOrEmpty(_currentStep.Target.WindowTitle))
        {
            // Pencere var mı kontrol et
            var windows = AutomationElement.RootElement.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
            );

            bool found = false;
            foreach (AutomationElement window in windows)
            {
                try
                {
                    if (window.Current.Name == _currentStep.Target.WindowTitle)
                    {
                        found = true;
                        LogMessage($"✓ Pencere bulundu: {_currentStep.Target.WindowTitle}");
                        MessageBox.Show("Hedef pencere doğrulandı!", "Test Başarılı",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }
                }
                catch { }
            }

            if (!found)
            {
                LogMessage($"⚠ Pencere bulunamadı: {_currentStep.Target.WindowTitle}");
                MessageBox.Show("Hedef pencere şu anda bulunamadı!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void TestUIElementAction()
    {
        if (_currentStep.UIElement == null)
        {
            MessageBox.Show("Lütfen önce bir UI element seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LogMessage("UI Element test ediliyor...");
        LogMessage($"Element: {_currentStep.UIElement.Name}");
        LogMessage($"Action: {cmbActionType.Text}");

        // Basit test: Elementi tekrar bulmayı dene
        try
        {
            AutomationElement? element = null;

            // AutomationId ile bulmayı dene
            if (!string.IsNullOrEmpty(_currentStep.UIElement.AutomationId))
            {
                var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, _currentStep.UIElement.AutomationId);
                element = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
            }

            // Name ile bulmayı dene
            if (element == null && !string.IsNullOrEmpty(_currentStep.UIElement.Name))
            {
                var condition = new PropertyCondition(AutomationElement.NameProperty, _currentStep.UIElement.Name);
                element = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
            }

            if (element != null)
            {
                LogMessage("✓ Element bulundu ve erişilebilir!");
                MessageBox.Show($"Element test başarılı!\n\nElement: {_currentStep.UIElement.Name}\nİşlem: {cmbActionType.Text}",
                    "Test Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                LogMessage("⚠ Element bulunamadı!");
                MessageBox.Show("Element şu anda bulunamadı! Hedef pencere açık mı kontrol edin.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Element test hatası: {ex.Message}");
            throw;
        }
    }

    private void btnNextStep_Click(object? sender, EventArgs e)
    {
        if (!_currentChain.Steps.Any(s => s.StepNumber == _currentStepNumber))
        {
            MessageBox.Show("Lütfen önce mevcut adımı kaydedin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _currentStepNumber++;
        _currentStep = new TaskStep
        {
            StepNumber = _currentStepNumber,
            StepType = StepType.TargetSelection
        };

        UpdateStepNumberLabel();

        // Formu temizle
        txtProgramPath.Clear();
        txtElementProperties.Clear();
        txtKeysToPress.Clear();
        cmbStepType.SelectedIndex = 0; // Varsayılan: Tip 1

        LogMessage($"\n--- Yeni Adım: {_currentStepNumber} ---");
    }

    private void btnSaveChain_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtChainName.Text))
        {
            MessageBox.Show("Lütfen görev zincirine bir isim verin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtChainName.Focus();
            return;
        }

        if (_currentChain.Steps.Count == 0)
        {
            MessageBox.Show("Lütfen en az bir adım kaydedin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _currentChain.Name = txtChainName.Text;
            _currentChain.Description = $"{_currentChain.Steps.Count} adımlı görev zinciri";
            _database.Add(_currentChain);

            LogMessage($"✓✓✓ Görev zinciri kaydedildi: {_currentChain.Name}");
            LogMessage($"Database yolu: {_database.GetDatabasePath()}");

            MessageBox.Show($"Görev zinciri '{_currentChain.Name}' başarıyla kaydedildi!\n\n" +
                          $"Toplam {_currentChain.Steps.Count} adım kaydedildi.",
                "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"HATA: {ex.Message}");
            MessageBox.Show($"Kaydetme hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnPickElement_Click(object? sender, EventArgs e)
    {
        LogMessage("Element seçimi başlatılıyor...");
        LogMessage("3 saniye içinde hedef UI elementinin üzerine mouse'u getirin...");

        btnPickElement.Enabled = false;

        try
        {
            // 3 saniye bekle
            await Task.Delay(3000);

            // Mouse pozisyonundaki elementi yakala (async + 3 teknoloji entegrasyonu)
            var elementInfo = await UIElementPicker.CaptureElementAtMousePositionAsync();

            DisplayElementInfo(elementInfo);
        }
        catch (Exception ex)
        {
            LogMessage($"HATA: {ex.Message}");
            btnPickElement.Enabled = true;
        }
    }

    private void DisplayElementInfo(UIElementInfo? elementInfo)
    {
        btnPickElement.Enabled = true;

        if (elementInfo == null)
        {
            LogMessage("HATA: Element yakalanamadı!");
            MessageBox.Show("Element yakalanamadı! Lütfen tekrar deneyin.", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Element bilgilerini göster
        txtElementProperties.Clear();

        txtElementProperties.AppendText($"=== PENCERE BİLGİLERİ ==={Environment.NewLine}");
        AppendIfNotEmpty("WindowTitle", elementInfo.WindowTitle);
        AppendIfNotEmpty("WindowProcessName", elementInfo.WindowProcessName);
        AppendIfNotEmpty("WindowProcessId", elementInfo.WindowProcessId?.ToString());
        AppendIfNotEmpty("WindowClassName", elementInfo.WindowClassName);

        txtElementProperties.AppendText($"{Environment.NewLine}=== UI AUTOMATION ÖZELLİKLERİ ==={Environment.NewLine}");
        AppendIfNotEmpty("AutomationId", elementInfo.AutomationId);
        AppendIfNotEmpty("RuntimeId", elementInfo.RuntimeId);
        AppendIfNotEmpty("Name", elementInfo.Name);
        AppendIfNotEmpty("ClassName", elementInfo.ClassName);
        AppendIfNotEmpty("ControlType", elementInfo.ControlType);
        AppendIfNotEmpty("FrameworkId", elementInfo.FrameworkId);
        AppendIfNotEmpty("LocalizedControlType", elementInfo.LocalizedControlType);
        AppendIfNotEmpty("ItemType", elementInfo.ItemType);
        AppendIfNotEmpty("ItemStatus", elementInfo.ItemStatus);
        AppendIfNotEmpty("HelpText", elementInfo.HelpText);
        AppendIfNotEmpty("AcceleratorKey", elementInfo.AcceleratorKey);
        AppendIfNotEmpty("AccessKey", elementInfo.AccessKey);

        txtElementProperties.AppendText($"{Environment.NewLine}=== DURUM ÖZELLİKLERİ ==={Environment.NewLine}");
        AppendIfNotEmpty("IsEnabled", elementInfo.IsEnabled?.ToString());
        AppendIfNotEmpty("IsVisible", elementInfo.IsVisible?.ToString());
        AppendIfNotEmpty("IsOffscreen", elementInfo.IsOffscreen?.ToString());
        AppendIfNotEmpty("HasKeyboardFocus", elementInfo.HasKeyboardFocus?.ToString());
        AppendIfNotEmpty("IsKeyboardFocusable", elementInfo.IsKeyboardFocusable?.ToString());
        AppendIfNotEmpty("IsPassword", elementInfo.IsPassword?.ToString());
        AppendIfNotEmpty("IsContentElement", elementInfo.IsContentElement?.ToString());
        AppendIfNotEmpty("IsControlElement", elementInfo.IsControlElement?.ToString());

        txtElementProperties.AppendText($"{Environment.NewLine}=== HİYERARŞİ VE PATH ==={Environment.NewLine}");
        AppendIfNotEmpty("ElementPath", elementInfo.ElementPath);
        AppendIfNotEmpty("TreePath", elementInfo.TreePath);
        AppendIfNotEmpty("ParentChain", elementInfo.ParentChain);
        AppendIfNotEmpty("ParentName", elementInfo.ParentName);
        AppendIfNotEmpty("ParentAutomationId", elementInfo.ParentAutomationId);
        AppendIfNotEmpty("ParentClassName", elementInfo.ParentClassName);
        AppendIfNotEmpty("IndexInParent", elementInfo.IndexInParent?.ToString());

        txtElementProperties.AppendText($"{Environment.NewLine}=== ETİKET VE İLİŞKİLER ==={Environment.NewLine}");
        AppendIfNotEmpty("LabeledByElement", elementInfo.LabeledByElement);
        AppendIfNotEmpty("DescribedByElement", elementInfo.DescribedByElement);

        txtElementProperties.AppendText($"{Environment.NewLine}=== WEB/HTML ÖZELLİKLERİ ==={Environment.NewLine}");
        AppendIfNotEmpty("HtmlId", elementInfo.HtmlId);
        AppendIfNotEmpty("TagName", elementInfo.TagName);
        AppendIfNotEmpty("HtmlName", elementInfo.HtmlName);
        AppendIfNotEmpty("Type", elementInfo.Type);
        AppendIfNotEmpty("InnerText", elementInfo.InnerText);
        AppendIfNotEmpty("Value", elementInfo.Value);
        AppendIfNotEmpty("Href", elementInfo.Href);
        AppendIfNotEmpty("Src", elementInfo.Src);
        AppendIfNotEmpty("Alt", elementInfo.Alt);
        AppendIfNotEmpty("Placeholder", elementInfo.Placeholder);

        txtElementProperties.AppendText($"{Environment.NewLine}=== ARIA ÖZELLİKLERİ ==={Environment.NewLine}");
        AppendIfNotEmpty("AriaLabel", elementInfo.AriaLabel);
        AppendIfNotEmpty("AriaLabelledBy", elementInfo.AriaLabelledBy);
        AppendIfNotEmpty("AriaDescribedBy", elementInfo.AriaDescribedBy);
        AppendIfNotEmpty("AriaRole", elementInfo.AriaRole);
        AppendIfNotEmpty("AriaRequired", elementInfo.AriaRequired);
        AppendIfNotEmpty("AriaExpanded", elementInfo.AriaExpanded);
        AppendIfNotEmpty("AriaChecked", elementInfo.AriaChecked);
        AppendIfNotEmpty("AriaHidden", elementInfo.AriaHidden);

        txtElementProperties.AppendText($"{Environment.NewLine}=== SELECTORS ==={Environment.NewLine}");
        AppendIfNotEmpty("XPath", elementInfo.XPath);
        AppendIfNotEmpty("CssSelector", elementInfo.CssSelector);
        AppendIfNotEmpty("PlaywrightSelector", elementInfo.PlaywrightSelector);

        // data-* attributes
        if (elementInfo.DataAttributes != null && elementInfo.DataAttributes.Count > 0)
        {
            txtElementProperties.AppendText($"{Environment.NewLine}=== DATA-* ATTRIBUTES ==={Environment.NewLine}");
            foreach (var kvp in elementInfo.DataAttributes)
            {
                txtElementProperties.AppendText($"{kvp.Key}: {kvp.Value}{Environment.NewLine}");
            }
        }

        txtElementProperties.AppendText($"{Environment.NewLine}=== KONUM VE BOYUT ==={Environment.NewLine}");
        txtElementProperties.AppendText($"X: {elementInfo.X}, Y: {elementInfo.Y}{Environment.NewLine}");
        txtElementProperties.AppendText($"Width: {elementInfo.Width}, Height: {elementInfo.Height}{Environment.NewLine}");
        AppendIfNotEmpty("BoundingRectangle", elementInfo.BoundingRectangle);

        txtElementProperties.AppendText($"{Environment.NewLine}=== TEKNOLOJİ ==={Environment.NewLine}");
        txtElementProperties.AppendText($"DetectionMethod: {elementInfo.DetectionMethod}{Environment.NewLine}");

        // Element bilgilerini current step'e kaydet
        _currentStep.UIElement = elementInfo;

        LogMessage($"✓ Element yakalandı: {elementInfo.Name} ({elementInfo.ControlType})");
        LogMessage($"  FrameworkId: {elementInfo.FrameworkId}");
        LogMessage($"  TreePath: {elementInfo.TreePath}");

        // Stratejileri otomatik oluştur
        GenerateStrategies(elementInfo);
    }

    private void GenerateStrategies(UIElementInfo elementInfo)
    {
        LogMessage("\n--- Stratejiler Oluşturuluyor ---");

        // Stratejileri üret
        _availableStrategies = ElementLocatorTester.GenerateStrategies(elementInfo);

        // ListBox'ı temizle ve stratejileri ekle
        lstStrategies.Items.Clear();
        foreach (var strategy in _availableStrategies)
        {
            lstStrategies.Items.Add($"⚪ {strategy.Name} - {strategy.Description}");
        }

        // Strateji panelini göster
        grpStrategyTest.Visible = true;

        LogMessage($"✓ {_availableStrategies.Count} strateji oluşturuldu");
        LogMessage("Şimdi 'Tüm Stratejileri Test Et' butonuna tıklayın.");
    }

    private void AppendIfNotEmpty(string label, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            txtElementProperties.AppendText($"{label}: {value}{Environment.NewLine}");
        }
    }

    private void cmbActionType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Klavye Tuşları veya Metin Yaz seçildiğinde input alanını göster
        bool showKeysInput = cmbActionType.SelectedIndex == 4 || cmbActionType.SelectedIndex == 5;
        lblKeysToPress.Visible = showKeysInput;
        txtKeysToPress.Visible = showKeysInput;

        if (showKeysInput)
        {
            if (cmbActionType.SelectedIndex == 4)
            {
                lblKeysToPress.Text = "Klavye Tuşları:";
                txtKeysToPress.PlaceholderText = "Örn: {ENTER}, {TAB}, {F5}...";
            }
            else
            {
                lblKeysToPress.Text = "Yazılacak Metin:";
                txtKeysToPress.PlaceholderText = "Yazılacak metni girin...";
            }
        }
    }

    private async void btnTestAllStrategies_Click(object? sender, EventArgs e)
    {
        if (_availableStrategies.Count == 0)
        {
            MessageBox.Show("Önce bir element seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnTestAllStrategies.Enabled = false;
        btnTestAllStrategies.Text = "⏳ Test Ediliyor...";
        LogMessage("\n=== STRATEJİ TESTLERİ BAŞLATILIYOR ===");

        lstStrategies.Items.Clear();

        int successCount = 0;
        int failCount = 0;

        for (int i = 0; i < _availableStrategies.Count; i++)
        {
            var strategy = _availableStrategies[i];
            LogMessage($"Test #{i + 1}: {strategy.Name}...");

            // Stratejiyi test et
            var testedStrategy = await ElementLocatorTester.TestStrategy(strategy);
            _availableStrategies[i] = testedStrategy;

            // Sonucu göster
            string icon = testedStrategy.IsSuccessful ? "✅" : "❌";
            string result = testedStrategy.IsSuccessful
                ? $"Başarılı ({testedStrategy.TestDurationMs}ms)"
                : $"Başarısız: {testedStrategy.ErrorMessage}";

            lstStrategies.Items.Add($"{icon} {testedStrategy.Name} - {result}");

            if (testedStrategy.IsSuccessful)
            {
                successCount++;
                LogMessage($"  ✅ Başarılı! ({testedStrategy.TestDurationMs}ms)");
            }
            else
            {
                failCount++;
                LogMessage($"  ❌ Başarısız: {testedStrategy.ErrorMessage}");
            }

            // UI güncellenmesi için kısa bekle
            await Task.Delay(50);
        }

        btnTestAllStrategies.Enabled = true;
        btnTestAllStrategies.Text = "🧪 Tüm Stratejileri Test Et";

        LogMessage($"\n✅ Test tamamlandı: {successCount} başarılı, {failCount} başarısız");
        LogMessage("Bir strateji seçin ve 'Adımı Kaydet' butonuna tıklayın.");

        MessageBox.Show($"Test tamamlandı!\n\nBaşarılı: {successCount}\nBaşarısız: {failCount}",
            "Test Sonucu", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void lstStrategies_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstStrategies.SelectedIndex >= 0 && lstStrategies.SelectedIndex < _availableStrategies.Count)
        {
            _selectedStrategy = _availableStrategies[lstStrategies.SelectedIndex];
            lblSelectedStrategy.Text = $"Seçili Strateji: {_selectedStrategy.Name}";
            lblSelectedStrategy.ForeColor = _selectedStrategy.IsSuccessful ? Color.Green : Color.Red;

            LogMessage($"\n✓ Strateji seçildi: {_selectedStrategy.Name}");
            if (_selectedStrategy.IsSuccessful)
            {
                LogMessage($"  Bu strateji ile element {_selectedStrategy.TestDurationMs}ms'de bulunabiliyor.");
            }
            else
            {
                LogMessage($"  ⚠ DİKKAT: Bu strateji başarısız! Yine de kaydetmek istiyor musunuz?");
            }
        }
    }

    private void btnClose_Click(object? sender, EventArgs e)
    {
        Close();
    }
}
