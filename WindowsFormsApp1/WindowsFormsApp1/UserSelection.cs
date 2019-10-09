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

namespace WindowsFormsApp1
{
    public partial class UserSelection : Form
    {
        public String parUserNo;
        public String parUserNm;

        public UserSelection()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            dgvData.Rows.Clear();

            dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvData.MultiSelect = false;
            // 新規行を追加させない
            this.dgvData.AllowUserToAddRows = false;
            dgvData.ReadOnly = true;


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
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            parUserNo = dgvData.Rows[e.RowIndex].Cells[0].Value.ToString();
            parUserNm = dgvData.Rows[e.RowIndex].Cells[1].Value.ToString();
            this.Close();
        }

    }
}
