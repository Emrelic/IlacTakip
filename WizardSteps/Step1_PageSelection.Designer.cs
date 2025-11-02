namespace MedulaOtomasyon;

partial class Step1_PageSelection
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

    #region Component Designer generated code

    private void InitializeComponent()
    {
        // Labels
        lblInstruction = new Label();
        lblMouseOption = new Label();
        lblOrSeparator = new Label();
        lblListOption = new Label();
        lblSelectedPage = new Label();

        // Buttons
        btnSelectWithMouse = new Button();
        btnSelectFromList = new Button();
        btnRefresh = new Button();

        // ComboBox
        cmbWindows = new ComboBox();

        // GroupBoxes
        grpMouseSelection = new GroupBox();
        grpListSelection = new GroupBox();
        grpSelectedPage = new GroupBox();

        grpMouseSelection.SuspendLayout();
        grpListSelection.SuspendLayout();
        grpSelectedPage.SuspendLayout();
        SuspendLayout();

        //
        // Step1_PageSelection
        //
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.White;
        this.Size = new Size(1080, 560);

        //
        // lblInstruction
        //
        lblInstruction.AutoSize = false;
        lblInstruction.Location = new Point(20, 20);
        lblInstruction.Size = new Size(1040, 50);
        lblInstruction.Text = "Koşulları kontrol edeceğiniz hedef sayfayı seçin.\nMouse ile doğrudan tıklayarak veya açık pencereler listesinden seçebilirsiniz.";
        lblInstruction.Font = new Font("Segoe UI", 10F);
        lblInstruction.ForeColor = System.Drawing.Color.DarkSlateGray;

        //
        // grpMouseSelection
        //
        grpMouseSelection.Location = new Point(20, 80);
        grpMouseSelection.Size = new Size(1040, 120);
        grpMouseSelection.Text = "Yöntem 1: Mouse ile Doğrudan Seçim (Önerilen)";
        grpMouseSelection.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpMouseSelection.Controls.Add(lblMouseOption);
        grpMouseSelection.Controls.Add(btnSelectWithMouse);

        //
        // lblMouseOption
        //
        lblMouseOption.AutoSize = false;
        lblMouseOption.Location = new Point(15, 30);
        lblMouseOption.Size = new Size(1010, 40);
        lblMouseOption.Text = "Butona tıklayın, 3 saniye bekleyin ve hedef sayfaya tıklayın.\nSayfa + Container + Element bilgileri otomatik tespit edilir.";
        lblMouseOption.Font = new Font("Segoe UI", 9F);

        //
        // btnSelectWithMouse
        //
        btnSelectWithMouse.Location = new Point(15, 75);
        btnSelectWithMouse.Size = new Size(220, 35);
        btnSelectWithMouse.Text = "🖱️ Mouse ile Sayfa Seç";
        btnSelectWithMouse.Font = new Font("Segoe UI", 10F);
        btnSelectWithMouse.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnSelectWithMouse.ForeColor = System.Drawing.Color.White;
        btnSelectWithMouse.FlatStyle = FlatStyle.Flat;
        btnSelectWithMouse.Click += BtnSelectWithMouse_Click;

        //
        // lblOrSeparator
        //
        lblOrSeparator.AutoSize = false;
        lblOrSeparator.Location = new Point(20, 210);
        lblOrSeparator.Size = new Size(1040, 30);
        lblOrSeparator.Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ VEYA ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
        lblOrSeparator.TextAlign = ContentAlignment.MiddleCenter;
        lblOrSeparator.Font = new Font("Segoe UI", 9F);
        lblOrSeparator.ForeColor = System.Drawing.Color.Gray;

        //
        // grpListSelection
        //
        grpListSelection.Location = new Point(20, 250);
        grpListSelection.Size = new Size(1040, 150);
        grpListSelection.Text = "Yöntem 2: Açık Pencereler Listesinden Seç";
        grpListSelection.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpListSelection.Controls.Add(lblListOption);
        grpListSelection.Controls.Add(cmbWindows);
        grpListSelection.Controls.Add(btnSelectFromList);
        grpListSelection.Controls.Add(btnRefresh);

        //
        // lblListOption
        //
        lblListOption.AutoSize = false;
        lblListOption.Location = new Point(15, 30);
        lblListOption.Size = new Size(1010, 25);
        lblListOption.Text = "Açık pencereler arasından hedef sayfayı seçin:";
        lblListOption.Font = new Font("Segoe UI", 9F);

        //
        // cmbWindows
        //
        cmbWindows.Location = new Point(15, 60);
        cmbWindows.Size = new Size(890, 25);
        cmbWindows.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbWindows.Font = new Font("Segoe UI", 9F);

        //
        // btnRefresh
        //
        btnRefresh.Location = new Point(915, 58);
        btnRefresh.Size = new Size(110, 29);
        btnRefresh.Text = "🔄 Yenile";
        btnRefresh.Font = new Font("Segoe UI", 9F);
        btnRefresh.Click += BtnRefresh_Click;

        //
        // btnSelectFromList
        //
        btnSelectFromList.Location = new Point(15, 100);
        btnSelectFromList.Size = new Size(220, 35);
        btnSelectFromList.Text = "✓ Listeden Seç";
        btnSelectFromList.Font = new Font("Segoe UI", 10F);
        btnSelectFromList.Click += BtnSelectFromList_Click;

        //
        // grpSelectedPage
        //
        grpSelectedPage.Location = new Point(20, 420);
        grpSelectedPage.Size = new Size(1040, 120);
        grpSelectedPage.Text = "Seçili Hedef Sayfa";
        grpSelectedPage.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpSelectedPage.Controls.Add(lblSelectedPage);

        //
        // lblSelectedPage
        //
        lblSelectedPage.AutoSize = false;
        lblSelectedPage.Location = new Point(15, 30);
        lblSelectedPage.Size = new Size(1010, 75);
        lblSelectedPage.Text = "❌ Henüz sayfa seçilmedi.\nYukarıdaki yöntemlerden birini kullanarak hedef sayfayı seçin.";
        lblSelectedPage.Font = new Font("Segoe UI", 10F);
        lblSelectedPage.ForeColor = System.Drawing.Color.Gray;
        lblSelectedPage.TextAlign = ContentAlignment.TopLeft;

        // Add controls
        this.Controls.Add(lblInstruction);
        this.Controls.Add(grpMouseSelection);
        this.Controls.Add(lblOrSeparator);
        this.Controls.Add(grpListSelection);
        this.Controls.Add(grpSelectedPage);

        grpMouseSelection.ResumeLayout(false);
        grpListSelection.ResumeLayout(false);
        grpSelectedPage.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Label lblInstruction;
    private Label lblMouseOption;
    private Label lblOrSeparator;
    private Label lblListOption;
    private Label lblSelectedPage;
    private Button btnSelectWithMouse;
    private Button btnSelectFromList;
    private Button btnRefresh;
    private ComboBox cmbWindows;
    private GroupBox grpMouseSelection;
    private GroupBox grpListSelection;
    private GroupBox grpSelectedPage;
}
