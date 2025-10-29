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
            btnEditChain = new Button();
            grpSteps = new GroupBox();
            lstSteps = new ListBox();
            grpControls = new GroupBox();
            btnPlay = new Button();
            btnPause = new Button();
            btnStop = new Button();
            btnStopAndEdit = new Button();
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
            grpChainList.Controls.Add(btnEditChain);
            grpChainList.Controls.Add(btnRefresh);
            grpChainList.Controls.Add(lstChains);
            grpChainList.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpChainList.Location = new Point(12, 12);
            grpChainList.Name = "grpChainList";
            grpChainList.Size = new Size(450, 380);
            grpChainList.TabIndex = 0;
            grpChainList.TabStop = false;
            grpChainList.Text = "Kaydedilmiş Görev Zincirleri";
            //
            // lstChains
            //
            lstChains.Font = new Font("Segoe UI", 9.5F);
            lstChains.FormattingEnabled = true;
            lstChains.ItemHeight = 17;
            lstChains.Location = new Point(10, 28);
            lstChains.Name = "lstChains";
            lstChains.Size = new Size(430, 278);
            lstChains.TabIndex = 0;
            lstChains.SelectedIndexChanged += lstChains_SelectedIndexChanged;
            //
            // btnRefresh
            //
            btnRefresh.Font = new Font("Segoe UI", 10F);
            btnRefresh.Location = new Point(10, 315);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(210, 35);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Yenile";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            //
            // btnEditChain
            //
            btnEditChain.Font = new Font("Segoe UI", 10F);
            btnEditChain.Location = new Point(230, 315);
            btnEditChain.Name = "btnEditChain";
            btnEditChain.Size = new Size(210, 35);
            btnEditChain.TabIndex = 2;
            btnEditChain.Text = "✏️ Zinciri Düzenle";
            btnEditChain.UseVisualStyleBackColor = true;
            btnEditChain.Click += btnEditChain_Click;
            //
            // grpSteps
            //
            grpSteps.Controls.Add(lstSteps);
            grpSteps.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpSteps.Location = new Point(12, 402);
            grpSteps.Name = "grpSteps";
            grpSteps.Size = new Size(450, 280);
            grpSteps.TabIndex = 1;
            grpSteps.TabStop = false;
            grpSteps.Text = "Seçili Zincir Adımları";
            //
            // lstSteps
            //
            lstSteps.Font = new Font("Consolas", 9F);
            lstSteps.FormattingEnabled = true;
            lstSteps.ItemHeight = 16;
            lstSteps.Location = new Point(10, 28);
            lstSteps.Name = "lstSteps";
            lstSteps.Size = new Size(430, 240);
            lstSteps.TabIndex = 0;
            //
            // grpControls
            //
            grpControls.Controls.Add(btnStopAndEdit);
            grpControls.Controls.Add(btnDebug);
            grpControls.Controls.Add(btnStop);
            grpControls.Controls.Add(btnPause);
            grpControls.Controls.Add(btnPlay);
            grpControls.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpControls.Location = new Point(12, 692);
            grpControls.Name = "grpControls";
            grpControls.Size = new Size(450, 130);
            grpControls.TabIndex = 2;
            grpControls.TabStop = false;
            grpControls.Text = "Kontroller";
            //
            // btnPlay
            //
            btnPlay.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnPlay.Location = new Point(10, 28);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(100, 48);
            btnPlay.TabIndex = 0;
            btnPlay.Text = "▶ Oynat";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            //
            // btnPause
            //
            btnPause.Enabled = false;
            btnPause.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnPause.Location = new Point(120, 28);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(100, 48);
            btnPause.TabIndex = 1;
            btnPause.Text = "⏸ Duraklat";
            btnPause.UseVisualStyleBackColor = true;
            btnPause.Click += btnPause_Click;
            //
            // btnStop
            //
            btnStop.Enabled = false;
            btnStop.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnStop.Location = new Point(230, 28);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(100, 48);
            btnStop.TabIndex = 2;
            btnStop.Text = "⏹ Durdur";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            //
            // btnDebug
            //
            btnDebug.Font = new Font("Segoe UI", 10F);
            btnDebug.Location = new Point(340, 28);
            btnDebug.Name = "btnDebug";
            btnDebug.Size = new Size(100, 48);
            btnDebug.TabIndex = 3;
            btnDebug.Text = "🐛 Debug";
            btnDebug.UseVisualStyleBackColor = true;
            btnDebug.Click += btnDebug_Click;
            //
            // btnStopAndEdit
            //
            btnStopAndEdit.BackColor = Color.FromArgb(255, 140, 0);
            btnStopAndEdit.Enabled = false;
            btnStopAndEdit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStopAndEdit.ForeColor = Color.White;
            btnStopAndEdit.Location = new Point(10, 82);
            btnStopAndEdit.Name = "btnStopAndEdit";
            btnStopAndEdit.Size = new Size(430, 40);
            btnStopAndEdit.TabIndex = 4;
            btnStopAndEdit.Text = "⏹ Durdur ve Düzenle";
            btnStopAndEdit.UseVisualStyleBackColor = false;
            btnStopAndEdit.Click += btnStopAndEdit_Click;
            //
            // grpSpeed
            //
            grpSpeed.Controls.Add(rbFast);
            grpSpeed.Controls.Add(rbNormal);
            grpSpeed.Controls.Add(rbSlow);
            grpSpeed.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpSpeed.Location = new Point(12, 832);
            grpSpeed.Name = "grpSpeed";
            grpSpeed.Size = new Size(220, 105);
            grpSpeed.TabIndex = 3;
            grpSpeed.TabStop = false;
            grpSpeed.Text = "Hız";
            //
            // rbSlow
            //
            rbSlow.AutoSize = true;
            rbSlow.Font = new Font("Segoe UI", 10F);
            rbSlow.Location = new Point(10, 28);
            rbSlow.Name = "rbSlow";
            rbSlow.Size = new Size(140, 23);
            rbSlow.TabIndex = 0;
            rbSlow.Text = "🐌 Yavaş (2000ms)";
            rbSlow.UseVisualStyleBackColor = true;
            //
            // rbNormal
            //
            rbNormal.AutoSize = true;
            rbNormal.Checked = true;
            rbNormal.Font = new Font("Segoe UI", 10F);
            rbNormal.Location = new Point(10, 52);
            rbNormal.Name = "rbNormal";
            rbNormal.Size = new Size(159, 23);
            rbNormal.TabIndex = 1;
            rbNormal.TabStop = true;
            rbNormal.Text = "🚶 Normal (1000ms)";
            rbNormal.UseVisualStyleBackColor = true;
            //
            // rbFast
            //
            rbFast.AutoSize = true;
            rbFast.Font = new Font("Segoe UI", 10F);
            rbFast.Location = new Point(10, 76);
            rbFast.Name = "rbFast";
            rbFast.Size = new Size(105, 23);
            rbFast.TabIndex = 2;
            rbFast.Text = "🚀 Hızlı (0ms)";
            rbFast.UseVisualStyleBackColor = true;
            //
            // grpErrorHandling
            //
            grpErrorHandling.Controls.Add(rbSkip);
            grpErrorHandling.Controls.Add(rbRetry);
            grpErrorHandling.Controls.Add(rbStop);
            grpErrorHandling.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpErrorHandling.Location = new Point(242, 832);
            grpErrorHandling.Name = "grpErrorHandling";
            grpErrorHandling.Size = new Size(220, 105);
            grpErrorHandling.TabIndex = 4;
            grpErrorHandling.TabStop = false;
            grpErrorHandling.Text = "Hata Durumunda";
            //
            // rbStop
            //
            rbStop.AutoSize = true;
            rbStop.Checked = true;
            rbStop.Font = new Font("Segoe UI", 10F);
            rbStop.Location = new Point(10, 28);
            rbStop.Name = "rbStop";
            rbStop.Size = new Size(93, 23);
            rbStop.TabIndex = 0;
            rbStop.TabStop = true;
            rbStop.Text = "⏹ Durdur";
            rbStop.UseVisualStyleBackColor = true;
            //
            // rbRetry
            //
            rbRetry.AutoSize = true;
            rbRetry.Font = new Font("Segoe UI", 10F);
            rbRetry.Location = new Point(10, 52);
            rbRetry.Name = "rbRetry";
            rbRetry.Size = new Size(130, 23);
            rbRetry.TabIndex = 1;
            rbRetry.Text = "🔄 Tekrar Dene";
            rbRetry.UseVisualStyleBackColor = true;
            //
            // rbSkip
            //
            rbSkip.AutoSize = true;
            rbSkip.Font = new Font("Segoe UI", 10F);
            rbSkip.Location = new Point(10, 76);
            rbSkip.Name = "rbSkip";
            rbSkip.Size = new Size(76, 23);
            rbSkip.TabIndex = 2;
            rbSkip.Text = "⏭ Atla";
            rbSkip.UseVisualStyleBackColor = true;
            //
            // grpOptions
            //
            grpOptions.Controls.Add(chkSaveLog);
            grpOptions.Controls.Add(chkScreenshot);
            grpOptions.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpOptions.Location = new Point(12, 947);
            grpOptions.Name = "grpOptions";
            grpOptions.Size = new Size(450, 90);
            grpOptions.TabIndex = 5;
            grpOptions.TabStop = false;
            grpOptions.Text = "Seçenekler";
            //
            // chkScreenshot
            //
            chkScreenshot.AutoSize = true;
            chkScreenshot.Font = new Font("Segoe UI", 10F);
            chkScreenshot.Location = new Point(10, 28);
            chkScreenshot.Name = "chkScreenshot";
            chkScreenshot.Size = new Size(215, 23);
            chkScreenshot.TabIndex = 0;
            chkScreenshot.Text = "📸 Her adımda screenshot al";
            chkScreenshot.UseVisualStyleBackColor = true;
            //
            // chkSaveLog
            //
            chkSaveLog.AutoSize = true;
            chkSaveLog.Checked = true;
            chkSaveLog.CheckState = CheckState.Checked;
            chkSaveLog.Font = new Font("Segoe UI", 10F);
            chkSaveLog.Location = new Point(10, 54);
            chkSaveLog.Name = "chkSaveLog";
            chkSaveLog.Size = new Size(173, 23);
            chkSaveLog.TabIndex = 1;
            chkSaveLog.Text = "💾 Execution log kaydet";
            chkSaveLog.UseVisualStyleBackColor = true;
            //
            // grpProgress
            //
            grpProgress.Controls.Add(lblProgress);
            grpProgress.Controls.Add(progressBar);
            grpProgress.Controls.Add(lblCurrentStep);
            grpProgress.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpProgress.Location = new Point(12, 1047);
            grpProgress.Name = "grpProgress";
            grpProgress.Size = new Size(450, 105);
            grpProgress.TabIndex = 6;
            grpProgress.TabStop = false;
            grpProgress.Text = "İlerleme";
            //
            // lblCurrentStep
            //
            lblCurrentStep.AutoSize = true;
            lblCurrentStep.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCurrentStep.Location = new Point(10, 28);
            lblCurrentStep.Name = "lblCurrentStep";
            lblCurrentStep.Size = new Size(131, 19);
            lblCurrentStep.TabIndex = 0;
            lblCurrentStep.Text = "Şu anda: Hazır...";
            //
            // progressBar
            //
            progressBar.Location = new Point(10, 50);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(430, 26);
            progressBar.TabIndex = 1;
            //
            // lblProgress
            //
            lblProgress.AutoSize = true;
            lblProgress.Font = new Font("Segoe UI", 10F);
            lblProgress.Location = new Point(10, 79);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(108, 19);
            lblProgress.TabIndex = 2;
            lblProgress.Text = "Progress: 0/0 (0%)";
            //
            // grpLog
            //
            grpLog.Controls.Add(btnClearLog);
            grpLog.Controls.Add(txtLog);
            grpLog.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpLog.Location = new Point(12, 1162);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(450, 200);
            grpLog.TabIndex = 7;
            grpLog.TabStop = false;
            grpLog.Text = "Execution Log";
            //
            // txtLog
            //
            txtLog.BackColor = Color.Black;
            txtLog.Font = new Font("Consolas", 9.5F);
            txtLog.ForeColor = Color.LightGreen;
            txtLog.Location = new Point(10, 28);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(430, 130);
            txtLog.TabIndex = 0;
            //
            // btnClearLog
            //
            btnClearLog.Font = new Font("Segoe UI", 10F);
            btnClearLog.Location = new Point(10, 165);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(120, 28);
            btnClearLog.TabIndex = 1;
            btnClearLog.Text = "Temizle";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            //
            // TaskChainPlayerForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(490, 1060);
            Controls.Add(grpLog);
            Controls.Add(grpProgress);
            Controls.Add(grpOptions);
            Controls.Add(grpErrorHandling);
            Controls.Add(grpSpeed);
            Controls.Add(grpControls);
            Controls.Add(grpSteps);
            Controls.Add(grpChainList);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            MinimumSize = new Size(490, 600);
            Name = "TaskChainPlayerForm";
            StartPosition = FormStartPosition.Manual;
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
        private Button btnEditChain;
        private GroupBox grpSteps;
        private ListBox lstSteps;
        private GroupBox grpControls;
        private Button btnPlay;
        private Button btnPause;
        private Button btnStop;
        private Button btnStopAndEdit;
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
