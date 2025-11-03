namespace MedulaOtomasyon;

partial class Step3_ConditionSetup
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
        lblElementInfo = new Label();
        grpConditionEntry = new GroupBox();
        lblProperty = new Label();
        cmbProperty = new ComboBox();
        lblOperator = new Label();
        cmbOperator = new ComboBox();
        lblValue = new Label();
        txtValue = new TextBox();
        btnAddCondition = new Button();
        grpConditionsList = new GroupBox();
        lstConditions = new ListBox();
        btnRemoveCondition = new Button();

        grpConditionEntry.SuspendLayout();
        grpConditionsList.SuspendLayout();
        SuspendLayout();

        this.Size = new Size(1080, 560);
        this.BackColor = System.Drawing.Color.White;

        //
        // lblInstruction
        //
        lblInstruction.Location = new Point(20, 20);
        lblInstruction.Size = new Size(1040, 30);
        lblInstruction.Text = "⚙️ Koşul Tanımlama - Element özelliklerini kontrol edin";
        lblInstruction.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

        //
        // lblElementInfo
        //
        lblElementInfo.Location = new Point(20, 60);
        lblElementInfo.Size = new Size(1040, 30);
        lblElementInfo.Font = new Font("Segoe UI", 10F);
        lblElementInfo.ForeColor = System.Drawing.Color.DarkGreen;

        //
        // grpConditionEntry
        //
        grpConditionEntry.Location = new Point(20, 100);
        grpConditionEntry.Size = new Size(1040, 180);
        grpConditionEntry.Text = "Yeni Koşul Ekle";
        grpConditionEntry.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpConditionEntry.Controls.Add(lblProperty);
        grpConditionEntry.Controls.Add(cmbProperty);
        grpConditionEntry.Controls.Add(lblOperator);
        grpConditionEntry.Controls.Add(cmbOperator);
        grpConditionEntry.Controls.Add(lblValue);
        grpConditionEntry.Controls.Add(txtValue);
        grpConditionEntry.Controls.Add(btnAddCondition);

        //
        // lblProperty
        //
        lblProperty.Location = new Point(15, 35);
        lblProperty.Size = new Size(200, 20);
        lblProperty.Text = "Kontrol edilecek özellik:";
        lblProperty.Font = new Font("Segoe UI", 9F);

        //
        // cmbProperty
        //
        cmbProperty.Location = new Point(15, 60);
        cmbProperty.Size = new Size(500, 25);
        cmbProperty.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbProperty.Font = new Font("Segoe UI", 9F);

        //
        // lblOperator
        //
        lblOperator.Location = new Point(535, 35);
        lblOperator.Size = new Size(150, 20);
        lblOperator.Text = "Operatör:";
        lblOperator.Font = new Font("Segoe UI", 9F);

        //
        // cmbOperator
        //
        cmbOperator.Location = new Point(535, 60);
        cmbOperator.Size = new Size(200, 25);
        cmbOperator.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbOperator.Font = new Font("Segoe UI", 9F);

        //
        // lblValue
        //
        lblValue.Location = new Point(15, 95);
        lblValue.Size = new Size(200, 20);
        lblValue.Text = "Beklenen değer:";
        lblValue.Font = new Font("Segoe UI", 9F);

        //
        // txtValue
        //
        txtValue.Location = new Point(15, 120);
        txtValue.Size = new Size(720, 25);
        txtValue.Font = new Font("Segoe UI", 9F);

        //
        // btnAddCondition
        //
        btnAddCondition.Location = new Point(755, 115);
        btnAddCondition.Size = new Size(270, 35);
        btnAddCondition.Text = "➕ Koşul Ekle";
        btnAddCondition.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnAddCondition.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnAddCondition.ForeColor = System.Drawing.Color.White;
        btnAddCondition.FlatStyle = FlatStyle.Flat;
        btnAddCondition.Click += BtnAddCondition_Click;

        //
        // grpConditionsList
        //
        grpConditionsList.Location = new Point(20, 300);
        grpConditionsList.Size = new Size(1040, 240);
        grpConditionsList.Text = "Tanımlı Koşullar";
        grpConditionsList.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grpConditionsList.Controls.Add(lstConditions);
        grpConditionsList.Controls.Add(btnRemoveCondition);

        //
        // lstConditions
        //
        lstConditions.Location = new Point(15, 30);
        lstConditions.Size = new Size(1010, 150);
        lstConditions.Font = new Font("Consolas", 10F);

        //
        // btnRemoveCondition
        //
        btnRemoveCondition.Location = new Point(15, 190);
        btnRemoveCondition.Size = new Size(200, 35);
        btnRemoveCondition.Text = "➖ Seçili Koşulu Sil";
        btnRemoveCondition.Font = new Font("Segoe UI", 9F);
        btnRemoveCondition.Click += BtnRemoveCondition_Click;

        // Add controls
        this.Controls.Add(lblInstruction);
        this.Controls.Add(lblElementInfo);
        this.Controls.Add(grpConditionEntry);
        this.Controls.Add(grpConditionsList);

        grpConditionEntry.ResumeLayout(false);
        grpConditionsList.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Label lblInstruction;
    private Label lblElementInfo;
    private GroupBox grpConditionEntry;
    private Label lblProperty;
    private ComboBox cmbProperty;
    private Label lblOperator;
    private ComboBox cmbOperator;
    private Label lblValue;
    private TextBox txtValue;
    private Button btnAddCondition;
    private GroupBox grpConditionsList;
    private ListBox lstConditions;
    private Button btnRemoveCondition;
}
