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

namespace UserMasterMaintenance
{
    public partial class UserImportCsv : Form
    {
        #region 変数・定数
        // CSVファイル配置情報
        public const int COL_USER_INFO_NO = 0;
        public const int COL_USER_INFO_NAME = 1;
        public const int COL_USER_INFO_KANA = 2;

        /// <summary>
        /// 作業者登録CSVファイル
        /// </summary>
        public class UserCsvInfo
        {
            public string strUserID { get; set; }
            public string strUserName { get; set; }
            public string strUserKana { get; set; }
        }
        #endregion

        #region イベント
        /// <summary>
        /// 初期表示
        /// </summary>
        public UserImportCsv()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// CSVファイル選択ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            // デフォルトのフォルダを指定する
            ofDialog.InitialDirectory = @"C:";

            ofDialog.Filter = "csv files (*.csv)|*.csv";
            ofDialog.FilterIndex = 1;
            //ダイアログのタイトルを指定する
            ofDialog.Title = "作業者登録CSV選択ダイアログ";

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

        /// <summary>
        /// CSV取り込みボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImport_Click(object sender, EventArgs e)
        {

            // 読み込みデータ
            List<UserCsvInfo> lstUserData = new List<UserCsvInfo>();

            // 未入力チェック
            if (txtCsvFile.Text == "") 
            {
                MessageBox.Show("CSVファイルを選択してください");
                btnCsvFile.Focus();
                return;
            }

            if (MessageBox.Show("指定したCSVファイルを取込みます。\r\nよろしいですか？"
                              , "確認"
                              , MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // ファイル存在チェック
                string fileName = txtCsvFile.Text;
                if (System.IO.File.Exists(fileName) == false)
                {
                    MessageBox.Show("指定されたcsvファイルが存在しません。" 
                                   + Environment.NewLine 
                                   + fileName);
                    return;
                }

                // CSVファイル読み込み＆入力データチェックを行う
                if (ReadCsvData(fileName, out lstUserData) == false) 
                {
                    return;
                }

                MessageBox.Show("取込み処理が完了しました");
                this.Close();
            }
        }
        #endregion

        #region メソッド
        /// <summary>
        /// CSVファイル読み込み
        /// </summary>
        /// <param name="strFileName">読み込むCSVファイル名</param>
        /// <param name="lstUserData">CSVファイルデータ</param>
        /// <returns></returns>
        private Boolean ReadCsvData(string strFileName
                                  , out List<UserCsvInfo> lstUserData) 
        {
            int intRowCount = 1;

            // 読み込みデータ
            lstUserData = new List<UserCsvInfo>();

            // 選択ファイル読み込み
            using (StreamReader sr = new StreamReader(strFileName, Encoding.GetEncoding("Shift_JIS")))
            {
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    UserCsvInfo uciUserData = new UserCsvInfo();

                    // マーカCSVファイルを１行読み込む
                    string strFileTextLine = sr.ReadLine();
                    if (strFileTextLine == "")
                    {
                        // 空行（最終行）の場合読み飛ばす
                        continue;
                    }

                    // CSVを読み込む
                    if (SetUserInfoCsv(strFileTextLine, out uciUserData) == false) 
                    {
                        MessageBox.Show(intRowCount + "行目の列数が不足しています" 
                                      + Environment.NewLine 
                                      + "データ：" + strFileTextLine);
                        return false;
                    }

                    lstUserData.Add(uciUserData);
                    intRowCount = intRowCount + 1;
                }
            }

            if (lstUserData.Count == 0) 
            {
                MessageBox.Show("CSVのデータが0件です");
                return false;
            }

            return true;
        }

        /// <summary>
        /// ＣＳＶ→構造体格納（ユーザ情報CSV）
        /// </summary>
        /// <param name="strFileReadLine">読み込みＣＳＶ情報</param>
        /// <returns></returns>
        private static Boolean SetUserInfoCsv(string strFileReadLine
                                             ,out UserCsvInfo uciData)
        {
            string[] stArrayData;

            uciData = new UserCsvInfo();

            // 半角スペース区切りで分割して配列に格納する
            stArrayData = strFileReadLine.Split(',');

            // 列数チェック
            if (stArrayData.Length <= COL_USER_INFO_KANA) 
            {
                return false;
            }

            // CSVの各項目を構造体へ格納する
            uciData.strUserID = stArrayData[COL_USER_INFO_NO];
            uciData.strUserName = stArrayData[COL_USER_INFO_NAME];
            uciData.strUserKana = stArrayData[COL_USER_INFO_KANA];

            return true;
        }
        #endregion
    }
}
