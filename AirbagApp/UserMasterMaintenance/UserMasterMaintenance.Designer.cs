namespace UserMasterMaintenance
{
    partial class UserMasterMaintenance
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserMasterMaintenance));
            this.dgvUser = new System.Windows.Forms.DataGridView();
            this.btnReg = new System.Windows.Forms.Button();
            this.btnDel = new System.Windows.Forms.Button();
            this.btnImportCsv = new System.Windows.Forms.Button();
            this.gbxJoken = new System.Windows.Forms.GroupBox();
            this.txtUserNo_To = new System.Windows.Forms.TextBox();
            this.txtUserNo_From = new System.Windows.Forms.TextBox();
            this.llkワ = new System.Windows.Forms.LinkLabel();
            this.llkラ = new System.Windows.Forms.LinkLabel();
            this.llkヤ = new System.Windows.Forms.LinkLabel();
            this.llkマ = new System.Windows.Forms.LinkLabel();
            this.llkハ = new System.Windows.Forms.LinkLabel();
            this.llkナ = new System.Windows.Forms.LinkLabel();
            this.llkタ = new System.Windows.Forms.LinkLabel();
            this.llkサ = new System.Windows.Forms.LinkLabel();
            this.llkカ = new System.Windows.Forms.LinkLabel();
            this.llkNon = new System.Windows.Forms.LinkLabel();
            this.llkEtc = new System.Windows.Forms.LinkLabel();
            this.llkア = new System.Windows.Forms.LinkLabel();
            this.lblUserNo_Between = new System.Windows.Forms.Label();
            this.lblTitleUserNo = new System.Windows.Forms.Label();
            this.lblTitleUserYomiGana = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.UserNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UserNm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.YomiGana = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUser)).BeginInit();
            this.gbxJoken.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvUser
            // 
            this.dgvUser.AllowUserToAddRows = false;
            this.dgvUser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvUser.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvUser.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUser.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.UserNo,
            this.UserNm,
            this.YomiGana});
            this.dgvUser.Location = new System.Drawing.Point(12, 173);
            this.dgvUser.MultiSelect = false;
            this.dgvUser.Name = "dgvUser";
            this.dgvUser.ReadOnly = true;
            this.dgvUser.RowHeadersVisible = false;
            this.dgvUser.RowTemplate.Height = 21;
            this.dgvUser.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUser.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvUser.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUser.Size = new System.Drawing.Size(452, 274);
            this.dgvUser.TabIndex = 2;
            this.dgvUser.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUser_CellDoubleClick);
            // 
            // btnReg
            // 
            this.btnReg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReg.Location = new System.Drawing.Point(308, 453);
            this.btnReg.Name = "btnReg";
            this.btnReg.Size = new System.Drawing.Size(75, 23);
            this.btnReg.TabIndex = 4;
            this.btnReg.Text = "登録";
            this.btnReg.UseVisualStyleBackColor = true;
            this.btnReg.Click += new System.EventHandler(this.btnReg_Click);
            // 
            // btnDel
            // 
            this.btnDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDel.Location = new System.Drawing.Point(389, 453);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(75, 23);
            this.btnDel.TabIndex = 5;
            this.btnDel.Text = "削除";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
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
            this.gbxJoken.Controls.Add(this.txtUserNo_To);
            this.gbxJoken.Controls.Add(this.txtUserNo_From);
            this.gbxJoken.Controls.Add(this.llkワ);
            this.gbxJoken.Controls.Add(this.llkラ);
            this.gbxJoken.Controls.Add(this.llkヤ);
            this.gbxJoken.Controls.Add(this.llkマ);
            this.gbxJoken.Controls.Add(this.llkハ);
            this.gbxJoken.Controls.Add(this.llkナ);
            this.gbxJoken.Controls.Add(this.llkタ);
            this.gbxJoken.Controls.Add(this.llkサ);
            this.gbxJoken.Controls.Add(this.llkカ);
            this.gbxJoken.Controls.Add(this.llkNon);
            this.gbxJoken.Controls.Add(this.llkEtc);
            this.gbxJoken.Controls.Add(this.llkア);
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
            // txtUserNo_To
            // 
            this.txtUserNo_To.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtUserNo_To.Location = new System.Drawing.Point(147, 97);
            this.txtUserNo_To.MaxLength = 4;
            this.txtUserNo_To.Name = "txtUserNo_To";
            this.txtUserNo_To.ShortcutsEnabled = false;
            this.txtUserNo_To.Size = new System.Drawing.Size(36, 19);
            this.txtUserNo_To.TabIndex = 16;
            this.txtUserNo_To.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUserNo_KeyPress);
            this.txtUserNo_To.Leave += new System.EventHandler(this.txtUserNo_To_Leave);
            // 
            // txtUserNo_From
            // 
            this.txtUserNo_From.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtUserNo_From.Location = new System.Drawing.Point(82, 97);
            this.txtUserNo_From.MaxLength = 4;
            this.txtUserNo_From.Name = "txtUserNo_From";
            this.txtUserNo_From.ShortcutsEnabled = false;
            this.txtUserNo_From.Size = new System.Drawing.Size(36, 19);
            this.txtUserNo_From.TabIndex = 14;
            this.txtUserNo_From.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUserNo_KeyPress);
            this.txtUserNo_From.Leave += new System.EventHandler(this.txtUserNo_From_Leave);
            // 
            // llkワ
            // 
            this.llkワ.AutoSize = true;
            this.llkワ.Location = new System.Drawing.Point(231, 69);
            this.llkワ.Name = "llkワ";
            this.llkワ.Size = new System.Drawing.Size(14, 12);
            this.llkワ.TabIndex = 10;
            this.llkワ.TabStop = true;
            this.llkワ.Text = "ワ";
            this.llkワ.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkラ
            // 
            this.llkラ.AutoSize = true;
            this.llkラ.Location = new System.Drawing.Point(181, 69);
            this.llkラ.Name = "llkラ";
            this.llkラ.Size = new System.Drawing.Size(13, 12);
            this.llkラ.TabIndex = 9;
            this.llkラ.TabStop = true;
            this.llkラ.Text = "ラ";
            this.llkラ.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkヤ
            // 
            this.llkヤ.AutoSize = true;
            this.llkヤ.Location = new System.Drawing.Point(131, 69);
            this.llkヤ.Name = "llkヤ";
            this.llkヤ.Size = new System.Drawing.Size(15, 12);
            this.llkヤ.TabIndex = 8;
            this.llkヤ.TabStop = true;
            this.llkヤ.Text = "ヤ";
            this.llkヤ.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkマ
            // 
            this.llkマ.AutoSize = true;
            this.llkマ.Location = new System.Drawing.Point(81, 69);
            this.llkマ.Name = "llkマ";
            this.llkマ.Size = new System.Drawing.Size(14, 12);
            this.llkマ.TabIndex = 7;
            this.llkマ.TabStop = true;
            this.llkマ.Text = "マ";
            this.llkマ.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkハ
            // 
            this.llkハ.AutoSize = true;
            this.llkハ.Location = new System.Drawing.Point(31, 69);
            this.llkハ.Name = "llkハ";
            this.llkハ.Size = new System.Drawing.Size(15, 12);
            this.llkハ.TabIndex = 6;
            this.llkハ.TabStop = true;
            this.llkハ.Text = "ハ";
            this.llkハ.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkナ
            // 
            this.llkナ.AutoSize = true;
            this.llkナ.Location = new System.Drawing.Point(231, 43);
            this.llkナ.Name = "llkナ";
            this.llkナ.Size = new System.Drawing.Size(15, 12);
            this.llkナ.TabIndex = 5;
            this.llkナ.TabStop = true;
            this.llkナ.Text = "ナ";
            this.llkナ.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkタ
            // 
            this.llkタ.AutoSize = true;
            this.llkタ.Location = new System.Drawing.Point(181, 43);
            this.llkタ.Name = "llkタ";
            this.llkタ.Size = new System.Drawing.Size(13, 12);
            this.llkタ.TabIndex = 4;
            this.llkタ.TabStop = true;
            this.llkタ.Text = "タ";
            this.llkタ.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkサ
            // 
            this.llkサ.AutoSize = true;
            this.llkサ.Location = new System.Drawing.Point(131, 43);
            this.llkサ.Name = "llkサ";
            this.llkサ.Size = new System.Drawing.Size(15, 12);
            this.llkサ.TabIndex = 3;
            this.llkサ.TabStop = true;
            this.llkサ.Text = "サ";
            this.llkサ.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkカ
            // 
            this.llkカ.AutoSize = true;
            this.llkカ.Location = new System.Drawing.Point(81, 43);
            this.llkカ.Name = "llkカ";
            this.llkカ.Size = new System.Drawing.Size(14, 12);
            this.llkカ.TabIndex = 2;
            this.llkカ.TabStop = true;
            this.llkカ.Text = "カ";
            this.llkカ.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkNon
            // 
            this.llkNon.AutoSize = true;
            this.llkNon.Location = new System.Drawing.Point(362, 57);
            this.llkNon.Name = "llkNon";
            this.llkNon.Size = new System.Drawing.Size(48, 12);
            this.llkNon.TabIndex = 12;
            this.llkNon.TabStop = true;
            this.llkNon.Text = "条件なし";
            this.llkNon.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkEtc
            // 
            this.llkEtc.AutoSize = true;
            this.llkEtc.Location = new System.Drawing.Point(294, 57);
            this.llkEtc.Name = "llkEtc";
            this.llkEtc.Size = new System.Drawing.Size(36, 12);
            this.llkEtc.TabIndex = 11;
            this.llkEtc.TabStop = true;
            this.llkEtc.Text = "その他";
            this.llkEtc.Click += new System.EventHandler(this.llk_Click);
            // 
            // llkア
            // 
            this.llkア.AutoSize = true;
            this.llkア.Location = new System.Drawing.Point(31, 43);
            this.llkア.Name = "llkア";
            this.llkア.Size = new System.Drawing.Size(14, 12);
            this.llkア.TabIndex = 1;
            this.llkア.TabStop = true;
            this.llkア.Text = "ア";
            this.llkア.Click += new System.EventHandler(this.llk_Click);
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
            // UserNo
            // 
            this.UserNo.HeaderText = "社員番号";
            this.UserNo.Name = "UserNo";
            this.UserNo.ReadOnly = true;
            this.UserNo.Width = 78;
            // 
            // UserNm
            // 
            this.UserNm.FillWeight = 120F;
            this.UserNm.HeaderText = "作業者名";
            this.UserNm.MinimumWidth = 120;
            this.UserNm.Name = "UserNm";
            this.UserNm.ReadOnly = true;
            this.UserNm.Width = 120;
            // 
            // YomiGana
            // 
            this.YomiGana.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.YomiGana.FillWeight = 180F;
            this.YomiGana.HeaderText = "読みカナ";
            this.YomiGana.MinimumWidth = 180;
            this.YomiGana.Name = "YomiGana";
            this.YomiGana.ReadOnly = true;
            // 
            // UserMasterMaintenance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 485);
            this.Controls.Add(this.gbxJoken);
            this.Controls.Add(this.btnDel);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.btnImportCsv);
            this.Controls.Add(this.btnReg);
            this.Controls.Add(this.dgvUser);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UserMasterMaintenance";
            this.Text = "作業者マスタメンテナンス";
            ((System.ComponentModel.ISupportInitialize)(this.dgvUser)).EndInit();
            this.gbxJoken.ResumeLayout(false);
            this.gbxJoken.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvUser;
        private System.Windows.Forms.Button btnReg;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.Button btnImportCsv;
        private System.Windows.Forms.GroupBox gbxJoken;
        private System.Windows.Forms.TextBox txtUserNo_To;
        private System.Windows.Forms.TextBox txtUserNo_From;
        private System.Windows.Forms.LinkLabel llkワ;
        private System.Windows.Forms.LinkLabel llkラ;
        private System.Windows.Forms.LinkLabel llkヤ;
        private System.Windows.Forms.LinkLabel llkマ;
        private System.Windows.Forms.LinkLabel llkハ;
        private System.Windows.Forms.LinkLabel llkナ;
        private System.Windows.Forms.LinkLabel llkタ;
        private System.Windows.Forms.LinkLabel llkサ;
        private System.Windows.Forms.LinkLabel llkカ;
        private System.Windows.Forms.LinkLabel llkNon;
        private System.Windows.Forms.LinkLabel llkEtc;
        private System.Windows.Forms.LinkLabel llkア;
        private System.Windows.Forms.Label lblUserNo_Between;
        private System.Windows.Forms.Label lblTitleUserNo;
        private System.Windows.Forms.Label lblTitleUserYomiGana;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserNm;
        private System.Windows.Forms.DataGridViewTextBoxColumn YomiGana;
    }
}

