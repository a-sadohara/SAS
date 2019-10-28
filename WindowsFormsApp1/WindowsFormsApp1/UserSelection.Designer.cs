namespace WindowsFormsApp1
{
    partial class UserSelection
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lblTiitle = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblSearchわ = new System.Windows.Forms.Label();
            this.lblSearchら = new System.Windows.Forms.Label();
            this.lblSearchや = new System.Windows.Forms.Label();
            this.lblSearchま = new System.Windows.Forms.Label();
            this.lblSearchは = new System.Windows.Forms.Label();
            this.lblSearchな = new System.Windows.Forms.Label();
            this.lblSearchた = new System.Windows.Forms.Label();
            this.lblSearchさ = new System.Windows.Forms.Label();
            this.lblSearchか = new System.Windows.Forms.Label();
            this.lblSearchあ = new System.Windows.Forms.Label();
            this.dgvUser = new System.Windows.Forms.DataGridView();
            this.UserNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UserNm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUser)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTiitle
            // 
            this.lblTiitle.AutoSize = true;
            this.lblTiitle.Font = new System.Drawing.Font("メイリオ", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTiitle.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblTiitle.Location = new System.Drawing.Point(12, 9);
            this.lblTiitle.Name = "lblTiitle";
            this.lblTiitle.Size = new System.Drawing.Size(90, 24);
            this.lblTiitle.TabIndex = 0;
            this.lblTiitle.Text = "作業者検索";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblSearchわ);
            this.panel1.Controls.Add(this.lblSearchら);
            this.panel1.Controls.Add(this.lblSearchや);
            this.panel1.Controls.Add(this.lblSearchま);
            this.panel1.Controls.Add(this.lblSearchは);
            this.panel1.Controls.Add(this.lblSearchな);
            this.panel1.Controls.Add(this.lblSearchた);
            this.panel1.Controls.Add(this.lblSearchさ);
            this.panel1.Controls.Add(this.lblSearchか);
            this.panel1.Controls.Add(this.lblSearchあ);
            this.panel1.Location = new System.Drawing.Point(16, 36);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(490, 132);
            this.panel1.TabIndex = 0;
            // 
            // lblSearchわ
            // 
            this.lblSearchわ.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchわ.AutoSize = true;
            this.lblSearchわ.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchわ.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchわ.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchわ.Location = new System.Drawing.Point(410, 84);
            this.lblSearchわ.Name = "lblSearchわ";
            this.lblSearchわ.Size = new System.Drawing.Size(31, 28);
            this.lblSearchわ.TabIndex = 0;
            this.lblSearchわ.Text = "わ";
            this.lblSearchわ.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // lblSearchら
            // 
            this.lblSearchら.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchら.AutoSize = true;
            this.lblSearchら.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchら.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchら.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchら.Location = new System.Drawing.Point(318, 84);
            this.lblSearchら.Name = "lblSearchら";
            this.lblSearchら.Size = new System.Drawing.Size(31, 28);
            this.lblSearchら.TabIndex = 0;
            this.lblSearchら.Text = "ら";
            this.lblSearchら.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // lblSearchや
            // 
            this.lblSearchや.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchや.AutoSize = true;
            this.lblSearchや.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchや.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchや.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchや.Location = new System.Drawing.Point(225, 84);
            this.lblSearchや.Name = "lblSearchや";
            this.lblSearchや.Size = new System.Drawing.Size(31, 28);
            this.lblSearchや.TabIndex = 0;
            this.lblSearchや.Text = "や";
            this.lblSearchや.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // lblSearchま
            // 
            this.lblSearchま.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchま.AutoSize = true;
            this.lblSearchま.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchま.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchま.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchま.Location = new System.Drawing.Point(133, 84);
            this.lblSearchま.Name = "lblSearchま";
            this.lblSearchま.Size = new System.Drawing.Size(31, 28);
            this.lblSearchま.TabIndex = 0;
            this.lblSearchま.Text = "ま";
            this.lblSearchま.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // lblSearchは
            // 
            this.lblSearchは.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchは.AutoSize = true;
            this.lblSearchは.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchは.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchは.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchは.Location = new System.Drawing.Point(40, 84);
            this.lblSearchは.Name = "lblSearchは";
            this.lblSearchは.Size = new System.Drawing.Size(31, 28);
            this.lblSearchは.TabIndex = 0;
            this.lblSearchは.Text = "は";
            this.lblSearchは.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // lblSearchな
            // 
            this.lblSearchな.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchな.AutoSize = true;
            this.lblSearchな.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchな.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchな.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchな.Location = new System.Drawing.Point(410, 15);
            this.lblSearchな.Name = "lblSearchな";
            this.lblSearchな.Size = new System.Drawing.Size(31, 28);
            this.lblSearchな.TabIndex = 0;
            this.lblSearchな.Text = "な";
            this.lblSearchな.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // lblSearchた
            // 
            this.lblSearchた.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchた.AutoSize = true;
            this.lblSearchた.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchた.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchた.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchた.Location = new System.Drawing.Point(318, 15);
            this.lblSearchた.Name = "lblSearchた";
            this.lblSearchた.Size = new System.Drawing.Size(31, 28);
            this.lblSearchた.TabIndex = 0;
            this.lblSearchた.Text = "た";
            this.lblSearchた.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // lblSearchさ
            // 
            this.lblSearchさ.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchさ.AutoSize = true;
            this.lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchさ.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchさ.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchさ.Location = new System.Drawing.Point(225, 15);
            this.lblSearchさ.Name = "lblSearchさ";
            this.lblSearchさ.Size = new System.Drawing.Size(31, 28);
            this.lblSearchさ.TabIndex = 0;
            this.lblSearchさ.Text = "さ";
            this.lblSearchさ.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // lblSearchか
            // 
            this.lblSearchか.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchか.AutoSize = true;
            this.lblSearchか.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchか.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblSearchか.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchか.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchか.Location = new System.Drawing.Point(133, 15);
            this.lblSearchか.Name = "lblSearchか";
            this.lblSearchか.Size = new System.Drawing.Size(31, 28);
            this.lblSearchか.TabIndex = 0;
            this.lblSearchか.Text = "か";
            this.lblSearchか.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // lblSearchあ
            // 
            this.lblSearchあ.AutoSize = true;
            this.lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            this.lblSearchあ.Font = new System.Drawing.Font("メイリオ", 14.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblSearchあ.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSearchあ.Location = new System.Drawing.Point(40, 15);
            this.lblSearchあ.Name = "lblSearchあ";
            this.lblSearchあ.Size = new System.Drawing.Size(31, 28);
            this.lblSearchあ.TabIndex = 0;
            this.lblSearchあ.Text = "あ";
            this.lblSearchあ.Click += new System.EventHandler(this.lblSearch_Click);
            // 
            // dgvUser
            // 
            this.dgvUser.AllowUserToAddRows = false;
            this.dgvUser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvUser.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvUser.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvUser.ColumnHeadersHeight = 30;
            this.dgvUser.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvUser.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.UserNo,
            this.UserNm});
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle10.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUser.DefaultCellStyle = dataGridViewCellStyle10;
            this.dgvUser.EnableHeadersVisualStyles = false;
            this.dgvUser.Location = new System.Drawing.Point(16, 188);
            this.dgvUser.MultiSelect = false;
            this.dgvUser.Name = "dgvUser";
            this.dgvUser.ReadOnly = true;
            this.dgvUser.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("メイリオ", 9F);
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvUser.RowHeadersDefaultCellStyle = dataGridViewCellStyle11;
            this.dgvUser.RowHeadersVisible = false;
            this.dgvUser.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.dgvUser.RowsDefaultCellStyle = dataGridViewCellStyle12;
            this.dgvUser.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.dgvUser.RowTemplate.Height = 30;
            this.dgvUser.RowTemplate.ReadOnly = true;
            this.dgvUser.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUser.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvUser.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUser.Size = new System.Drawing.Size(490, 387);
            this.dgvUser.TabIndex = 1;
            this.dgvUser.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUser_CellClick);
            // 
            // UserNo
            // 
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.Transparent;
            this.UserNo.DefaultCellStyle = dataGridViewCellStyle8;
            this.UserNo.HeaderText = "社員番号";
            this.UserNo.Name = "UserNo";
            this.UserNo.ReadOnly = true;
            this.UserNo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // UserNm
            // 
            this.UserNm.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.Transparent;
            this.UserNm.DefaultCellStyle = dataGridViewCellStyle9;
            this.UserNm.HeaderText = "作業者名";
            this.UserNm.Name = "UserNm";
            this.UserNm.ReadOnly = true;
            this.UserNm.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // UserSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.ClientSize = new System.Drawing.Size(521, 587);
            this.Controls.Add(this.dgvUser);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblTiitle);
            this.Name = "UserSelection";
            this.Text = "作業者検索";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTiitle;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblSearchま;
        private System.Windows.Forms.Label lblSearchは;
        private System.Windows.Forms.Label lblSearchな;
        private System.Windows.Forms.Label lblSearchた;
        private System.Windows.Forms.Label lblSearchさ;
        private System.Windows.Forms.Label lblSearchか;
        private System.Windows.Forms.Label lblSearchあ;
        private System.Windows.Forms.Label lblSearchわ;
        private System.Windows.Forms.Label lblSearchら;
        private System.Windows.Forms.Label lblSearchや;
        private System.Windows.Forms.DataGridView dgvUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserNm;
    }
}