using System;
using System.Data;
using System.Windows.Forms;
using static BeforeInspection.Common;

namespace BeforeInspection
{
    public partial class ProductNameSelection : Form
    {
        // 品名
        public string strProductName { get; set; }

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProductNameSelection()
        {
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
        private void ProductNameSelection_Load(object sender, EventArgs e)
        {
            bool bolProcOkNg = false;

            string strSQL = string.Empty;
            DataTable dtData = new DataTable();

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            dgvProductInfo.Rows.Clear();

            try
            {
                // SQL抽出
                strSQL += @"SELECT DISTINCT ";
                strSQL += @"    0 AS cdk_select, ";
                strSQL += @"    product_name ";
                strSQL += @"FROM mst_product_info ";
                strSQL += @"WHERE register_flg = 1 ";
                strSQL += @"ORDER BY product_name ";

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL);

                // データグリッドビューに反映
                foreach (DataRow dr in dtData.Rows)
                {
                    this.dgvProductInfo.Rows.Add(dr.ItemArray);
                }

                bolProcOkNg = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0021, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            finally
            {
                dtData.Dispose();

                if (bolProcOkNg == false)
                {
                    this.Close();
                }
            }
        }

        /// <summary>
        /// OKボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            // 選択行の品番をプロパティにセットする
            foreach (DataGridViewRow row in dgvProductInfo.Rows)
            {
                if (row.Cells[0].Value.Equals(true))
                {
                    strProductName = row.Cells[1].Value.ToString();
                    this.Close();
                }
            }
        }

        /// <summary>
        /// 行選択イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvProductInfo_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            // 選択行にチェックを入れる
            dgvProductInfo.Rows[e.RowIndex].Cells[0].Value = true;
            dgvProductInfo.Rows[e.RowIndex].Cells[0].ReadOnly = true;

            // 選択行以外のチェックを外す
            for (int rowIndex = 0; rowIndex < dgvProductInfo.Rows.Count; rowIndex++)
            {
                if (rowIndex != e.RowIndex)
                {
                    dgvProductInfo.Rows[rowIndex].Cells[0].Value = false;
                    dgvProductInfo.Rows[rowIndex].Cells[0].ReadOnly = false;
                }
            }
        }
        #endregion
    }
}