namespace WindowsFormsApp1
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.lblTtleUserId = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.rdbDispNum2 = new System.Windows.Forms.RadioButton();
            this.rdbDispNum4 = new System.Windows.Forms.RadioButton();
            this.rdbDispNum9 = new System.Windows.Forms.RadioButton();
            this.picDispNum2 = new System.Windows.Forms.PictureBox();
            this.picDispNum4 = new System.Windows.Forms.PictureBox();
            this.picDispNum9 = new System.Windows.Forms.PictureBox();
            this.lblTitleDispNum = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picDispNum2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDispNum4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDispNum9)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTtleUserId
            // 
            this.lblTtleUserId.AutoSize = true;
            this.lblTtleUserId.Font = new System.Drawing.Font("メイリオ", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTtleUserId.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.lblTtleUserId.Location = new System.Drawing.Point(17, 13);
            this.lblTtleUserId.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTtleUserId.Name = "lblTtleUserId";
            this.lblTtleUserId.Size = new System.Drawing.Size(87, 23);
            this.lblTtleUserId.TabIndex = 0;
            this.lblTtleUserId.Text = "ユーザID：";
            // 
            // txtUser
            // 
            this.txtUser.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtUser.Location = new System.Drawing.Point(108, 12);
            this.txtUser.Margin = new System.Windows.Forms.Padding(2);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(199, 23);
            this.txtUser.TabIndex = 1;
            this.txtUser.Click += new System.EventHandler(this.textBox2_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.Font = new System.Drawing.Font("メイリオ", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnLogin.Location = new System.Drawing.Point(82, 200);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(2);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(188, 70);
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "ログイン";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.Button1_Click);
            // 
            // rdbDispNum2
            // 
            this.rdbDispNum2.AutoSize = true;
            this.rdbDispNum2.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.rdbDispNum2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.rdbDispNum2.Location = new System.Drawing.Point(21, 81);
            this.rdbDispNum2.Name = "rdbDispNum2";
            this.rdbDispNum2.Size = new System.Drawing.Size(35, 24);
            this.rdbDispNum2.TabIndex = 4;
            this.rdbDispNum2.TabStop = true;
            this.rdbDispNum2.Text = "2";
            this.rdbDispNum2.UseVisualStyleBackColor = true;
            // 
            // rdbDispNum4
            // 
            this.rdbDispNum4.AutoSize = true;
            this.rdbDispNum4.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.rdbDispNum4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.rdbDispNum4.Location = new System.Drawing.Point(136, 81);
            this.rdbDispNum4.Name = "rdbDispNum4";
            this.rdbDispNum4.Size = new System.Drawing.Size(35, 24);
            this.rdbDispNum4.TabIndex = 4;
            this.rdbDispNum4.TabStop = true;
            this.rdbDispNum4.Text = "4";
            this.rdbDispNum4.UseVisualStyleBackColor = true;
            // 
            // rdbDispNum9
            // 
            this.rdbDispNum9.AutoSize = true;
            this.rdbDispNum9.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.rdbDispNum9.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.rdbDispNum9.Location = new System.Drawing.Point(251, 81);
            this.rdbDispNum9.Name = "rdbDispNum9";
            this.rdbDispNum9.Size = new System.Drawing.Size(35, 24);
            this.rdbDispNum9.TabIndex = 4;
            this.rdbDispNum9.TabStop = true;
            this.rdbDispNum9.Text = "9";
            this.rdbDispNum9.UseVisualStyleBackColor = true;
            // 
            // picDispNum2
            // 
            this.picDispNum2.Image = ((System.Drawing.Image)(resources.GetObject("picDispNum2.Image")));
            this.picDispNum2.Location = new System.Drawing.Point(21, 102);
            this.picDispNum2.Name = "picDispNum2";
            this.picDispNum2.Size = new System.Drawing.Size(88, 80);
            this.picDispNum2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDispNum2.TabIndex = 5;
            this.picDispNum2.TabStop = false;
            this.picDispNum2.Click += new System.EventHandler(this.picDispNum2_Click);
            // 
            // picDispNum4
            // 
            this.picDispNum4.Image = ((System.Drawing.Image)(resources.GetObject("picDispNum4.Image")));
            this.picDispNum4.Location = new System.Drawing.Point(136, 102);
            this.picDispNum4.Name = "picDispNum4";
            this.picDispNum4.Size = new System.Drawing.Size(80, 80);
            this.picDispNum4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDispNum4.TabIndex = 5;
            this.picDispNum4.TabStop = false;
            this.picDispNum4.Click += new System.EventHandler(this.picDispNum4_Click);
            // 
            // picDispNum9
            // 
            this.picDispNum9.Image = ((System.Drawing.Image)(resources.GetObject("picDispNum9.Image")));
            this.picDispNum9.Location = new System.Drawing.Point(251, 102);
            this.picDispNum9.Name = "picDispNum9";
            this.picDispNum9.Size = new System.Drawing.Size(80, 80);
            this.picDispNum9.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDispNum9.TabIndex = 5;
            this.picDispNum9.TabStop = false;
            this.picDispNum9.Click += new System.EventHandler(this.picDispNum9_Click);
            // 
            // lblTitleDispNum
            // 
            this.lblTitleDispNum.AutoSize = true;
            this.lblTitleDispNum.Font = new System.Drawing.Font("メイリオ", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTitleDispNum.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.lblTitleDispNum.Location = new System.Drawing.Point(17, 58);
            this.lblTitleDispNum.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitleDispNum.Name = "lblTitleDispNum";
            this.lblTitleDispNum.Size = new System.Drawing.Size(295, 23);
            this.lblTitleDispNum.TabIndex = 0;
            this.lblTitleDispNum.Text = "過検知除外レイアウト選択（表示枚数）：";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.ClientSize = new System.Drawing.Size(356, 281);
            this.Controls.Add(this.picDispNum9);
            this.Controls.Add(this.picDispNum4);
            this.Controls.Add(this.picDispNum2);
            this.Controls.Add(this.rdbDispNum9);
            this.Controls.Add(this.rdbDispNum4);
            this.Controls.Add(this.rdbDispNum2);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.lblTitleDispNum);
            this.Controls.Add(this.lblTtleUserId);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "LoginForm";
            this.Text = "ログイン";
            ((System.ComponentModel.ISupportInitialize)(this.picDispNum2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDispNum4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDispNum9)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTtleUserId;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.RadioButton rdbDispNum2;
        private System.Windows.Forms.RadioButton rdbDispNum4;
        private System.Windows.Forms.RadioButton rdbDispNum9;
        private System.Windows.Forms.PictureBox picDispNum2;
        private System.Windows.Forms.PictureBox picDispNum4;
        private System.Windows.Forms.PictureBox picDispNum9;
        private System.Windows.Forms.Label lblTitleDispNum;
    }
}