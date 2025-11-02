namespace MedulaOtomasyon;

partial class Step5_Summary
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
        grpPageInfo = new GroupBox();
        lblPageTitle = new Label();
        lblPageValue = new Label();

        grpElementInfo = new GroupBox();
        lblElementTitle = new Label();
        txtElementInfo = new TextBox();

        grpConditions = new GroupBox();
        txtConditions = new TextBox();

        grpBranches = new GroupBox();
        txtBranches = new TextBox();

        btnSave = new Button();
        btnCancel = new Button();

        grpPageInfo.SuspendLayout();
        grpElementInfo.SuspendLayout();
        grpConditions.SuspendLayout();
        grpBranches.SuspendLayout();
        SuspendLayout();

        this.Size = new Size(1080, 560);
        this.BackColor = System.Drawing.Color.White;

        //
        // lblInstruction
        //
        lblInstruction.Location = new Point(20, 20);
        lblInstruction.Size = new Size(1040, 30);
        lblInstruction.Text = "📋 Özet - Tüm ayarları kontrol edin ve kaydedin";
        lblInstruction.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

        //
        // grpPageInfo
        //
        grpPageInfo.Location = new Point(20, 60);
        grpPageInfo.Size = new Size(1040, 70);
        grpPageInfo.Text = "🎯 Hedef Sayfa";
        grpPageInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpPageInfo.Controls.Add(lblPageTitle);
        grpPageInfo.Controls.Add(lblPageValue);

        //
        // lblPageTitle
        //
        lblPageTitle.Location = new Point(15, 25);
        lblPageTitle.Size = new Size(100, 20);
        lblPageTitle.Text = "Sayfa:";
        lblPageTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        //
        // lblPageValue
        //
        lblPageValue.Location = new Point(120, 25);
        lblPageValue.Size = new Size(900, 35);
        lblPageValue.Font = new Font("Segoe UI", 10F);
        lblPageValue.ForeColor = System.Drawing.Color.DarkBlue;

        //
        // grpElementInfo
        //
        grpElementInfo.Location = new Point(20, 140);
        grpElementInfo.Size = new Size(1040, 100);
        grpElementInfo.Text = "🔍 Seçili UI Element";
        grpElementInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpElementInfo.Controls.Add(lblElementTitle);
        grpElementInfo.Controls.Add(txtElementInfo);

        //
        // lblElementTitle
        //
        lblElementTitle.Location = new Point(15, 25);
        lblElementTitle.Size = new Size(1010, 20);
        lblElementTitle.Text = "Element Özellikleri:";
        lblElementTitle.Font = new Font("Segoe UI", 9F);

        //
        // txtElementInfo
        //
        txtElementInfo.Location = new Point(15, 45);
        txtElementInfo.Size = new Size(1010, 45);
        txtElementInfo.Multiline = true;
        txtElementInfo.ReadOnly = true;
        txtElementInfo.Font = new Font("Consolas", 9F);
        txtElementInfo.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);

        //
        // grpConditions
        //
        grpConditions.Location = new Point(20, 250);
        grpConditions.Size = new Size(510, 180);
        grpConditions.Text = "⚙️ Koşullar";
        grpConditions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpConditions.Controls.Add(txtConditions);

        //
        // txtConditions
        //
        txtConditions.Location = new Point(15, 25);
        txtConditions.Size = new Size(480, 145);
        txtConditions.Multiline = true;
        txtConditions.ReadOnly = true;
        txtConditions.Font = new Font("Consolas", 9F);
        txtConditions.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
        txtConditions.ScrollBars = ScrollBars.Vertical;

        //
        // grpBranches
        //
        grpBranches.Location = new Point(550, 250);
        grpBranches.Size = new Size(510, 180);
        grpBranches.Text = "🔀 Dallanma Yolları";
        grpBranches.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpBranches.ForeColor = System.Drawing.Color.DarkGreen;
        grpBranches.Controls.Add(txtBranches);

        //
        // txtBranches
        //
        txtBranches.Location = new Point(15, 25);
        txtBranches.Size = new Size(480, 145);
        txtBranches.Multiline = true;
        txtBranches.ReadOnly = true;
        txtBranches.Font = new Font("Consolas", 9F);
        txtBranches.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
        txtBranches.ScrollBars = ScrollBars.Vertical;

        //
        // btnSave
        //
        btnSave.Location = new Point(720, 450);
        btnSave.Size = new Size(170, 45);
        btnSave.Text = "💾 Kaydet";
        btnSave.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnSave.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnSave.ForeColor = System.Drawing.Color.White;
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.Click += BtnSave_Click;

        //
        // btnCancel
        //
        btnCancel.Location = new Point(900, 450);
        btnCancel.Size = new Size(160, 45);
        btnCancel.Text = "❌ İptal";
        btnCancel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnCancel.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
        btnCancel.ForeColor = System.Drawing.Color.White;
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.Click += BtnCancel_Click;

        // Add controls
        this.Controls.Add(lblInstruction);
        this.Controls.Add(grpPageInfo);
        this.Controls.Add(grpElementInfo);
        this.Controls.Add(grpConditions);
        this.Controls.Add(grpBranches);
        this.Controls.Add(btnSave);
        this.Controls.Add(btnCancel);

        grpPageInfo.ResumeLayout(false);
        grpElementInfo.ResumeLayout(false);
        grpConditions.ResumeLayout(false);
        grpBranches.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Label lblInstruction;

    private GroupBox grpPageInfo;
    private Label lblPageTitle;
    private Label lblPageValue;

    private GroupBox grpElementInfo;
    private Label lblElementTitle;
    private TextBox txtElementInfo;

    private GroupBox grpConditions;
    private TextBox txtConditions;

    private GroupBox grpBranches;
    private TextBox txtBranches;

    private Button btnSave;
    private Button btnCancel;
}
