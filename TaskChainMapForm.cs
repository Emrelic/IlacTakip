using System.Drawing.Drawing2D;

namespace MedulaOtomasyon;

/// <summary>
/// Görev zincirini görsel harita olarak gösteren form
/// Görevler boncuk gibi kutular şeklinde, aralarında bağlantılarla gösterilir
/// </summary>
public partial class TaskChainMapForm : Form
{
    private TaskChain _chain;
    private Panel _mapPanel;
    private Dictionary<int, TaskBox> _taskBoxes = new Dictionary<int, TaskBox>();
    private int _selectedStepIndex = -1;
    private Point _lastMousePos;
    private bool _isDragging = false;

    public event EventHandler<int>? InsertStepRequested;
    public event EventHandler<int>? EditStepRequested;
    public event EventHandler<int>? DeleteStepRequested;

    public TaskChainMapForm(TaskChain chain)
    {
        _chain = chain;
        InitializeComponent();
        BuildMap();
    }

    private void InitializeComponent()
    {
        this.Text = "📍 Görev Haritası";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(30, 30, 30);

        // Toolbar
        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(10)
        };

        var lblTitle = new Label
        {
            Text = $"🗺️ {_chain.Name ?? "Görev Haritası"}",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(10, 15)
        };

        var btnZoomIn = new Button
        {
            Text = "🔍+",
            Location = new Point(400, 10),
            Size = new Size(60, 40),
            BackColor = Color.FromArgb(60, 60, 65),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnZoomIn.Click += (s, e) => ZoomIn();

        var btnZoomOut = new Button
        {
            Text = "🔍-",
            Location = new Point(470, 10),
            Size = new Size(60, 40),
            BackColor = Color.FromArgb(60, 60, 65),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnZoomOut.Click += (s, e) => ZoomOut();

        var btnRefresh = new Button
        {
            Text = "🔄 Yenile",
            Location = new Point(540, 10),
            Size = new Size(100, 40),
            BackColor = Color.FromArgb(60, 60, 65),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnRefresh.Click += (s, e) => BuildMap();

        var btnClose = new Button
        {
            Text = "✖ Kapat",
            Location = new Point(1280, 10),
            Size = new Size(100, 40),
            BackColor = Color.FromArgb(192, 0, 0),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnClose.Click += (s, e) => Close();

        toolbar.Controls.AddRange(new Control[] { lblTitle, btnZoomIn, btnZoomOut, btnRefresh, btnClose });

        // Map Panel (scrollable)
        var scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.FromArgb(30, 30, 30)
        };

        _mapPanel = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(3000, 2000), // Büyük bir canvas
            BackColor = Color.FromArgb(30, 30, 30)
        };
        _mapPanel.Paint += MapPanel_Paint;
        _mapPanel.MouseDown += MapPanel_MouseDown;
        _mapPanel.MouseMove += MapPanel_MouseMove;
        _mapPanel.MouseUp += MapPanel_MouseUp;

        scrollPanel.Controls.Add(_mapPanel);

        // Legend (açıklama paneli)
        var legend = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(10)
        };

        var legendText = new Label
        {
            Text = "🟦 Normal Görev  |  🟩 Hedef Pencere  |  🟨 Koşullu Dallanma  |  🟧 Döngü Sonlandırma  |  🟥 Klavye Girdisi  |  ➡️ Akış  |  🔄 Döngü  |  ↗️ Dallanma",
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(10, 10),
            Font = new Font("Segoe UI", 10)
        };

        var legendText2 = new Label
        {
            Text = "💡 İpucu: Görev kutusuna sağ tıklayarak araya/sonra görev ekleyebilir, düzenleyebilir veya silebilirsiniz.",
            ForeColor = Color.LightGray,
            AutoSize = true,
            Location = new Point(10, 35),
            Font = new Font("Segoe UI", 9, FontStyle.Italic)
        };

        legend.Controls.AddRange(new Control[] { legendText, legendText2 });

        this.Controls.Add(scrollPanel);
        this.Controls.Add(toolbar);
        this.Controls.Add(legend);
    }

    private float _zoomLevel = 1.0f;

