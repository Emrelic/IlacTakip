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
        // Labels
        lblTitle = new Label();
        lblCurrentStep = new Label();
        lblChainName = new Label();
        lblStepType = new Label();

        // Buttons
        btnTopmost = new Button();

        // TextBoxes
        txtChainName = new TextBox();
        txtLog = new TextBox();

        // ComboBox
        cmbStepType = new ComboBox();

        // GroupBox for Tip 1 - Target Selection
        grpTargetSelection = new GroupBox();
        lblTargetInfo = new Label();
        txtProgramPath = new TextBox();
        btnBrowse = new Button();
        btnSelectWindow = new Button();
        btnDesktop = new Button();

        // GroupBox for Tip 2 - UI Element Action
        grpUIElementAction = new GroupBox();
        lblUIElementInfo = new Label();
        btnPickElement = new Button();
        btnAnalyzeStructure = new Button();
        txtElementProperties = new TextBox();
        lblActionType = new Label();
        cmbActionType = new ComboBox();
        txtKeysToPress = new TextBox();
        lblKeysToPress = new Label();

        // GroupBox for Strategy Testing
        grpStrategyTest = new GroupBox();
        lblStrategyInfo = new Label();
        btnTestAllStrategies = new Button();
        lstStrategies = new ListBox();
        lblSelectedStrategy = new Label();
        lblTestResult = new Label();

        // Buttons
        btnSaveStep = new Button();
        btnTestStep = new Button();
        btnNextStep = new Button();
        btnSaveChain = new Button();
        btnClose = new Button();

        SuspendLayout();

        //
        // lblTitle
        //
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.Location = new Point(10, 10);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(220, 21);
        lblTitle.Text = "Görev Zinciri Kaydedici";

        //
        // lblCurrentStep
        //
        lblCurrentStep.AutoSize = true;
        lblCurrentStep.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblCurrentStep.Location = new Point(380, 13);
        lblCurrentStep.Name = "lblCurrentStep";
        lblCurrentStep.Size = new Size(90, 17);
        lblCurrentStep.Text = "Adım: 1";

        //
        // btnTopmost
        //
        btnTopmost.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnTopmost.Location = new Point(740, 8);
        btnTopmost.Name = "btnTopmost";
        btnTopmost.Size = new Size(100, 28);
        btnTopmost.TabIndex = 99;
        btnTopmost.Text = "📌 En Üstte";
        btnTopmost.UseVisualStyleBackColor = true;
        btnTopmost.Click += btnTopmost_Click;

        //
        // lblChainName
        //
        lblChainName.AutoSize = true;
        lblChainName.Font = new Font("Segoe UI", 9.5F);
        lblChainName.Location = new Point(10, 45);
        lblChainName.Name = "lblChainName";
        lblChainName.Size = new Size(70, 17);
        lblChainName.Text = "Zincir Adı:";

        //
        // txtChainName
        //
        txtChainName.Font = new Font("Segoe UI", 9.5F);
        txtChainName.Location = new Point(85, 42);
        txtChainName.Name = "txtChainName";
        txtChainName.Size = new Size(270, 25);
        txtChainName.PlaceholderText = "Görev zinciri adını girin...";

        //
        // lblStepType
        //
        lblStepType.AutoSize = true;
        lblStepType.Font = new Font("Segoe UI", 9.5F);
        lblStepType.Location = new Point(10, 76);
        lblStepType.Name = "lblStepType";
        lblStepType.Size = new Size(70, 17);
        lblStepType.Text = "Görev Tipi:";

        //
        // cmbStepType
        //
        cmbStepType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbStepType.Font = new Font("Segoe UI", 9.5F);
        cmbStepType.FormattingEnabled = true;
        cmbStepType.Items.AddRange(new object[] {
            "Tip 1: Hedef Program/Pencere Seçimi",
            "Tip 2: UI Element Tıklama/Tuşlama",
            "Tip 3: Sayfa Durum Kontrolü (Koşullu Dallanma)",
            "Tip 4: Döngü veya Bitiş Koşulu"
        });
        cmbStepType.Location = new Point(85, 73);
        cmbStepType.Name = "cmbStepType";
        cmbStepType.Size = new Size(370, 25);
        cmbStepType.SelectedIndex = 0;
        cmbStepType.SelectedIndexChanged += cmbStepType_SelectedIndexChanged;

        //
        // grpTargetSelection
        //
        grpTargetSelection.Font = new Font("Segoe UI", 9.5F);
        grpTargetSelection.Location = new Point(10, 108);
        grpTargetSelection.Name = "grpTargetSelection";
        grpTargetSelection.Size = new Size(540, 138);
        grpTargetSelection.Text = "0) Hedef Pencere Seçimi";
        grpTargetSelection.Controls.Add(lblTargetInfo);
        grpTargetSelection.Controls.Add(txtProgramPath);
        grpTargetSelection.Controls.Add(btnBrowse);
        grpTargetSelection.Controls.Add(btnSelectWindow);
        grpTargetSelection.Controls.Add(btnDesktop);

        //
        // lblTargetInfo
        //
        lblTargetInfo.AutoSize = true;
        lblTargetInfo.Font = new Font("Segoe UI", 9F);
        lblTargetInfo.Location = new Point(10, 22);
        lblTargetInfo.Name = "lblTargetInfo";
        lblTargetInfo.Size = new Size(300, 15);
        lblTargetInfo.Text = "Hedef program veya pencereyi seçin:";

        //
        // txtProgramPath
        //
        txtProgramPath.Font = new Font("Segoe UI", 9F);
        txtProgramPath.Location = new Point(10, 42);
        txtProgramPath.Name = "txtProgramPath";
        txtProgramPath.ReadOnly = true;
        txtProgramPath.Size = new Size(520, 23);
        txtProgramPath.PlaceholderText = "Seçilen hedef burada görünecek...";

        //
        // btnBrowse
        //
        btnBrowse.Font = new Font("Segoe UI", 9F);
        btnBrowse.Location = new Point(10, 75);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(165, 35);
        btnBrowse.Text = "📁 Gözat (Program Seç)";
        btnBrowse.UseVisualStyleBackColor = true;
        btnBrowse.Click += btnBrowse_Click;

        //
        // btnSelectWindow
        //
        btnSelectWindow.Font = new Font("Segoe UI", 9F);
        btnSelectWindow.Location = new Point(185, 75);
        btnSelectWindow.Name = "btnSelectWindow";
        btnSelectWindow.Size = new Size(175, 35);
        btnSelectWindow.Text = "🖱️ Sayfa Seç (Aktif Pencere)";
        btnSelectWindow.UseVisualStyleBackColor = true;
        btnSelectWindow.Click += btnSelectWindow_Click;

        //
        // btnDesktop
        //
        btnDesktop.Font = new Font("Segoe UI", 9F);
        btnDesktop.Location = new Point(370, 75);
        btnDesktop.Name = "btnDesktop";
        btnDesktop.Size = new Size(160, 35);
        btnDesktop.Text = "🖥️ Masaüstü";
        btnDesktop.UseVisualStyleBackColor = true;
        btnDesktop.Click += btnDesktop_Click;

        //
        // grpUIElementAction
        //
        grpUIElementAction.Font = new Font("Segoe UI", 9.5F);
        grpUIElementAction.Location = new Point(10, 108);
        grpUIElementAction.Name = "grpUIElementAction";
        grpUIElementAction.Size = new Size(540, 138);
        grpUIElementAction.Text = "2) UI Element Tıklama/Tuşlama";
        grpUIElementAction.Visible = false;
        grpUIElementAction.Controls.Add(lblUIElementInfo);
        grpUIElementAction.Controls.Add(btnPickElement);
        grpUIElementAction.Controls.Add(btnAnalyzeStructure);
        grpUIElementAction.Controls.Add(txtElementProperties);
        grpUIElementAction.Controls.Add(lblActionType);
        grpUIElementAction.Controls.Add(cmbActionType);
        grpUIElementAction.Controls.Add(lblKeysToPress);
        grpUIElementAction.Controls.Add(txtKeysToPress);

        //
        // lblUIElementInfo
        //
        lblUIElementInfo.AutoSize = true;
        lblUIElementInfo.Font = new Font("Segoe UI", 9F);
        lblUIElementInfo.Location = new Point(10, 22);
        lblUIElementInfo.Name = "lblUIElementInfo";
        lblUIElementInfo.Size = new Size(300, 15);
        lblUIElementInfo.Text = "Mouse ile tıklayarak UI elementini seçin:";

        //
        // btnPickElement
        //
        btnPickElement.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnPickElement.Location = new Point(10, 42);
        btnPickElement.Name = "btnPickElement";
        btnPickElement.Size = new Size(135, 35);
        btnPickElement.Text = "🎯 Element Seç";
        btnPickElement.UseVisualStyleBackColor = true;
        btnPickElement.Click += btnPickElement_Click;

        //
        // btnAnalyzeStructure
        //
        btnAnalyzeStructure.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnAnalyzeStructure.Location = new Point(10, 83);
        btnAnalyzeStructure.Name = "btnAnalyzeStructure";
        btnAnalyzeStructure.Size = new Size(135, 35);
        btnAnalyzeStructure.Text = "🔍 Yapı Analizi";
        btnAnalyzeStructure.UseVisualStyleBackColor = true;
        btnAnalyzeStructure.Click += btnAnalyzeStructure_Click;

        //
        // txtElementProperties
        //
        txtElementProperties.Font = new Font("Consolas", 8F);
        txtElementProperties.Location = new Point(155, 42);
        txtElementProperties.Multiline = true;
        txtElementProperties.Name = "txtElementProperties";
        txtElementProperties.ReadOnly = true;
        txtElementProperties.ScrollBars = ScrollBars.Vertical;
        txtElementProperties.Size = new Size(375, 52);
        txtElementProperties.PlaceholderText = "Seçilen elementin özellikleri burada görünecek...";

        //
        // lblActionType
        //
        lblActionType.AutoSize = true;
        lblActionType.Font = new Font("Segoe UI", 9F);
        lblActionType.Location = new Point(155, 100);
        lblActionType.Name = "lblActionType";
        lblActionType.Size = new Size(85, 15);
        lblActionType.Text = "Yapılacak İşlem:";

        //
        // cmbActionType
        //
        cmbActionType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbActionType.Font = new Font("Segoe UI", 9F);
        cmbActionType.FormattingEnabled = true;
        cmbActionType.Items.AddRange(new object[] {
            "Sol Tık",
            "Sağ Tık",
            "Çift Tık",
            "Mouse Tekerlek",
            "Klavye Tuşları",
            "Metin Yaz"
        });
        cmbActionType.Location = new Point(250, 97);
        cmbActionType.Name = "cmbActionType";
        cmbActionType.Size = new Size(130, 23);
        cmbActionType.SelectedIndex = 0;
        cmbActionType.SelectedIndexChanged += cmbActionType_SelectedIndexChanged;

        //
        // lblKeysToPress
        //
        lblKeysToPress.AutoSize = true;
        lblKeysToPress.Font = new Font("Segoe UI", 9F);
        lblKeysToPress.Location = new Point(385, 100);
        lblKeysToPress.Name = "lblKeysToPress";
        lblKeysToPress.Size = new Size(75, 15);
        lblKeysToPress.Text = "Tuşlar/Metin:";
        lblKeysToPress.Visible = false;

        //
        // txtKeysToPress
        //
        txtKeysToPress.Font = new Font("Segoe UI", 9F);
        txtKeysToPress.Location = new Point(460, 97);
        txtKeysToPress.Name = "txtKeysToPress";
        txtKeysToPress.Size = new Size(70, 23);
        txtKeysToPress.PlaceholderText = "{ENTER}";
        txtKeysToPress.Visible = false;

        //
        // grpStrategyTest
        //
        grpStrategyTest.Font = new Font("Segoe UI", 9.5F);
        grpStrategyTest.Location = new Point(560, 108);
        grpStrategyTest.Name = "grpStrategyTest";
        grpStrategyTest.Size = new Size(310, 250);
        grpStrategyTest.Text = "Element Bulma Stratejileri";
        grpStrategyTest.Visible = false;
        grpStrategyTest.Controls.Add(lblStrategyInfo);
        grpStrategyTest.Controls.Add(btnTestAllStrategies);
        grpStrategyTest.Controls.Add(lstStrategies);
        grpStrategyTest.Controls.Add(lblSelectedStrategy);
        grpStrategyTest.Controls.Add(lblTestResult);

        //
        // lblStrategyInfo
        //
        lblStrategyInfo.AutoSize = true;
        lblStrategyInfo.Font = new Font("Segoe UI", 9F);
        lblStrategyInfo.Location = new Point(10, 22);
        lblStrategyInfo.Name = "lblStrategyInfo";
        lblStrategyInfo.Size = new Size(250, 15);
        lblStrategyInfo.Text = "Element için olası tıklama stratejileri:";

        //
        // btnTestAllStrategies
        //
        btnTestAllStrategies.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnTestAllStrategies.Location = new Point(10, 42);
        btnTestAllStrategies.Name = "btnTestAllStrategies";
        btnTestAllStrategies.Size = new Size(290, 32);
        btnTestAllStrategies.Text = "🧪 Tüm Stratejileri Test Et";
        btnTestAllStrategies.UseVisualStyleBackColor = true;
        btnTestAllStrategies.Click += btnTestAllStrategies_Click;

        //
        // lstStrategies
        //
        lstStrategies.Font = new Font("Consolas", 8F);
        lstStrategies.FormattingEnabled = true;
        lstStrategies.ItemHeight = 13;
        lstStrategies.Location = new Point(10, 80);
        lstStrategies.Name = "lstStrategies";
        lstStrategies.Size = new Size(290, 130);
        lstStrategies.SelectionMode = SelectionMode.One;
        lstStrategies.SelectedIndexChanged += lstStrategies_SelectedIndexChanged;

        //
        // lblSelectedStrategy
        //
        lblSelectedStrategy.AutoSize = true;
        lblSelectedStrategy.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblSelectedStrategy.Location = new Point(10, 215);
        lblSelectedStrategy.Name = "lblSelectedStrategy";
        lblSelectedStrategy.Size = new Size(180, 13);
        lblSelectedStrategy.Text = "Seçili Strateji: Henüz seçilmedi";
        lblSelectedStrategy.ForeColor = Color.Blue;

        //
        // lblTestResult
        //
        lblTestResult.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblTestResult.Location = new Point(10, 232);
        lblTestResult.Name = "lblTestResult";
        lblTestResult.Size = new Size(290, 18);
        lblTestResult.Text = "";
        lblTestResult.TextAlign = ContentAlignment.MiddleLeft;
        lblTestResult.ForeColor = Color.Black;

        //
        // txtLog
        //
        txtLog.Font = new Font("Consolas", 8F);
        txtLog.Location = new Point(10, 368);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(860, 90);
        txtLog.Text = "Görev kaydedici hazır...\n";

        //
        // btnSaveStep
        //
        btnSaveStep.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSaveStep.Location = new Point(10, 468);
        btnSaveStep.Name = "btnSaveStep";
        btnSaveStep.Size = new Size(130, 35);
        btnSaveStep.Text = "💾 Adımı Kaydet";
        btnSaveStep.UseVisualStyleBackColor = true;
        btnSaveStep.Click += btnSaveStep_Click;

        //
        // btnTestStep
        //
        btnTestStep.Font = new Font("Segoe UI", 9F);
        btnTestStep.Location = new Point(150, 468);
        btnTestStep.Name = "btnTestStep";
        btnTestStep.Size = new Size(120, 35);
        btnTestStep.Text = "🧪 Test Adım";
        btnTestStep.UseVisualStyleBackColor = true;
        btnTestStep.Click += btnTestStep_Click;

        //
        // btnNextStep
        //
        btnNextStep.Font = new Font("Segoe UI", 9F);
        btnNextStep.Location = new Point(280, 468);
        btnNextStep.Name = "btnNextStep";
        btnNextStep.Size = new Size(130, 35);
        btnNextStep.Text = "➡️ Sonraki Adım";
        btnNextStep.UseVisualStyleBackColor = true;
        btnNextStep.Click += btnNextStep_Click;

        //
        // btnSaveChain
        //
        btnSaveChain.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSaveChain.Location = new Point(420, 468);
        btnSaveChain.Name = "btnSaveChain";
        btnSaveChain.Size = new Size(140, 35);
        btnSaveChain.Text = "💾 Zinciri Kaydet";
        btnSaveChain.UseVisualStyleBackColor = true;
        btnSaveChain.Click += btnSaveChain_Click;

        //
        // btnClose
        //
        btnClose.Font = new Font("Segoe UI", 9F);
        btnClose.Location = new Point(810, 468);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(60, 35);
        btnClose.Text = "Kapat";
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += btnClose_Click;

        //
        // TaskChainRecorderForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(880, 520);
        Controls.Add(btnTopmost);
        Controls.Add(lblTitle);
        Controls.Add(lblCurrentStep);
        Controls.Add(lblChainName);
        Controls.Add(txtChainName);
        Controls.Add(lblStepType);
        Controls.Add(cmbStepType);
        Controls.Add(grpTargetSelection);
        Controls.Add(grpUIElementAction);
        Controls.Add(grpStrategyTest);
        Controls.Add(txtLog);
        Controls.Add(btnSaveStep);
        Controls.Add(btnTestStep);
        Controls.Add(btnNextStep);
        Controls.Add(btnSaveChain);
        Controls.Add(btnClose);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(880, 520);
        Name = "TaskChainRecorderForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Görev Zinciri Kaydedici - Element Strateji Test";
        grpTargetSelection.ResumeLayout(false);
        grpTargetSelection.PerformLayout();
        grpUIElementAction.ResumeLayout(false);
        grpUIElementAction.PerformLayout();
        grpStrategyTest.ResumeLayout(false);
        grpStrategyTest.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitle;
    private Label lblCurrentStep;
    private Label lblChainName;
    private Label lblStepType;
    private Button btnTopmost;
    private TextBox txtChainName;
    private TextBox txtLog;
    private ComboBox cmbStepType;
    private GroupBox grpTargetSelection;
    private Label lblTargetInfo;
    private TextBox txtProgramPath;
    private Button btnBrowse;
    private Button btnSelectWindow;
    private Button btnDesktop;
    private GroupBox grpUIElementAction;
    private Label lblUIElementInfo;
    private Button btnPickElement;
    private Button btnAnalyzeStructure;
    private TextBox txtElementProperties;
    private Label lblActionType;
    private ComboBox cmbActionType;
    private TextBox txtKeysToPress;
    private Label lblKeysToPress;
    private GroupBox grpStrategyTest;
    private Label lblStrategyInfo;
    private Button btnTestAllStrategies;
    private ListBox lstStrategies;
    private Label lblSelectedStrategy;
    private Label lblTestResult;
    private Button btnSaveStep;
    private Button btnTestStep;
    private Button btnNextStep;
    private Button btnSaveChain;
    private Button btnClose;
}
