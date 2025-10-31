namespace MedulaOtomasyon;

partial class TaskChainRecorderForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // Header
        pnlHeader = new Panel();
        lblTitle = new Label();
        lblCurrentStep = new Label();

        // Task Chain Viewer (sağ panel)
        pnlTaskChainViewer = new Panel();
        lblTaskChainTitle = new Label();
        txtTaskChainSteps = new TextBox();
        btnDeleteLastStep = new Button();
        btnDeleteStep = new Button();
        btnEditStep = new Button();
        btnInsertStep = new Button();
        btnDeleteAllSteps = new Button();

        // Main content - TabControl
        tabControl = new TabControl();
        tabBasicInfo = new TabPage();
        tabTargetSelection = new TabPage();
        tabUIElement = new TabPage();
        tabSmartElement = new TabPage();
        tabStrategies = new TabPage();

        // Tab 1: Basic Info
        lblChainName = new Label();
        txtChainName = new TextBox();
        lblStepType = new Label();
        cmbStepType = new ComboBox();

        // Tab 2: Target Selection
        lblTargetInfo = new Label();
        txtProgramPath = new TextBox();
        btnBrowse = new Button();
        btnSelectWindow = new Button();
        btnDesktop = new Button();

        // Tab 3: UI Element
        lblUIElementInfo = new Label();
        btnPickElement = new Button();
        btnAnalyzeStructure = new Button();
        txtElementProperties = new TextBox();
        lblActionType = new Label();
        cmbActionType = new ComboBox();
        lblKeysToPress = new Label();
        txtKeysToPress = new TextBox();
        lblScrollAmount = new Label();
        numScrollAmount = new NumericUpDown();
        lblDoubleClickDelay = new Label();
        numDoubleClickDelay = new NumericUpDown();

        // Tab 4: Smart Element
        lblSmartInfo = new Label();
        btnSmartPick = new Button();
        btnTestSelectedSmartStrategy = new Button();
        btnTestSmartStrategies = new Button();
        lstSmartStrategies = new ListBox();
        txtSmartElementProperties = new TextBox();
        lblSmartSelectedStrategy = new Label();
        lblSmartTestResult = new Label();

        // Tab 5: Strategies
        lblStrategyInfo = new Label();
        btnTestSelectedStrategy = new Button();
        btnTestAllStrategies = new Button();
        lstStrategies = new ListBox();
        lblSelectedStrategy = new Label();
        lblTestResult = new Label();

        // Log Panel
        pnlLog = new Panel();
        lblLog = new Label();
        txtLog = new TextBox();

        // Footer
        pnlFooter = new Panel();
        btnSaveStep = new Button();
        btnTestStep = new Button();
        btnNextStep = new Button();
        btnSaveChain = new Button();
        btnLoadChain = new Button();
        btnPlayChain = new Button();
        btnMakeLooped = new Button();
        btnClose = new Button();

        pnlHeader.SuspendLayout();
        pnlTaskChainViewer.SuspendLayout();
        tabControl.SuspendLayout();
        tabBasicInfo.SuspendLayout();
        tabTargetSelection.SuspendLayout();
        tabUIElement.SuspendLayout();
        tabSmartElement.SuspendLayout();
        tabStrategies.SuspendLayout();
        pnlLog.SuspendLayout();
        pnlFooter.SuspendLayout();
        SuspendLayout();

        // ==========================================
        // HEADER PANEL
        // ==========================================
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 90;
        pnlHeader.BackColor = Color.FromArgb(45, 45, 48);
        pnlHeader.Padding = new Padding(10, 5, 10, 5);
        pnlHeader.Controls.Add(lblCurrentStep);
        pnlHeader.Controls.Add(lblTitle);

        // lblTitle
        lblTitle.Text = "📌 Görev Kaydedici";
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(10, 10);
        lblTitle.Size = new Size(460, 25);
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;

        // lblCurrentStep
        lblCurrentStep.Text = "Adım: 1";
        lblCurrentStep.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblCurrentStep.ForeColor = Color.FromArgb(100, 200, 255);
        lblCurrentStep.Location = new Point(10, 45);
        lblCurrentStep.Size = new Size(460, 40);
        lblCurrentStep.TextAlign = ContentAlignment.MiddleCenter;

        // ==========================================
        // TAB CONTROL
        // ==========================================
        tabControl.Dock = DockStyle.Fill;
        tabControl.Font = new Font("Segoe UI", 9.5F);
        tabControl.Padding = new Point(10, 8);
        tabControl.TabPages.Add(tabBasicInfo);
        tabControl.TabPages.Add(tabTargetSelection);
        tabControl.TabPages.Add(tabUIElement);
        tabControl.TabPages.Add(tabSmartElement);
        tabControl.TabPages.Add(tabStrategies);

        // ==========================================
        // TAB 1: Basic Info
        // ==========================================
        tabBasicInfo.Text = "📋 Temel Bilgiler";
        tabBasicInfo.BackColor = Color.White;
        tabBasicInfo.Padding = new Padding(20);
        tabBasicInfo.AutoScroll = true;
        tabBasicInfo.Controls.Add(cmbStepType);
        tabBasicInfo.Controls.Add(lblStepType);
        tabBasicInfo.Controls.Add(txtChainName);
        tabBasicInfo.Controls.Add(lblChainName);

        // lblChainName
        lblChainName.Text = "Zincir Adı";
        lblChainName.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblChainName.Location = new Point(15, 20);
        lblChainName.Size = new Size(430, 22);

        // txtChainName
        txtChainName.Font = new Font("Segoe UI", 10.5F);
        txtChainName.Location = new Point(15, 45);
        txtChainName.Size = new Size(430, 32);
        txtChainName.PlaceholderText = "Görev zinciri adını girin...";

        // lblStepType
        lblStepType.Text = "Görev Tipi: Lütfen Seçiniz";
        lblStepType.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblStepType.ForeColor = Color.Gray;
        lblStepType.Location = new Point(15, 90);
        lblStepType.Size = new Size(430, 22);

        // cmbStepType
        cmbStepType.Font = new Font("Segoe UI", 10.5F);
        cmbStepType.Location = new Point(15, 115);
        cmbStepType.Size = new Size(430, 32);
        cmbStepType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbStepType.Items.AddRange(new object[] {
            "Tip 1: Hedef Program/Pencere Seçimi",
            "Tip 2: UI Element Tıklama/Tuşlama",
            "Tip 3: Sayfa Durum Kontrolü (Koşullu Dallanma)",
            "Tip 4: Döngü veya Bitiş Koşulu"
        });
        cmbStepType.SelectedIndex = -1; // Hiçbiri seçili değil
        cmbStepType.SelectedIndexChanged += cmbStepType_SelectedIndexChanged;

        // ==========================================
        // TAB 2: Target Selection
        // ==========================================
        tabTargetSelection.Text = "🎯 Hedef Seçimi";
        tabTargetSelection.BackColor = Color.White;
        tabTargetSelection.Padding = new Padding(20);
        tabTargetSelection.AutoScroll = true;
        tabTargetSelection.Controls.Add(btnDesktop);
        tabTargetSelection.Controls.Add(btnSelectWindow);
        tabTargetSelection.Controls.Add(btnBrowse);
        tabTargetSelection.Controls.Add(txtProgramPath);
        tabTargetSelection.Controls.Add(lblTargetInfo);

        // lblTargetInfo
        lblTargetInfo.Text = "Hedef program veya pencereyi seçin:";
        lblTargetInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblTargetInfo.Location = new Point(15, 20);
        lblTargetInfo.Size = new Size(430, 22);

        // txtProgramPath
        txtProgramPath.Font = new Font("Segoe UI", 10F);
        txtProgramPath.Location = new Point(15, 45);
        txtProgramPath.Size = new Size(430, 30);
        txtProgramPath.PlaceholderText = "Seçilen hedef burada görünecek...";
        txtProgramPath.ReadOnly = true;
        txtProgramPath.BackColor = Color.FromArgb(245, 245, 245);

        // btnBrowse
        btnBrowse.Text = "📁 Program";
        btnBrowse.Font = new Font("Segoe UI", 10F);
        btnBrowse.Location = new Point(15, 90);
        btnBrowse.Size = new Size(135, 40);
        btnBrowse.BackColor = Color.FromArgb(0, 120, 212);
        btnBrowse.ForeColor = Color.White;
        btnBrowse.FlatStyle = FlatStyle.Flat;
        btnBrowse.FlatAppearance.BorderSize = 0;
        btnBrowse.Cursor = Cursors.Hand;
        btnBrowse.Click += btnBrowse_Click;

        // btnSelectWindow
        btnSelectWindow.Text = "🖱️ Pencere";
        btnSelectWindow.Font = new Font("Segoe UI", 10F);
        btnSelectWindow.Location = new Point(160, 90);
        btnSelectWindow.Size = new Size(135, 40);
        btnSelectWindow.BackColor = Color.FromArgb(0, 120, 212);
        btnSelectWindow.ForeColor = Color.White;
        btnSelectWindow.FlatStyle = FlatStyle.Flat;
        btnSelectWindow.FlatAppearance.BorderSize = 0;
        btnSelectWindow.Cursor = Cursors.Hand;
        btnSelectWindow.Click += btnSelectWindow_Click;

        // btnDesktop
        btnDesktop.Text = "🖥️ Masaüstü";
        btnDesktop.Font = new Font("Segoe UI", 10F);
        btnDesktop.Location = new Point(305, 90);
        btnDesktop.Size = new Size(140, 40);
        btnDesktop.BackColor = Color.FromArgb(0, 120, 212);
        btnDesktop.ForeColor = Color.White;
        btnDesktop.FlatStyle = FlatStyle.Flat;
        btnDesktop.FlatAppearance.BorderSize = 0;
        btnDesktop.Cursor = Cursors.Hand;
        btnDesktop.Click += btnDesktop_Click;

        // ==========================================
        // TAB 3: UI Element
        // ==========================================
        tabUIElement.Text = "🎯 UI Element";
        tabUIElement.BackColor = Color.White;
        tabUIElement.Padding = new Padding(20);
        tabUIElement.AutoScroll = true;
        tabUIElement.Controls.Add(numDoubleClickDelay);
        tabUIElement.Controls.Add(lblDoubleClickDelay);
        tabUIElement.Controls.Add(numScrollAmount);
        tabUIElement.Controls.Add(lblScrollAmount);
        tabUIElement.Controls.Add(txtKeysToPress);
        tabUIElement.Controls.Add(lblKeysToPress);
        tabUIElement.Controls.Add(cmbActionType);
        tabUIElement.Controls.Add(lblActionType);
        tabUIElement.Controls.Add(txtElementProperties);
        tabUIElement.Controls.Add(btnAnalyzeStructure);
        tabUIElement.Controls.Add(btnPickElement);
        tabUIElement.Controls.Add(lblUIElementInfo);

        // lblUIElementInfo
        lblUIElementInfo.Text = "Mouse ile element seçin:";
        lblUIElementInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblUIElementInfo.Location = new Point(15, 20);
        lblUIElementInfo.Size = new Size(430, 22);

        // btnPickElement
        btnPickElement.Text = "🎯 Element Seç";
        btnPickElement.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnPickElement.Location = new Point(15, 45);
        btnPickElement.Size = new Size(205, 40);
        btnPickElement.BackColor = Color.FromArgb(16, 124, 16);
        btnPickElement.ForeColor = Color.White;
        btnPickElement.FlatStyle = FlatStyle.Flat;
        btnPickElement.FlatAppearance.BorderSize = 0;
        btnPickElement.Cursor = Cursors.Hand;
        btnPickElement.Click += btnPickElement_Click;

        // btnAnalyzeStructure
        btnAnalyzeStructure.Text = "🔍 Yapı Analizi";
        btnAnalyzeStructure.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnAnalyzeStructure.Location = new Point(230, 45);
        btnAnalyzeStructure.Size = new Size(215, 40);
        btnAnalyzeStructure.BackColor = Color.FromArgb(106, 90, 205);
        btnAnalyzeStructure.ForeColor = Color.White;
        btnAnalyzeStructure.FlatStyle = FlatStyle.Flat;
        btnAnalyzeStructure.FlatAppearance.BorderSize = 0;
        btnAnalyzeStructure.Cursor = Cursors.Hand;
        btnAnalyzeStructure.Click += btnAnalyzeStructure_Click;

        // txtElementProperties
        txtElementProperties.Font = new Font("Consolas", 9F);
        txtElementProperties.Location = new Point(15, 95);
        txtElementProperties.Size = new Size(430, 120);
        txtElementProperties.Multiline = true;
        txtElementProperties.ReadOnly = true;
        txtElementProperties.ScrollBars = ScrollBars.Vertical;
        txtElementProperties.BackColor = Color.FromArgb(245, 245, 245);
        txtElementProperties.Text = "Seçilen elementin özellikleri burada görünecek...";

        // lblActionType
        lblActionType.Text = "Yapılacak İşlem";
        lblActionType.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblActionType.Location = new Point(15, 230);
        lblActionType.Size = new Size(200, 22);

        // cmbActionType
        cmbActionType.Font = new Font("Segoe UI", 10F);
        cmbActionType.Location = new Point(15, 255);
        cmbActionType.Size = new Size(200, 30);
        cmbActionType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbActionType.Items.AddRange(new object[] {
            "Sol Tık",
            "Sağ Tık",
            "Çift Tık",
            "Mouse Tekerlek",
            "Klavye Tuşları",
            "Metin Yaz"
        });
        cmbActionType.SelectedIndex = 0;
        cmbActionType.SelectedIndexChanged += cmbActionType_SelectedIndexChanged;

        // lblKeysToPress
        lblKeysToPress.Text = "Tuşlar/Metin:";
        lblKeysToPress.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblKeysToPress.Location = new Point(15, 295);
        lblKeysToPress.Size = new Size(200, 22);
        lblKeysToPress.Visible = false;

        // txtKeysToPress
        txtKeysToPress.Font = new Font("Segoe UI", 10F);
        txtKeysToPress.Location = new Point(15, 320);
        txtKeysToPress.Size = new Size(430, 30);
        txtKeysToPress.PlaceholderText = "{ENTER}";
        txtKeysToPress.Visible = false;

        // lblScrollAmount
        lblScrollAmount.Text = "Scroll Miktarı (adım sayısı):";
        lblScrollAmount.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblScrollAmount.Location = new Point(15, 295);
        lblScrollAmount.Size = new Size(250, 22);
        lblScrollAmount.Visible = false;

        // numScrollAmount
        numScrollAmount.Font = new Font("Segoe UI", 10F);
        numScrollAmount.Location = new Point(15, 320);
        numScrollAmount.Size = new Size(120, 30);
        numScrollAmount.Minimum = -10;
        numScrollAmount.Maximum = 10;
        numScrollAmount.Value = 1;
        numScrollAmount.Visible = false;

        // lblDoubleClickDelay
        lblDoubleClickDelay.Text = "Tıklama Aralığı (ms):";
        lblDoubleClickDelay.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblDoubleClickDelay.Location = new Point(15, 295);
        lblDoubleClickDelay.Size = new Size(200, 22);
        lblDoubleClickDelay.Visible = false;

        // numDoubleClickDelay
        numDoubleClickDelay.Font = new Font("Segoe UI", 10F);
        numDoubleClickDelay.Location = new Point(15, 320);
        numDoubleClickDelay.Size = new Size(120, 30);
        numDoubleClickDelay.Minimum = 10;
        numDoubleClickDelay.Maximum = 2000;
        numDoubleClickDelay.Value = 100;
        numDoubleClickDelay.Visible = false;

        // ==========================================
        // TAB 4: Smart Element
        // ==========================================
        tabSmartElement.Text = "🧠 Akıllı Element";
        tabSmartElement.BackColor = Color.White;
        tabSmartElement.Padding = new Padding(20);
        tabSmartElement.AutoScroll = true;
        tabSmartElement.Controls.Add(lblSmartTestResult);
        tabSmartElement.Controls.Add(lblSmartSelectedStrategy);
        tabSmartElement.Controls.Add(txtSmartElementProperties);
        tabSmartElement.Controls.Add(lstSmartStrategies);
        tabSmartElement.Controls.Add(btnTestSelectedSmartStrategy);
        tabSmartElement.Controls.Add(btnTestSmartStrategies);
        tabSmartElement.Controls.Add(btnSmartPick);
        tabSmartElement.Controls.Add(lblSmartInfo);

        // lblSmartInfo
        lblSmartInfo.Text = "Tablo satırlarını kaydetmek için (Koordinat kullanmaz)";
        lblSmartInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblSmartInfo.ForeColor = Color.FromArgb(16, 124, 16);
        lblSmartInfo.Location = new Point(15, 20);
        lblSmartInfo.Size = new Size(430, 22);

        // btnSmartPick
        btnSmartPick.Text = "🧠 Akıllı Seç";
        btnSmartPick.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSmartPick.Location = new Point(15, 50);
        btnSmartPick.Size = new Size(145, 40);
        btnSmartPick.BackColor = Color.FromArgb(16, 124, 16);
        btnSmartPick.ForeColor = Color.White;
        btnSmartPick.FlatStyle = FlatStyle.Flat;
        btnSmartPick.FlatAppearance.BorderSize = 0;
        btnSmartPick.Cursor = Cursors.Hand;
        btnSmartPick.Click += btnSmartPick_Click;

        // btnTestSelectedSmartStrategy
        btnTestSelectedSmartStrategy.Text = "🔍 Seçili Testi";
        btnTestSelectedSmartStrategy.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnTestSelectedSmartStrategy.Location = new Point(170, 50);
        btnTestSelectedSmartStrategy.Size = new Size(135, 40);
        btnTestSelectedSmartStrategy.BackColor = Color.FromArgb(70, 130, 180);
        btnTestSelectedSmartStrategy.ForeColor = Color.White;
        btnTestSelectedSmartStrategy.FlatStyle = FlatStyle.Flat;
        btnTestSelectedSmartStrategy.FlatAppearance.BorderSize = 0;
        btnTestSelectedSmartStrategy.Enabled = false;
        btnTestSelectedSmartStrategy.Cursor = Cursors.Hand;
        btnTestSelectedSmartStrategy.Click += btnTestSelectedSmartStrategy_Click;

        // btnTestSmartStrategies
        btnTestSmartStrategies.Text = "🧪 Tümünü Test";
        btnTestSmartStrategies.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnTestSmartStrategies.Location = new Point(315, 50);
        btnTestSmartStrategies.Size = new Size(130, 40);
        btnTestSmartStrategies.BackColor = Color.FromArgb(218, 165, 32);
        btnTestSmartStrategies.ForeColor = Color.White;
        btnTestSmartStrategies.FlatStyle = FlatStyle.Flat;
        btnTestSmartStrategies.FlatAppearance.BorderSize = 0;
        btnTestSmartStrategies.Enabled = false;
        btnTestSmartStrategies.Cursor = Cursors.Hand;
        btnTestSmartStrategies.Click += btnTestSmartStrategies_Click;

        // lstSmartStrategies
        lstSmartStrategies.Font = new Font("Consolas", 9F);
        lstSmartStrategies.Location = new Point(15, 100);
        lstSmartStrategies.Size = new Size(430, 90);
        lstSmartStrategies.SelectionMode = SelectionMode.One;
        lstSmartStrategies.SelectedIndexChanged += lstSmartStrategies_SelectedIndexChanged;

        // txtSmartElementProperties
        txtSmartElementProperties.Font = new Font("Consolas", 9F);
        txtSmartElementProperties.Location = new Point(15, 200);
        txtSmartElementProperties.Size = new Size(430, 90);
        txtSmartElementProperties.Multiline = true;
        txtSmartElementProperties.ReadOnly = true;
        txtSmartElementProperties.ScrollBars = ScrollBars.Vertical;
        txtSmartElementProperties.BackColor = Color.FromArgb(245, 245, 245);
        txtSmartElementProperties.Text = "Element bilgileri burada görünecek...";

        // lblSmartSelectedStrategy
        lblSmartSelectedStrategy.Text = "Seçili: -";
        lblSmartSelectedStrategy.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblSmartSelectedStrategy.ForeColor = Color.Blue;
        lblSmartSelectedStrategy.Location = new Point(15, 300);
        lblSmartSelectedStrategy.Size = new Size(430, 22);

        // lblSmartTestResult
        lblSmartTestResult.Text = "";
        lblSmartTestResult.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblSmartTestResult.ForeColor = Color.Black;
        lblSmartTestResult.Location = new Point(15, 325);
        lblSmartTestResult.Size = new Size(430, 22);

        // ==========================================
        // TAB 5: Strategies
        // ==========================================
        tabStrategies.Text = "🧪 Stratejiler";
        tabStrategies.BackColor = Color.White;
        tabStrategies.Padding = new Padding(20);
        tabStrategies.AutoScroll = true;
        tabStrategies.Controls.Add(lblTestResult);
        tabStrategies.Controls.Add(lblSelectedStrategy);
        tabStrategies.Controls.Add(lstStrategies);
        tabStrategies.Controls.Add(btnTestSelectedStrategy);
        tabStrategies.Controls.Add(btnTestAllStrategies);
        tabStrategies.Controls.Add(lblStrategyInfo);

        // lblStrategyInfo
        lblStrategyInfo.Text = "Element için olası tıklama stratejileri:";
        lblStrategyInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblStrategyInfo.Location = new Point(15, 20);
        lblStrategyInfo.Size = new Size(430, 22);

        // btnTestSelectedStrategy
        btnTestSelectedStrategy.Text = "🔍 Seçili Stratejiyi Test Et";
        btnTestSelectedStrategy.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnTestSelectedStrategy.Location = new Point(15, 50);
        btnTestSelectedStrategy.Size = new Size(205, 40);
        btnTestSelectedStrategy.BackColor = Color.FromArgb(70, 130, 180);
        btnTestSelectedStrategy.ForeColor = Color.White;
        btnTestSelectedStrategy.FlatStyle = FlatStyle.Flat;
        btnTestSelectedStrategy.FlatAppearance.BorderSize = 0;
        btnTestSelectedStrategy.Cursor = Cursors.Hand;
        btnTestSelectedStrategy.Click += btnTestSelectedStrategy_Click;

        // btnTestAllStrategies
        btnTestAllStrategies.Text = "🧪 Tüm Stratejileri Test Et";
        btnTestAllStrategies.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnTestAllStrategies.Location = new Point(230, 50);
        btnTestAllStrategies.Size = new Size(215, 40);
        btnTestAllStrategies.BackColor = Color.FromArgb(218, 165, 32);
        btnTestAllStrategies.ForeColor = Color.White;
        btnTestAllStrategies.FlatStyle = FlatStyle.Flat;
        btnTestAllStrategies.FlatAppearance.BorderSize = 0;
        btnTestAllStrategies.Cursor = Cursors.Hand;
        btnTestAllStrategies.Click += btnTestAllStrategies_Click;

        // lstStrategies
        lstStrategies.Font = new Font("Consolas", 9F);
        lstStrategies.Location = new Point(15, 100);
        lstStrategies.Size = new Size(430, 150);
        lstStrategies.SelectionMode = SelectionMode.One;
        lstStrategies.SelectedIndexChanged += lstStrategies_SelectedIndexChanged;

        // lblSelectedStrategy
        lblSelectedStrategy.Text = "Seçili Strateji: Henüz seçilmedi";
        lblSelectedStrategy.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblSelectedStrategy.ForeColor = Color.Blue;
        lblSelectedStrategy.Location = new Point(15, 260);
        lblSelectedStrategy.Size = new Size(430, 22);

        // lblTestResult
        lblTestResult.Text = "";
        lblTestResult.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblTestResult.ForeColor = Color.Black;
        lblTestResult.Location = new Point(15, 285);
        lblTestResult.Size = new Size(430, 40);

        // ==========================================
        // LOG PANEL
        // ==========================================
        pnlLog.Dock = DockStyle.Bottom;
        pnlLog.Height = 120;
        pnlLog.BackColor = Color.FromArgb(30, 30, 30);
        pnlLog.Padding = new Padding(10, 5, 10, 5);
        pnlLog.Controls.Add(txtLog);
        pnlLog.Controls.Add(lblLog);

        // lblLog
        lblLog.Text = "📋 Log";
        lblLog.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblLog.ForeColor = Color.White;
        lblLog.Dock = DockStyle.Top;
        lblLog.Height = 24;

        // txtLog
        txtLog.Font = new Font("Consolas", 8.5F);
        txtLog.Dock = DockStyle.Fill;
        txtLog.Multiline = true;
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.BackColor = Color.FromArgb(40, 40, 40);
        txtLog.ForeColor = Color.FromArgb(220, 220, 220);
        txtLog.BorderStyle = BorderStyle.None;
        txtLog.Text = "Görev kaydedici hazır...\n";

        // ==========================================
        // FOOTER PANEL
        // ==========================================
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Height = 150;
        pnlFooter.BackColor = Color.FromArgb(240, 240, 240);
        pnlFooter.Padding = new Padding(10, 10, 10, 10);
        pnlFooter.Controls.Add(btnClose);
        pnlFooter.Controls.Add(btnLoadChain);
        pnlFooter.Controls.Add(btnPlayChain);
        pnlFooter.Controls.Add(btnMakeLooped); // Döngüsel yap butonu
        pnlFooter.Controls.Add(btnSaveChain);
        pnlFooter.Controls.Add(btnNextStep);
        pnlFooter.Controls.Add(btnTestStep);
        pnlFooter.Controls.Add(btnSaveStep);

        // btnSaveStep
        btnSaveStep.Text = "💾 Kaydet";
        btnSaveStep.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSaveStep.Location = new Point(10, 10);
        btnSaveStep.Size = new Size(225, 38);
        btnSaveStep.BackColor = Color.FromArgb(0, 120, 212);
        btnSaveStep.ForeColor = Color.White;
        btnSaveStep.FlatStyle = FlatStyle.Flat;
        btnSaveStep.FlatAppearance.BorderSize = 0;
        btnSaveStep.Cursor = Cursors.Hand;
        btnSaveStep.Click += btnSaveStep_Click;

        // btnTestStep
        btnTestStep.Text = "🧪 Test";
        btnTestStep.Font = new Font("Segoe UI", 10F);
        btnTestStep.Location = new Point(245, 10);
        btnTestStep.Size = new Size(225, 38);
        btnTestStep.BackColor = Color.FromArgb(218, 165, 32);
        btnTestStep.ForeColor = Color.White;
        btnTestStep.FlatStyle = FlatStyle.Flat;
        btnTestStep.FlatAppearance.BorderSize = 0;
        btnTestStep.Cursor = Cursors.Hand;
        btnTestStep.Click += btnTestStep_Click;

        // btnNextStep
        btnNextStep.Text = "➡️ Sonraki";
        btnNextStep.Font = new Font("Segoe UI", 10F);
        btnNextStep.Location = new Point(10, 53);
        btnNextStep.Size = new Size(145, 38);
        btnNextStep.BackColor = Color.FromArgb(106, 90, 205);
        btnNextStep.ForeColor = Color.White;
        btnNextStep.FlatStyle = FlatStyle.Flat;
        btnNextStep.FlatAppearance.BorderSize = 0;
        btnNextStep.Cursor = Cursors.Hand;
        btnNextStep.Click += btnNextStep_Click;

        // btnSaveChain
        btnSaveChain.Text = "💾 Zinciri Kaydet";
        btnSaveChain.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSaveChain.Location = new Point(165, 53);
        btnSaveChain.Size = new Size(220, 38);
        btnSaveChain.BackColor = Color.FromArgb(16, 124, 16);
        btnSaveChain.ForeColor = Color.White;
        btnSaveChain.FlatStyle = FlatStyle.Flat;
        btnSaveChain.FlatAppearance.BorderSize = 0;
        btnSaveChain.Cursor = Cursors.Hand;
        btnSaveChain.Click += btnSaveChain_Click;

        // btnLoadChain
        btnLoadChain.Text = "📂 Düzenle";
        btnLoadChain.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnLoadChain.Location = new Point(10, 96);
        btnLoadChain.Size = new Size(100, 38);
        btnLoadChain.BackColor = Color.FromArgb(30, 144, 255);
        btnLoadChain.ForeColor = Color.White;
        btnLoadChain.FlatStyle = FlatStyle.Flat;
        btnLoadChain.FlatAppearance.BorderSize = 0;
        btnLoadChain.Cursor = Cursors.Hand;
        btnLoadChain.Click += btnLoadChain_Click;

        // btnPlayChain
        btnPlayChain.Text = "▶️ Oynat";
        btnPlayChain.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnPlayChain.Location = new Point(115, 96);
        btnPlayChain.Size = new Size(100, 38);
        btnPlayChain.BackColor = Color.FromArgb(40, 167, 69);
        btnPlayChain.ForeColor = Color.White;
        btnPlayChain.FlatStyle = FlatStyle.Flat;
        btnPlayChain.FlatAppearance.BorderSize = 0;
        btnPlayChain.Cursor = Cursors.Hand;
        btnPlayChain.Click += btnPlayChain_Click;

        // btnClose
        btnClose.Text = "❌";
        btnClose.Font = new Font("Segoe UI", 10F);
        btnClose.Location = new Point(400, 96);
        btnClose.Size = new Size(65, 38);
        btnClose.BackColor = Color.FromArgb(180, 180, 180);
        btnClose.ForeColor = Color.White;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Cursor = Cursors.Hand;
        btnClose.Click += btnClose_Click;

        // ==========================================
        // DÖNGÜSEL GÖREV KONTROLLERİ
        // ==========================================
        pnlLoopSettings = new Panel();
        chkIsLooped = new CheckBox();
        numLoopStartIndex = new NumericUpDown();
        numLoopEndIndex = new NumericUpDown();
        numMaxLoopCount = new NumericUpDown();
        lblLoopStart = new Label();
        lblLoopEnd = new Label();
        lblMaxLoopCount = new Label();

        // pnlLoopSettings
        pnlLoopSettings.Dock = DockStyle.Bottom;
        pnlLoopSettings.Height = 110;
        pnlLoopSettings.BackColor = Color.FromArgb(250, 250, 250);
        pnlLoopSettings.BorderStyle = BorderStyle.FixedSingle;
        pnlLoopSettings.Padding = new Padding(10);
        pnlLoopSettings.Controls.Add(lblMaxLoopCount);
        pnlLoopSettings.Controls.Add(numMaxLoopCount);
        pnlLoopSettings.Controls.Add(lblLoopEnd);
        pnlLoopSettings.Controls.Add(numLoopEndIndex);
        pnlLoopSettings.Controls.Add(lblLoopStart);
        pnlLoopSettings.Controls.Add(numLoopStartIndex);
        pnlLoopSettings.Controls.Add(chkIsLooped);
        // btnMakeLooped artık Footer panelinde olacak
        pnlLoopSettings.Visible = false; // Başlangıçta gizli

        // btnMakeLooped - Footer panelinde görünür
        btnMakeLooped.Text = "🔄 Döngüsel";
        btnMakeLooped.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnMakeLooped.Location = new Point(220, 96);  // btnPlayChain'in yanında
        btnMakeLooped.Size = new Size(170, 38);
        btnMakeLooped.BackColor = Color.FromArgb(255, 140, 0);
        btnMakeLooped.ForeColor = Color.White;
        btnMakeLooped.FlatStyle = FlatStyle.Flat;
        btnMakeLooped.FlatAppearance.BorderSize = 0;
        btnMakeLooped.Cursor = Cursors.Hand;
        btnMakeLooped.Click += btnMakeLooped_Click;

        // chkIsLooped
        chkIsLooped.Text = "Döngü Aktif";
        chkIsLooped.Font = new Font("Segoe UI", 10F);
        chkIsLooped.Location = new Point(200, 15);
        chkIsLooped.Size = new Size(100, 25);
        chkIsLooped.CheckedChanged += chkIsLooped_CheckedChanged;

        // lblLoopStart
        lblLoopStart.Text = "Döngü Başlangıcı:";
        lblLoopStart.Font = new Font("Segoe UI", 9F);
        lblLoopStart.Location = new Point(10, 45);
        lblLoopStart.Size = new Size(110, 25);
        lblLoopStart.TextAlign = ContentAlignment.MiddleLeft;

        // numLoopStartIndex
        ((System.ComponentModel.ISupportInitialize)(numLoopStartIndex)).BeginInit();
        numLoopStartIndex.Font = new Font("Segoe UI", 9F);
        numLoopStartIndex.Location = new Point(120, 45);
        numLoopStartIndex.Size = new Size(60, 25);
        numLoopStartIndex.Minimum = 1;
        numLoopStartIndex.Maximum = 100;
        numLoopStartIndex.Value = 5; // Varsayılan olarak 5. adıma döner
        ((System.ComponentModel.ISupportInitialize)(numLoopStartIndex)).EndInit();

        // lblLoopEnd
        lblLoopEnd.Text = "Döngü Sonu:";
        lblLoopEnd.Font = new Font("Segoe UI", 9F);
        lblLoopEnd.Location = new Point(200, 45);
        lblLoopEnd.Size = new Size(85, 25);
        lblLoopEnd.TextAlign = ContentAlignment.MiddleLeft;

        // numLoopEndIndex
        ((System.ComponentModel.ISupportInitialize)(numLoopEndIndex)).BeginInit();
        numLoopEndIndex.Font = new Font("Segoe UI", 9F);
        numLoopEndIndex.Location = new Point(285, 45);
        numLoopEndIndex.Size = new Size(60, 25);
        numLoopEndIndex.Minimum = 1;
        numLoopEndIndex.Maximum = 100;
        numLoopEndIndex.Value = 11; // Varsayılan olarak 11. adımda döner
        ((System.ComponentModel.ISupportInitialize)(numLoopEndIndex)).EndInit();

        // lblMaxLoopCount
        lblMaxLoopCount.Text = "Maks. Döngü:";
        lblMaxLoopCount.Font = new Font("Segoe UI", 9F);
        lblMaxLoopCount.Location = new Point(10, 75);
        lblMaxLoopCount.Size = new Size(85, 25);
        lblMaxLoopCount.TextAlign = ContentAlignment.MiddleLeft;

        // numMaxLoopCount
        ((System.ComponentModel.ISupportInitialize)(numMaxLoopCount)).BeginInit();
        numMaxLoopCount.Font = new Font("Segoe UI", 9F);
        numMaxLoopCount.Location = new Point(95, 75);
        numMaxLoopCount.Size = new Size(80, 25);
        numMaxLoopCount.Minimum = 1;
        numMaxLoopCount.Maximum = 10000;
        numMaxLoopCount.Value = 100; // Varsayılan olarak 100 döngü
        ((System.ComponentModel.ISupportInitialize)(numMaxLoopCount)).EndInit();

        // ==========================================
        // TASK CHAIN VIEWER PANEL (ALT PANEL)
        // ==========================================
        pnlTaskChainViewer.Dock = DockStyle.Bottom;
        pnlTaskChainViewer.Height = 230;
        pnlTaskChainViewer.BackColor = Color.White;
        pnlTaskChainViewer.BorderStyle = BorderStyle.FixedSingle;
        pnlTaskChainViewer.Padding = new Padding(5);
        pnlTaskChainViewer.Controls.Add(btnDeleteAllSteps);
        pnlTaskChainViewer.Controls.Add(btnInsertStep);
        pnlTaskChainViewer.Controls.Add(btnEditStep);
        pnlTaskChainViewer.Controls.Add(btnDeleteStep);
        pnlTaskChainViewer.Controls.Add(btnDeleteLastStep);
        pnlTaskChainViewer.Controls.Add(txtTaskChainSteps);
        pnlTaskChainViewer.Controls.Add(lblTaskChainTitle);

        // lblTaskChainTitle
        lblTaskChainTitle.Text = "Görev Zinciri Adımları";
        lblTaskChainTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblTaskChainTitle.ForeColor = Color.White;
        lblTaskChainTitle.BackColor = Color.FromArgb(70, 130, 180);
        lblTaskChainTitle.Dock = DockStyle.Top;
        lblTaskChainTitle.Height = 38;
        lblTaskChainTitle.TextAlign = ContentAlignment.MiddleCenter;

        // txtTaskChainSteps
        txtTaskChainSteps.Font = new Font("Consolas", 8.5F);
        txtTaskChainSteps.Multiline = true;
        txtTaskChainSteps.ReadOnly = true;
        txtTaskChainSteps.ScrollBars = ScrollBars.Vertical;
        txtTaskChainSteps.BackColor = Color.White;
        txtTaskChainSteps.ForeColor = Color.Black;
        txtTaskChainSteps.BorderStyle = BorderStyle.None;
        txtTaskChainSteps.Location = new Point(5, 43);
        txtTaskChainSteps.Size = new Size(460, 145);
        txtTaskChainSteps.TabStop = false;
        txtTaskChainSteps.Text = "Görev adımları kaydedildikçe burada görünecektir...";

        // btnDeleteLastStep
        btnDeleteLastStep.Text = "🗑️ Son";
        btnDeleteLastStep.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnDeleteLastStep.Location = new Point(5, 193);
        btnDeleteLastStep.Size = new Size(85, 34);
        btnDeleteLastStep.BackColor = Color.FromArgb(255, 140, 0);
        btnDeleteLastStep.ForeColor = Color.White;
        btnDeleteLastStep.FlatStyle = FlatStyle.Flat;
        btnDeleteLastStep.FlatAppearance.BorderSize = 0;
        btnDeleteLastStep.Cursor = Cursors.Hand;
        btnDeleteLastStep.Click += btnDeleteLastStep_Click;

        // btnDeleteStep
        btnDeleteStep.Text = "🗑️ Sil";
        btnDeleteStep.Font = new Font("Segoe UI", 9F);
        btnDeleteStep.Location = new Point(95, 193);
        btnDeleteStep.Size = new Size(85, 34);
        btnDeleteStep.BackColor = Color.FromArgb(220, 20, 60);
        btnDeleteStep.ForeColor = Color.White;
        btnDeleteStep.FlatStyle = FlatStyle.Flat;
        btnDeleteStep.FlatAppearance.BorderSize = 0;
        btnDeleteStep.Cursor = Cursors.Hand;
        btnDeleteStep.Click += btnDeleteStep_Click;

        // btnEditStep
        btnEditStep.Text = "✏️ Düzenle";
        btnEditStep.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnEditStep.Location = new Point(190, 193);
        btnEditStep.Size = new Size(85, 34);
        btnEditStep.BackColor = Color.FromArgb(0, 120, 212);
        btnEditStep.ForeColor = Color.White;
        btnEditStep.FlatStyle = FlatStyle.Flat;
        btnEditStep.FlatAppearance.BorderSize = 0;
        btnEditStep.Cursor = Cursors.Hand;
        btnEditStep.Click += btnEditStep_Click;

        // btnInsertStep
        btnInsertStep.Text = "➕ Araya";
        btnInsertStep.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnInsertStep.Location = new Point(285, 193);
        btnInsertStep.Size = new Size(85, 34);
        btnInsertStep.BackColor = Color.FromArgb(0, 150, 100);
        btnInsertStep.ForeColor = Color.White;
        btnInsertStep.FlatStyle = FlatStyle.Flat;
        btnInsertStep.FlatAppearance.BorderSize = 0;
        btnInsertStep.Cursor = Cursors.Hand;
        btnInsertStep.Click += btnInsertStep_Click;

        // btnDeleteAllSteps
        btnDeleteAllSteps.Text = "🗑️ Tümü";
        btnDeleteAllSteps.Font = new Font("Segoe UI", 9F);
        btnDeleteAllSteps.Location = new Point(380, 193);
        btnDeleteAllSteps.Size = new Size(85, 34);
        btnDeleteAllSteps.BackColor = Color.FromArgb(139, 0, 0);
        btnDeleteAllSteps.ForeColor = Color.White;
        btnDeleteAllSteps.FlatStyle = FlatStyle.Flat;
        btnDeleteAllSteps.FlatAppearance.BorderSize = 0;
        btnDeleteAllSteps.Cursor = Cursors.Hand;
        btnDeleteAllSteps.Click += btnDeleteAllSteps_Click;

        // ==========================================
        // FORM
        // ==========================================
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(480, 1070);
        Controls.Add(pnlTaskChainViewer);
        Controls.Add(pnlLoopSettings);
        Controls.Add(tabControl);
        Controls.Add(pnlLog);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);
        FormBorderStyle = FormBorderStyle.SizableToolWindow;
        MaximizeBox = false;
        MinimizeBox = true;
        MinimumSize = new Size(480, 600);
        Name = "TaskChainRecorderForm";
        StartPosition = FormStartPosition.Manual;
        Text = "Görev Kaydedici";

        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlTaskChainViewer.ResumeLayout(false);
        pnlTaskChainViewer.PerformLayout();
        tabControl.ResumeLayout(false);
        tabBasicInfo.ResumeLayout(false);
        tabTargetSelection.ResumeLayout(false);
        tabUIElement.ResumeLayout(false);
        tabSmartElement.ResumeLayout(false);
        tabStrategies.ResumeLayout(false);
        pnlLog.ResumeLayout(false);
        pnlLoopSettings.ResumeLayout(false);
        pnlFooter.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblCurrentStep;

    private TabControl tabControl;
    private TabPage tabBasicInfo;
    private TabPage tabTargetSelection;
    private TabPage tabUIElement;
    private TabPage tabSmartElement;
    private TabPage tabStrategies;

    private Label lblChainName;
    private TextBox txtChainName;
    private Label lblStepType;
    private ComboBox cmbStepType;

    private Label lblTargetInfo;
    private TextBox txtProgramPath;
    private Button btnBrowse;
    private Button btnSelectWindow;
    private Button btnDesktop;

    private Label lblUIElementInfo;
    private Button btnPickElement;
    private Button btnAnalyzeStructure;
    private TextBox txtElementProperties;
    private Label lblActionType;
    private ComboBox cmbActionType;
    private Label lblKeysToPress;
    private TextBox txtKeysToPress;
    private Label lblScrollAmount;
    private NumericUpDown numScrollAmount;
    private Label lblDoubleClickDelay;
    private NumericUpDown numDoubleClickDelay;

    private Label lblSmartInfo;
    private Button btnSmartPick;
    private Button btnTestSelectedSmartStrategy;
    private Button btnTestSmartStrategies;
    private ListBox lstSmartStrategies;
    private TextBox txtSmartElementProperties;
    private Label lblSmartSelectedStrategy;
    private Label lblSmartTestResult;

    private Label lblStrategyInfo;
    private Button btnTestSelectedStrategy;
    private Button btnTestAllStrategies;
    private ListBox lstStrategies;
    private Label lblSelectedStrategy;
    private Label lblTestResult;

    private Panel pnlLog;
    private Label lblLog;
    private TextBox txtLog;

    private Panel pnlFooter;
    private Button btnSaveStep;
    private Button btnTestStep;
    private Button btnNextStep;
    private Button btnSaveChain;
    private Button btnLoadChain;
    private Button btnPlayChain;
    private Button btnClose;

    // Döngüsel görev kontrolleri
    private Button btnMakeLooped;
    private CheckBox chkIsLooped;
    private NumericUpDown numLoopStartIndex;
    private NumericUpDown numLoopEndIndex;
    private NumericUpDown numMaxLoopCount;
    private Label lblLoopStart;
    private Label lblLoopEnd;
    private Label lblMaxLoopCount;
    private Panel pnlLoopSettings;

    private Panel pnlTaskChainViewer;
    private Label lblTaskChainTitle;
    private TextBox txtTaskChainSteps;
    private Button btnDeleteStep;
    private Button btnEditStep;
    private Button btnInsertStep;
    private Button btnDeleteAllSteps;
    private Button btnDeleteLastStep;
}
