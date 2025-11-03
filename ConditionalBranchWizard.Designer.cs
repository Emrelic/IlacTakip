namespace MedulaOtomasyon;

partial class ConditionalBranchWizard
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
        lblStepTitle = new Label();
        lblStepDescription = new Label();

        // Panels
        pnlHeader = new Panel();
        pnlStepContainer = new Panel();
        pnlFooter = new Panel();

        // Buttons
        btnBack = new Button();
        btnNext = new Button();
        btnCancel = new Button();

        pnlHeader.SuspendLayout();
        pnlFooter.SuspendLayout();
        SuspendLayout();

        //
        // Form
        //
        this.ClientSize = new Size(1100, 700);
        this.Text = "Koşullu Dallanma Sihirbazı";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        //
        // pnlHeader
        //
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 80;
        pnlHeader.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        pnlHeader.BorderStyle = BorderStyle.FixedSingle;
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblStepTitle);
        pnlHeader.Controls.Add(lblStepDescription);

        //
        // lblTitle
        //
        lblTitle.AutoSize = false;
        lblTitle.Location = new Point(20, 10);
        lblTitle.Size = new Size(600, 25);
        lblTitle.Text = "Koşullu Dallanma Sihirbazı";
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.DarkBlue;

        //
        // lblStepTitle
        //
        lblStepTitle.AutoSize = false;
        lblStepTitle.Location = new Point(900, 10);
        lblStepTitle.Size = new Size(180, 25);
        lblStepTitle.Text = "Adım 1 / 5";
        lblStepTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblStepTitle.ForeColor = System.Drawing.Color.DarkGreen;
        lblStepTitle.TextAlign = ContentAlignment.MiddleRight;

        //
        // lblStepDescription
        //
        lblStepDescription.AutoSize = false;
        lblStepDescription.Location = new Point(20, 45);
        lblStepDescription.Size = new Size(1060, 25);
        lblStepDescription.Text = "📄 Hedef Sayfa Seçimi";
        lblStepDescription.Font = new Font("Segoe UI", 12F);
        lblStepDescription.ForeColor = System.Drawing.Color.DarkSlateGray;

        //
        // pnlStepContainer
        //
        pnlStepContainer.Dock = DockStyle.Fill;
        pnlStepContainer.BackColor = System.Drawing.Color.White;
        pnlStepContainer.Padding = new Padding(10);

        //
        // pnlFooter
        //
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Height = 60;
        pnlFooter.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        pnlFooter.BorderStyle = BorderStyle.FixedSingle;
        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Controls.Add(btnNext);
        pnlFooter.Controls.Add(btnBack);

        //
        // btnBack
        //
        btnBack.Location = new Point(730, 12);
        btnBack.Size = new Size(110, 35);
        btnBack.Text = "← Geri";
        btnBack.Font = new Font("Segoe UI", 10F);
        btnBack.UseVisualStyleBackColor = true;
        btnBack.Click += BtnBack_Click;

        //
        // btnNext
        //
        btnNext.Location = new Point(850, 12);
        btnNext.Size = new Size(110, 35);
        btnNext.Text = "İleri →";
        btnNext.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnNext.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnNext.ForeColor = System.Drawing.Color.White;
        btnNext.FlatStyle = FlatStyle.Flat;
        btnNext.Click += BtnNext_Click;

        //
        // btnCancel
        //
        btnCancel.Location = new Point(970, 12);
        btnCancel.Size = new Size(110, 35);
        btnCancel.Text = "❌ İptal";
        btnCancel.Font = new Font("Segoe UI", 10F);
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += BtnCancel_Click;

        // Add controls to form
        this.Controls.Add(pnlStepContainer);
        this.Controls.Add(pnlHeader);
        this.Controls.Add(pnlFooter);

        pnlHeader.ResumeLayout(false);
        pnlFooter.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Label lblTitle;
    private Label lblStepTitle;
    private Label lblStepDescription;
    private Panel pnlHeader;
    private Panel pnlStepContainer;
    private Panel pnlFooter;
    private Button btnBack;
    private Button btnNext;
    private Button btnCancel;
}
