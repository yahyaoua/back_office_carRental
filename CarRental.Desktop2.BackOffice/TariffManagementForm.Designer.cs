namespace CarRental.Desktop2.BackOffice
{
    partial class TariffManagementForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvTariffs = new DataGridView();
            btnLoadTest = new Button();
            lblTariffName = new Label();
            txtTariffName = new TextBox();
            lblDailyRate = new Label();
            txtDailyRate = new TextBox();
            btnDelete = new Button();
            btnClear = new Button();
            btnAdd = new Button();
            btnUpdate = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvTariffs).BeginInit();
            SuspendLayout();
            // 
            // dgvTariffs
            // 
            dgvTariffs.AccessibleDescription = "tariffDataGridView";
            dgvTariffs.AccessibleName = "tariffDataGridView";
            dgvTariffs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTariffs.Location = new Point(120, 178);
            dgvTariffs.Name = "dgvTariffs";
            dgvTariffs.Size = new Size(531, 34);
            dgvTariffs.TabIndex = 0;
            dgvTariffs.CellContentClick += tariffDataGridView_CellContentClick;
            // 
            // btnLoadTest
            // 
            btnLoadTest.AccessibleDescription = "btnLoadTest";
            btnLoadTest.AccessibleName = "btnLoadTest";
            btnLoadTest.Location = new Point(35, 353);
            btnLoadTest.Name = "btnLoadTest";
            btnLoadTest.Size = new Size(122, 56);
            btnLoadTest.TabIndex = 1;
            btnLoadTest.Text = "Tester Connexion";
            btnLoadTest.UseVisualStyleBackColor = true;
            // 
            // lblTariffName
            // 
            lblTariffName.AutoSize = true;
            lblTariffName.Location = new Point(54, 35);
            lblTariffName.Name = "lblTariffName";
            lblTariffName.Size = new Size(81, 15);
            lblTariffName.TabIndex = 2;
            lblTariffName.Text = "Nom du tarif :";
            lblTariffName.Click += lblTariffName_Click;
            // 
            // txtTariffName
            // 
            txtTariffName.Location = new Point(151, 32);
            txtTariffName.Name = "txtTariffName";
            txtTariffName.Size = new Size(259, 23);
            txtTariffName.TabIndex = 3;
            txtTariffName.TextChanged += textBox1_TextChanged;
            // 
            // lblDailyRate
            // 
            lblDailyRate.AutoSize = true;
            lblDailyRate.Location = new Point(45, 78);
            lblDailyRate.Name = "lblDailyRate";
            lblDailyRate.Size = new Size(90, 15);
            lblDailyRate.TabIndex = 4;
            lblDailyRate.Text = "Taux journalier :";
            // 
            // txtDailyRate
            // 
            txtDailyRate.Location = new Point(151, 75);
            txtDailyRate.Name = "txtDailyRate";
            txtDailyRate.Size = new Size(259, 23);
            txtDailyRate.TabIndex = 5;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(450, 357);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(100, 52);
            btnDelete.TabIndex = 8;
            btnDelete.Text = "Supprimer";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(591, 357);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(124, 52);
            btnClear.TabIndex = 9;
            btnClear.Text = "Effacer";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(185, 355);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(93, 56);
            btnAdd.TabIndex = 10;
            btnAdd.Text = "Ajouter";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(318, 355);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(92, 56);
            btnUpdate.TabIndex = 11;
            btnUpdate.Text = "Modifier";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // TariffManagementForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnUpdate);
            Controls.Add(btnAdd);
            Controls.Add(btnClear);
            Controls.Add(btnDelete);
            Controls.Add(txtDailyRate);
            Controls.Add(lblDailyRate);
            Controls.Add(txtTariffName);
            Controls.Add(lblTariffName);
            Controls.Add(btnLoadTest);
            Controls.Add(dgvTariffs);
            Name = "TariffManagementForm";
            Text = "TariffManagementForm";
            Load += TariffManagementForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvTariffs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView tariffDataGridView;
        private Button btnLoadTest;
        private DataGridView dgvTariffs;
        private Label lblTariffName;
        private TextBox txtTariffName;
        private Label lblDailyRate;
        private TextBox txtDailyRate;
        private Button btnDelete;
        private Button btnClear;
        private Button btnAdd;
        private Button btnUpdate;
    }
}