    private void ZoomIn()
    {
        _zoomLevel = Math.Min(_zoomLevel + 0.1f, 2.0f);
        BuildMap();
    }

    private void ZoomOut()
    {
        _zoomLevel = Math.Max(_zoomLevel - 0.1f, 0.5f);
        BuildMap();
    }

    private void BuildMap()
    {
        _taskBoxes.Clear();
        _mapPanel.Controls.Clear();

        if (_chain.Steps == null || _chain.Steps.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "Henüz görev eklenmemiş.",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 14),
                Location = new Point(50, 50),
                AutoSize = true
            };
            _mapPanel.Controls.Add(emptyLabel);
            return;
        }

        int x = 50;
        int y = 50;
        int boxWidth = (int)(250 * _zoomLevel);
        int boxHeight = (int)(120 * _zoomLevel);
        int horizontalSpacing = (int)(100 * _zoomLevel);
        int verticalSpacing = (int)(150 * _zoomLevel);

        // Döngü ve dallanma bilgilerini analiz et
        var loopInfo = AnalyzeLoops();
        var branchInfo = AnalyzeBranches();

        for (int i = 0; i < _chain.Steps.Count; i++)
        {
            var step = _chain.Steps[i];

            // Y pozisyonunu dallanmalara göre ayarla
            if (branchInfo.ContainsKey(i))
            {
                y += verticalSpacing;
            }

            var taskBox = CreateTaskBox(step, i, x, y, boxWidth, boxHeight);
            taskBox.IsLoopPoint = loopInfo.Contains(i);
            taskBox.IsBranchPoint = branchInfo.ContainsKey(i);

            _taskBoxes[i] = taskBox;
            _mapPanel.Controls.Add(taskBox);

            // Sonraki kutu için X pozisyonu
            x += boxWidth + horizontalSpacing;

            // Eğer çok sağa gittiyse alt satıra geç
            if (x > 2500)
            {
                x = 50;
                y += boxHeight + verticalSpacing;
            }
        }

        _mapPanel.Invalidate(); // Çizimi tetikle
    }

    private HashSet<int> AnalyzeLoops()
    {
        var loopPoints = new HashSet<int>();

        for (int i = 0; i < _chain.Steps.Count; i++)
        {
            var step = _chain.Steps[i];

            // Tip 3 görevleri kontrol et (Koşullu Dallanma)
            if (step.StepType == StepType.ConditionalBranch && step.Condition != null)
            {
                // Dallar arasında geri dönen var mı?
                foreach (var branch in step.Condition.Branches)
                {
                    if (!string.IsNullOrEmpty(branch.TargetStepId))
                    {
                        // Hedef adım numarasını parse et
                        var targetStepNum = ParseStepNumber(branch.TargetStepId);
                        if (targetStepNum.HasValue && targetStepNum.Value <= i)
                        {
                            // Geri dönen bir dal var - döngü
                            loopPoints.Add(targetStepNum.Value);
                            loopPoints.Add(i);
                        }
                    }
                }

                // Döngü sonlandırma modu aktif mi?
                if (step.Condition.IsLoopTerminationMode)
                {
                    loopPoints.Add(i);
                }
            }

            // Tip 4 görevleri kontrol et (Döngü veya Bitiş)
            if (step.StepType == StepType.LoopOrEnd && step.IsLoopEnd && step.LoopBackToStep.HasValue)
            {
                loopPoints.Add(step.LoopBackToStep.Value - 1); // 0-based index
                loopPoints.Add(i);
            }
        }

        return loopPoints;
    }

    private Dictionary<int, List<string>> AnalyzeBranches()
    {
        var branches = new Dictionary<int, List<string>>();

        for (int i = 0; i < _chain.Steps.Count; i++)
        {
            var step = _chain.Steps[i];

            if (step.StepType == StepType.ConditionalBranch && step.Condition != null)
            {
                var branchTargets = new List<string>();
                foreach (var branch in step.Condition.Branches)
                {
                    if (!string.IsNullOrEmpty(branch.TargetStepId))
                    {
                        branchTargets.Add(branch.TargetStepId);
                    }
                }

                if (branchTargets.Count > 0)
                {
                    branches[i] = branchTargets;
                }
            }
        }

        return branches;
    }

    private int? ParseStepNumber(string stepId)
    {
        // "5A", "10B" gibi formatları parse et
        if (string.IsNullOrEmpty(stepId)) return null;

        var numPart = new string(stepId.TakeWhile(char.IsDigit).ToArray());
        if (int.TryParse(numPart, out int num))
        {
            return num - 1; // 0-based index
        }

        return null;
    }

    private TaskBox CreateTaskBox(TaskStep step, int index, int x, int y, int width, int height)
    {
        var box = new TaskBox
        {
            StepIndex = index,
            Step = step,
            Location = new Point(x, y),
            Size = new Size(width, height)
        };

        // Sağ tık menü
        var contextMenu = new ContextMenuStrip();

        var menuInsertBefore = new ToolStripMenuItem("➕ Önüne Görev Ekle");
        menuInsertBefore.Click += (s, e) => InsertStepRequested?.Invoke(this, index);

        var menuInsertAfter = new ToolStripMenuItem("➕ Sonrasına Görev Ekle");
        menuInsertAfter.Click += (s, e) => InsertStepRequested?.Invoke(this, index + 1);

        var menuEdit = new ToolStripMenuItem("✏️ Düzenle");
        menuEdit.Click += (s, e) => EditStepRequested?.Invoke(this, index);

        var menuDelete = new ToolStripMenuItem("🗑️ Sil");
        menuDelete.Click += (s, e) => DeleteStepRequested?.Invoke(this, index);

        contextMenu.Items.AddRange(new ToolStripItem[] { menuInsertBefore, menuInsertAfter, new ToolStripSeparator(), menuEdit, menuDelete });

        box.ContextMenuStrip = contextMenu;

        return box;
    }

    private void MapPanel_Paint(object? sender, PaintEventArgs e)
    {
        if (_taskBoxes.Count == 0) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Bağlantıları çiz
        for (int i = 0; i < _chain.Steps.Count - 1; i++)
        {
            if (!_taskBoxes.ContainsKey(i) || !_taskBoxes.ContainsKey(i + 1))
                continue;

            var box1 = _taskBoxes[i];
            var box2 = _taskBoxes[i + 1];

            // Normal akış oku
            DrawArrow(g,
                new Point(box1.Right, box1.Top + box1.Height / 2),
                new Point(box2.Left, box2.Top + box2.Height / 2),
                Color.LightBlue, 3);
        }

        // Dallanma bağlantılarını çiz
        var branchInfo = AnalyzeBranches();
        foreach (var kvp in branchInfo)
        {
            var sourceIndex = kvp.Key;
            if (!_taskBoxes.ContainsKey(sourceIndex)) continue;

            var sourceBox = _taskBoxes[sourceIndex];

            foreach (var targetId in kvp.Value)
            {
                var targetIndex = ParseStepNumber(targetId);
                if (targetIndex.HasValue && _taskBoxes.ContainsKey(targetIndex.Value))
                {
                    var targetBox = _taskBoxes[targetIndex.Value];

                    // Dallanma oku (sarı veya turuncu)
                    var isLoopBack = targetIndex.Value <= sourceIndex;
                    var color = isLoopBack ? Color.Orange : Color.Yellow;

                    DrawCurvedArrow(g,
                        new Point(sourceBox.Right, sourceBox.Bottom - 10),
                        new Point(targetBox.Left, targetBox.Top + 10),
                        color, 2, isLoopBack);
                }
            }
        }
    }

    private void DrawArrow(Graphics g, Point start, Point end, Color color, int thickness)
    {
        using var pen = new Pen(color, thickness);
        pen.EndCap = LineCap.ArrowAnchor;
        pen.CustomEndCap = new AdjustableArrowCap(5, 5);

        g.DrawLine(pen, start, end);
    }

    private void DrawCurvedArrow(Graphics g, Point start, Point end, Color color, int thickness, bool isLoopBack)
    {
        using var pen = new Pen(color, thickness);
        pen.EndCap = LineCap.ArrowAnchor;
        pen.CustomEndCap = new AdjustableArrowCap(5, 5);

        if (isLoopBack)
        {
            // Döngü için yukarı doğru eğri
            var controlPoint1 = new Point(start.X + 50, start.Y - 100);
            var controlPoint2 = new Point(end.X - 50, end.Y - 100);

            g.DrawBezier(pen, start, controlPoint1, controlPoint2, end);
        }
        else
        {
            // Normal dal için aşağı doğru eğri
            var controlPoint1 = new Point(start.X + 50, start.Y + 50);
            var controlPoint2 = new Point(end.X - 50, end.Y - 50);

            g.DrawBezier(pen, start, controlPoint1, controlPoint2, end);
        }
    }

    private void MapPanel_MouseDown(object? sender, MouseEventArgs e)
    {
        _lastMousePos = e.Location;
        _isDragging = true;
    }

    private void MapPanel_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDragging && e.Button == MouseButtons.Left)
        {
            // Pan görünümü
            var dx = e.X - _lastMousePos.X;
            var dy = e.Y - _lastMousePos.Y;

            _lastMousePos = e.Location;
        }
    }

    private void MapPanel_MouseUp(object? sender, MouseEventArgs e)
    {
        _isDragging = false;
    }
}

