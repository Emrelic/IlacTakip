using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MedulaOtomasyon;

/// <summary>
/// Kayıtlı görev zincirlerinden birini seçmek için dialog
/// </summary>
public partial class TaskChainSelectorDialog : Form
{
    private ListBox lstChains;
    private TextBox txtSearch;
    private Label lblSearch;
    private Label lblChains;
    private Button btnSelect;
    private Button btnCancel;
    private Button btnDelete;
    private TextBox txtPreview;
    private Label lblPreview;
    private Panel pnlTop;
    private Panel pnlMiddle;
    private Panel pnlBottom;
    private Panel pnlPreview;

    private readonly TaskChainDatabase _database;
    private List<TaskChain> _chains;
    private List<TaskChain> _filteredChains;

    public TaskChain? SelectedChain { get; private set; }

    public TaskChainSelectorDialog(TaskChainDatabase database)
    {
        _database = database;
        _chains = new List<TaskChain>();
        _filteredChains = new List<TaskChain>();

        InitializeComponent();
        LoadChains();
    }

    private void InitializeComponent()
    {
        Text = "Görev Zinciri Seç";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Top Panel - Arama
        pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        lblSearch = new Label
        {
            Text = "Görev Ara:",
            Location = new Point(10, 15),
            Size = new Size(100, 20),
            Font = new Font("Segoe UI", 10F)
        };

        txtSearch = new TextBox
        {
            Location = new Point(10, 35),
            Size = new Size(760, 25),
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = "Görev adı veya açıklama ile ara..."
        };
        txtSearch.TextChanged += TxtSearch_TextChanged;

        pnlTop.Controls.AddRange(new Control[] { lblSearch, txtSearch });

        // Middle Panel - Liste ve Önizleme
        pnlMiddle = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        lblChains = new Label
        {
            Text = "Kayıtlı Görev Zincirleri:",
            Location = new Point(10, 5),
            Size = new Size(360, 20),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        lstChains = new ListBox
        {
            Location = new Point(10, 30),
            Size = new Size(360, 380),
            Font = new Font("Segoe UI", 9.5F),
            ScrollAlwaysVisible = true
        };
        lstChains.SelectedIndexChanged += LstChains_SelectedIndexChanged;
        lstChains.DoubleClick += LstChains_DoubleClick;

        // Preview Panel
        pnlPreview = new Panel
        {
            Location = new Point(380, 5),
            Size = new Size(400, 405),
            BorderStyle = BorderStyle.FixedSingle
        };

        lblPreview = new Label
        {
            Text = "Önizleme:",
            Location = new Point(5, 5),
            Size = new Size(390, 20),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtPreview = new TextBox
        {
            Location = new Point(5, 30),
            Size = new Size(390, 370),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(250, 250, 250),
            Font = new Font("Consolas", 9F)
        };

        pnlPreview.Controls.AddRange(new Control[] { lblPreview, txtPreview });

        pnlMiddle.Controls.AddRange(new Control[] { lblChains, lstChains, pnlPreview });

        // Bottom Panel - Butonlar
        pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = Color.FromArgb(240, 240, 240),
            Padding = new Padding(10)
        };

        btnSelect = new Button
        {
            Text = "✓ Seç ve Düzenle",
            Location = new Point(10, 10),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.OK,
            Cursor = Cursors.Hand
        };
        btnSelect.FlatAppearance.BorderSize = 0;
        btnSelect.Click += BtnSelect_Click;

        btnDelete = new Button
        {
            Text = "🗑️ Sil",
            Location = new Point(170, 10),
            Size = new Size(100, 40),
            Font = new Font("Segoe UI", 10F),
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Click += BtnDelete_Click;

        btnCancel = new Button
        {
            Text = "İptal",
            Location = new Point(680, 10),
            Size = new Size(100, 40),
            Font = new Font("Segoe UI", 10F),
            BackColor = Color.FromArgb(108, 117, 125),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;

        pnlBottom.Controls.AddRange(new Control[] { btnSelect, btnDelete, btnCancel });

        // Form'a ekle
        Controls.Add(pnlMiddle);
        Controls.Add(pnlBottom);
        Controls.Add(pnlTop);
    }

    private void LoadChains()
    {
        try
        {
            _chains = _database.GetAll();
            _filteredChains = _chains.ToList();
            UpdateList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Görev zincirleri yüklenirken hata: {ex.Message}",
                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateList()
    {
        lstChains.Items.Clear();

        foreach (var chain in _filteredChains)
        {
            string displayText = $"{chain.Name} ({chain.Steps.Count} adım) - {chain.CreatedDate:dd.MM.yyyy HH:mm}";
            if (chain.IsLooped)
            {
                displayText = "🔄 " + displayText;
            }
            lstChains.Items.Add(displayText);
        }

        if (lstChains.Items.Count > 0)
        {
            lstChains.SelectedIndex = 0;
        }
        else
        {
            txtPreview.Clear();
            btnSelect.Enabled = false;
            btnDelete.Enabled = false;
        }
    }

    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        string searchText = txtSearch.Text.ToLower();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            _filteredChains = _chains.ToList();
        }
        else
        {
            _filteredChains = _chains
                .Where(c => c.Name.ToLower().Contains(searchText) ||
                           c.Description.ToLower().Contains(searchText))
                .ToList();
        }

        UpdateList();
    }

    private void LstChains_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstChains.SelectedIndex >= 0 && lstChains.SelectedIndex < _filteredChains.Count)
        {
            var chain = _filteredChains[lstChains.SelectedIndex];
            ShowPreview(chain);
            btnSelect.Enabled = true;
            btnDelete.Enabled = true;
        }
        else
        {
            txtPreview.Clear();
            btnSelect.Enabled = false;
            btnDelete.Enabled = false;
        }
    }

    private void ShowPreview(TaskChain chain)
    {
        var preview = $"Görev Adı: {chain.Name}\n";
        preview += $"Açıklama: {chain.Description}\n";
        preview += $"Oluşturulma: {chain.CreatedDate:dd.MM.yyyy HH:mm:ss}\n";

        if (chain.LastModifiedDate.HasValue)
        {
            preview += $"Son Değişiklik: {chain.LastModifiedDate:dd.MM.yyyy HH:mm:ss}\n";
        }

        if (chain.IsLooped)
        {
            preview += $"\n🔄 DÖNGÜSEL GÖREV\n";
            preview += $"Döngü Başlangıcı: Adım {chain.LoopStartIndex + 1}\n";
            preview += $"Döngü Sonu: Adım {chain.LoopEndIndex + 1}\n";
            preview += $"Maksimum Döngü: {chain.MaxLoopCount}\n";
        }

        preview += $"\n======== ADIMLAR ({chain.Steps.Count}) ========\n\n";

        int stepNum = 1;
        foreach (var step in chain.Steps)
        {
            preview += $"Adım {stepNum}: {step.StepType}\n";

            if (!string.IsNullOrEmpty(step.Description))
            {
                preview += $"  Açıklama: {step.Description}\n";
            }

            switch (step.StepType)
            {
                case StepType.TargetSelection:
                    if (step.Target != null)
                    {
                        if (!string.IsNullOrEmpty(step.Target.ProgramPath))
                        {
                            preview += $"  Program: {step.Target.ProgramPath}\n";
                        }
                        if (!string.IsNullOrEmpty(step.Target.WindowTitle))
                        {
                            preview += $"  Pencere: {step.Target.WindowTitle}\n";
                        }
                        if (step.Target.IsDesktop)
                        {
                            preview += $"  Hedef: Masaüstü\n";
                        }
                    }
                    break;

                case StepType.UIElementAction:
                    if (step.UIElement != null)
                    {
                        preview += $"  Element: {step.UIElement.Name ?? "İsimsiz"}\n";
                        preview += $"  Eylem: {step.Action}\n";

                        if (!string.IsNullOrEmpty(step.TextToType))
                        {
                            preview += $"  Yazılacak: {step.TextToType}\n";
                        }

                        if (!string.IsNullOrEmpty(step.KeysToPress))
                        {
                            preview += $"  Tuşlar: {step.KeysToPress}\n";
                        }
                    }
                    break;

                case StepType.ConditionalBranch:
                    if (step.Condition != null)
                    {
                        if (!string.IsNullOrEmpty(step.Condition.PageIdentifier))
                        {
                            preview += $"  Sayfa: {step.Condition.PageIdentifier}\n";
                        }
                        if (step.Condition.Conditions?.Count > 0)
                        {
                            preview += $"  Koşul Sayısı: {step.Condition.Conditions.Count}\n";
                        }
                    }
                    break;

                case StepType.LoopOrEnd:
                    preview += $"  Döngü veya Bitiş Kontrolü\n";
                    break;
            }

            preview += "\n";
            stepNum++;
        }

        txtPreview.Text = preview;
    }

    private void LstChains_DoubleClick(object? sender, EventArgs e)
    {
        BtnSelect_Click(sender, e);
    }

    private void BtnSelect_Click(object? sender, EventArgs e)
    {
        if (lstChains.SelectedIndex >= 0 && lstChains.SelectedIndex < _filteredChains.Count)
        {
            SelectedChain = _filteredChains[lstChains.SelectedIndex];
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (lstChains.SelectedIndex >= 0 && lstChains.SelectedIndex < _filteredChains.Count)
        {
            var chain = _filteredChains[lstChains.SelectedIndex];

            var result = MessageBox.Show(
                $"'{chain.Name}' görev zincirini silmek istediğinize emin misiniz?\n\n" +
                $"Bu işlem geri alınamaz!",
                "Silme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _database.Delete(chain.Name);
                    LoadChains();

                    MessageBox.Show("Görev zinciri başarıyla silindi.",
                        "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Silme hatası: {ex.Message}",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}