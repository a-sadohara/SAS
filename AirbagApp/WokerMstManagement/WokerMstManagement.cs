using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using static WokerMstManagement.Common;
using Npgsql;
using System.IO;

namespace WokerMstManagement
{
    public partial class WokerMstManagement : Form
    {
        #region 定数・変数
        private DataTable m_dtData;

        private string m_strKanaSta = "";
        private string m_strKanaEnd = "";

        private const Int32 m_CON_COL_USERNO = 0;
        private const Int32 m_CON_COL_USERNAME = 1;
        private const Int32 m_CON_COL_USERNAMEKANA = 2;
        #endregion

        #region イベント
        /// <summary>
        /// 初期表示
        /// </summary>
        public WokerMstManagement()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            // 初期表示モードで明細表示を呼び出し
            dispDataGridView(0, "0000", 0);
        }

        /// <summary>
        /// 登録ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReg_Click(object sender, EventArgs e)
        {
            // 作業者登録画面を登録モードで表示する
            WokerMstEdit frmUserReg = new WokerMstEdit(g_CON_EDITMODE_REG);
            frmUserReg.ShowDialog();
            if(frmUserReg.g_intUpdateFlg == 1) 
            {
                // 登録表示モードで明細表示を呼び出し
                dispDataGridView(1, frmUserReg.g_strRegWorkerNo, 0);
            }
        }