/// <summary>
/// Görev kutusunu temsil eden özel panel
/// </summary>
public class TaskBox : Panel
{
    public int StepIndex { get; set; }
    public TaskStep? Step { get; set; }
    public bool IsLoopPoint { get; set; }
    public bool IsBranchPoint { get; set; }

    public TaskBox()
    {
        this.BorderStyle = BorderStyle.None;
        this.BackColor = Color.Transparent;
        this.Paint += TaskBox_Paint;
        this.DoubleBuffered = true;
    }

    private void TaskBox_Paint(object? sender, PaintEventArgs e)
    {
        if (Step == null) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Renk seçimi - görev tipine göre
        Color boxColor = GetBoxColor();
        Color borderColor = IsLoopPoint ? Color.Orange : (IsBranchPoint ? Color.Yellow : Color.LightBlue);

        // Kutu çiz (rounded rectangle)
        using (var brush = new SolidBrush(boxColor))
        using (var pen = new Pen(borderColor, 3))
        {
            var rect = new Rectangle(5, 5, Width - 10, Height - 10);
            var path = GetRoundedRect(rect, 10);

            g.FillPath(brush, path);
            g.DrawPath(pen, path);
        }

        // İkon ve başlık
        var icon = GetStepIcon();
        using (var iconFont = new Font("Segoe UI Emoji", 16, FontStyle.Bold))
        using (var titleFont = new Font("Segoe UI", 9, FontStyle.Bold))
        using (var textFont = new Font("Segoe UI", 8))
        using (var brush = new SolidBrush(Color.White))
        {
            // İkon
            g.DrawString(icon, iconFont, brush, 15, 10);

            // Adım numarası
            g.DrawString($"Adım {Step.StepNumber}", titleFont, brush, 50, 12);

            // Görev tipi
            var stepType = GetStepTypeText();
            g.DrawString(stepType, textFont, new SolidBrush(Color.LightGray), 15, 40);

            // Hedef sayfa
            var targetPage = GetTargetPageText();
            if (!string.IsNullOrEmpty(targetPage))
            {
                g.DrawString($"📄 {TruncateText(targetPage, 28)}", textFont, brush, 15, 60);
            }

            // Hedef UI / İşlem
            var targetUI = GetTargetUIText();
            if (!string.IsNullOrEmpty(targetUI))
            {
                g.DrawString($"🎯 {TruncateText(targetUI, 28)}", textFont, brush, 15, 80);
            }

            // Özel işaretler
            if (IsLoopPoint)
            {
                g.DrawString("🔄", iconFont, new SolidBrush(Color.Orange), Width - 40, Height - 35);
            }
            if (IsBranchPoint)
            {
                g.DrawString("↗️", iconFont, new SolidBrush(Color.Yellow), Width - 65, Height - 35);
            }
        }
    }

