namespace CarRental.Desktop2.BackOffice
{
    partial class MainForm
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
            btnManageTariffs = new Button();
            SuspendLayout();
            // 
            // btnManageTariffs
            // 
            btnManageTariffs.Location = new Point(420, 170);
            btnManageTariffs.Name = "btnManageTariffs";
            btnManageTariffs.Size = new Size(75, 23);
            btnManageTariffs.TabIndex = 0;
            btnManageTariffs.Text = "Gérer les Tarifs";
            btnManageTariffs.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnManageTariffs);
            Name = "MainForm";
            Text = "MainForm";
            ResumeLayout(false);
        }

        #endregion

        private Button btnManageTariffs;
    }
}