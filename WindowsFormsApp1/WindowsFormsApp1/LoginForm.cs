using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp1.Common;
using Npgsql;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        public String parUserNo;
        public String parUserNm;
        public NpgsqlConnection NpgsqlCon;
        public DataTable dtData;

        #region イベント

        private void picDispNum2_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum2.Checked = true;
        }

        private void picDispNum4_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum4.Checked = true;
        }

        private void picDispNum6_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum6.Checked = true;
        }

        private void picDispNum9_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum9.Checked = true;
        }

        private void txtUser_FocusIn(object sender, EventArgs e)
        {
            // 選択情報を初期化
            parUserNo = "";
            parUserNm = "";
            txtUser.Text = "";

            // 入力制限
            txtUser.ReadOnly = false;
            txtUser.MaxLength = 4;
        }

        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (String.IsNullOrEmpty(txtUser.Text))
                {
                    // 入力されていない　⇒　チェックせずに選択画面に遷移
                    SelectUser(false);
                }
                else
                {
                    // 入力されている　⇒　チェック
                    SelectUser();
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (!String.IsNullOrEmpty(parUserNo))
                {
                    // フォーカスを当てた状態（初期化＆入力制限）にする
                    txtUser_FocusIn(sender, e);
                }
            }
        }

        private void txtUser_DoubleClick(object sender, EventArgs e)
        {
            SelectUser(false);
        }

        private void txtUser_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 0～9、バックスペース、エンター以外の時は、イベントをキャンセルする
            if (e.KeyChar != '\r' && e.KeyChar != '\b' && e.KeyChar < '0' || '9' < e.KeyChar)
            {
                e.Handled = true;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // 入力チェック
            if (String.IsNullOrEmpty(parUserNo))
            {
                MessageBox.Show("職員を選択してください。");
                return;
            }

            // 共通パラメータ設定
            g_parUserNo = parUserNo;
            g_parUserNm = parUserNm;
            if (this.rdbDispNum2.Checked == true) { g_parDispNum = 2; }
            if (this.rdbDispNum4.Checked == true) { g_parDispNum = 4; }
            if (this.rdbDispNum6.Checked == true) { g_parDispNum = 6; }
            if (this.rdbDispNum9.Checked == true) { g_parDispNum = 9; }

            LogIn();

            // 検査対象選択画面に遷移
            this.Visible = false;
            TargetSelection frmTargetSelection = new TargetSelection();

            frmTargetSelection.ShowDialog(this);
            this.Visible = true;

        }

        #endregion

        #region メソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LoginForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            // 初期設定

            // 1.ユーザID
            if (!String.IsNullOrEmpty(parUserNo))
            {
                txtUser.Text = parUserNo + " " + parUserNm;
            }
            else
            {
                txtUser.Text = "";
            }

            // 2.表示枚数
            switch (g_parDispNum)
            {
                case 2:
                    rdbDispNum2.Checked = true;
                    break;
                case 4:
                    rdbDispNum4.Checked = true;
                    break;
                case 9:
                    rdbDispNum9.Checked = true;
                    break;
                default:
                    rdbDispNum9.Checked = true;
                    break;
            }
        }

        /// <summary>
        /// 職員選択
        /// </summary>
        /// <param name="parChk">チェック有無</param>
        private void SelectUser(bool parChk = true)
        {
            string strSQL = "";

            if (parChk)
            {
                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
                {
                    NpgsqlCon.Open();

                    // SQL抽出
                    NpgsqlCommand NpgsqlCom = null;
                    NpgsqlDataAdapter NpgsqlDtAd = null;
                    dtData = new DataTable();
                    strSQL += "SELECT USERNAME FROM SAGYOSYA ";
                    strSQL += "WHERE USERNO = '" + txtUser.Text + "'";
                    NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                    NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                    NpgsqlDtAd.Fill(dtData);

                    if (dtData.Rows.Count > 0)
                    {
                        parUserNo = txtUser.Text;
                        parUserNm = dtData.Rows[0][0].ToString();
                    }
                    else
                    {
                        MessageBox.Show("入力された職員番号は存在しません");

                        UserSelection frmTargetSelection = new UserSelection();
                        frmTargetSelection.ShowDialog(this);
                        this.Visible = true;

                        parUserNo = frmTargetSelection.strUserNo;
                        parUserNm = frmTargetSelection.strUserNm;
                    }
                }
            }
            else
            {
                UserSelection frmTargetSelection = new UserSelection();
                frmTargetSelection.ShowDialog(this);
                this.Visible = true;

                parUserNo = frmTargetSelection.strUserNo;
                parUserNm = frmTargetSelection.strUserNm;
            }

            if (!String.IsNullOrEmpty(parUserNo))
            {
                txtUser.Text = parUserNo + " "  + parUserNm;

                // 入力不可にする
                txtUser.ReadOnly = true;
                txtUser.BackColor = SystemColors.Window;
            }
            else
            {
                // 選択情報を初期化
                parUserNo = "";
                parUserNm = "";
                txtUser.Text = "";

                // 入力制限
                txtUser.ReadOnly = false;
                txtUser.MaxLength = 4;
            }

        }

        #endregion

    }
}
