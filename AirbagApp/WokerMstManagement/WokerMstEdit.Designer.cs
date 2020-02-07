namespace WokerMstManagement
{
    partial class WokerMstEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WokerMstEdit));
            this.txtEmployeeNum = new System.Windows.Forms.TextBox();
            this.txtWorkerNameSei = new System.Windows.Forms.TextBox();
            this.txtWorkerNameSeiKana = new System.Windows.Forms.TextBox();
            this.lblTitleEmployeeNum = new System.Windows.Forms.Label();
            this.lblTitleWorkerName = new System.Windows.Forms.Label();
            this.lblTitleWorkerNameKana = new System.Windows.Forms.Label();
            this.lblTitleWorkerNameSei = new System.Windows.Forms.Label();
            this.lblTitleWorkerNameMei = new System.Windows.Forms.Label();
            this.txtWorkerNameMei = new System.Windows.Forms.TextBox();
            this.btnDecision = new System.Windows.Forms.Button();
            this.txtWorkerNameMeiKana = new System.Windows.Forms.TextBox();
            this.lblTitleWorkerNameSeiKana = new System.Windows.Forms.Label();
            this.lblTitleWorkerNameMeiKana = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtEmployeeNum
            // 
            this.txtEmployeeNum.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.txtEmployeeNum.Location = new System.Drawing.Point(94, 30);
            this.txtEmployeeNum.MaxLength = 4;
            this.txtEmployeeNum.Name = "txtEmployeeNum";
            this.txtEmployeeNum.ShortcutsEnabled = false;
            this.txtEmployeeNum.Size = new System.Drawing.Size(36, 19);
            this.txtEmployeeNum.TabIndex = 1;
            this.txtEmployeeNum.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtEmployeeNum_KeyPress);
            this.txtEmployeeNum.Leave += new System.EventHandler(this.txtEmployeeNum_Leave);
            // 
            // txtWorkerNameSei
            // 
            this.txtWorkerNameSei.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
            this.txtWorkerNameSei.Location = new System.Drawing.Point(143, 68);
            this.txtWorkerNameSei.MaxLength = 10;
            this.txtWorkerNameSei.Name = "txtWorkerNameSei";
            this.txtWorkerNameSei.Size = new System.Drawing.Size(68, 19);
            this.txtWorkerNameSei.TabIndex = 3;
            this.txtWorkerNameSei.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtWorkerName_KeyPress);
            // 
            // txtWorkerNameSeiKana
            // 
            this.txtWorkerNameSeiKana.ImeMode = System.Windows.Forms.ImeMode.Katakana;
            this.txtWorkerNameSeiKana.Location = new System.Drawing.Point(143, 134);
            this.txtWorkerNameSeiKana.MaxLength = 30;
            this.txtWorkerNameSeiKana.Name = "txtWorkerNameSeiKana";
            this.txtWorkerNameSeiKana.Size = new System.Drawing.Size(92, 19);
            this.txtWorkerNameSeiKana.TabIndex = 8;
            this.txtWorkerNameSeiKana.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtWorkerNameKana_KeyPress);
            // 
            // lblTitleEmployeeNum
            // 
            this.lblTitleEmployeeNum.AutoSize = true;
            this.lblTitleEmployeeNum.Location = new System.Drawing.Point(36, 33);
            this.lblTitleEmployeeNum.Name = "lblTitleEmployeeNum";
            this.lblTitleEmployeeNum.Size = new System.Drawing.Size(53, 12);
            this.lblTitleEmployeeNum.TabIndex = 0;
            this.lblTitleEmployeeNum.Text = "社員番号";
            // 
            // lblTitleWorkerName
            // 
            this.lblTitleWorkerName.AutoSize = true;
            this.lblTitleWorkerName.Location = new System.Drawing.Point(35, 71);
            this.lblTitleWorkerName.Name = "lblTitleWorkerName";
            this.lblTitleWorkerName.Size = new System.Drawing.Size(53, 12);
            this.lblTitleWorkerName.TabIndex = 1;
            this.lblTitleWorkerName.Text = "作業者名";
            // 
            // lblTitleWorkerNameKana
            // 
            this.lblTitleWorkerNameKana.AutoSize = true;
            this.lblTitleWorkerNameKana.Location = new System.Drawing.Point(35, 137);
            this.lblTitleWorkerNameKana.Name = "lblTitleWorkerNameKana";
            this.lblTitleWorkerNameKana.Size = new System.Drawing.Size(47, 12);
            this.lblTitleWorkerNameKana.TabIndex = 6;
            this.lblTitleWorkerNameKana.Text = "読みカナ";
            // 
            // lblTitleWorkerNameSei
            // 
            this.lblTitleWorkerNameSei.AutoSize = true;
            this.lblTitleWorkerNameSei.Location = new System.Drawing.Point(120, 71);
            this.lblTitleWorkerNameSei.Name = "lblTitleWorkerNameSei";
            this.lblTitleWorkerNameSei.Size = new System.Drawing.Size(17, 12);
            this.lblTitleWorkerNameSei.TabIndex = 2;
            this.lblTitleWorkerNameSei.Text = "姓";
            // 
            // lblTitleWorkerNameMei
            // 
            this.lblTitleWorkerNameMei.AutoSize = true;
            this.lblTitleWorkerNameMei.Location = new System.Drawing.Point(120, 103);
            this.lblTitleWorkerNameMei.Name = "lblTitleWorkerNameMei";
            this.lblTitleWorkerNameMei.Size = new System.Drawing.Size(17, 12);
            this.lblTitleWorkerNameMei.TabIndex = 4;
            this.lblTitleWorkerNameMei.Text = "名";
            // 
            // txtWorkerNameMei
            // 
            this.txtWorkerNameMei.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
            this.txtWorkerNameMei.Location = new System.Drawing.Point(143, 100);
            this.txtWorkerNameMei.MaxLength = 10;
            this.txtWorkerNameMei.Name = "txtWorkerNameMei";
            this.txtWorkerNameMei.Size = new System.Drawing.Size(68, 19);
            this.txtWorkerNameMei.TabIndex = 5;
            this.txtWorkerNameMei.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtWorkerName_KeyPress);
            // 
            // btnDecision
            // 
            this.btnDecision.Location = new System.Drawing.Point(160, 215);
            this.btnDecision.Name = "btnDecision";
            this.btnDecision.Size = new System.Drawing.Size(75, 23);
            this.btnDecision.TabIndex = 11;
            this.btnDecision.Text = "確定";
            this.btnDecision.UseVisualStyleBackColor = true;
            this.btnDecision.Click += new System.EventHandler(this.btnDecision_Click);
            // 
            // txtWorkerNameMeiKana
            // 
            this.txtWorkerNameMeiKana.ImeMode = System.Windows.Forms.ImeMode.Katakana;
            this.txtWorkerNameMeiKana.Location = new System.Drawing.Point(143, 168);
            this.txtWorkerNameMeiKana.MaxLength = 30;
            this.txtWorkerNameMeiKana.Name = "txtWorkerNameMeiKana";
            this.txtWorkerNameMeiKana.Size = new System.Drawing.Size(92, 19);
            this.txtWorkerNameMeiKana.TabIndex = 10;
            this.txtWorkerNameMeiKana.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtWorkerNameKana_KeyPress);
            // 
            // lblTitleWorkerNameSeiKana
            // 
            this.lblTitleWorkerNameSeiKana.AutoSize = true;
            this.lblTitleWorkerNameSeiKana.Location = new System.Drawing.Point(113, 137);
            this.lblTitleWorkerNameSeiKana.Name = "lblTitleWorkerNameSeiKana";
            this.lblTitleWorkerNameSeiKana.Size = new System.Drawing.Size(24, 12);
            this.lblTitleWorkerNameSeiKana.TabIndex = 7;
            this.lblTitleWorkerNameSeiKana.Text = "セイ";
            // 
            // lblTitleWorkerNameMeiKana
            // 
            this.lblTitleWorkerNameMeiKana.AutoSize = true;
            this.lblTitleWorkerNameMeiKana.Location = new System.Drawing.Point(115, 171);
            this.lblTitleWorkerNameMeiKana.Name = "lblTitleWorkerNameMeiKana";
            this.lblTitleWorkerNameMeiKana.Size = new System.Drawing.Size(22, 12);
            this.lblTitleWorkerNameMeiKana.TabIndex = 9;
            this.lblTitleWorkerNameMeiKana.Text = "メイ";
            // 
            // WokerMstEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 261);
            this.Controls.Add(this.btnDecision);
            this.Controls.Add(this.lblTitleWorkerNameKana);
            this.Controls.Add(this.lblTitleWorkerNameMei);
            this.Controls.Add(this.lblTitleWorkerNameMeiKana);
            this.Controls.Add(this.lblTitleWorkerNameSeiKana);
            this.Controls.Add(this.lblTitleWorkerNameSei);
            this.Controls.Add(this.lblTitleWorkerName);
            this.Controls.Add(this.lblTitleEmployeeNum);
            this.Controls.Add(this.txtWorkerNameMeiKana);
            this.Controls.Add(this.txtWorkerNameSeiKana);
            this.Controls.Add(this.txtWorkerNameMei);
            this.Controls.Add(this.txtWorkerNameSei);
            this.Controls.Add(this.txtEmployeeNum);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WokerMstEdit";
            this.Text = "作業者編集";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtEmployeeNum;
        private System.Windows.Forms.TextBox txtWorkerNameSei;
        private System.Windows.Forms.TextBox txtWorkerNameSeiKana;
        private System.Windows.Forms.Label lblTitleEmployeeNum;
        private System.Windows.Forms.Label lblTitleWorkerName;
        private System.Windows.Forms.Label lblTitleWorkerNameKana;
        private System.Windows.Forms.Label lblTitleWorkerNameSei;
        private System.Windows.Forms.Label lblTitleWorkerNameMei;
        private System.Windows.Forms.TextBox txtWorkerNameMei;
        private System.Windows.Forms.Button btnDecision;
        private System.Windows.Forms.TextBox txtWorkerNameMeiKana;
        private System.Windows.Forms.Label lblTitleWorkerNameSeiKana;
        private System.Windows.Forms.Label lblTitleWorkerNameMeiKana;
    }
}