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
    public partial class Result2 : Form
    {

        TargetInfoDto objTargetInfoDto;
        int intRow;

        public Result2(TargetInfoDto objTargetInfo,int intRowIndex)
        {
            InitializeComponent();

            objTargetInfoDto = objTargetInfo;
            intRow = intRowIndex;

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

        }

        private void Result_Load(object sender, EventArgs e)
        {

            this.WindowState = FormWindowState.Maximized;


            dataGridView1.Rows.Clear();

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            // 新規行を追加させない
            this.dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;


            foreach (string line in File.ReadLines("判定登録.tsv", Encoding.Default))
            {

                // 改行コードを変換
                string strLine = line.Replace("\\rn", Environment.NewLine);

                string[] csv = strLine.Split('\t');
                string[] data = new string[csv.Length];
                Array.Copy(csv, 0, data, 0, data.Length);
                this.dataGridView1.Rows.Add(data);

            }

        }

        private void Button1_Click(object sender, EventArgs e)
        {

            DataTable dtTargetInfo = objTargetInfoDto.getTargetInfoDTO();
            dtTargetInfo.Rows[intRow]["Status"] = "3";

            objTargetInfoDto.setTargetInfoDTO(dtTargetInfo);

            this.Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            DataTable dtTargetInfo = objTargetInfoDto.getTargetInfoDTO();
            dtTargetInfo.Rows[intRow]["Status"] = "4";
            dtTargetInfo.Rows[intRow]["Process"] = "検査結果";

            objTargetInfoDto.setTargetInfoDTO(dtTargetInfo);

            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ViewEnlargedimage frmViewEnlargedimage = new ViewEnlargedimage(System.Drawing.Image.FromFile(".\\Image\\05_F1_B0019.jpg"));
            frmViewEnlargedimage.ShowDialog(this);
        }
    }
}
