namespace MedulaOtomasyon;

public partial class Form1 : Form
{
    private MedulaAutomation? _automation;
    private bool _isRunning = false;

    public Form1()
    {
        InitializeComponent();
        _automation = new MedulaAutomation(LogMessage);

        // Formu sağ alt köşede aç
        this.Load += Form1_Load;
    }

    private void Form1_Load(object? sender, EventArgs e)
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

    private void LogMessage(string message)
    {
        if (txtLog.InvokeRequired)
        {
            txtLog.Invoke(() => LogMessage(message));
            return;
        }

        txtLog.AppendText(message + Environment.NewLine);
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
    }

    private void UpdateStatus(string status)
    {
        if (lblStatus.InvokeRequired)
        {
            lblStatus.Invoke(() => UpdateStatus(status));
            return;
        }

        lblStatus.Text = $"Durum: {status}";
    }

    private void SetButtonsEnabled(bool enabled)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetButtonsEnabled(enabled));
            return;
        }

        btnAGrubu.Enabled = enabled;
        btnBGrubu.Enabled = enabled;
        btnCGrubu.Enabled = enabled;
    }

    private async void btnAGrubu_Click(object sender, EventArgs e)
    {
        if (_isRunning)
        {
            MessageBox.Show("Otomasyon zaten çalışıyor!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _isRunning = true;
        SetButtonsEnabled(false);
        UpdateStatus("A Grubu işlemi çalışıyor...");
        txtLog.Clear();

        try
        {
            await Task.Run(() => _automation!.RunAGrubuAsync());
            UpdateStatus("A Grubu tamamlandı");
            MessageBox.Show("A Grubu otomasyonu başarıyla tamamlandı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            UpdateStatus("Hata oluştu");
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _isRunning = false;
            SetButtonsEnabled(true);
        }
    }

    private void btnBGrubu_Click(object sender, EventArgs e)
    {
        MessageBox.Show("B Grubu henüz uygulanmadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnCGrubu_Click(object sender, EventArgs e)
    {
        MessageBox.Show("C Grubu henüz uygulanmadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnDebug_Click(object sender, EventArgs e)
    {
        txtLog.Clear();
        LogMessage("=== DEBUG BAŞLATILIYOR ===\n");

        try
        {
            _automation!.DebugListWindows(LogMessage);
            LogMessage("\n=== İlk 5 buton aranıyor ===\n");
            _automation!.DebugFindButtons(LogMessage);
        }
        catch (Exception ex)
        {
            LogMessage($"Debug hatası: {ex.Message}");
        }
    }

    private void btnTaskChainRecorder_Click(object sender, EventArgs e)
    {
        var recorderForm = new TaskChainRecorderForm();
        recorderForm.ShowDialog();
    }

    private void btnTaskChainPlayer_Click(object sender, EventArgs e)
    {
        var playerForm = new TaskChainPlayerForm();
        playerForm.ShowDialog();
    }

    private void btnTopmost_Click(object sender, EventArgs e)
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
}
