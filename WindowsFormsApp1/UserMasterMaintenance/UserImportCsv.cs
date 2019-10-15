using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserMasterMaintenance
{
    public partial class UserImportCsv : Form
    {
        public UserImportCsv()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("指定したCSVファイルを取込みます。\r\nよろしいですか？", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                MessageBox.Show("取込み処理が完了しました");
                this.Close();
            }
        }

        private void CsvFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            // デフォルトのフォルダを指定する
            ofDialog.InitialDirectory = @"C:";

            //ダイアログのタイトルを指定する
            ofDialog.Title = "ダイアログのタイトル";

            //ダイアログを表示する
            if (ofDialog.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(ofDialog.FileName);
            }
            else
            {
                Console.WriteLine("キャンセルされました");
            }

            txtCsvFile.Text = ofDialog.FileName;

            // オブジェクトを破棄する
            ofDialog.Dispose();
        }
    }
}
