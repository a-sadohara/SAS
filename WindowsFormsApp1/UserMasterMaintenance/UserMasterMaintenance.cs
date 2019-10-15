using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static UserMasterMaintenance.Common;
using Npgsql;
using System.IO;

namespace UserMasterMaintenance
{
    public partial class UserMasterMaintenance : Form
    {
        private NpgsqlConnection NpgsqlCon;
        private const string strConnect = "Server=192.168.2.17;Port=5432;User ID=postgres;Database=postgres;Password=password;Enlist=true";
        private DataTable dtData;

        private string strKanaSta = "";
        private string strKanaEnd = "";
        private string strYomiGanaSta = "";
        private string strYomiGanaEnd = "";

        private int intSelRow = 0;

        public UserMasterMaintenance()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            dispDataGridView();
        }

        private void dispDataGridView()
        {
            string strSQL = "";

            foreach (DataGridViewRow r in dgvUser.SelectedRows) {intSelRow = r.Index;}
            
            dgvUser.Rows.Clear();

            try
            {
                // 条件が指定されていない場合は抽出しない
                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(strConnect))
                {
                    NpgsqlCon.Open();

                    // SQL抽出
                    NpgsqlCommand NpgsqlCom = null;
                    NpgsqlDataAdapter NpgsqlDtAd = null;
                    dtData = new DataTable();
                    strSQL += "SELECT * FROM SAGYOSYA ";
                    strSQL += "WHERE 1 = 1 ";
                    if (!string.IsNullOrEmpty(strKanaSta))
                    {
                        strSQL += "AND SUBSTRING(USERYOMIGANA,1,1) >= '" + strKanaSta + "' ";
                    }
                    if (!string.IsNullOrEmpty(strKanaEnd))
                    {
                        strSQL += "AND SUBSTRING(USERYOMIGANA,1,1) <= '" + strKanaEnd + "' ";
                    }
                    if (!string.IsNullOrEmpty(strYomiGanaSta))
                    {
                        strSQL += "AND TO_NUMBER(USERNO) >= " + strYomiGanaSta + " ";
                    }
                    if (!string.IsNullOrEmpty(strYomiGanaEnd))
                    {
                        strSQL += "AND TO_NUMBER(USERNO) <= " + strYomiGanaEnd + " ";
                    }
                    strSQL += "ORDER BY USERNO ASC ;";
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
                    this.dgvUser.Rows.Add(data);
                }
            }

            // 選択行設定
            if (intSelRow <= dgvUser.Rows.Count - 1)
            {
                dgvUser.Rows[intSelRow].Selected = true;
            }
            else if (dgvUser.Rows.Count > 0)
            {
                dgvUser.Rows[dgvUser.Rows.Count - 1].Selected = true;
            }

        }

        private void btnReg_Click(object sender, EventArgs e)
        {
            UserEdit frmUserReg = new UserEdit(CON_EDITMODE_REG);
            frmUserReg.ShowDialog();
            dispDataGridView();
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (dgvUser.SelectedRows.Count == 0)
            {
                MessageBox.Show("削除する行を選択してください");
                return;
            }
            else
            {
                foreach (DataGridViewRow r in dgvUser.SelectedRows)
                {
                    if (MessageBox.Show("選択されている行（データ）を削除しますか？", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        MessageBox.Show("削除しました");

                        dispDataGridView();
                    }
                }  
            }
        }
        private void btnImportCsv_Click(object sender, EventArgs e)
        {
            UserImportCsv frmUserImportCsv = new UserImportCsv();
            frmUserImportCsv.ShowDialog();
        }

        private void llk_Click(dynamic sender, EventArgs e)
        {
            if (sender == llkあ) { strKanaSta = "ア"; strKanaEnd = "オ"; }
            if (sender == llkか) { strKanaSta = "カ"; strKanaEnd = "コ"; }
            if (sender == llkさ) { strKanaSta = "サ"; strKanaEnd = "ソ"; }
            if (sender == llkた) { strKanaSta = "タ"; strKanaEnd = "ト"; }
            if (sender == llkな) { strKanaSta = "ナ"; strKanaEnd = "ノ"; }
            if (sender == llkは) { strKanaSta = "ハ"; strKanaEnd = "ホ"; }
            if (sender == llkま) { strKanaSta = "マ"; strKanaEnd = "モ"; }
            if (sender == llkや) { strKanaSta = "ヤ"; strKanaEnd = "ヨ"; }
            if (sender == llkら) { strKanaSta = "ラ"; strKanaEnd = "ロ"; }
            if (sender == llkわ) { strKanaSta = "ワ"; strKanaEnd = "ン"; }
            if (sender == llkEtc) { strKanaSta = "！"; strKanaEnd = "！"; }
            if (sender == llkNon) { strKanaSta = ""; strKanaEnd = ""; }

            foreach (Label lbl in new Label[] { llkあ, llkか, llkさ, llkた, llkな, 
                                                llkは, llkま, llkや, llkら, llkわ, 
                                                llkEtc, llkNon })
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
        }

        private void dgvUser_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            UserEdit frmUserReg = new UserEdit(CON_EDITMODE_UPD, 
                                               dgvUser.Rows[e.RowIndex].Cells[0].Value.ToString(), 
                                               dgvUser.Rows[e.RowIndex].Cells[1].Value.ToString(),
                                               dgvUser.Rows[e.RowIndex].Cells[2].Value.ToString());
            frmUserReg.ShowDialog();
            dispDataGridView();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            dispDataGridView();
        }
    }
}
