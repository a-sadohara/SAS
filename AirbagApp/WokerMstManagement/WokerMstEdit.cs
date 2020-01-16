using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static WokerMstManagement.Common;

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
        private void btnDecision_Click(object sender, EventArgs e)
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
        private void txtEmployeeNum_KeyPress(object sender, KeyPressEventArgs e)
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
        private void txtEmployeeNum_Leave(object sender, EventArgs e)
        {
            if (txtEmployeeNum.TextLength > 0)
            {
                txtEmployeeNum.Text = String.Format("{0:D4}", int.Parse(txtEmployeeNum.Text));
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
                txtEmployeeNum.Text = parUserNo;
                txtEmployeeNum.Enabled = false;
                // 作業者名
                if (parUserNm.Split(Convert.ToChar(g_CON_NAME_SEPARATE)).Count() == 2)
                {
                    txtWorkerNameSei.Text = parUserNm.Split(Convert.ToChar(g_CON_NAME_SEPARATE))[0];
                    txtWorkerNameMei.Text = parUserNm.Split(Convert.ToChar(g_CON_NAME_SEPARATE))[1];
                }
                else
                {
                    if (txtWorkerNameSei.MaxLength < parUserNm.Length)
                    {
                        txtWorkerNameSei.MaxLength = parUserNm.Length;
                    }
                    txtWorkerNameSei.Text = parUserNm;
                }
                // 読みカナ
                if (parUserYomiGana.Split(Convert.ToChar(g_CON_NAME_SEPARATE)).Count() == 2)
                {
                    txtWorkerNameSeiKana.Text = parUserYomiGana.Split(Convert.ToChar(g_CON_NAME_SEPARATE))[0];
                    txtWorkerNameMeiKana.Text = parUserYomiGana.Split(Convert.ToChar(g_CON_NAME_SEPARATE))[1];
                }
                else
                {
                    if (txtWorkerNameSeiKana.MaxLength < parUserYomiGana.Length)
                    {
                        txtWorkerNameSeiKana.MaxLength = parUserYomiGana.Length;
                    }
                    txtWorkerNameSeiKana.Text = parUserYomiGana;
                }
            }
            else
            {
                this.Text = "登録";
                // 登録モードで起動された場合、画面の各項目を空白で表示する
                txtEmployeeNum.Clear();
                txtWorkerNameSei.Clear();
                txtWorkerNameMei.Clear();
                txtWorkerNameSeiKana.Clear();
            }
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <returns></returns>
        private Boolean InputDataCheck() 
        {
            // 必須入力チェック
            if (CheckRequiredInput(txtEmployeeNum, "社員番号") == false ||
                CheckRequiredInput(txtWorkerNameSei, "作業者名") == false ||
                CheckRequiredInput(txtWorkerNameMei, "作業者名") == false ||
                CheckRequiredInput(txtWorkerNameSeiKana, "読みカナ") == false ||
                CheckRequiredInput(txtWorkerNameMeiKana, "読みカナ") == false) 
            {
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
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0007, strItemName), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserNo", DbType = DbType.String, Value = txtEmployeeNum.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserSurname", DbType = DbType.String, Value = txtWorkerNameSei.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserName", DbType = DbType.String, Value = txtWorkerNameMei.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserSurnameKana", DbType = DbType.String, Value = txtWorkerNameSeiKana.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserNameKana", DbType = DbType.String, Value = txtWorkerNameMeiKana.Text });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand);

                g_clsConnectionNpgsql.DbCommit();

                g_strRegWorkerNo = txtEmployeeNum.Text;

                return true;
            }
            catch (Exception ex) 
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0002 + "\r\n" + ex.Message);

                if (((Npgsql.PostgresException)ex).SqlState == "23505")
                {
                    // 重複の例外時

                    // メッセージ出力
                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0008, "社員番号"), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // それ以外の例外時

                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0004, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <returns>true:正常終了 false:異常終了</returns>
        private Boolean UpdateUser()
        {
            try
            {
                // SQL文を作成する
                string strUpdateSql = @"UPDATE mst_Worker
                                           SET worker_name_sei = :UserSurname
                                             , worker_name_mei = :UserName
                                             , worker_name_sei_kana = :UserSurnameKana
                                             , worker_name_mei_kana = :UserNameKana
                                         WHERE employee_num = :UserNo";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserSurname", DbType = DbType.String, Value = txtWorkerNameSei.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserName", DbType = DbType.String, Value = txtWorkerNameMei.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserSurnameKana", DbType = DbType.String, Value = txtWorkerNameSeiKana.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserNameKana", DbType = DbType.String, Value = txtWorkerNameMeiKana.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserNo", DbType = DbType.String, Value = txtEmployeeNum.Text });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);

                g_clsConnectionNpgsql.DbCommit();

                g_strRegWorkerNo = txtEmployeeNum.Text;

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0002 + "\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0005, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 読みカナ入力イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtWorkerNameKana_KeyPress(object sender, KeyPressEventArgs e)
        {
            //カタカナ以外の時は、イベントをキャンセルする
            if (Regex.IsMatch(e.KeyChar.ToString(), @"^\p{IsKatakana}*$") == false && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }
        #endregion
    }
}
