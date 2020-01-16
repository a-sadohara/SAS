namespace WokerMstManagement
{
    partial class WokerMstManagement
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WokerMstManagement));
            this.dgvWorker = new System.Windows.Forms.DataGridView();
            this.EmployeeNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WorkerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WorkerNameKana = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRegistration = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnImportCsv = new System.Windows.Forms.Button();
            this.gbxJoken = new System.Windows.Forms.GroupBox();
            this.txtEmployeeNumTo = new System.Windows.Forms.TextBox();
            this.txtEmployeeNumFrom = new System.Windows.Forms.TextBox();
            this.lblWorkerNameKanaワ = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaラ = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaヤ = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaマ = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaハ = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaナ = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaタ = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaサ = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaカ = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaNonCondition = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaEtc = new System.Windows.Forms.LinkLabel();
            this.lblWorkerNameKanaア = new System.Windows.Forms.LinkLabel();
            this.lblUserNo_Between = new System.Windows.Forms.Label();
            this.lblTitleUserNo = new System.Windows.Forms.Label();
            this.lblTitleUserYomiGana = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorker)).BeginInit();
            this.gbxJoken.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvWorker
            // 
            this.dgvWorker.AllowUserToAddRows = false;
            this.dgvWorker.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvWorker.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvWorker.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWorker.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.EmployeeNum,
            this.WorkerName,
            this.WorkerNameKana});
            this.dgvWorker.Location = new System.Drawing.Point(12, 173);
            this.dgvWorker.MultiSelect = false;
            this.dgvWorker.Name = "dgvWorker";
            this.dgvWorker.ReadOnly = true;
            this.dgvWorker.RowHeadersVisible = false;
            this.dgvWorker.RowTemplate.Height = 21;
            this.dgvWorker.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvWorker.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvWorker.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvWorker.Size = new System.Drawing.Size(452, 274);
            this.dgvWorker.TabIndex = 2;
            this.dgvWorker.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvWorker_CellDoubleClick);
            // 
            // EmployeeNum
            // 
            this.EmployeeNum.HeaderText = "社員番号";
            this.EmployeeNum.Name = "EmployeeNum";
            this.EmployeeNum.ReadOnly = true;
            this.EmployeeNum.Width = 78;
            // 
            // WorkerName
            // 
            this.WorkerName.FillWeight = 120F;
            this.WorkerName.HeaderText = "作業者名";
            this.WorkerName.MinimumWidth = 120;
            this.WorkerName.Name = "WorkerName";
            this.WorkerName.ReadOnly = true;
            this.WorkerName.Width = 120;
            // 
            // WorkerNameKana
            // 
            this.WorkerNameKana.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.WorkerNameKana.FillWeight = 180F;
            this.WorkerNameKana.HeaderText = "読みカナ";
            this.WorkerNameKana.MinimumWidth = 180;
            this.WorkerNameKana.Name = "WorkerNameKana";
            this.WorkerNameKana.ReadOnly = true;
            // 
            // btnRegistration
            // 
            this.btnRegistration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRegistration.Location = new System.Drawing.Point(308, 453);
            this.btnRegistration.Name = "btnRegistration";
            this.btnRegistration.Size = new System.Drawing.Size(75, 23);
            this.btnRegistration.TabIndex = 4;
            this.btnRegistration.Text = "登録";
            this.btnRegistration.UseVisualStyleBackColor = true;
            this.btnRegistration.Click += new System.EventHandler(this.btnRegistration_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(389, 453);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 5;
            this.btnDelete.Text = "削除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnImportCsv
            // 
            this.btnImportCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnImportCsv.Location = new System.Drawing.Point(12, 453);
            this.btnImportCsv.Name = "btnImportCsv";
            this.btnImportCsv.Size = new System.Drawing.Size(75, 23);
            this.btnImportCsv.TabIndex = 3;
            this.btnImportCsv.Text = "CSV取込";
            this.btnImportCsv.UseVisualStyleBackColor = true;
            this.btnImportCsv.Click += new System.EventHandler(this.btnImportCsv_Click);
            // 
            // gbxJoken
            // 
            this.gbxJoken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxJoken.Controls.Add(this.txtEmployeeNumTo);
            this.gbxJoken.Controls.Add(this.txtEmployeeNumFrom);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaワ);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaラ);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaヤ);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaマ);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaハ);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaナ);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaタ);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaサ);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaカ);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaNonCondition);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaEtc);
            this.gbxJoken.Controls.Add(this.lblWorkerNameKanaア);
            this.gbxJoken.Controls.Add(this.lblUserNo_Between);
            this.gbxJoken.Controls.Add(this.lblTitleUserNo);
            this.gbxJoken.Controls.Add(this.lblTitleUserYomiGana);
            this.gbxJoken.Location = new System.Drawing.Point(12, 12);
            this.gbxJoken.Name = "gbxJoken";
            this.gbxJoken.Size = new System.Drawing.Size(452, 126);
            this.gbxJoken.TabIndex = 0;
            this.gbxJoken.TabStop = false;
            this.gbxJoken.Text = "検索条件";
            // 
            // txtEmployeeNumTo
            // 
            this.txtEmployeeNumTo.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.txtEmployeeNumTo.Location = new System.Drawing.Point(147, 97);
            this.txtEmployeeNumTo.MaxLength = 4;
            this.txtEmployeeNumTo.Name = "txtEmployeeNumTo";
            this.txtEmployeeNumTo.ShortcutsEnabled = false;
            this.txtEmployeeNumTo.Size = new System.Drawing.Size(36, 19);
            this.txtEmployeeNumTo.TabIndex = 16;
            this.txtEmployeeNumTo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtEmployeeNum_KeyPress);
            this.txtEmployeeNumTo.Leave += new System.EventHandler(this.txtEmployeeNum_Leave);
            // 
            // txtEmployeeNumFrom
            // 
            this.txtEmployeeNumFrom.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.txtEmployeeNumFrom.Location = new System.Drawing.Point(82, 97);
            this.txtEmployeeNumFrom.MaxLength = 4;
            this.txtEmployeeNumFrom.Name = "txtEmployeeNumFrom";
            this.txtEmployeeNumFrom.ShortcutsEnabled = false;
            this.txtEmployeeNumFrom.Size = new System.Drawing.Size(36, 19);
            this.txtEmployeeNumFrom.TabIndex = 14;
            this.txtEmployeeNumFrom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtEmployeeNum_KeyPress);
            this.txtEmployeeNumFrom.Leave += new System.EventHandler(this.txtEmployeeNum_Leave);
            // 
            // lblWorkerNameKanaワ
            // 
            this.lblWorkerNameKanaワ.AutoSize = true;
            this.lblWorkerNameKanaワ.Location = new System.Drawing.Point(231, 69);
            this.lblWorkerNameKanaワ.Name = "lblWorkerNameKanaワ";
            this.lblWorkerNameKanaワ.Size = new System.Drawing.Size(14, 12);
            this.lblWorkerNameKanaワ.TabIndex = 10;
            this.lblWorkerNameKanaワ.TabStop = true;
            this.lblWorkerNameKanaワ.Text = "ワ";
            this.lblWorkerNameKanaワ.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaラ
            // 
            this.lblWorkerNameKanaラ.AutoSize = true;
            this.lblWorkerNameKanaラ.Location = new System.Drawing.Point(181, 69);
            this.lblWorkerNameKanaラ.Name = "lblWorkerNameKanaラ";
            this.lblWorkerNameKanaラ.Size = new System.Drawing.Size(13, 12);
            this.lblWorkerNameKanaラ.TabIndex = 9;
            this.lblWorkerNameKanaラ.TabStop = true;
            this.lblWorkerNameKanaラ.Text = "ラ";
            this.lblWorkerNameKanaラ.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaヤ
            // 
            this.lblWorkerNameKanaヤ.AutoSize = true;
            this.lblWorkerNameKanaヤ.Location = new System.Drawing.Point(131, 69);
            this.lblWorkerNameKanaヤ.Name = "lblWorkerNameKanaヤ";
            this.lblWorkerNameKanaヤ.Size = new System.Drawing.Size(15, 12);
            this.lblWorkerNameKanaヤ.TabIndex = 8;
            this.lblWorkerNameKanaヤ.TabStop = true;
            this.lblWorkerNameKanaヤ.Text = "ヤ";
            this.lblWorkerNameKanaヤ.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaマ
            // 
            this.lblWorkerNameKanaマ.AutoSize = true;
            this.lblWorkerNameKanaマ.Location = new System.Drawing.Point(81, 69);
            this.lblWorkerNameKanaマ.Name = "lblWorkerNameKanaマ";
            this.lblWorkerNameKanaマ.Size = new System.Drawing.Size(14, 12);
            this.lblWorkerNameKanaマ.TabIndex = 7;
            this.lblWorkerNameKanaマ.TabStop = true;
            this.lblWorkerNameKanaマ.Text = "マ";
            this.lblWorkerNameKanaマ.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaハ
            // 
            this.lblWorkerNameKanaハ.AutoSize = true;
            this.lblWorkerNameKanaハ.Location = new System.Drawing.Point(31, 69);
            this.lblWorkerNameKanaハ.Name = "lblWorkerNameKanaハ";
            this.lblWorkerNameKanaハ.Size = new System.Drawing.Size(15, 12);
            this.lblWorkerNameKanaハ.TabIndex = 6;
            this.lblWorkerNameKanaハ.TabStop = true;
            this.lblWorkerNameKanaハ.Text = "ハ";
            this.lblWorkerNameKanaハ.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaナ
            // 
            this.lblWorkerNameKanaナ.AutoSize = true;
            this.lblWorkerNameKanaナ.Location = new System.Drawing.Point(231, 43);
            this.lblWorkerNameKanaナ.Name = "lblWorkerNameKanaナ";
            this.lblWorkerNameKanaナ.Size = new System.Drawing.Size(15, 12);
            this.lblWorkerNameKanaナ.TabIndex = 5;
            this.lblWorkerNameKanaナ.TabStop = true;
            this.lblWorkerNameKanaナ.Text = "ナ";
            this.lblWorkerNameKanaナ.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaタ
            // 
            this.lblWorkerNameKanaタ.AutoSize = true;
            this.lblWorkerNameKanaタ.Location = new System.Drawing.Point(181, 43);
            this.lblWorkerNameKanaタ.Name = "lblWorkerNameKanaタ";
            this.lblWorkerNameKanaタ.Size = new System.Drawing.Size(13, 12);
            this.lblWorkerNameKanaタ.TabIndex = 4;
            this.lblWorkerNameKanaタ.TabStop = true;
            this.lblWorkerNameKanaタ.Text = "タ";
            this.lblWorkerNameKanaタ.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaサ
            // 
            this.lblWorkerNameKanaサ.AutoSize = true;
            this.lblWorkerNameKanaサ.Location = new System.Drawing.Point(131, 43);
            this.lblWorkerNameKanaサ.Name = "lblWorkerNameKanaサ";
            this.lblWorkerNameKanaサ.Size = new System.Drawing.Size(15, 12);
            this.lblWorkerNameKanaサ.TabIndex = 3;
            this.lblWorkerNameKanaサ.TabStop = true;
            this.lblWorkerNameKanaサ.Text = "サ";
            this.lblWorkerNameKanaサ.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaカ
            // 
            this.lblWorkerNameKanaカ.AutoSize = true;
            this.lblWorkerNameKanaカ.Location = new System.Drawing.Point(81, 43);
            this.lblWorkerNameKanaカ.Name = "lblWorkerNameKanaカ";
            this.lblWorkerNameKanaカ.Size = new System.Drawing.Size(14, 12);
            this.lblWorkerNameKanaカ.TabIndex = 2;
            this.lblWorkerNameKanaカ.TabStop = true;
            this.lblWorkerNameKanaカ.Text = "カ";
            this.lblWorkerNameKanaカ.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaNonCondition
            // 
            this.lblWorkerNameKanaNonCondition.AutoSize = true;
            this.lblWorkerNameKanaNonCondition.Location = new System.Drawing.Point(362, 57);
            this.lblWorkerNameKanaNonCondition.Name = "lblWorkerNameKanaNonCondition";
            this.lblWorkerNameKanaNonCondition.Size = new System.Drawing.Size(48, 12);
            this.lblWorkerNameKanaNonCondition.TabIndex = 12;
            this.lblWorkerNameKanaNonCondition.TabStop = true;
            this.lblWorkerNameKanaNonCondition.Text = "条件なし";
            this.lblWorkerNameKanaNonCondition.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaEtc
            // 
            this.lblWorkerNameKanaEtc.AutoSize = true;
            this.lblWorkerNameKanaEtc.Location = new System.Drawing.Point(294, 57);
            this.lblWorkerNameKanaEtc.Name = "lblWorkerNameKanaEtc";
            this.lblWorkerNameKanaEtc.Size = new System.Drawing.Size(36, 12);
            this.lblWorkerNameKanaEtc.TabIndex = 11;
            this.lblWorkerNameKanaEtc.TabStop = true;
            this.lblWorkerNameKanaEtc.Text = "その他";
            this.lblWorkerNameKanaEtc.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblWorkerNameKanaア
            // 
            this.lblWorkerNameKanaア.AutoSize = true;
            this.lblWorkerNameKanaア.Location = new System.Drawing.Point(31, 43);
            this.lblWorkerNameKanaア.Name = "lblWorkerNameKanaア";
            this.lblWorkerNameKanaア.Size = new System.Drawing.Size(14, 12);
            this.lblWorkerNameKanaア.TabIndex = 1;
            this.lblWorkerNameKanaア.TabStop = true;
            this.lblWorkerNameKanaア.Text = "ア";
            this.lblWorkerNameKanaア.Click += new System.EventHandler(this.llk_Click);
            // 
            // lblUserNo_Between
            // 
            this.lblUserNo_Between.AutoSize = true;
            this.lblUserNo_Between.Location = new System.Drawing.Point(124, 100);
            this.lblUserNo_Between.Name = "lblUserNo_Between";
            this.lblUserNo_Between.Size = new System.Drawing.Size(17, 12);
            this.lblUserNo_Between.TabIndex = 15;
            this.lblUserNo_Between.Text = "～";
            // 
            // lblTitleUserNo
            // 
            this.lblTitleUserNo.AutoSize = true;
            this.lblTitleUserNo.Location = new System.Drawing.Point(17, 100);
            this.lblTitleUserNo.Name = "lblTitleUserNo";
            this.lblTitleUserNo.Size = new System.Drawing.Size(59, 12);
            this.lblTitleUserNo.TabIndex = 13;
            this.lblTitleUserNo.Text = "社員番号：";
            // 
            // lblTitleUserYomiGana
            // 
            this.lblTitleUserYomiGana.AutoSize = true;
            this.lblTitleUserYomiGana.Location = new System.Drawing.Point(17, 21);
            this.lblTitleUserYomiGana.Name = "lblTitleUserYomiGana";
            this.lblTitleUserYomiGana.Size = new System.Drawing.Size(113, 12);
            this.lblTitleUserYomiGana.TabIndex = 0;
            this.lblTitleUserYomiGana.Text = "作業者名（読みカナ）：";
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(389, 144);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = "検索";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // WokerMstManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 485);
            this.Controls.Add(this.gbxJoken);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.btnImportCsv);
            this.Controls.Add(this.btnRegistration);
            this.Controls.Add(this.dgvWorker);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WokerMstManagement";
            this.Text = "作業者マスタメンテナンス";
            this.Load += new System.EventHandler(this.WokerMstManagement_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvWorker)).EndInit();
            this.gbxJoken.ResumeLayout(false);
            this.gbxJoken.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvWorker;
        private System.Windows.Forms.Button btnRegistration;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnImportCsv;
        private System.Windows.Forms.GroupBox gbxJoken;
        private System.Windows.Forms.TextBox txtEmployeeNumTo;
        private System.Windows.Forms.TextBox txtEmployeeNumFrom;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaワ;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaラ;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaヤ;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaマ;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaハ;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaナ;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaタ;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaサ;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaカ;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaNonCondition;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaEtc;
        private System.Windows.Forms.LinkLabel lblWorkerNameKanaア;
        private System.Windows.Forms.Label lblUserNo_Between;
        private System.Windows.Forms.Label lblTitleUserNo;
        private System.Windows.Forms.Label lblTitleUserYomiGana;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn EmployeeNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn WorkerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn WorkerNameKana;
    }
}

