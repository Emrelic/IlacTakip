namespace MedulaOtomasyon;

partial class KeyboardInputDialog
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
        // Panels
        pnlHeader = new Panel();
        pnlModifiers = new Panel();
        pnlCombination = new Panel();
        pnlKeys = new Panel();
        pnlTextInput = new Panel();
        pnlPreview = new Panel();
        pnlFooter = new Panel();

        // Header
        lblTitle = new Label();

        // Modifier tuşları
        lblModifiers = new Label();
        chkCtrl = new CheckBox();
        chkShift = new CheckBox();
        chkAlt = new CheckBox();
        chkWin = new CheckBox();

        // Kombinasyon modu
        chkCombinationMode = new CheckBox();
        lblCombinationInfo = new Label();
        lblSelectedKeys = new Label();
        lstSelectedKeys = new ListBox();
        btnRemoveKey = new Button();
        btnClearSelectedKeys = new Button();

        // Özel tuşlar
        lblSpecialKeys = new Label();
        btnEnter = new Button();
        btnTab = new Button();
        btnEsc = new Button();
        btnSpace = new Button();
        btnBackspace = new Button();
        btnDelete = new Button();
        btnHome = new Button();
        btnEnd = new Button();
        btnPageUp = new Button();
        btnPageDown = new Button();
        btnInsert = new Button();
        btnPrintScreen = new Button();

        // Ok tuşları
        lblArrowKeys = new Label();
        btnUp = new Button();
        btnDown = new Button();
        btnLeft = new Button();
        btnRight = new Button();

        // F tuşları
        lblFunctionKeys = new Label();
        pnlFKeys = new Panel();

        // Harfler
        lblLetters = new Label();
        pnlLetters = new Panel();

        // Rakamlar
        lblNumbers = new Label();
        pnlNumbers = new Panel();

        // Noktalama ve özel karakterler
        lblPunctuation = new Label();
        pnlPunctuation = new Panel();

        // Metin girişi
        lblTextInput = new Label();
        txtTextInput = new TextBox();
        btnAddText = new Button();
        btnClearText = new Button();

        // Önizleme
        lblPreview = new Label();
        txtPreview = new TextBox();
        btnClear = new Button();

        // Footer
        btnOK = new Button();
        btnCancel = new Button();

        SuspendLayout();

        // ==========================================
        // FORM
        // ==========================================
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 700);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Klavye Tuşları Seçimi";
        BackColor = Color.White;

        // ==========================================
        // HEADER PANEL
        // ==========================================
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 50;
        pnlHeader.BackColor = Color.FromArgb(45, 45, 48);
        pnlHeader.Controls.Add(lblTitle);

        lblTitle.Text = "⌨️ Klavye Tuşları Seçimi";
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Dock = DockStyle.Fill;
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;

        // ==========================================
        // MODIFIER PANEL
        // ==========================================
        pnlModifiers.Location = new Point(10, 60);
        pnlModifiers.Size = new Size(780, 80);
        pnlModifiers.BackColor = Color.FromArgb(240, 240, 240);
        pnlModifiers.BorderStyle = BorderStyle.FixedSingle;

        lblModifiers.Text = "Modifier Tuşları (Basılı Tutulabilir):";
        lblModifiers.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblModifiers.Location = new Point(10, 5);
        lblModifiers.Size = new Size(300, 25);

        chkCtrl.Text = "Ctrl";
        chkCtrl.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        chkCtrl.Location = new Point(20, 35);
        chkCtrl.Size = new Size(150, 30);
        chkCtrl.Appearance = Appearance.Button;
        chkCtrl.TextAlign = ContentAlignment.MiddleCenter;
        chkCtrl.BackColor = Color.White;
        chkCtrl.FlatStyle = FlatStyle.Flat;
        chkCtrl.CheckedChanged += ModifierKey_CheckedChanged;

        chkShift.Text = "Shift";
        chkShift.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        chkShift.Location = new Point(180, 35);
        chkShift.Size = new Size(150, 30);
        chkShift.Appearance = Appearance.Button;
        chkShift.TextAlign = ContentAlignment.MiddleCenter;
        chkShift.BackColor = Color.White;
        chkShift.FlatStyle = FlatStyle.Flat;
        chkShift.CheckedChanged += ModifierKey_CheckedChanged;

        chkAlt.Text = "Alt";
        chkAlt.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        chkAlt.Location = new Point(340, 35);
        chkAlt.Size = new Size(150, 30);
        chkAlt.Appearance = Appearance.Button;
        chkAlt.TextAlign = ContentAlignment.MiddleCenter;
        chkAlt.BackColor = Color.White;
        chkAlt.FlatStyle = FlatStyle.Flat;
        chkAlt.CheckedChanged += ModifierKey_CheckedChanged;

        chkWin.Text = "Win";
        chkWin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        chkWin.Location = new Point(500, 35);
        chkWin.Size = new Size(150, 30);
        chkWin.Appearance = Appearance.Button;
        chkWin.TextAlign = ContentAlignment.MiddleCenter;
        chkWin.BackColor = Color.White;
        chkWin.FlatStyle = FlatStyle.Flat;
        chkWin.CheckedChanged += ModifierKey_CheckedChanged;

        pnlModifiers.Controls.Add(lblModifiers);
        pnlModifiers.Controls.Add(chkCtrl);
        pnlModifiers.Controls.Add(chkShift);
        pnlModifiers.Controls.Add(chkAlt);
        pnlModifiers.Controls.Add(chkWin);

        // ==========================================
        // COMBINATION PANEL (Multi-key selection)
        // ==========================================
        pnlCombination.Location = new Point(800, 60);
        pnlCombination.Size = new Size(190, 530);
        pnlCombination.BackColor = Color.FromArgb(240, 248, 255);
        pnlCombination.BorderStyle = BorderStyle.FixedSingle;

        chkCombinationMode.Text = "Kombinasyon Modu";
        chkCombinationMode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        chkCombinationMode.Location = new Point(10, 10);
        chkCombinationMode.Size = new Size(170, 25);
        chkCombinationMode.CheckedChanged += chkCombinationMode_CheckedChanged;

        lblCombinationInfo.Text = "Aynı anda basılacak\ntuşları seçin (Max: 8)";
        lblCombinationInfo.Font = new Font("Segoe UI", 8.5F);
        lblCombinationInfo.ForeColor = Color.FromArgb(70, 70, 70);
        lblCombinationInfo.Location = new Point(10, 40);
        lblCombinationInfo.Size = new Size(170, 35);
        lblCombinationInfo.TextAlign = ContentAlignment.TopLeft;
        lblCombinationInfo.Visible = false;

        lblSelectedKeys.Text = "Seçili Tuşlar:";
        lblSelectedKeys.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSelectedKeys.Location = new Point(10, 80);
        lblSelectedKeys.Size = new Size(170, 20);
        lblSelectedKeys.Visible = false;

        lstSelectedKeys.Font = new Font("Consolas", 9F);
        lstSelectedKeys.Location = new Point(10, 105);
        lstSelectedKeys.Size = new Size(170, 340);
        lstSelectedKeys.BackColor = Color.White;
        lstSelectedKeys.BorderStyle = BorderStyle.FixedSingle;
        lstSelectedKeys.Visible = false;

        btnRemoveKey.Text = "❌ Kaldır";
        btnRemoveKey.Font = new Font("Segoe UI", 9F);
        btnRemoveKey.Location = new Point(10, 450);
        btnRemoveKey.Size = new Size(170, 32);
        btnRemoveKey.BackColor = Color.FromArgb(220, 53, 69);
        btnRemoveKey.ForeColor = Color.White;
        btnRemoveKey.FlatStyle = FlatStyle.Flat;
        btnRemoveKey.FlatAppearance.BorderSize = 0;
        btnRemoveKey.Cursor = Cursors.Hand;
        btnRemoveKey.Visible = false;
        btnRemoveKey.Click += btnRemoveKey_Click;

        btnClearSelectedKeys.Text = "🗑️ Tümünü Temizle";
        btnClearSelectedKeys.Font = new Font("Segoe UI", 9F);
        btnClearSelectedKeys.Location = new Point(10, 487);
        btnClearSelectedKeys.Size = new Size(170, 32);
        btnClearSelectedKeys.BackColor = Color.FromArgb(255, 140, 0);
        btnClearSelectedKeys.ForeColor = Color.White;
        btnClearSelectedKeys.FlatStyle = FlatStyle.Flat;
        btnClearSelectedKeys.FlatAppearance.BorderSize = 0;
        btnClearSelectedKeys.Cursor = Cursors.Hand;
        btnClearSelectedKeys.Visible = false;
        btnClearSelectedKeys.Click += btnClearSelectedKeys_Click;

        pnlCombination.Controls.Add(chkCombinationMode);
        pnlCombination.Controls.Add(lblCombinationInfo);
        pnlCombination.Controls.Add(lblSelectedKeys);
        pnlCombination.Controls.Add(lstSelectedKeys);
        pnlCombination.Controls.Add(btnRemoveKey);
        pnlCombination.Controls.Add(btnClearSelectedKeys);

        // ==========================================
        // KEYS PANEL (Scrollable)
        // ==========================================
        pnlKeys.Location = new Point(10, 150);
        pnlKeys.Size = new Size(780, 350);
        pnlKeys.AutoScroll = true;
        pnlKeys.BorderStyle = BorderStyle.FixedSingle;
        pnlKeys.BackColor = Color.White;

        int yPos = 10;

        // Özel tuşlar
        lblSpecialKeys.Text = "Özel Tuşlar:";
        lblSpecialKeys.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSpecialKeys.Location = new Point(10, yPos);
        lblSpecialKeys.Size = new Size(150, 20);
        pnlKeys.Controls.Add(lblSpecialKeys);
        yPos += 25;

        // Özel tuşlar satır 1
        btnEnter = CreateKeyButton("Enter", 10, yPos, 90);
        btnTab = CreateKeyButton("Tab", 110, yPos, 90);
        btnEsc = CreateKeyButton("Esc", 210, yPos, 90);
        btnSpace = CreateKeyButton("Space", 310, yPos, 90);
        btnBackspace = CreateKeyButton("Backspace", 410, yPos, 120);
        btnDelete = CreateKeyButton("Delete", 540, yPos, 90);
        pnlKeys.Controls.AddRange(new Control[] { btnEnter, btnTab, btnEsc, btnSpace, btnBackspace, btnDelete });
        yPos += 40;

        // Özel tuşlar satır 2
        btnHome = CreateKeyButton("Home", 10, yPos, 90);
        btnEnd = CreateKeyButton("End", 110, yPos, 90);
        btnPageUp = CreateKeyButton("PageUp", 210, yPos, 90);
        btnPageDown = CreateKeyButton("PageDown", 310, yPos, 90);
        btnInsert = CreateKeyButton("Insert", 410, yPos, 90);
        btnPrintScreen = CreateKeyButton("PrintScreen", 510, yPos, 120);
        pnlKeys.Controls.AddRange(new Control[] { btnHome, btnEnd, btnPageUp, btnPageDown, btnInsert, btnPrintScreen });
        yPos += 50;

        // Ok tuşları
        lblArrowKeys.Text = "Ok Tuşları:";
        lblArrowKeys.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblArrowKeys.Location = new Point(10, yPos);
        lblArrowKeys.Size = new Size(150, 20);
        pnlKeys.Controls.Add(lblArrowKeys);
        yPos += 25;

        btnUp = CreateKeyButton("↑", 110, yPos, 50);
        pnlKeys.Controls.Add(btnUp);
        yPos += 40;

        btnLeft = CreateKeyButton("←", 60, yPos, 50);
        btnDown = CreateKeyButton("↓", 110, yPos, 50);
        btnRight = CreateKeyButton("→", 160, yPos, 50);
        pnlKeys.Controls.AddRange(new Control[] { btnLeft, btnDown, btnRight });
        yPos += 50;

        // F tuşları
        lblFunctionKeys.Text = "Fonksiyon Tuşları:";
        lblFunctionKeys.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblFunctionKeys.Location = new Point(10, yPos);
        lblFunctionKeys.Size = new Size(150, 20);
        pnlKeys.Controls.Add(lblFunctionKeys);
        yPos += 25;

        pnlFKeys.Location = new Point(10, yPos);
        pnlFKeys.Size = new Size(740, 70);
        pnlFKeys.BackColor = Color.FromArgb(250, 250, 250);

        for (int i = 1; i <= 12; i++)
        {
            int row = (i - 1) / 6;
            int col = (i - 1) % 6;
            var btnF = CreateKeyButton($"F{i}", 10 + col * 60, 5 + row * 35, 55);
            pnlFKeys.Controls.Add(btnF);
        }
        pnlKeys.Controls.Add(pnlFKeys);
        yPos += 75;

        // Rakamlar
        lblNumbers.Text = "Rakamlar:";
        lblNumbers.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblNumbers.Location = new Point(10, yPos);
        lblNumbers.Size = new Size(150, 20);
        pnlKeys.Controls.Add(lblNumbers);
        yPos += 25;

        pnlNumbers.Location = new Point(10, yPos);
        pnlNumbers.Size = new Size(740, 40);
        pnlNumbers.BackColor = Color.FromArgb(250, 250, 250);

        for (int i = 0; i <= 9; i++)
        {
            var btnNum = CreateKeyButton(i.ToString(), 10 + i * 40, 5, 35);
            pnlNumbers.Controls.Add(btnNum);
        }
        pnlKeys.Controls.Add(pnlNumbers);
        yPos += 45;

        // Harfler
        lblLetters.Text = "Harfler:";
        lblLetters.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblLetters.Location = new Point(10, yPos);
        lblLetters.Size = new Size(150, 20);
        pnlKeys.Controls.Add(lblLetters);
        yPos += 25;

        pnlLetters.Location = new Point(10, yPos);
        pnlLetters.Size = new Size(740, 110);
        pnlLetters.BackColor = Color.FromArgb(250, 250, 250);

        string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < letters.Length; i++)
        {
            int row = i / 13;
            int col = i % 13;
            var btnLetter = CreateKeyButton(letters[i].ToString(), 10 + col * 28, 5 + row * 35, 25);
            pnlLetters.Controls.Add(btnLetter);
        }
        pnlKeys.Controls.Add(pnlLetters);
        yPos += 115;

        // Noktalama ve özel karakterler
        lblPunctuation.Text = "Noktalama & Özel Karakterler:";
        lblPunctuation.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblPunctuation.Location = new Point(10, yPos);
        lblPunctuation.Size = new Size(250, 20);
        pnlKeys.Controls.Add(lblPunctuation);
        yPos += 25;

        pnlPunctuation.Location = new Point(10, yPos);
        pnlPunctuation.Size = new Size(740, 70);
        pnlPunctuation.BackColor = Color.FromArgb(250, 250, 250);

        string[] punctuation = { ".", ",", ";", ":", "!", "?", "-", "_", "+", "=", "*", "/", "\\", "|", "(", ")", "[", "]", "{", "}", "<", ">", "@", "#", "$", "%", "^", "&", "~", "`", "\"", "'" };
        for (int i = 0; i < punctuation.Length; i++)
        {
            int row = i / 16;
            int col = i % 16;
            var btnPunc = CreateKeyButton(punctuation[i], 10 + col * 25, 5 + row * 35, 23);
            pnlPunctuation.Controls.Add(btnPunc);
        }
        pnlKeys.Controls.Add(pnlPunctuation);

        // ==========================================
        // TEXT INPUT PANEL
        // ==========================================
        pnlTextInput.Location = new Point(10, 510);
        pnlTextInput.Size = new Size(780, 80);
        pnlTextInput.BackColor = Color.FromArgb(240, 240, 240);
        pnlTextInput.BorderStyle = BorderStyle.FixedSingle;

        lblTextInput.Text = "Veya Direkt Metin Girin:";
        lblTextInput.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblTextInput.Location = new Point(10, 5);
        lblTextInput.Size = new Size(200, 25);

        txtTextInput.Font = new Font("Segoe UI", 10F);
        txtTextInput.Location = new Point(10, 35);
        txtTextInput.Size = new Size(550, 30);
        txtTextInput.PlaceholderText = "Yazılacak metni buraya girin...";

        btnAddText.Text = "Ekle";
        btnAddText.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnAddText.Location = new Point(570, 32);
        btnAddText.Size = new Size(90, 35);
        btnAddText.BackColor = Color.FromArgb(0, 120, 212);
        btnAddText.ForeColor = Color.White;
        btnAddText.FlatStyle = FlatStyle.Flat;
        btnAddText.FlatAppearance.BorderSize = 0;
        btnAddText.Click += btnAddText_Click;

        btnClearText.Text = "Temizle";
        btnClearText.Font = new Font("Segoe UI", 10F);
        btnClearText.Location = new Point(670, 32);
        btnClearText.Size = new Size(90, 35);
        btnClearText.BackColor = Color.FromArgb(220, 53, 69);
        btnClearText.ForeColor = Color.White;
        btnClearText.FlatStyle = FlatStyle.Flat;
        btnClearText.FlatAppearance.BorderSize = 0;
        btnClearText.Click += btnClearText_Click;

        pnlTextInput.Controls.Add(lblTextInput);
        pnlTextInput.Controls.Add(txtTextInput);
        pnlTextInput.Controls.Add(btnAddText);
        pnlTextInput.Controls.Add(btnClearText);

        // ==========================================
        // PREVIEW PANEL
        // ==========================================
        pnlPreview.Location = new Point(10, 600);
        pnlPreview.Size = new Size(780, 50);
        pnlPreview.BackColor = Color.FromArgb(240, 240, 240);
        pnlPreview.BorderStyle = BorderStyle.FixedSingle;

        lblPreview.Text = "Önizleme:";
        lblPreview.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblPreview.Location = new Point(5, 3);
        lblPreview.Size = new Size(100, 20);

        txtPreview.Font = new Font("Consolas", 10F);
        txtPreview.Location = new Point(10, 23);
        txtPreview.Size = new Size(660, 22);
        txtPreview.ReadOnly = true;
        txtPreview.BackColor = Color.White;

        btnClear.Text = "🗑️";
        btnClear.Font = new Font("Segoe UI", 10F);
        btnClear.Location = new Point(680, 18);
        btnClear.Size = new Size(80, 28);
        btnClear.BackColor = Color.FromArgb(255, 140, 0);
        btnClear.ForeColor = Color.White;
        btnClear.FlatStyle = FlatStyle.Flat;
        btnClear.FlatAppearance.BorderSize = 0;
        btnClear.Click += btnClear_Click;

        pnlPreview.Controls.Add(lblPreview);
        pnlPreview.Controls.Add(txtPreview);
        pnlPreview.Controls.Add(btnClear);

        // ==========================================
        // FOOTER PANEL
        // ==========================================
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Height = 60;
        pnlFooter.BackColor = Color.FromArgb(240, 240, 240);
        pnlFooter.BorderStyle = BorderStyle.FixedSingle;

        btnOK.Text = "✓ Tamam";
        btnOK.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnOK.Location = new Point(520, 12);
        btnOK.Size = new Size(120, 38);
        btnOK.BackColor = Color.FromArgb(16, 124, 16);
        btnOK.ForeColor = Color.White;
        btnOK.FlatStyle = FlatStyle.Flat;
        btnOK.FlatAppearance.BorderSize = 0;
        btnOK.DialogResult = DialogResult.OK;

        btnCancel.Text = "✗ İptal";
        btnCancel.Font = new Font("Segoe UI", 11F);
        btnCancel.Location = new Point(650, 12);
        btnCancel.Size = new Size(120, 38);
        btnCancel.BackColor = Color.FromArgb(108, 117, 125);
        btnCancel.ForeColor = Color.White;
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.DialogResult = DialogResult.Cancel;

        pnlFooter.Controls.Add(btnOK);
        pnlFooter.Controls.Add(btnCancel);

        // Add all panels to form
        Controls.Add(pnlHeader);
        Controls.Add(pnlModifiers);
        Controls.Add(pnlCombination);
        Controls.Add(pnlKeys);
        Controls.Add(pnlTextInput);
        Controls.Add(pnlPreview);
        Controls.Add(pnlFooter);

        ResumeLayout(false);
    }

    #endregion

    private Panel pnlHeader;
    private Panel pnlModifiers;
    private Panel pnlCombination;
    private Panel pnlKeys;
    private Panel pnlTextInput;
    private Panel pnlPreview;
    private Panel pnlFooter;

    private Label lblTitle;

    // Modifier tuşları
    private Label lblModifiers;
    private CheckBox chkCtrl;
    private CheckBox chkShift;
    private CheckBox chkAlt;
    private CheckBox chkWin;

    // Kombinasyon modu
    private CheckBox chkCombinationMode;
    private Label lblCombinationInfo;
    private Label lblSelectedKeys;
    private ListBox lstSelectedKeys;
    private Button btnRemoveKey;
    private Button btnClearSelectedKeys;

    // Kategoriler
    private Label lblSpecialKeys;
    private Label lblArrowKeys;
    private Label lblFunctionKeys;
    private Label lblNumbers;
    private Label lblLetters;
    private Label lblPunctuation;

    // Özel tuş butonları
    private Button btnEnter;
    private Button btnTab;
    private Button btnEsc;
    private Button btnSpace;
    private Button btnBackspace;
    private Button btnDelete;
    private Button btnHome;
    private Button btnEnd;
    private Button btnPageUp;
    private Button btnPageDown;
    private Button btnInsert;
    private Button btnPrintScreen;

    // Ok tuşları
    private Button btnUp;
    private Button btnDown;
    private Button btnLeft;
    private Button btnRight;

    // Paneller
    private Panel pnlFKeys;
    private Panel pnlNumbers;
    private Panel pnlLetters;
    private Panel pnlPunctuation;

    // Metin girişi
    private Label lblTextInput;
    private TextBox txtTextInput;
    private Button btnAddText;
    private Button btnClearText;

    // Önizleme
    private Label lblPreview;
    private TextBox txtPreview;
    private Button btnClear;

    // Footer
    private Button btnOK;
    private Button btnCancel;
}
