using System.Text;

namespace MedulaOtomasyon;

/// <summary>
/// Klavye tuşları seçim dialog'u
/// SendKeys formatında tuş kombinasyonları oluşturur
/// </summary>
public partial class KeyboardInputDialog : Form
{
    private StringBuilder _keySequence = new StringBuilder();
    private List<string> _selectedKeys = new List<string>(); // Kombinasyon modu için seçili tuşlar
    private const int MAX_COMBINATION_KEYS = 8; // Maksimum tuş sayısı

    /// <summary>
    /// Seçilen tuş kombinasyonu (SendKeys formatında)
    /// </summary>
    public string KeySequence => _keySequence.ToString();

    /// <summary>
    /// Sadece metin girişi mi yoksa tuş kombinasyonu mu?
    /// </summary>
    public bool IsTextInput { get; private set; }

    /// <summary>
    /// Kombinasyon modu aktif mi?
    /// </summary>
    public bool IsCombinationMode => chkCombinationMode.Checked;

    // SendKeys özel tuşları mapping
    private readonly Dictionary<string, string> _specialKeys = new()
    {
        { "Enter", "{ENTER}" },
        { "Tab", "{TAB}" },
        { "Esc", "{ESC}" },
        { "Space", " " },
        { "Backspace", "{BACKSPACE}" },
        { "Delete", "{DELETE}" },
        { "Home", "{HOME}" },
        { "End", "{END}" },
        { "PageUp", "{PGUP}" },
        { "PageDown", "{PGDN}" },
        { "Insert", "{INSERT}" },
        { "PrintScreen", "{PRTSC}" },
        { "↑", "{UP}" },
        { "↓", "{DOWN}" },
        { "←", "{LEFT}" },
        { "→", "{RIGHT}" }
    };

    public KeyboardInputDialog()
    {
        InitializeComponent();
        UpdatePreview();
    }

    public KeyboardInputDialog(string initialKeys) : this()
    {
        if (!string.IsNullOrEmpty(initialKeys))
        {
            _keySequence.Append(initialKeys);
            UpdatePreview();
        }
    }