    private Color GetBoxColor()
    {
        return Step?.StepType switch
        {
            StepType.TargetSelection => Color.FromArgb(60, 179, 113), // Yeşil - Hedef pencere
            StepType.ConditionalBranch when Step.Condition?.IsLoopTerminationMode == true
                => Color.FromArgb(255, 140, 0), // Turuncu - Döngü sonlandırma
            StepType.ConditionalBranch => Color.FromArgb(218, 165, 32), // Altın sarısı - Dallanma
            StepType.UIElementAction when Step.Action == ActionType.KeyPress || Step.Action == ActionType.TypeText
                => Color.FromArgb(220, 20, 60), // Kırmızı - Klavye
            StepType.UIElementAction => Color.FromArgb(70, 130, 180), // Mavi - UI etkileşimi
            StepType.LoopOrEnd => Color.FromArgb(153, 50, 204), // Mor - Döngü/bitiş
            _ => Color.FromArgb(75, 75, 85) // Varsayılan gri
        };
    }

    private string GetStepIcon()
    {
        return Step?.StepType switch
        {
            StepType.TargetSelection => "🪟",
            StepType.ConditionalBranch => Step.Condition?.IsLoopTerminationMode == true ? "🔄" : "↗️",
            StepType.UIElementAction when Step.Action == ActionType.KeyPress || Step.Action == ActionType.TypeText => "⌨️",
            StepType.UIElementAction => "🖱️",
            StepType.LoopOrEnd => "🔄",
            _ => "📋"
        };
    }

