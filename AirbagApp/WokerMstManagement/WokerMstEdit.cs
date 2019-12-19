using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using static WokerMstManagement.Common;
using System.Text.RegularExpressions;
using Npgsql;

namespace WokerMstManagement
{
    public partial class WokerMstEdit : Form
    {
        #region 定数・変数
        private int m_intEditMode;
        public int g_intUpdateFlg = 0;
        public string g_strRegWorkerNo = "";
        #endregion

        #region イベント
        /// <summary>
        /// 確定ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFix_Click(object sender, EventArgs e)
        {
            // 入力チェックを行う
            if (InputDataCheck() == false)
            {
                return;
            }

            if (m_intEditMode == g_CON_EDITMODE_REG)
            {
                // 登録処理を行う
                if (RegistrationUser() == true)
                {
                    g_intUpdateFlg = 1;
                    MessageBox.Show("登録しました");
                }
                else 
                {
                    return;
                }
            }
            if (m_intEditMode == g_CON_EDITMODE_UPD) 
            {
                // 更新処理を行う
                if (UpdateUser() == true)
                {
                    g_intUpdateFlg = 1;
                    MessageBox.Show("更新しました");
                }
                else 
                {
                    return;
                }
            }

            this.Close();
        }

        /// <summary>
        /// 作業者番号入力イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 社員番号フォーカス消失処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserNo_Leave(object sender, EventArgs e)
        {
            int intUserNo = 0;

            if (txtUserNo.TextLength > 0)
            {
                if (Int32.TryParse(txtUserNo.Text, out intUserNo) == false)
                {
                    MessageBox.Show("数値のみ入力してください。");
                    txtUserNo.Focus();
                    return;
                }
                else
                {
                    txtUserNo.Text = String.Format("{0:D4}", intUserNo);
                }
            }
        }
        #endregion

        #region メソッド
        /// <summary>
        /// 別画面遷移時初期表示処理
        /// </summary>
        /// <param name="parEditMode">実行モード（1：登録、2：更新）</param>
        /// <param name="parUserNo">初期表示ユーザ番号</param>
        /// <param name="parUserNm">初期表示ユーザ名</param>
        /// <param name="parUserYomiGana">初期表示ユーザ名カナ</param>
        public WokerMstEdit(int parEditMode,
                        string parUserNo = "",
                        string parUserNm = "",
                        string parUserYomiGana = "")
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            m_intEditMode = parEditMode;

            // 更新モードで起動された場合、引数の情報を初期表示する
            if (m_intEditMode == g_CON_EDITMODE_UPD)
            {
                this.Text = "更新";
                // 社員番号
                txtUserNo.Text = parUserNo;
                txtUserNo.Enabled = false;
                // 作業者名
                if (parUserNm.Split(Convert.ToChar(g_CON_NAME_SEPARATE)).Count() == 2)
                {
                    txtUserNm_Sei.Text = parUserNm.Split(Convert.ToChar(g_CON_NAME_SEPARATE))[0];
                    txtUserNm_Mei.Text = parUserNm.Split(Convert.ToChar(g_CON_NAME_SEPARATE))[1];
                }
                else
                {
                    if (txtUserNm_Sei.MaxLength < parUserNm.Length)
                    {
                        txtUserNm_Sei.MaxLength = parUserNm.Length;
                    }
                    txtUserNm_Sei.Text = parUserNm;
                }
                // 読み仮名
                if (parUserYomiGana.Split(Convert.ToChar(g_CON_NAME_SEPARATE)).Count() == 2)
                {
                    txtUserYomiGana_Sei.Text = parUserYomiGana.Split(Convert.ToChar(g_CON_NAME_SEPARATE))[0];
                    txtUserYomiGana_Mei.Text = parUserYomiGana.Split(Convert.ToChar(g_CON_NAME_SEPARATE))[1];
                }
                else
                {
                    if (txtUserYomiGana_Sei.MaxLength < parUserYomiGana.Length)
                    {
                        txtUserYomiGana_Sei.MaxLength = parUserYomiGana.Length;
                    }
                    txtUserYomiGana_Sei.Text = parUserYomiGana;
                }
            }
            else
            {
                this.Text = "登録";
                // 登録モードで起動された場合、画面の各項目を空白で表示する
                txtUserNo.Clear();
                txtUserNm_Sei.Clear();
                txtUserNm_Mei.Clear();
                txtUserYomiGana_Sei.Clear();
            }
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <returns></returns>
        private Boolean InputDataCheck() 
        {
            // 必須入力チェック
            if (CheckRequiredInput(txtUserNo, "社員番号") == false ||
                CheckRequiredInput(txtUserNm_Sei, "作業者名") == false ||
                CheckRequiredInput(txtUserNm_Mei, "作業者名") == false ||
                CheckRequiredInput(txtUserYomiGana_Sei, "読み仮名") == false ||
                CheckRequiredInput(txtUserYomiGana_Mei, "読み仮名") == false) 
            {
                return false;
            }


            // カナ入力チェック
            if (Regex.IsMatch(txtUserYomiGana_Sei.Text, "^[ァ-ヶー]*$") == false) 
            {
                MessageBox.Show("全角カナのみ入力してください。");
                txtUserYomiGana_Sei.Focus();
                return false;
            }
            if (Regex.IsMatch(txtUserYomiGana_Mei.Text, "^[ァ-ヶー]*$") == false)
            {
                MessageBox.Show("全角カナのみ入力してください。");
                txtUserYomiGana_Mei.Focus();
                return false;
            }


            return true;
        }

