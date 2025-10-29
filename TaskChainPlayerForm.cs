using System.Windows.Forms;

namespace MedulaOtomasyon;

public partial class TaskChainPlayerForm : Form
{
    private readonly TaskChainDatabase _chainDb;
    private readonly TaskChainExecutor _executor;
    private List<TaskChain> _chains = new();
    private TaskChain? _selectedChain;
    private bool _isDebugMode = false;

    public TaskChainPlayerForm()
    {
        InitializeComponent();
        _chainDb = new TaskChainDatabase();
        _executor = new TaskChainExecutor();

        // Event handlers
        _executor.ExecutionStarted += Executor_ExecutionStarted;
        _executor.ExecutionCompleted += Executor_ExecutionCompleted;
        _executor.ExecutionFailed += Executor_ExecutionFailed;
        _executor.ExecutionPaused += Executor_ExecutionPaused;
        _executor.ExecutionResumed += Executor_ExecutionResumed;
        _executor.ExecutionStopped += Executor_ExecutionStopped;
        _executor.StepStarted += Executor_StepStarted;
        _executor.StepCompleted += Executor_StepCompleted;
        _executor.StepFailed += Executor_StepFailed;
        _executor.LogMessage += Executor_LogMessage;
        _executor.ErrorOccurred += Executor_ErrorOccurred;
    }

    private void TaskChainPlayerForm_Load(object? sender, EventArgs e)
    {
        // Formu sağ tarafta aç, üst kısım ekranda kalacak şekilde
        var workingArea = Screen.PrimaryScreen!.WorkingArea;
        this.StartPosition = FormStartPosition.Manual;

        // X koordinatı: Sağ kenar
        int x = workingArea.Right - this.Width;

        // Y koordinatı: Alt köşe ama üst kısım ekranda kalacak şekilde
        int y = workingArea.Bottom - this.Height;

        // Eğer form ekran yüksekliğinden uzunsa, üstten başlat
        if (y < workingArea.Top)
        {
            y = workingArea.Top;
        }

        this.Location = new Point(x, y);

        LoadChains();
        Log("Görev Zinciri Oynatıcı hazır.");
    }

    /// <summary>
    /// Task chain listesini yükle
    /// </summary>
    private void LoadChains()
    {
        _chains = _chainDb.LoadAll();
        lstChains.Items.Clear();

        if (_chains.Count == 0)
        {
            lstChains.Items.Add("(Henüz kaydedilmiş görev yok)");
            Log("⚠ Henüz kaydedilmiş görev zinciri bulunamadı.");
        }
        else
        {
            foreach (var chain in _chains)
            {
                lstChains.Items.Add($"📋 {chain.Name} ({chain.Steps.Count} adım)");
            }
            Log($"✓ {_chains.Count} görev zinciri yüklendi.");
        }
    }

