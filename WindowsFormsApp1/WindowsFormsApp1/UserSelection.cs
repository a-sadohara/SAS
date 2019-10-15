using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.IO;
using WindowsFormsApp1.DTO;
using static WindowsFormsApp1.Common;
using Npgsql;

namespace WindowsFormsApp1
{
    public partial class UserSelection : Form
    {
        public String parUserNo;
        public String parUserNm;
        public NpgsqlConnection NpgsqlCon;
        public const string  strConnect = "Server=192.168.2.17;Port=5432;User ID=postgres;Database=postgres;Password=password;Enlist=true";
        public DataTable dtData;

        public UserSelection()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            dispDataGridView();
        }

        private void lblSearchあ_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.SystemColors.Highlight;
            lblSearchか.BackColor = System.Drawing.Color.Transparent;
            lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            lblSearchた.BackColor = System.Drawing.Color.Transparent;
            lblSearchな.BackColor = System.Drawing.Color.Transparent;
            lblSearchは.BackColor = System.Drawing.Color.Transparent;
            lblSearchま.BackColor = System.Drawing.Color.Transparent;
            lblSearchや.BackColor = System.Drawing.Color.Transparent;
            lblSearchら.BackColor = System.Drawing.Color.Transparent;
            lblSearchわ.BackColor = System.Drawing.Color.Transparent;

