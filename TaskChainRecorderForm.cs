using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace MedulaOtomasyon;

public partial class TaskChainRecorderForm : Form
{
    private readonly Color _topmostActiveColor = Color.FromArgb(76, 175, 80);
    private readonly Color _topmostInactiveColor = Color.FromArgb(189, 189, 189);
    private bool _isDocking;

    private TaskChain _currentChain;
    private TaskChainDatabase _database;
    private int _currentStepNumber = 1;
    private TaskStep _currentStep;
    private List<ElementLocatorStrategy> _availableStrategies = new();
    private ElementLocatorStrategy? _selectedStrategy = null;
    private CancellationTokenSource? _testCancellationTokenSource = null;
    private bool _isEditingMode = false;
    private TaskStep? _stepBeingEdited = null;

    // Smart Element Recorder için field'lar
    private SmartElementRecorder? _smartRecorder = null;
    private bool _isSmartRecording = false;
    private RecordedElement? _lastRecordedElement = null;
    private List<ElementLocatorStrategy> _smartStrategies = new();
    private ElementLocatorStrategy? _selectedSmartStrategy = null;
    private readonly string _medulaHtmlPath = Path.Combine(AppContext.BaseDirectory, "medula sayfası kaynak kodları.txt");

    public TaskChainRecorderForm()
    {
        InitializeComponent();
        this.TopMost = true; // Her zaman en üstte tut
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

        Load += (_, _) => DockWindowToRightEdge();
        Shown += TaskChainRecorderForm_Shown;
        LocationChanged += TaskChainRecorderForm_LocationChanged;

        // Başlangıçta görev görüntüleyiciyi güncelle
        UpdateTaskChainViewer();
    }

