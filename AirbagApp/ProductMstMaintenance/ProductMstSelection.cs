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

namespace ProductMstMaintenance
{
    public partial class ProductMstSelection : Form
    {

        public string strHinNm;

        public ProductMstSelection()
        {
            InitializeComponent();
            
            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

        }

        private void SelectErrorReason_Load(object sender, EventArgs e)
        {
            // 行選択モードに変更
            this.dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            // 新規行を追加させない
            this.dgvData.AllowUserToAddRows = false;
            // 読み取り専用
            this.dgvData.ReadOnly = true;
            this.dgvData.MultiSelect = false;


            dgvData.Rows.Clear();


            foreach (string line in File.ReadLines("品名.TSV", Encoding.Default))
            {

                // 改行コードを変換
                string strLine = line.Replace("\\rn", Environment.NewLine);

                string[] csv = strLine.Split('\t');
                string[] data = new string[csv.Length];
                Array.Copy(csv, 0, data, 0, data.Length);
                this.dgvData.Rows.Add(data);
                
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {

            foreach(DataGridViewRow row in dgvData.Rows)
            {

                if(row.Cells[0].Value == null)
                {
                    continue;
                }

                if(row.Cells[0].Value.Equals(true))
                {
                    strHinNm = row.Cells[1].Value.ToString();
                    this.Close();
                }

                
            }
        }

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
    }
}