    private string GetStepTypeText()
    {
        return Step?.StepType switch
        {
            StepType.TargetSelection => "Hedef Pencere Seçimi",
            StepType.ConditionalBranch when Step.Condition?.IsLoopTerminationMode == true
                => "Döngü Sonlandırma",
            StepType.ConditionalBranch => "Koşullu Dallanma",
            StepType.UIElementAction when Step.Action == ActionType.KeyPress || Step.Action == ActionType.TypeText
                => "Klavye Girdisi",
            StepType.UIElementAction => "UI Etkileşimi",
            StepType.LoopOrEnd => "Döngü/Bitiş",
            _ => "Bilinmeyen Tip"
        };
    }

    private string GetTargetPageText()
    {
        if (Step?.Target != null)
        {
            return Step.Target.WindowTitle ?? Step.Target.ProgramPath ?? "";
        }

        if (Step?.Condition != null)
        {
            return Step.Condition.PageIdentifier ?? "Koşul Kontrolü";
        }

        return "";
    }

    private string GetTargetUIText()
    {
        if (Step?.UIElement != null)
        {
            var action = Step.Action switch
            {
                ActionType.LeftClick => "Tıkla",
                ActionType.DoubleClick => "Çift Tıkla",
                ActionType.RightClick => "Sağ Tıkla",
                ActionType.TypeText => $"Yaz: {Step.TextToType}",
                ActionType.KeyPress => $"Tuş: {Step.KeysToPress}",
                ActionType.MouseWheel => $"Kaydır: {Step.MouseWheelDelta}",
                _ => "Etkileşim"
            };

            var elementName = Step.UIElement.Name ?? Step.UIElement.AutomationId ?? "Element";
            return $"{action} → {elementName}";
        }

        if (Step?.Condition != null && Step.Condition.Conditions.Count > 0)
        {
            var firstCondition = Step.Condition.Conditions[0];
            return $"Koşul: {firstCondition.Element?.Name ?? "?"}.{firstCondition.PropertyName}";
        }

        if (Step?.StepType == StepType.UIElementAction &&
            (Step.Action == ActionType.KeyPress || Step.Action == ActionType.TypeText))
        {
            return $"Tuşlar: {Step.KeysToPress}";
        }

        return "";
    }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "";
        if (text.Length <= maxLength) return text;
        return text.Substring(0, maxLength - 3) + "...";
    }

    private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();

        path.AddArc(bounds.Left, bounds.Top, radius, radius, 180, 90);
        path.AddArc(bounds.Right - radius, bounds.Top, radius, radius, 270, 90);
        path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();

        return path;
    }
}
