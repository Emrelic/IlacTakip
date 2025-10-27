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
    private CancellationTokenSource? _testCancellationTokenSource = null;

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

        // Formu sağ alt köşede aç
        this.Load += TaskChainRecorderForm_Load;
    }

    private void TaskChainRecorderForm_Load(object? sender, EventArgs e)
    {
        // Ekranın çalışma alanını al
        var workingArea = Screen.PrimaryScreen!.WorkingArea;

        // Formun sağ alt köşe pozisyonunu hesapla
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(
            workingArea.Right - this.Width,
            workingArea.Bottom - this.Height
        );
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
                ShowMessage("Tip 3: Sayfa Durum Kontrolü henüz uygulanmadı.\nYakında eklenecek.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbStepType.SelectedIndex = 0;
                break;

            case 3: // Tip 4: Döngü veya Bitiş Koşulu
                ShowMessage("Tip 4: Döngü veya Bitiş Koşulu henüz uygulanmadı.\nYakında eklenecek.",
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

        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
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
                ShowMessage("Lütfen önce bir hedef seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _currentStep.Description = $"Adım {_currentStepNumber}: Hedef - {txtProgramPath.Text}";
        }
        else if (_currentStep.StepType == StepType.UIElementAction)
        {
            if (_currentStep.UIElement == null)
            {
                ShowMessage("Lütfen önce bir UI element seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Strateji seçilmeli
            if (_selectedStrategy == null)
            {
                ShowMessage("Lütfen bir element bulma stratejisi seçin!\n\n" +
                              "1. Element Seç butonuna tıklayın\n" +
                              "2. Tüm Stratejileri Test Et'e tıklayın\n" +
                              "3. Listeden bir strateji seçin",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Başarısız strateji uyarısı
            if (!_selectedStrategy.IsSuccessful)
            {
                var result = ShowMessage(
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

        ShowMessage($"Adım {_currentStepNumber} başarıyla kaydedildi!", "Başarılı",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnTestStep_Click(object? sender, EventArgs e)
    {
        LogMessage("Test başlatılıyor...");

        // Test sonuç labelını temizle
        if (lblTestResult != null)
        {
            lblTestResult.Text = "⏳ Test ediliyor...";
            lblTestResult.ForeColor = Color.Blue;
        }

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
            if (lblTestResult != null)
            {
                lblTestResult.Text = $"❌ Test Başarısız: {ex.Message}";
                lblTestResult.ForeColor = Color.Red;
            }
        }
    }

    private void TestTargetSelection()
    {
        if (_currentStep.Target == null)
        {
            ShowMessage("Lütfen önce bir hedef seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_currentStep.Target.IsDesktop)
        {
            LogMessage("✓ Masaüstü hedefi - test başarılı.");
            if (lblTestResult != null)
            {
                lblTestResult.Text = "✅ Test Başarılı - Masaüstü hedefi doğrulandı";
                lblTestResult.ForeColor = Color.Green;
            }
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
                if (lblTestResult != null)
                {
                    lblTestResult.Text = "✅ Test Başarılı - Program başlatıldı";
                    lblTestResult.ForeColor = Color.Green;
                }
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
                        if (lblTestResult != null)
                        {
                            lblTestResult.Text = "✅ Test Başarılı - Hedef pencere doğrulandı";
                            lblTestResult.ForeColor = Color.Green;
                        }
                        break;
                    }
                }
                catch { }
            }

            if (!found)
            {
                LogMessage($"⚠ Pencere bulunamadı: {_currentStep.Target.WindowTitle}");
                if (lblTestResult != null)
                {
                    lblTestResult.Text = "⚠ Hedef pencere bulunamadı";
                    lblTestResult.ForeColor = Color.Orange;
                }
            }
        }
    }

    private void TestUIElementAction()
    {
        if (_currentStep.UIElement == null)
        {
            ShowMessage("Lütfen önce bir UI element seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Strateji seçilmemiş ise uyar
        if (_selectedStrategy == null)
        {
            ShowMessage("Lütfen önce 'Tüm Stratejileri Test Et' butonuna tıklayıp bir strateji seçin!",
                "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LogMessage("UI Element test ediliyor...");
        LogMessage($"Element: {_currentStep.UIElement.Name}");
        LogMessage($"Strateji: {_selectedStrategy.Name}");
        LogMessage($"Action: {cmbActionType.Text}");

        // DEBUG: Window bilgisini logla
        LogMessage($"Window: {_currentStep.UIElement.WindowTitle ?? "N/A"}");
        LogMessage($"ProcessId: {_currentStep.UIElement.WindowProcessId?.ToString() ?? "N/A"}");

        try
        {
            // Seçili strateji ile elementi bul (windowInfo ile hızlandırma)
            var element = ElementLocatorTester.FindElementByStrategy(_selectedStrategy, _currentStep.UIElement);

            if (element == null)
            {
                LogMessage("⚠ Element bulunamadı!");
                if (lblTestResult != null)
                {
                    lblTestResult.Text = $"⚠ Element bulunamadı - Strateji: {_selectedStrategy.Name}";
                    lblTestResult.ForeColor = Color.Orange;
                }
                return;
            }

            LogMessage($"✓ Element bulundu: {element.Current.Name}");

            // Action tipini al
            var action = cmbActionType.SelectedIndex switch
            {
                0 => ActionType.LeftClick,
                1 => ActionType.RightClick,
                2 => ActionType.DoubleClick,
                3 => ActionType.MouseWheel,
                4 => ActionType.KeyPress,
                5 => ActionType.TypeText,
                _ => ActionType.None
            };

            // Eylemi gerçekleştir
            ExecuteTestAction(element, action, txtKeysToPress.Text);

            LogMessage($"✓ Eylem başarıyla gerçekleştirildi: {cmbActionType.Text}");
            if (lblTestResult != null)
            {
                lblTestResult.Text = $"✅ Test Başarılı - {cmbActionType.Text} gerçekleştirildi";
                lblTestResult.ForeColor = Color.Green;
            }
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Test hatası: {ex.Message}");
            if (lblTestResult != null)
            {
                lblTestResult.Text = $"❌ Test Başarısız - {ex.Message}";
                lblTestResult.ForeColor = Color.Red;
            }
        }
    }

    private void ExecuteTestAction(AutomationElement element, ActionType action, string inputText)
    {
        switch (action)
        {
            case ActionType.LeftClick:
                LogMessage("Sol tıklama yapılıyor...");
                ClickElement(element);
                break;

            case ActionType.DoubleClick:
                LogMessage("Çift tıklama yapılıyor...");
                DoubleClickElement(element);
                break;

            case ActionType.TypeText:
                LogMessage($"Metin yazılıyor: {inputText}");
                TypeText(element, inputText);
                break;

            case ActionType.KeyPress:
                LogMessage($"Klavye tuşları gönderiliyor: {inputText}");
                PressKeys(element, inputText);
                break;

            case ActionType.RightClick:
                LogMessage("Sağ tıklama henüz desteklenmiyor.");
                throw new NotSupportedException("Sağ tıklama henüz implement edilmedi.");

            case ActionType.MouseWheel:
                LogMessage("Mouse wheel henüz desteklenmiyor.");
                throw new NotSupportedException("Mouse wheel henüz implement edilmedi.");

            default:
                throw new NotSupportedException($"Action type desteklenmiyor: {action}");
        }
    }

    private void ClickElement(AutomationElement element)
    {
        if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object? pattern) &&
            pattern is InvokePattern invokePattern)
        {
            invokePattern.Invoke();
        }
        else
        {
            // Fallback: Mouse click
            var rect = element.Current.BoundingRectangle;
            var centerX = (int)(rect.Left + rect.Width / 2);
            var centerY = (int)(rect.Top + rect.Height / 2);
            MedulaAutomation.MouseClick(centerX, centerY);
        }
    }

    private void DoubleClickElement(AutomationElement element)
    {
        var rect = element.Current.BoundingRectangle;
        var centerX = (int)(rect.Left + rect.Width / 2);
        var centerY = (int)(rect.Top + rect.Height / 2);
        MedulaAutomation.MouseClick(centerX, centerY);
        Thread.Sleep(50);
        MedulaAutomation.MouseClick(centerX, centerY);
    }

    private void TypeText(AutomationElement element, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            LogMessage("⚠ Yazılacak metin boş!");
            return;
        }

        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? pattern) &&
            pattern is ValuePattern valuePattern)
        {
            valuePattern.SetValue(text);
        }
        else
        {
            // Fallback: SendKeys
            element.SetFocus();
            System.Windows.Forms.SendKeys.SendWait(text);
        }
    }

    private void PressKeys(AutomationElement element, string keys)
    {
        if (string.IsNullOrEmpty(keys))
        {
            LogMessage("⚠ Gönderilecek tuş boş!");
            return;
        }

        element.SetFocus();
        System.Windows.Forms.SendKeys.SendWait(keys);
    }

    private void btnNextStep_Click(object? sender, EventArgs e)
    {
        if (!_currentChain.Steps.Any(s => s.StepNumber == _currentStepNumber))
        {
            ShowMessage("Lütfen önce mevcut adımı kaydedin!", "Uyarı",
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
            ShowMessage("Lütfen görev zincirine bir isim verin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtChainName.Focus();
            return;
        }

        if (_currentChain.Steps.Count == 0)
        {
            ShowMessage("Lütfen en az bir adım kaydedin!", "Uyarı",
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

            ShowMessage($"Görev zinciri '{_currentChain.Name}' başarıyla kaydedildi!\n\n" +
                          $"Toplam {_currentChain.Steps.Count} adım kaydedildi.",
                "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"HATA: {ex.Message}");
            ShowMessage($"Kaydetme hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnAnalyzeStructure_Click(object? sender, EventArgs e)
    {
        LogMessage("\n🔍 === YAPI ANALİZİ BAŞLATILIYOR ===");
        LogMessage("5 saniye içinde analiz edilecek pencereyi aktif hale getirin...");

        btnAnalyzeStructure.Enabled = false;

        try
        {
            await Task.Delay(5000);

            // Aktif pencereyi yakala
            var foregroundWindow = AutomationElement.FocusedElement;
            if (foregroundWindow == null)
            {
                LogMessage("❌ Aktif pencere bulunamadı!");
                return;
            }

            // Window elementini bul
            var window = foregroundWindow;
            while (window != null && window.Current.ControlType != ControlType.Window)
            {
                try
                {
                    window = TreeWalker.RawViewWalker.GetParent(window);
                }
                catch
                {
                    break;
                }
            }

            if (window == null)
            {
                LogMessage("❌ Window elementi bulunamadı!");
                return;
            }

            LogMessage($"✅ Pencere yakalandı: {window.Current.Name}");
            LogMessage($"   ProcessId: {window.Current.ProcessId}");
            LogMessage($"   ClassName: {window.Current.ClassName}");
            LogMessage("");

            // Window'un tüm child elementlerini analiz et
            LogMessage("📊 WINDOW YAPISINI ANALİZ EDİYORUM...");
            LogMessage("━".PadRight(80, '━'));

            AnalyzeElementTree(window, 0, 1);

            LogMessage("━".PadRight(80, '━'));
            LogMessage("✅ Yapı analizi tamamlandı!");
            LogMessage("");
            LogMessage("💡 İpucu: Container'ları ve element isimlerini not alın.");
            LogMessage("   Element seçerken bu bilgileri kullanabilirsiniz.");
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Hata: {ex.Message}");
        }
        finally
        {
            btnAnalyzeStructure.Enabled = true;
        }
    }

    private void AnalyzeElementTree(AutomationElement element, int level, int maxDepth)
    {
        if (level >= maxDepth) return;

        try
        {
            var indent = new string(' ', level * 2);
            var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);

            LogMessage($"{indent}📁 Level {level}: {children.Count} child element bulundu");
            LogMessage("");

            int index = 0;
            foreach (AutomationElement child in children)
            {
                try
                {
                    var controlType = child.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");
                    var name = string.IsNullOrEmpty(child.Current.Name) ? "(isimsiz)" : child.Current.Name;
                    var automationId = string.IsNullOrEmpty(child.Current.AutomationId) ? "(ID yok)" : child.Current.AutomationId;
                    var className = string.IsNullOrEmpty(child.Current.ClassName) ? "(class yok)" : child.Current.ClassName;

                    // Önemli container'ları vurgula
                    var isImportant = controlType == "Pane" || controlType == "Document" || controlType == "Group" || controlType == "Custom";
                    var marker = isImportant ? "⭐" : "  ";

                    LogMessage($"{indent}{marker} [{index}] {controlType}");
                    LogMessage($"{indent}      Name: {name}");
                    LogMessage($"{indent}      AutomationId: {automationId}");

                    if (!className.StartsWith("WindowsForms10."))
                    {
                        LogMessage($"{indent}      ClassName: {className}");
                    }

                    // Alt elementleri sayısını göster
                    try
                    {
                        var grandChildren = child.FindAll(TreeScope.Children, Condition.TrueCondition);
                        if (grandChildren.Count > 0)
                        {
                            LogMessage($"{indent}      └─ {grandChildren.Count} child element içeriyor");
                        }
                    }
                    catch { }

                    LogMessage("");
                    index++;

                    // İlk 50 elementi göster (performans için)
                    if (index >= 50)
                    {
                        LogMessage($"{indent}   ... ve {children.Count - 50} element daha (gösterilmiyor)");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"{indent}   ⚠ Element #{index} okunamadı: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"   ❌ Analiz hatası: {ex.Message}");
        }
    }

    private async void btnPickElement_Click(object? sender, EventArgs e)
    {
        // Eğer test devam ediyorsa iptal et
        if (_testCancellationTokenSource != null)
        {
            _testCancellationTokenSource.Cancel();
            _testCancellationTokenSource = null;
            btnTestAllStrategies.Text = "🧪 Tüm Stratejileri Test Et";
        }

        // Önceki strateji listesini ve seçimi temizle
        _availableStrategies.Clear();
        _selectedStrategy = null;
        lstStrategies.Items.Clear();
        lblSelectedStrategy.Text = "Seçili Strateji: -";
        lblSelectedStrategy.ForeColor = Color.Black;
        lblTestResult.Text = "";
        grpStrategyTest.Visible = false;

        LogMessage("\n=== YENİ ELEMENT SEÇİMİ ===");
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
            ShowMessage("Element yakalanamadı! Lütfen tekrar deneyin.", "Hata",
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

        txtElementProperties.AppendText($"{Environment.NewLine}=== CONTAINER BİLGİLERİ (Overlay/Pane Tespiti) ==={Environment.NewLine}");
        AppendIfNotEmpty("ContainerControlType", elementInfo.ContainerControlType);
        AppendIfNotEmpty("ContainerName", elementInfo.ContainerName);
        AppendIfNotEmpty("ContainerAutomationId", elementInfo.ContainerAutomationId);
        AppendIfNotEmpty("ContainerClassName", elementInfo.ContainerClassName);

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
        AppendIfNotEmpty("GrandParentName", elementInfo.GrandParentName);
        AppendIfNotEmpty("GrandParentAutomationId", elementInfo.GrandParentAutomationId);
        AppendIfNotEmpty("IndexInParent", elementInfo.IndexInParent?.ToString());
        AppendIfNotEmpty("SiblingCount", elementInfo.SiblingCount?.ToString());
        AppendIfNotEmpty("SiblingContext", elementInfo.SiblingContext);

        txtElementProperties.AppendText($"{Environment.NewLine}=== ETİKET VE İLİŞKİLER ==={Environment.NewLine}");
        AppendIfNotEmpty("LabeledByElement", elementInfo.LabeledByElement);
        AppendIfNotEmpty("DescribedByElement", elementInfo.DescribedByElement);

        txtElementProperties.AppendText($"{Environment.NewLine}=== WEB/HTML ÖZELLİKLERİ ==={Environment.NewLine}");
        AppendIfNotEmpty("HtmlId", elementInfo.HtmlId);
        AppendIfNotEmpty("TagName", elementInfo.TagName);
        AppendIfNotEmpty("HtmlName", elementInfo.HtmlName);
        AppendIfNotEmpty("Type", elementInfo.Type);
        AppendIfNotEmpty("Title", elementInfo.Title);
        AppendIfNotEmpty("Role", elementInfo.Role);
        AppendIfNotEmpty("InnerText", elementInfo.InnerText);
        AppendIfNotEmpty("TextContent", elementInfo.TextContent);
        AppendIfNotEmpty("Value", elementInfo.Value);
        AppendIfNotEmpty("Href", elementInfo.Href);
        AppendIfNotEmpty("Src", elementInfo.Src);
        AppendIfNotEmpty("Alt", elementInfo.Alt);
        AppendIfNotEmpty("Placeholder", elementInfo.Placeholder);

        // OuterHtml sadece kısa ise göster (çok uzun olabilir)
        if (!string.IsNullOrEmpty(elementInfo.OuterHtml) && elementInfo.OuterHtml.Length < 500)
        {
            AppendIfNotEmpty("OuterHtml", elementInfo.OuterHtml);
        }
        else if (!string.IsNullOrEmpty(elementInfo.OuterHtml))
        {
            AppendIfNotEmpty("OuterHtml", elementInfo.OuterHtml.Substring(0, 497) + "...");
        }

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

        // Container bilgisini log'a kaydet
        if (!string.IsNullOrEmpty(elementInfo.ContainerControlType))
        {
            LogMessage($"  Container: {elementInfo.ContainerControlType}");
            if (!string.IsNullOrEmpty(elementInfo.ContainerName))
            {
                LogMessage($"    Container Name: {elementInfo.ContainerName}");
            }
        }
        else
        {
            LogMessage("  ⚠ Container bilgisi bulunamadı");
        }

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
        // Eğer test çalışıyorsa durdur
        if (_testCancellationTokenSource != null)
        {
            LogMessage("\n⏹ Test durduruldu.");
            _testCancellationTokenSource.Cancel();
            _testCancellationTokenSource = null;
            btnTestAllStrategies.Text = "🧪 Tüm Stratejileri Test Et";
            return;
        }

        if (_availableStrategies.Count == 0)
        {
            ShowMessage("Önce bir element seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Yeni cancellation token oluştur
        _testCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _testCancellationTokenSource.Token;

        btnTestAllStrategies.Text = "⏹ Testi Durdur";
        LogMessage("\n=== STRATEJİ TESTLERİ BAŞLATILIYOR ===");

        // Debug log oturumunu başlat
        DebugLogger.StartNewSession();
        var debugLogPath = DebugLogger.GetLogFilePath();
        LogMessage($"📝 Debug log dosyası: {debugLogPath}");
        DebugLogger.LogSeparator('=', 80);
        DebugLogger.Log("STRATEJİ TESTLERİ BAŞLATILIYOR");
        DebugLogger.LogSeparator('=', 80);

        lstStrategies.Items.Clear();
        lblTestResult.Text = "⏳ Test ediliyor...";
        lblTestResult.ForeColor = Color.Blue;

        int successCount = 0;
        int failCount = 0;

        try
        {
            for (int i = 0; i < _availableStrategies.Count; i++)
            {
                // İptal kontrolü
                if (cancellationToken.IsCancellationRequested)
                {
                    LogMessage($"\n⚠ Test {i + 1}/{_availableStrategies.Count} adımında durduruldu.");
                    break;
                }

                var strategy = _availableStrategies[i];
                LogMessage($"Test #{i + 1}: {strategy.Name}...");

                // DEBUG: Window bilgisini logla
                if (_currentStep.UIElement != null)
                {
                    LogMessage($"  Window: {_currentStep.UIElement.WindowTitle ?? "N/A"}");
                    LogMessage($"  ProcessId: {_currentStep.UIElement.WindowProcessId?.ToString() ?? "N/A"}");
                }

                // Stratejiyi test et (elementInfo ile hızlandırma)
                var testedStrategy = await ElementLocatorTester.TestStrategy(strategy, _currentStep.UIElement);
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
                await Task.Delay(50, cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                LogMessage($"\n✅ Test tamamlandı: {successCount} başarılı, {failCount} başarısız");
                LogMessage("Bir strateji seçin ve 'Adımı Kaydet' butonuna tıklayın.");

                // Sonucu label'da göster
                lblTestResult.Text = $"✅ Test Tamamlandı - Başarılı: {successCount}, Başarısız: {failCount}";
                lblTestResult.ForeColor = successCount > 0 ? Color.Green : Color.Orange;

                // Log dosyası özeti
                DebugLogger.LogSeparator('=', 80);
                DebugLogger.Log($"TEST SONUÇLARI: {successCount} Başarılı, {failCount} Başarısız");
                DebugLogger.LogSeparator('=', 80);
                var logPath = DebugLogger.GetLogFilePath();
                LogMessage($"📁 Detaylı log kaydedildi: {logPath}");
            }
        }
        catch (OperationCanceledException)
        {
            LogMessage("\n⏹ Test kullanıcı tarafından durduruldu.");
            lblTestResult.Text = "⏹ Test durduruldu";
            lblTestResult.ForeColor = Color.Gray;

            DebugLogger.Log("TEST DURDURULDU");
        }
        finally
        {
            _testCancellationTokenSource = null;
            btnTestAllStrategies.Text = "🧪 Tüm Stratejileri Test Et";
        }
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

    private void btnTopmost_Click(object? sender, EventArgs e)
    {
        this.TopMost = !this.TopMost;

        if (this.TopMost)
        {
            btnTopmost.Text = "📌 En Üstte";
            btnTopmost.BackColor = Color.LightGreen;
        }
        else
        {
            btnTopmost.Text = "📌 En Üstte Tut";
            btnTopmost.BackColor = SystemColors.Control;
        }
    }

    /// <summary>
    /// MessageBox göster - Form topmost ise MessageBox da topmost olur
    /// </summary>
    private DialogResult ShowMessage(string text, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
    {
        return MessageBox.Show(this, text, caption, buttons, icon);
    }
}
