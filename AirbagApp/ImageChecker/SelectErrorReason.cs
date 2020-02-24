using System;
using System.Data;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class SelectErrorReason : Form
    {
        // 内容
        public string strDecisionReason { get; set; }

        // パラメータ
        private bool m_bolMainReason = false;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectErrorReason(bool bolMainReason)
        {
            m_bolMainReason = bolMainReason;

            strDecisionReason = string.Empty;

            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectErrorReason_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            bool bolProcOkNg = false;

            string strSQL = string.Empty;
            DataTable dtData = new DataTable();

            dgvMstDecisionReason.Rows.Clear();

            try
            {
                // SQL抽出
                dtData = new DataTable();
                strSQL += @"SELECT  ";
                strSQL += @"    0 AS cdk_select, ";
                strSQL += @"    decision_reason ";
                strSQL += @"FROM mst_decision_reason ";

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL);

                // データグリッドビューに反映
                // 主要
                if (m_bolMainReason == true)
                {
                    this.dgvMstDecisionReason.Rows.Add(new object[] { 0, g_CON_NG_REASON_OK });
                    this.dgvMstDecisionReason.Rows.Add(new object[] { 0, g_CON_NG_REASON_WHITE_THREAD_ONE });
                    this.dgvMstDecisionReason.Rows.Add(new object[] { 0, g_CON_NG_REASON_WHITE_THREAD_MULTI });
                    this.dgvMstDecisionReason.Rows.Add(new object[] { 0, g_CON_NG_REASON_BLACK_THREAD_ONE });
                    this.dgvMstDecisionReason.Rows.Add(new object[] { 0, g_CON_NG_REASON_BLACK_THREAD_MULTI });
                    this.dgvMstDecisionReason.Rows.Add(new object[] { 0, g_CON_NG_REASON_OTHER_NG_JUDGEMENT });
                }

                // その他
                foreach (DataRow dr in dtData.Rows)
                {
                    // 主要が含まれていたら除く
                    if (m_bolMainReason == true)
                    {
                        switch (dr["decision_reason"].ToString())
                        {
                            case g_CON_NG_REASON_OK:
                            case g_CON_NG_REASON_WHITE_THREAD_ONE:
                            case g_CON_NG_REASON_WHITE_THREAD_MULTI:
                            case g_CON_NG_REASON_BLACK_THREAD_ONE:
                            case g_CON_NG_REASON_BLACK_THREAD_MULTI:
                            case g_CON_NG_REASON_OTHER_NG_JUDGEMENT:

                                continue;
                        }
                    }

                    this.dgvMstDecisionReason.Rows.Add(dr.ItemArray);
                }

                bolProcOkNg = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR,string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001 ,Environment.NewLine, ex.Message));
                // メッセージ出力
                System.Windows.Forms.MessageBox.Show(g_clsMessageInfo.strMsgE0030, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            finally
            {
                if (bolProcOkNg == false)
                    this.Close();

                this.ResumeLayout();
            }
        }

        /// <summary>
        /// フォーム表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectErrorReason_Shown(object sender, EventArgs e)
        {
            // 初期表示時、一覧にフォーカスセット
            dgvMstDecisionReason.Focus();
        }

        /// <summary>
        /// OKボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            // 選択行の品番をプロパティにセットする
            foreach (DataGridViewRow row in dgvMstDecisionReason.Rows)
            {
                if (row.Cells[0].Value.Equals(true))
                {
                    strDecisionReason = row.Cells[1].Value.ToString();
                    this.Close();
                }
            }
        }

        /// <summary>
        /// 行選択イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvMstDecisionReason_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            // 選択行にチェックを入れる
            dgvMstDecisionReason.Rows[e.RowIndex].Cells[0].Value = true;

            // 選択行以外のチェックを外す
            for (int rowIndex = 0; rowIndex < dgvMstDecisionReason.Rows.Count; rowIndex++)
            {
                if (rowIndex != e.RowIndex)
                {
                    dgvMstDecisionReason.Rows[rowIndex].Cells[0].Value = false;
                }
            }
        }
        #endregion
    }
}
