namespace MedulaOtomasyon;

partial class ConditionalBranchRecorderForm
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
        lblPageInfo = new Label();
        lblDetectWarning = new Label();
        lblBranchType = new Label();
        lblConditions = new Label();
        lblBranches = new Label();

        // Buttons
        btnTopmost = new Button();
        btnDetectTargetPage = new Button();
        btnRefreshElements = new Button();
        btnAddCondition = new Button();
        btnRemoveCondition = new Button();
        btnAddBranch = new Button();
        btnRemoveBranch = new Button();
        btnSave = new Button();
        btnCancel = new Button();

        // ComboBox
        cmbBranchType = new ComboBox();

        // CheckBoxes
        chkLoopTerminationMode = new CheckBox();

        // TextBoxes
        txtPageIdentifier = new TextBox();

        // GroupBox for Conditions
        grpConditions = new GroupBox();
        lstConditions = new ListBox();
        pnlConditionEntry = new Panel();
        lblElement = new Label();
        cmbElement = new ComboBox();
        btnPickElement = new Button();
        lblProperty = new Label();
        cmbProperty = new ComboBox();
        lblOperator = new Label();
        cmbOperator = new ComboBox();
        lblValue = new Label();
        txtValue = new TextBox();
        lblLogicalOp = new Label();
        cmbLogicalOp = new ComboBox();

        // GroupBox for Branches
        grpBranches = new GroupBox();
        lstBranches = new ListBox();
        pnlBranchEntry = new Panel();
        lblBranchName = new Label();
        txtBranchName = new TextBox();
        lblTargetStepId = new Label();
        txtTargetStepId = new TextBox();
        lblConditionValue = new Label();
        txtConditionValue = new TextBox();
        lblBranchDesc = new Label();
        txtBranchDesc = new TextBox();

        // Default Branch
        lblDefaultBranch = new Label();
        txtDefaultBranch = new TextBox();

        SuspendLayout();

        //
        // Form
        //
        this.ClientSize = new Size(1200, 820);
        this.Text = "Koşullu Dallanma Kaydedici (Tip 3)";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MinimumSize = new Size(1000, 720);

        //
        // lblTitle
        //
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.Location = new Point(15, 15);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(350, 25);
        lblTitle.Text = "Koşullu Dallanma Görev Kaydedici";

        //
        // btnTopmost
        //
        btnTopmost.Location = new Point(1050, 10);
        btnTopmost.Name = "btnTopmost";
        btnTopmost.Size = new Size(130, 35);
        btnTopmost.Text = "📌 Üstte Tut";
        btnTopmost.UseVisualStyleBackColor = true;
        btnTopmost.Click += BtnTopmost_Click;

        //
        // lblPageInfo
        //
        lblPageInfo.AutoSize = true;
        lblPageInfo.Location = new Point(15, 55);
        lblPageInfo.Name = "lblPageInfo";
        lblPageInfo.Size = new Size(150, 15);
        lblPageInfo.Text = "Sayfa Tanımlayıcı:";

        //
        // txtPageIdentifier
        //
        txtPageIdentifier.Location = new Point(15, 75);
        txtPageIdentifier.Name = "txtPageIdentifier";
        txtPageIdentifier.Size = new Size(300, 23);
        txtPageIdentifier.PlaceholderText = "Örn: Hasta Kayıt Sayfası, URL, vb.";
        txtPageIdentifier.ReadOnly = true;
        txtPageIdentifier.BackColor = System.Drawing.SystemColors.Window;

        //
        // btnDetectTargetPage
        //
        btnDetectTargetPage.Location = new Point(320, 73);
        btnDetectTargetPage.Name = "btnDetectTargetPage";
        btnDetectTargetPage.Size = new Size(150, 27);
        btnDetectTargetPage.Text = "🎯 Hedef Sayfayı Belirle";
        btnDetectTargetPage.UseVisualStyleBackColor = true;
        btnDetectTargetPage.Click += BtnDetectTargetPage_Click;

        //
        // lblDetectWarning
        //
        lblDetectWarning.AutoSize = true;
        lblDetectWarning.Location = new Point(15, 103);
        lblDetectWarning.Name = "lblDetectWarning";
        lblDetectWarning.Size = new Size(0, 15);
        lblDetectWarning.ForeColor = System.Drawing.Color.Red;
        lblDetectWarning.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        //
        // lblBranchType
        //
        lblBranchType.AutoSize = true;
        lblBranchType.Location = new Point(440, 55);
        lblBranchType.Name = "lblBranchType";
        lblBranchType.Size = new Size(100, 15);
        lblBranchType.Text = "Dallanma Tipi:";

        //
        // cmbBranchType
        //
        cmbBranchType.Location = new Point(440, 75);
        cmbBranchType.Name = "cmbBranchType";
        cmbBranchType.Size = new Size(200, 23);
        cmbBranchType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbBranchType.Items.AddRange(new object[] { "Boolean (True/False)", "SwitchCase (Çoklu Dal)" });
        cmbBranchType.SelectedIndex = 0;
        cmbBranchType.SelectedIndexChanged += CmbBranchType_SelectedIndexChanged;

        //
        // chkLoopTerminationMode
        //
        chkLoopTerminationMode.AutoSize = true;
        chkLoopTerminationMode.Location = new Point(820, 77);
        chkLoopTerminationMode.Name = "chkLoopTerminationMode";
        chkLoopTerminationMode.Size = new Size(200, 19);
        chkLoopTerminationMode.Text = "🔄 Döngü Sonlanma Modu";
        chkLoopTerminationMode.UseVisualStyleBackColor = true;
        chkLoopTerminationMode.CheckedChanged += ChkLoopTerminationMode_CheckedChanged;

        //
        // btnRefreshElements
        //
        btnRefreshElements.Location = new Point(660, 70);
        btnRefreshElements.Name = "btnRefreshElements";
        btnRefreshElements.Size = new Size(140, 30);
        btnRefreshElements.Text = "🔄 Elementleri Listele";
        btnRefreshElements.UseVisualStyleBackColor = true;
        btnRefreshElements.Click += BtnRefreshElements_Click;

        //
        // grpConditions
        //
        grpConditions.Location = new Point(15, 130);
        grpConditions.Name = "grpConditions";
        grpConditions.Size = new Size(1165, 300);
        grpConditions.TabStop = false;
        grpConditions.Text = "Koşullar";

        //
        // lblConditions
        //
        lblConditions.AutoSize = true;
        lblConditions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblConditions.Location = new Point(10, 20);
        lblConditions.Name = "lblConditions";
        lblConditions.Size = new Size(300, 15);
        lblConditions.Text = "Tanımlı Koşullar (AND/OR ile bağlanır):";
        lblConditions.Parent = grpConditions;

        //
        // lstConditions
        //
        lstConditions.Location = new Point(10, 40);
        lstConditions.Name = "lstConditions";
        lstConditions.Size = new Size(1145, 120);
        lstConditions.Parent = grpConditions;
        lstConditions.SelectedIndexChanged += LstConditions_SelectedIndexChanged;

        //
        // pnlConditionEntry
        //
        pnlConditionEntry.Location = new Point(10, 170);
        pnlConditionEntry.Name = "pnlConditionEntry";
        pnlConditionEntry.Size = new Size(1145, 90);
        pnlConditionEntry.BorderStyle = BorderStyle.FixedSingle;
        pnlConditionEntry.Parent = grpConditions;

        //
        // lblElement
        //
        lblElement.AutoSize = true;
        lblElement.Location = new Point(5, 8);
        lblElement.Name = "lblElement";
        lblElement.Size = new Size(60, 15);
        lblElement.Text = "Element:";
        lblElement.Parent = pnlConditionEntry;

        //
        // cmbElement
        //
        cmbElement.Location = new Point(5, 26);
        cmbElement.Name = "cmbElement";
        cmbElement.Size = new Size(250, 23);
        cmbElement.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbElement.Parent = pnlConditionEntry;
        cmbElement.SelectedIndexChanged += CmbElement_SelectedIndexChanged;

        //
        // btnPickElement
        //
        btnPickElement.Location = new Point(260, 24);
        btnPickElement.Name = "btnPickElement";
        btnPickElement.Size = new Size(80, 27);
        btnPickElement.Text = "🎯 Seç";
        btnPickElement.UseVisualStyleBackColor = true;
        btnPickElement.Parent = pnlConditionEntry;
        btnPickElement.Click += BtnPickElement_Click;

        //
        // lblProperty
        //
        lblProperty.AutoSize = true;
        lblProperty.Location = new Point(350, 8);
        lblProperty.Name = "lblProperty";
        lblProperty.Size = new Size(55, 15);
        lblProperty.Text = "Özellik:";
        lblProperty.Parent = pnlConditionEntry;

        //
        // cmbProperty
        //
        cmbProperty.Location = new Point(350, 26);
        cmbProperty.Name = "cmbProperty";
        cmbProperty.Size = new Size(150, 23);
        cmbProperty.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbProperty.Parent = pnlConditionEntry;

        //
        // lblOperator
        //
        lblOperator.AutoSize = true;
        lblOperator.Location = new Point(510, 8);
        lblOperator.Name = "lblOperator";
        lblOperator.Size = new Size(60, 15);
        lblOperator.Text = "Operatör:";
        lblOperator.Parent = pnlConditionEntry;

        //
        // cmbOperator
        //
        cmbOperator.Location = new Point(510, 26);
        cmbOperator.Name = "cmbOperator";
        cmbOperator.Size = new Size(130, 23);
        cmbOperator.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbOperator.Parent = pnlConditionEntry;

        //
        // lblValue
        //
        lblValue.AutoSize = true;
        lblValue.Location = new Point(650, 8);
        lblValue.Name = "lblValue";
        lblValue.Size = new Size(40, 15);
        lblValue.Text = "Değer:";
        lblValue.Parent = pnlConditionEntry;

        //
        // txtValue
        //
        txtValue.Location = new Point(650, 26);
        txtValue.Name = "txtValue";
        txtValue.Size = new Size(180, 23);
        txtValue.Parent = pnlConditionEntry;

        //
        // lblLogicalOp
        //
        lblLogicalOp.AutoSize = true;
        lblLogicalOp.Location = new Point(840, 8);
        lblLogicalOp.Name = "lblLogicalOp";
        lblLogicalOp.Size = new Size(80, 15);
        lblLogicalOp.Text = "Sonraki ile:";
        lblLogicalOp.Parent = pnlConditionEntry;

        //
        // cmbLogicalOp
        //
        cmbLogicalOp.Location = new Point(840, 26);
        cmbLogicalOp.Name = "cmbLogicalOp";
        cmbLogicalOp.Size = new Size(100, 23);
        cmbLogicalOp.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLogicalOp.Parent = pnlConditionEntry;

        //
        // btnAddCondition
        //
        btnAddCondition.Location = new Point(950, 22);
        btnAddCondition.Name = "btnAddCondition";
        btnAddCondition.Size = new Size(90, 30);
        btnAddCondition.Text = "➕ Ekle";
        btnAddCondition.UseVisualStyleBackColor = true;
        btnAddCondition.Parent = pnlConditionEntry;
        btnAddCondition.Click += BtnAddCondition_Click;

        //
        // btnRemoveCondition
        //
        btnRemoveCondition.Location = new Point(1045, 22);
        btnRemoveCondition.Name = "btnRemoveCondition";
        btnRemoveCondition.Size = new Size(90, 30);
        btnRemoveCondition.Text = "❌ Sil";
        btnRemoveCondition.UseVisualStyleBackColor = true;
        btnRemoveCondition.Parent = pnlConditionEntry;
        btnRemoveCondition.Click += BtnRemoveCondition_Click;

        //
        // grpBranches
        //
        grpBranches.Location = new Point(15, 450);
        grpBranches.Name = "grpBranches";
        grpBranches.Size = new Size(1165, 260);
        grpBranches.TabStop = false;
        grpBranches.Text = "Dallanma Hedefleri";

        //
        // lblBranches
        //
        lblBranches.AutoSize = true;
        lblBranches.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblBranches.Location = new Point(10, 20);
        lblBranches.Name = "lblBranches";
        lblBranches.Size = new Size(400, 15);
        lblBranches.Text = "Dallar (Koşul sonucuna göre gidilecek adımlar):";
        lblBranches.Parent = grpBranches;

        //
        // lstBranches
        //
        lstBranches.Location = new Point(10, 40);
        lstBranches.Name = "lstBranches";
        lstBranches.Size = new Size(1145, 80);
        lstBranches.Parent = grpBranches;
        lstBranches.SelectedIndexChanged += LstBranches_SelectedIndexChanged;

        //
        // pnlBranchEntry
        //
        pnlBranchEntry.Location = new Point(10, 130);
        pnlBranchEntry.Name = "pnlBranchEntry";
        pnlBranchEntry.Size = new Size(1145, 90);
        pnlBranchEntry.BorderStyle = BorderStyle.FixedSingle;
        pnlBranchEntry.Parent = grpBranches;

        //
        // lblBranchName
        //
        lblBranchName.AutoSize = true;
        lblBranchName.Location = new Point(5, 8);
        lblBranchName.Name = "lblBranchName";
        lblBranchName.Size = new Size(60, 15);
        lblBranchName.Text = "Dal Adı:";
        lblBranchName.Parent = pnlBranchEntry;

        //
        // txtBranchName
        //
        txtBranchName.Location = new Point(5, 26);
        txtBranchName.Name = "txtBranchName";
        txtBranchName.Size = new Size(80, 23);
        txtBranchName.PlaceholderText = "A, B, C...";
        txtBranchName.Parent = pnlBranchEntry;

        //
        // lblTargetStepId
        //
        lblTargetStepId.AutoSize = true;
        lblTargetStepId.Location = new Point(95, 8);
        lblTargetStepId.Name = "lblTargetStepId";
        lblTargetStepId.Size = new Size(100, 15);
        lblTargetStepId.Text = "Hedef Adım ID:";
        lblTargetStepId.Parent = pnlBranchEntry;

        //
        // txtTargetStepId
        //
        txtTargetStepId.Location = new Point(95, 26);
        txtTargetStepId.Name = "txtTargetStepId";
        txtTargetStepId.Size = new Size(100, 23);
        txtTargetStepId.PlaceholderText = "6A, 7B...";
        txtTargetStepId.Parent = pnlBranchEntry;

        //
        // lblConditionValue
        //
        lblConditionValue.AutoSize = true;
        lblConditionValue.Location = new Point(205, 8);
        lblConditionValue.Name = "lblConditionValue";
        lblConditionValue.Size = new Size(100, 15);
        lblConditionValue.Text = "Koşul Değeri:";
        lblConditionValue.Parent = pnlBranchEntry;

        //
        // txtConditionValue
        //
        txtConditionValue.Location = new Point(205, 26);
        txtConditionValue.Name = "txtConditionValue";
        txtConditionValue.Size = new Size(120, 23);
        txtConditionValue.PlaceholderText = "true/false/...";
        txtConditionValue.Parent = pnlBranchEntry;

        //
        // lblBranchDesc
        //
        lblBranchDesc.AutoSize = true;
        lblBranchDesc.Location = new Point(335, 8);
        lblBranchDesc.Name = "lblBranchDesc";
        lblBranchDesc.Size = new Size(70, 15);
        lblBranchDesc.Text = "Açıklama:";
        lblBranchDesc.Parent = pnlBranchEntry;

        //
        // txtBranchDesc
        //
        txtBranchDesc.Location = new Point(335, 26);
        txtBranchDesc.Name = "txtBranchDesc";
        txtBranchDesc.Size = new Size(600, 23);
        txtBranchDesc.PlaceholderText = "Bu dal ne zaman seçilir?";
        txtBranchDesc.Parent = pnlBranchEntry;

        //
        // btnAddBranch
        //
        btnAddBranch.Location = new Point(950, 22);
        btnAddBranch.Name = "btnAddBranch";
        btnAddBranch.Size = new Size(90, 30);
        btnAddBranch.Text = "➕ Ekle";
        btnAddBranch.UseVisualStyleBackColor = true;
        btnAddBranch.Parent = pnlBranchEntry;
        btnAddBranch.Click += BtnAddBranch_Click;

        //
        // btnRemoveBranch
        //
        btnRemoveBranch.Location = new Point(1045, 22);
        btnRemoveBranch.Name = "btnRemoveBranch";
        btnRemoveBranch.Size = new Size(90, 30);
        btnRemoveBranch.Text = "❌ Sil";
        btnRemoveBranch.UseVisualStyleBackColor = true;
        btnRemoveBranch.Parent = pnlBranchEntry;
        btnRemoveBranch.Click += BtnRemoveBranch_Click;

        //
        // lblDefaultBranch
        //
        lblDefaultBranch.AutoSize = true;
        lblDefaultBranch.Location = new Point(15, 725);
        lblDefaultBranch.Name = "lblDefaultBranch";
        lblDefaultBranch.Size = new Size(250, 15);
        lblDefaultBranch.Text = "Varsayılan Dal (koşul sağlanmazsa):";

        //
        // txtDefaultBranch
        //
        txtDefaultBranch.Location = new Point(270, 722);
        txtDefaultBranch.Name = "txtDefaultBranch";
        txtDefaultBranch.Size = new Size(150, 23);
        txtDefaultBranch.PlaceholderText = "Adım ID";

        //
        // btnSave
        //
        btnSave.Location = new Point(900, 720);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(130, 35);
        btnSave.Text = "💾 Kaydet";
        btnSave.UseVisualStyleBackColor = true;
        btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSave.Click += BtnSave_Click;

        //
        // btnCancel
        //
        btnCancel.Location = new Point(1050, 720);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(130, 35);
        btnCancel.Text = "❌ İptal";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += BtnCancel_Click;

        // Add controls to form
        this.Controls.Add(lblTitle);
        this.Controls.Add(btnTopmost);
        this.Controls.Add(lblPageInfo);
        this.Controls.Add(txtPageIdentifier);
        this.Controls.Add(btnDetectTargetPage);
        this.Controls.Add(lblDetectWarning);
        this.Controls.Add(lblBranchType);
        this.Controls.Add(cmbBranchType);
        this.Controls.Add(chkLoopTerminationMode);
        this.Controls.Add(btnRefreshElements);
        this.Controls.Add(grpConditions);
        this.Controls.Add(grpBranches);
        this.Controls.Add(lblDefaultBranch);
        this.Controls.Add(txtDefaultBranch);
        this.Controls.Add(btnSave);
        this.Controls.Add(btnCancel);

        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitle;
    private Label lblPageInfo;
    private Label lblDetectWarning;
    private Label lblBranchType;
    private Label lblConditions;
    private Label lblBranches;
    private Button btnTopmost;
    private Button btnDetectTargetPage;
    private Button btnRefreshElements;
    private Button btnAddCondition;
    private Button btnRemoveCondition;
    private Button btnAddBranch;
    private Button btnRemoveBranch;
    private Button btnSave;
    private Button btnCancel;
    private ComboBox cmbBranchType;
    private CheckBox chkLoopTerminationMode;
    private TextBox txtPageIdentifier;
    private GroupBox grpConditions;
    private ListBox lstConditions;
    private Panel pnlConditionEntry;
    private Label lblElement;
    private ComboBox cmbElement;
    private Button btnPickElement;
    private Label lblProperty;
    private ComboBox cmbProperty;
    private Label lblOperator;
    private ComboBox cmbOperator;
    private Label lblValue;
    private TextBox txtValue;
    private Label lblLogicalOp;
    private ComboBox cmbLogicalOp;
    private GroupBox grpBranches;
    private ListBox lstBranches;
    private Panel pnlBranchEntry;
    private Label lblBranchName;
    private TextBox txtBranchName;
    private Label lblTargetStepId;
    private TextBox txtTargetStepId;
    private Label lblConditionValue;
    private TextBox txtConditionValue;
    private Label lblBranchDesc;
    private TextBox txtBranchDesc;
    private Label lblDefaultBranch;
    private TextBox txtDefaultBranch;
}
