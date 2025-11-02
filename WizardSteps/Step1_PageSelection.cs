using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace MedulaOtomasyon;

/// <summary>
/// Wizard Adım 1: Hedef Sayfa Seçimi
/// </summary>
public partial class Step1_PageSelection : UserControl, IWizardStep
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private ConditionInfo _conditionInfo;
    private string? _selectedPageTitle;
    private string? _selectedProcessName;
    private int _selectedProcessId;

    public Step1_PageSelection(ConditionInfo conditionInfo)
    {
        InitializeComponent();
        _conditionInfo = conditionInfo;
    }

    public void OnStepEnter()
    {
        // Açık pencereleri listele
        RefreshWindowList();
    }

    public bool OnStepExit()
    {
        // Sayfa seçilmiş mi?
        if (string.IsNullOrEmpty(_selectedPageTitle))
        {
            MessageBox.Show("Lütfen bir hedef sayfa seçin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // ConditionInfo'ya kaydet
        _conditionInfo.PageIdentifier = $"{_selectedPageTitle} ({_selectedProcessName})";
        return true;
    }

    public bool CanProceed()
    {
        return !string.IsNullOrEmpty(_selectedPageTitle);
    }

    /// <summary>
    /// Açık pencereleri listele
    /// </summary>
    private void RefreshWindowList()
    {
        cmbWindows.Items.Clear();
        var windows = GetOpenWindows();

        foreach (var window in windows)
        {
            cmbWindows.Items.Add(window);
        }

        if (cmbWindows.Items.Count > 0)
        {
            cmbWindows.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Açık pencereleri al
    /// </summary>
    private List<WindowInfo> GetOpenWindows()
    {
        var windows = new List<WindowInfo>();

        EnumWindows((hWnd, lParam) =>
        {
            if (IsWindowVisible(hWnd))
            {
                var title = new System.Text.StringBuilder(256);
                GetWindowText(hWnd, title, title.Capacity);

                if (title.Length > 0)
                {
                    try
                    {
                        var element = AutomationElement.FromHandle(hWnd);
                        var processId = element.Current.ProcessId;
                        var process = System.Diagnostics.Process.GetProcessById(processId);

                        windows.Add(new WindowInfo
                        {
                            Title = title.ToString(),
                            ProcessName = process.ProcessName,
                            ProcessId = processId,
                            Handle = hWnd
                        });
                    }
                    catch { }
                }
            }
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    /// <summary>
    /// Mouse ile sayfa seç
    /// </summary>
    private async void BtnSelectWithMouse_Click(object? sender, EventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                "Tamam'a bastıktan sonra 3 saniye içinde\nhedef sayfaya tıklayın!",
                "Sayfa Seç",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (result != DialogResult.OK)
                return;

            // Formu gizle
            var parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.Opacity = 0.3;
            }

            await Task.Delay(3000);

            // Foreground window'u al
            var targetWindow = GetForegroundWindow();

            if (targetWindow != IntPtr.Zero)
            {
                var element = AutomationElement.FromHandle(targetWindow);
                var processId = element.Current.ProcessId;
                var process = System.Diagnostics.Process.GetProcessById(processId);

                _selectedPageTitle = element.Current.Name;
                _selectedProcessName = process.ProcessName;
                _selectedProcessId = processId;

                lblSelectedPage.Text = $"✅ Seçili: {_selectedPageTitle} ({_selectedProcessName})";
                lblSelectedPage.ForeColor = System.Drawing.Color.DarkGreen;

                // Parent formun butonlarını güncelle
                NotifySelectionChanged();
            }

            // Formu göster
            if (parentForm != null)
            {
                parentForm.Opacity = 1.0;
                parentForm.BringToFront();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Sayfa seçim hatası: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            var parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.Opacity = 1.0;
            }
        }
    }

    /// <summary>
    /// Listeden pencere seç
    /// </summary>
    private void BtnSelectFromList_Click(object? sender, EventArgs e)
    {
        if (cmbWindows.SelectedItem is WindowInfo window)
        {
            _selectedPageTitle = window.Title;
            _selectedProcessName = window.ProcessName;
            _selectedProcessId = window.ProcessId;

            lblSelectedPage.Text = $"✅ Seçili: {_selectedPageTitle} ({_selectedProcessName})";
            lblSelectedPage.ForeColor = System.Drawing.Color.DarkGreen;

            // Parent formun butonlarını güncelle
            NotifySelectionChanged();
        }
        else
        {
            MessageBox.Show("Lütfen listeden bir pencere seçin!", "Uyarı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>
    /// Seçim değiştiğinde parent formu bilgilendir
    /// </summary>
    private void NotifySelectionChanged()
    {
        // Parent form'u bul ve UpdateButtons çağır
        var parentForm = this.FindForm() as ConditionalBranchWizard;
        if (parentForm != null)
        {
            // Parent formun UpdateButtons metodunu çağır
            parentForm.RefreshButtons();
        }
    }

    /// <summary>
    /// Listeyi yenile
    /// </summary>
    private void BtnRefresh_Click(object? sender, EventArgs e)
    {
        RefreshWindowList();
    }
}

/// <summary>
/// Pencere bilgisi
/// </summary>
public class WindowInfo
{
    public string Title { get; set; } = "";
    public string ProcessName { get; set; } = "";
    public int ProcessId { get; set; }
    public IntPtr Handle { get; set; }

    public override string ToString()
    {
        return $"{Title} ({ProcessName})";
    }
}
