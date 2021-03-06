﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
        public string g_strRegWorkerNo = string.Empty;
        public string g_strShow = "表示";
        public string g_strHide = "非表示";
        #endregion

        #region イベント
        /// <summary>
        /// 確定ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDecision_Click(object sender, EventArgs e)
        {
            if (m_intEditMode == g_CON_EDITMODE_REG)
            {
                if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0002
                                                , chkDelFlg.Checked ? g_strHide : g_strShow
                                                , txtEmployeeNum.Text
                                                , txtWorkerNameSei.Text
                                                , txtWorkerNameMei.Text
                                                , txtWorkerNameSeiKana.Text
                                                , txtWorkerNameMeiKana.Text)
                                   , "確認"
                                   , MessageBoxButtons.YesNo
                                   , MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }


                // 入力チェックを行う
                if (InputDataCheck() == false)
                {
                    return;
                }

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
                if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0003
                                                , chkDelFlg.Checked ? g_strHide : g_strShow
                                                , txtEmployeeNum.Text
                                                , txtWorkerNameSei.Text
                                                , txtWorkerNameMei.Text
                                                , txtWorkerNameSeiKana.Text
                                                , txtWorkerNameMeiKana.Text)
                                   , "確認"
                                   , MessageBoxButtons.YesNo
                                   , MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // 入力チェックを行う
                if (InputDataCheck() == false)
                {
                    return;
                }

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
            if (e.KeyChar == '\r')
            {
                // 改行の時は、次のコントロールをアクティブにする
                this.SelectNextControl(this.ActiveControl, true, true, true, true);
            }
            else if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                // 0～9と、バックスペース以外の時は、イベントをキャンセルする
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
        /// <param name="parUserYomiGana">初期表示削除フラグ</param>
        public WokerMstEdit(int parEditMode,
                            string parUserNo = "",
                            string parUserNm = "",
                            string parUserYomiGana = "",
                            bool parDelFlg = false)
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            m_intEditMode = parEditMode;

            if (m_intEditMode != g_CON_EDITMODE_UPD)
            {
                // 登録モードで起動された場合、画面の各項目を空白で表示する
                txtEmployeeNum.Clear();
                txtWorkerNameSei.Clear();
                txtWorkerNameMei.Clear();
                txtWorkerNameSeiKana.Clear();
                return;
            }

            // 更新モードで起動された場合、引数の情報を初期表示する
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

            // 非表示フラグ
            chkDelFlg.Checked = parDelFlg;
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <returns></returns>
        private Boolean InputDataCheck()
        {
            // 必須入力チェック
            if (CheckRequiredInput(txtEmployeeNum, "社員番号") == false ||
                CheckRequiredInput(txtWorkerNameSei, "作業者名　姓") == false ||
                CheckRequiredInput(txtWorkerNameMei, "作業者名　名") == false ||
                CheckRequiredInput(txtWorkerNameSeiKana, "読みカナ　姓") == false ||
                CheckRequiredInput(txtWorkerNameMeiKana, "読みカナ　名") == false)
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
            if (string.IsNullOrEmpty(tbxCheckData.Text))
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
                                                      ,:DelFlg)";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserNo", DbType = DbType.String, Value = txtEmployeeNum.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserSurname", DbType = DbType.String, Value = txtWorkerNameSei.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserName", DbType = DbType.String, Value = txtWorkerNameMei.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserSurnameKana", DbType = DbType.String, Value = txtWorkerNameSeiKana.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserNameKana", DbType = DbType.String, Value = txtWorkerNameMeiKana.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "DelFlg", DbType = DbType.Int32, Value = chkDelFlg.Checked ? 1 : 0 });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand);

                g_clsConnectionNpgsql.DbCommit();

                g_strRegWorkerNo = txtEmployeeNum.Text;

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    if (((Npgsql.PostgresException)ex).SqlState == "23505")
                    {
                        // 重複の例外時

                        // メッセージ出力
                        MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0008, "社員番号"), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        txtEmployeeNum.Focus();
                    }
                    else
                    {
                        // それ以外の例外時

                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));

                        // メッセージ出力
                        MessageBox.Show(g_clsMessageInfo.strMsgE0004, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception)
                {
                    // ※DB非接続時は重複時の判断でエラーになる考慮

                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));

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
                                             , del_flg = :DelFlg
                                         WHERE employee_num = :UserNo";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserSurname", DbType = DbType.String, Value = txtWorkerNameSei.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserName", DbType = DbType.String, Value = txtWorkerNameMei.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserSurnameKana", DbType = DbType.String, Value = txtWorkerNameSeiKana.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserNameKana", DbType = DbType.String, Value = txtWorkerNameMeiKana.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "DelFlg", DbType = DbType.Int32, Value = chkDelFlg.Checked ? 1 : 0 });
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
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));
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
        /// 作業者名入力イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtWorkerName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                // 改行の時は、次のコントロールをアクティブにする
                this.SelectNextControl(this.ActiveControl, true, true, true, true);
            }
            else if (!(Encoding.GetEncoding("Shift-JIS").GetByteCount(e.KeyChar.ToString()) == e.KeyChar.ToString().Length * 2) && e.KeyChar != '\b')
            {
                // 全角文字以外の時は、イベントをキャンセルする
                e.Handled = true;
            }
        }

        /// <summary>
        /// 読みカナ入力イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtWorkerNameKana_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                // 改行の時は、次のコントロールをアクティブにする
                this.SelectNextControl(this.ActiveControl, true, true, true, true);
            }
            else if (Regex.IsMatch(e.KeyChar.ToString(), @"^\p{IsKatakana}*$") == false && e.KeyChar != '\b')
            {
                // カタカナ以外の時は、イベントをキャンセルする
                e.Handled = true;
            }
        }
        #endregion
    }
}