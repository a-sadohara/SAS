using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using Npgsql;
using static UserMasterMaintenance.Common;

namespace UserMasterMaintenance
{
    public partial class UserImportCsv : Form
    {
        #region 変数・定数
        // CSVファイル配置情報
        private const int m_CON_COL_USER_INFO_PROCESS_TYPE = 0;
        private const int m_CON_COL_USER_INFO_NO = 1;
        private const int m_CON_COL_USER_INFO_NAME_SEI = 2;
        private const int m_CON_COL_USER_INFO_NAME_MEI = 3;
        private const int m_CON_COL_USER_INFO_KANA_SEI = 4;
        private const int m_CON_COL_USER_INFO_KANA_MEI = 5;

        private const string m_CON_COL_PROCESS_TYPE_REG = "C";
        private const string m_CON_COL_PROCESS_TYPE_UPD = "U";
        private const string m_CON_COL_PROCESS_TYPE_DEL = "D";

        private const string m_CON_OUTLOGFILE_NAME = "UserImportLog";

        private static string m_strInputFileName = "";

        /// <summary>
        /// 作業者登録CSVファイル
        /// </summary>
        public class UserCsvInfo
        {
            public string strProcessType { get; set; }
            public string strUserID { get; set; }
            public string strUserNameSei { get; set; }
            public string strUserNameMei { get; set; }
            public string strUserKanaSei { get; set; }
            public string strUserKanaMei { get; set; }
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
            //List<UserCsvInfo> lstUserData = new List<UserCsvInfo>();
            UserCsvInfo uciUserData = new UserCsvInfo();
            int intReadData = 0;

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
                string strFileName = txtCsvFile.Text;
                if (System.IO.File.Exists(strFileName) == false)
                {
                    MessageBox.Show("指定されたcsvファイルが存在しません。"
                                   + Environment.NewLine
                                   + strFileName);
                    return;
                }

                m_strInputFileName = strFileName;

                // 選択ファイル読み込み
                using (StreamReader sr = new StreamReader(strFileName, Encoding.GetEncoding("Shift_JIS")))
                {
                    int intRowCount = 0;

                    // PostgreSQLへ接続
                    using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_CON_DB_INFO))
                    {
                        NpgsqlCon.Open();

                        using (var transaction = NpgsqlCon.BeginTransaction())
                        {
                            // ストリームの末尾まで繰り返す
                            while (!sr.EndOfStream)
                            {
                                intRowCount = intRowCount + 1;

                                // マーカCSVファイルを１行読み込む
                                string strFileTextLine = sr.ReadLine();
                                if (strFileTextLine == "" || intRowCount == 1)
                                {
                                    // ヘッダ行(1行目)または空行（最終行）の場合読み飛ばす
                                    continue;
                                }

                                // CSVファイル読み込み＆入力データチェックを行う
                                if (ReadCsvData(strFileTextLine, intRowCount, out uciUserData) == false)
                                {
                                    continue;
                                }

                                // 登録処理実施
                                if (RegistrationUser(uciUserData, NpgsqlCon, transaction) == true)
                                {
                                    intReadData = intReadData + 1;
                                }
                                else
                                {
                                    return;
                                }
                            }

                            // トランザクションコミット
                            transaction.Commit();
                        }
                    }

                    if (intRowCount == 1)
                    {
                        MessageBox.Show("CSVのデータが0件です");
                        // ログファイルにエラー出力を行う
                        OutPutImportLog("取り込み件数0件");
                        this.DialogResult = System.Windows.Forms.DialogResult.OK;
                        this.Close();
                        return;
                    }
                }

