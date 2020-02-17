using System;
using System.Reflection;
using System.Windows.Forms;

namespace ImageChecker
{
    //class DBDataGridView : DataGridView
    //{
    //    public DBDataGridView() { DoubleBuffered = true; }
    //}

    //public static class ExtensionMethods
    //{
    //    public static void DoubleBuffered(this DataGridView dgv, bool setting)
    //    {
    //        Type dgvType = dgv.GetType();
    //        PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
    //        pi.SetValue(dgv, setting, null);
    //    }
    //}

    partial class TargetSelection
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TargetSelection));
            this.lblWorkerName = new System.Windows.Forms.Label();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            //this.dgvTargetSelection = new System.Windows.Forms.DataGridView();
            this.dgvTargetSelection = new MyDataGridView();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CheckInfo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TargetInfo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.No = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnReviseLine = new System.Windows.Forms.Button();
            this.btnExceptTarget = new System.Windows.Forms.Button();
            this.btnCheckInspectionHistory = new System.Windows.Forms.Button();
            this.pnlTop.SuspendLayout();
            this.tlpMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTargetSelection)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblWorkerName
            // 
            this.lblWorkerName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWorkerName.AutoEllipsis = true;
            this.lblWorkerName.AutoSize = true;
            this.lblWorkerName.Font = new System.Drawing.Font("メイリオ", 8F);
            this.lblWorkerName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblWorkerName.Location = new System.Drawing.Point(539, 19);
            this.lblWorkerName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lblWorkerName.MaximumSize = new System.Drawing.Size(300, 0);
            this.lblWorkerName.MinimumSize = new System.Drawing.Size(300, 0);
            this.lblWorkerName.Name = "lblWorkerName";
            this.lblWorkerName.Size = new System.Drawing.Size(300, 17);
            this.lblWorkerName.TabIndex = 1;
            this.lblWorkerName.Text = "作業者名：XXXXXXXX";
            this.lblWorkerName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.btnLogout);
            this.pnlTop.Controls.Add(this.lblWorkerName);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(963, 53);
            this.pnlTop.TabIndex = 2;
            // 
            // btnLogout
            // 
            this.btnLogout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogout.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnLogout.Location = new System.Drawing.Point(844, 9);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(107, 36);
            this.btnLogout.TabIndex = 2;
            this.btnLogout.Text = "ログアウト";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.pnlBottom, 0, 1);
            this.tlpMain.Controls.Add(this.dgvTargetSelection, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 53);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tlpMain.Size = new System.Drawing.Size(963, 470);
            this.tlpMain.TabIndex = 5;
            // 
            // dgvTargetSelection
            // 
            this.dgvTargetSelection.AllowUserToAddRows = false;
            this.dgvTargetSelection.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvTargetSelection.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTargetSelection.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvTargetSelection.ColumnHeadersHeight = 29;
            this.dgvTargetSelection.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.No,
            this.TargetInfo,
            this.CheckInfo,
            this.Status});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvTargetSelection.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvTargetSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTargetSelection.EnableHeadersVisualStyles = false;
            this.dgvTargetSelection.Location = new System.Drawing.Point(2, 2);
            this.dgvTargetSelection.Margin = new System.Windows.Forms.Padding(2);
            this.dgvTargetSelection.MultiSelect = false;
            this.dgvTargetSelection.Name = "dgvTargetSelection";
            this.dgvTargetSelection.ReadOnly = true;
            this.dgvTargetSelection.RowHeadersVisible = false;
            this.dgvTargetSelection.RowHeadersWidth = 51;
            this.dgvTargetSelection.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.dgvTargetSelection.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.dgvTargetSelection.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTargetSelection.RowTemplate.Height = 24;
            this.dgvTargetSelection.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvTargetSelection.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTargetSelection.Size = new System.Drawing.Size(959, 397);
            this.dgvTargetSelection.TabIndex = 0;
            this.dgvTargetSelection.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellContentClick);
            // 
            // Status
            // 
            this.Status.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("メイリオ", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Status.DefaultCellStyle = dataGridViewCellStyle4;
            this.Status.HeaderText = "欠点検査状態";
            this.Status.MinimumWidth = 6;
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // CheckInfo
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.CheckInfo.DefaultCellStyle = dataGridViewCellStyle3;
            this.CheckInfo.HeaderText = "外観検査情報";
            this.CheckInfo.MinimumWidth = 6;
            this.CheckInfo.Name = "CheckInfo";
            this.CheckInfo.ReadOnly = true;
            this.CheckInfo.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.CheckInfo.Width = 350;
            // 
            // TargetInfo
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.TargetInfo.DefaultCellStyle = dataGridViewCellStyle2;
            this.TargetInfo.HeaderText = "反物情報";
            this.TargetInfo.MinimumWidth = 6;
            this.TargetInfo.Name = "TargetInfo";
            this.TargetInfo.ReadOnly = true;
            this.TargetInfo.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.TargetInfo.Width = 200;
            // 
            // No
            // 
            this.No.HeaderText = "№";
            this.No.MinimumWidth = 6;
            this.No.Name = "No";
            this.No.ReadOnly = true;
            this.No.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.No.Width = 60;
            // 
            // pnlBottom
            // 
            this.pnlBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBottom.Controls.Add(this.btnCheckInspectionHistory);
            this.pnlBottom.Controls.Add(this.btnExceptTarget);
            this.pnlBottom.Controls.Add(this.btnReviseLine);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottom.Location = new System.Drawing.Point(0, 401);
            this.pnlBottom.Margin = new System.Windows.Forms.Padding(0);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(963, 69);
            this.pnlBottom.TabIndex = 4;
            // 
            // btnReviseLine
            // 
            this.btnReviseLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReviseLine.Font = new System.Drawing.Font("メイリオ", 9.75F);
            this.btnReviseLine.Location = new System.Drawing.Point(11, 7);
            this.btnReviseLine.Name = "btnReviseLine";
            this.btnReviseLine.Size = new System.Drawing.Size(156, 54);
            this.btnReviseLine.TabIndex = 3;
            this.btnReviseLine.Text = "行補正";
            this.btnReviseLine.UseVisualStyleBackColor = true;
            this.btnReviseLine.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // btnExceptTarget
            // 
            this.btnExceptTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExceptTarget.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnExceptTarget.Location = new System.Drawing.Point(181, 7);
            this.btnExceptTarget.Name = "btnExceptTarget";
            this.btnExceptTarget.Size = new System.Drawing.Size(156, 54);
            this.btnExceptTarget.TabIndex = 3;
            this.btnExceptTarget.Text = "検査対象外";
            this.btnExceptTarget.UseVisualStyleBackColor = true;
            this.btnExceptTarget.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnCheckInspectionHistory
            // 
            this.btnCheckInspectionHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckInspectionHistory.Font = new System.Drawing.Font("メイリオ", 9.75F);
            this.btnCheckInspectionHistory.Location = new System.Drawing.Point(794, 7);
            this.btnCheckInspectionHistory.Name = "btnCheckInspectionHistory";
            this.btnCheckInspectionHistory.Size = new System.Drawing.Size(156, 54);
            this.btnCheckInspectionHistory.TabIndex = 3;
            this.btnCheckInspectionHistory.Text = "検査履歴照会";
            this.btnCheckInspectionHistory.UseVisualStyleBackColor = true;
            this.btnCheckInspectionHistory.Click += new System.EventHandler(this.btnDisplayResultsAgo_Click);
            // 
            // TargetSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106)))));
            this.ClientSize = new System.Drawing.Size(963, 523);
            this.Controls.Add(this.tlpMain);
            this.Controls.Add(this.pnlTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "TargetSelection";
            this.Text = "検査対象選択";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Activated += new System.EventHandler(this.TargetSelection_Activated);
            this.Load += new System.EventHandler(this.TargetSelection_Load);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTargetSelection)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblWorkerName;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Button btnLogout;
        private TableLayoutPanel tlpMain;
        private Panel pnlBottom;
        private Button btnCheckInspectionHistory;
        private Button btnExceptTarget;
        private Button btnReviseLine;
        private DataGridView dgvTargetSelection;
        private DataGridViewTextBoxColumn No;
        private DataGridViewTextBoxColumn TargetInfo;
        private DataGridViewTextBoxColumn CheckInfo;
        private DataGridViewTextBoxColumn Status;
    }
}