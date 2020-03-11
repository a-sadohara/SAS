﻿namespace ImageChecker
{
    partial class Summary
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Summary));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblCushionCount = new System.Windows.Forms.Label();
            this.lblImageCount = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.lblWorkerName = new System.Windows.Forms.Label();
            this.lblDecisionEndTime = new System.Windows.Forms.Label();
            this.lblDecisionStartTime = new System.Windows.Forms.Label();
            this.lblInspectionLine = new System.Windows.Forms.Label();
            this.lblEndDatetime = new System.Windows.Forms.Label();
            this.lblStartDatetime = new System.Windows.Forms.Label();
            this.lblInspectionNum = new System.Windows.Forms.Label();
            this.lblFabricName = new System.Windows.Forms.Label();
            this.lblOrderImg = new System.Windows.Forms.Label();
            this.lblProductName = new System.Windows.Forms.Label();
            this.lblUnitNum = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnAcceptanceCheck = new System.Windows.Forms.Button();
            this.btnTargetSelection = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.dgvDecisionResult = new ImageChecker.MyDataGridView();
            this.No = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Line = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Columns = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NgFace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NgDistanceXY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OverDetectionExceptResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NgReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OverDetectionExceptDatetime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OverDetectionExceptWorker = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDecisionResult)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel1.Controls.Add(this.lblCushionCount);
            this.panel1.Controls.Add(this.lblImageCount);
            this.panel1.Controls.Add(this.btnLogout);
            this.panel1.Controls.Add(this.lblWorkerName);
            this.panel1.Controls.Add(this.lblDecisionEndTime);
            this.panel1.Controls.Add(this.lblDecisionStartTime);
            this.panel1.Controls.Add(this.lblInspectionLine);
            this.panel1.Controls.Add(this.lblEndDatetime);
            this.panel1.Controls.Add(this.lblStartDatetime);
            this.panel1.Controls.Add(this.lblInspectionNum);
            this.panel1.Controls.Add(this.lblFabricName);
            this.panel1.Controls.Add(this.lblOrderImg);
            this.panel1.Controls.Add(this.lblProductName);
            this.panel1.Controls.Add(this.lblUnitNum);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1284, 131);
            this.panel1.TabIndex = 0;
            // 
            // lblCushionCount
            // 
            this.lblCushionCount.AutoSize = true;
            this.lblCushionCount.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblCushionCount.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblCushionCount.Location = new System.Drawing.Point(187, 111);
            this.lblCushionCount.Name = "lblCushionCount";
            this.lblCushionCount.Size = new System.Drawing.Size(257, 17);
            this.lblCushionCount.TabIndex = 26;
            this.lblCushionCount.Text = "クッション数：9999（NG：9999 / OK：9999）";
            // 
            // lblImageCount
            // 
            this.lblImageCount.AutoSize = true;
            this.lblImageCount.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblImageCount.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblImageCount.Location = new System.Drawing.Point(187, 94);
            this.lblImageCount.Name = "lblImageCount";
            this.lblImageCount.Size = new System.Drawing.Size(225, 34);
            this.lblImageCount.TabIndex = 26;
            this.lblImageCount.Text = "画像検査枚数：999（NG：999 / OK：999\r\n）";
            // 
            // btnLogout
            // 
            this.btnLogout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogout.Font = new System.Drawing.Font("メイリオ", 9F);
            this.btnLogout.Location = new System.Drawing.Point(1166, 9);
            this.btnLogout.Margin = new System.Windows.Forms.Padding(2);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(107, 36);
            this.btnLogout.TabIndex = 0;
            this.btnLogout.Text = "ログアウト";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // lblWorkerName
            // 
            this.lblWorkerName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorkerName.AutoSize = true;
            this.lblWorkerName.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblWorkerName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblWorkerName.Location = new System.Drawing.Point(862, 19);
            this.lblWorkerName.Margin = new System.Windows.Forms.Padding(2);
            this.lblWorkerName.MaximumSize = new System.Drawing.Size(300, 0);
            this.lblWorkerName.MinimumSize = new System.Drawing.Size(300, 0);
            this.lblWorkerName.Name = "lblWorkerName";
            this.lblWorkerName.Size = new System.Drawing.Size(300, 17);
            this.lblWorkerName.TabIndex = 25;
            this.lblWorkerName.Text = "作業者名：XXXXXXXX";
            this.lblWorkerName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDecisionEndTime
            // 
            this.lblDecisionEndTime.AutoSize = true;
            this.lblDecisionEndTime.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblDecisionEndTime.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblDecisionEndTime.Location = new System.Drawing.Point(187, 77);
            this.lblDecisionEndTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDecisionEndTime.Name = "lblDecisionEndTime";
            this.lblDecisionEndTime.Size = new System.Drawing.Size(207, 17);
            this.lblDecisionEndTime.TabIndex = 24;
            this.lblDecisionEndTime.Text = "判定終了日時：2999/12/31 23:59:59";
            // 
            // lblDecisionStartTime
            // 
            this.lblDecisionStartTime.AutoSize = true;
            this.lblDecisionStartTime.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblDecisionStartTime.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblDecisionStartTime.Location = new System.Drawing.Point(187, 60);
            this.lblDecisionStartTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDecisionStartTime.Name = "lblDecisionStartTime";
            this.lblDecisionStartTime.Size = new System.Drawing.Size(207, 17);
            this.lblDecisionStartTime.TabIndex = 23;
            this.lblDecisionStartTime.Text = "判定開始日時：2999/12/31 23:59:59";
            // 
            // lblInspectionLine
            // 
            this.lblInspectionLine.AutoSize = true;
            this.lblInspectionLine.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblInspectionLine.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblInspectionLine.Location = new System.Drawing.Point(187, 43);
            this.lblInspectionLine.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInspectionLine.Name = "lblInspectionLine";
            this.lblInspectionLine.Size = new System.Drawing.Size(152, 17);
            this.lblInspectionLine.TabIndex = 21;
            this.lblInspectionLine.Text = "検査範囲行　：9999～9999";
            // 
            // lblEndDatetime
            // 
            this.lblEndDatetime.AutoSize = true;
            this.lblEndDatetime.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblEndDatetime.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblEndDatetime.Location = new System.Drawing.Point(187, 26);
            this.lblEndDatetime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEndDatetime.Name = "lblEndDatetime";
            this.lblEndDatetime.Size = new System.Drawing.Size(207, 17);
            this.lblEndDatetime.TabIndex = 20;
            this.lblEndDatetime.Text = "搬送終了日時：2999/12/31 23:59:59";
            // 
            // lblStartDatetime
            // 
            this.lblStartDatetime.AutoSize = true;
            this.lblStartDatetime.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblStartDatetime.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblStartDatetime.Location = new System.Drawing.Point(187, 9);
            this.lblStartDatetime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStartDatetime.Name = "lblStartDatetime";
            this.lblStartDatetime.Size = new System.Drawing.Size(207, 17);
            this.lblStartDatetime.TabIndex = 19;
            this.lblStartDatetime.Text = "搬送開始日時：2999/12/31 23:59:59";
            // 
            // lblInspectionNum
            // 
            this.lblInspectionNum.AutoSize = true;
            this.lblInspectionNum.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblInspectionNum.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblInspectionNum.Location = new System.Drawing.Point(422, 9);
            this.lblInspectionNum.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInspectionNum.Name = "lblInspectionNum";
            this.lblInspectionNum.Size = new System.Drawing.Size(77, 17);
            this.lblInspectionNum.TabIndex = 1;
            this.lblInspectionNum.Text = "検査番号：99";
            // 
            // lblFabricName
            // 
            this.lblFabricName.AutoSize = true;
            this.lblFabricName.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblFabricName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblFabricName.Location = new System.Drawing.Point(16, 60);
            this.lblFabricName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFabricName.Name = "lblFabricName";
            this.lblFabricName.Size = new System.Drawing.Size(117, 17);
            this.lblFabricName.TabIndex = 1;
            this.lblFabricName.Text = "反番：999999-0WW";
            // 
            // lblOrderImg
            // 
            this.lblOrderImg.AutoSize = true;
            this.lblOrderImg.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblOrderImg.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblOrderImg.Location = new System.Drawing.Point(16, 42);
            this.lblOrderImg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblOrderImg.Name = "lblOrderImg";
            this.lblOrderImg.Size = new System.Drawing.Size(90, 17);
            this.lblOrderImg.TabIndex = 1;
            this.lblOrderImg.Text = "指図：9999999";
            // 
            // lblProductName
            // 
            this.lblProductName.AutoSize = true;
            this.lblProductName.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblProductName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblProductName.Location = new System.Drawing.Point(16, 26);
            this.lblProductName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(73, 17);
            this.lblProductName.TabIndex = 1;
            this.lblProductName.Text = "品名：W999";
            // 
            // lblUnitNum
            // 
            this.lblUnitNum.AutoSize = true;
            this.lblUnitNum.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblUnitNum.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblUnitNum.Location = new System.Drawing.Point(16, 9);
            this.lblUnitNum.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblUnitNum.Name = "lblUnitNum";
            this.lblUnitNum.Size = new System.Drawing.Size(63, 17);
            this.lblUnitNum.TabIndex = 1;
            this.lblUnitNum.Text = "号機：WW";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel4.Controls.Add(this.btnAcceptanceCheck);
            this.panel4.Controls.Add(this.btnTargetSelection);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 508);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1284, 91);
            this.panel4.TabIndex = 3;
            // 
            // btnAcceptanceCheck
            // 
            this.btnAcceptanceCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAcceptanceCheck.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnAcceptanceCheck.Location = new System.Drawing.Point(1121, 13);
            this.btnAcceptanceCheck.Margin = new System.Windows.Forms.Padding(2);
            this.btnAcceptanceCheck.Name = "btnAcceptanceCheck";
            this.btnAcceptanceCheck.Size = new System.Drawing.Size(146, 54);
            this.btnAcceptanceCheck.TabIndex = 0;
            this.btnAcceptanceCheck.Text = "合否確認";
            this.btnAcceptanceCheck.UseVisualStyleBackColor = true;
            this.btnAcceptanceCheck.Click += new System.EventHandler(this.btnAcceptanceCheck_Click);
            // 
            // btnTargetSelection
            // 
            this.btnTargetSelection.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnTargetSelection.Location = new System.Drawing.Point(18, 13);
            this.btnTargetSelection.Margin = new System.Windows.Forms.Padding(2);
            this.btnTargetSelection.Name = "btnTargetSelection";
            this.btnTargetSelection.Size = new System.Drawing.Size(146, 54);
            this.btnTargetSelection.TabIndex = 0;
            this.btnTargetSelection.Text = "検査対象\r\n選択へ戻る";
            this.btnTargetSelection.UseVisualStyleBackColor = true;
            this.btnTargetSelection.Click += new System.EventHandler(this.btnTargetSelection_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 131);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(40, 377);
            this.panel2.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(1227, 131);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(57, 377);
            this.panel3.TabIndex = 5;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel6.Controls.Add(this.dgvDecisionResult);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(40, 131);
            this.panel6.Margin = new System.Windows.Forms.Padding(2);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1187, 377);
            this.panel6.TabIndex = 6;
            // 
            // dgvDecisionResult
            // 
            this.dgvDecisionResult.AllowUserToAddRows = false;
            this.dgvDecisionResult.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDecisionResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDecisionResult.ColumnHeadersHeight = 30;
            this.dgvDecisionResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.No,
            this.Line,
            this.Columns,
            this.NgFace,
            this.NgDistanceXY,
            this.OverDetectionExceptResult,
            this.NgReason,
            this.OverDetectionExceptDatetime,
            this.OverDetectionExceptWorker});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDecisionResult.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDecisionResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDecisionResult.EnableHeadersVisualStyles = false;
            this.dgvDecisionResult.Location = new System.Drawing.Point(0, 0);
            this.dgvDecisionResult.Margin = new System.Windows.Forms.Padding(2);
            this.dgvDecisionResult.MultiSelect = false;
            this.dgvDecisionResult.Name = "dgvDecisionResult";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDecisionResult.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvDecisionResult.RowHeadersVisible = false;
            this.dgvDecisionResult.RowHeadersWidth = 51;
            this.dgvDecisionResult.RowTemplate.Height = 75;
            this.dgvDecisionResult.RowTemplate.ReadOnly = true;
            this.dgvDecisionResult.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDecisionResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDecisionResult.Size = new System.Drawing.Size(1187, 377);
            this.dgvDecisionResult.TabIndex = 0;
            this.dgvDecisionResult.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDecisionResult_CellClick);
            this.dgvDecisionResult.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDecisionResult_CellDoubleClick);
            this.dgvDecisionResult.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgvDecisionResult_MouseDown);
            this.dgvDecisionResult.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgvDecisionResult_MouseMove);
            // 
            // No
            // 
            this.No.HeaderText = "№";
            this.No.MinimumWidth = 6;
            this.No.Name = "No";
            this.No.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.No.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Line
            // 
            this.Line.HeaderText = "行";
            this.Line.MinimumWidth = 6;
            this.Line.Name = "Line";
            this.Line.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Line.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Columns
            // 
            this.Columns.HeaderText = "列";
            this.Columns.MinimumWidth = 6;
            this.Columns.Name = "Columns";
            this.Columns.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Columns.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // NgFace
            // 
            this.NgFace.HeaderText = "NG面";
            this.NgFace.MinimumWidth = 6;
            this.NgFace.Name = "NgFace";
            this.NgFace.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.NgFace.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.NgFace.Width = 120;
            // 
            // NgDistanceXY
            // 
            this.NgDistanceXY.HeaderText = "位置(X,Y)cm";
            this.NgDistanceXY.MinimumWidth = 6;
            this.NgDistanceXY.Name = "NgDistanceXY";
            this.NgDistanceXY.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.NgDistanceXY.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.NgDistanceXY.Width = 200;
            // 
            // OverDetectionExceptResult
            // 
            this.OverDetectionExceptResult.HeaderText = "過検知除外";
            this.OverDetectionExceptResult.Name = "OverDetectionExceptResult";
            this.OverDetectionExceptResult.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.OverDetectionExceptResult.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.OverDetectionExceptResult.Width = 200;
            // 
            // NgReason
            // 
            this.NgReason.HeaderText = "NG理由";
            this.NgReason.Name = "NgReason";
            this.NgReason.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.NgReason.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.NgReason.Width = 260;
            // 
            // OverDetectionExceptDatetime
            // 
            this.OverDetectionExceptDatetime.HeaderText = "過検知検査時刻";
            this.OverDetectionExceptDatetime.Name = "OverDetectionExceptDatetime";
            this.OverDetectionExceptDatetime.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.OverDetectionExceptDatetime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.OverDetectionExceptDatetime.Width = 300;
            // 
            // OverDetectionExceptWorker
            // 
            this.OverDetectionExceptWorker.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.OverDetectionExceptWorker.HeaderText = "過検知検査作業者";
            this.OverDetectionExceptWorker.Name = "OverDetectionExceptWorker";
            this.OverDetectionExceptWorker.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.OverDetectionExceptWorker.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Summary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 599);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Summary";
            this.Text = "過検知除外結果";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Summary_FormClosing);
            this.Load += new System.EventHandler(this.Summary_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDecisionResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblFabricName;
        private System.Windows.Forms.Label lblOrderImg;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.Label lblUnitNum;
        private System.Windows.Forms.Label lblDecisionEndTime;
        private System.Windows.Forms.Label lblDecisionStartTime;
        private System.Windows.Forms.Label lblInspectionLine;
        private System.Windows.Forms.Label lblEndDatetime;
        private System.Windows.Forms.Label lblStartDatetime;
        private System.Windows.Forms.Label lblWorkerName;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnAcceptanceCheck;
        private System.Windows.Forms.Button btnTargetSelection;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel6;
        //private System.Windows.Forms.DataGridView dgvDecisionResult;
        private MyDataGridView dgvDecisionResult;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblImageCount;
        private System.Windows.Forms.Label lblInspectionNum;
        private System.Windows.Forms.Label lblCushionCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn No;
        private System.Windows.Forms.DataGridViewTextBoxColumn Line;
        private System.Windows.Forms.DataGridViewTextBoxColumn Columns;
        private System.Windows.Forms.DataGridViewTextBoxColumn NgFace;
        private System.Windows.Forms.DataGridViewTextBoxColumn NgDistanceXY;
        private System.Windows.Forms.DataGridViewTextBoxColumn OverDetectionExceptResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn NgReason;
        private System.Windows.Forms.DataGridViewTextBoxColumn OverDetectionExceptDatetime;
        private System.Windows.Forms.DataGridViewTextBoxColumn OverDetectionExceptWorker;
    }
}