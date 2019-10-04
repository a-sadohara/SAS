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

namespace WindowsFormsApp1
{
    public partial class SelectErrorReason : Form
    {

        ErrorReasonDTO eorrReasonDto;

        public SelectErrorReason(ErrorReasonDTO errorReason)
        {
            InitializeComponent();

            eorrReasonDto = errorReason;

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void SelectErrorReason_Load(object sender, EventArgs e)
        {
            // 行選択モードに変更
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            // 新規行を追加させない
            this.dataGridView1.AllowUserToAddRows = false;
            // 読み取り専用
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.MultiSelect = false;


            dataGridView1.Rows.Clear();


            foreach (string line in File.ReadLines("エラー理由一覧.TSV", Encoding.Default))
            {

                // 改行コードを変換
                string strLine = line.Replace("\\rn", Environment.NewLine);

                string[] csv = strLine.Split('\t');
                string[] data = new string[csv.Length];
                Array.Copy(csv, 0, data, 0, data.Length);
                this.dataGridView1.Rows.Add(data);
                

            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {

            foreach(DataGridViewRow row in dataGridView1.Rows)
            {

                if(row.Cells[0].Value == null)
                {
                    continue;
                }

                if(row.Cells[0].Value.Equals(true))
                {
                    eorrReasonDto.setStrErrorReason(row.Cells[1].Value.ToString());
                    this.Close();
                }

                
            }
        }

        private void DataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {

            dataGridView1.Rows[e.RowIndex].Cells[0].Value = true;
            dataGridView1.Rows[e.RowIndex].Cells[0].ReadOnly = true;

            for (int rowIndex = 0; rowIndex < dataGridView1.Rows.Count; rowIndex++)
            {
                if (rowIndex != e.RowIndex)
                {
                    dataGridView1.Rows[rowIndex].Cells[0].Value = false;
                    dataGridView1.Rows[rowIndex].Cells[0].ReadOnly = false;
                }
            }

        }
    }
}
