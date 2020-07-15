using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using static BeforeInspection.Common;

namespace BeforeInspection
{
    public partial class WorkerSelection : Form
    {
        // 社員番号
        public string strEmployeeNum { get; set; }
        // 作業者名
        public string strWorkerName { get; set; }

        // 抽出データ
        private DataTable m_dtData;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WorkerSelection()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;
        }

        /// <summary>
        /// データグリッドビュー表示
        /// </summary>
        /// <param name="strKanaSta">カナ（開始）</param>
        /// <param name="strKanaEnd">カナ（終了）</param>
        private void dispDataGridView(string strKanaSta = "", string strKanaEnd = "")
        {
            string strSQL = string.Empty;

            dgvWorker.Rows.Clear();

            try
            {
                // SQL抽出
                m_dtData = new DataTable();
                strSQL = @"SELECT employee_num
                                , worker_name_sei || worker_name_mei AS  worker_name
                             FROM mst_worker 
                            WHERE del_flg = 0 ";

                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                if (strKanaSta == "！" && strKanaEnd == "！")
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
                    if (!string.IsNullOrEmpty(strKanaSta))
                    {
                        strSQL += "AND SUBSTRING(worker_name_sei_kana,1,1) >= :worker_name_sei_kana_sta ";
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "worker_name_sei_kana_sta", DbType = DbType.String, Value = strKanaSta });
                    }
                    if (!string.IsNullOrEmpty(strKanaEnd))
                    {
                        strSQL += "AND SUBSTRING(worker_name_sei_kana,1,1) <= :worker_name_sei_kana_end ";
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "worker_name_sei_kana_end", DbType = DbType.String, Value = strKanaEnd });
                    }
                }
                strSQL += "ORDER BY employee_num ASC ";

                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);

                // データグリッドに反映
                foreach (DataRow row in m_dtData.Rows)
                {
                    this.dgvWorker.Rows.Add(row.ItemArray);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserSelection_Load(object sender, EventArgs e)
        {
            bool bolProcOkNg = false;

            try
            {
                dispDataGridView();

                bolProcOkNg = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                System.Windows.Forms.MessageBox.Show(g_clsMessageInfo.strMsgE0003, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// 検索ラベルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblWorkerNameKana_Click(object sender, EventArgs e)
        {
            Label lblSearch = (Label)sender;

            string strKanaSta = string.Empty;
            string strKanaEnd = string.Empty;

            if (lblSearch == lblWorkerNameKanaあ) { strKanaSta = "ア"; strKanaEnd = "オ"; }
            else if (lblSearch == lblWorkerNameKanaか) { strKanaSta = "カ"; strKanaEnd = "コ"; }
            else if (lblSearch == lblWorkerNameKanaさ) { strKanaSta = "サ"; strKanaEnd = "ソ"; }
            else if (lblSearch == lblWorkerNameKanaた) { strKanaSta = "タ"; strKanaEnd = "ト"; }
            else if (lblSearch == lblWorkerNameKanaな) { strKanaSta = "ナ"; strKanaEnd = "ノ"; }
            else if (lblSearch == lblWorkerNameKanaは) { strKanaSta = "ハ"; strKanaEnd = "ホ"; }
            else if (lblSearch == lblWorkerNameKanaま) { strKanaSta = "マ"; strKanaEnd = "モ"; }
            else if (lblSearch == lblWorkerNameKanaや) { strKanaSta = "ヤ"; strKanaEnd = "ヨ"; }
            else if (lblSearch == lblWorkerNameKanaら) { strKanaSta = "ラ"; strKanaEnd = "ロ"; }
            else if (lblSearch == lblWorkerNameKanaわ) { strKanaSta = "ワ"; strKanaEnd = "ン"; }
            else if (lblSearch == lblWorkerNameKanaOther) { strKanaSta = "！"; strKanaEnd = "！"; }
            else if (lblSearch == lblWorkerNameKanaNoCondition) { strKanaSta = ""; strKanaEnd = ""; }

            foreach (Label lbl in new Label[] { lblWorkerNameKanaあ, lblWorkerNameKanaか, lblWorkerNameKanaさ, lblWorkerNameKanaた, lblWorkerNameKanaな,
                                                lblWorkerNameKanaは, lblWorkerNameKanaま, lblWorkerNameKanaや, lblWorkerNameKanaら, lblWorkerNameKanaわ,
                                                lblWorkerNameKanaOther, lblWorkerNameKanaNoCondition})
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

            try
            {
                dispDataGridView(strKanaSta, strKanaEnd);
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                System.Windows.Forms.MessageBox.Show(g_clsMessageInfo.strMsgE0003, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 作業者一覧クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvWorker_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            // 選択行の情報をパラメータにセット
            strEmployeeNum = dgvWorker.Rows[e.RowIndex].Cells[0].Value.ToString();
            strWorkerName = dgvWorker.Rows[e.RowIndex].Cells[1].Value.ToString();

            this.Close();
        }
        #endregion
    }
}