        /// <summary>
        /// 削除ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, EventArgs e)
        {
            string strSelUserNo = "";
            string strSelUserName = "";
            string strSelUserNameKana = "";
            int intSelRow = 0;

            if (dgvUser.SelectedRows.Count == 0)
            {
                MessageBox.Show("削除する行を選択してください");
                return;
            }
            else
            {
                foreach (DataGridViewRow r in dgvUser.SelectedRows)
                {
                    strSelUserNo = NulltoString(r.Cells[m_CON_COL_USERNO].Value);
                    strSelUserName = NulltoString(r.Cells[m_CON_COL_USERNAME].Value);
                    strSelUserNameKana = NulltoString(r.Cells[m_CON_COL_USERNAMEKANA].Value);
                    // 削除後の選択行
                    if (r.Index == dgvUser.Rows.Count - 1)
                    {
                        intSelRow = r.Index - 1;
                    }
                    else 
                    {
                        intSelRow = r.Index;
                    }
                }

                if (MessageBox.Show("選択されている行（データ）を削除しますか？"
                                   + Environment.NewLine
                                   + "社員番号：" + strSelUserNo + " 作業者名：" + strSelUserName + " 読み仮名：" + strSelUserNameKana
                                   , "確認"
                                   , MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (g_bolModeNonDBCon == true)
                    {
                        MessageBox.Show("削除しました");
                    }
                    else
                    {
                        try
                        {
                            // PostgreSQLへ接続
                            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
                            {
                                NpgsqlCon.Open();

                                using (var transaction = NpgsqlCon.BeginTransaction())
                                {
                                    if (DelUser(NpgsqlCon, transaction, strSelUserNo) == true)
                                    {
                                        transaction.Commit();
                                        MessageBox.Show("削除しました");

                                        // 更新表示モードで明細表示を呼び出し
                                        dispDataGridView(2, "0000", intSelRow);
                                    }
                                    else
                                    {
                                        MessageBox.Show("削除に失敗したためロールバックします");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("削除時にエラーが発生しました。"
                                           + Environment.NewLine
                                           + ex.Message);
                        }
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
                // 初期表示モードで明細表示を呼び出し
                dispDataGridView(0, "0000", 0);
            }
        }

        /// <summary>
        /// 文字リンククリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void llk_Click(dynamic sender, EventArgs e)
        {
            if (sender == llkア) { m_strKanaSta = "ア"; m_strKanaEnd = "オ"; }
            if (sender == llkカ) { m_strKanaSta = "カ"; m_strKanaEnd = "コ"; }
            if (sender == llkサ) { m_strKanaSta = "サ"; m_strKanaEnd = "ソ"; }
            if (sender == llkタ) { m_strKanaSta = "タ"; m_strKanaEnd = "ト"; }
            if (sender == llkナ) { m_strKanaSta = "ナ"; m_strKanaEnd = "ノ"; }
            if (sender == llkハ) { m_strKanaSta = "ハ"; m_strKanaEnd = "ホ"; }
            if (sender == llkマ) { m_strKanaSta = "マ"; m_strKanaEnd = "モ"; }
            if (sender == llkヤ) { m_strKanaSta = "ヤ"; m_strKanaEnd = "ヨ"; }
            if (sender == llkラ) { m_strKanaSta = "ラ"; m_strKanaEnd = "ロ"; }
            if (sender == llkワ) { m_strKanaSta = "ワ"; m_strKanaEnd = "ン"; }
            if (sender == llkEtc) { m_strKanaSta = "！"; m_strKanaEnd = "！"; }
            if (sender == llkNon) { m_strKanaSta = ""; m_strKanaEnd = ""; }

            foreach (Label lbl in new Label[] { llkア, llkカ, llkサ, llkタ, llkナ,
                                                llkハ, llkマ, llkヤ, llkラ, llkワ,
                                                llkEtc, llkNon })
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
        private void dgvUser_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            // 作業者登録画面を更新モードで表示する
            WokerMstEdit frmUserReg = new WokerMstEdit(g_CON_EDITMODE_UPD,
                                               dgvUser.Rows[e.RowIndex].Cells[0].Value.ToString(),
                                               dgvUser.Rows[e.RowIndex].Cells[1].Value.ToString(),
                                               dgvUser.Rows[e.RowIndex].Cells[2].Value.ToString());
            frmUserReg.ShowDialog();
            if (frmUserReg.g_intUpdateFlg == 1)
            {
                // 更新表示モードで明細表示を呼び出し
                dispDataGridView(2, "0000", e.RowIndex);
            }
        }

        /// <summary>
        /// 検索ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            // 初期表示モードで明細表示を呼び出し
            dispDataGridView(0, "0000", 0);
        }

        /// <summary>
        /// 作業者番号From入力イベント
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
        /// 作業者番号Fromフォーカス消失
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserNo_From_Leave(object sender, EventArgs e)
        {
            int intUserNo = 0;

            if (txtUserNo_From.TextLength > 0)
            {
                if (Int32.TryParse(txtUserNo_From.Text, out intUserNo) == false)
                {
                    MessageBox.Show("数値のみ入力してください。");
                    txtUserNo_From.Focus();
                    return;
                }
                else
                {
                    txtUserNo_From.Text = String.Format("{0:D4}", intUserNo);
                }
            }
        }

        /// <summary>
        /// 作業者番号Toフォーカス消失
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserNo_To_Leave(object sender, EventArgs e)
        {
            int intUserNo = 0;

            if (txtUserNo_To.TextLength > 0)
            {
                if (Int32.TryParse(txtUserNo_To.Text, out intUserNo) == false)
                {
                    MessageBox.Show("数値のみ入力してください。");
                    txtUserNo_To.Focus();
                    return;
                }
                else
                {
                    txtUserNo_To.Text = String.Format("{0:D4}", intUserNo);
                }
            }
        }
        #endregion

        #region メソッド
        /// <summary>
        /// データグリッドビュー表示処理
        /// </summary>
        /// <param name="intExecMode">検索モード
        ///   0：初期表示＆検索
        ///   1：登録
        ///   2：更新
        /// </param>
        /// <param name="strSelWorkerNo">明細検索対象</param>
        /// <param name="intLastTimeSelLine">前回選択行</param>
        private void dispDataGridView(int intExecMode
                                    , string strSelWorkerNo
                                    , int intLastTimeSelLine)
        {
            string strSQL = "";

            dgvUser.Rows.Clear();

            int intUserNoSta = 0;
            int intUserNoEnd = 0;

            try
            {
                if (g_bolModeNonDBCon == true)
                    throw new Exception("DB非接続モードです");

                // 条件が指定されていない場合は抽出しない
                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
                {
                    NpgsqlCon.Open();

                    // SQL抽出
                    NpgsqlCommand NpgsqlCom = null;
                    NpgsqlDataAdapter NpgsqlDtAd = null;
                    m_dtData = new DataTable();
                    strSQL += @"SELECT 
                                    employee_num
                                  , worker_name_sei || '" + g_CON_NAME_SEPARATE + @"' || worker_name_mei as worker_name
                                  , worker_name_sei_kana || '" + g_CON_NAME_SEPARATE + @"' || worker_name_mei_kana as worker_name_kana
                                FROM 
                                    mst_Worker ";
                    strSQL += "WHERE del_flg = 0 ";
                    if (!string.IsNullOrEmpty(m_strKanaSta))
                    {
                        strSQL += "AND SUBSTRING(worker_name_sei_kana,1,1) >= '" + m_strKanaSta + "' ";
                    }
                    if (!string.IsNullOrEmpty(m_strKanaEnd))
                    {
                        strSQL += "AND SUBSTRING(worker_name_sei_kana,1,1) <= '" + m_strKanaEnd + "' ";
                    }
                    if (txtUserNo_From.Text != "")
                    {
                        Int32.TryParse(txtUserNo_From.Text, out intUserNoSta);
                        strSQL += "AND TO_NUMBER(employee_num, '0000') >= " + intUserNoSta + " ";
                    }
                    if (txtUserNo_To.Text != "")
                    {
                        Int32.TryParse(txtUserNo_To.Text, out intUserNoEnd);
                        strSQL += "AND TO_NUMBER(employee_num, '0000') <= " + intUserNoEnd + " ";
                    }
                    strSQL += "ORDER BY employee_num ASC ;";
                    NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                    NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                    NpgsqlDtAd.Fill(m_dtData);

                    // データグリッドビューに反映
                    foreach (DataRow row in m_dtData.Rows)
                    {
                        this.dgvUser.Rows.Add(row.ItemArray);
                    }
                }
            }
            catch (Exception e)
            {
                string strErrMsg = "";
                strErrMsg = e.Message;

                // 後々この処理は消す
                foreach (string line in File.ReadLines("作業者.tsv", Encoding.Default))
                {
                    // 改行コードを変換
                    string strLine = line.Replace("\\rn", Environment.NewLine);

                    string[] csv = strLine.Split('\t');
                    string[] data = new string[csv.Length];
                    Array.Copy(csv, 0, data, 0, data.Length);
                    this.dgvUser.Rows.Add(data);
                }
            }

            dgvUser.CurrentCell = null;

            if (dgvUser.Rows.Count > 0)
            {
                // 表示モードが初期表示の場合
                if (intExecMode == 0)
                {
                    // 0行目表示
                    dgvUser.Rows[0].Selected = true;
                    dgvUser.FirstDisplayedScrollingRowIndex = 0;
                }
                else if (intExecMode == 1) 
                {
                    int intSelRow = 0;

                    // 対象の選択行を探す
                    foreach (DataGridViewRow r in dgvUser.Rows)
                    {
                        if (strSelWorkerNo == NulltoString(r.Cells[m_CON_COL_USERNO].Value)) 
                        {
                            intSelRow = r.Index;
                            break;
                        }
                    }

                    // 更新行表示
                    dgvUser.Rows[intSelRow].Selected = true;
                    // 対象行が既に画面内に表示されている時は何もしない
                    if (dgvUser.SelectedRows[0].Displayed)
                    {
                        return;
                    }
                    dgvUser.FirstDisplayedScrollingRowIndex = intSelRow;
                }
                // 表示モードが更新表示の場合
                else if (intExecMode == 2)
                {
                    // 更新行表示
                    dgvUser.Rows[intLastTimeSelLine].Selected = true;

                    // 対象行が既に画面内に表示されている時は何もしない
                    if (dgvUser.SelectedRows[0].Displayed)
                    {
                        return;
                    }
                    dgvUser.FirstDisplayedScrollingRowIndex = intLastTimeSelLine;
                }
            }
        }

        /// <summary>
        /// ユーザ削除処理
        /// </summary>
        /// <param name="NpgsqlCon">接続子</param>
        /// <param name="transaction">トランザクション</param>
        /// <returns></returns>
        private Boolean DelUser(NpgsqlConnection NpgsqlCon
                              , NpgsqlTransaction transaction
                              , string strSelUserNo)
        {
            // SQL文を作成する
            string strUpdateSql = @"UPDATE mst_Worker
                                           SET del_flg = 1
                                         WHERE employee_num = :UserNo";

            // SQLコマンドに各パラメータを設定する
            var command = new NpgsqlCommand(strUpdateSql, NpgsqlCon, transaction);

            command.Parameters.Add(new NpgsqlParameter("UserNo", DbType.String) { Value = strSelUserNo });

            // sqlを実行する
            if (ExecTranSQL(command, transaction) == false)
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}
