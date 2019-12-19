using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;
using static BeforeInspection.Common;

namespace BeforeInspection
{
    public partial class UserSelection : Form
    {
        // 社員番号
        public string strUserNo { get; set; }
        // 作業者名
        public string strUserNm { get; set; }

        public DataTable dtData;
        //public DataGridWpf_UserCtrl usrctlDataGridWpf;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UserSelection()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;

            dispDataGridView();
        }

        /// <summary>
        /// データグリッドビュー表示
        /// </summary>
        /// <param name="strKanaSta">カナ（開始）</param>
        /// <param name="strKanaEnd">カナ（終了）</param>
        private void dispDataGridView(string strKanaSta = "", string strKanaEnd = "")
        {
            string strSQL = "";

            dgvUser.Rows.Clear();

            try
            {
                DbOpen();

                // SQL抽出
                NpgsqlCommand NpgsqlCom = null;
                NpgsqlDataAdapter NpgsqlDtAd = null;
                dtData = new DataTable();
                strSQL += @"SELECT 
                                        employee_num
                                      , worker_name_sei || worker_name_mei as worker_name
                                    FROM 
                                        mst_Worker ";
                strSQL += "WHERE del_flg = 0 ";
                if (strKanaSta == "！" && strKanaEnd == "！")
                {
                    strSQL += "AND NOT( ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ア' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'オ') ";
                    strSQL += "    OR ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'カ' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'コ') ";
                    strSQL += "    OR ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'サ' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'ソ') ";
                    strSQL += "    OR ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'タ' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'ト') ";
                    strSQL += "    OR ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ナ' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'ノ') ";
                    strSQL += "    OR ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ハ' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'ホ') ";
                    strSQL += "    OR ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'マ' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'モ') ";
                    strSQL += "    OR ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ヤ' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'ヨ') ";
                    strSQL += "    OR ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ラ' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'ロ') ";
                    strSQL += "    OR ";
                    strSQL += "    ( SUBSTRING(worker_name_sei_kana,1,1) >= 'ワ' AND ";
                    strSQL += "      SUBSTRING(worker_name_sei_kana,1,1) <= 'ン') ";
                    strSQL += ") ";
                }
                else
                {
                    if (!string.IsNullOrEmpty(strKanaSta))
                    {
                        strSQL += "AND SUBSTRING(worker_name_sei_kana,1,1) >= '" + strKanaSta + "' ";
                    }
                    if (!string.IsNullOrEmpty(strKanaEnd))
                    {
                        strSQL += "AND SUBSTRING(worker_name_sei_kana,1,1) <= '" + strKanaEnd + "' ";
                    }
                }
                strSQL += "ORDER BY employee_num ASC ;";
                NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                NpgsqlDtAd.Fill(dtData);

                // データグリッドに反映
                foreach (DataRow row in dtData.Rows)
                {
                    this.dgvUser.Rows.Add(row.ItemArray);
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("作業者マスタの取得で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
        }
        #endregion

        #region イベント
        private void lblSearch_Click(object sender, EventArgs e)
        {
            Label lblSearch = (Label)sender;

            string strKanaSta = "";
            string strKanaEnd = "";

            if (lblSearch == lblSearchあ) { strKanaSta = "ア"; strKanaEnd = "オ"; }
            else if (lblSearch == lblSearchか) { strKanaSta = "カ"; strKanaEnd = "コ"; }
            else if (lblSearch == lblSearchさ) { strKanaSta = "サ"; strKanaEnd = "ソ"; }
            else if (lblSearch == lblSearchた) { strKanaSta = "タ"; strKanaEnd = "ト"; }
            else if (lblSearch == lblSearchな) { strKanaSta = "ナ"; strKanaEnd = "ノ"; }
            else if (lblSearch == lblSearchは) { strKanaSta = "ハ"; strKanaEnd = "ホ"; }
            else if (lblSearch == lblSearchま) { strKanaSta = "マ"; strKanaEnd = "モ"; }
            else if (lblSearch == lblSearchや) { strKanaSta = "ヤ"; strKanaEnd = "ヨ"; }
            else if (lblSearch == lblSearchら) { strKanaSta = "ラ"; strKanaEnd = "ロ"; }
            else if (lblSearch == lblSearchわ) { strKanaSta = "ワ"; strKanaEnd = "ン"; }
            else if (lblSearch == lblSearchEtc) { strKanaSta = "！"; strKanaEnd = "！"; }
            else if (lblSearch == lblSearchNon) { strKanaSta = ""; strKanaEnd = ""; }

            foreach (Label lbl in new Label[] { lblSearchあ, lblSearchか, lblSearchさ, lblSearchた, lblSearchな,
                                                lblSearchは, lblSearchま, lblSearchや, lblSearchら, lblSearchわ,
                                                lblSearchEtc, lblSearchNon})
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

            dispDataGridView(strKanaSta, strKanaEnd);
        }

        private void dgvUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) { return; }

            // 選択行の情報をパラメータにセット
            strUserNo = dgvUser.Rows[e.RowIndex].Cells[0].Value.ToString();
            strUserNm = dgvUser.Rows[e.RowIndex].Cells[1].Value.ToString();

            this.Close();
        }
        #endregion
    }
}
