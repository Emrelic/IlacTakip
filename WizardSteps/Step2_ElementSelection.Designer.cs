namespace MedulaOtomasyon;

partial class Step2_ElementSelection
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
        lblPageInfo = new Label();
        lblInstruction = new Label();
        btnSelectWithMouse = new Button();
        lblSelectedElement = new Label();

        SuspendLayout();

        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.White;
        this.Size = new Size(1080, 560);

        lblPageInfo.Location = new Point(20, 20);
        lblPageInfo.Size = new Size(1040, 30);
        lblPageInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        lblInstruction.Location = new Point(20, 60);
        lblInstruction.Size = new Size(1040, 50);
        lblInstruction.Text = "Koşul kontrol edeceğiniz UI elementini seçin.\nMouse ile doğrudan element'e tıklayın.";
        lblInstruction.Font = new Font("Segoe UI", 10F);

        btnSelectWithMouse.Location = new Point(20, 130);
        btnSelectWithMouse.Size = new Size(250, 40);
        btnSelectWithMouse.Text = "🎯 Mouse ile Element Seç";
        btnSelectWithMouse.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnSelectWithMouse.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnSelectWithMouse.ForeColor = System.Drawing.Color.White;
        btnSelectWithMouse.FlatStyle = FlatStyle.Flat;
        btnSelectWithMouse.Click += BtnSelectWithMouse_Click;

        lblSelectedElement.Location = new Point(20, 200);
        lblSelectedElement.Size = new Size(1040, 100);
        lblSelectedElement.Text = "❌ Henüz element seçilmedi.";
        lblSelectedElement.Font = new Font("Segoe UI", 10F);
        lblSelectedElement.ForeColor = System.Drawing.Color.Gray;

        this.Controls.Add(lblPageInfo);
        this.Controls.Add(lblInstruction);
        this.Controls.Add(btnSelectWithMouse);
        this.Controls.Add(lblSelectedElement);

        ResumeLayout(false);
    }

    private Label lblPageInfo;
    private Label lblInstruction;
    private Button btnSelectWithMouse;
    private Label lblSelectedElement;
}
