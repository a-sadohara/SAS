using System;
using System.Data;
using System.Windows.Forms;
using static ProductMstMaintenance.Common;

namespace ProductMstMaintenance
{
    public partial class SelectErrorReason : Form
    {

        #region 定数・変数
        //ErrorReasonDTO eorrReasonDto;

        private DataTable m_dtData;
        #endregion

        #region イベント
        /// <summary>
        /// 初期表示
        /// </summary>
        public SelectErrorReason()
        {
            InitializeComponent();

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;
        }

        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectErrorReason_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            // 行選択モードに変更
            this.dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            // 新規行を追加させない
            this.dgvData.AllowUserToAddRows = false;
            // 読み取り専用
            this.dgvData.ReadOnly = true;
            this.dgvData.MultiSelect = false;

            dgvData.Rows.Clear();

            // 初期表示呼び出し
            if (dispDataGridView() == false)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 閉じるボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, EventArgs e)
        {

            this.Close();
        }

        /// <summary>
        /// 不要なイベントっぽい
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {

            dgvData.Rows[e.RowIndex].Cells[0].Value = true;
            dgvData.Rows[e.RowIndex].Cells[0].ReadOnly = true;

            for (int rowIndex = 0; rowIndex < dgvData.Rows.Count; rowIndex++)
            {
                if (rowIndex != e.RowIndex)
                {
                    dgvData.Rows[rowIndex].Cells[0].Value = false;
                    dgvData.Rows[rowIndex].Cells[0].ReadOnly = false;
                }
            }

        }
        #endregion

        #region メソッド
        /// <summary>
        /// データグリッドビュー表示処理
        /// </summary>
        private Boolean dispDataGridView()
        {

            dgvData.Rows.Clear();

            try
            {
                // 条件が指定されていない場合は抽出しない
                // SQL抽出
                m_dtData = new DataTable();
                string strSQL = g_CON_SELECT_INFO_DECISION_REASON_SER;
                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL);

                // データグリッドビューに反映
                foreach (DataRow row in m_dtData.Rows)
                {
                    this.dgvData.Rows.Add(row.ItemArray);
                }
            }
            catch (Exception ex)
            {
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);
                MessageBox.Show(g_clsMessageInfo.strMsgE0030, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            dgvData.CurrentCell = null;

            // 明細にデータが存在してる場合、１行目を選択状態にする
            if (dgvData.Rows.Count > 0)
            {
                // 0行目表示
                dgvData.Rows[0].Selected = true;
                dgvData.FirstDisplayedScrollingRowIndex = 0;
            }

            return true;
        }
        #endregion
    }
}
