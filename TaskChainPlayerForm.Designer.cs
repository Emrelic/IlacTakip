namespace MedulaOtomasyon
{
    partial class TaskChainPlayerForm
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
            grpChainList = new GroupBox();
            lstChains = new ListBox();
            btnRefresh = new Button();
            grpSteps = new GroupBox();
            lstSteps = new ListBox();
            grpControls = new GroupBox();
            btnPlay = new Button();
            btnPause = new Button();
            btnStop = new Button();
            btnDebug = new Button();
            grpSpeed = new GroupBox();
            rbSlow = new RadioButton();
            rbNormal = new RadioButton();
            rbFast = new RadioButton();
            grpErrorHandling = new GroupBox();
            rbStop = new RadioButton();
            rbRetry = new RadioButton();
            rbSkip = new RadioButton();
            grpOptions = new GroupBox();
            chkScreenshot = new CheckBox();
            chkSaveLog = new CheckBox();
            grpProgress = new GroupBox();
            lblCurrentStep = new Label();
            progressBar = new ProgressBar();
            lblProgress = new Label();
            grpLog = new GroupBox();
            txtLog = new TextBox();
            btnClearLog = new Button();
            grpChainList.SuspendLayout();
            grpSteps.SuspendLayout();
            grpControls.SuspendLayout();
            grpSpeed.SuspendLayout();
            grpErrorHandling.SuspendLayout();
            grpOptions.SuspendLayout();
            grpProgress.SuspendLayout();
            grpLog.SuspendLayout();
            SuspendLayout();
            //
            // grpChainList
            //
            grpChainList.Controls.Add(btnRefresh);
            grpChainList.Controls.Add(lstChains);
            grpChainList.Location = new Point(12, 12);
            grpChainList.Name = "grpChainList";
            grpChainList.Size = new Size(320, 350);
            grpChainList.TabIndex = 0;
            grpChainList.TabStop = false;
            grpChainList.Text = "Kaydedilmiş Görev Zincirleri";
            //
            // lstChains
            //
            lstChains.FormattingEnabled = true;
            lstChains.ItemHeight = 15;
            lstChains.Location = new Point(6, 22);
            lstChains.Name = "lstChains";
            lstChains.Size = new Size(308, 274);
            lstChains.TabIndex = 0;
            lstChains.SelectedIndexChanged += lstChains_SelectedIndexChanged;
            //
            // btnRefresh
            //
            btnRefresh.Location = new Point(6, 307);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(308, 30);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Yenile";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            //
            // grpSteps
            //
            grpSteps.Controls.Add(lstSteps);
            grpSteps.Location = new Point(338, 12);
            grpSteps.Name = "grpSteps";
            grpSteps.Size = new Size(450, 350);
            grpSteps.TabIndex = 1;
            grpSteps.TabStop = false;
            grpSteps.Text = "Seçili Zincir Adımları";
            //
            // lstSteps
            //
            lstSteps.FormattingEnabled = true;
            lstSteps.ItemHeight = 15;
            lstSteps.Location = new Point(6, 22);
            lstSteps.Name = "lstSteps";
            lstSteps.Size = new Size(438, 319);
            lstSteps.TabIndex = 0;
            //
            // grpControls
            //
            grpControls.Controls.Add(btnDebug);
            grpControls.Controls.Add(btnStop);
            grpControls.Controls.Add(btnPause);
            grpControls.Controls.Add(btnPlay);
            grpControls.Location = new Point(12, 368);
            grpControls.Name = "grpControls";
            grpControls.Size = new Size(320, 90);
            grpControls.TabIndex = 2;
            grpControls.TabStop = false;
            grpControls.Text = "Kontroller";
            //
            // btnPlay
            //
            btnPlay.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPlay.Location = new Point(6, 22);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(75, 55);
            btnPlay.TabIndex = 0;
            btnPlay.Text = "▶ Oynat";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            //
            // btnPause
            //
            btnPause.Enabled = false;
            btnPause.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPause.Location = new Point(87, 22);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(75, 55);
            btnPause.TabIndex = 1;
            btnPause.Text = "⏸ Duraklat";
            btnPause.UseVisualStyleBackColor = true;
            btnPause.Click += btnPause_Click;
            //
            // btnStop
            //
            btnStop.Enabled = false;
            btnStop.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnStop.Location = new Point(168, 22);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(70, 55);
            btnStop.TabIndex = 2;
            btnStop.Text = "⏹ Durdur";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            //
            // btnDebug
            //
            btnDebug.Font = new Font("Segoe UI", 10F);
            btnDebug.Location = new Point(244, 22);
            btnDebug.Name = "btnDebug";
            btnDebug.Size = new Size(70, 55);
            btnDebug.TabIndex = 3;
            btnDebug.Text = "🐛 Debug";
            btnDebug.UseVisualStyleBackColor = true;
            btnDebug.Click += btnDebug_Click;
            //
            // grpSpeed
            //
            grpSpeed.Controls.Add(rbFast);
            grpSpeed.Controls.Add(rbNormal);
            grpSpeed.Controls.Add(rbSlow);
            grpSpeed.Location = new Point(12, 464);
            grpSpeed.Name = "grpSpeed";
            grpSpeed.Size = new Size(155, 90);
            grpSpeed.TabIndex = 3;
            grpSpeed.TabStop = false;
            grpSpeed.Text = "Hız";
            //
            // rbSlow
            //
            rbSlow.AutoSize = true;
            rbSlow.Location = new Point(6, 22);
            rbSlow.Name = "rbSlow";
            rbSlow.Size = new Size(114, 19);
            rbSlow.TabIndex = 0;
            rbSlow.Text = "🐌 Yavaş (2000ms)";
            rbSlow.UseVisualStyleBackColor = true;
            //
            // rbNormal
            //
            rbNormal.AutoSize = true;
            rbNormal.Checked = true;
            rbNormal.Location = new Point(6, 44);
            rbNormal.Name = "rbNormal";
            rbNormal.Size = new Size(130, 19);
            rbNormal.TabIndex = 1;
            rbNormal.TabStop = true;
            rbNormal.Text = "🚶 Normal (1000ms)";
            rbNormal.UseVisualStyleBackColor = true;
            //
            // rbFast
            //
            rbFast.AutoSize = true;
            rbFast.Location = new Point(6, 66);
            rbFast.Name = "rbFast";
            rbFast.Size = new Size(85, 19);
            rbFast.TabIndex = 2;
            rbFast.Text = "🚀 Hızlı (0ms)";
            rbFast.UseVisualStyleBackColor = true;
            //
            // grpErrorHandling
            //
            grpErrorHandling.Controls.Add(rbSkip);
            grpErrorHandling.Controls.Add(rbRetry);
            grpErrorHandling.Controls.Add(rbStop);
            grpErrorHandling.Location = new Point(173, 464);
            grpErrorHandling.Name = "grpErrorHandling";
            grpErrorHandling.Size = new Size(159, 90);
            grpErrorHandling.TabIndex = 4;
            grpErrorHandling.TabStop = false;
            grpErrorHandling.Text = "Hata Durumunda";
            //
            // rbStop
            //
            rbStop.AutoSize = true;
            rbStop.Checked = true;
            rbStop.Location = new Point(6, 22);
            rbStop.Name = "rbStop";
            rbStop.Size = new Size(76, 19);
            rbStop.TabIndex = 0;
            rbStop.TabStop = true;
            rbStop.Text = "⏹ Durdur";
            rbStop.UseVisualStyleBackColor = true;
            //
            // rbRetry
            //
            rbRetry.AutoSize = true;
            rbRetry.Location = new Point(6, 44);
            rbRetry.Name = "rbRetry";
            rbRetry.Size = new Size(106, 19);
            rbRetry.TabIndex = 1;
            rbRetry.Text = "🔄 Tekrar Dene";
            rbRetry.UseVisualStyleBackColor = true;
            //
            // rbSkip
            //
            rbSkip.AutoSize = true;
            rbSkip.Location = new Point(6, 66);
            rbSkip.Name = "rbSkip";
            rbSkip.Size = new Size(62, 19);
            rbSkip.TabIndex = 2;
            rbSkip.Text = "⏭ Atla";
            rbSkip.UseVisualStyleBackColor = true;
            //
            // grpOptions
            //
            grpOptions.Controls.Add(chkSaveLog);
            grpOptions.Controls.Add(chkScreenshot);
            grpOptions.Location = new Point(338, 368);
            grpOptions.Name = "grpOptions";
            grpOptions.Size = new Size(200, 90);
            grpOptions.TabIndex = 5;
            grpOptions.TabStop = false;
            grpOptions.Text = "Seçenekler";
            //
            // chkScreenshot
            //
            chkScreenshot.AutoSize = true;
            chkScreenshot.Location = new Point(6, 22);
            chkScreenshot.Name = "chkScreenshot";
            chkScreenshot.Size = new Size(176, 19);
            chkScreenshot.TabIndex = 0;
            chkScreenshot.Text = "📸 Her adımda screenshot al";
            chkScreenshot.UseVisualStyleBackColor = true;
            //
            // chkSaveLog
            //
            chkSaveLog.AutoSize = true;
            chkSaveLog.Checked = true;
            chkSaveLog.CheckState = CheckState.Checked;
            chkSaveLog.Location = new Point(6, 44);
            chkSaveLog.Name = "chkSaveLog";
            chkSaveLog.Size = new Size(142, 19);
            chkSaveLog.TabIndex = 1;
            chkSaveLog.Text = "💾 Execution log kaydet";
            chkSaveLog.UseVisualStyleBackColor = true;
            //
            // grpProgress
            //
            grpProgress.Controls.Add(lblProgress);
            grpProgress.Controls.Add(progressBar);
            grpProgress.Controls.Add(lblCurrentStep);
            grpProgress.Location = new Point(544, 368);
            grpProgress.Name = "grpProgress";
            grpProgress.Size = new Size(244, 90);
            grpProgress.TabIndex = 6;
            grpProgress.TabStop = false;
            grpProgress.Text = "İlerleme";
            //
            // lblCurrentStep
            //
            lblCurrentStep.AutoSize = true;
            lblCurrentStep.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCurrentStep.Location = new Point(6, 22);
            lblCurrentStep.Name = "lblCurrentStep";
            lblCurrentStep.Size = new Size(109, 15);
            lblCurrentStep.TabIndex = 0;
            lblCurrentStep.Text = "Şu anda: Hazır...";
            //
            // progressBar
            //
            progressBar.Location = new Point(6, 40);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(232, 23);
            progressBar.TabIndex = 1;
            //
            // lblProgress
            //
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(6, 66);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(88, 15);
            lblProgress.TabIndex = 2;
            lblProgress.Text = "Progress: 0/0 (0%)";
            //
            // grpLog
            //
            grpLog.Controls.Add(btnClearLog);
            grpLog.Controls.Add(txtLog);
            grpLog.Location = new Point(12, 560);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(776, 200);
            grpLog.TabIndex = 7;
            grpLog.TabStop = false;
            grpLog.Text = "Execution Log";
            //
            // txtLog
            //
            txtLog.BackColor = Color.Black;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.ForeColor = Color.LightGreen;
            txtLog.Location = new Point(6, 22);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(764, 136);
            txtLog.TabIndex = 0;
            //
            // btnClearLog
            //
            btnClearLog.Location = new Point(6, 164);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(100, 25);
            btnClearLog.TabIndex = 1;
            btnClearLog.Text = "Temizle";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            //
            // TaskChainPlayerForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 772);
            Controls.Add(grpLog);
            Controls.Add(grpProgress);
            Controls.Add(grpOptions);
            Controls.Add(grpErrorHandling);
            Controls.Add(grpSpeed);
            Controls.Add(grpControls);
            Controls.Add(grpSteps);
            Controls.Add(grpChainList);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "TaskChainPlayerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Görev Zinciri Oynatıcı";
            Load += TaskChainPlayerForm_Load;
            grpChainList.ResumeLayout(false);
            grpSteps.ResumeLayout(false);
            grpControls.ResumeLayout(false);
            grpSpeed.ResumeLayout(false);
            grpSpeed.PerformLayout();
            grpErrorHandling.ResumeLayout(false);
            grpErrorHandling.PerformLayout();
            grpOptions.ResumeLayout(false);
            grpOptions.PerformLayout();
            grpProgress.ResumeLayout(false);
            grpProgress.PerformLayout();
            grpLog.ResumeLayout(false);
            grpLog.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpChainList;
        private ListBox lstChains;
        private Button btnRefresh;
        private GroupBox grpSteps;
        private ListBox lstSteps;
        private GroupBox grpControls;
        private Button btnPlay;
        private Button btnPause;
        private Button btnStop;
        private Button btnDebug;
        private GroupBox grpSpeed;
        private RadioButton rbSlow;
        private RadioButton rbNormal;
        private RadioButton rbFast;
        private GroupBox grpErrorHandling;
        private RadioButton rbStop;
        private RadioButton rbRetry;
        private RadioButton rbSkip;
        private GroupBox grpOptions;
        private CheckBox chkScreenshot;
        private CheckBox chkSaveLog;
        private GroupBox grpProgress;
        private Label lblCurrentStep;
        private ProgressBar progressBar;
        private Label lblProgress;
        private GroupBox grpLog;
        private TextBox txtLog;
        private Button btnClearLog;
    }
}
