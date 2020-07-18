namespace RecoveryTool
{
    partial class RecoveryScreen
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecoveryScreen));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblUnitNum = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnRecovery = new System.Windows.Forms.Button();
            this.lblExecutionResult = new System.Windows.Forms.Label();
            this.txtExecutionResult = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.chkSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dtInspectionDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.intInspectionNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtFabricName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dtImagingStarttime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.lblUnitNum);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(567, 19);
            this.panel1.TabIndex = 0;
            // 
            // lblUnitNum
            // 
            this.lblUnitNum.AutoSize = true;
            this.lblUnitNum.Location = new System.Drawing.Point(20, 5);
            this.lblUnitNum.Name = "lblUnitNum";
            this.lblUnitNum.Size = new System.Drawing.Size(37, 12);
            this.lblUnitNum.TabIndex = 8;
            this.lblUnitNum.Text = "N号機";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.Controls.Add(this.btnRecovery);
            this.panel2.Controls.Add(this.lblExecutionResult);
            this.panel2.Controls.Add(this.txtExecutionResult);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 251);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(567, 524);
            this.panel2.TabIndex = 1;
            // 
            // btnRecovery
            // 
            this.btnRecovery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRecovery.Location = new System.Drawing.Point(391, 8);
            this.btnRecovery.Margin = new System.Windows.Forms.Padding(2);
            this.btnRecovery.Name = "btnRecovery";
            this.btnRecovery.Size = new System.Drawing.Size(154, 37);
            this.btnRecovery.TabIndex = 0;
            this.btnRecovery.Text = "リセット";
            this.btnRecovery.UseVisualStyleBackColor = true;
            this.btnRecovery.Click += new System.EventHandler(this.btnRecovery_Click);
            // 
            // lblExecutionResult
            // 
            this.lblExecutionResult.AutoSize = true;
            this.lblExecutionResult.Location = new System.Drawing.Point(20, 38);
            this.lblExecutionResult.Name = "lblExecutionResult";
            this.lblExecutionResult.Size = new System.Drawing.Size(53, 12);
            this.lblExecutionResult.TabIndex = 7;
            this.lblExecutionResult.Text = "実行結果";
            // 
            // txtExecutionResult
            // 
            this.txtExecutionResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExecutionResult.BackColor = System.Drawing.SystemColors.Control;
            this.txtExecutionResult.Location = new System.Drawing.Point(22, 53);
            this.txtExecutionResult.Multiline = true;
            this.txtExecutionResult.Name = "txtExecutionResult";
            this.txtExecutionResult.ReadOnly = true;
            this.txtExecutionResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExecutionResult.Size = new System.Drawing.Size(523, 449);
            this.txtExecutionResult.TabIndex = 5;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.Control;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 19);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(22, 232);
            this.panel3.TabIndex = 2;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.SystemColors.Control;
            this.panel4.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel4.Location = new System.Drawing.Point(545, 19);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(22, 232);
            this.panel4.TabIndex = 3;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.SystemColors.Control;
            this.panel5.Controls.Add(this.dgvData);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(22, 19);
            this.panel5.Margin = new System.Windows.Forms.Padding(2);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(523, 232);
            this.panel5.TabIndex = 4;
            // 
            // dgvData
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.ColumnHeadersHeight = 20;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chkSelect,
            this.dtInspectionDate,
            this.intInspectionNum,
            this.txtFabricName,
            this.dtImagingStarttime});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.EnableHeadersVisualStyles = false;
            this.dgvData.Location = new System.Drawing.Point(0, 0);
            this.dgvData.Margin = new System.Windows.Forms.Padding(2);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowHeadersWidth = 21;
            this.dgvData.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvData.RowTemplate.Height = 21;
            this.dgvData.RowTemplate.ReadOnly = true;
            this.dgvData.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(523, 232);
            this.dgvData.TabIndex = 0;
            this.dgvData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellClick);
            // 
            // chkSelect
            // 
            this.chkSelect.HeaderText = "";
            this.chkSelect.MinimumWidth = 6;
            this.chkSelect.Name = "chkSelect";
            this.chkSelect.ReadOnly = true;
            this.chkSelect.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.chkSelect.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.chkSelect.Width = 40;
            // 
            // dtInspectionDate
            // 
            dataGridViewCellStyle2.Format = "d";
            dataGridViewCellStyle2.NullValue = null;
            this.dtInspectionDate.DefaultCellStyle = dataGridViewCellStyle2;
            this.dtInspectionDate.HeaderText = "検査日付";
            this.dtInspectionDate.MinimumWidth = 6;
            this.dtInspectionDate.Name = "dtInspectionDate";
            this.dtInspectionDate.ReadOnly = true;
            this.dtInspectionDate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dtInspectionDate.Width = 110;
            // 
            // intInspectionNum
            // 
            this.intInspectionNum.HeaderText = "検査番号";
            this.intInspectionNum.MinimumWidth = 6;
            this.intInspectionNum.Name = "intInspectionNum";
            this.intInspectionNum.ReadOnly = true;
            this.intInspectionNum.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // txtFabricName
            // 
            this.txtFabricName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.txtFabricName.HeaderText = "反番";
            this.txtFabricName.MinimumWidth = 6;
            this.txtFabricName.Name = "txtFabricName";
            this.txtFabricName.ReadOnly = true;
            this.txtFabricName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dtImagingStarttime
            // 
            this.dtImagingStarttime.HeaderText = "撮像開始時刻";
            this.dtImagingStarttime.MinimumWidth = 6;
            this.dtImagingStarttime.Name = "dtImagingStarttime";
            this.dtImagingStarttime.ReadOnly = true;
            this.dtImagingStarttime.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dtImagingStarttime.Visible = false;
            this.dtImagingStarttime.Width = 110;
            // 
            // RecoveryScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.ClientSize = new System.Drawing.Size(567, 775);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "RecoveryScreen";
            this.Text = "リセットツール";
            this.Load += new System.EventHandler(this.RecoveryScreen_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button btnRecovery;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TextBox txtExecutionResult;
        private System.Windows.Forms.Label lblExecutionResult;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtInspectionDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn intInspectionNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn txtFabricName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dtImagingStarttime;
        private System.Windows.Forms.Label lblUnitNum;
    }
}