            dispDataGridView("ア", "オ");
        }
        private void lblSearchか_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            lblSearchか.BackColor = System.Drawing.SystemColors.Highlight;
            lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            lblSearchた.BackColor = System.Drawing.Color.Transparent;
            lblSearchな.BackColor = System.Drawing.Color.Transparent;
            lblSearchは.BackColor = System.Drawing.Color.Transparent;
            lblSearchま.BackColor = System.Drawing.Color.Transparent;
            lblSearchや.BackColor = System.Drawing.Color.Transparent;
            lblSearchら.BackColor = System.Drawing.Color.Transparent;
            lblSearchわ.BackColor = System.Drawing.Color.Transparent;

            dispDataGridView("カ", "コ");
        }
        private void lblSearchさ_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            lblSearchか.BackColor = System.Drawing.Color.Transparent;
            lblSearchさ.BackColor = System.Drawing.SystemColors.Highlight;
            lblSearchた.BackColor = System.Drawing.Color.Transparent;
            lblSearchな.BackColor = System.Drawing.Color.Transparent;
            lblSearchは.BackColor = System.Drawing.Color.Transparent;
            lblSearchま.BackColor = System.Drawing.Color.Transparent;
            lblSearchや.BackColor = System.Drawing.Color.Transparent;
            lblSearchら.BackColor = System.Drawing.Color.Transparent;
            lblSearchわ.BackColor = System.Drawing.Color.Transparent;

            dispDataGridView("サ", "ソ");
        }
        private void lblSearchた_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            lblSearchか.BackColor = System.Drawing.Color.Transparent;
            lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            lblSearchた.BackColor = System.Drawing.SystemColors.Highlight;
            lblSearchな.BackColor = System.Drawing.Color.Transparent;
            lblSearchは.BackColor = System.Drawing.Color.Transparent;
            lblSearchま.BackColor = System.Drawing.Color.Transparent;
            lblSearchや.BackColor = System.Drawing.Color.Transparent;
            lblSearchら.BackColor = System.Drawing.Color.Transparent;
            lblSearchわ.BackColor = System.Drawing.Color.Transparent;

            dispDataGridView("タ", "ト");
        }
        private void lblSearchな_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            lblSearchか.BackColor = System.Drawing.Color.Transparent;
            lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            lblSearchた.BackColor = System.Drawing.Color.Transparent;
            lblSearchな.BackColor = System.Drawing.SystemColors.Highlight;
            lblSearchは.BackColor = System.Drawing.Color.Transparent;
            lblSearchま.BackColor = System.Drawing.Color.Transparent;
            lblSearchや.BackColor = System.Drawing.Color.Transparent;
            lblSearchら.BackColor = System.Drawing.Color.Transparent;
            lblSearchわ.BackColor = System.Drawing.Color.Transparent;

            dispDataGridView("ナ", "ノ");
        }
        private void lblSearchは_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            lblSearchか.BackColor = System.Drawing.Color.Transparent;
            lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            lblSearchた.BackColor = System.Drawing.Color.Transparent;
            lblSearchな.BackColor = System.Drawing.Color.Transparent;
            lblSearchは.BackColor = System.Drawing.SystemColors.Highlight;
            lblSearchま.BackColor = System.Drawing.Color.Transparent;
            lblSearchや.BackColor = System.Drawing.Color.Transparent;
            lblSearchら.BackColor = System.Drawing.Color.Transparent;
            lblSearchわ.BackColor = System.Drawing.Color.Transparent;

            dispDataGridView("ハ", "ホ");
        }
        private void lblSearchま_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            lblSearchか.BackColor = System.Drawing.Color.Transparent;
            lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            lblSearchた.BackColor = System.Drawing.Color.Transparent;
            lblSearchな.BackColor = System.Drawing.Color.Transparent;
            lblSearchは.BackColor = System.Drawing.Color.Transparent;
            lblSearchま.BackColor = System.Drawing.SystemColors.Highlight;
            lblSearchや.BackColor = System.Drawing.Color.Transparent;
            lblSearchら.BackColor = System.Drawing.Color.Transparent;
            lblSearchわ.BackColor = System.Drawing.Color.Transparent;

            dispDataGridView("マ", "モ");
        }
        private void lblSearchや_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            lblSearchか.BackColor = System.Drawing.Color.Transparent;
            lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            lblSearchた.BackColor = System.Drawing.Color.Transparent;
            lblSearchな.BackColor = System.Drawing.Color.Transparent;
            lblSearchは.BackColor = System.Drawing.Color.Transparent;
            lblSearchま.BackColor = System.Drawing.Color.Transparent;
            lblSearchや.BackColor = System.Drawing.SystemColors.Highlight;
            lblSearchら.BackColor = System.Drawing.Color.Transparent;
            lblSearchわ.BackColor = System.Drawing.Color.Transparent;

            dispDataGridView("ヤ", "ヨ");
        }
        private void lblSearchら_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            lblSearchか.BackColor = System.Drawing.Color.Transparent;
            lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            lblSearchた.BackColor = System.Drawing.Color.Transparent;
            lblSearchな.BackColor = System.Drawing.Color.Transparent;
            lblSearchは.BackColor = System.Drawing.Color.Transparent;
            lblSearchま.BackColor = System.Drawing.Color.Transparent;
            lblSearchや.BackColor = System.Drawing.Color.Transparent;
            lblSearchら.BackColor = System.Drawing.SystemColors.Highlight;
            lblSearchわ.BackColor = System.Drawing.Color.Transparent;

            dispDataGridView("ラ", "ロ");
        }
        private void lblSearchわ_Click(object sender, EventArgs e)
        {
            lblSearchあ.BackColor = System.Drawing.Color.Transparent;
            lblSearchか.BackColor = System.Drawing.Color.Transparent;
            lblSearchさ.BackColor = System.Drawing.Color.Transparent;
            lblSearchた.BackColor = System.Drawing.Color.Transparent;
            lblSearchな.BackColor = System.Drawing.Color.Transparent;
            lblSearchは.BackColor = System.Drawing.Color.Transparent;
            lblSearchま.BackColor = System.Drawing.Color.Transparent;
            lblSearchや.BackColor = System.Drawing.Color.Transparent;
            lblSearchら.BackColor = System.Drawing.Color.Transparent;
            lblSearchわ.BackColor = System.Drawing.SystemColors.Highlight;

            dispDataGridView("ワ", "ン");
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            parUserNo = dgvData.Rows[e.RowIndex].Cells[0].Value.ToString();
            parUserNm = dgvData.Rows[e.RowIndex].Cells[1].Value.ToString();
            this.Close();
        }

        private void dispDataGridView(string strKanaSta = "", string strKanaEnd = "")
        {
            string strSQL = "";

            dgvData.Rows.Clear();

            try
            {
                // 条件が指定されていない場合は抽出しない
                if (!string.IsNullOrEmpty(strKanaSta) && !string.IsNullOrEmpty(strKanaEnd))
                {
                    // PostgreSQLへ接続
                    using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(strConnect))
                    {
                        NpgsqlCon.Open();

                        // SQL抽出
                        NpgsqlCommand NpgsqlCom = null;
                        NpgsqlDataAdapter NpgsqlDtAd = null;
                        dtData = new DataTable();
                        strSQL += "SELECT * FROM public.USER ";
                        strSQL += "WHERE SUBSTRING(USERYOMIGANA,1,1) BETWEEN '" + strKanaSta + "' AND '" + strKanaEnd + "'";
                        strSQL += "ORDER BY USERNO ASC ";
                        NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                        NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                        NpgsqlDtAd.Fill(dtData);

                        // データグリッドビューに反映
                        foreach (DataRow row in dtData.Rows)
                        {
                            this.dgvData.Rows.Add(row.ItemArray);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // 後々この処理は消す
                foreach (string line in File.ReadLines("作業者.tsv", Encoding.Default))
                {
                    // 改行コードを変換
                    string strLine = line.Replace("\\rn", Environment.NewLine);

                    string[] csv = strLine.Split('\t');
                    string[] data = new string[csv.Length];
                    Array.Copy(csv, 0, data, 0, data.Length);
                    this.dgvData.Rows.Add(data);
                }
            }
        }
    }
}