    /// <summary>
    /// Mevcut bir zinciri düzenleme için yükle (Oynatıcıdan çağrılır)
    /// </summary>
    public void LoadChainForEditing(TaskChain chain, int highlightStepIndex = -1)
    {
        if (chain == null)
        {
            LogMessage("❌ HATA: Yüklenecek zincir null!");
            return;
        }

        LogMessage($"📂 Mevcut zincir yükleniyor: {chain.Name}");

        // Zinciri yükle
        _currentChain = chain;
        txtChainName.Text = chain.Name;

        // Sonraki adım numarasını ayarla
        _currentStepNumber = chain.Steps.Count > 0 ? chain.Steps.Max(s => s.StepNumber) + 1 : 1;
        UpdateStepNumberLabel();

        // Görev görüntüleyiciyi güncelle
        UpdateTaskChainViewer();

        // Eğer belirli bir adım vurgulanacaksa
        if (highlightStepIndex >= 0 && highlightStepIndex < chain.Steps.Count)
        {
            var step = chain.Steps[highlightStepIndex];
            LogMessage($"⚠️ Dikkat: Adım {step.StepNumber} çalışırken hata verdi veya durduruldu.");
            LogMessage($"   Bu adımı düzenlemek için 'Düzenle' butonunu kullanın.");

            // Form başlığını değiştir
            lblTitle.Text = $"Görev Zinciri Düzenleyici - {chain.Name}";
            lblTitle.ForeColor = Color.FromArgb(255, 140, 0);
        }
        else
        {
            lblTitle.Text = $"Görev Zinciri Düzenleyici - {chain.Name}";
        }

        LogMessage($"✅ Zincir yüklendi: {chain.Steps.Count} adım");
        LogMessage("   Yeni adım ekleyebilir veya mevcut adımları düzenleyebilirsiniz.");
        LogMessage("   Değişiklikleri kaydetmek için 'Zinciri Kaydet' butonuna basın.");
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

    private void TaskChainRecorderForm_Shown(object? sender, EventArgs e)
    {
        BeginInvoke(new Action(() =>
        {
            DockWindowToRightEdge();
            PushOtherWindowsToLeft();

            // Temel Bilgiler sekmesini aktif hale getir
            if (tabControl.SelectedTab != tabBasicInfo)
            {
                tabControl.SelectedTab = tabBasicInfo;
                LogMessage("Temel Bilgiler sekmesi aktif hale getirildi.");
            }
        }));
    }

    private void TaskChainRecorderForm_LocationChanged(object? sender, EventArgs e)
    {
        if (!IsHandleCreated || _isDocking)
            return;

        _isDocking = true;
        DockWindowToRightEdge();
        PushOtherWindowsToLeft();
        _isDocking = false;
    }

    private void DockWindowToRightEdge()
    {
        var screen = Screen.PrimaryScreen!;
        var work = screen.WorkingArea;
        int desiredWidth = 480; // Ekranın 1/4'ü (1920 / 4 = 480)

        StartPosition = FormStartPosition.Manual;

        // Form yüksekliği ekran yüksekliğini geçmemeli
        int formHeight = Math.Min(1060, work.Height);
        Size = new Size(desiredWidth, formHeight);

        // Formun sağ kenara yapışması ve ekran sınırları içinde kalması
        Location = new Point(work.Right - desiredWidth, work.Top);
    }

    private void PushOtherWindowsToLeft()
    {
        var work = Screen.PrimaryScreen!.WorkingArea;
        int leftWidth = Math.Max(0, work.Width - Width);

        if (leftWidth <= 0)
            return;

        IntPtr selfHandle = Handle;
        uint currentPid = (uint)Process.GetCurrentProcess().Id;
        var primaryBounds = new Rectangle(work.Left, work.Top, work.Width, work.Height);

        EnumWindows((hWnd, _) =>
        {
            if (hWnd == selfHandle)
                return true;

            if (!IsWindowVisible(hWnd) || IsIconic(hWnd))
                return true;

            GetWindowThreadProcessId(hWnd, out uint pid);
            if (pid == currentPid)
                return true;

            if (!GetWindowRect(hWnd, out RECT rect))
                return true;

            var windowRect = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
            if (!windowRect.IntersectsWith(primaryBounds))
                return true;

            if (rect.Left >= work.Left && rect.Right <= work.Left + leftWidth)
                return true;

            SetWindowPos(
                hWnd,
                IntPtr.Zero,
                work.Left,
                work.Top,
                leftWidth,
                work.Height,
                SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);

            return true;
        }, IntPtr.Zero);
    }

    private void cmbStepType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Tip seçimine göre ilgili TabPage'leri aktif et
        switch (cmbStepType.SelectedIndex)
        {
            case -1: // Hiçbiri seçili değil
                lblStepType.Text = "Görev Tipi: Lütfen Seçiniz";
                lblStepType.ForeColor = Color.Gray;
                break;

            case 0: // Tip 1: Hedef Program/Pencere Seçimi
                _currentStep.StepType = StepType.TargetSelection;
                lblStepType.Text = "Görev Tipi: Hedef Program/Pencere Seçimi";
                lblStepType.ForeColor = Color.FromArgb(0, 120, 212);
                LogMessage("Tip 1 seçildi: Hedef Program/Pencere Seçimi");
                tabControl.SelectedTab = tabTargetSelection;
                break;

            case 1: // Tip 2: UI Element Tıklama/Tuşlama
                _currentStep.StepType = StepType.UIElementAction;
                lblStepType.Text = "Görev Tipi: UI Element Tıklama/Tuşlama";
                lblStepType.ForeColor = Color.FromArgb(0, 120, 212);
                LogMessage("Tip 2 seçildi: UI Element Tıklama/Tuşlama");
                tabControl.SelectedTab = tabUIElement;
                break;

            case 2: // Tip 3: Sayfa Durum Kontrolü (Koşullu Dallanma)
                _currentStep.StepType = StepType.ConditionalBranch;
                lblStepType.Text = "Görev Tipi: Sayfa Durum Kontrolü";
                lblStepType.ForeColor = Color.FromArgb(0, 120, 212);
                LogMessage("Tip 3 seçildi: Sayfa Durum Kontrolü (Koşullu Dallanma)");
                OpenConditionalBranchRecorder();
                break;

            case 3: // Tip 4: Döngü veya Bitiş Koşulu
                ShowMessage("Tip 4: Döngü veya Bitiş Koşulu henüz uygulanmadı.\nYakında eklenecek.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbStepType.SelectedIndex = -1;
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
            _currentStep.Description = $"Adım {_currentStep.StepNumber}: Hedef - {txtProgramPath.Text}";
        }
        else if (_currentStep.StepType == StepType.UIElementAction)
        {
            // Akıllı element seç kullanıldıysa
            if (_selectedSmartStrategy != null && _lastRecordedElement != null)
            {
                LogMessage("🧠 Akıllı element seçimi kullanılıyor...");

                // RecordedElement'i UIElementInfo'ya dönüştür
                _currentStep.UIElement = SmartElementRecorder.ConvertToUIElementInfo(_lastRecordedElement);
                _currentStep.SelectedStrategy = _selectedSmartStrategy;

                LogMessage($"✅ Element dönüştürüldü: {_currentStep.UIElement.Name ?? _currentStep.UIElement.ClassName}");
                LogMessage($"✅ Strateji seçildi: {_selectedSmartStrategy.Name}");
            }
            // Normal element seç kullanıldıysa
            else
            {
                if (_currentStep.UIElement == null)
                {
                    ShowMessage("Lütfen önce bir UI element seçin!\n\n" +
                                  "Yöntem 1: 'Element Seç' butonunu kullanın\n" +
                                  "Yöntem 2: '🧠 Akıllı Element Seç' butonunu kullanın",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Strateji seçilmeli (Düzenleme modundaysa ve strateji mevcutsa kontrol etme)
                if (_selectedStrategy == null && !_isEditingMode)
                {
                    ShowMessage("Lütfen bir element bulma stratejisi seçin!\n\n" +
                                  "1. Element Seç butonuna tıklayın\n" +
                                  "2. Tüm Stratejileri Test Et'e tıklayın\n" +
                                  "3. Listeden bir strateji seçin",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Başarısız strateji uyarısı
                if (_selectedStrategy != null && !_selectedStrategy.IsSuccessful)
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

                if (_selectedStrategy != null)
                {
                    _currentStep.SelectedStrategy = _selectedStrategy;
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

            // Action'a göre özel parametreleri kaydet
            if (_currentStep.Action == ActionType.KeyPress)
            {
                _currentStep.KeysToPress = txtKeysToPress.Text;
            }
            else if (_currentStep.Action == ActionType.TypeText)
            {
                _currentStep.TextToType = txtKeysToPress.Text;
            }
            else if (_currentStep.Action == ActionType.MouseWheel)
            {
                // Scroll miktarını delta değerine çevir (1 adım = 120 delta)
                _currentStep.MouseWheelDelta = (int)numScrollAmount.Value * 120;
            }
            else if (_currentStep.Action == ActionType.DoubleClick)
            {
                // Double click delay değerini kaydet (şu anda kullanılmıyor ama gelecekte eklenebilir)
                _currentStep.WaitMilliseconds = (int)numDoubleClickDelay.Value;
            }

            var strategyName = _currentStep.SelectedStrategy?.Name ?? "NoStrategy";
            _currentStep.Description = $"Adım {_currentStep.StepNumber}: {_currentStep.Action} - {_currentStep.UIElement.Name ?? _currentStep.UIElement.ClassName} [{strategyName}]";
        }

        // Düzenleme modunda mı?
        if (_isEditingMode && _stepBeingEdited != null)
        {
            // Mevcut adımı güncelle (zincirden çıkarmadan)
            LogMessage($"✓ Adım {_currentStep.StepNumber} güncellendi: {_currentStep.Description}");

            // Düzenleme modundan çık
            CancelEditMode();

            // Görev zinciri görüntüleyiciyi güncelle
            UpdateTaskChainViewer();

            ShowMessage($"Adım {_currentStep.StepNumber} başarıyla güncellendi!", "Başarılı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            // Yeni adım ekleme modu
            _currentChain.Steps.Add(_currentStep);
            LogMessage($"✓ Adım {_currentStepNumber} kaydedildi: {_currentStep.Description}");

            // Görev zinciri görüntüleyiciyi güncelle
            UpdateTaskChainViewer();

            ShowMessage($"Adım {_currentStepNumber} başarıyla kaydedildi!", "Başarılı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Otomatik olarak bir sonraki adıma geç
            btnNextStep_Click(null, EventArgs.Empty);
        }
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
            // Akıllı strateji veya klasik strateji seçilmişse UI Element Action testi yap
            var hasStrategy = _selectedStrategy != null || _selectedSmartStrategy != null;
            var hasUIElement = _currentStep.UIElement != null ||
                              (_selectedSmartStrategy?.RecordedElement != null);

            if (hasStrategy && hasUIElement)
            {
                // Strateji seçiliyse ve UI element bilgisi varsa, UI Element Action testi yap
                TestUIElementAction();
            }
            else if (_currentStep.StepType == StepType.TargetSelection)
            {
                TestTargetSelection();
            }
            else if (_currentStep.StepType == StepType.UIElementAction)
            {
                TestUIElementAction();
            }
            else
            {
                ShowMessage("Test etmek için önce bir hedef veya strateji seçin!",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        var strategy = _selectedStrategy ?? _selectedSmartStrategy;

        if (strategy == null)
        {
            ShowMessage("Lütfen önce bir strateji seçin! (Akıllı Stratejiler listesi veya klasik liste)", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_currentStep.UIElement == null && strategy.RecordedElement != null)
        {
            _currentStep.UIElement = SmartElementRecorder.ConvertToUIElementInfo(strategy.RecordedElement);
            LogMessage("ℹ️ UIElement bilgisi akıllı kayıttan dolduruldu.");
        }

        if (_currentStep.UIElement == null)
        {
            ShowMessage("Lütfen önce bir UI element seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LogMessage("UI Element test ediliyor...");
        LogMessage($"Element: {_currentStep.UIElement.Name}");
        LogMessage($"Strateji: {strategy.Name}");
        LogMessage($"Action: {cmbActionType.Text}");

        // DEBUG: Window bilgisini logla
        LogMessage($"Window: {_currentStep.UIElement.WindowTitle ?? "N/A"}");
        LogMessage($"ProcessId: {_currentStep.UIElement.WindowProcessId?.ToString() ?? "N/A"}");

        try
        {
            // Seçili strateji ile elementi bul (windowInfo ile hızlandırma)
            AutomationElement? element = null;

            if (strategy == _selectedSmartStrategy && _smartRecorder != null)
            {
                var smartSuccess = _smartRecorder.ExecuteLocatorStrategy(strategy);
                if (!smartSuccess)
                {
                    LogMessage("⚠ Akıllı strateji ile element bulunamadı!");
                    if (lblTestResult != null)
                    {
                        lblTestResult.Text = $"⚠ Element bulunamadı - Strateji: {strategy.Name}";
                        lblTestResult.ForeColor = Color.Orange;
                    }
                    return;
                }

                // SmartElementRecorder.PlaybackElement doğrudan etkileşimi yaptı; ileri log ver.
                LogMessage("✅ Akıllı strateji başarıyla yürütüldü.");
                if (lblTestResult != null)
                {
                    lblTestResult.Text = $"✅ Test Başarılı - {strategy.Name}";
                    lblTestResult.ForeColor = Color.Green;
                }
                return;
            }
            else
            {
                element = ElementLocatorTester.FindElementByStrategy(strategy, _currentStep.UIElement);
            }

            if (element == null)
            {
                LogMessage("⚠ Element bulunamadı!");
                if (lblTestResult != null)
                {
                    lblTestResult.Text = $"⚠ Element bulunamadı - Strateji: {strategy.Name}";
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

            // Mouse wheel delta değerini al (varsa)
            int mouseWheelDelta = 0;
            if (action == ActionType.MouseWheel)
            {
                mouseWheelDelta = _currentStep.MouseWheelDelta ?? 120; // Varsayılan 120 (yukarı scroll)
            }

            // Eylemi gerçekleştir
            ExecuteTestAction(element, action, txtKeysToPress.Text, mouseWheelDelta);

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

    private void ExecuteTestAction(AutomationElement element, ActionType action, string inputText, int mouseWheelDelta = 0)
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
                LogMessage("Sağ tıklama yapılıyor...");
                RightClickElement(element);
                break;

            case ActionType.MouseWheel:
                LogMessage($"Mouse tekerlek yapılıyor... (Delta: {mouseWheelDelta})");
                MouseWheelOnElement(element, mouseWheelDelta);
                break;

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

    private void RightClickElement(AutomationElement element)
    {
        var rect = element.Current.BoundingRectangle;
        var centerX = (int)(rect.Left + rect.Width / 2);
        var centerY = (int)(rect.Top + rect.Height / 2);
        MedulaAutomation.MouseRightClick(centerX, centerY);
    }

    private void MouseWheelOnElement(AutomationElement element, int delta)
    {
        var rect = element.Current.BoundingRectangle;
        var centerX = (int)(rect.Left + rect.Width / 2);
        var centerY = (int)(rect.Top + rect.Height / 2);
        MedulaAutomation.MouseWheel(centerX, centerY, delta);
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
        txtSmartElementProperties.Clear();

        // Stratejileri ve seçimleri temizle
        _selectedStrategy = null;
        _selectedSmartStrategy = null;
        _availableStrategies.Clear();
        _smartStrategies.Clear();
        lstStrategies.Items.Clear();
        lstSmartStrategies.Items.Clear();

        if (lblTestResult != null)
        {
            lblTestResult.Text = "";
        }
        if (lblSmartTestResult != null)
        {
            lblSmartTestResult.Text = "";
        }
        if (lblSelectedStrategy != null)
        {
            lblSelectedStrategy.Text = "Seçili strateji: Yok";
        }
        if (lblSmartSelectedStrategy != null)
        {
            lblSmartSelectedStrategy.Text = "Seçili strateji: Yok";
        }

        // Temel Bilgiler sekmesine dön
        tabControl.SelectedTab = tabBasicInfo;

        // Adım tipini sıfırla (event tetiklemeden)
        cmbStepType.SelectedIndexChanged -= cmbStepType_SelectedIndexChanged;
        cmbStepType.SelectedIndex = -1; // Hiçbiri seçili değil
        cmbStepType.SelectedIndexChanged += cmbStepType_SelectedIndexChanged;

        LogMessage($"\n--- Yeni Adım: {_currentStepNumber} ---");
        LogMessage("Lütfen adım tipini seçin ve gerekli bilgileri doldurun.");
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

        // Stratejiler sekmesine geç
        tabControl.SelectedTab = tabStrategies;

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
        // Tüm dinamik kontrolleri gizle
        lblKeysToPress.Visible = false;
        txtKeysToPress.Visible = false;
        lblScrollAmount.Visible = false;
        numScrollAmount.Visible = false;
        lblDoubleClickDelay.Visible = false;
        numDoubleClickDelay.Visible = false;

        // Seçime göre ilgili kontrolleri göster
        // 0: Sol Tık, 1: Sağ Tık, 2: Çift Tık, 3: Mouse Tekerlek, 4: Klavye Tuşları, 5: Metin Yaz
        switch (cmbActionType.SelectedIndex)
        {
            case 2: // Çift Tık
                lblDoubleClickDelay.Visible = true;
                numDoubleClickDelay.Visible = true;
                break;

            case 3: // Mouse Tekerlek
                lblScrollAmount.Visible = true;
                numScrollAmount.Visible = true;
                break;

            case 4: // Klavye Tuşları
                lblKeysToPress.Visible = true;
                txtKeysToPress.Visible = true;
                lblKeysToPress.Text = "Klavye Tuşları:";
                txtKeysToPress.PlaceholderText = "Örn: {ENTER}, {TAB}, ^c (Ctrl+C), %(F4) (Alt+F4)...";
                break;

            case 5: // Metin Yaz
                lblKeysToPress.Visible = true;
                txtKeysToPress.Visible = true;
                lblKeysToPress.Text = "Yazılacak Metin:";
                txtKeysToPress.PlaceholderText = "Yazılacak metni girin...";
                break;
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

    private async void btnTestSelectedStrategy_Click(object? sender, EventArgs e)
    {
        var strategy = _selectedStrategy ?? _selectedSmartStrategy;

        if (strategy == null)
        {
            ShowMessage("Lütfen önce bir strateji seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_currentStep.UIElement == null)
        {
            ShowMessage("UI Element bilgisi bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LogMessage($"\n=== SEÇİLİ STRATEJİ TEST EDİLİYOR: {strategy.Name} ===");

        // Debug log oturumunu başlat
        DebugLogger.StartNewSession();
        var debugLogPath = DebugLogger.GetLogFilePath();
        LogMessage($"📝 Debug log dosyası: {debugLogPath}");
        DebugLogger.LogSeparator('=', 80);
        DebugLogger.Log($"SEÇİLİ STRATEJİ TEST EDİLİYOR: {strategy.Name}");
        DebugLogger.LogSeparator('=', 80);

        lblTestResult.Text = "⏳ Test ediliyor...";
        lblTestResult.ForeColor = Color.Blue;

        try
        {
            // DEBUG: Window bilgisini logla
            if (_currentStep.UIElement != null)
            {
                LogMessage($"  Window: {_currentStep.UIElement.WindowTitle ?? "N/A"}");
                LogMessage($"  ProcessId: {_currentStep.UIElement.WindowProcessId?.ToString() ?? "N/A"}");
            }

            // Stratejiyi test et
            var testedStrategy = await ElementLocatorTester.TestStrategy(strategy, _currentStep.UIElement);

            // Sonucu güncelle
            if (strategy == _selectedStrategy)
            {
                int index = _availableStrategies.IndexOf(_selectedStrategy);
                if (index >= 0)
                {
                    _availableStrategies[index] = testedStrategy;
                    _selectedStrategy = testedStrategy;
                }
            }
            else if (strategy == _selectedSmartStrategy)
            {
                int index = _smartStrategies.IndexOf(_selectedSmartStrategy);
                if (index >= 0)
                {
                    _smartStrategies[index] = testedStrategy;
                    _selectedSmartStrategy = testedStrategy;
                }
            }

            // Sonucu göster
            string icon = testedStrategy.IsSuccessful ? "✅" : "❌";
            string result = testedStrategy.IsSuccessful
                ? $"Başarılı ({testedStrategy.TestDurationMs}ms)"
                : $"Başarısız: {testedStrategy.ErrorMessage}";

            if (testedStrategy.IsSuccessful)
            {
                LogMessage($"  ✅ Başarılı! ({testedStrategy.TestDurationMs}ms)");
                lblTestResult.Text = $"✅ Test Başarılı - {testedStrategy.TestDurationMs}ms";
                lblTestResult.ForeColor = Color.Green;
            }
            else
            {
                LogMessage($"  ❌ Başarısız: {testedStrategy.ErrorMessage}");
                lblTestResult.Text = $"❌ Test Başarısız - {testedStrategy.ErrorMessage}";
                lblTestResult.ForeColor = Color.Red;
            }

            // Log dosyası özeti
            DebugLogger.LogSeparator('=', 80);
            DebugLogger.Log($"TEST SONUCU: {(testedStrategy.IsSuccessful ? "BAŞARILI" : "BAŞARISIZ")}");
            DebugLogger.LogSeparator('=', 80);
            var logPath = DebugLogger.GetLogFilePath();
            LogMessage($"📁 Detaylı log kaydedildi: {logPath}");
        }
        catch (Exception ex)
        {
            LogMessage($"\n❌ Test sırasında hata: {ex.Message}");
            lblTestResult.Text = $"❌ Hata: {ex.Message}";
            lblTestResult.ForeColor = Color.Red;

            DebugLogger.Log($"HATA: {ex.Message}");
            DebugLogger.Log($"Stack Trace: {ex.StackTrace}");
        }
    }

    private void lstStrategies_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstStrategies.SelectedIndex >= 0 && lstStrategies.SelectedIndex < _availableStrategies.Count)
        {
            _selectedStrategy = _availableStrategies[lstStrategies.SelectedIndex];
            lblSelectedStrategy.Text = $"Seçili Strateji: {_selectedStrategy.Name}";
            lblSelectedStrategy.ForeColor = _selectedStrategy.IsSuccessful ? Color.Green : Color.Red;

            // Element bilgisini akıllı kayıttan doldur
            if (_currentStep.UIElement == null && _selectedStrategy.RecordedElement != null)
            {
                _currentStep.UIElement = SmartElementRecorder.ConvertToUIElementInfo(_selectedStrategy.RecordedElement);
                LogMessage("ℹ️ UIElement bilgisi akıllı kayıttan dolduruldu.");
            }

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

    // btnTopmost kaldırıldı - form her zaman topmost

    /// <summary>
    /// MessageBox göster - Form topmost ise MessageBox da topmost olur
    /// </summary>
    private DialogResult ShowMessage(string text, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
    {
        return MessageBox.Show(this, text, caption, buttons, icon);
    }

    #region Native helpers

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion

    #region Smart Element Recorder Methods

    /// <summary>
    /// Akıllı Element Seç butonuna tıklanınca
    /// </summary>
    private void btnSmartPick_Click(object? sender, EventArgs e)
    {
        // Recording aktif mi kontrol et
        if (_isSmartRecording)
        {
            // Recording'i durdur
            StopSmartRecording();
            return;
        }

        LogMessage("\n=== 🧠 AKILLI ELEMENT KAYDEDİCİ ===");
        LogMessage("⚠️ Bu özellik TABLO SATIRLARI için optimize edilmiştir.");
        LogMessage("📹 Recording başlatılıyor...");
        LogMessage("👉 Medula sayfasındaki tablo satırına tıklayın!");

        // Smart Recording'i başlat
        StartSmartRecording();
    }

    /// <summary>
    /// Smart Recording'i başlatır
    /// </summary>
    private void StartSmartRecording()
    {
        try
        {
            LogMessage($"[DEBUG] StartSmartRecording çağrıldı");

            // Önceki kayıtları temizle
            _lastRecordedElement = null;
            _smartStrategies.Clear();
            lstSmartStrategies.Items.Clear();
            txtSmartElementProperties.Text = "";
            LogMessage($"[DEBUG] Önceki kayıtlar temizlendi");

            // SmartElementRecorder oluştur
            if (_smartRecorder == null)
            {
                _smartRecorder = new SmartElementRecorder();
                _smartRecorder.ElementRecorded += OnSmartElementRecorded;
                _smartRecorder.RecordingStatusChanged += OnSmartRecordingStatusChanged;
                LogMessage($"[DEBUG] SmartElementRecorder oluşturuldu ve event handler'lar bağlandı");
            }

            // Recording'i başlat
            _smartRecorder.StartRecording();
            _isSmartRecording = true;
            LogMessage($"[DEBUG] _isSmartRecording = true");

            // Buton görünümünü değiştir
            btnSmartPick.Text = "⏹️ Kaydı Durdur";
            btnSmartPick.BackColor = Color.Red;
            btnSmartPick.ForeColor = Color.White;

            LogMessage("✅ Smart Recording başlatıldı!");
            LogMessage("👉 Şimdi istediğiniz tablo satırına tıklayın...");
        }
        catch (Exception ex)
        {
            LogMessage($"HATA: Smart Recording başlatılamadı: {ex.Message}");
            LogMessage($"HATA detay: {ex.StackTrace}");
            ShowMessage($"Smart Recording başlatılamadı:\n{ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Smart Recording'i durdurur
    /// </summary>
    private void StopSmartRecording()
    {
        try
        {
            _smartRecorder?.StopRecording();
            _isSmartRecording = false;

            // Buton görünümünü eski haline getir
            btnSmartPick.Text = "🧠 Akıllı Element Seç";
            btnSmartPick.BackColor = SystemColors.Control;
            btnSmartPick.ForeColor = SystemColors.ControlText;

            LogMessage("⏹️ Smart Recording durduruldu.");
        }
        catch (Exception ex)
        {
            LogMessage($"HATA: Recording durdurulamadı: {ex.Message}");
        }
    }

    /// <summary>
    /// Smart Recording status değişikliklerini loglar
    /// </summary>
    private void OnSmartRecordingStatusChanged(object? sender, string status)
    {
        // Thread-safe log
        if (InvokeRequired)
        {
            Invoke(() => LogMessage($"[SmartRecorder] {status}"));
        }
        else
        {
            LogMessage($"[SmartRecorder] {status}");
        }
    }

    /// <summary>
    /// Element kaydedildiğinde çağrılır
    /// </summary>
    private void OnSmartElementRecorded(object? sender, ElementRecordedEventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(async () =>
            {
                LogMessage("[DEBUG] OnSmartElementRecorded event tetiklendi (Invoke ile)");
                await ProcessSmartRecordedElementAsync(e.Element);
            }));
        }
        else
        {
            LogMessage("[DEBUG] OnSmartElementRecorded event tetiklendi (direkt)");
            _ = ProcessSmartRecordedElementAsync(e.Element);
        }
    }

    /// <summary>
    /// Kaydedilen elementi işler ve stratejileri oluşturur
    /// </summary>
    private async Task ProcessSmartRecordedElementAsync(RecordedElement element)
    {
        try
        {
            var processingStartTime = DateTime.Now;

            LogMessage($"\n🎯 ELEMENT YAKALANDI!");
            LogMessage($"Tip: {element.ElementType}");
            LogMessage($"Açıklama: {element.Description}");
            LogMessage($"⏱️ Yakalama Zamanı: {element.Timestamp:HH:mm:ss.fff}");

            // Element bilgilerini sakla
            _lastRecordedElement = element;
            LogMessage($"[DEBUG] _lastRecordedElement atandı: {_lastRecordedElement != null}");

            var playwrightStartTime = DateTime.Now;
            await EnrichWithPlaywrightAsync(element);
            var playwrightDuration = (DateTime.Now - playwrightStartTime).TotalMilliseconds;
            LogMessage($"⏱️ Playwright Analiz Süresi: {playwrightDuration:F0}ms");

            // Element bilgilerini göster
            DisplaySmartElementInfo(element);

            // Stratejileri oluştur
            var strategyStartTime = DateTime.Now;
            CreateSmartStrategies(element);
            var strategyDuration = (DateTime.Now - strategyStartTime).TotalMilliseconds;
            LogMessage($"[DEBUG] Strateji sayısı: {_smartStrategies.Count}");
            LogMessage($"⏱️ Strateji Oluşturma Süresi: {strategyDuration:F0}ms");

            // Recording'i otomatik durdur
            StopSmartRecording();

            // Test butonunu aktif et
            btnTestSmartStrategies.Enabled = true;

            var totalDuration = (DateTime.Now - processingStartTime).TotalMilliseconds;
            LogMessage($"✅ {_smartStrategies.Count} akıllı strateji oluşturuldu!");
            LogMessage($"⏱️ Toplam İşlem Süresi: {totalDuration:F0}ms");
            LogMessage("👉 'Akıllı Stratejileri Test Et' butonuna tıklayarak test edin.");
        }
        catch (Exception ex)
        {
            LogMessage($"HATA: Element işlenirken hata: {ex.Message}");
            LogMessage($"HATA detay: {ex.StackTrace}");
        }
    }

    private async Task EnrichWithPlaywrightAsync(RecordedElement element)
    {
        if (!File.Exists(_medulaHtmlPath))
        {
            LogMessage($"⚠️ Playwright kaynak dosyası bulunamadı: {_medulaHtmlPath}");
            return;
        }

        try
        {
            LogMessage("🌐 Playwright analizi başlatılıyor...");
            var info = await PlaywrightRowAnalyzer.AnalyzeAsync(element, _medulaHtmlPath);
            SmartElementRecorder.ApplyPlaywrightMetadata(element, info);

            if (!string.IsNullOrEmpty(info.ErrorMessage))
            {
                LogMessage($"⚠️ Playwright analizi uyarısı: {info.ErrorMessage}");
            }
            else
            {
                LogMessage($"✅ Playwright analizi tamamlandı. {info.Selectors.Count} selector üretildi.");
                if (info.Selectors.TryGetValue("table-row", out var selector))
                {
                    LogMessage($"   Önerilen selector: {selector}");
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"⚠️ Playwright analizi başarısız: {ex.Message}");
        }
    }

    /// <summary>
    /// Element bilgilerini textbox'a yazar
    /// </summary>
    private void DisplaySmartElementInfo(RecordedElement element)
    {
        var sb = new System.Text.StringBuilder();

        if (element.TableInfo != null)
        {
            sb.AppendLine($"Tablo: {element.TableInfo.TableId ?? "?"}");
            sb.AppendLine($"Satır: {element.TableInfo.RowIndex}");
            sb.AppendLine($"Hücreler: {element.TableInfo.CellTexts.Count}");

            if (element.TableInfo.CellTexts.Any())
            {
                sb.Append($"İçerik: {string.Join(" | ", element.TableInfo.CellTexts.Take(2))}");
                if (element.TableInfo.CellTexts.Count > 2)
                    sb.Append("...");
            }
        }
        else
        {
            sb.AppendLine($"Tip: {element.ElementType}");
            sb.AppendLine($"Name: {element.Name ?? "?"}");
            sb.AppendLine($"Class: {element.ClassName ?? "?"}");
        }

        if (element.PlaywrightInfo != null)
        {
            sb.AppendLine();
            sb.AppendLine("Playwright:");

            if (!string.IsNullOrEmpty(element.PlaywrightInfo.TableSelector))
            {
                sb.AppendLine($"  Tablo: {element.PlaywrightInfo.TableSelector}");
            }

            if (element.PlaywrightInfo.RowIndex >= 0)
            {
                sb.AppendLine($"  Satır Index: {element.PlaywrightInfo.RowIndex}");
            }

            if (element.PlaywrightInfo.Selectors.TryGetValue("table-row", out var tableRowSelector))
            {
                sb.AppendLine($"  Selector: {tableRowSelector}");
            }
            else if (element.PlaywrightInfo.Selectors.TryGetValue("css", out var cssSelector))
            {
                sb.AppendLine($"  CSS: {cssSelector}");
            }

            if (!string.IsNullOrEmpty(element.PlaywrightInfo.ErrorMessage))
            {
                sb.AppendLine($"  Uyarı: {element.PlaywrightInfo.ErrorMessage}");
            }
        }

        txtSmartElementProperties.Text = sb.ToString();
    }

    /// <summary>
    /// Akıllı stratejileri oluşturur (SmartElementRecorder'dan alır)
    /// </summary>
    private void CreateSmartStrategies(RecordedElement element)
    {
        _smartStrategies.Clear();
        lstSmartStrategies.Items.Clear();

        // SmartElementRecorder'daki GenerateLocatorStrategies metodunu kullan
        var strategies = SmartElementRecorder.GenerateLocatorStrategies(element);

        LogMessage($"📋 {strategies.Count} strateji oluşturuldu:");

        foreach (var strategy in strategies)
        {
            _smartStrategies.Add(strategy);

            // ListBox'a ekle
            var displayText = $"{strategy.Name} - {strategy.Description}";
            lstSmartStrategies.Items.Add(displayText);

            LogMessage($"  ✓ {strategy.Name}: {strategy.Description}");
        }

        // İlk stratejiyi varsayılan olarak seç
        if (lstSmartStrategies.Items.Count > 0)
        {
            lstSmartStrategies.SelectedIndex = 0;
        }

        // Eski manuel strateji ekleme kodları (yedek olarak saklanıyor, artık kullanılmıyor)
        /*
        // Strateji 1: AutomationId (varsa)
        if (!string.IsNullOrEmpty(element.AutomationId))
        {
            AddSmartStrategy("AutomationId", $"ID='{element.AutomationId}'",
                LocatorType.AutomationId, new Dictionary<string, string>
                {
                    { "AutomationId", element.AutomationId }
                }, element);
        }

        // Strateji 2: Table Row Index (varsa)
        if (element.TableInfo != null && element.TableInfo.RowIndex >= 0)
        {
            AddSmartStrategy("Tablo Satır Index",
                $"Tablo[{element.TableInfo.RowIndex}]",
                LocatorType.TableRowIndex, new Dictionary<string, string>
                {
                    { "TableId", element.TableInfo.TableId ?? "" },
                    { "RowIndex", element.TableInfo.RowIndex.ToString() }
                }, element);
        }

        // Strateji 3: Hücre Text İçeriği (varsa)
        if (element.TableInfo?.CellTexts != null && element.TableInfo.CellTexts.Any())
        {
            AddSmartStrategy("Hücre İçeriği",
                $"Text='{string.Join("|", element.TableInfo.CellTexts.Take(2))}'",
                LocatorType.TextContent, new Dictionary<string, string>
                {
                    { "CellTexts", string.Join("|", element.TableInfo.CellTexts) }
                }, element);
        }

        // Strateji 4: ClassName + Name (varsa)
        if (!string.IsNullOrEmpty(element.ClassName) && !string.IsNullOrEmpty(element.Name))
        {
            AddSmartStrategy("Class+Name",
                $"{element.ClassName}+{element.Name}",
                LocatorType.ClassAndName, new Dictionary<string, string>
                {
                    { "ClassName", element.ClassName },
                    { "Name", element.Name }
                }, element);
        }
        */

        lblSmartTestResult.Text = $"{_smartStrategies.Count} strateji hazır - test edin";
        lblSmartTestResult.ForeColor = Color.Blue;
    }

    /// <summary>
    /// Listeye strateji ekler
    /// </summary>
    private void AddSmartStrategy(string name, string description, LocatorType type,
        Dictionary<string, string> properties, RecordedElement element)
    {
        var strategy = new ElementLocatorStrategy
        {
            Name = name,
            Description = description,
            Type = type,
            Properties = properties,
            RecordedElement = element
        };

        _smartStrategies.Add(strategy);
        lstSmartStrategies.Items.Add($"[{_smartStrategies.Count}] {name}: {description}");
    }

    /// <summary>
    /// Seçili akıllı stratejiyi test et butonuna tıklanınca
    /// </summary>
    private async void btnTestSelectedSmartStrategy_Click(object? sender, EventArgs e)
    {
        if (_selectedSmartStrategy == null)
        {
            ShowMessage("Lütfen önce bir strateji seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_lastRecordedElement == null)
        {
            ShowMessage("Element bilgisi bulunamadı! Önce 'Akıllı Seç' ile element seçin.",
                "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LogMessage($"\n=== SEÇİLİ AKILLI STRATEJİ TEST EDİLİYOR: {_selectedSmartStrategy.Name} ===");

        lblSmartTestResult.Text = "⏳ Test ediliyor...";
        lblSmartTestResult.ForeColor = Color.Blue;

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var playwrightSuccess = false;
            var uiaSuccess = false;
            var errors = new List<string>();

            // 1. Playwright ile test et (statik sayfa)
            if (File.Exists(_medulaHtmlPath))
            {
                try
                {
                    LogMessage("  📄 Playwright ile statik sayfa testi yapılıyor...");
                    playwrightSuccess = await PlaywrightRowAnalyzer.TestStrategyAsync(_selectedSmartStrategy, _medulaHtmlPath);
                    if (playwrightSuccess)
                    {
                        LogMessage("  ✅ Playwright testi başarılı (statik sayfa)");
                    }
                    else
                    {
                        errors.Add("Playwright testi başarısız");
                        LogMessage("  ⚠️ Playwright testi başarısız");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Playwright hatası: {ex.Message}");
                    LogMessage($"  ⚠️ Playwright hatası: {ex.Message}");
                }
            }
            else
            {
                errors.Add("Playwright kaynağı bulunamadı");
                LogMessage($"  ⚠️ Playwright kaynağı bulunamadı: {_medulaHtmlPath}");
            }

            // 2. UI Automation ile test et (canlı UI)
            if (_smartRecorder != null)
            {
                try
                {
                    LogMessage("  🖥️ UI Automation ile canlı UI testi yapılıyor...");
                    uiaSuccess = _smartRecorder.ExecuteLocatorStrategy(_selectedSmartStrategy);
                    if (uiaSuccess)
                    {
                        LogMessage("  ✅ UI Automation testi başarılı");
                    }
                    else
                    {
                        errors.Add("UI Automation testi başarısız");
                        LogMessage("  ❌ UI Automation testi başarısız");
                        if (playwrightSuccess)
                        {
                            LogMessage("  ⚠️ Not: Playwright selector statik sayfada çalıştı ancak canlı UI bulunamadı.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"UI Automation hatası: {ex.Message}");
                    LogMessage($"  ❌ UI Automation hatası: {ex.Message}");
                }
            }

            stopwatch.Stop();

            // Test sonucunu değerlendir
            var success = playwrightSuccess || uiaSuccess;

            // Stratejiyi güncelle
            _selectedSmartStrategy.IsSuccessful = success;
            _selectedSmartStrategy.TestDurationMs = (int)stopwatch.ElapsedMilliseconds;

            if (!success)
            {
                _selectedSmartStrategy.ErrorMessage = string.Join("; ", errors);
            }

            // Sonucu göster
            if (success)
            {
                LogMessage($"  ✅ Test başarılı! ({stopwatch.ElapsedMilliseconds}ms)");
                lblSmartTestResult.Text = $"✅ Test Başarılı - {stopwatch.ElapsedMilliseconds}ms";
                lblSmartTestResult.ForeColor = Color.Green;

                // Strateji listesini güncelle
                var index = lstSmartStrategies.SelectedIndex;
                if (index >= 0)
                {
                    lstSmartStrategies.Items[index] = $"[{index + 1}] {_selectedSmartStrategy.Name}: {_selectedSmartStrategy.Description} ✅";
                    lblSmartSelectedStrategy.Text = $"Seçili: {_selectedSmartStrategy.Name} ✅ BAŞARILI";
                    lblSmartSelectedStrategy.ForeColor = Color.Green;
                }
            }
            else
            {
                LogMessage($"  ❌ Test başarısız: {string.Join("; ", errors)}");
                lblSmartTestResult.Text = $"❌ Test Başarısız - {string.Join("; ", errors)}";
                lblSmartTestResult.ForeColor = Color.Red;

                // Strateji listesini güncelle
                var index = lstSmartStrategies.SelectedIndex;
                if (index >= 0)
                {
                    lstSmartStrategies.Items[index] = $"[{index + 1}] {_selectedSmartStrategy.Name}: {_selectedSmartStrategy.Description} ❌";
                    lblSmartSelectedStrategy.Text = $"Seçili: {_selectedSmartStrategy.Name} ❌ BAŞARISIZ";
                    lblSmartSelectedStrategy.ForeColor = Color.Red;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"\n❌ Test sırasında hata: {ex.Message}");
            lblSmartTestResult.Text = $"❌ Hata: {ex.Message}";
            lblSmartTestResult.ForeColor = Color.Red;
        }
    }

    /// <summary>
    /// Akıllı stratejileri test et butonuna tıklanınca
    /// </summary>
    private async void btnTestSmartStrategies_Click(object? sender, EventArgs e)
    {
        LogMessage($"[DEBUG] Test butonuna tıklandı");
        LogMessage($"[DEBUG] _lastRecordedElement: {(_lastRecordedElement == null ? "NULL" : "VAR")}");
        LogMessage($"[DEBUG] _smartStrategies.Count: {_smartStrategies.Count}");

        if (_lastRecordedElement == null || !_smartStrategies.Any())
        {
            ShowMessage("Önce 'Akıllı Element Seç' butonuna tıklayıp bir element seçin!",
                "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LogMessage("\n=== 🧪 AKILLI STRATEJİLERİ TEST EDİLİYOR ===");

        int successCount = 0;
        foreach (var strategy in _smartStrategies)
        {
            try
            {
                LogMessage($"\n[Test] {strategy.Name}: {strategy.Description}");

                var stopwatch = Stopwatch.StartNew();
                var errors = new List<string>();
                var success = false;
                var playwrightSuccess = false;
                var uiaSuccess = false;

                if (File.Exists(_medulaHtmlPath))
                {
                    try
                    {
                        var pwStartTime = Stopwatch.StartNew();
                        playwrightSuccess = await PlaywrightRowAnalyzer.TestStrategyAsync(strategy, _medulaHtmlPath);
                        pwStartTime.Stop();

                        if (playwrightSuccess)
                        {
                            LogMessage($"  ✅ Playwright testi başarılı ({pwStartTime.ElapsedMilliseconds}ms)");
                        }
                        else
                        {
                            errors.Add("Playwright testi başarısız");
                            LogMessage($"  ⚠️ Playwright testi başarısız ({pwStartTime.ElapsedMilliseconds}ms)");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Playwright hatası: {ex.Message}");
                        LogMessage($"  ⚠️ Playwright hatası: {ex.Message}");
                    }
                }
                else
                {
                    errors.Add("Playwright kaynağı bulunamadı");
                    LogMessage($"  ⚠️ Playwright kaynağı bulunamadı: {_medulaHtmlPath}");
                }

                if (_smartRecorder != null)
                {
                    var uiaStartTime = Stopwatch.StartNew();
                    uiaSuccess = _smartRecorder.ExecuteLocatorStrategy(strategy);
                    uiaStartTime.Stop();

                    if (uiaSuccess)
                    {
                        LogMessage($"  ✅ UI Automation testi başarılı ({uiaStartTime.ElapsedMilliseconds}ms)");
                    }
                    else
                    {
                        errors.Add("UI Automation testi başarısız");
                        LogMessage($"  ❌ UI Automation testi başarısız ({uiaStartTime.ElapsedMilliseconds}ms)");
                        if (playwrightSuccess)
                        {
                            LogMessage("  ⚠️ Not: Playwright selector statik sayfada çalıştı ancak canlı UI bulunamadı.");
                        }
                    }
                }
                else
                {
                    errors.Add("SmartElementRecorder hazır değil");
                    LogMessage("  ⚠️ SmartElementRecorder hazır değil, UI Automation testi atlandı");
                }

                success = uiaSuccess;

                stopwatch.Stop();

                strategy.TestDurationMs = (int)stopwatch.ElapsedMilliseconds;
                strategy.IsSuccessful = success;
                strategy.ErrorMessage = success ? null : string.Join(" | ", errors.Distinct());

                if (success)
                {
                    successCount++;
                    LogMessage($"  ✅ SONUÇ: Başarılı (Toplam: {stopwatch.ElapsedMilliseconds}ms)");
                }
                else
                {
                    LogMessage($"  ❌ SONUÇ: Başarısız (Toplam: {stopwatch.ElapsedMilliseconds}ms)");
                }

                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                strategy.IsSuccessful = false;
                strategy.ErrorMessage = ex.Message;
                LogMessage($"  ❌ HATA: {ex.Message}");
            }
        }

        // Sonuçları güncelle
        UpdateSmartStrategiesList();

        lblSmartTestResult.Text = $"Test tamamlandı: {successCount}/{_smartStrategies.Count} başarılı";
        lblSmartTestResult.ForeColor = successCount > 0 ? Color.Green : Color.Red;

        LogMessage($"\n📊 Test Sonuçları: {successCount}/{_smartStrategies.Count} başarılı");

        if (successCount > 0)
        {
            LogMessage("✅ En az bir strateji çalışıyor. İlk başarılı strateji otomatik olarak seçiliyor.");

            var bestStrategy = _smartStrategies.First(s => s.IsSuccessful);
            var bestIndex = _smartStrategies.IndexOf(bestStrategy);
            if (bestIndex >= 0)
            {
                lstSmartStrategies.SelectedIndex = bestIndex;
            }

            // Element bilgisini doldur (kaydetmek kullanıcıya bırakılır)
            if (_currentStep.StepType != StepType.UIElementAction)
            {
                cmbStepType.SelectedIndex = 1;
                _currentStep.StepType = StepType.UIElementAction;
            }

            _selectedSmartStrategy = bestStrategy;
            _currentStep.UIElement = SmartElementRecorder.ConvertToUIElementInfo(_lastRecordedElement);

            LogMessage("💾 Test başarılı - Şimdi 'Adımı Kaydet' butonuna tıklayabilirsiniz.");
            lblSmartTestResult.Text = "✅ Başarılı! Adımı kaydetmek için 'Adımı Kaydet' butonuna tıklayın.";
            lblSmartTestResult.ForeColor = Color.Green;
        }
        else
        {
            LogMessage("❌ Hiçbir strateji çalışmadı. Element yapısını ve kayıt adımlarını kontrol edin.");
        }
    }

    /// <summary>
    /// Strateji listesini test sonuçlarına göre günceller
    /// </summary>
    private void UpdateSmartStrategiesList()
    {
        lstSmartStrategies.Items.Clear();

        for (int i = 0; i < _smartStrategies.Count; i++)
        {
            var strategy = _smartStrategies[i];
            var prefix = strategy.IsSuccessful ? "✅" : "❌";
            var text = $"{prefix} [{i + 1}] {strategy.Name}: {strategy.Description}";
            lstSmartStrategies.Items.Add(text);
        }
    }

    /// <summary>
    /// Smart strateji listesinden bir öğe seçildiğinde
    /// </summary>
    private void lstSmartStrategies_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstSmartStrategies.SelectedIndex < 0 ||
            lstSmartStrategies.SelectedIndex >= _smartStrategies.Count)
        {
            _selectedSmartStrategy = null;
            lblSmartSelectedStrategy.Text = "Seçili: -";
            lblSmartSelectedStrategy.ForeColor = Color.Blue;
            btnTestSelectedSmartStrategy.Enabled = false;
            return;
        }

        var strategy = _smartStrategies[lstSmartStrategies.SelectedIndex];
        _selectedSmartStrategy = strategy;

        // Seçili strateji test butonunu aktif hale getir
        btnTestSelectedSmartStrategy.Enabled = true;

        // Element bilgisini akıllı kayıttan doldur
        if (_currentStep.UIElement == null && strategy.RecordedElement != null)
        {
            _currentStep.UIElement = SmartElementRecorder.ConvertToUIElementInfo(strategy.RecordedElement);
            LogMessage("ℹ️ UIElement bilgisi akıllı kayıttan dolduruldu.");
        }

        var statusText = strategy.IsSuccessful ? "✅ BAŞARILI" : "❌ BAŞARISIZ";
        lblSmartSelectedStrategy.Text = $"Seçili: {strategy.Name} {statusText}";
        lblSmartSelectedStrategy.ForeColor = strategy.IsSuccessful ? Color.Green : Color.Red;

        LogMessage($"\n📌 Seçili Akıllı Strateji: {strategy.Name}");
        LogMessage($"   Açıklama: {strategy.Description}");
        LogMessage($"   Durum: {statusText}");

        if (!strategy.IsSuccessful && !string.IsNullOrEmpty(strategy.ErrorMessage))
        {
            LogMessage($"   Hata: {strategy.ErrorMessage}");
        }
    }

    #endregion

    #region Task Chain Viewer Methods

    /// <summary>
    /// Sağ panelde görev zinciri adımlarını günceller
    /// S1, G1, S2, G2, S3, G3... formatında
    /// </summary>
    private void UpdateTaskChainViewer()
    {
        var sb = new System.Text.StringBuilder();

        // Başlık
        sb.AppendLine("═══════════════════════════════════════════════════════════════════");
        sb.AppendLine($"  Görev Zinciri: {txtChainName.Text}");
        sb.AppendLine($"  Toplam Adım: {_currentChain.Steps.Count}");
        sb.AppendLine("═══════════════════════════════════════════════════════════════════");
        sb.AppendLine();

        if (_currentChain.Steps.Count == 0)
        {
            sb.AppendLine("  (Henüz adım kaydedilmedi)");
            sb.AppendLine();
            sb.AppendLine("  Format:");
            sb.AppendLine("  S1 | Başlangıç Sayfası");
            sb.AppendLine("  G1 | UI Element İsmi | Görev | Teknoloji");
            sb.AppendLine("  S2 | Sonuç Sayfası (G1'den sonra)");
            sb.AppendLine("  G2 | UI Element İsmi | Görev | Teknoloji");
            sb.AppendLine("  ...");
        }
        else
        {
            // Her adım için S (Sayfa) ve G (Görev) satırlarını oluştur
            foreach (var step in _currentChain.Steps.OrderBy(s => s.StepNumber))
            {
                int stepNum = step.StepNumber;

                // S satırı - Hangi sayfadayız
                string pageName = GetPageName(step);
                sb.AppendLine($"S{stepNum} | {pageName}");

                // G satırı - Ne yapıyoruz
                if (step.StepType == StepType.UIElementAction ||
                    step.StepType == StepType.TargetSelection)
                {
                    string elementName = GetElementName(step);
                    string taskDesc = GetTaskDescription(step);
                    string technology = GetTechnologyInfo(step);

                    sb.AppendLine($"G{stepNum} | {elementName} | {taskDesc} | {technology}");
                }
                else if (step.StepType == StepType.ConditionalBranch)
                {
                    sb.AppendLine($"G{stepNum} | Koşul Kontrolü | Dallanma | Tip3");
                }
                else if (step.StepType == StepType.LoopOrEnd)
                {
                    string loopDesc = step.IsChainEnd ? "Zincir Bitir" : $"Döngü→{step.LoopBackToStepId}";
                    sb.AppendLine($"G{stepNum} | Döngü/Bitiş | {loopDesc} | Tip4");
                }

                sb.AppendLine(); // Boş satır (okunabilirlik için)
            }
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════════════");
        sb.AppendLine($"  Mevcut Adım: {_currentStepNumber}");
        sb.AppendLine("═══════════════════════════════════════════════════════════════════");

        txtTaskChainSteps.Text = sb.ToString();
    }

    /// <summary>
    /// Adımdan sayfa ismini çıkarır
    /// </summary>
    private string GetPageName(TaskStep step)
    {
        if (step.StepType == StepType.TargetSelection && step.Target != null)
        {
            return TruncateString(step.Target.WindowTitle ?? "Hedef Sayfa", 23);
        }
        else if (step.UIElement != null)
        {
            return TruncateString(step.UIElement.WindowTitle ?? step.UIElement.WindowName ?? "Sayfa", 23);
        }
        else if (step.StepType == StepType.ConditionalBranch)
        {
            return TruncateString(step.Condition?.PageIdentifier ?? "Koşul Sayfa", 23);
        }
        return "-";
    }

    /// <summary>
    /// Adımdan UI element ismini çıkarır
    /// </summary>
    private string GetElementName(TaskStep step)
    {
        if (step.UIElement != null)
        {
            // En anlamlı ismi bul
            if (!string.IsNullOrEmpty(step.UIElement.Name))
                return TruncateString(step.UIElement.Name, 30);
            if (!string.IsNullOrEmpty(step.UIElement.AutomationId))
                return TruncateString($"[{step.UIElement.AutomationId}]", 30);
            if (!string.IsNullOrEmpty(step.UIElement.ControlType))
                return TruncateString(step.UIElement.ControlType, 30);
        }

        if (step.StepType == StepType.TargetSelection)
        {
            // Hedef seçimi için hedef bilgisini göster
            if (step.Target != null)
            {
                if (step.Target.IsDesktop)
                    return "Masaüstü";
                if (!string.IsNullOrEmpty(step.Target.WindowTitle))
                    return TruncateString(step.Target.WindowTitle, 30);
                if (!string.IsNullOrEmpty(step.Target.ProgramPath))
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(step.Target.ProgramPath);
                    return TruncateString(fileName, 30);
                }
            }
            return "Hedef Pencere";
        }

        if (step.StepType == StepType.ConditionalBranch)
            return "Koşul Kontrolü";
        if (step.StepType == StepType.LoopOrEnd)
            return step.IsChainEnd ? "Zincir Sonu" : "Döngü";

        return "-";
    }

    /// <summary>
    /// Adımdan görev açıklamasını çıkarır
    /// </summary>
    private string GetTaskDescription(TaskStep step)
    {
        string desc = "";

        // Action type'a göre kısa ve net açıklama
        desc = step.Action switch
        {
            ActionType.LeftClick => "Sol Tık",
            ActionType.RightClick => "Sağ Tık",
            ActionType.DoubleClick => "Çift Tık",
            ActionType.KeyPress => $"Tuş[{TruncateString(step.KeysToPress ?? "", 10)}]",
            ActionType.TypeText => $"Yaz[{TruncateString(step.TextToType ?? "", 10)}]",
            ActionType.MouseWheel => step.MouseWheelDelta > 0 ? "Tekerlek↑" : "Tekerlek↓",
            ActionType.CheckCondition => "Koşul Kontrol",
            _ => ""
        };

        // StepType'a göre
        if (string.IsNullOrEmpty(desc))
        {
            if (step.StepType == StepType.TargetSelection)
                desc = "Pencereyi Aç/Seç";
            else if (step.StepType == StepType.ConditionalBranch)
                desc = "Koşul Kontrol";
            else if (step.StepType == StepType.LoopOrEnd && step.IsLoopEnd)
                desc = $"Döngü→{step.LoopBackToStepId}";
            else if (step.StepType == StepType.LoopOrEnd && step.IsChainEnd)
                desc = "Zincir Bitir";
        }

        return TruncateString(desc, 20);
    }

    /// <summary>
    /// Metni belirtilen uzunlukta keser ve "..." ekler
    /// </summary>
    private string TruncateString(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        if (text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Kullanılan teknoloji bilgisini döndürür
    /// </summary>
    private string GetTechnologyInfo(TaskStep step)
    {
        // Seçili strateji varsa ondan bilgi al
        var strategy = step.SelectedStrategy;
        if (strategy != null)
        {
            // Smart Element Recorder kullanıldıysa
            if (strategy.RecordedElement != null)
            {
                // Strateji tipine göre
                switch (strategy.Type)
                {
                    case LocatorType.TableRowIndex:
                        return "Smart:TableRow";
                    case LocatorType.TextContent:
                        return "Smart:CellText";
                    case LocatorType.ClassAndName:
                        return "Smart:Class+Name";
                    case LocatorType.PlaywrightSelector:
                        return "Smart:Playwright";
                    default:
                        return $"Smart:{strategy.Type}";
                }
            }

            // Normal UI Automation stratejisi
            switch (strategy.Type)
            {
                case LocatorType.AutomationId:
                    return "UIA:AutomationId";
                case LocatorType.Name:
                    return "UIA:Name";
                case LocatorType.ClassName:
                    return "UIA:ClassName";
                case LocatorType.AutomationIdAndControlType:
                    return "UIA:Id+Type";
                case LocatorType.NameAndControlType:
                    return "UIA:Name+Type";
                case LocatorType.ElementPath:
                    return "UIA:Path";
                case LocatorType.TreePath:
                    return "UIA:TreePath";
                case LocatorType.XPath:
                    return "Web:XPath";
                case LocatorType.CssSelector:
                    return "Web:CSS";
                case LocatorType.HtmlId:
                    return "Web:HtmlId";
                case LocatorType.NameAndParent:
                    return "UIA:Name+Parent";
                case LocatorType.ClassNameAndIndex:
                    return "UIA:Class+Index";
                case LocatorType.Coordinates:
                    return "Mouse:Coords";
                case LocatorType.NameAndControlTypeAndIndex:
                    return "UIA:Name+Type+Idx";
                case LocatorType.NameAndParentAndIndex:
                    return "UIA:Name+Parent+Idx";
                default:
                    return $"UIA:{strategy.Type}";
            }
        }

        // Strateji yoksa step type'a göre
        if (step.StepType == StepType.TargetSelection)
        {
            if (step.Target?.IsDesktop == true)
                return "System:Desktop";
            if (!string.IsNullOrEmpty(step.Target?.ProgramPath))
                return "System:Program";
            if (!string.IsNullOrEmpty(step.Target?.WindowTitle))
                return "UIA:Window";
            return "System";
        }

        // UIElement varsa DetectionMethod'a bak
        if (step.UIElement != null && !string.IsNullOrEmpty(step.UIElement.DetectionMethod))
        {
            return step.UIElement.DetectionMethod;
        }

        return "Unknown";
    }

    /// <summary>
    /// Son adımı sil butonu
    /// </summary>
    private void btnDeleteLastStep_Click(object? sender, EventArgs e)
    {
        if (_currentChain.Steps.Count == 0)
        {
            ShowMessage("Silinecek adım yok!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var lastStep = _currentChain.Steps.OrderBy(s => s.StepNumber).Last();

        var result = ShowMessage(
            $"Son adımı silmek istediğinizden emin misiniz?\n\n" +
            $"Adım {lastStep.StepNumber}: {lastStep.Description}",
            "Son Adımı Sil",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _currentChain.Steps.Remove(lastStep);
            LogMessage($"✓ Adım {lastStep.StepNumber} silindi");

            // Mevcut adım numarasını güncelle
            if (_currentStepNumber > 1)
                _currentStepNumber--;

            UpdateStepNumberLabel();
            UpdateTaskChainViewer();
        }
    }

    /// <summary>
    /// Belirli bir adımı sil (kullanıcıdan adım numarası sor)
    /// </summary>
    private void btnDeleteStep_Click(object? sender, EventArgs e)
    {
        if (_currentChain.Steps.Count == 0)
        {
            ShowMessage("Silinecek adım yok!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Adım listesini göster
        var stepList = string.Join("\n", _currentChain.Steps
            .OrderBy(s => s.StepNumber)
            .Select(s => $"Adım {s.StepNumber}: {s.Description}"));

        // Input dialog göster
        string input = Microsoft.VisualBasic.Interaction.InputBox(
            $"Silmek istediğiniz adımın numarasını girin:\n\n{stepList}",
            "Adım Sil",
            "");

        if (string.IsNullOrWhiteSpace(input))
            return;

        if (int.TryParse(input.Trim(), out int stepNumber))
        {
            var stepToDelete = _currentChain.Steps.FirstOrDefault(s => s.StepNumber == stepNumber);

            if (stepToDelete == null)
            {
                ShowMessage($"Adım {stepNumber} bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = ShowMessage(
                $"Bu adımı silmek istediğinizden emin misiniz?\n\n" +
                $"Adım {stepToDelete.StepNumber}: {stepToDelete.Description}\n\n" +
                $"NOT: Daha sonraki adımların numaraları değişmeyecektir.",
                "Adım Sil",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _currentChain.Steps.Remove(stepToDelete);
                LogMessage($"✓ Adım {stepToDelete.StepNumber} silindi");
                UpdateTaskChainViewer();
            }
        }
        else
        {
            ShowMessage("Geçersiz adım numarası!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Adımı düzenle
    /// </summary>
    private void btnEditStep_Click(object? sender, EventArgs e)
    {
        if (_currentChain.Steps.Count == 0)
        {
            ShowMessage("Düzenlenecek adım yok!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Düzenleme modundayken tekrar düzenle butonuna basılmasını engelle
        if (_isEditingMode)
        {
            ShowMessage("Zaten bir adımı düzenliyorsunuz. Lütfen önce 'Adımı Kaydet' butonuna basın veya düzenlemeyi iptal edin.",
                "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Adım listesini göster
        var stepList = string.Join("\n", _currentChain.Steps
            .OrderBy(s => s.StepNumber)
            .Select(s => $"Adım {s.StepNumber}: {s.Description}"));

        // Input dialog göster
        string input = Microsoft.VisualBasic.Interaction.InputBox(
            $"Düzenlemek istediğiniz adımın numarasını girin:\n\n{stepList}",
            "Adım Düzenle",
            "");

        if (string.IsNullOrWhiteSpace(input))
            return;

        if (int.TryParse(input.Trim(), out int stepNumber))
        {
            var stepToEdit = _currentChain.Steps.FirstOrDefault(s => s.StepNumber == stepNumber);

            if (stepToEdit == null)
            {
                ShowMessage($"Adım {stepNumber} bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Düzenleme moduna geç
            LoadStepForEditing(stepToEdit);
        }
        else
        {
            ShowMessage("Geçersiz adım numarası!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Adımı düzenleme için forma yükle
    /// </summary>
    private void LoadStepForEditing(TaskStep step)
    {
        _isEditingMode = true;
        _stepBeingEdited = step;

        LogMessage($"📝 Adım {step.StepNumber} düzenleme için yükleniyor...");

        // Header'ı güncelle
        lblTitle.Text = $"Görev Zinciri Kaydedici - Adım {step.StepNumber} Düzenleniyor";
        lblCurrentStep.Text = $"Düzenleme Modu: Adım {step.StepNumber}";
        lblCurrentStep.ForeColor = Color.FromArgb(255, 165, 0); // Turuncu renk

        // Buton metnini değiştir
        btnSaveStep.Text = "💾 Değişiklikleri Kaydet";
        btnSaveStep.BackColor = Color.FromArgb(255, 140, 0); // Turuncu

        // Adım tipine göre formu doldur
        switch (step.StepType)
        {
            case StepType.TargetSelection:
                cmbStepType.SelectedIndex = 0;
                LoadTargetSelectionForEditing(step);
                break;

            case StepType.UIElementAction:
                cmbStepType.SelectedIndex = 1;
                LoadUIElementActionForEditing(step);
                break;

            case StepType.ConditionalBranch:
                cmbStepType.SelectedIndex = 2;
                LogMessage("⚠️ Tip 3 (Koşullu Dallanma) adımları şu an düzenlenemez.");
                ShowMessage("Tip 3 (Koşullu Dallanma) adımları şu an düzenlenemez.\nBu özellik yakında eklenecektir.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CancelEditMode();
                break;
        }
    }

    /// <summary>
    /// Hedef seçimi bilgilerini forma yükle
    /// </summary>
    private void LoadTargetSelectionForEditing(TaskStep step)
    {
        if (step.Target == null) return;

        _currentStep = step;
        tabControl.SelectedTab = tabTargetSelection;

        if (step.Target.IsDesktop)
        {
            txtProgramPath.Text = "Hedef: Masaüstü";
        }
        else if (!string.IsNullOrEmpty(step.Target.ProgramPath))
        {
            txtProgramPath.Text = step.Target.ProgramPath;
        }
        else if (!string.IsNullOrEmpty(step.Target.WindowTitle))
        {
            txtProgramPath.Text = $"Pencere: {step.Target.WindowTitle} (Class: {step.Target.WindowClassName})";
        }

        LogMessage("✓ Hedef seçimi bilgileri yüklendi");
    }

    /// <summary>
    /// UI Element işlem bilgilerini forma yükle
    /// </summary>
    private void LoadUIElementActionForEditing(TaskStep step)
    {
        if (step.UIElement == null) return;

        _currentStep = step;
        tabControl.SelectedTab = tabUIElement;

        // Element özelliklerini göster
        var properties = new System.Text.StringBuilder();
        properties.AppendLine($"Name: {step.UIElement.Name ?? "N/A"}");
        properties.AppendLine($"AutomationId: {step.UIElement.AutomationId ?? "N/A"}");
        properties.AppendLine($"ClassName: {step.UIElement.ClassName ?? "N/A"}");
        properties.AppendLine($"ControlType: {step.UIElement.ControlType ?? "N/A"}");

        if (!string.IsNullOrEmpty(step.UIElement.BoundingRectangle))
        {
            properties.AppendLine($"BoundingRectangle: {step.UIElement.BoundingRectangle}");
        }
        else if (step.UIElement.X.HasValue && step.UIElement.Y.HasValue)
        {
            properties.AppendLine($"Position: X={step.UIElement.X}, Y={step.UIElement.Y}");
            if (step.UIElement.Width.HasValue && step.UIElement.Height.HasValue)
            {
                properties.AppendLine($"Size: W={step.UIElement.Width}, H={step.UIElement.Height}");
            }
        }

        txtElementProperties.Text = properties.ToString();

        // Action tipini ayarla
        cmbActionType.SelectedIndex = step.Action switch
        {
            ActionType.LeftClick => 0,
            ActionType.RightClick => 1,
            ActionType.DoubleClick => 2,
            ActionType.MouseWheel => 3,
            ActionType.KeyPress => 4,
            ActionType.TypeText => 5,
            _ => 0
        };

        // Klavye tuşları veya metni yükle
        if (step.Action == ActionType.KeyPress && !string.IsNullOrEmpty(step.KeysToPress))
        {
            txtKeysToPress.Text = step.KeysToPress;
            lblKeysToPress.Visible = true;
            txtKeysToPress.Visible = true;
        }
        else if (step.Action == ActionType.TypeText && !string.IsNullOrEmpty(step.TextToType))
        {
            txtKeysToPress.Text = step.TextToType;
            lblKeysToPress.Visible = true;
            txtKeysToPress.Visible = true;
        }

        // Stratejileri yükle
        if (step.SelectedStrategy != null)
        {
            _selectedStrategy = step.SelectedStrategy;
            _availableStrategies.Clear();
            _availableStrategies.Add(step.SelectedStrategy);

            lstStrategies.Items.Clear();
            var strategyDisplay = $"{step.SelectedStrategy.Name} [{(step.SelectedStrategy.IsSuccessful ? "✓" : "✗")}]";
            lstStrategies.Items.Add(strategyDisplay);
            lstStrategies.SelectedIndex = 0;

            lblSelectedStrategy.Text = $"Seçili Strateji: {step.SelectedStrategy.Name}";
            lblSelectedStrategy.ForeColor = step.SelectedStrategy.IsSuccessful ? Color.Green : Color.Red;
        }

        LogMessage("✓ UI Element işlem bilgileri yüklendi");
    }

    /// <summary>
    /// Düzenleme modunu iptal et
    /// </summary>
    private void CancelEditMode()
    {
        _isEditingMode = false;
        _stepBeingEdited = null;

        // Header'ı sıfırla
        lblTitle.Text = "Görev Zinciri Kaydedici";
        lblCurrentStep.Text = $"Adım: {_currentStepNumber}";
        lblCurrentStep.ForeColor = Color.FromArgb(100, 200, 255);

        // Buton metnini sıfırla
        btnSaveStep.Text = "💾 Adımı Kaydet";
        btnSaveStep.BackColor = Color.FromArgb(0, 120, 212);

        // Yeni bir adım oluştur
        _currentStep = new TaskStep
        {
            StepNumber = _currentStepNumber,
            StepType = StepType.TargetSelection
        };

        LogMessage("✓ Düzenleme modu iptal edildi");
    }

    /// <summary>
    /// Tüm adımları sil
    /// </summary>
    private void btnDeleteAllSteps_Click(object? sender, EventArgs e)
    {
        if (_currentChain.Steps.Count == 0)
        {
            ShowMessage("Silinecek adım yok!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = ShowMessage(
            $"TÜM ADıMLARI silmek istediğinizden emin misiniz?\n\n" +
            $"Toplam {_currentChain.Steps.Count} adım silinecek!\n\n" +
            $"Bu işlem geri alınamaz!",
            "Tüm Adımları Sil",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            var count = _currentChain.Steps.Count;
            _currentChain.Steps.Clear();
            _currentStepNumber = 1;

            UpdateStepNumberLabel();
            UpdateTaskChainViewer();
            LogMessage($"✓ Tüm adımlar silindi ({count} adım)");
        }
    }

    #endregion

    #region Tip 3 - Koşullu Dallanma

    /// <summary>
    /// Koşullu dallanma kaydedici formunu aç
    /// </summary>
    private void OpenConditionalBranchRecorder()
    {
        try
        {
            using var form = new ConditionalBranchRecorderForm();

            if (form.ShowDialog(this) == DialogResult.OK && form.Result != null)
            {
                // Koşul bilgisini mevcut adıma kaydet
                _currentStep.Condition = form.Result;
                _currentStep.Description = $"Koşullu Dallanma: {form.Result.PageIdentifier ?? "Sayfa kontrolü"}";

                LogMessage($"✓ Koşullu dallanma kaydedildi:");
                LogMessage($"  - Sayfa: {form.Result.PageIdentifier}");
                LogMessage($"  - Koşul sayısı: {form.Result.Conditions.Count}");
                LogMessage($"  - Dal sayısı: {form.Result.Branches.Count}");

                // Adımı otomatik kaydet
                btnSaveStep_Click(null, EventArgs.Empty);
            }
            else
            {
                LogMessage("Koşullu dallanma kaydı iptal edildi.");
                cmbStepType.SelectedIndex = 0; // Tip 1'e geri dön
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Koşullu dallanma kaydedici açılırken hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            LogMessage($"❌ Hata: {ex.Message}");
            cmbStepType.SelectedIndex = 0; // Tip 1'e geri dön
        }
    }

    #endregion
}
