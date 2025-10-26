namespace MedulaOtomasyon;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        btnAGrubu = new Button();
        btnBGrubu = new Button();
        btnCGrubu = new Button();
        btnDebug = new Button();
        btnTopmost = new Button();
        btnTaskChainRecorder = new Button();
        btnTaskChainPlayer = new Button();
        lblStatus = new Label();
        txtLog = new TextBox();
        SuspendLayout();
        //
        // btnAGrubu
        //
        btnAGrubu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        btnAGrubu.Location = new Point(50, 50);
        btnAGrubu.Name = "btnAGrubu";
        btnAGrubu.Size = new Size(200, 80);
        btnAGrubu.TabIndex = 0;
        btnAGrubu.Text = "A Grubu";
        btnAGrubu.UseVisualStyleBackColor = true;
        btnAGrubu.Click += btnAGrubu_Click;
        //
        // btnBGrubu
        //
        btnBGrubu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        btnBGrubu.Location = new Point(300, 50);
        btnBGrubu.Name = "btnBGrubu";
        btnBGrubu.Size = new Size(200, 80);
        btnBGrubu.TabIndex = 1;
        btnBGrubu.Text = "B Grubu";
        btnBGrubu.UseVisualStyleBackColor = true;
        btnBGrubu.Click += btnBGrubu_Click;
        //
        // btnCGrubu
        //
        btnCGrubu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        btnCGrubu.Location = new Point(550, 50);
        btnCGrubu.Name = "btnCGrubu";
        btnCGrubu.Size = new Size(200, 80);
        btnCGrubu.TabIndex = 2;
        btnCGrubu.Text = "C Grubu";
        btnCGrubu.UseVisualStyleBackColor = true;
        btnCGrubu.Click += btnCGrubu_Click;
        //
        // btnDebug
        //
        btnDebug.Font = new Font("Segoe UI", 9F);
        btnDebug.Location = new Point(650, 150);
        btnDebug.Name = "btnDebug";
        btnDebug.Size = new Size(100, 30);
        btnDebug.TabIndex = 5;
        btnDebug.Text = "Debug";
        btnDebug.UseVisualStyleBackColor = true;
        btnDebug.Click += btnDebug_Click;
        //
        // btnTopmost
        //
        btnTopmost.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnTopmost.Location = new Point(530, 150);
        btnTopmost.Name = "btnTopmost";
        btnTopmost.Size = new Size(110, 30);
        btnTopmost.TabIndex = 8;
        btnTopmost.Text = "📌 En Üstte Tut";
        btnTopmost.UseVisualStyleBackColor = true;
        btnTopmost.Click += btnTopmost_Click;
        //
        // btnTaskChainRecorder
        //
        btnTaskChainRecorder.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnTaskChainRecorder.Location = new Point(300, 550);
        btnTaskChainRecorder.Name = "btnTaskChainRecorder";
        btnTaskChainRecorder.Size = new Size(200, 40);
        btnTaskChainRecorder.TabIndex = 6;
        btnTaskChainRecorder.Text = "📝 Görev Zinciri Kaydet";
        btnTaskChainRecorder.UseVisualStyleBackColor = true;
        btnTaskChainRecorder.Click += btnTaskChainRecorder_Click;
        //
        // btnTaskChainPlayer
        //
        btnTaskChainPlayer.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnTaskChainPlayer.Location = new Point(510, 550);
        btnTaskChainPlayer.Name = "btnTaskChainPlayer";
        btnTaskChainPlayer.Size = new Size(200, 40);
        btnTaskChainPlayer.TabIndex = 7;
        btnTaskChainPlayer.Text = "▶ Görev Zinciri Oynat";
        btnTaskChainPlayer.UseVisualStyleBackColor = true;
        btnTaskChainPlayer.Click += btnTaskChainPlayer_Click;
        //
        // lblStatus
        //
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 10F);
        lblStatus.Location = new Point(50, 150);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(90, 19);
        lblStatus.TabIndex = 3;
        lblStatus.Text = "Durum: Hazır";
        //
        // txtLog
        //
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.Location = new Point(50, 180);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(700, 350);
        txtLog.TabIndex = 4;
        //
        // Form1
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 610);
        Controls.Add(btnTopmost);
        Controls.Add(btnTaskChainPlayer);
        Controls.Add(btnTaskChainRecorder);
        Controls.Add(txtLog);
        Controls.Add(btnDebug);
        Controls.Add(lblStatus);
        Controls.Add(btnCGrubu);
        Controls.Add(btnBGrubu);
        Controls.Add(btnAGrubu);
        Name = "Form1";
        Text = "Medula Reçete Otomasyonu";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button btnAGrubu;
    private Button btnBGrubu;
    private Button btnCGrubu;
    private Button btnDebug;
    private Button btnTopmost;
    private Button btnTaskChainRecorder;
    private Button btnTaskChainPlayer;
    private Label lblStatus;
    private TextBox txtLog;
}
