namespace MedulaOtomasyon;

partial class Step4_BranchPaths
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

    private void InitializeComponent()
    {
        lblInstruction = new Label();
        lblConditionSummary = new Label();
        lblBranchType = new Label();
        cmbBranchType = new ComboBox();

        grpTrueBranch = new GroupBox();
        lblTrueInfo = new Label();
        lblTrueTargetStep = new Label();
        txtTrueTargetStep = new TextBox();
        btnSuggestTrue = new Button();
        lblTrueDescription = new Label();
        txtTrueDescription = new TextBox();

        grpFalseBranch = new GroupBox();
        lblFalseInfo = new Label();
        lblFalseTargetStep = new Label();
        txtFalseTargetStep = new TextBox();
        btnSuggestFalse = new Button();
        lblFalseDescription = new Label();
        txtFalseDescription = new TextBox();

        grpDefault = new GroupBox();
        lblDefaultInfo = new Label();
        lblDefaultStep = new Label();
        txtDefaultStep = new TextBox();

        btnSave = new Button();

        grpTrueBranch.SuspendLayout();
        grpFalseBranch.SuspendLayout();
        grpDefault.SuspendLayout();
        SuspendLayout();

        this.Size = new Size(1080, 560);
        this.BackColor = System.Drawing.Color.White;

        //
        // lblInstruction
        //
        lblInstruction.Location = new Point(20, 20);
        lblInstruction.Size = new Size(1040, 30);
        lblInstruction.Text = "🔀 Dallanma Yolları - Koşul sonucuna göre program hangi yola gitsin?";
        lblInstruction.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

        //
        // lblConditionSummary
        //
        lblConditionSummary.Location = new Point(20, 60);
        lblConditionSummary.Size = new Size(1040, 60);
        lblConditionSummary.Font = new Font("Consolas", 9F);
        lblConditionSummary.ForeColor = System.Drawing.Color.DarkBlue;

        //
        // lblBranchType
        //
        lblBranchType.Location = new Point(20, 130);
        lblBranchType.Size = new Size(150, 20);
        lblBranchType.Text = "Dallanma Türü:";
        lblBranchType.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        //
        // cmbBranchType
        //
        cmbBranchType.Location = new Point(180, 128);
        cmbBranchType.Size = new Size(300, 25);
        cmbBranchType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbBranchType.Font = new Font("Segoe UI", 9F);

        //
        // grpTrueBranch
        //
        grpTrueBranch.Location = new Point(20, 170);
        grpTrueBranch.Size = new Size(500, 180);
        grpTrueBranch.Text = "✅ TRUE Dalı (Koşul Sağlandığında)";
        grpTrueBranch.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpTrueBranch.ForeColor = System.Drawing.Color.DarkGreen;
        grpTrueBranch.Controls.Add(lblTrueInfo);
        grpTrueBranch.Controls.Add(lblTrueTargetStep);
        grpTrueBranch.Controls.Add(txtTrueTargetStep);
        grpTrueBranch.Controls.Add(btnSuggestTrue);
        grpTrueBranch.Controls.Add(lblTrueDescription);
        grpTrueBranch.Controls.Add(txtTrueDescription);

        //
        // lblTrueInfo
        //
        lblTrueInfo.Location = new Point(15, 30);
        lblTrueInfo.Size = new Size(470, 20);
        lblTrueInfo.Text = "Koşul TRUE ise programa nereye gitmesini söyleyin:";
        lblTrueInfo.Font = new Font("Segoe UI", 9F);
        lblTrueInfo.ForeColor = System.Drawing.Color.Black;

        //
        // lblTrueTargetStep
        //
        lblTrueTargetStep.Location = new Point(15, 55);
        lblTrueTargetStep.Size = new Size(120, 20);
        lblTrueTargetStep.Text = "Hedef Adım:";
        lblTrueTargetStep.Font = new Font("Segoe UI", 9F);
        lblTrueTargetStep.ForeColor = System.Drawing.Color.Black;

        //
        // txtTrueTargetStep
        //
        txtTrueTargetStep.Location = new Point(15, 80);
        txtTrueTargetStep.Size = new Size(150, 25);
        txtTrueTargetStep.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        txtTrueTargetStep.PlaceholderText = "örn: 7A";

        //
        // btnSuggestTrue
        //
        btnSuggestTrue.Location = new Point(180, 78);
        btnSuggestTrue.Size = new Size(150, 29);
        btnSuggestTrue.Text = "💡 Otomatik Öner";
        btnSuggestTrue.Font = new Font("Segoe UI", 9F);
        btnSuggestTrue.Click += BtnSuggestTrue_Click;

        //
        // lblTrueDescription
        //
        lblTrueDescription.Location = new Point(15, 115);
        lblTrueDescription.Size = new Size(200, 20);
        lblTrueDescription.Text = "Açıklama (opsiyonel):";
        lblTrueDescription.Font = new Font("Segoe UI", 9F);
        lblTrueDescription.ForeColor = System.Drawing.Color.Black;

        //
        // txtTrueDescription
        //
        txtTrueDescription.Location = new Point(15, 140);
        txtTrueDescription.Size = new Size(470, 25);
        txtTrueDescription.Font = new Font("Segoe UI", 9F);
        txtTrueDescription.PlaceholderText = "örn: Hata mesajı var, alternatif akış";

        //
        // grpFalseBranch
        //
        grpFalseBranch.Location = new Point(540, 170);
        grpFalseBranch.Size = new Size(520, 180);
        grpFalseBranch.Text = "❌ FALSE Dalı (Koşul Sağlanmadığında)";
        grpFalseBranch.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpFalseBranch.ForeColor = System.Drawing.Color.DarkRed;
        grpFalseBranch.Controls.Add(lblFalseInfo);
        grpFalseBranch.Controls.Add(lblFalseTargetStep);
        grpFalseBranch.Controls.Add(txtFalseTargetStep);
        grpFalseBranch.Controls.Add(btnSuggestFalse);
        grpFalseBranch.Controls.Add(lblFalseDescription);
        grpFalseBranch.Controls.Add(txtFalseDescription);

        //
        // lblFalseInfo
        //
        lblFalseInfo.Location = new Point(15, 30);
        lblFalseInfo.Size = new Size(490, 20);
        lblFalseInfo.Text = "Koşul FALSE ise programın nereye gitmesini söyleyin:";
        lblFalseInfo.Font = new Font("Segoe UI", 9F);
        lblFalseInfo.ForeColor = System.Drawing.Color.Black;

        //
        // lblFalseTargetStep
        //
        lblFalseTargetStep.Location = new Point(15, 55);
        lblFalseTargetStep.Size = new Size(120, 20);
        lblFalseTargetStep.Text = "Hedef Adım:";
        lblFalseTargetStep.Font = new Font("Segoe UI", 9F);
        lblFalseTargetStep.ForeColor = System.Drawing.Color.Black;

        //
        // txtFalseTargetStep
        //
        txtFalseTargetStep.Location = new Point(15, 80);
        txtFalseTargetStep.Size = new Size(150, 25);
        txtFalseTargetStep.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        txtFalseTargetStep.PlaceholderText = "örn: 7B";

        //
        // btnSuggestFalse
        //
        btnSuggestFalse.Location = new Point(180, 78);
        btnSuggestFalse.Size = new Size(150, 29);
        btnSuggestFalse.Text = "💡 Otomatik Öner";
        btnSuggestFalse.Font = new Font("Segoe UI", 9F);
        btnSuggestFalse.Click += BtnSuggestFalse_Click;

        //
        // lblFalseDescription
        //
        lblFalseDescription.Location = new Point(15, 115);
        lblFalseDescription.Size = new Size(200, 20);
        lblFalseDescription.Text = "Açıklama (opsiyonel):";
        lblFalseDescription.Font = new Font("Segoe UI", 9F);
        lblFalseDescription.ForeColor = System.Drawing.Color.Black;

        //
        // txtFalseDescription
        //
        txtFalseDescription.Location = new Point(15, 140);
        txtFalseDescription.Size = new Size(490, 25);
        txtFalseDescription.Font = new Font("Segoe UI", 9F);
        txtFalseDescription.PlaceholderText = "örn: Normal akış, devam et";

        //
        // grpDefault
        //
        grpDefault.Location = new Point(20, 370);
        grpDefault.Size = new Size(1040, 120);
        grpDefault.Text = "🔧 Varsayılan Dal (Opsiyonel - Koşul hata verirse)";
        grpDefault.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpDefault.ForeColor = System.Drawing.Color.DarkOrange;
        grpDefault.Controls.Add(lblDefaultInfo);
        grpDefault.Controls.Add(lblDefaultStep);
        grpDefault.Controls.Add(txtDefaultStep);

        //
        // lblDefaultInfo
        //
        lblDefaultInfo.Location = new Point(15, 30);
        lblDefaultInfo.Size = new Size(1010, 20);
        lblDefaultInfo.Text = "Koşul kontrolü sırasında bir hata oluşursa (element bulunamadı, vb.) programa nereye gitsin?";
        lblDefaultInfo.Font = new Font("Segoe UI", 9F);
        lblDefaultInfo.ForeColor = System.Drawing.Color.Black;

        //
        // lblDefaultStep
        //
        lblDefaultStep.Location = new Point(15, 55);
        lblDefaultStep.Size = new Size(120, 20);
        lblDefaultStep.Text = "Hedef Adım:";
        lblDefaultStep.Font = new Font("Segoe UI", 9F);
        lblDefaultStep.ForeColor = System.Drawing.Color.Black;

        //
        // txtDefaultStep
        //
        txtDefaultStep.Location = new Point(15, 80);
        txtDefaultStep.Size = new Size(200, 25);
        txtDefaultStep.Font = new Font("Segoe UI", 10F);
        txtDefaultStep.PlaceholderText = "örn: SONLANDIR veya 8";

        //
        // btnSave
        //
        btnSave.Location = new Point(800, 505);
        btnSave.Size = new Size(260, 40);
        btnSave.Text = "💾 Dallanma Yollarını Kaydet";
        btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSave.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnSave.ForeColor = System.Drawing.Color.White;
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.Click += BtnSave_Click;

        // Add controls
        this.Controls.Add(lblInstruction);
        this.Controls.Add(lblConditionSummary);
        this.Controls.Add(lblBranchType);
        this.Controls.Add(cmbBranchType);
        this.Controls.Add(grpTrueBranch);
        this.Controls.Add(grpFalseBranch);
        this.Controls.Add(grpDefault);
        this.Controls.Add(btnSave);

        grpTrueBranch.ResumeLayout(false);
        grpFalseBranch.ResumeLayout(false);
        grpDefault.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Label lblInstruction;
    private Label lblConditionSummary;
    private Label lblBranchType;
    private ComboBox cmbBranchType;

    private GroupBox grpTrueBranch;
    private Label lblTrueInfo;
    private Label lblTrueTargetStep;
    private TextBox txtTrueTargetStep;
    private Button btnSuggestTrue;
    private Label lblTrueDescription;
    private TextBox txtTrueDescription;

    private GroupBox grpFalseBranch;
    private Label lblFalseInfo;
    private Label lblFalseTargetStep;
    private TextBox txtFalseTargetStep;
    private Button btnSuggestFalse;
    private Label lblFalseDescription;
    private TextBox txtFalseDescription;

    private GroupBox grpDefault;
    private Label lblDefaultInfo;
    private Label lblDefaultStep;
    private TextBox txtDefaultStep;

    private Button btnSave;
}
