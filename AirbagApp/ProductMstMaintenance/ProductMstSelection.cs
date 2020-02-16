using System;
using System.Data;
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


        #endregion

        #region イベント
        /// <summary>
        /// 画面初期表示
        /// </summary>
        public ProductMstSelection()
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

            strHinNm = "";

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
            strHinNm = "";
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
        /// データグリッドビュー行エンター
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {

            dgvData.Rows[e.RowIndex].Cells[m_CON_COL_SELECT_BOX].Value = true;
            dgvData.Rows[e.RowIndex].Cells[m_CON_COL_SELECT_BOX].ReadOnly = true;

            for (int rowIndex = 0; rowIndex < dgvData.Rows.Count; rowIndex++)
            {
                if (rowIndex != e.RowIndex)
                {
                    dgvData.Rows[rowIndex].Cells[m_CON_COL_SELECT_BOX].Value = false;
                    dgvData.Rows[rowIndex].Cells[m_CON_COL_SELECT_BOX].ReadOnly = false;
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
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);
                MessageBox.Show(g_clsMessageInfo.strMsgE0021, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
