using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using static BeforeInspection.Common;

namespace BeforeInspection
{
    public partial class HinNoSelection : Form
    {
        // 品名
        public string strProductName { get; set; }

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HinNoSelection()
        {
            string strSQL = "";
            DataTable dtData = new DataTable();

            InitializeComponent();

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            dgvProductName.Rows.Clear();

            // SQL抽出
            DbOpen();

            NpgsqlCommand NpgsqlCom = null;
            NpgsqlDataAdapter NpgsqlDtAd = null;
            dtData = new DataTable();
            strSQL += @"SELECT DISTINCT ";
            strSQL += @"    0 AS cdk_select, ";
            strSQL += @"    product_name ";
            strSQL += @"FROM mst_product_info ";
            strSQL += @"WHERE register_flg = 1 ";
            NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
            NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
            NpgsqlDtAd.Fill(dtData);

            // データグリッドビューに反映
            foreach (DataRow dr in dtData.Rows)
            {
                this.dgvProductName.Rows.Add(dr.ItemArray);
            }
        }
        #endregion

        #region イベント
        /// <summary>
        /// OKボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            // 選択行の品番をプロパティにセットする
            foreach (DataGridViewRow row in dgvProductName.Rows)
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
        private void dgvProductName_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            // 選択行にチェックを入れる
            dgvProductName.Rows[e.RowIndex].Cells[0].Value = true;
            dgvProductName.Rows[e.RowIndex].Cells[0].ReadOnly = true;

            // 選択行以外のチェックを外す
            for (int rowIndex = 0; rowIndex < dgvProductName.Rows.Count; rowIndex++)
            {
                if (rowIndex != e.RowIndex)
                {
                    dgvProductName.Rows[rowIndex].Cells[0].Value = false;
                    dgvProductName.Rows[rowIndex].Cells[0].ReadOnly = false;
                }
            }
        }
        #endregion
    }
}
