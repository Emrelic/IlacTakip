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
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.Location = new Point(20, 20);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(250, 30);
        lblTitle.Text = "Görev Zinciri Kaydedici";

        //
        // lblCurrentStep
        //
        lblCurrentStep.AutoSize = true;
        lblCurrentStep.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblCurrentStep.Location = new Point(600, 25);
        lblCurrentStep.Name = "lblCurrentStep";
        lblCurrentStep.Size = new Size(100, 21);
        lblCurrentStep.Text = "Adım: 1";

        //
        // btnTopmost
        //
        btnTopmost.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnTopmost.Location = new Point(1060, 20);
        btnTopmost.Name = "btnTopmost";
        btnTopmost.Size = new Size(110, 30);
        btnTopmost.TabIndex = 99;
        btnTopmost.Text = "📌 En Üstte Tut";
        btnTopmost.UseVisualStyleBackColor = true;
        btnTopmost.Click += btnTopmost_Click;

        //
        // lblChainName
        //
        lblChainName.AutoSize = true;
        lblChainName.Font = new Font("Segoe UI", 10F);
        lblChainName.Location = new Point(20, 70);
        lblChainName.Name = "lblChainName";
        lblChainName.Size = new Size(80, 19);
        lblChainName.Text = "Zincir Adı:";

        //
        // txtChainName
        //
        txtChainName.Font = new Font("Segoe UI", 10F);
        txtChainName.Location = new Point(110, 67);
        txtChainName.Name = "txtChainName";
        txtChainName.Size = new Size(300, 25);
        txtChainName.PlaceholderText = "Görev zinciri adını girin...";

        //
        // lblStepType
        //
        lblStepType.AutoSize = true;
        lblStepType.Font = new Font("Segoe UI", 10F);
        lblStepType.Location = new Point(20, 110);
        lblStepType.Name = "lblStepType";
        lblStepType.Size = new Size(80, 19);
        lblStepType.Text = "Görev Tipi:";

        //
        // cmbStepType
        //
        cmbStepType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbStepType.Font = new Font("Segoe UI", 10F);
        cmbStepType.FormattingEnabled = true;
        cmbStepType.Items.AddRange(new object[] {
            "Tip 1: Hedef Program/Pencere Seçimi",
            "Tip 2: UI Element Tıklama/Tuşlama",
            "Tip 3: Sayfa Durum Kontrolü (Koşullu Dallanma)",
            "Tip 4: Döngü veya Bitiş Koşulu"
        });
        cmbStepType.Location = new Point(110, 107);
        cmbStepType.Name = "cmbStepType";
        cmbStepType.Size = new Size(400, 25);
        cmbStepType.SelectedIndex = 0;
        cmbStepType.SelectedIndexChanged += cmbStepType_SelectedIndexChanged;

        //
        // grpTargetSelection
        //
        grpTargetSelection.Font = new Font("Segoe UI", 10F);
        grpTargetSelection.Location = new Point(20, 150);
        grpTargetSelection.Name = "grpTargetSelection";
        grpTargetSelection.Size = new Size(680, 200);
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
        lblTargetInfo.Location = new Point(15, 30);
        lblTargetInfo.Name = "lblTargetInfo";
        lblTargetInfo.Size = new Size(400, 15);
        lblTargetInfo.Text = "Hedef program veya pencereyi seçin:";

        //
        // txtProgramPath
        //
        txtProgramPath.Font = new Font("Segoe UI", 9F);
        txtProgramPath.Location = new Point(15, 55);
        txtProgramPath.Name = "txtProgramPath";
        txtProgramPath.ReadOnly = true;
        txtProgramPath.Size = new Size(650, 23);
        txtProgramPath.PlaceholderText = "Seçilen hedef burada görünecek...";

        //
        // btnBrowse
        //
        btnBrowse.Font = new Font("Segoe UI", 10F);
        btnBrowse.Location = new Point(15, 95);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(200, 40);
        btnBrowse.Text = "📁 Gözat (Program Seç)";
        btnBrowse.UseVisualStyleBackColor = true;
        btnBrowse.Click += btnBrowse_Click;

        //
        // btnSelectWindow
        //
        btnSelectWindow.Font = new Font("Segoe UI", 10F);
        btnSelectWindow.Location = new Point(240, 95);
        btnSelectWindow.Name = "btnSelectWindow";
        btnSelectWindow.Size = new Size(200, 40);
        btnSelectWindow.Text = "🖱️ Sayfa Seç (Aktif Pencere)";
        btnSelectWindow.UseVisualStyleBackColor = true;
        btnSelectWindow.Click += btnSelectWindow_Click;

        //
        // btnDesktop
        //
        btnDesktop.Font = new Font("Segoe UI", 10F);
        btnDesktop.Location = new Point(465, 95);
        btnDesktop.Name = "btnDesktop";
        btnDesktop.Size = new Size(200, 40);
        btnDesktop.Text = "🖥️ Masaüstü";
        btnDesktop.UseVisualStyleBackColor = true;
        btnDesktop.Click += btnDesktop_Click;

        //
        // grpUIElementAction
        //
        grpUIElementAction.Font = new Font("Segoe UI", 10F);
        grpUIElementAction.Location = new Point(20, 150);
        grpUIElementAction.Name = "grpUIElementAction";
        grpUIElementAction.Size = new Size(680, 200);
        grpUIElementAction.Text = "2) UI Element Tıklama/Tuşlama";
        grpUIElementAction.Visible = false;
        grpUIElementAction.Controls.Add(lblUIElementInfo);
        grpUIElementAction.Controls.Add(btnPickElement);
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
        lblUIElementInfo.Location = new Point(15, 30);
        lblUIElementInfo.Name = "lblUIElementInfo";
        lblUIElementInfo.Size = new Size(400, 15);
        lblUIElementInfo.Text = "Mouse ile tıklayarak UI elementini seçin:";

        //
        // btnPickElement
        //
        btnPickElement.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnPickElement.Location = new Point(15, 55);
        btnPickElement.Name = "btnPickElement";
        btnPickElement.Size = new Size(200, 40);
        btnPickElement.Text = "🎯 Element Seç";
        btnPickElement.UseVisualStyleBackColor = true;
        btnPickElement.Click += btnPickElement_Click;

        //
        // txtElementProperties
        //
        txtElementProperties.Font = new Font("Consolas", 8F);
        txtElementProperties.Location = new Point(240, 55);
        txtElementProperties.Multiline = true;
        txtElementProperties.Name = "txtElementProperties";
        txtElementProperties.ReadOnly = true;
        txtElementProperties.ScrollBars = ScrollBars.Vertical;
        txtElementProperties.Size = new Size(425, 80);
        txtElementProperties.PlaceholderText = "Seçilen elementin özellikleri burada görünecek...";

        //
        // lblActionType
        //
        lblActionType.AutoSize = true;
        lblActionType.Font = new Font("Segoe UI", 9F);
        lblActionType.Location = new Point(15, 150);
        lblActionType.Name = "lblActionType";
        lblActionType.Size = new Size(80, 15);
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
        cmbActionType.Location = new Point(110, 147);
        cmbActionType.Name = "cmbActionType";
        cmbActionType.Size = new Size(200, 23);
        cmbActionType.SelectedIndex = 0;
        cmbActionType.SelectedIndexChanged += cmbActionType_SelectedIndexChanged;

        //
        // lblKeysToPress
        //
        lblKeysToPress.AutoSize = true;
        lblKeysToPress.Font = new Font("Segoe UI", 9F);
        lblKeysToPress.Location = new Point(330, 150);
        lblKeysToPress.Name = "lblKeysToPress";
        lblKeysToPress.Size = new Size(100, 15);
        lblKeysToPress.Text = "Tuşlar/Metin:";
        lblKeysToPress.Visible = false;

        //
        // txtKeysToPress
        //
        txtKeysToPress.Font = new Font("Segoe UI", 9F);
        txtKeysToPress.Location = new Point(435, 147);
        txtKeysToPress.Name = "txtKeysToPress";
        txtKeysToPress.Size = new Size(230, 23);
        txtKeysToPress.PlaceholderText = "Örn: {ENTER} veya metin...";
        txtKeysToPress.Visible = false;

        //
        // grpStrategyTest
        //
        grpStrategyTest.Font = new Font("Segoe UI", 10F);
        grpStrategyTest.Location = new Point(720, 150);
        grpStrategyTest.Name = "grpStrategyTest";
        grpStrategyTest.Size = new Size(450, 380);
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
        lblStrategyInfo.Location = new Point(15, 30);
        lblStrategyInfo.Name = "lblStrategyInfo";
        lblStrategyInfo.Size = new Size(400, 15);
        lblStrategyInfo.Text = "Element için olası tıklama stratejileri:";

        //
        // btnTestAllStrategies
        //
        btnTestAllStrategies.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnTestAllStrategies.Location = new Point(15, 55);
        btnTestAllStrategies.Name = "btnTestAllStrategies";
        btnTestAllStrategies.Size = new Size(420, 35);
        btnTestAllStrategies.Text = "🧪 Tüm Stratejileri Test Et";
        btnTestAllStrategies.UseVisualStyleBackColor = true;
        btnTestAllStrategies.Click += btnTestAllStrategies_Click;

        //
        // lstStrategies
        //
        lstStrategies.Font = new Font("Consolas", 8F);
        lstStrategies.FormattingEnabled = true;
        lstStrategies.ItemHeight = 13;
        lstStrategies.Location = new Point(15, 100);
        lstStrategies.Name = "lstStrategies";
        lstStrategies.Size = new Size(420, 220);
        lstStrategies.SelectionMode = SelectionMode.One;
        lstStrategies.SelectedIndexChanged += lstStrategies_SelectedIndexChanged;

        //
        // lblSelectedStrategy
        //
        lblSelectedStrategy.AutoSize = true;
        lblSelectedStrategy.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSelectedStrategy.Location = new Point(15, 330);
        lblSelectedStrategy.Name = "lblSelectedStrategy";
        lblSelectedStrategy.Size = new Size(300, 15);
        lblSelectedStrategy.Text = "Seçili Strateji: Henüz seçilmedi";
        lblSelectedStrategy.ForeColor = Color.Blue;

        //
        // lblTestResult
        //
        lblTestResult.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblTestResult.Location = new Point(15, 350);
        lblTestResult.Name = "lblTestResult";
        lblTestResult.Size = new Size(420, 25);
        lblTestResult.Text = "";
        lblTestResult.TextAlign = ContentAlignment.MiddleLeft;
        lblTestResult.ForeColor = Color.Black;

        //
        // txtLog
        //
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.Location = new Point(20, 545);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(1150, 120);
        txtLog.Text = "Görev kaydedici hazır...\n";

        //
        // btnSaveStep
        //
        btnSaveStep.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSaveStep.Location = new Point(20, 680);
        btnSaveStep.Name = "btnSaveStep";
        btnSaveStep.Size = new Size(150, 40);
        btnSaveStep.Text = "💾 Adımı Kaydet";
        btnSaveStep.UseVisualStyleBackColor = true;
        btnSaveStep.Click += btnSaveStep_Click;

        //
        // btnTestStep
        //
        btnTestStep.Font = new Font("Segoe UI", 10F);
        btnTestStep.Location = new Point(190, 680);
        btnTestStep.Name = "btnTestStep";
        btnTestStep.Size = new Size(150, 40);
        btnTestStep.Text = "🧪 Test Adım";
        btnTestStep.UseVisualStyleBackColor = true;
        btnTestStep.Click += btnTestStep_Click;

        //
        // btnNextStep
        //
        btnNextStep.Font = new Font("Segoe UI", 10F);
        btnNextStep.Location = new Point(360, 680);
        btnNextStep.Name = "btnNextStep";
        btnNextStep.Size = new Size(150, 40);
        btnNextStep.Text = "➡️ Sonraki Adım";
        btnNextStep.UseVisualStyleBackColor = true;
        btnNextStep.Click += btnNextStep_Click;

        //
        // btnSaveChain
        //
        btnSaveChain.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSaveChain.Location = new Point(530, 680);
        btnSaveChain.Name = "btnSaveChain";
        btnSaveChain.Size = new Size(170, 40);
        btnSaveChain.Text = "💾 Zinciri Kaydet";
        btnSaveChain.UseVisualStyleBackColor = true;
        btnSaveChain.Click += btnSaveChain_Click;

        //
        // btnClose
        //
        btnClose.Font = new Font("Segoe UI", 9F);
        btnClose.Location = new Point(1110, 680);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(60, 40);
        btnClose.Text = "Kapat";
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += btnClose_Click;

        //
        // TaskChainRecorderForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1190, 740);
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
        MinimumSize = new Size(1200, 780);
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
