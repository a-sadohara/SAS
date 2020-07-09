using System;
using System.Data;
using System.Drawing;
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
        private int m_intMaxRowCount = 8;
        private int m_intMaxColumnCount = 6;

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

            if (m_bolMainReason)
            {
                m_intMaxColumnCount = 8;
                this.Size = new Size(this.Size.Width + 291, this.Size.Height);
                btnOK.Location = new Point(btnOK.Location.X + 145, btnOK.Location.Y);
            }
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
            bool bolInputDecisionReason = false;
            int intDataRowIndex = 0;
            int intTargetColumnIndex = 0;
            string strDecisionReason = string.Empty;
            string strSQL = string.Empty;
            DataTable dtData = new DataTable();

            this.dgvMstDecisionReason.Rows.Clear();

            // グリッドの行数・列数を指定
            this.dgvMstDecisionReason.ColumnCount = m_intMaxColumnCount;
            this.dgvMstDecisionReason.RowCount = m_intMaxRowCount;

            if (m_bolMainReason)
            {
                // 主要のNG理由内容を設定する
                this.dgvMstDecisionReason.Rows[0].Cells[1].Value = g_CON_ACCEPTANCE_CHECK_RESULT_NG_DETECT;
                this.dgvMstDecisionReason.Rows[1].Cells[1].Value = g_CON_NG_REASON_OK;
                this.dgvMstDecisionReason.Rows[2].Cells[1].Value = g_CON_ACCEPTANCE_CHECK_RESULT_NG_NON_DETECT;
                this.dgvMstDecisionReason.Rows[3].Cells[1].Value = g_clsSystemSettingInfo.strMainNGReason1;
                this.dgvMstDecisionReason.Rows[4].Cells[1].Value = g_clsSystemSettingInfo.strMainNGReason2;
                this.dgvMstDecisionReason.Rows[5].Cells[1].Value = g_clsSystemSettingInfo.strMainNGReason3;
                this.dgvMstDecisionReason.Rows[6].Cells[1].Value = g_clsSystemSettingInfo.strMainNGReason4;
                this.dgvMstDecisionReason.Rows[7].Cells[1].Value = g_CON_NG_REASON_OTHER_NG_JUDGEMENT;
            }

            try
            {
                // SQL抽出
                strSQL += @"SELECT  ";
                strSQL += @"    0 AS cdk_select, ";
                strSQL += @"    decision_reason ";
                strSQL += @"FROM mst_decision_reason ";

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL);

                for (int intColumnIndex = 0; intColumnIndex < m_intMaxColumnCount; intColumnIndex++)
                {
                    // チェックボックス列をスキップ
                    intColumnIndex++;

                    for (int intRowIndex = 0; intRowIndex < m_intMaxRowCount; intRowIndex++)
                    {
                        // 既にNG理由が設定済みのセルはスキップ
                        if (this.dgvMstDecisionReason.Rows[intRowIndex].Cells[intColumnIndex].Value != null)
                        {
                            continue;
                        }

                        bolInputDecisionReason = false;
                        strDecisionReason = string.Empty;

                        // DBから移送が必要なレコードを特定する
                        while (!bolInputDecisionReason && dtData.Rows.Count > intDataRowIndex)
                        {
                            strDecisionReason = dtData.Rows[intDataRowIndex]["decision_reason"].ToString();
                            bolInputDecisionReason = CheckNGReason(strDecisionReason);
                            intDataRowIndex++;
                        }

                        if (!string.IsNullOrWhiteSpace(strDecisionReason))
                        {
                            this.dgvMstDecisionReason.Rows[intRowIndex].Cells[intColumnIndex].Value = strDecisionReason;
                        }
                        else
                        {
                            intTargetColumnIndex = intColumnIndex - 1;

                            // 移送するレコードが存在しない場合、チェックボックスを編集不可とする
                            DataGridViewCell cell = this.dgvMstDecisionReason.Rows[intRowIndex].Cells[intTargetColumnIndex];
                            DataGridViewCheckBoxCell chkCell = cell as DataGridViewCheckBoxCell;
                            chkCell.FlatStyle = FlatStyle.Flat;
                            chkCell.Style.ForeColor = Color.DarkGray;
                            chkCell.ReadOnly = true;
                        }
                    }
                }

                bolProcOkNg = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0030, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            finally
            {
                dtData.Dispose();

                if (bolProcOkNg == false)
                {
                    this.Close();
                }

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
            // 選択行のNG理由をプロパティにセットする
            for (int intColumnIndex = 0; intColumnIndex < m_intMaxColumnCount; intColumnIndex++)
            {
                for (int intRowIndex = 0; intRowIndex < m_intMaxRowCount; intRowIndex++)
                {
                    if (this.dgvMstDecisionReason.Rows[intRowIndex].Cells[intColumnIndex].Value.Equals(true))
                    {
                        strDecisionReason = this.dgvMstDecisionReason.Rows[intRowIndex].Cells[++intColumnIndex].Value.ToString();
                        this.Close();
                        return;
                    }
                }

                // 理由列をスキップ
                intColumnIndex++;
            }
        }

        /// <summary>
        /// セル選択イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvMstDecisionReason_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            int intTargetCheckIndex = e.ColumnIndex;
            int intTargetReasonIndex = e.ColumnIndex;

            if (e.ColumnIndex % 2 == 1)
            {
                // インデックスをチェックボックス列へ補正する
                intTargetCheckIndex--;
            }
            else
            {
                // インデックスを理由列へ補正する
                intTargetReasonIndex++;
            }

            // 理由未設定のセルはスキップする
            if (dgvMstDecisionReason.Rows[e.RowIndex].Cells[intTargetReasonIndex].Value == null)
            {
                return;
            }

            // 全てのチェックを外す
            for (int intColumnIndex = 0; intColumnIndex < m_intMaxColumnCount; intColumnIndex++)
            {
                for (int intRowIndex = 0; intRowIndex < m_intMaxRowCount; intRowIndex++)
                {
                    this.dgvMstDecisionReason.Rows[intRowIndex].Cells[intColumnIndex].Value = false;
                }

                // 理由列をスキップ
                intColumnIndex++;
            }

            // 選択行にチェックを入れる
            dgvMstDecisionReason.Rows[e.RowIndex].Cells[intTargetCheckIndex].Value = true;
        }

        /// <summary>
        /// セル編集イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvMstDecisionReason_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            int intTargetReasonIndex = e.ColumnIndex;

            if (e.ColumnIndex % 2 == 0)
            {
                // インデックスを理由列へ補正する
                intTargetReasonIndex++;
            }

            // 理由未設定のセルはスキップする
            if (dgvMstDecisionReason.Rows[e.RowIndex].Cells[intTargetReasonIndex].Value == null)
            {
                e.Cancel = true;
            }
        }
        #endregion

        #region 内部メソッド
        /// <summary>
        /// NG内容チェック
        /// </summary>
        /// <param name="strNGReason">NG内容</param>
        private bool CheckNGReason(string strNGReason)
        {
            // グリッドに追加するNG内容をチェックする
            if (strNGReason.Equals(g_CON_NG_REASON_OK) ||
                strNGReason.Equals(g_clsSystemSettingInfo.strMainNGReason1) ||
                strNGReason.Equals(g_clsSystemSettingInfo.strMainNGReason2) ||
                strNGReason.Equals(g_clsSystemSettingInfo.strMainNGReason3) ||
                strNGReason.Equals(g_clsSystemSettingInfo.strMainNGReason4) ||
                strNGReason.Equals(g_CON_NG_REASON_OTHER_NG_JUDGEMENT))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}