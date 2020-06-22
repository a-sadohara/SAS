using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using static ProductMstMaintenance.Common;

namespace ProductMstMaintenance
{
    public partial class ProductMstSelection : Form
    {

        #region 定数・変数
        // 戻り値用変数
        public string strHinNm;

        private const Int32 m_CON_COL_SELECT_BOX = 0;
        private const Int32 m_CON_COL_PRODUCT_NAME = 1;

        private DataTable m_dtData;
        private string m_strProductName = string.Empty;

        #endregion

        #region イベント
        /// <summary>
        /// 画面初期表示
        /// </summary>
        /// <param name="strProductName">品名</param>
        public ProductMstSelection(string strProductName)
        {
            m_strProductName = strProductName;

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
            // 複数選択させない
            this.dgvData.MultiSelect = false;

            dgvData.Rows.Clear();

            strHinNm = string.Empty;

            // 初期表示呼び出し
            if (dispDataGridView() == false)
            {
                this.Close();
            }
        }

        /// <summary>
        /// OKボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            strHinNm = string.Empty;
            // 明細行を全行確認する
            foreach (DataGridViewRow row in dgvData.Rows)
            {
                // 選択行でない場合スルーする
                if (row.Cells[m_CON_COL_SELECT_BOX].Value == null)
                {
                    continue;
                }

                // 選択行の場合値を変数にセットして返却する
                if (row.Cells[m_CON_COL_SELECT_BOX].Value.Equals(true))
                {
                    strHinNm = row.Cells[m_CON_COL_PRODUCT_NAME].Value.ToString();
                    this.Close();
                }
            }
        }

        /// <summary>
        /// データグリッドビューセルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // ヘッダクリックでソートする際、下記処理をスキップする
            if (e.RowIndex == -1)
            {
                return;
            }

            // 選択行のみチェック状態とする
            foreach (DataGridViewRow dgvRow in dgvData.Rows)
            {
                dgvRow.Cells[m_CON_COL_SELECT_BOX].Value = false;
            }

            dgvData.Rows[e.RowIndex].Cells[m_CON_COL_SELECT_BOX].Value = true;
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
                string strSQL = g_CON_SELECT_MST_PRODUCT_INFO_PMS;
                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL);

                // データグリッドビューに反映
                foreach (DataRow row in m_dtData.Rows)
                {
                    this.dgvData.Rows.Add(row.ItemArray);
                }
            }
            catch (Exception ex)
            {
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                MessageBox.Show(g_clsMessageInfo.strMsgE0021, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            dgvData.CurrentCell = null;

            if (dgvData.Rows.Count > 0)
            {
                // 現在設定されている品名に紐付く情報を抽出する
                DataGridViewRow dgvRow =
                    dgvData.Rows.Cast<DataGridViewRow>().Where(
                        x => x.Cells[m_CON_COL_PRODUCT_NAME].Value.Equals(m_strProductName)).FirstOrDefault();

                if (dgvRow != null)
                {
                    // 現在設定されている品名を選択状態にする
                    dgvRow.Selected = true;
                    dgvData.FirstDisplayedScrollingRowIndex = dgvRow.Index;
                    dgvRow.Cells[m_CON_COL_SELECT_BOX].Value = true;
                }
                else
                {
                    // 1行目を選択状態にする
                    dgvData.Rows[0].Selected = true;
                    dgvData.FirstDisplayedScrollingRowIndex = 0;
                    dgvData.Rows[0].Cells[m_CON_COL_SELECT_BOX].Value = true;
                }
            }

            return true;
        }
        #endregion
    }
}