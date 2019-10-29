namespace UserMasterMaintenance
{
    partial class UserEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserEdit));
            this.txtUserNo = new System.Windows.Forms.TextBox();
            this.txtUserNm_Sei = new System.Windows.Forms.TextBox();
            this.txtUserYomiGana_Sei = new System.Windows.Forms.TextBox();
            this.lblTitleUserNo = new System.Windows.Forms.Label();
            this.lblTitleUserNm = new System.Windows.Forms.Label();
            this.lblTitleYomiGana = new System.Windows.Forms.Label();
            this.lblTitleUserNm_Sei = new System.Windows.Forms.Label();
            this.lblTitleUserNm_Mei = new System.Windows.Forms.Label();
            this.txtUserNm_Mei = new System.Windows.Forms.TextBox();
            this.btnFix = new System.Windows.Forms.Button();
            this.txtUserYomiGana_Mei = new System.Windows.Forms.TextBox();
            this.lblTitleYomiGana_Sei = new System.Windows.Forms.Label();
            this.lblTitleYomiGana_Mei = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtUserNo
            // 
            this.txtUserNo.Location = new System.Drawing.Point(94, 30);
            this.txtUserNo.MaxLength = 4;
            this.txtUserNo.Name = "txtUserNo";
            this.txtUserNo.Size = new System.Drawing.Size(36, 19);
            this.txtUserNo.TabIndex = 1;
            this.txtUserNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUserNo_KeyPress);
            this.txtUserNo.Leave += new System.EventHandler(this.txtUserNo_Leave);
            // 
            // txtUserNm_Sei
            // 
            this.txtUserNm_Sei.Location = new System.Drawing.Point(143, 68);
            this.txtUserNm_Sei.MaxLength = 10;
            this.txtUserNm_Sei.Name = "txtUserNm_Sei";
            this.txtUserNm_Sei.Size = new System.Drawing.Size(68, 19);
            this.txtUserNm_Sei.TabIndex = 3;
            // 
            // txtUserYomiGana_Sei
            // 
            this.txtUserYomiGana_Sei.Location = new System.Drawing.Point(143, 134);
            this.txtUserYomiGana_Sei.MaxLength = 30;
            this.txtUserYomiGana_Sei.Name = "txtUserYomiGana_Sei";
            this.txtUserYomiGana_Sei.Size = new System.Drawing.Size(92, 19);
            this.txtUserYomiGana_Sei.TabIndex = 8;
            // 
            // lblTitleUserNo
            // 
            this.lblTitleUserNo.AutoSize = true;
            this.lblTitleUserNo.Location = new System.Drawing.Point(36, 33);
            this.lblTitleUserNo.Name = "lblTitleUserNo";
            this.lblTitleUserNo.Size = new System.Drawing.Size(53, 12);
            this.lblTitleUserNo.TabIndex = 0;
            this.lblTitleUserNo.Text = "社員番号";
            // 
            // lblTitleUserNm
            // 
            this.lblTitleUserNm.AutoSize = true;
            this.lblTitleUserNm.Location = new System.Drawing.Point(35, 71);
            this.lblTitleUserNm.Name = "lblTitleUserNm";
            this.lblTitleUserNm.Size = new System.Drawing.Size(53, 12);
            this.lblTitleUserNm.TabIndex = 1;
            this.lblTitleUserNm.Text = "作業者名";
            // 
            // lblTitleYomiGana
            // 
            this.lblTitleYomiGana.AutoSize = true;
            this.lblTitleYomiGana.Location = new System.Drawing.Point(35, 137);
            this.lblTitleYomiGana.Name = "lblTitleYomiGana";
            this.lblTitleYomiGana.Size = new System.Drawing.Size(47, 12);
            this.lblTitleYomiGana.TabIndex = 6;
            this.lblTitleYomiGana.Text = "読みカナ";
            // 
            // lblTitleUserNm_Sei
            // 
            this.lblTitleUserNm_Sei.AutoSize = true;
            this.lblTitleUserNm_Sei.Location = new System.Drawing.Point(120, 71);
            this.lblTitleUserNm_Sei.Name = "lblTitleUserNm_Sei";
            this.lblTitleUserNm_Sei.Size = new System.Drawing.Size(17, 12);
            this.lblTitleUserNm_Sei.TabIndex = 2;
            this.lblTitleUserNm_Sei.Text = "姓";
            // 
            // lblTitleUserNm_Mei
            // 
            this.lblTitleUserNm_Mei.AutoSize = true;
            this.lblTitleUserNm_Mei.Location = new System.Drawing.Point(120, 103);
            this.lblTitleUserNm_Mei.Name = "lblTitleUserNm_Mei";
            this.lblTitleUserNm_Mei.Size = new System.Drawing.Size(17, 12);
            this.lblTitleUserNm_Mei.TabIndex = 4;
            this.lblTitleUserNm_Mei.Text = "名";
            // 
            // txtUserNm_Mei
            // 
            this.txtUserNm_Mei.Location = new System.Drawing.Point(143, 100);
            this.txtUserNm_Mei.MaxLength = 10;
            this.txtUserNm_Mei.Name = "txtUserNm_Mei";
            this.txtUserNm_Mei.Size = new System.Drawing.Size(68, 19);
            this.txtUserNm_Mei.TabIndex = 5;
            // 
            // btnFix
            // 
            this.btnFix.Location = new System.Drawing.Point(160, 215);
            this.btnFix.Name = "btnFix";
            this.btnFix.Size = new System.Drawing.Size(75, 23);
            this.btnFix.TabIndex = 11;
            this.btnFix.Text = "確定";
            this.btnFix.UseVisualStyleBackColor = true;
            this.btnFix.Click += new System.EventHandler(this.btnFix_Click);
            // 
            // txtUserYomiGana_Mei
            // 
            this.txtUserYomiGana_Mei.Location = new System.Drawing.Point(143, 168);
            this.txtUserYomiGana_Mei.MaxLength = 30;
            this.txtUserYomiGana_Mei.Name = "txtUserYomiGana_Mei";
            this.txtUserYomiGana_Mei.Size = new System.Drawing.Size(92, 19);
            this.txtUserYomiGana_Mei.TabIndex = 10;
            // 
            // lblTitleYomiGana_Sei
            // 
            this.lblTitleYomiGana_Sei.AutoSize = true;
            this.lblTitleYomiGana_Sei.Location = new System.Drawing.Point(120, 137);
            this.lblTitleYomiGana_Sei.Name = "lblTitleYomiGana_Sei";
            this.lblTitleYomiGana_Sei.Size = new System.Drawing.Size(17, 12);
            this.lblTitleYomiGana_Sei.TabIndex = 7;
            this.lblTitleYomiGana_Sei.Text = "姓";
            // 
            // lblTitleYomiGana_Mei
            // 
            this.lblTitleYomiGana_Mei.AutoSize = true;
            this.lblTitleYomiGana_Mei.Location = new System.Drawing.Point(120, 171);
            this.lblTitleYomiGana_Mei.Name = "lblTitleYomiGana_Mei";
            this.lblTitleYomiGana_Mei.Size = new System.Drawing.Size(17, 12);
            this.lblTitleYomiGana_Mei.TabIndex = 9;
            this.lblTitleYomiGana_Mei.Text = "名";
            // 
            // UserEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 261);
            this.Controls.Add(this.btnFix);
            this.Controls.Add(this.lblTitleYomiGana);
            this.Controls.Add(this.lblTitleUserNm_Mei);
            this.Controls.Add(this.lblTitleYomiGana_Mei);
            this.Controls.Add(this.lblTitleYomiGana_Sei);
            this.Controls.Add(this.lblTitleUserNm_Sei);
            this.Controls.Add(this.lblTitleUserNm);
            this.Controls.Add(this.lblTitleUserNo);
            this.Controls.Add(this.txtUserYomiGana_Mei);
            this.Controls.Add(this.txtUserYomiGana_Sei);
            this.Controls.Add(this.txtUserNm_Mei);
            this.Controls.Add(this.txtUserNm_Sei);
            this.Controls.Add(this.txtUserNo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UserEdit";
            this.Text = "登録/更新";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtUserNo;
        private System.Windows.Forms.TextBox txtUserNm_Sei;
        private System.Windows.Forms.TextBox txtUserYomiGana_Sei;
        private System.Windows.Forms.Label lblTitleUserNo;
        private System.Windows.Forms.Label lblTitleUserNm;
        private System.Windows.Forms.Label lblTitleYomiGana;
        private System.Windows.Forms.Label lblTitleUserNm_Sei;
        private System.Windows.Forms.Label lblTitleUserNm_Mei;
        private System.Windows.Forms.TextBox txtUserNm_Mei;
        private System.Windows.Forms.Button btnFix;
        private System.Windows.Forms.TextBox txtUserYomiGana_Mei;
        private System.Windows.Forms.Label lblTitleYomiGana_Sei;
        private System.Windows.Forms.Label lblTitleYomiGana_Mei;
    }
}