        /// <summary>
        /// 必須入力チェック
        /// </summary>
        /// <param name="tbxCheckData">チェック対象テキストボックス</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <returns></returns>
        private Boolean CheckRequiredInput(TextBox tbxCheckData, String strItemName) 
        {
            // 必須入力チェック
            if (tbxCheckData.Text == "")
            {
                MessageBox.Show(strItemName + "は必須入力の項目です。");
                tbxCheckData.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <returns></returns>
        private Boolean RegistrationUser() 
        {
            try
            {
                if (g_bolModeNonDBCon == true)
                    return true;

                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
                {
                    NpgsqlCon.Open();

                    // SQL文を作成する
                    string strSelSql = @"SELECT 
                                             *
                                         FROM
                                             mst_Worker 
                                         WHERE
                                             employee_num = '" + txtUserNo.Text + "'";

                    // 重複チェックを行う
                    if (KeyDuplicateCheck(strSelSql, NpgsqlCon, "社員番号") == false)
                    {
                        txtUserNo.Focus();
                        return false;
                    }
                    else 
                    {
                        using (var transaction = NpgsqlCon.BeginTransaction())
                        {
                            // SQL文を作成する
                            string strCreateSql = @"INSERT INTO mst_Worker (
                                                                    employee_num
                                                                  , worker_name_sei
                                                                  , worker_name_mei
                                                                  , worker_name_sei_kana
                                                                  , worker_name_mei_kana
                                                                  , del_flg
                                                                  )VALUES(
                                                                   :UserNo
                                                                  ,:UserSurname
                                                                  ,:UserName
                                                                  ,:UserSurnameKana
                                                                  ,:UserNameKana
                                                                  , 0)";

                            // SQLコマンドに各パラメータを設定する
                            var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                            command.Parameters.Add(new NpgsqlParameter("UserNo", DbType.String) { Value = txtUserNo.Text });
                            command.Parameters.Add(new NpgsqlParameter("UserSurname", DbType.String) { Value = txtUserNm_Sei.Text });
                            command.Parameters.Add(new NpgsqlParameter("UserName", DbType.String) { Value = txtUserNm_Mei.Text });
                            command.Parameters.Add(new NpgsqlParameter("UserSurnameKana", DbType.String) { Value = txtUserYomiGana_Sei.Text });
                            command.Parameters.Add(new NpgsqlParameter("UserNameKana", DbType.String) { Value = txtUserYomiGana_Mei.Text });

                            // sqlを実行する
                            if (ExecTranSQL(command, transaction) == false)
                            {
                                return false;
                            }

                            transaction.Commit();

                            g_strRegWorkerNo = txtUserNo.Text;
                        }
                    }
                }

                return true;

            }
            catch (Exception ex) 
            {
                MessageBox.Show("登録時にエラーが発生しました。"
                               + Environment.NewLine
                               + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <returns></returns>
        private Boolean UpdateUser()
        {
            try
            {
                if (g_bolModeNonDBCon == true)
                    return true;

                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
                {
                    NpgsqlCon.Open();

                    using (var transaction = NpgsqlCon.BeginTransaction())
                    {
                        // SQL文を作成する
                        string strUpdateSql = @"UPDATE mst_Worker
                                                   SET worker_name_sei = :UserSurname
                                                     , worker_name_mei = :UserName
                                                     , worker_name_sei_kana = :UserSurnameKana
                                                     , worker_name_mei_kana = :UserNameKana
                                                 WHERE employee_num = :UserNo";

                        // SQLコマンドに各パラメータを設定する
                        var command = new NpgsqlCommand(strUpdateSql, NpgsqlCon, transaction);

                        command.Parameters.Add(new NpgsqlParameter("UserNo", DbType.String) { Value = txtUserNo.Text });
                        command.Parameters.Add(new NpgsqlParameter("UserSurname", DbType.String){ Value = txtUserNm_Sei.Text });
                        command.Parameters.Add(new NpgsqlParameter("UserName", DbType.String) { Value = txtUserNm_Mei.Text });
                        command.Parameters.Add(new NpgsqlParameter("UserSurnameKana", DbType.String) { Value = txtUserYomiGana_Sei.Text });
                        command.Parameters.Add(new NpgsqlParameter("UserNameKana", DbType.String) { Value = txtUserYomiGana_Mei.Text });


                        // sqlを実行する
                        if (ExecTranSQL(command, transaction) == false)
                        {
                            return false;
                        }

                        transaction.Commit();

                        g_strRegWorkerNo = txtUserNo.Text;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("更新時にエラーが発生しました。"
                               + Environment.NewLine
                               + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// キー重複チェック
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="NpgsqlCon"></param>
        /// <returns></returns>
        public static Boolean KeyDuplicateCheck(string strSql
                                              , NpgsqlConnection NpgsqlCon
                                              , String strItemName) 
        {

            try
            {
                var dataAdapter = new NpgsqlDataAdapter();
                dataAdapter = new NpgsqlDataAdapter(strSql, NpgsqlCon);
                // 作成したSQLを実行する
                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);

                // 取得件数が0件の場合は登録OK
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    return true;
                }
                else 
                {
                    MessageBox.Show(strItemName + "は既に使用されています。");
                    return false;
                }    
            }
            catch (Exception ex) 
            {
                MessageBox.Show("重複チェック時にエラーが発生しました。"
                               + Environment.NewLine
                               + ex.Message);
                return false;
            }

        }
        #endregion
    }
}