    /// <summary>
    /// Chain seçildiğinde adımları göster
    /// </summary>
    private void lstChains_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstChains.SelectedIndex < 0 || lstChains.SelectedIndex >= _chains.Count)
        {
            _selectedChain = null;
            lstSteps.Items.Clear();
            btnPlay.Enabled = false;
            return;
        }

        _selectedChain = _chains[lstChains.SelectedIndex];
        DisplayChainSteps(_selectedChain);
        btnPlay.Enabled = true;

        Log($"Görev seçildi: {_selectedChain.Name}");
    }

    /// <summary>
    /// Seçili chain'in adımlarını göster
    /// </summary>
    private void DisplayChainSteps(TaskChain chain)
    {
        lstSteps.Items.Clear();

        foreach (var step in chain.Steps)
        {
            var icon = step.StepType switch
            {
                StepType.TargetSelection => "🎯",
                StepType.UIElementAction => GetActionIcon(step.Action),
                StepType.ConditionalBranch => "🔀",
                StepType.LoopOrEnd => "🔁",
                _ => "❓"
            };

            lstSteps.Items.Add($"{icon} Adım {step.StepNumber}: {step.Description}");
        }
    }

    /// <summary>
    /// Action için ikon
    /// </summary>
    private string GetActionIcon(ActionType action)
    {
        return action switch
        {
            ActionType.LeftClick => "🖱️",
            ActionType.DoubleClick => "🖱️🖱️",
            ActionType.TypeText => "⌨️",
            ActionType.KeyPress => "⌨️",
            _ => "▶️"
        };
    }

    /// <summary>
    /// Yenile butonu
    /// </summary>
    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        LoadChains();
    }

    /// <summary>
    /// Oynat butonu
    /// </summary>
    private async void btnPlay_Click(object? sender, EventArgs e)
    {
        if (_selectedChain == null)
        {
            ShowMessage("Lütfen bir görev zinciri seçin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Executor ayarlarını uygula
        ApplyExecutorSettings();

        // UI durumunu değiştir
        SetExecutionUIState(isRunning: true);

        // Progress'i sıfırla
        progressBar.Value = 0;
        progressBar.Maximum = _selectedChain.Steps.Count;
        lblProgress.Text = $"Progress: 0/{_selectedChain.Steps.Count} (0%)";
        lblCurrentStep.Text = "Şu anda: Başlatılıyor...";

        Log($"▶ Çalıştırma başlatılıyor: {_selectedChain.Name}");
        Log("─────────────────────────────────────────");

        // Executor'u başlat (async)
        await _executor.ExecuteAsync(_selectedChain);

        // Tamamlandığında UI'yı sıfırla
        SetExecutionUIState(isRunning: false);
    }

    /// <summary>
    /// Duraklat/Devam butonu
    /// </summary>
    private void btnPause_Click(object? sender, EventArgs e)
    {
        if (_executor.IsPaused)
        {
            _executor.Resume();
            btnPause.Text = "⏸ Duraklat";
        }
        else
        {
            _executor.Pause();
            btnPause.Text = "▶ Devam";
        }
    }

    /// <summary>
    /// Durdur butonu
    /// </summary>
    private void btnStop_Click(object? sender, EventArgs e)
    {
        var result = ShowMessage(
            "Çalıştırmayı durdurmak istediğinizden emin misiniz?",
            "Onay",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _executor.Stop();
            Log("⏹ Kullanıcı tarafından durduruldu.");
        }
    }

    /// <summary>
    /// Debug butonu
    /// </summary>
    private void btnDebug_Click(object? sender, EventArgs e)
    {
        _isDebugMode = !_isDebugMode;
        btnDebug.BackColor = _isDebugMode ? Color.Yellow : SystemColors.Control;
        btnDebug.ForeColor = _isDebugMode ? Color.Black : SystemColors.ControlText;

        Log(_isDebugMode ? "🐛 Debug modu aktif" : "Debug modu kapalı");
    }

    /// <summary>
    /// Durdur ve Düzenle butonu
    /// </summary>
    private void btnStopAndEdit_Click(object? sender, EventArgs e)
    {
        var result = ShowMessage(
            "Çalıştırmayı durdurup düzenleme moduna geçmek istiyor musunuz?\n\n" +
            "Zincir ve şu anki adım Görev Kaydedici'ye yüklenecek.",
            "Durdur ve Düzenle",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            // Executor'u durdur
            _executor.Stop();
            Log("⏹ Çalıştırma durduruldu - Düzenleme moduna geçiliyor...");

            // Görev kaydedici formunu aç ve zinciri yükle
            OpenRecorderForEditing(_selectedChain!, _executor.CurrentStepIndex);
        }
    }

    /// <summary>
    /// Zinciri Düzenle butonu (çalışmıyorken)
    /// </summary>
    private void btnEditChain_Click(object? sender, EventArgs e)
    {
        if (_selectedChain == null)
        {
            ShowMessage("Lütfen düzenlemek için bir görev zinciri seçin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenRecorderForEditing(_selectedChain, -1);
    }

    /// <summary>
    /// Görev kaydediciyi düzenleme modunda aç
    /// </summary>
    private void OpenRecorderForEditing(TaskChain chain, int currentStepIndex)
    {
        try
        {
            var recorderForm = new TaskChainRecorderForm();
            recorderForm.LoadChainForEditing(chain, currentStepIndex);
            recorderForm.Show();

            Log($"✏️ Görev zinciri düzenleme için açıldı: {chain.Name}");

            if (currentStepIndex >= 0)
            {
                Log($"   Şu anki adım: {currentStepIndex + 1}");
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Görev kaydedici açılırken hata oluştu:\n{ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log($"❌ HATA: {ex.Message}");
        }
    }

    /// <summary>
    /// Log temizle
    /// </summary>
    private void btnClearLog_Click(object? sender, EventArgs e)
    {
        txtLog.Clear();
    }

    /// <summary>
    /// Executor ayarlarını uygula
    /// </summary>
    private void ApplyExecutorSettings()
    {
        // Hız
        if (rbSlow.Checked)
            _executor.Speed = ExecutionSpeed.Slow;
        else if (rbNormal.Checked)
            _executor.Speed = ExecutionSpeed.Normal;
        else if (rbFast.Checked)
            _executor.Speed = ExecutionSpeed.Fast;

        // Hata yönetimi
        if (rbStop.Checked)
            _executor.DefaultErrorAction = ErrorAction.Stop;
        else if (rbRetry.Checked)
            _executor.DefaultErrorAction = ErrorAction.Retry;
        else if (rbSkip.Checked)
            _executor.DefaultErrorAction = ErrorAction.Skip;

        // Seçenekler
        _executor.ScreenshotEnabled = chkScreenshot.Checked;
        _executor.DebugMode = _isDebugMode;

        Log($"Ayarlar: Hız={_executor.Speed}, Hata={_executor.DefaultErrorAction}, Screenshot={_executor.ScreenshotEnabled}, Debug={_executor.DebugMode}");
    }

    /// <summary>
    /// UI durumunu değiştir (çalışıyor/durdu)
    /// </summary>
    private void SetExecutionUIState(bool isRunning)
    {
        btnPlay.Enabled = !isRunning;
        btnPause.Enabled = isRunning;
        btnStop.Enabled = isRunning;
        btnStopAndEdit.Enabled = isRunning;
        lstChains.Enabled = !isRunning;
        btnRefresh.Enabled = !isRunning;
        btnEditChain.Enabled = !isRunning && _selectedChain != null;
        grpSpeed.Enabled = !isRunning;
        grpErrorHandling.Enabled = !isRunning;
        grpOptions.Enabled = !isRunning;
    }

    /// <summary>
    /// Log mesajı ekle
    /// </summary>
    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => Log(message));
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        txtLog.AppendText($"[{timestamp}] {message}\r\n");
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
    }

    #region Executor Event Handlers

    private void Executor_ExecutionStarted(object? sender, ExecutionEventArgs e)
    {
        Log($"✅ Çalıştırma başladı: {e.Record.ChainName}");
    }

    private void Executor_ExecutionCompleted(object? sender, ExecutionEventArgs e)
    {
        Log("─────────────────────────────────────────");
        Log($"✅ Çalıştırma tamamlandı!");
        Log($"   Toplam süre: {e.Record.TotalDurationMs}ms");
        Log($"   Başarılı adımlar: {e.Record.SuccessfulSteps}/{e.Record.TotalSteps}");
        Log($"   Başarısız adımlar: {e.Record.FailedSteps}");
        Log($"   Atlanan adımlar: {e.Record.SkippedSteps}");
        Log("─────────────────────────────────────────");

        if (InvokeRequired)
        {
            Invoke(() =>
            {
                ShowMessage(
                    $"Görev zinciri başarıyla tamamlandı!\n\n" +
                    $"Toplam süre: {e.Record.TotalDurationMs}ms\n" +
                    $"Başarılı: {e.Record.SuccessfulSteps}/{e.Record.TotalSteps}",
                    "Başarılı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            });
        }
    }

    private void Executor_ExecutionFailed(object? sender, ExecutionEventArgs e)
    {
        Log("─────────────────────────────────────────");
        Log($"❌ Çalıştırma başarısız!");
        Log($"   Hata: {e.Message}");
        Log($"   Başarılı adımlar: {e.Record.SuccessfulSteps}/{e.Record.TotalSteps}");
        Log("─────────────────────────────────────────");

        if (InvokeRequired)
        {
            Invoke(() =>
            {
                ShowMessage(
                    $"Görev zinciri başarısız!\n\n" +
                    $"Hata: {e.Message}\n" +
                    $"Başarılı: {e.Record.SuccessfulSteps}/{e.Record.TotalSteps}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            });
        }
    }

    private void Executor_ExecutionPaused(object? sender, ExecutionEventArgs e)
    {
        Log("⏸ Çalıştırma duraklatıldı.");
    }

    private void Executor_ExecutionResumed(object? sender, ExecutionEventArgs e)
    {
        Log("▶ Çalıştırma devam ediyor...");
    }

    private void Executor_ExecutionStopped(object? sender, ExecutionEventArgs e)
    {
        Log("⏹ Çalıştırma durduruldu.");
    }

    private void Executor_StepStarted(object? sender, StepExecutionEventArgs e)
    {
        Log($"▶ Adım {e.Step.StepNumber}: {e.Step.Description}");

        if (InvokeRequired)
        {
            Invoke(() => UpdateProgress(e.Step.StepNumber, "Running"));
        }
        else
        {
            UpdateProgress(e.Step.StepNumber, "Running");
        }
    }

    private void Executor_StepCompleted(object? sender, StepExecutionEventArgs e)
    {
        Log($"  ✓ Tamamlandı ({e.StepRecord.DurationMs}ms)");

        if (InvokeRequired)
        {
            Invoke(() => UpdateProgress(e.Step.StepNumber, "Success"));
        }
        else
        {
            UpdateProgress(e.Step.StepNumber, "Success");
        }
    }

    private void Executor_StepFailed(object? sender, StepExecutionEventArgs e)
    {
        Log($"  ✗ Başarısız: {e.Message}");

        if (InvokeRequired)
        {
            Invoke(() => UpdateProgress(e.Step.StepNumber, "Failed"));
        }
        else
        {
            UpdateProgress(e.Step.StepNumber, "Failed");
        }
    }

    private void Executor_LogMessage(object? sender, string message)
    {
        Log($"  {message}");
    }

    /// <summary>
    /// Hata olduğunda kullanıcıya sor
    /// </summary>
    private ErrorAction Executor_ErrorOccurred(TaskStep step, string errorMessage)
    {
        if (InvokeRequired)
        {
            return (ErrorAction)Invoke(() => Executor_ErrorOccurred(step, errorMessage));
        }

        var dialog = new Form
        {
            Text = "Hata Oluştu",
            Width = 450,
            Height = 180,
            StartPosition = FormStartPosition.Manual,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            TopMost = true
        };

        // Dialog'u sağ alt köşede aç
        var workingArea = Screen.PrimaryScreen!.WorkingArea;
        dialog.Location = new Point(
            workingArea.Right - dialog.Width,
            workingArea.Bottom - dialog.Height
        );

        var lblMessage = new Label
        {
            Text = $"Adım {step.StepNumber} sırasında hata oluştu:\n\n{errorMessage}\n\nNe yapmak istersiniz?",
            Location = new Point(10, 10),
            Size = new Size(410, 60),
            AutoSize = false
        };

        var btnStop = new Button
        {
            Text = "⏹ Durdur",
            Location = new Point(10, 80),
            Size = new Size(100, 40),
            DialogResult = DialogResult.Abort
        };

        var btnRetry = new Button
        {
            Text = "🔄 Tekrar Dene",
            Location = new Point(120, 80),
            Size = new Size(100, 40),
            DialogResult = DialogResult.Retry
        };

        var btnSkip = new Button
        {
            Text = "⏭ Atla",
            Location = new Point(230, 80),
            Size = new Size(100, 40),
            DialogResult = DialogResult.Ignore
        };

        var btnCancel = new Button
        {
            Text = "❌ İptal",
            Location = new Point(340, 80),
            Size = new Size(100, 40),
            DialogResult = DialogResult.Cancel
        };

        dialog.Controls.AddRange(new Control[] { lblMessage, btnStop, btnRetry, btnSkip, btnCancel });
        dialog.AcceptButton = btnRetry;
        dialog.CancelButton = btnCancel;

        // Executor durumunu kontrol eden timer - dialog açıkken Stop basılırsa dialog'u otomatik kapat
        var checkTimer = new System.Windows.Forms.Timer();
        checkTimer.Interval = 100;
        checkTimer.Tick += (s, e) =>
        {
            if (!_executor.IsRunning)
            {
                checkTimer.Stop();
                dialog.DialogResult = DialogResult.Cancel;
                dialog.Close();
            }
        };
        checkTimer.Start();

        var result = dialog.ShowDialog();
        checkTimer.Stop();
        checkTimer.Dispose();

        // Cancel durumunda executor'ı durdur
        if (result == DialogResult.Cancel)
        {
            _executor.Stop();
        }

        return result switch
        {
            DialogResult.Abort => ErrorAction.Stop,
            DialogResult.Retry => ErrorAction.Retry,
            DialogResult.Ignore => ErrorAction.Skip,
            DialogResult.Cancel => ErrorAction.Stop,
            _ => ErrorAction.Stop
        };
    }

    #endregion

    /// <summary>
    /// Progress güncelle
    /// </summary>
    private void UpdateProgress(int currentStep, string status)
    {
        if (_selectedChain == null) return;

        progressBar.Value = Math.Min(currentStep, progressBar.Maximum);

        var percentage = (int)((double)currentStep / _selectedChain.Steps.Count * 100);
        lblProgress.Text = $"Progress: {currentStep}/{_selectedChain.Steps.Count} ({percentage}%)";

        var statusIcon = status switch
        {
            "Running" => "⏳",
            "Success" => "✅",
            "Failed" => "❌",
            _ => "▶"
        };

        lblCurrentStep.Text = $"Şu anda: {statusIcon} Adım {currentStep}";
        lblCurrentStep.ForeColor = status switch
        {
            "Running" => Color.Blue,
            "Success" => Color.Green,
            "Failed" => Color.Red,
            _ => Color.Black
        };

        // Listbox'ta ilgili adımı highlight et
        if (currentStep - 1 < lstSteps.Items.Count)
        {
            lstSteps.SelectedIndex = currentStep - 1;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Çalışıyorsa durdur
        if (_executor.IsRunning)
        {
            var result = ShowMessage(
                "Görev zinciri hala çalışıyor. Kapatmak istediğinizden emin misiniz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            _executor.Stop();
        }

        base.OnFormClosing(e);
    }

    /// <summary>
    /// MessageBox göster - Form topmost ise MessageBox da topmost olur
    /// </summary>
    private DialogResult ShowMessage(string text, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
    {
        return MessageBox.Show(this, text, caption, buttons, icon);
    }
}
