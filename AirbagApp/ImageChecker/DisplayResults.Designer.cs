namespace ImageChecker
{
    partial class DisplayResults
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DisplayResults));
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.grbWhere = new System.Windows.Forms.GroupBox();
            this.lblTitleUserNm = new System.Windows.Forms.Label();
            this.lblNgReason = new System.Windows.Forms.Label();
            this.txtNgReason = new System.Windows.Forms.TextBox();
            this.lblNgSide = new System.Windows.Forms.Label();
            this.lblCol = new System.Windows.Forms.Label();
            this.lblRow = new System.Windows.Forms.Label();
            this.txtWorkerName = new System.Windows.Forms.TextBox();
            this.lblTitleSearchCushionCnt = new System.Windows.Forms.Label();
            this.txtLine = new System.Windows.Forms.TextBox();
            this.lblTitleSearchImageCnt = new System.Windows.Forms.Label();
            this.lblCushionSearchCount = new System.Windows.Forms.Label();
            this.lblImageSearchCount = new System.Windows.Forms.Label();
            this.lblTitleSearchCnt = new System.Windows.Forms.Label();
            this.cmbColumns = new System.Windows.Forms.ComboBox();
            this.cmbNgFace = new System.Windows.Forms.ComboBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.lblWorkerName = new System.Windows.Forms.Label();
            this.lblDecisionEndTime = new System.Windows.Forms.Label();
            this.lblDecisionStartTime = new System.Windows.Forms.Label();
            this.lblInspectionLine = new System.Windows.Forms.Label();
            this.lblEndDatetime = new System.Windows.Forms.Label();
            this.lblStartDatetime = new System.Windows.Forms.Label();
            this.lblCushionCount = new System.Windows.Forms.Label();
            this.lblImageCount = new System.Windows.Forms.Label();
            this.lblInspectionNum = new System.Windows.Forms.Label();
            this.lblFabricName = new System.Windows.Forms.Label();
            this.lblOrderImg = new System.Windows.Forms.Label();
            this.lblProductName = new System.Windows.Forms.Label();
            this.lblUnitNum = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnReprint = new System.Windows.Forms.Button();
            this.btnInspectionUpdate = new System.Windows.Forms.Button();
            this.btnTargetSelection = new System.Windows.Forms.Button();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.dgvDecisionResult = new ImageChecker.MyDataGridView();
            this.No = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Row = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NGArea = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvLocationXY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SearchExclude = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Check = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NGReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SearchTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SearchUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CheckTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CheckUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UpdateUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UpdateNGReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlTop.SuspendLayout();
            this.grbWhere.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDecisionResult)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.pnlTop.Controls.Add(this.btnLogout);
            this.pnlTop.Controls.Add(this.grbWhere);
            this.pnlTop.Controls.Add(this.btnSearch);
            this.pnlTop.Controls.Add(this.lblWorkerName);
            this.pnlTop.Controls.Add(this.lblDecisionEndTime);
            this.pnlTop.Controls.Add(this.lblDecisionStartTime);
            this.pnlTop.Controls.Add(this.lblInspectionLine);
            this.pnlTop.Controls.Add(this.lblEndDatetime);
            this.pnlTop.Controls.Add(this.lblStartDatetime);
            this.pnlTop.Controls.Add(this.lblCushionCount);
            this.pnlTop.Controls.Add(this.lblImageCount);
            this.pnlTop.Controls.Add(this.lblInspectionNum);
            this.pnlTop.Controls.Add(this.lblFabricName);
            this.pnlTop.Controls.Add(this.lblOrderImg);
            this.pnlTop.Controls.Add(this.lblProductName);
            this.pnlTop.Controls.Add(this.lblUnitNum);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Margin = new System.Windows.Forms.Padding(2);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1276, 156);
            this.pnlTop.TabIndex = 0;
            // 
            // btnLogout
            // 
            this.btnLogout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogout.Font = new System.Drawing.Font("メイリオ", 9F);
            this.btnLogout.Location = new System.Drawing.Point(1158, 9);
            this.btnLogout.Margin = new System.Windows.Forms.Padding(2);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(107, 36);
            this.btnLogout.TabIndex = 0;
            this.btnLogout.Text = "ログアウト";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // grbWhere
            // 
            this.grbWhere.Controls.Add(this.lblTitleUserNm);
            this.grbWhere.Controls.Add(this.lblNgReason);
            this.grbWhere.Controls.Add(this.txtNgReason);
            this.grbWhere.Controls.Add(this.lblNgSide);
            this.grbWhere.Controls.Add(this.lblCol);
            this.grbWhere.Controls.Add(this.lblRow);
            this.grbWhere.Controls.Add(this.txtWorkerName);
            this.grbWhere.Controls.Add(this.lblTitleSearchCushionCnt);
            this.grbWhere.Controls.Add(this.txtLine);
            this.grbWhere.Controls.Add(this.lblTitleSearchImageCnt);
            this.grbWhere.Controls.Add(this.lblCushionSearchCount);
            this.grbWhere.Controls.Add(this.lblImageSearchCount);
            this.grbWhere.Controls.Add(this.lblTitleSearchCnt);
            this.grbWhere.Controls.Add(this.cmbColumns);
            this.grbWhere.Controls.Add(this.cmbNgFace);
            this.grbWhere.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.grbWhere.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.grbWhere.Location = new System.Drawing.Point(448, 4);
            this.grbWhere.Name = "grbWhere";
            this.grbWhere.Size = new System.Drawing.Size(530, 146);
            this.grbWhere.TabIndex = 28;
            this.grbWhere.TabStop = false;
            this.grbWhere.Text = "検索条件";
            // 
            // lblTitleUserNm
            // 
            this.lblTitleUserNm.AutoSize = true;
            this.lblTitleUserNm.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblTitleUserNm.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblTitleUserNm.Location = new System.Drawing.Point(21, 29);
            this.lblTitleUserNm.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitleUserNm.Name = "lblTitleUserNm";
            this.lblTitleUserNm.Size = new System.Drawing.Size(63, 17);
            this.lblTitleUserNm.TabIndex = 1;
            this.lblTitleUserNm.Text = "作業者名：";
            // 
            // lblNgReason
            // 
            this.lblNgReason.AutoSize = true;
            this.lblNgReason.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblNgReason.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblNgReason.Location = new System.Drawing.Point(244, 55);
            this.lblNgReason.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblNgReason.Name = "lblNgReason";
            this.lblNgReason.Size = new System.Drawing.Size(57, 17);
            this.lblNgReason.TabIndex = 1;
            this.lblNgReason.Text = "NG理由：";
            // 
            // txtNgReason
            // 
            this.txtNgReason.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtNgReason.Location = new System.Drawing.Point(305, 52);
            this.txtNgReason.MaxLength = 16;
            this.txtNgReason.Name = "txtNgReason";
            this.txtNgReason.Size = new System.Drawing.Size(185, 24);
            this.txtNgReason.TabIndex = 26;
            this.txtNgReason.Text = "ＷＷＷＷＷＷＷＷＷＷＷＷＷＷＷＷ";
            this.txtNgReason.DoubleClick += new System.EventHandler(this.txtNgReason_DoubleClick);
            // 
            // lblNgSide
            // 
            this.lblNgSide.AutoSize = true;
            this.lblNgSide.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblNgSide.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblNgSide.Location = new System.Drawing.Point(244, 29);
            this.lblNgSide.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblNgSide.Name = "lblNgSide";
            this.lblNgSide.Size = new System.Drawing.Size(46, 17);
            this.lblNgSide.TabIndex = 1;
            this.lblNgSide.Text = "NG面：";
            // 
            // lblCol
            // 
            this.lblCol.AutoSize = true;
            this.lblCol.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblCol.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblCol.Location = new System.Drawing.Point(21, 81);
            this.lblCol.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCol.Name = "lblCol";
            this.lblCol.Size = new System.Drawing.Size(30, 17);
            this.lblCol.TabIndex = 1;
            this.lblCol.Text = "列：";
            // 
            // lblRow
            // 
            this.lblRow.AutoSize = true;
            this.lblRow.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblRow.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblRow.Location = new System.Drawing.Point(21, 55);
            this.lblRow.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRow.Name = "lblRow";
            this.lblRow.Size = new System.Drawing.Size(30, 17);
            this.lblRow.TabIndex = 1;
            this.lblRow.Text = "行：";
            // 
            // txtWorkerName
            // 
            this.txtWorkerName.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtWorkerName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtWorkerName.Location = new System.Drawing.Point(89, 26);
            this.txtWorkerName.MaxLength = 10;
            this.txtWorkerName.Name = "txtWorkerName";
            this.txtWorkerName.Size = new System.Drawing.Size(119, 24);
            this.txtWorkerName.TabIndex = 26;
            this.txtWorkerName.Text = "ＷＷＷＷＷＷＷＷＷＷ";
            this.txtWorkerName.DoubleClick += new System.EventHandler(this.txtWorkerName_DoubleClick);
            this.txtWorkerName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtWorkerName_KeyPress);
            // 
            // lblTitleSearchCushionCnt
            // 
            this.lblTitleSearchCushionCnt.AutoSize = true;
            this.lblTitleSearchCushionCnt.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblTitleSearchCushionCnt.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblTitleSearchCushionCnt.Location = new System.Drawing.Point(272, 119);
            this.lblTitleSearchCushionCnt.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitleSearchCushionCnt.Name = "lblTitleSearchCushionCnt";
            this.lblTitleSearchCushionCnt.Size = new System.Drawing.Size(85, 17);
            this.lblTitleSearchCushionCnt.TabIndex = 1;
            this.lblTitleSearchCushionCnt.Text = "クッション数：";
            // 
            // txtLine
            // 
            this.txtLine.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtLine.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.txtLine.Location = new System.Drawing.Point(89, 52);
            this.txtLine.MaxLength = 4;
            this.txtLine.Name = "txtLine";
            this.txtLine.Size = new System.Drawing.Size(53, 24);
            this.txtLine.TabIndex = 26;
            this.txtLine.Text = "9999";
            this.txtLine.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLine_KeyPress);
            // 
            // lblTitleSearchImageCnt
            // 
            this.lblTitleSearchImageCnt.AutoSize = true;
            this.lblTitleSearchImageCnt.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblTitleSearchImageCnt.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblTitleSearchImageCnt.Location = new System.Drawing.Point(272, 102);
            this.lblTitleSearchImageCnt.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitleSearchImageCnt.Name = "lblTitleSearchImageCnt";
            this.lblTitleSearchImageCnt.Size = new System.Drawing.Size(85, 17);
            this.lblTitleSearchImageCnt.TabIndex = 1;
            this.lblTitleSearchImageCnt.Text = "画像検査枚数：";
            // 
            // lblCushionSearchCount
            // 
            this.lblCushionSearchCount.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblCushionSearchCount.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblCushionSearchCount.Location = new System.Drawing.Point(361, 119);
            this.lblCushionSearchCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCushionSearchCount.Name = "lblCushionSearchCount";
            this.lblCushionSearchCount.Size = new System.Drawing.Size(77, 17);
            this.lblCushionSearchCount.TabIndex = 1;
            this.lblCushionSearchCount.Text = "9999 / 9999";
            this.lblCushionSearchCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblImageSearchCount
            // 
            this.lblImageSearchCount.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblImageSearchCount.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblImageSearchCount.Location = new System.Drawing.Point(361, 102);
            this.lblImageSearchCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblImageSearchCount.Name = "lblImageSearchCount";
            this.lblImageSearchCount.Size = new System.Drawing.Size(77, 17);
            this.lblImageSearchCount.TabIndex = 1;
            this.lblImageSearchCount.Text = "9999 / 9999";
            this.lblImageSearchCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTitleSearchCnt
            // 
            this.lblTitleSearchCnt.AutoSize = true;
            this.lblTitleSearchCnt.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblTitleSearchCnt.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblTitleSearchCnt.Location = new System.Drawing.Point(244, 85);
            this.lblTitleSearchCnt.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitleSearchCnt.Name = "lblTitleSearchCnt";
            this.lblTitleSearchCnt.Size = new System.Drawing.Size(63, 17);
            this.lblTitleSearchCnt.TabIndex = 1;
            this.lblTitleSearchCnt.Text = "検索件数：";
            // 
            // cmbColumns
            // 
            this.cmbColumns.Font = new System.Drawing.Font("メイリオ", 8.25F);
            this.cmbColumns.FormattingEnabled = true;
            this.cmbColumns.Items.AddRange(new object[] {
            "",
            "A",
            "B",
            "C",
            "D",
            "E"});
            this.cmbColumns.Location = new System.Drawing.Point(89, 78);
            this.cmbColumns.Name = "cmbColumns";
            this.cmbColumns.Size = new System.Drawing.Size(53, 25);
            this.cmbColumns.TabIndex = 27;
            this.cmbColumns.Text = "W";
            // 
            // cmbNgFace
            // 
            this.cmbNgFace.Font = new System.Drawing.Font("メイリオ", 8.25F);
            this.cmbNgFace.FormattingEnabled = true;
            this.cmbNgFace.Items.AddRange(new object[] {
            "",
            "#1",
            "#2"});
            this.cmbNgFace.Location = new System.Drawing.Point(305, 26);
            this.cmbNgFace.Name = "cmbNgFace";
            this.cmbNgFace.Size = new System.Drawing.Size(53, 25);
            this.cmbNgFace.TabIndex = 27;
            this.cmbNgFace.Text = "WW";
            // 
            // btnSearch
            // 
            this.btnSearch.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnSearch.Location = new System.Drawing.Point(1001, 114);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(2);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(107, 36);
            this.btnSearch.TabIndex = 0;
            this.btnSearch.Text = "検索";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // lblWorkerName
            // 
            this.lblWorkerName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorkerName.AutoSize = true;
            this.lblWorkerName.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblWorkerName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblWorkerName.Location = new System.Drawing.Point(854, 19);
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
            // lblCushionCount
            // 
            this.lblCushionCount.AutoSize = true;
            this.lblCushionCount.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblCushionCount.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblCushionCount.Location = new System.Drawing.Point(187, 111);
            this.lblCushionCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCushionCount.Name = "lblCushionCount";
            this.lblCushionCount.Size = new System.Drawing.Size(257, 17);
            this.lblCushionCount.TabIndex = 1;
            this.lblCushionCount.Text = "クッション数：9999（NG：9999 / OK：9999）";
            // 
            // lblImageCount
            // 
            this.lblImageCount.AutoSize = true;
            this.lblImageCount.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblImageCount.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblImageCount.Location = new System.Drawing.Point(187, 94);
            this.lblImageCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblImageCount.Name = "lblImageCount";
            this.lblImageCount.Size = new System.Drawing.Size(243, 17);
            this.lblImageCount.TabIndex = 1;
            this.lblImageCount.Text = "画像検査枚数：9999（NG：999 / OK：999）";
            // 
            // lblInspectionNum
            // 
            this.lblInspectionNum.AutoSize = true;
            this.lblInspectionNum.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblInspectionNum.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblInspectionNum.Location = new System.Drawing.Point(187, 128);
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
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.pnlBottom.Controls.Add(this.btnReprint);
            this.pnlBottom.Controls.Add(this.btnInspectionUpdate);
            this.pnlBottom.Controls.Add(this.btnTargetSelection);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 521);
            this.pnlBottom.Margin = new System.Windows.Forms.Padding(2);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1276, 78);
            this.pnlBottom.TabIndex = 3;
            // 
            // btnReprint
            // 
            this.btnReprint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReprint.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnReprint.Location = new System.Drawing.Point(988, 13);
            this.btnReprint.Margin = new System.Windows.Forms.Padding(2);
            this.btnReprint.Name = "btnReprint";
            this.btnReprint.Size = new System.Drawing.Size(127, 54);
            this.btnReprint.TabIndex = 0;
            this.btnReprint.Text = "再印刷";
            this.btnReprint.UseVisualStyleBackColor = true;
            this.btnReprint.Click += new System.EventHandler(this.btnReprint_Click);
            // 
            // btnInspectionUpdate
            // 
            this.btnInspectionUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInspectionUpdate.Font = new System.Drawing.Font("メイリオ", 9F);
            this.btnInspectionUpdate.Location = new System.Drawing.Point(1131, 13);
            this.btnInspectionUpdate.Margin = new System.Windows.Forms.Padding(2);
            this.btnInspectionUpdate.Name = "btnInspectionUpdate";
            this.btnInspectionUpdate.Size = new System.Drawing.Size(127, 54);
            this.btnInspectionUpdate.TabIndex = 1;
            this.btnInspectionUpdate.Text = "検査結果を\r\n更新する";
            this.btnInspectionUpdate.UseVisualStyleBackColor = true;
            this.btnInspectionUpdate.Click += new System.EventHandler(this.btnInspectionUpdate_Click);
            // 
            // btnTargetSelection
            // 
            this.btnTargetSelection.Font = new System.Drawing.Font("メイリオ", 9F);
            this.btnTargetSelection.Location = new System.Drawing.Point(18, 13);
            this.btnTargetSelection.Margin = new System.Windows.Forms.Padding(2);
            this.btnTargetSelection.Name = "btnTargetSelection";
            this.btnTargetSelection.Size = new System.Drawing.Size(127, 54);
            this.btnTargetSelection.TabIndex = 0;
            this.btnTargetSelection.Text = "検査対象\r\n選択へ戻る";
            this.btnTargetSelection.UseVisualStyleBackColor = true;
            this.btnTargetSelection.Click += new System.EventHandler(this.btnTargetSelection_Click);
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 156);
            this.pnlLeft.Margin = new System.Windows.Forms.Padding(2);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(40, 365);
            this.pnlLeft.TabIndex = 4;
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRight.Location = new System.Drawing.Point(1219, 156);
            this.pnlRight.Margin = new System.Windows.Forms.Padding(2);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(57, 365);
            this.pnlRight.TabIndex = 5;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.panel6.Controls.Add(this.dgvDecisionResult);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(40, 156);
            this.panel6.Margin = new System.Windows.Forms.Padding(2);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1179, 365);
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
            this.Row,
            this.Column,
            this.NGArea,
            this.dgvLocationXY,
            this.SearchExclude,
            this.Check,
            this.NGReason,
            this.SearchTime,
            this.SearchUser,
            this.CheckTime,
            this.CheckUser,
            this.UpdateTime,
            this.UpdateUser,
            this.UpdateNGReason});
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
            this.dgvDecisionResult.ReadOnly = true;
            this.dgvDecisionResult.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
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
            this.dgvDecisionResult.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvDecisionResult.RowTemplate.Height = 75;
            this.dgvDecisionResult.RowTemplate.ReadOnly = true;
            this.dgvDecisionResult.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDecisionResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDecisionResult.Size = new System.Drawing.Size(1179, 365);
            this.dgvDecisionResult.TabIndex = 0;
            this.dgvDecisionResult.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDecisionResult_CellClick);
            this.dgvDecisionResult.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgvDecisionResult_MouseDown);
            this.dgvDecisionResult.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgvDecisionResult_MouseMove);
            // 
            // No
            // 
            this.No.HeaderText = "№";
            this.No.MinimumWidth = 6;
            this.No.Name = "No";
            this.No.ReadOnly = true;
            this.No.Width = 50;
            // 
            // Row
            // 
            this.Row.HeaderText = "行";
            this.Row.MinimumWidth = 6;
            this.Row.Name = "Row";
            this.Row.ReadOnly = true;
            this.Row.Width = 68;
            // 
            // Column
            // 
            this.Column.HeaderText = "列";
            this.Column.MinimumWidth = 6;
            this.Column.Name = "Column";
            this.Column.ReadOnly = true;
            this.Column.Width = 47;
            // 
            // NGArea
            // 
            this.NGArea.HeaderText = "NG面";
            this.NGArea.MinimumWidth = 6;
            this.NGArea.Name = "NGArea";
            this.NGArea.ReadOnly = true;
            this.NGArea.Width = 66;
            // 
            // dgvLocationXY
            // 
            this.dgvLocationXY.HeaderText = "位置(X,Y)cm";
            this.dgvLocationXY.MinimumWidth = 6;
            this.dgvLocationXY.Name = "dgvLocationXY";
            this.dgvLocationXY.ReadOnly = true;
            this.dgvLocationXY.Width = 113;
            // 
            // SearchExclude
            // 
            this.SearchExclude.HeaderText = "過検知除外";
            this.SearchExclude.Name = "SearchExclude";
            this.SearchExclude.ReadOnly = true;
            this.SearchExclude.Width = 143;
            // 
            // Check
            // 
            this.Check.HeaderText = "合否確認";
            this.Check.Name = "Check";
            this.Check.ReadOnly = true;
            this.Check.Width = 137;
            // 
            // NGReason
            // 
            this.NGReason.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.NGReason.HeaderText = "NG理由";
            this.NGReason.MinimumWidth = 240;
            this.NGReason.Name = "NGReason";
            this.NGReason.ReadOnly = true;
            this.NGReason.Width = 240;
            // 
            // SearchTime
            // 
            this.SearchTime.HeaderText = "過検知検査時刻";
            this.SearchTime.Name = "SearchTime";
            this.SearchTime.ReadOnly = true;
            this.SearchTime.Width = 240;
            // 
            // SearchUser
            // 
            this.SearchUser.HeaderText = "過検知検査作業者";
            this.SearchUser.Name = "SearchUser";
            this.SearchUser.ReadOnly = true;
            this.SearchUser.Width = 190;
            // 
            // CheckTime
            // 
            this.CheckTime.HeaderText = "合否確認時刻";
            this.CheckTime.Name = "CheckTime";
            this.CheckTime.ReadOnly = true;
            this.CheckTime.Width = 240;
            // 
            // CheckUser
            // 
            this.CheckUser.HeaderText = "合否確認作業者";
            this.CheckUser.Name = "CheckUser";
            this.CheckUser.ReadOnly = true;
            this.CheckUser.Width = 190;
            // 
            // UpdateTime
            // 
            this.UpdateTime.HeaderText = "結果更新時刻";
            this.UpdateTime.Name = "UpdateTime";
            this.UpdateTime.ReadOnly = true;
            this.UpdateTime.Width = 240;
            // 
            // UpdateUser
            // 
            this.UpdateUser.HeaderText = "結果更新作業者";
            this.UpdateUser.Name = "UpdateUser";
            this.UpdateUser.ReadOnly = true;
            this.UpdateUser.Width = 190;
            // 
            // UpdateNGReason
            // 
            this.UpdateNGReason.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.UpdateNGReason.HeaderText = "更新前NG理由";
            this.UpdateNGReason.MinimumWidth = 240;
            this.UpdateNGReason.Name = "UpdateNGReason";
            this.UpdateNGReason.ReadOnly = true;
            this.UpdateNGReason.Width = 240;
            // 
            // DisplayResults
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1276, 599);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DisplayResults";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "検査結果確認";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DisplayResults_FormClosing);
            this.Load += new System.EventHandler(this.Result_Load);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.grbWhere.ResumeLayout(false);
            this.grbWhere.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDecisionResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
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
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnTargetSelection;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.TextBox txtLine;
        private System.Windows.Forms.TextBox txtNgReason;
        private System.Windows.Forms.TextBox txtWorkerName;
        private System.Windows.Forms.Label lblNgReason;
        private System.Windows.Forms.Label lblNgSide;
        private System.Windows.Forms.Label lblCol;
        private System.Windows.Forms.Label lblRow;
        private System.Windows.Forms.Label lblTitleUserNm;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.GroupBox grbWhere;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblInspectionNum;
        private System.Windows.Forms.Button btnInspectionUpdate;
        private System.Windows.Forms.Label lblTitleSearchCnt;
        private System.Windows.Forms.Label lblImageSearchCount;
        private System.Windows.Forms.Label lblImageCount;
        private System.Windows.Forms.Label lblCushionCount;
        private System.Windows.Forms.Label lblTitleSearchCushionCnt;
        private System.Windows.Forms.Label lblTitleSearchImageCnt;
        private System.Windows.Forms.Label lblCushionSearchCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn No;
        private System.Windows.Forms.DataGridViewTextBoxColumn Row;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column;
        private System.Windows.Forms.DataGridViewTextBoxColumn NGArea;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvLocationXY;
        private System.Windows.Forms.DataGridViewTextBoxColumn SearchExclude;
        private System.Windows.Forms.DataGridViewTextBoxColumn Check;
        private System.Windows.Forms.DataGridViewTextBoxColumn NGReason;
        private System.Windows.Forms.DataGridViewTextBoxColumn SearchTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn SearchUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn CheckTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn CheckUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn UpdateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn UpdateUser;
        private System.Windows.Forms.DataGridViewTextBoxColumn UpdateNGReason;
        private MyDataGridView dgvDecisionResult;
        private System.Windows.Forms.ComboBox cmbColumns;
        private System.Windows.Forms.ComboBox cmbNgFace;
        private System.Windows.Forms.Button btnReprint;
    }
}