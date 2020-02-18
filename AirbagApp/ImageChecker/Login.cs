using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class Login : Form
    {
        // 作業者情報選関連
        public string m_strEmployeeNum;
        public string m_strWorkerNm;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Login()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Login_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            // ユーザIDを初期選択
            txtUserId.Select();

            // 初期設定
            txtUserId.Text = "";
            rdbDispNum2.Checked = false;
            rdbDispNum4.Checked = false;
            rdbDispNum6.Checked = true;
            rdbDispNum9.Checked = false;

            this.ResumeLayout();
        }

        /// <summary>
        /// ユーザIDクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserId_Click(object sender, EventArgs e)
        {
            string strEmployeeNum = "";
            string strWorkerName = "";

            // 作業者検索画面を表示
            WorkerSelection frmTargetSelection = new WorkerSelection();
            frmTargetSelection.ShowDialog(this);
            this.Visible = true;

            // 返却値をセット
            strEmployeeNum = frmTargetSelection.strEmployeeNum;
            strWorkerName = frmTargetSelection.strWorkerName;

            if (!String.IsNullOrEmpty(strEmployeeNum))
            {
                // パラメータ反映
                m_strEmployeeNum = strEmployeeNum;
                m_strWorkerNm = strWorkerName;

                // ユーザIDに表示
                txtUserId.Text = m_strEmployeeNum + " " + m_strWorkerNm;

                // 入力不可にする
                txtUserId.ReadOnly = true;
                txtUserId.BackColor = SystemColors.Window;
            }
        }

        /// <summary>
        /// 表示数(画像)２クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum2_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum2.Checked = true;
        }

        /// <summary>
        /// 表示数(画像)４クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum4_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum4.Checked = true;
        }

        /// <summary>
        /// 表示数(画像)６クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum6_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum6.Checked = true;
        }

        /// <summary>
        /// 表示数(画像)９クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum9_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum9.Checked = true;
        }

        /// <summary>
        /// ログインボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            int intDispNum = 0;

            // 入力チェック
            if (String.IsNullOrEmpty(m_strEmployeeNum))
            {
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0038, "作業者"), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUserId_Click(null, null);
                return;
            }

            // 共通パラメータ設定
            if (this.rdbDispNum2.Checked == true) { intDispNum = 2; }
            else if (this.rdbDispNum4.Checked == true) { intDispNum = 4; }
            else if (this.rdbDispNum6.Checked == true) { intDispNum = 6; }
            else if (this.rdbDispNum9.Checked == true) { intDispNum = 9; }

            // ログイン処理
            g_clsLoginInfo.Login(m_strEmployeeNum, m_strWorkerNm, intDispNum);

            // 検査対象選択画面に遷移
            this.Visible = false;
            TargetSelection frmTargetSelection = new TargetSelection();
            frmTargetSelection.ShowDialog(this);
            this.Visible = true;
        }
        #endregion
    }
}