                MessageBox.Show("取込み処理が完了しました");
                OutPutImportLog("取込み処理が完了しました。取り込み件数：" + intReadData + "件");
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }
        #endregion

        #region メソッド
        /// <summary>
        /// CSVファイル読み込み
        /// </summary>
        /// <param name="strFileTextLine">CSVファイル行テキスト</param>
        /// <param name="lstUserData">CSVファイルデータ</param>
        /// <returns></returns>
        private Boolean ReadCsvData(string strFileTextLine
                                  , int intRowCount
                                  , out UserCsvInfo uciUserData)
        {
            uciUserData = new UserCsvInfo();

            // CSVを読み込む
            if (SetUserInfoCsv(strFileTextLine, out uciUserData) == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目の列数が不足しています");
                return false;
            }

            // 入力データチェックを行う
            if (InputDataCheck(uciUserData, intRowCount) == false)
            {
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
                                             , out UserCsvInfo uciData)
        {
            string[] stArrayData;

            uciData = new UserCsvInfo();

            // 半角スペース区切りで分割して配列に格納する
            stArrayData = strFileReadLine.Split(',');

            // 列数チェック
            if (stArrayData.Length <= m_CON_COL_USER_INFO_KANA_MEI)
            {
                return false;
            }

            // CSVの各項目を構造体へ格納する
            uciData.strProcessType = stArrayData[m_CON_COL_USER_INFO_PROCESS_TYPE];
            uciData.strUserID = stArrayData[m_CON_COL_USER_INFO_NO];
            uciData.strUserNameSei = stArrayData[m_CON_COL_USER_INFO_NAME_SEI];
            uciData.strUserNameMei = stArrayData[m_CON_COL_USER_INFO_NAME_MEI];
            uciData.strUserKanaSei = stArrayData[m_CON_COL_USER_INFO_KANA_SEI];
            uciData.strUserKanaMei = stArrayData[m_CON_COL_USER_INFO_KANA_MEI];

            return true;
        }

        /// <summary>
        /// /入力チェック
        /// </summary>
        /// <param name="uciCheckData">読み込みユーザ情報リスト</param>
        /// <param name="intRowCount">対象行番号</param>
        /// <returns></returns>
        private Boolean InputDataCheck(UserCsvInfo uciCheckData
                                     , int intRowCount)
        {
            Int32 intUserNo = 0;

            // 必須入力チェック
            if (CheckRequiredInput(uciCheckData.strProcessType, "処理区分", intRowCount, 1) == false ||
                CheckRequiredInput(uciCheckData.strUserID, "社員番号", intRowCount, 4) == false ||
                CheckRequiredInput(uciCheckData.strUserNameSei, "作業者名 性", intRowCount, 10) == false ||
                CheckRequiredInput(uciCheckData.strUserNameMei, "作業者名 名", intRowCount, 10) == false ||
                CheckRequiredInput(uciCheckData.strUserKanaSei, "読み仮名 性", intRowCount, 10) == false ||
                CheckRequiredInput(uciCheckData.strUserKanaMei, "読み仮名 名", intRowCount, 10) == false)
            {
                return false;
            }

            // 文字入力チェック
            if (uciCheckData.strProcessType != m_CON_COL_PROCESS_TYPE_REG &&
                uciCheckData.strProcessType != m_CON_COL_PROCESS_TYPE_UPD &&
                uciCheckData.strProcessType != m_CON_COL_PROCESS_TYPE_DEL)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目の処理区分は「C,U,D」のいずれかを指定してください。");
                return false;
            }

            // 数値入力チェック
            if (Int32.TryParse(uciCheckData.strUserID, out intUserNo) == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目の社員番号は数字のみ入力してください。");
                return false;
            }

            // カナ入力チェック
            if (Regex.IsMatch(uciCheckData.strUserKanaSei, "^[ァ-ヶー]*$") == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目の読み仮名 性は全角カナのみ入力してください。");
                return false;
            }
            if (Regex.IsMatch(uciCheckData.strUserKanaMei, "^[ァ-ヶー]*$") == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目の読み仮名 名は全角カナのみ入力してください。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 必須入力チェック
        /// </summary>
        /// <param name="strCheckData">チェック対象テキスト</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <param name="intRowCount">チェック対象行番号</param>
        /// <param name="intMaxLength">項目最大長</param>
        /// <returns></returns>
        private Boolean CheckRequiredInput(String strCheckData
                                         , String strItemName
                                         , Int32 intRowCount
                                         , Int32 intMaxLength)
        {
            // 必須入力チェック
            if (strCheckData == "")
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目の" + strItemName + "は必須入力の項目です。");
                return false;
            }
            else if (strCheckData.Length > intMaxLength)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目の" + strItemName + "が最大長を超えています。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private Boolean RegistrationUser(UserCsvInfo uciCheckData
                                       , NpgsqlConnection NpgsqlCon
                                       , NpgsqlTransaction transaction)
        {
            try
            {
                if (g_bolModeNonDBCon == true)
                    return true;

                // 登録・更新
                if (uciCheckData.strProcessType == m_CON_COL_PROCESS_TYPE_REG ||
                    uciCheckData.strProcessType == m_CON_COL_PROCESS_TYPE_UPD)
                {
                    // SQL文を作成する
                    string strCreateSql = @"INSERT INTO mst_Worker (WorkerNo, WorkerSurname, WorkerName, WorkerSurnameKana, WorkerNameKana, Delflg)
                                                                    VALUES (:UserNo, :UserSurname, :UserName, :UserSurnameKana, :UserNameKana, 0)
                                                               ON CONFLICT (WorkerNo)
                                                             DO UPDATE SET WorkerSurname = :UserSurname
                                                                         , WorkerName = :UserName
                                                                         , WorkerSurnameKana = :UserSurnameKana
                                                                         , WorkerNameKana = :UserNameKana";

                    // SQLコマンドに各パラメータを設定する
                    var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                    command.Parameters.Add(new NpgsqlParameter("UserNo", DbType.String) { Value = String.Format("{0:D4}", Int32.Parse(uciCheckData.strUserID)) });
                    command.Parameters.Add(new NpgsqlParameter("UserSurname", DbType.String) { Value = uciCheckData.strUserNameSei });
                    command.Parameters.Add(new NpgsqlParameter("UserName", DbType.String) { Value = uciCheckData.strUserNameMei });
                    command.Parameters.Add(new NpgsqlParameter("UserSurnameKana", DbType.String) { Value = uciCheckData.strUserKanaSei });
                    command.Parameters.Add(new NpgsqlParameter("UserNameKana", DbType.String) { Value = uciCheckData.strUserKanaMei });

                    // sqlを実行する
                    if (ExecTranSQL(command, transaction) == false)
                    {
                        return false;
                    }
                }
                // 削除
                else if (uciCheckData.strProcessType == m_CON_COL_PROCESS_TYPE_DEL)
                {
                    string strSelUserNo = String.Format("{0:D4}", Int32.Parse(uciCheckData.strUserID));

                    // ユーザ削除
                    if (DelUser(NpgsqlCon, transaction, strSelUserNo) == false)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("登録時にエラーが発生しました。"
                               + Environment.NewLine
                               + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// ユーザ削除処理
        /// </summary>
        /// <param name="NpgsqlCon">接続子</param>
        /// <param name="transaction">トランザクション</param>
        /// <returns></returns>
        private Boolean DelUser(NpgsqlConnection NpgsqlCon
                              , NpgsqlTransaction transaction
                              , string strSelUserNo)
        {
            // SQL文を作成する
            string strUpdateSql = @"UPDATE mst_Worker
                                       SET Delflg = 1
                                     WHERE WorkerNo = :UserNo";

            // SQLコマンドに各パラメータを設定する
            var command = new NpgsqlCommand(strUpdateSql, NpgsqlCon, transaction);

            command.Parameters.Add(new NpgsqlParameter("UserNo", DbType.String) { Value = strSelUserNo });

            // sqlを実行する
            if (ExecTranSQL(command, transaction) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// インポートログ出力
        /// </summary>
        private static void OutPutImportLog(string strLogText)
        {
            string strOutPutFilePath = "";
            string time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");

            // 出力ファイル設定
            strOutPutFilePath = System.IO.Directory.GetCurrentDirectory() + @"\"
                                                                          + m_CON_OUTLOGFILE_NAME
                                                                          + "_"
                                                                          + DateTime.Now.ToString("yyyyMMdd")
                                                                          + ".txt";

            try
            {
                //Shift JISで書き込む
                //書き込むファイルが既に存在している場合は、上書きする
                using (StreamWriter sw = new StreamWriter(strOutPutFilePath
                                                        , true
                                                        , Encoding.GetEncoding("shift_jis")))
                {
                    // １行ずつ出力を行う
                    sw.WriteLine(m_strInputFileName + " " + time + ":" + strLogText);
                }
            }
            catch
            {
                return;
            }
        }
        #endregion    
    }
}