    /// <summary>
    /// Tuş butonu oluşturur
    /// </summary>
    private Button CreateKeyButton(string text, int x, int y, int width)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, 30),
            Font = new Font("Segoe UI", 8.5F),
            BackColor = Color.FromArgb(240, 240, 240),
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Tag = text // Tuş adını tag'de sakla
        };
        btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
        btn.FlatAppearance.BorderSize = 1;
        btn.Click += KeyButton_Click;

        return btn;
    }

    /// <summary>
    /// Tuş butonuna tıklandığında
    /// </summary>
    private void KeyButton_Click(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string key)
            return;

        IsTextInput = false;
        AddKey(key);
    }

    /// <summary>
    /// Tuş ekler (modifier tuşlarıyla birlikte veya kombinasyon modunda)
    /// </summary>
    private void AddKey(string key)
    {
        // Kombinasyon modu aktifse
        if (IsCombinationMode)
        {
            // Maksimum tuş kontrolü
            if (_selectedKeys.Count >= MAX_COMBINATION_KEYS)
            {
                MessageBox.Show($"Maksimum {MAX_COMBINATION_KEYS} tuş seçilebilir!",
                    "Limit Aşıldı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tuş zaten seçili mi kontrol et
            if (_selectedKeys.Contains(key))
            {
                // Seçimi kaldır
                _selectedKeys.Remove(key);
                lstSelectedKeys.Items.Remove(key);
            }
            else
            {
                // Tuşu ekle
                _selectedKeys.Add(key);
                lstSelectedKeys.Items.Add(key);
            }

            // Önizlemeyi güncelle
            UpdateCombinationPreview();
        }
        else
        {
            // Normal mod - modifier tuşlarıyla birlikte
            var modifiers = GetCurrentModifiers();
            var keyCode = GetKeyCode(key);

            if (!string.IsNullOrEmpty(modifiers))
            {
                // Modifier tuşları ile birlikte
                _keySequence.Append(modifiers);
                _keySequence.Append(keyCode);

                // Modifier tuşlarını temizle
                ClearModifiers();
            }
            else
            {
                // Sadece tuş
                _keySequence.Append(keyCode);
            }

            UpdatePreview();
        }
    }

    /// <summary>
    /// Aktif modifier tuşlarını SendKeys formatında döner
    /// </summary>
    private string GetCurrentModifiers()
    {
        var modifiers = new StringBuilder();

        if (chkCtrl.Checked)
            modifiers.Append("^");

        if (chkAlt.Checked)
            modifiers.Append("%");

        if (chkShift.Checked)
            modifiers.Append("+");

        if (chkWin.Checked)
            modifiers.Append("^{ESC}"); // Win tuşu için yaklaşık (SendKeys Win tuşunu direkt desteklemiyor)

        return modifiers.ToString();
    }

    /// <summary>
    /// Tuş kodunu SendKeys formatına çevirir
    /// </summary>
    private string GetKeyCode(string key)
    {
        // Özel tuşlar için mapping kontrolü
        if (_specialKeys.ContainsKey(key))
            return _specialKeys[key];

        // F tuşları
        if (key.StartsWith("F") && int.TryParse(key[1..], out int fNum) && fNum >= 1 && fNum <= 12)
            return $"{{{key}}}";

        // Özel karakterler - escape edilmesi gerekenler
        if (key is "+" or "^" or "%" or "~" or "(" or ")" or "{" or "}" or "[" or "]")
            return $"{{{key}}}";

        // Normal karakterler
        return key.ToLower();
    }

    /// <summary>
    /// Modifier tuşlarını temizler
    /// </summary>
    private void ClearModifiers()
    {
        chkCtrl.Checked = false;
        chkShift.Checked = false;
        chkAlt.Checked = false;
        chkWin.Checked = false;
    }

    /// <summary>
    /// Modifier tuş değiştiğinde renk değiştir
    /// </summary>
    private void ModifierKey_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is not CheckBox chk)
            return;

        if (chk.Checked)
        {
            chk.BackColor = Color.FromArgb(0, 120, 212);
            chk.ForeColor = Color.White;
        }
        else
        {
            chk.BackColor = Color.White;
            chk.ForeColor = Color.Black;
        }
    }

    /// <summary>
    /// Önizlemeyi günceller
    /// </summary>
    private void UpdatePreview()
    {
        txtPreview.Text = _keySequence.ToString();
    }

    /// <summary>
    /// Metin ekle butonu
    /// </summary>
    private void btnAddText_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtTextInput.Text))
            return;

        IsTextInput = true;

        // Metin olduğu gibi eklenir (SendKeys özel karakterleri escape edilir)
        var text = EscapeTextForSendKeys(txtTextInput.Text);
        _keySequence.Append(text);

        UpdatePreview();
        txtTextInput.Clear();
    }

    /// <summary>
    /// SendKeys için metni escape eder
    /// </summary>
    private string EscapeTextForSendKeys(string text)
    {
        var sb = new StringBuilder();

        foreach (char c in text)
        {
            // SendKeys'de özel anlamı olan karakterleri escape et
            if (c == '+' || c == '^' || c == '%' || c == '~' || c == '(' || c == ')' || c == '{' || c == '}' || c == '[' || c == ']')
            {
                sb.Append('{');
                sb.Append(c);
                sb.Append('}');
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Metin temizle butonu
    /// </summary>
    private void btnClearText_Click(object? sender, EventArgs e)
    {
        txtTextInput.Clear();
    }

    /// <summary>
    /// Tüm önizlemeyi temizle
    /// </summary>
    private void btnClear_Click(object? sender, EventArgs e)
    {
        _keySequence.Clear();
        _selectedKeys.Clear();
        lstSelectedKeys.Items.Clear();
        ClearModifiers();
        UpdatePreview();
        IsTextInput = false;
    }

    /// <summary>
    /// Kombinasyon modu değiştiğinde
    /// </summary>
    private void chkCombinationMode_CheckedChanged(object? sender, EventArgs e)
    {
        bool isChecked = chkCombinationMode.Checked;

        // Kombinasyon paneli kontrollerini göster/gizle
        lblCombinationInfo.Visible = isChecked;
        lblSelectedKeys.Visible = isChecked;
        lstSelectedKeys.Visible = isChecked;
        btnRemoveKey.Visible = isChecked;
        btnClearSelectedKeys.Visible = isChecked;

        // Modifier tuşlarını devre dışı bırak (kombinasyon modunda)
        pnlModifiers.Enabled = !isChecked;

        // Mod değiştiğinde mevcut seçimleri temizle
        if (isChecked)
        {
            _selectedKeys.Clear();
            lstSelectedKeys.Items.Clear();
            ClearModifiers();
        }
        else
        {
            // Normal moda dönülürken, seçili tuşları sequence'e ekle
            if (_selectedKeys.Count > 0)
            {
                GenerateCombinationSequence();
            }
        }

        UpdatePreview();
    }

    /// <summary>
    /// Seçili tuşu listeden kaldır
    /// </summary>
    private void btnRemoveKey_Click(object? sender, EventArgs e)
    {
        if (lstSelectedKeys.SelectedItem is string selectedKey)
        {
            _selectedKeys.Remove(selectedKey);
            lstSelectedKeys.Items.Remove(selectedKey);
            UpdateCombinationPreview();
        }
    }

    /// <summary>
    /// Tüm seçili tuşları temizle
    /// </summary>
    private void btnClearSelectedKeys_Click(object? sender, EventArgs e)
    {
        _selectedKeys.Clear();
        lstSelectedKeys.Items.Clear();
        UpdateCombinationPreview();
    }

    /// <summary>
    /// Kombinasyon modu önizlemesini güncelle
    /// </summary>
    private void UpdateCombinationPreview()
    {
        if (_selectedKeys.Count == 0)
        {
            txtPreview.Text = "";
            return;
        }

        // Seçili tuşları SendKeys formatında göster
        var preview = new StringBuilder();
        preview.Append("Seçili: ");
        preview.Append(string.Join(" + ", _selectedKeys));
        preview.Append(" → ");

        // SendKeys formatı
        GenerateCombinationSequence();
        preview.Append(_keySequence.ToString());

        txtPreview.Text = preview.ToString();
    }

    /// <summary>
    /// Kombinasyon tuşlarından SendKeys sequence'i oluştur
    /// </summary>
    private void GenerateCombinationSequence()
    {
        _keySequence.Clear();

        if (_selectedKeys.Count == 0)
            return;

        // NOT: SendKeys çoklu tuş kombinasyonları için sınırlı destek sunar
        // Bu yüzden tuşları parantez içinde gruplayacağız
        // Örnek: Ctrl+Shift+A için ^+(a) formatı

        // Tüm tuşları SendKeys formatına çevir ve ekle
        var keyCodes = _selectedKeys.Select(k => GetKeyCode(k)).ToList();

        // Eğer sadece 1 tuş varsa
        if (keyCodes.Count == 1)
        {
            _keySequence.Append(keyCodes[0]);
        }
        else
        {
            // Çoklu tuş kombinasyonu
            // SendKeys formatı: Her tuşu sırayla bas
            foreach (var keyCode in keyCodes)
            {
                _keySequence.Append(keyCode);
            }
        }
    }

    /// <summary>
    /// Klavye kısayolları için
    /// </summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Ctrl+A, Ctrl+C, vb. ile hızlı ekleme
        if (keyData == (Keys.Control | Keys.A))
        {
            chkCtrl.Checked = true;
            AddKey("A");
            return true;
        }
        else if (keyData == (Keys.Control | Keys.C))
        {
            chkCtrl.Checked = true;
            AddKey("C");
            return true;
        }
        else if (keyData == (Keys.Control | Keys.V))
        {
            chkCtrl.Checked = true;
            AddKey("V");
            return true;
        }
        else if (keyData == Keys.Enter && txtTextInput.Focused)
        {
            // TextBox'ta Enter'a basıldığında metni ekle
            btnAddText_Click(btnAddText, EventArgs.Empty);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}
