using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ImageChecker.Common;
using Npgsql;

namespace ImageChecker
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
        public LoginForm(string UserNo = "", short DispNum = 0)
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            // ユーザIDを初期選択
            txtUser.Select();

            // 初期設定
            // 1.ユーザID
            if (!String.IsNullOrEmpty(UserNo))
            {
                // ユーザIDを初期選択
                txtUser.Select();

                // 入力制限
                txtUser.ReadOnly = false;
                txtUser.MaxLength = 4;
                txtUser.Text = UserNo;
            }
            else
            {
                txtUser.Text = "";
            }

            // 2.表示枚数
            switch (DispNum)
            {
                case 2:
                    rdbDispNum2.Checked = true;
                    break;
                case 4:
                    rdbDispNum4.Checked = true;
                    break;
                case 6:
                    rdbDispNum6.Checked = true;
                    break;
                case 9:
                    rdbDispNum9.Checked = true;
                    break;
                default:
                    rdbDispNum6.Checked = true;
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
                try
                {
                    if (bolModeNonDBCon == true)
                        throw new Exception("DB非接続モードです");

                    // PostgreSQLへ接続
                    using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
                    {
                        NpgsqlCon.Open();

                        // SQL抽出
                        NpgsqlCommand NpgsqlCom = null;
                        NpgsqlDataAdapter NpgsqlDtAd = null;

                        dtData = new DataTable();
                        strSQL += "SELECT employee_num,  ";
                        strSQL += "worker_name_sei || worker_name_mei AS worker_name, ";
                        strSQL += "worker_name_sei_kana || worker_name_mei_kana AS worker_name_kana  ";
                        strSQL += "FROM mst_worker ";
                        strSQL += "WHERE employee_num = '" + txtUser.Text + "'";
                        NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                        NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                        NpgsqlDtAd.Fill(dtData);
                    }
                }
                catch
                {
                    dtData = new DataTable();
                    dtData.Columns.Add("USERNO");
                    dtData.Columns.Add("USERNAME");
                    dtData.Columns.Add("USERYOMIGANA");

                    // 後々この処理は消す
                    foreach (string line in System.IO.File.ReadLines("作業者.tsv", Encoding.Default))
                    {
                        // 改行コードを変換
                        string strLine = line.Replace("\\rn", Environment.NewLine);

                        string[] csv = strLine.Split('\t');
                        string[] data = new string[csv.Length];
                        Array.Copy(csv, 0, data, 0, data.Length);

                        if (data[0].ToString() != txtUser.Text)
                            continue;

                        dtData.Rows.Add(data);
                    }
                }

                if (dtData.Rows.Count > 0)
                {
                    parUserNo = txtUser.Text;
                    parUserNm = dtData.Rows[0][1].ToString();
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

                // 次のコントロールを選択する
                this.SelectNextControl(txtUser, true, true, true, true);
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

        private void LoginForm_Load(object sender, EventArgs e)
        {
            //// ユーザIDを初期選択
            //txtUser.Select();
        }
    }
}
