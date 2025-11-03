using System.IO;
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
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isAsyncOperationRunning = false;
    private UIElementInfo? _selectedElement; // Seçili element

    public ConditionInfo? Result { get; private set; }

    public ConditionalBranchRecorderForm()
    {
        try
        {
            DebugLog("Constructor: Starting...");
            InitializeComponent();
            DebugLog("Constructor: InitializeComponent done");
            _conditionInfo = new ConditionInfo();
            _availableElements = new List<UIElementInfo>();
            _cancellationTokenSource = new CancellationTokenSource();
            DebugLog("Constructor: Fields initialized");
            InitializeOperators();
            InitializeLogicalOperators();
            InitializeProperties();
            DebugLog("Constructor: ComboBoxes initialized");

            // Form kapatılırken async işlemleri iptal et
            this.FormClosing += (s, e) =>
            {
                DebugLog($"FormClosing: CloseReason = {e.CloseReason}, Cancel = {e.Cancel}, AsyncRunning = {_isAsyncOperationRunning}");

                // Eğer async işlem devam ediyorsa, kapatmayı TAMAMEN ENGELLE
                if (_isAsyncOperationRunning)
                {
                    DebugLog("FormClosing: BLOCKING - Async operation is running!");
                    e.Cancel = true; // Kapatmayı engelle
                    // Sessizce engelle, mesaj gösterme (çünkü otomatik kapanma denemeleri çok oluyor)
                    return;
                }

                _cancellationTokenSource?.Cancel();
                DebugLog("FormClosing: Allowed - Cancellation requested");
            };

            this.Load += (s, e) => DebugLog("Form.Load event fired");
            this.Shown += (s, e) => DebugLog("Form.Shown event fired");

            DebugLog("Constructor: Complete successfully");
        }
        catch (Exception ex)
        {
            DebugLog($"Constructor EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            MessageBox.Show($"Constructor Error:\n{ex.Message}\n\n{ex.StackTrace}", "Constructor Error");
            throw;
        }
    }

    private void DebugLog(string message)
    {
        try
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_conditional_form.txt");
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n";
            File.AppendAllText(logFile, logMessage);
        }
        catch { }
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
    /// Hedef sayfayı tespit et
    /// </summary>
    private async void BtnDetectTargetPage_Click(object? sender, EventArgs e)
    {
        DebugLog("=== BtnDetectTargetPage_Click: STARTED ===");
        try
        {
            DebugLog("BtnDetectTargetPage_Click: Calling DetectTargetPageAsync...");
            await DetectTargetPageAsync();
            DebugLog("BtnDetectTargetPage_Click: DetectTargetPageAsync completed");
        }
        catch (Exception ex)
        {
            DebugLog($"BtnDetectTargetPage_Click EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            DebugLog($"StackTrace: {ex.StackTrace}");
            MessageBox.Show($"FATAL ERROR in BtnDetectTargetPage_Click:\n\n{ex.GetType().Name}\n{ex.Message}\n\n{ex.StackTrace}",
                "Fatal Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        DebugLog("=== BtnDetectTargetPage_Click: ENDED ===");
    }

    /// <summary>
    /// Hedef sayfayı tespit et (async implementation)
    /// </summary>
    private async Task DetectTargetPageAsync()
    {
        DebugLog("DetectTargetPageAsync: Method started");

        // Async işlem başlıyor - flag'i set et
        _isAsyncOperationRunning = true;
        DebugLog("DetectTargetPageAsync: Set _isAsyncOperationRunning = true");

        if (_cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested)
        {
            DebugLog("DetectTargetPageAsync: Cancellation token check failed, returning");
            _isAsyncOperationRunning = false;
            return;
        }

        if (IsDisposed || !IsHandleCreated)
        {
            DebugLog("DetectTargetPageAsync: Form disposed or handle not created, returning");
            _isAsyncOperationRunning = false;
            return;
        }

        bool wasTopMost = false;
        try
        {
            wasTopMost = this.TopMost;
            DebugLog($"DetectTargetPageAsync: wasTopMost = {wasTopMost}");
        }
        catch (Exception ex)
        {
            DebugLog($"DetectTargetPageAsync: Error getting TopMost: {ex.Message}");
        }

        try
        {
            DebugLog("DetectTargetPageAsync: About to call SafeInvoke for UI update 1");
            // UI güncellemeleri
            SafeInvoke(() =>
            {
                btnDetectTargetPage.Enabled = false;
                lblDetectWarning.Text = "⏳ 3 saniye içinde hedef sayfaya tıklayın...";
                lblDetectWarning.ForeColor = System.Drawing.Color.Blue;
                DebugLog("DetectTargetPageAsync: UI updated - waiting message shown");
            });

            DebugLog("DetectTargetPageAsync: Starting 3 second delay");
            // 3 saniye bekle - iptal edilebilir
            await Task.Delay(3000, _cancellationTokenSource.Token);
            DebugLog("DetectTargetPageAsync: 3 second delay completed");

            // Form disposed oldu mu kontrol et
            if (IsDisposed || !IsHandleCreated || _cancellationTokenSource.Token.IsCancellationRequested)
                return;

            SafeInvoke(() =>
            {
                lblDetectWarning.Text = "🎯 Şimdi hedef sayfaya tıklayın!";
                lblDetectWarning.ForeColor = System.Drawing.Color.Red;
            });

            // Küçük bir delay ekle
            await Task.Delay(100);

            // Form disposed oldu mu kontrol et
            if (IsDisposed || !IsHandleCreated || _cancellationTokenSource.Token.IsCancellationRequested)
                return;

            // Formu gizle - try-catch ile koru
            try
            {
                SafeInvoke(() =>
                {
                    if (this.Visible)
                    {
                        this.Hide();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hide error: {ex.Message}");
            }

            await Task.Delay(500, _cancellationTokenSource.Token);

            // Form tekrar disposed oldu mu kontrol et
            if (IsDisposed || !IsHandleCreated || _cancellationTokenSource.Token.IsCancellationRequested)
                return;

            // Foreground window'u al
            var targetWindow = GetForegroundWindow();

            if (targetWindow == IntPtr.Zero)
            {
                SafeInvoke(() =>
                {
                    lblDetectWarning.Text = "❌ Hedef sayfa tespit edilemedi!";
                    lblDetectWarning.ForeColor = System.Drawing.Color.Red;
                });
                return;
            }

            // Window bilgisini al
            try
            {
                var rootElement = AutomationElement.FromHandle(targetWindow);
                var windowTitle = rootElement.Current.Name;
                var windowClassName = rootElement.Current.ClassName;
                var processId = rootElement.Current.ProcessId;

                // Process adını al
                string processName = "";
                try
                {
                    var process = System.Diagnostics.Process.GetProcessById(processId);
                    processName = process.ProcessName;
                }
                catch { }

                // Sayfa bilgisini textbox'a yaz (form disposed değilse)
                var pageInfo = $"{windowTitle} ({processName} - {windowClassName})";

                SafeInvoke(() =>
                {
                    txtPageIdentifier.Text = pageInfo;
                    lblDetectWarning.Text = $"✅ Hedef sayfa tespit edildi: {windowTitle}";
                    lblDetectWarning.ForeColor = System.Drawing.Color.Green;
                });
            }
            catch (Exception ex)
            {
                SafeInvoke(() =>
                {
                    lblDetectWarning.Text = $"❌ Hata: {ex.Message}";
                    lblDetectWarning.ForeColor = System.Drawing.Color.Red;
                });
            }
        }
        catch (OperationCanceledException)
        {
            // İptal edildi - sessizce çık
            System.Diagnostics.Debug.WriteLine("DetectTargetPageAsync: Operation cancelled");
        }
        catch (Exception ex)
        {
            // Detaylı hata loglama
            System.Diagnostics.Debug.WriteLine($"DetectTargetPageAsync Error: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

            try
            {
                SafeInvoke(() =>
                {
                    try
                    {
                        MessageBox.Show($"Hedef sayfa tespit hatası:\n\n{ex.GetType().Name}\n{ex.Message}\n\nDetay: {ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}",
                            "Hata",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        lblDetectWarning.Text = "❌ Bir hata oluştu!";
                        lblDetectWarning.ForeColor = System.Drawing.Color.Red;
                    }
                    catch { }
                });
            }
            catch { }
        }
        finally
        {
            // Async işlem bitti - flag'i temizle
            _isAsyncOperationRunning = false;
            DebugLog("DetectTargetPageAsync: Set _isAsyncOperationRunning = false (finally block)");

            // Form disposed olmadıysa göster
            try
            {
                DebugLog("DetectTargetPageAsync: About to SafeInvoke for showing form");
                SafeInvoke(() =>
                {
                    try
                    {
                        DebugLog($"DetectTargetPageAsync: Inside SafeInvoke - Visible={this.Visible}, IsDisposed={IsDisposed}");
                        if (!this.Visible)
                        {
                            DebugLog("DetectTargetPageAsync: Calling Show()");
                            this.Show();
                            this.BringToFront();
                            DebugLog("DetectTargetPageAsync: Show() completed");
                        }
                        this.TopMost = wasTopMost;
                        btnDetectTargetPage.Enabled = true;
                        DebugLog("DetectTargetPageAsync: Finally block UI updates completed");
                    }
                    catch (Exception ex)
                    {
                        DebugLog($"DetectTargetPageAsync: Finally block inner error: {ex.GetType().Name} - {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Finally block error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                DebugLog($"DetectTargetPageAsync: SafeInvoke in finally error: {ex.GetType().Name} - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SafeInvoke in finally error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// UI thread'inde güvenli bir şekilde metot çalıştır
    /// </summary>
    private void SafeInvoke(Action action)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        try
        {
            if (InvokeRequired)
            {
                // Senkron Invoke kullan (BeginInvoke değil)
                Invoke(new Action(() =>
                {
                    if (!IsDisposed && IsHandleCreated)
                    {
                        try
                        {
                            action();
                        }
                        catch (ObjectDisposedException) { }
                        catch (InvalidOperationException) { }
                    }
                }));
            }
            else
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    action();
                }
            }
        }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
    }

    /// <summary>
    /// Sayfadaki UI elementlerini listele
    /// </summary>
    private async void BtnRefreshElements_Click(object? sender, EventArgs e)
    {
        try
        {
            await RefreshElementsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"FATAL ERROR in BtnRefreshElements_Click:\n\n{ex.GetType().Name}\n{ex.Message}\n\n{ex.StackTrace}",
                "Fatal Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Sayfadaki UI elementlerini listele (async implementation)
    /// </summary>
    private async Task RefreshElementsAsync()
    {
        if (_cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested)
            return;

        if (IsDisposed || !IsHandleCreated)
            return;

        try
        {
            SafeInvoke(() =>
            {
                btnRefreshElements.Enabled = false;
                btnRefreshElements.Text = "⏳ Taranıyor...";
            });

            // Aktif pencereyi al
            var foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
            {
                SafeInvoke(() =>
                {
                    MessageBox.Show("Aktif pencere bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
                return;
            }

            // UI Automation ile elementleri topla
            var rootElement = AutomationElement.FromHandle(foregroundWindow);
            _availableElements.Clear();

            // Tüm elementleri recursive olarak topla - iptal edilebilir
            await Task.Run(() => CollectElements(rootElement), _cancellationTokenSource.Token);

            // Form disposed oldu mu kontrol et
            if (IsDisposed || !IsHandleCreated || _cancellationTokenSource.Token.IsCancellationRequested)
                return;

            // ComboBox'ı güncelle
            SafeInvoke(() =>
            {
                UpdateElementComboBox();
                MessageBox.Show($"{_availableElements.Count} element bulundu!", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
        catch (OperationCanceledException)
        {
            // İptal edildi - sessizce çık
        }
        catch (Exception ex)
        {
            SafeInvoke(() =>
            {
                try
                {
                    MessageBox.Show($"Element tarama hatası: {ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
            });
        }
        finally
        {
            SafeInvoke(() =>
            {
                btnRefreshElements.Enabled = true;
                btnRefreshElements.Text = "🔄 Elementleri Listele";
            });
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
            await PickElementAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"FATAL ERROR in BtnPickElement_Click:\n\n{ex.GetType().Name}\n{ex.Message}\n\n{ex.StackTrace}",
                "Fatal Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Element picker ile element seç (async implementation)
    /// </summary>
    private async Task PickElementAsync()
    {
        if (_cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested)
            return;

        if (IsDisposed || !IsHandleCreated)
            return;

        double originalOpacity = this.Opacity;
        bool wasTopMost = this.TopMost;

        try
        {
            // ÖNCE MessageBox'ı göster (form görünürken)
            DialogResult result = DialogResult.Cancel;

            if (InvokeRequired)
            {
                result = (DialogResult)Invoke(new Func<DialogResult>(() =>
                {
                    if (!IsDisposed && IsHandleCreated)
                    {
                        return MessageBox.Show("Tamam'a bastıktan sonra 2 saniye içinde\nmouse ile seçmek istediğiniz elemente tıklayın!",
                            "Element Seç",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Information);
                    }
                    return DialogResult.Cancel;
                }));
            }
            else
            {
                result = MessageBox.Show("Tamam'a bastıktan sonra 2 saniye içinde\nmouse ile seçmek istediğiniz elemente tıklayın!",
                    "Element Seç",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);
            }

            if (result != DialogResult.OK)
                return;

            // Formu yarı saydam yap ve arka plana gönder (GİZLEME, sadece transparan yap)
            SafeInvoke(() =>
            {
                this.TopMost = false;
                this.Opacity = 0.3; // %30 görünür, arka plandaki elementlere tıklanabilir
            });

            await Task.Delay(2000, _cancellationTokenSource.Token);

            // Form disposed oldu mu kontrol et
            if (IsDisposed || !IsHandleCreated || _cancellationTokenSource.Token.IsCancellationRequested)
                return;

            // Mouse pozisyonundaki elementi yakala
            var selectedElement = await UIElementPicker.CaptureElementAtMousePositionAsync();

            if (selectedElement != null && !IsDisposed && IsHandleCreated && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Seçilen elementi listeye ekle
                _availableElements.Add(selectedElement);

                SafeInvoke(() =>
                {
                    UpdateElementComboBox();
                    // Yeni eklenen elementi seç
                    cmbElement.SelectedIndex = _availableElements.Count - 1;
                    // Element özelliklerini göster
                    ShowElementProperties(selectedElement);

                    // Başarı mesajı
                    MessageBox.Show(
                        $"✅ Element başarıyla seçildi!\n\n" +
                        $"Element: {selectedElement.ControlType ?? "?"}\n" +
                        $"Name: {selectedElement.Name ?? selectedElement.AutomationId ?? "?"}\n\n" +
                        $"Sağ panelde tüm özellikleri görebilirsiniz.\n" +
                        $"Bir özelliğe ÇIFT TIKLAYIN koşul alanlarına otomatik dolsun.",
                        "Element Seçildi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                });
            }
        }
        catch (OperationCanceledException)
        {
            // İptal edildi - sessizce çık
        }
        catch (Exception ex)
        {
            SafeInvoke(() =>
            {
                try
                {
                    MessageBox.Show($"Element seçim hatası: {ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
            });
        }
        finally
        {
            // Formu eski haline getir
            SafeInvoke(() =>
            {
                this.Opacity = originalOpacity; // Opaklığı eski haline getir
                this.TopMost = wasTopMost; // TopMost'u eski haline getir
                this.BringToFront(); // Formu öne getir
            });
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
    /// Döngü sonlanma modu değiştiğinde
    /// </summary>
    private void ChkLoopTerminationMode_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkLoopTerminationMode.Checked)
        {
            // Döngü sonlanma modu aktif - kullanıcıyı bilgilendir
            MessageBox.Show(
                "Döngü Sonlanma Modu Aktif:\n\n" +
                "• Koşul TRUE ise: Program sonlanır\n" +
                "• Koşul FALSE ise: Belirtilen adıma döner (döngü devam eder)\n\n" +
                "Dallanma bölümünde FALSE durumu için döngü başlangıç adımını belirtin.",
                "Bilgi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
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
            _conditionInfo.IsLoopTerminationMode = chkLoopTerminationMode.Checked;

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

    #region Element Properties Display

    /// <summary>
    /// Element seçilince özelliklerini göster
    /// </summary>
    private void ShowElementProperties(UIElementInfo element)
    {
        if (element == null)
            return;

        _selectedElement = element;

        // Label güncelle
        lblSelectedElement.Text = $"📌 {element.ControlType ?? "Element"}: {element.Name ?? element.AutomationId ?? "İsimsiz"}";
        lblSelectedElement.ForeColor = System.Drawing.Color.DarkGreen;

        // DataGridView'ı temizle
        dgvElementProperties.Rows.Clear();

        // Tüm özellikleri ekle
        AddPropertyRow("AutomationId", element.AutomationId);
        AddPropertyRow("Name", element.Name);
        AddPropertyRow("ClassName", element.ClassName);
        AddPropertyRow("ControlType", element.ControlType);
        AddPropertyRow("LocalizedControlType", element.LocalizedControlType);
        AddPropertyRow("FrameworkId", element.FrameworkId);

        // Durum özellikleri
        AddPropertyRow("IsEnabled", element.IsEnabled?.ToString() ?? "null");
        AddPropertyRow("IsVisible", element.IsVisible?.ToString() ?? "null");
        AddPropertyRow("IsOffscreen", element.IsOffscreen?.ToString() ?? "null");
        AddPropertyRow("HasKeyboardFocus", element.HasKeyboardFocus?.ToString() ?? "null");
        AddPropertyRow("IsKeyboardFocusable", element.IsKeyboardFocusable?.ToString() ?? "null");
        AddPropertyRow("IsPassword", element.IsPassword?.ToString() ?? "null");

        // Text/Value özellikleri
        AddPropertyRow("InnerText", element.InnerText);
        AddPropertyRow("Value", element.Value);
        AddPropertyRow("HelpText", element.HelpText);

        // Web özellikleri
        if (!string.IsNullOrEmpty(element.HtmlId))
            AddPropertyRow("HtmlId", element.HtmlId);
        if (!string.IsNullOrEmpty(element.XPath))
            AddPropertyRow("XPath", element.XPath);
        if (!string.IsNullOrEmpty(element.CssSelector))
            AddPropertyRow("CssSelector", element.CssSelector);
        if (!string.IsNullOrEmpty(element.PlaywrightSelector))
            AddPropertyRow("PlaywrightSelector", element.PlaywrightSelector);
        if (!string.IsNullOrEmpty(element.Tag))
            AddPropertyRow("Tag", element.Tag);
        if (!string.IsNullOrEmpty(element.Placeholder))
            AddPropertyRow("Placeholder", element.Placeholder);

        // ARIA özellikleri
        if (!string.IsNullOrEmpty(element.AriaLabel))
            AddPropertyRow("AriaLabel", element.AriaLabel);
        if (!string.IsNullOrEmpty(element.AriaRole))
            AddPropertyRow("AriaRole", element.AriaRole);
        if (!string.IsNullOrEmpty(element.AriaChecked))
            AddPropertyRow("AriaChecked", element.AriaChecked);

        // Hiyerarşi bilgileri
        if (!string.IsNullOrEmpty(element.ParentName))
            AddPropertyRow("ParentName", element.ParentName);
        if (!string.IsNullOrEmpty(element.ParentAutomationId))
            AddPropertyRow("ParentAutomationId", element.ParentAutomationId);
        if (!string.IsNullOrEmpty(element.ContainerName))
            AddPropertyRow("ContainerName", element.ContainerName);
        if (!string.IsNullOrEmpty(element.ContainerAutomationId))
            AddPropertyRow("ContainerAutomationId", element.ContainerAutomationId);

        // Pencere bilgileri
        if (!string.IsNullOrEmpty(element.WindowTitle))
            AddPropertyRow("WindowTitle", element.WindowTitle);
        if (!string.IsNullOrEmpty(element.WindowProcessName))
            AddPropertyRow("WindowProcessName", element.WindowProcessName);

        // Konum ve boyut
        if (element.X.HasValue)
            AddPropertyRow("X", element.X.Value.ToString());
        if (element.Y.HasValue)
            AddPropertyRow("Y", element.Y.Value.ToString());
        if (element.Width.HasValue)
            AddPropertyRow("Width", element.Width.Value.ToString());
        if (element.Height.HasValue)
            AddPropertyRow("Height", element.Height.Value.ToString());

        // Index bilgileri
        if (element.IndexInParent.HasValue)
            AddPropertyRow("IndexInParent", element.IndexInParent.Value.ToString());
        if (element.SiblingIndex.HasValue)
            AddPropertyRow("SiblingIndex", element.SiblingIndex.Value.ToString());
    }

    /// <summary>
    /// DataGridView'a özellik satırı ekle
    /// </summary>
    private void AddPropertyRow(string propertyName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        dgvElementProperties.Rows.Add(propertyName, value);
    }

    /// <summary>
    /// DataGridView'da satıra çift tıklandığında koşul ekleme alanlarına doldur
    /// </summary>
    private void DgvElementProperties_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _selectedElement == null)
            return;

        try
        {
            var propertyName = dgvElementProperties.Rows[e.RowIndex].Cells[0].Value?.ToString();
            var propertyValue = dgvElementProperties.Rows[e.RowIndex].Cells[1].Value?.ToString();

            if (string.IsNullOrEmpty(propertyName))
                return;

            // Element ComboBox'ına ekle (eğer yoksa)
            if (!_availableElements.Contains(_selectedElement))
            {
                _availableElements.Add(_selectedElement);
                UpdateElementComboBox();
            }

            // Element'i seç
            var displayText = $"{_selectedElement.ControlType ?? "?"} - {_selectedElement.Name ?? _selectedElement.AutomationId ?? "??"}";
            cmbElement.SelectedIndex = cmbElement.Items.IndexOf(displayText);

            // Property'yi seç
            if (cmbProperty.Items.Contains(propertyName))
            {
                cmbProperty.SelectedItem = propertyName;
            }
            else
            {
                cmbProperty.Items.Add(propertyName);
                cmbProperty.SelectedItem = propertyName;
            }

            // Value'yu doldur
            txtValue.Text = propertyValue ?? "";

            // Operator'ü otomatik seç
            if (propertyValue?.ToLower() == "true" || propertyValue?.ToLower() == "false")
            {
                // Boolean için "Eşittir" seç
                cmbOperator.SelectedIndex = 0; // Eşittir
            }
            else if (propertyValue == "null" || string.IsNullOrEmpty(propertyValue))
            {
                // Boş için "Boş mu?" seç
                cmbOperator.SelectedIndex = 12; // Boş mu?
            }
            else
            {
                // Text için "Eşittir" seç
                cmbOperator.SelectedIndex = 0; // Eşittir
            }

            MessageBox.Show(
                $"Koşul alanlarına dolduruldu!\n\n" +
                $"Element: {_selectedElement.Name ?? _selectedElement.AutomationId}\n" +
                $"Özellik: {propertyName}\n" +
                $"Değer: {propertyValue}\n\n" +
                $"İsterseniz düzenleyip '+ Koşul Ekle' butonuna tıklayın.",
                "Otomatik Doldurma",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Otomatik doldurma hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion
}
