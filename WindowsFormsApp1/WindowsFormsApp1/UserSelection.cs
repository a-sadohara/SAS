using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using WindowsFormsApp1.DTO;
using static WindowsFormsApp1.Common;
using Npgsql;

namespace WindowsFormsApp1
{
    public partial class UserSelection : Form
    {
        public string strUserNo { get; set; }
        public string strUserNm { get; set; }

        public DataTable dtData;

        #region イベント

        private void lblSearch_Click(dynamic sender, EventArgs e)
        {
            string strKanaSta = "";
            string strKanaEnd = "";

            if (sender == lblSearchあ) { strKanaSta = "ア"; strKanaEnd = "オ"; }
            else if (sender == lblSearchか) { strKanaSta = "カ"; strKanaEnd = "コ"; }
            else if (sender == lblSearchさ) { strKanaSta = "サ"; strKanaEnd = "ソ"; }
            else if (sender == lblSearchた) { strKanaSta = "タ"; strKanaEnd = "ト"; }
            else if (sender == lblSearchな) { strKanaSta = "ナ"; strKanaEnd = "ノ"; }
            else if (sender == lblSearchは) { strKanaSta = "ハ"; strKanaEnd = "ホ"; }
            else if (sender == lblSearchま) { strKanaSta = "マ"; strKanaEnd = "モ"; }
            else if (sender == lblSearchや) { strKanaSta = "ヤ"; strKanaEnd = "ヨ"; }
            else if (sender == lblSearchら) { strKanaSta = "ラ"; strKanaEnd = "ロ"; }
            else if (sender == lblSearchわ) { strKanaSta = "ワ"; strKanaEnd = "ン"; }

            foreach (Label lbl in new Label[] { lblSearchあ, lblSearchか, lblSearchさ, lblSearchた, lblSearchな,
                                                lblSearchは, lblSearchま, lblSearchや, lblSearchら, lblSearchわ})
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

        #region メソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UserSelection()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

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
                // 条件が指定されていない場合は抽出しない
                if (!string.IsNullOrEmpty(strKanaSta) && !string.IsNullOrEmpty(strKanaEnd))
                {
                    // PostgreSQLへ接続
                    using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
                    {
                        NpgsqlCon.Open();

                        // SQL抽出
                        NpgsqlCommand NpgsqlCom = null;
                        NpgsqlDataAdapter NpgsqlDtAd = null;
                        dtData = new DataTable();
                        strSQL += "SELECT * FROM SAGYOSYA ";
                        strSQL += "WHERE SUBSTRING(USERYOMIGANA,1,1) BETWEEN '" + strKanaSta + "' AND '" + strKanaEnd + "'";
                        strSQL += "ORDER BY USERNO ASC ";
                        NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                        NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                        NpgsqlDtAd.Fill(dtData);

                        // データグリッドビューに反映
                        foreach (DataRow row in dtData.Rows)
                        {
                            this.dgvUser.Rows.Add(row.ItemArray);
                        }
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
        }

        #endregion
    }
}
