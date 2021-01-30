using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using static WokerMstManagement.Common;

namespace WokerMstManagement
{
    public partial class WokerMstManagement : Form
    {
        #region 定数・変数
        private DataTable m_dtData;

        private string m_strKanaSta = string.Empty;
        private string m_strKanaEnd = string.Empty;

        private const Int32 m_CON_COL_USERNO = 0;
        private const Int32 m_CON_COL_USERNAME = 1;
        private const Int32 m_CON_COL_USERNAMEKANA = 2;
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WokerMstManagement_Load(object sender, EventArgs e)
        {
            bool bolProcOkNg = false;

            try
            {
                // 初期表示モードで明細表示を呼び出し
                dispDataGridView(0, "0000", 0);

                bolProcOkNg = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0003, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (bolProcOkNg == false)
                {
                    this.Close();
                }

            }
        }

        /// <summary>
        /// 初期表示
        /// </summary>
        public WokerMstManagement()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// 登録ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRegistration_Click(object sender, EventArgs e)
        {
            // 作業者登録画面を登録モードで表示する
            WokerMstEdit frmUserReg = new WokerMstEdit(g_CON_EDITMODE_REG);
            frmUserReg.ShowDialog();

            if (frmUserReg.g_intUpdateFlg == 1)
            {
                try
                {
                    // 登録表示モードで明細表示を呼び出し
                    dispDataGridView(1, frmUserReg.g_strRegWorkerNo, 0);
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0003, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 削除ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string strSelUserNo = string.Empty;
            string strSelUserName = string.Empty;
            string strSelUserNameKana = string.Empty;
            int intSelRow = 0;

            foreach (DataGridViewRow r in dgvWorker.SelectedRows)
            {
                strSelUserNo = NulltoString(r.Cells[m_CON_COL_USERNO].Value);
                strSelUserName = NulltoString(r.Cells[m_CON_COL_USERNAME].Value);
                strSelUserNameKana = NulltoString(r.Cells[m_CON_COL_USERNAMEKANA].Value);
                // 削除後の選択行
                if (r.Index == dgvWorker.Rows.Count - 1)
                {
                    intSelRow = r.Index - 1;
                }
                else
                {
                    intSelRow = r.Index;
                }
            }

            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0001, strSelUserNo, strSelUserName, strSelUserNameKana)
                               , "確認"
                               , MessageBoxButtons.YesNo
                               , MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (DelUser(strSelUserNo) == true)
                {
                    try
                    {
                        // 更新表示モードで明細表示を呼び出し
                        dispDataGridView(0, "0000", intSelRow);
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                        // メッセージ出力
                        MessageBox.Show(g_clsMessageInfo.strMsgE0003, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// csvインポートボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImportCsv_Click(object sender, EventArgs e)
        {
            WokerMstImportCsv frmUserImportCsv = new WokerMstImportCsv();

            if (System.Windows.Forms.DialogResult.OK == frmUserImportCsv.ShowDialog())
            {
                try
                {
                    // 初期表示モードで明細表示を呼び出し
                    dispDataGridView(0, "0000", 0);
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0003, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 文字リンククリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void llk_Click(object sender, EventArgs e)
        {
            if (sender == lblWorkerNameKanaア)
            {
                m_strKanaSta = "ア";
                m_strKanaEnd = "オ";
            }
            else if (sender == lblWorkerNameKanaカ)
            {
                m_strKanaSta = "カ";
                m_strKanaEnd = "コ";
            }
            else if (sender == lblWorkerNameKanaサ)
            {
                m_strKanaSta = "サ";
                m_strKanaEnd = "ソ";
            }
            else if (sender == lblWorkerNameKanaタ)
            {
                m_strKanaSta = "タ";
                m_strKanaEnd = "ト";
            }
            else if (sender == lblWorkerNameKanaナ)
            {
                m_strKanaSta = "ナ";
                m_strKanaEnd = "ノ";
            }
            else if (sender == lblWorkerNameKanaハ)
            {
                m_strKanaSta = "ハ";
                m_strKanaEnd = "ホ";
            }
            else if (sender == lblWorkerNameKanaマ)
            {
                m_strKanaSta = "マ";
                m_strKanaEnd = "モ";
            }
            else if (sender == lblWorkerNameKanaヤ)
            {
                m_strKanaSta = "ヤ";
                m_strKanaEnd = "ヨ";
            }
            else if (sender == lblWorkerNameKanaラ)
            {
                m_strKanaSta = "ラ";
                m_strKanaEnd = "ロ";
            }
            else if (sender == lblWorkerNameKanaワ)
            {
                m_strKanaSta = "ワ";
                m_strKanaEnd = "ン";
            }
            else if (sender == lblWorkerNameKanaEtc)
            {
                m_strKanaSta = "！";
                m_strKanaEnd = "！";
            }
            else if (sender == lblWorkerNameKanaNonCondition)
            {
                m_strKanaSta = string.Empty;
                m_strKanaEnd = string.Empty;
                txtEmployeeNumFrom.Text = string.Empty;
                txtEmployeeNumTo.Text = string.Empty;
            }

            foreach (Label lbl in new Label[] { lblWorkerNameKanaア, lblWorkerNameKanaカ, lblWorkerNameKanaサ, lblWorkerNameKanaタ, lblWorkerNameKanaナ,
                                                lblWorkerNameKanaハ, lblWorkerNameKanaマ, lblWorkerNameKanaヤ, lblWorkerNameKanaラ, lblWorkerNameKanaワ,
                                                lblWorkerNameKanaEtc, lblWorkerNameKanaNonCondition })
            {
                if (sender == lbl)
                {
                    lbl.BackColor = System.Drawing.SystemColors.Highlight;
                }
                else
                {
                    lbl.BackColor = System.Drawing.Color.Transparent;
                }
            }
        }

        /// <summary>
        /// 作業者グリッドセルダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvWorker_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            string strSelEmployeeNum = "0000";
            bool bolDelFlg = int.Parse(dgvWorker.Rows[e.RowIndex].Cells[3].Value.ToString()) == 1 ? true : false;

            // 選択行の社員番号取得
            foreach (DataGridViewRow dgvRow in this.dgvWorker.SelectedRows)
            {
                strSelEmployeeNum = dgvWorker.Rows[dgvRow.Index].Cells["EmployeeNum"].Value.ToString();
                break;
            }

            // 作業者登録画面を更新モードで表示する
            WokerMstEdit frmUserReg = new WokerMstEdit(g_CON_EDITMODE_UPD,
                                                       dgvWorker.Rows[e.RowIndex].Cells[0].Value.ToString(),
                                                       dgvWorker.Rows[e.RowIndex].Cells[1].Value.ToString(),
                                                       dgvWorker.Rows[e.RowIndex].Cells[2].Value.ToString(),
                                                       bolDelFlg);
            frmUserReg.ShowDialog();
            if (frmUserReg.g_intUpdateFlg == 1)
            {
                try
                {
                    // 更新表示モードで明細表示を呼び出し
                    dispDataGridView(1, strSelEmployeeNum, e.RowIndex);
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0003, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 検索ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                // 初期表示モードで明細表示を呼び出し
                dispDataGridView(0, "0000", 0);
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0003, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        /// 作業者番号Fromフォーカス消失
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtEmployeeNum_Leave(object sender, EventArgs e)
        {
            int intUserNo = 0;

            TextBox tb = (TextBox)sender;

            if (tb.TextLength > 0)
            {
                if (Int32.TryParse(tb.Text, out intUserNo) == false)
                {
                    MessageBox.Show("数値のみ入力してください。");
                    tb.Focus();
                    return;
                }
                else
                {
                    tb.Text = String.Format("{0:D4}", intUserNo);
                }
            }
        }
        #endregion

        #region メソッド
        /// <summary>
        /// データグリッドビュー表示処理
        /// </summary>
        /// <param name="intExecMode">検索モード
        ///   0：初期表示＆検索＆削除
        ///   1：登録
        ///   2：更新
        /// </param>
        /// <param name="strSelWorkerNo">明細検索対象</param>
        /// <param name="intLastTimeSelLine">前回選択行</param>
        private void dispDataGridView(int intExecMode
                                    , string strSelWorkerNo
                                    , int intLastTimeSelLine)
        {
            string strSQL = string.Empty;

            dgvWorker.Rows.Clear();

            int intUserNoSta = 0;
            int intUserNoEnd = 0;

            try
            {
                // 条件が指定されていない場合は抽出しない
                // SQL抽出
                m_dtData = new DataTable();
                strSQL += @"SELECT 
                                employee_num
                              , worker_name_sei || '" + g_CON_NAME_SEPARATE + @"' || worker_name_mei as worker_name
                              , worker_name_sei_kana || '" + g_CON_NAME_SEPARATE + @"' || worker_name_mei_kana as worker_name_kana
                              , del_flg
                            FROM 
                                    mst_Worker ";
                strSQL += "WHERE 1 = 1 ";
                if (m_strKanaSta == "！" && m_strKanaEnd == "！")
                {
                    strSQL += @"AND NOT(
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ア' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'オ') 
                                   OR 
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'カ' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'コ') 
                                   OR 
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'サ' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'ソ') 
                                   OR 
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'タ' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'ト') 
                                   OR 
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ナ' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'ノ') 
                                   OR 
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ハ' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'ホ') 
                                   OR 
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'マ' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'モ') 
                                   OR 
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ヤ' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'ヨ') 
                                   OR 
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ラ' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'ロ') 
                                   OR 
                                   ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ワ' AND 
                                     SUBSTRING(worker_name_sei_kana,1,1) <= 'ン') 
                               ) ";
                }
                else
                {
                    if (!string.IsNullOrEmpty(m_strKanaSta))
                    {
                        strSQL += "AND SUBSTRING(worker_name_sei_kana,1,1) >= '" + m_strKanaSta + "' ";
                    }
                    if (!string.IsNullOrEmpty(m_strKanaEnd))
                    {
                        strSQL += "AND SUBSTRING(worker_name_sei_kana,1,1) <= '" + m_strKanaEnd + "' ";
                    }
                }

                if (!string.IsNullOrEmpty(txtEmployeeNumFrom.Text))
                {
                    Int32.TryParse(txtEmployeeNumFrom.Text, out intUserNoSta);
                    strSQL += "AND TO_NUMBER(employee_num, '0000') >= " + intUserNoSta + " ";
                }
                if (!string.IsNullOrEmpty(txtEmployeeNumTo.Text))
                {
                    Int32.TryParse(txtEmployeeNumTo.Text, out intUserNoEnd);
                    strSQL += "AND TO_NUMBER(employee_num, '0000') <= " + intUserNoEnd + " ";
                }

                strSQL += "ORDER BY employee_num ASC ;";
                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL);

                // データグリッドビューに反映
                foreach (DataRow row in m_dtData.Rows)
                {
                    this.dgvWorker.Rows.Add(row.ItemArray);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            dgvWorker.CurrentCell = null;

            if (dgvWorker.Rows.Count > 0)
            {
                // 表示モードが初期表示の場合
                if (intExecMode == 0)
                {
                    // 0行目表示
                    dgvWorker.Rows[0].Selected = true;
                    dgvWorker.FirstDisplayedScrollingRowIndex = 0;
                }

                if (intExecMode == 1)
                {
                    int intSelRow = 0;

                    // 対象の選択行を探す
                    foreach (DataGridViewRow r in dgvWorker.Rows)
                    {
                        if (strSelWorkerNo == NulltoString(r.Cells[m_CON_COL_USERNO].Value))
                        {
                            intSelRow = r.Index;
                            break;
                        }
                    }

                    // 更新行表示
                    dgvWorker.Rows[intSelRow].Selected = true;
                    // 対象行が既に画面内に表示されている時は何もしない
                    if (dgvWorker.SelectedRows[0].Displayed)
                    {
                        return;
                    }
                    dgvWorker.FirstDisplayedScrollingRowIndex = intSelRow;
                }

                // 表示モードが更新表示の場合
                if (intExecMode == 2)
                {
                    // 更新行表示
                    dgvWorker.Rows[intLastTimeSelLine].Selected = true;

                    // 対象行が既に画面内に表示されている時は何もしない
                    if (dgvWorker.SelectedRows[0].Displayed)
                    {
                        return;
                    }
                    dgvWorker.FirstDisplayedScrollingRowIndex = intLastTimeSelLine;
                }
            }
        }

        /// <summary>
        /// ユーザ削除処理
        /// </summary>
        /// <param name="NpgsqlCon">接続子</param>
        /// <param name="transaction">トランザクション</param>
        /// <returns></returns>
        private Boolean DelUser(string strSelUserNo)
        {
            try
            {
                // SQL文を作成する
                string strUpdateSql = @"UPDATE mst_Worker
                                       SET del_flg = 1
                                     WHERE employee_num = :UserNo";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "UserNo", DbType = DbType.String, Value = strSelUserNo });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);

                g_clsConnectionNpgsql.DbCommit();

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0006, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }
        #endregion
    }
}