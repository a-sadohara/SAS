namespace WokerMstManagement
{
    partial class WokerMstImportCsv
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WokerMstImportCsv));
            this.txtCsvFile = new System.Windows.Forms.TextBox();
            this.lblTitleCsvFile = new System.Windows.Forms.Label();
            this.btnCsvFile = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtCsvFile
            // 
            this.txtCsvFile.Location = new System.Drawing.Point(12, 36);
            this.txtCsvFile.Name = "txtCsvFile";
            this.txtCsvFile.ReadOnly = true;
            this.txtCsvFile.Size = new System.Drawing.Size(446, 19);
            this.txtCsvFile.TabIndex = 1;
            this.txtCsvFile.Click += new System.EventHandler(this.CsvFile_Click);
            // 
            // lblTitleCsvFile
            // 
            this.lblTitleCsvFile.AutoSize = true;
            this.lblTitleCsvFile.Location = new System.Drawing.Point(12, 21);
            this.lblTitleCsvFile.Name = "lblTitleCsvFile";
            this.lblTitleCsvFile.Size = new System.Drawing.Size(98, 12);
            this.lblTitleCsvFile.TabIndex = 0;
            this.lblTitleCsvFile.Text = "選択ファイル（CSV）";
            // 
            // btnCsvFile
            // 
            this.btnCsvFile.Location = new System.Drawing.Point(457, 36);
            this.btnCsvFile.Margin = new System.Windows.Forms.Padding(0);
            this.btnCsvFile.Name = "btnCsvFile";
            this.btnCsvFile.Size = new System.Drawing.Size(29, 19);
            this.btnCsvFile.TabIndex = 2;
            this.btnCsvFile.Text = "...";
            this.btnCsvFile.UseVisualStyleBackColor = true;
            this.btnCsvFile.Click += new System.EventHandler(this.CsvFile_Click);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(203, 71);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 3;
            this.btnImport.Text = "取込";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // UserImportCsv
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 106);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnCsvFile);
            this.Controls.Add(this.lblTitleCsvFile);
            this.Controls.Add(this.txtCsvFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UserImportCsv";
            this.Text = "作業者CSV取込";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCsvFile;
        private System.Windows.Forms.Label lblTitleCsvFile;
        private System.Windows.Forms.Button btnCsvFile;
        private System.Windows.Forms.Button btnImport;
    }
}