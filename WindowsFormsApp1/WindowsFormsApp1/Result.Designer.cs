namespace WindowsFormsApp1
{
    partial class Result
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Result));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnLogOut = new System.Windows.Forms.Button();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblEndDate = new System.Windows.Forms.Label();
            this.lblStaDate = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblHansoEndDate = new System.Windows.Forms.Label();
            this.lblHansoStaDate = new System.Windows.Forms.Label();
            this.lblHanNo = new System.Windows.Forms.Label();
            this.lblSashizu = new System.Windows.Forms.Label();
            this.lblShinaNm = new System.Windows.Forms.Label();
            this.lblGoki = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnRegResult = new System.Windows.Forms.Button();
            this.btnBackResultCheck = new System.Windows.Forms.Button();
            this.btnBackTargetSelection = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.no = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.line = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ng = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.position = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.reason = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel1.Controls.Add(this.btnLogOut);
            this.panel1.Controls.Add(this.lblUser);
            this.panel1.Controls.Add(this.lblEndDate);
            this.panel1.Controls.Add(this.lblStaDate);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.lblHansoEndDate);
            this.panel1.Controls.Add(this.lblHansoStaDate);
            this.panel1.Controls.Add(this.lblHanNo);
            this.panel1.Controls.Add(this.lblSashizu);
            this.panel1.Controls.Add(this.lblShinaNm);
            this.panel1.Controls.Add(this.lblGoki);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1363, 129);
            this.panel1.TabIndex = 0;
            // 
            // btnLogOut
            // 
            this.btnLogOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogOut.Font = new System.Drawing.Font("メイリオ", 9F);
            this.btnLogOut.Location = new System.Drawing.Point(1245, 9);
            this.btnLogOut.Margin = new System.Windows.Forms.Padding(2);
            this.btnLogOut.Name = "btnLogOut";
            this.btnLogOut.Size = new System.Drawing.Size(107, 36);
            this.btnLogOut.TabIndex = 0;
            this.btnLogOut.Text = "ログアウト";
            this.btnLogOut.UseVisualStyleBackColor = true;
            this.btnLogOut.Click += new System.EventHandler(this.btnLogOut_Click);
            // 
            // lblUser
            // 
            this.lblUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUser.AutoSize = true;
            this.lblUser.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblUser.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblUser.Location = new System.Drawing.Point(941, 19);
            this.lblUser.Margin = new System.Windows.Forms.Padding(2);
            this.lblUser.MaximumSize = new System.Drawing.Size(300, 0);
            this.lblUser.MinimumSize = new System.Drawing.Size(300, 0);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(300, 17);
            this.lblUser.TabIndex = 25;
            this.lblUser.Text = "作業者名：XXXXXXXX";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblEndDate
            // 
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblEndDate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblEndDate.Location = new System.Drawing.Point(157, 94);
            this.lblEndDate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new System.Drawing.Size(207, 17);
            this.lblEndDate.TabIndex = 24;
            this.lblEndDate.Text = "判定終了日時：2019/01/22 19:10:15";
            // 
            // lblStaDate
            // 
            this.lblStaDate.AutoSize = true;
            this.lblStaDate.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblStaDate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblStaDate.Location = new System.Drawing.Point(157, 77);
            this.lblStaDate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStaDate.Name = "lblStaDate";
            this.lblStaDate.Size = new System.Drawing.Size(207, 17);
            this.lblStaDate.TabIndex = 23;
            this.lblStaDate.Text = "判定開始日時：2019/01/22 16:30:15";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("メイリオ", 8F);
            this.label6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label6.Location = new System.Drawing.Point(157, 60);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(175, 17);
            this.label6.TabIndex = 22;
            this.label6.Text = "検査範囲行　検反部№2：1～351";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("メイリオ", 8F);
            this.label7.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label7.Location = new System.Drawing.Point(157, 43);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(175, 17);
            this.label7.TabIndex = 21;
            this.label7.Text = "検査範囲行　検反部№1：1～351";
            // 
            // lblHansoEndDate
            // 
            this.lblHansoEndDate.AutoSize = true;
            this.lblHansoEndDate.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblHansoEndDate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblHansoEndDate.Location = new System.Drawing.Point(157, 26);
            this.lblHansoEndDate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHansoEndDate.Name = "lblHansoEndDate";
            this.lblHansoEndDate.Size = new System.Drawing.Size(207, 17);
            this.lblHansoEndDate.TabIndex = 20;
            this.lblHansoEndDate.Text = "搬送終了日時：2019/01/22 19:10:15";
            // 
            // lblHansoStaDate
            // 
            this.lblHansoStaDate.AutoSize = true;
            this.lblHansoStaDate.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblHansoStaDate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblHansoStaDate.Location = new System.Drawing.Point(157, 9);
            this.lblHansoStaDate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHansoStaDate.Name = "lblHansoStaDate";
            this.lblHansoStaDate.Size = new System.Drawing.Size(207, 17);
            this.lblHansoStaDate.TabIndex = 19;
            this.lblHansoStaDate.Text = "搬送開始日時：2019/01/22 16:30:15";
            // 
            // lblHanNo
            // 
            this.lblHanNo.AutoSize = true;
            this.lblHanNo.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblHanNo.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblHanNo.Location = new System.Drawing.Point(16, 60);
            this.lblHanNo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHanNo.Name = "lblHanNo";
            this.lblHanNo.Size = new System.Drawing.Size(113, 17);
            this.lblHanNo.TabIndex = 1;
            this.lblHanNo.Text = "反番：470068-０AC";
            // 
            // lblSashizu
            // 
            this.lblSashizu.AutoSize = true;
            this.lblSashizu.Font = new System.Drawing.Font("メイリオ", 9F);
            this.lblSashizu.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSashizu.Location = new System.Drawing.Point(16, 42);
            this.lblSashizu.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSashizu.Name = "lblSashizu";
            this.lblSashizu.Size = new System.Drawing.Size(93, 18);
            this.lblSashizu.TabIndex = 1;
            this.lblSashizu.Text = "指図：1070551";
            // 
            // lblShinaNm
            // 
            this.lblShinaNm.AutoSize = true;
            this.lblShinaNm.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblShinaNm.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblShinaNm.Location = new System.Drawing.Point(16, 26);
            this.lblShinaNm.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblShinaNm.Name = "lblShinaNm";
            this.lblShinaNm.Size = new System.Drawing.Size(73, 17);
            this.lblShinaNm.TabIndex = 1;
            this.lblShinaNm.Text = "品名：S115 ";
            // 
            // lblGoki
            // 
            this.lblGoki.AutoSize = true;
            this.lblGoki.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblGoki.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblGoki.Location = new System.Drawing.Point(16, 9);
            this.lblGoki.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblGoki.Name = "lblGoki";
            this.lblGoki.Size = new System.Drawing.Size(52, 17);
            this.lblGoki.TabIndex = 1;
            this.lblGoki.Text = "号機：１";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel4.Controls.Add(this.btnRegResult);
            this.panel4.Controls.Add(this.btnBackResultCheck);
            this.panel4.Controls.Add(this.btnBackTargetSelection);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 521);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1363, 78);
            this.panel4.TabIndex = 3;
            // 
            // btnRegResult
            // 
            this.btnRegResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRegResult.Font = new System.Drawing.Font("メイリオ", 9F);
            this.btnRegResult.Location = new System.Drawing.Point(1223, 18);
            this.btnRegResult.Margin = new System.Windows.Forms.Padding(2);
            this.btnRegResult.Name = "btnRegResult";
            this.btnRegResult.Size = new System.Drawing.Size(107, 36);
            this.btnRegResult.TabIndex = 0;
            this.btnRegResult.Text = "判定登録";
            this.btnRegResult.UseVisualStyleBackColor = true;
            this.btnRegResult.Click += new System.EventHandler(this.Button3_Click);
            // 
            // btnBackResultCheck
            // 
            this.btnBackResultCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBackResultCheck.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnBackResultCheck.Location = new System.Drawing.Point(1095, 18);
            this.btnBackResultCheck.Margin = new System.Windows.Forms.Padding(2);
            this.btnBackResultCheck.Name = "btnBackResultCheck";
            this.btnBackResultCheck.Size = new System.Drawing.Size(107, 36);
            this.btnBackResultCheck.TabIndex = 0;
            this.btnBackResultCheck.Text = "合否確認へ戻る";
            this.btnBackResultCheck.UseVisualStyleBackColor = true;
            this.btnBackResultCheck.Click += new System.EventHandler(this.Button2_Click);
            // 
            // btnBackTargetSelection
            // 
            this.btnBackTargetSelection.Font = new System.Drawing.Font("メイリオ", 9F);
            this.btnBackTargetSelection.Location = new System.Drawing.Point(18, 13);
            this.btnBackTargetSelection.Margin = new System.Windows.Forms.Padding(2);
            this.btnBackTargetSelection.Name = "btnBackTargetSelection";
            this.btnBackTargetSelection.Size = new System.Drawing.Size(107, 54);
            this.btnBackTargetSelection.TabIndex = 0;
            this.btnBackTargetSelection.Text = "検査対象\r\n選択へ戻る";
            this.btnBackTargetSelection.UseVisualStyleBackColor = true;
            this.btnBackTargetSelection.Click += new System.EventHandler(this.Button1_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 129);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(60, 392);
            this.panel2.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(1277, 129);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(86, 392);
            this.panel3.TabIndex = 5;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel6.Controls.Add(this.dgvData);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(60, 129);
            this.panel6.Margin = new System.Windows.Forms.Padding(2);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1217, 392);
            this.panel6.TabIndex = 6;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.ColumnHeadersHeight = 30;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.no,
            this.line,
            this.Column,
            this.ng,
            this.position,
            this.reason});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.EnableHeadersVisualStyles = false;
            this.dgvData.Location = new System.Drawing.Point(0, 0);
            this.dgvData.Margin = new System.Windows.Forms.Padding(2);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowHeadersWidth = 51;
            this.dgvData.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvData.RowTemplate.Height = 30;
            this.dgvData.RowTemplate.ReadOnly = true;
            this.dgvData.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.Size = new System.Drawing.Size(1217, 392);
            this.dgvData.TabIndex = 0;
            this.dgvData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            // 
            // no
            // 
            this.no.HeaderText = "№";
            this.no.MinimumWidth = 6;
            this.no.Name = "no";
            this.no.Width = 60;
            // 
            // line
            // 
            this.line.HeaderText = "行";
            this.line.MinimumWidth = 6;
            this.line.Name = "line";
            this.line.Width = 60;
            // 
            // Column
            // 
            this.Column.HeaderText = "列";
            this.Column.MinimumWidth = 6;
            this.Column.Name = "Column";
            this.Column.Width = 60;
            // 
            // ng
            // 
            this.ng.HeaderText = "NG面";
            this.ng.MinimumWidth = 6;
            this.ng.Name = "ng";
            this.ng.Width = 150;
            // 
            // position
            // 
            this.position.HeaderText = "位置（X,Y）cm";
            this.position.MinimumWidth = 6;
            this.position.Name = "position";
            this.position.Width = 130;
            // 
            // reason
            // 
            this.reason.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.reason.HeaderText = "NG理由";
            this.reason.MinimumWidth = 6;
            this.reason.Name = "reason";
            // 
            // Result
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1363, 599);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Result";
            this.Text = "判定登録";
            this.Load += new System.EventHandler(this.Result_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblHanNo;
        private System.Windows.Forms.Label lblSashizu;
        private System.Windows.Forms.Label lblShinaNm;
        private System.Windows.Forms.Label lblGoki;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.Label lblStaDate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblHansoEndDate;
        private System.Windows.Forms.Label lblHansoStaDate;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnRegResult;
        private System.Windows.Forms.Button btnBackResultCheck;
        private System.Windows.Forms.Button btnBackTargetSelection;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Button btnLogOut;
        private System.Windows.Forms.DataGridViewTextBoxColumn no;
        private System.Windows.Forms.DataGridViewTextBoxColumn line;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column;
        private System.Windows.Forms.DataGridViewTextBoxColumn ng;
        private System.Windows.Forms.DataGridViewTextBoxColumn position;
        private System.Windows.Forms.DataGridViewTextBoxColumn reason;
    }
}