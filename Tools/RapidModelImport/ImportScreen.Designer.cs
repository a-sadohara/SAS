namespace RapidModelImport
{
    partial class ImportScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportScreen));
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblExecutionResult = new System.Windows.Forms.Label();
            this.txtExecutionResult = new System.Windows.Forms.TextBox();
            this.txtModelName = new System.Windows.Forms.TextBox();
            this.lblModelFileFolderSelection = new System.Windows.Forms.Label();
            this.btnReference = new System.Windows.Forms.Button();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.lblModelName = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.Controls.Add(this.lblExecutionResult);
            this.panel2.Controls.Add(this.txtExecutionResult);
            this.panel2.Controls.Add(this.txtModelName);
            this.panel2.Controls.Add(this.lblModelFileFolderSelection);
            this.panel2.Controls.Add(this.btnReference);
            this.panel2.Controls.Add(this.txtFolder);
            this.panel2.Controls.Add(this.lblModelName);
            this.panel2.Controls.Add(this.btnClose);
            this.panel2.Controls.Add(this.btnImport);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(793, 821);
            this.panel2.TabIndex = 1;
            // 
            // lblExecutionResult
            // 
            this.lblExecutionResult.AutoSize = true;
            this.lblExecutionResult.Location = new System.Drawing.Point(20, 156);
            this.lblExecutionResult.Name = "lblExecutionResult";
            this.lblExecutionResult.Size = new System.Drawing.Size(53, 12);
            this.lblExecutionResult.TabIndex = 6;
            this.lblExecutionResult.Text = "実行結果";
            // 
            // txtExecutionResult
            // 
            this.txtExecutionResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExecutionResult.BackColor = System.Drawing.SystemColors.Control;
            this.txtExecutionResult.Location = new System.Drawing.Point(22, 171);
            this.txtExecutionResult.Multiline = true;
            this.txtExecutionResult.Name = "txtExecutionResult";
            this.txtExecutionResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExecutionResult.Size = new System.Drawing.Size(749, 600);
            this.txtExecutionResult.TabIndex = 4;
            // 
            // txtModelName
            // 
            this.txtModelName.Location = new System.Drawing.Point(22, 26);
            this.txtModelName.MaxLength = 259;
            this.txtModelName.Name = "txtModelName";
            this.txtModelName.Size = new System.Drawing.Size(749, 19);
            this.txtModelName.TabIndex = 0;
            // 
            // lblModelFileFolderSelection
            // 
            this.lblModelFileFolderSelection.AutoSize = true;
            this.lblModelFileFolderSelection.Location = new System.Drawing.Point(20, 60);
            this.lblModelFileFolderSelection.Name = "lblModelFileFolderSelection";
            this.lblModelFileFolderSelection.Size = new System.Drawing.Size(127, 12);
            this.lblModelFileFolderSelection.TabIndex = 4;
            this.lblModelFileFolderSelection.Text = "モデルファイルフォルダ選択";
            // 
            // btnReference
            // 
            this.btnReference.Location = new System.Drawing.Point(695, 74);
            this.btnReference.Margin = new System.Windows.Forms.Padding(2);
            this.btnReference.Name = "btnReference";
            this.btnReference.Size = new System.Drawing.Size(76, 21);
            this.btnReference.TabIndex = 2;
            this.btnReference.Text = "参照";
            this.btnReference.UseVisualStyleBackColor = true;
            this.btnReference.Click += new System.EventHandler(this.btnReference_Click);
            // 
            // txtFolder
            // 
            this.txtFolder.BackColor = System.Drawing.SystemColors.Control;
            this.txtFolder.Location = new System.Drawing.Point(22, 75);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.ReadOnly = true;
            this.txtFolder.Size = new System.Drawing.Size(668, 19);
            this.txtFolder.TabIndex = 1;
            this.txtFolder.Click += new System.EventHandler(this.txtFolder_Click);
            // 
            // lblModelName
            // 
            this.lblModelName.AutoSize = true;
            this.lblModelName.Location = new System.Drawing.Point(20, 11);
            this.lblModelName.Name = "lblModelName";
            this.lblModelName.Size = new System.Drawing.Size(46, 12);
            this.lblModelName.TabIndex = 2;
            this.lblModelName.Text = "モデル名";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(617, 781);
            this.btnClose.Margin = new System.Windows.Forms.Padding(2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(154, 31);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "閉じる";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(617, 121);
            this.btnImport.Margin = new System.Windows.Forms.Padding(2);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(154, 31);
            this.btnImport.TabIndex = 3;
            this.btnImport.Text = "インポート";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.BtnImport_Click);
            // 
            // ImportScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.ClientSize = new System.Drawing.Size(793, 821);
            this.Controls.Add(this.panel2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ImportScreen";
            this.Text = "AIモデル取込処理";
            this.Load += new System.EventHandler(this.SelectAIModelName_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnReference;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Label lblModelName;
        private System.Windows.Forms.TextBox txtModelName;
        private System.Windows.Forms.Label lblModelFileFolderSelection;
        private System.Windows.Forms.Label lblExecutionResult;
        private System.Windows.Forms.TextBox txtExecutionResult;
    }
}