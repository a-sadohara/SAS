using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static UserMasterMaintenance.Common;
using System.Text.RegularExpressions;
using Npgsql;

namespace UserMasterMaintenance
{
    public partial class UserEdit : Form
    {
        #region 定数・変数
        public int intEditMode;
        public string UserNo;
        public string parUserNm;
        public string parUserYomiGana;
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

            if (intEditMode == CON_EDITMODE_REG)
            {
                // 登録処理を行う
                if (RegistrationUser() == true)
                {
                    MessageBox.Show("登録しました");
                }
                else 
                {
                    return;
                }
            }
            if (intEditMode == CON_EDITMODE_UPD) 
            {
                // 更新処理を行う
                if (UpdateUser() == true)
                {
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
        #endregion

        #region メソッド
        /// <summary>
        /// 別画面遷移時初期表示処理
        /// </summary>
        /// <param name="parEditMode">実行モード（1：登録、2：更新）</param>
        /// <param name="parUserNo">初期表示ユーザ番号</param>
        /// <param name="parUserNm">初期表示ユーザ名</param>
        /// <param name="parUserYomiGana">初期表示ユーザ名カナ</param>
        public UserEdit(int parEditMode,
                        string parUserNo = "",
                        string parUserNm = "",
                        string parUserYomiGana = "")
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            intEditMode = parEditMode;

            // 更新モードで起動された場合、引数の情報を初期表示する
            if (intEditMode == CON_EDITMODE_UPD)
            {
                // 社員番号
                txtUserNo.Text = parUserNo;
                txtUserNo.Enabled = false;
                // 作業者名
                if (parUserNm.Split('　').Count() == 2)
                {
                    txtUserNm_Sei.Text = parUserNm.Split('　')[0];
                    txtUserNm_Mei.Text = parUserNm.Split('　')[1];
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
                if (parUserYomiGana.Split('　').Count() == 2)
                {
                    txtUserYomiGana_Sei.Text = parUserYomiGana.Split('　')[0];
                    txtUserYomiGana_Mei.Text = parUserYomiGana.Split('　')[1];
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
                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
                {
                    NpgsqlCon.Open();

                    // SQL文を作成する
                    string strSelSql = @"SELECT 
                                             *
                                         FROM
                                             SAGYOSYA 
                                         WHERE
                                             USERNO = '" + txtUserNo.Text + "'";

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
                            string strCreateSql = @"INSERT INTO SAGYOSYA (USERNO, USERNAME, USERYOMIGANA)
                                                               VALUES(:UserNo, :UserName, :UserYomigana)";

                            // SQLコマンドに各パラメータを設定する
                            var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                            command.Parameters.Add(new NpgsqlParameter("UserNo", DbType.String) { Value = txtUserNo.Text });
                            command.Parameters.Add(new NpgsqlParameter("UserName", DbType.String) { Value = txtUserNm_Sei.Text + "　" + txtUserNm_Mei.Text });
                            command.Parameters.Add(new NpgsqlParameter("UserYomigana", DbType.String) { Value = txtUserYomiGana_Sei.Text + "　" + txtUserYomiGana_Mei.Text });

                            // sqlを実行する
                            if (ExecTranSQL(command, transaction) == false)
                            {
                                return false;
                            }


                            transaction.Commit();
                        }

                        return true;
                    }
                }
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
        /// 登録処理
        /// </summary>
        /// <returns></returns>
        private Boolean UpdateUser()
        {
            try
            {
                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
                {
                    NpgsqlCon.Open();

                    using (var transaction = NpgsqlCon.BeginTransaction())
                    {
                        // SQL文を作成する
                        string strUpdateSql = @"UPDATE SAGYOSYA
                                                   SET USERNAME = :UserName
                                                     , USERYOMIGANA = :UserYomigana
                                                 WHERE
                                                       USERNO = :UserNo";

                        // SQLコマンドに各パラメータを設定する
                        var command = new NpgsqlCommand(strUpdateSql, NpgsqlCon, transaction);

                        command.Parameters.Add(new NpgsqlParameter("UserNo", DbType.String) { Value = txtUserNo.Text });
                        command.Parameters.Add(new NpgsqlParameter("UserName", DbType.String) { Value = txtUserNm_Sei.Text 
                                                                                                      + "　" 
                                                                                                      + txtUserNm_Mei.Text });
                        command.Parameters.Add(new NpgsqlParameter("UserYomigana", DbType.String) { Value = txtUserYomiGana_Sei.Text 
                                                                                                          + "　" 
                                                                                                          + txtUserYomiGana_Mei.Text });

                        // sqlを実行する
                        if (ExecTranSQL(command, transaction) == false)
                        {
                            return false;
                        }

                        transaction.Commit();
                    }

                    return true;
                }
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
        /// 登録・更新処理実行
        /// </summary>
        /// <param name="nscCommand">実行SQLコマンド</param>
        /// <param name="transaction">トランザクション情報</param>
        /// <returns></returns>
        public static Boolean ExecTranSQL(NpgsqlCommand nscCommand
                                        , NpgsqlTransaction transaction)
        {
            
            try
            {
                nscCommand.ExecuteNonQuery();
                return true;
            }
            catch (NpgsqlException ex)
            {
                transaction.Rollback();
                MessageBox.Show("登録時にエラーが発生しました。" 
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
