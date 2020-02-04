namespace ProductMstMaintenance
{
    partial class ProductMstImportCsv
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductMstImportCsv));
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.lblTitleCsvFile = new System.Windows.Forms.Label();
            this.btnSearchFolder = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(12, 36);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.ReadOnly = true;
            this.txtFolder.Size = new System.Drawing.Size(446, 19);
            this.txtFolder.TabIndex = 1;
            this.txtFolder.Click += new System.EventHandler(this.txtFolder_Click);
            // 
            // lblTitleCsvFile
            // 
            this.lblTitleCsvFile.AutoSize = true;
            this.lblTitleCsvFile.Location = new System.Drawing.Point(12, 21);
            this.lblTitleCsvFile.Name = "lblTitleCsvFile";
            this.lblTitleCsvFile.Size = new System.Drawing.Size(64, 12);
            this.lblTitleCsvFile.TabIndex = 0;
            this.lblTitleCsvFile.Text = "選択フォルダ";
            // 
            // btnSearchFolder
            // 
            this.btnSearchFolder.Location = new System.Drawing.Point(457, 36);
            this.btnSearchFolder.Margin = new System.Windows.Forms.Padding(0);
            this.btnSearchFolder.Name = "btnSearchFolder";
            this.btnSearchFolder.Size = new System.Drawing.Size(29, 19);
            this.btnSearchFolder.TabIndex = 2;
            this.btnSearchFolder.Text = "...";
            this.btnSearchFolder.UseVisualStyleBackColor = true;
            this.btnSearchFolder.Click += new System.EventHandler(this.btnSearchFolder_Click);
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
            // ProductMstImportCsv
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 106);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnSearchFolder);
            this.Controls.Add(this.lblTitleCsvFile);
            this.Controls.Add(this.txtFolder);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProductMstImportCsv";
            this.Text = "品番マスタメンテナンス";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Label lblTitleCsvFile;
        private System.Windows.Forms.Button btnSearchFolder;
        private System.Windows.Forms.Button btnImport;
    }
}