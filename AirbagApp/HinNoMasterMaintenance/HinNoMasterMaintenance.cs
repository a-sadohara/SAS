using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HinNoMasterMaintenance
{
    public partial class HinNoMasterMaintenance : Form
    {
        public HinNoMasterMaintenance()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SelectErrorReason frmSelectErrorReason = new SelectErrorReason();
            frmSelectErrorReason.ShowDialog(this);
        }

        private void HinNoMasterMaintenance_Load(object sender, EventArgs e)
        {
            string strSQL = "";

            //dgvUser.Rows.Clear();

            //usrctlDataGridWpf = new DataGridWpf_UserCtrl(this, elementHost1, DataGridWpf_UserCtrl.COLUM_TYPE.USER);

            //try
            //{
            //    if (bolModeNonDBCon == true)
            //        throw new Exception("DB非接続モードです");

            //    // PostgreSQLへ接続
            //    using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
            //    {
            //        NpgsqlCon.Open();

            //        // SQL抽出
            //        NpgsqlCommand NpgsqlCom = null;
            //        NpgsqlDataAdapter NpgsqlDtAd = null;
            //        dtData = new DataTable();
            //        strSQL += "SELECT * FROM SAGYOSYA ";
            //        if (!string.IsNullOrEmpty(strKanaSta) && !string.IsNullOrEmpty(strKanaEnd))
            //        {
            //            strSQL += "WHERE SUBSTRING(USERYOMIGANA,1,1) BETWEEN '" + strKanaSta + "' AND '" + strKanaEnd + "'";
            //        }

            //        strSQL += "ORDER BY USERNO ASC ";
            //        NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
            //        NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
            //        NpgsqlDtAd.Fill(dtData);

            //        //TODO:
            //        // データグリッドに反映
            //        //usrctlDataGridWpf = new DataGridWpf_UserCtrl(this, elementHost1, DataGridWpf_UserCtrl.COLUM_TYPE.USER, dtData);
            //        //elementHost1.Child = usrctlDataGridWpf;
            //        // データグリッドビューに反映
            //        foreach (DataRow row in dtData.Rows)
            //        {
            //            this.dgvUser.Rows.Add(row.ItemArray);
            //        }
            //    }

            //    // 描画崩れ防止
            //    if (this.dgvUser.Rows.Count == 0)
            //    {
            //        this.dgvUser.Rows.Add(new object[] { null, null });
            //        dgvUser.Rows.RemoveAt(0);
            //    }
            //    elementHost1.Child = usrctlDataGridWpf;

            //}
            //catch (Exception e)
            //{
            //    string strErrMsg = "";
            //    strErrMsg = e.Message;

            // 後々この処理は消す
            //foreach (string line in File.ReadLines("品名.tsv", Encoding.Default))
            //{
            //    // 改行コードを変換
            //    string strLine = line.Replace("\\rn", Environment.NewLine);

            //    string[] csv = strLine.Split('\t');
            //    string[] data = new string[csv.Length];
            //    Array.Copy(csv, 0, data, 0, data.Length);
            //    this.dgvUser.Rows.Add(data);
            //}
            //}

            //txtHinNoCount.Text = dgvUser.RowCount.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HinNoImportCsv frmUserImportCsv = new HinNoImportCsv();
            frmUserImportCsv.ShowDialog(this);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            HinNoSelection frmHinNoSelection = new HinNoSelection();
            frmHinNoSelection.ShowDialog(this);
        }

        private void textBox5_Click(object sender, EventArgs e)
        {
            HinNoSelection frmHinNoSelection = new HinNoSelection();
            frmHinNoSelection.ShowDialog(this);
        }
    }
}
