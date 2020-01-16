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
            this.txtImportFilePath = new System.Windows.Forms.TextBox();
            this.lblTitleCsvFile = new System.Windows.Forms.Label();
            this.btnSelectImportFile = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtImportFilePath
            // 
            this.txtImportFilePath.Location = new System.Drawing.Point(12, 36);
            this.txtImportFilePath.Name = "txtImportFilePath";
            this.txtImportFilePath.ReadOnly = true;
            this.txtImportFilePath.Size = new System.Drawing.Size(446, 19);
            this.txtImportFilePath.TabIndex = 1;
            this.txtImportFilePath.Click += new System.EventHandler(this.SelectImportFile_Click);
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
            // btnSelectImportFile
            // 
            this.btnSelectImportFile.Location = new System.Drawing.Point(457, 36);
            this.btnSelectImportFile.Margin = new System.Windows.Forms.Padding(0);
            this.btnSelectImportFile.Name = "btnSelectImportFile";
            this.btnSelectImportFile.Size = new System.Drawing.Size(29, 19);
            this.btnSelectImportFile.TabIndex = 2;
            this.btnSelectImportFile.Text = "...";
            this.btnSelectImportFile.UseVisualStyleBackColor = true;
            this.btnSelectImportFile.Click += new System.EventHandler(this.SelectImportFile_Click);
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
            // WokerMstImportCsv
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 106);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnSelectImportFile);
            this.Controls.Add(this.lblTitleCsvFile);
            this.Controls.Add(this.txtImportFilePath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WokerMstImportCsv";
            this.Text = "作業者CSV取込";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtImportFilePath;
        private System.Windows.Forms.Label lblTitleCsvFile;
        private System.Windows.Forms.Button btnSelectImportFile;
        private System.Windows.Forms.Button btnImport;
    }
}