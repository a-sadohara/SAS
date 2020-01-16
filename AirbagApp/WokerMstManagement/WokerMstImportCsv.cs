using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static WokerMstManagement.Common;

namespace WokerMstManagement
{
    public partial class WokerMstImportCsv : Form
    {
        #region 変数・定数
        // CSVファイル配置情報
        private const int m_CON_COL_WORKER_INFO_PROCESS_TYPE = 0;
        private const int m_CON_COL_WORKER_INFO_NO = 1;
        private const int m_CON_COL_WORKER_INFO_NAME_SEI = 2;
        private const int m_CON_COL_WORKER_INFO_NAME_MEI = 3;
        private const int m_CON_COL_WORKER_INFO_KANA_SEI = 4;
        private const int m_CON_COL_WORKER_INFO_KANA_MEI = 5;

        private const string m_CON_OUTLOGFILE_NAME = "WorkerImportLog";

        private static string m_strInputFileName = "";

        private static string m_strOutPutFilePath = "";

        /// <summary>
        /// 作業者登録CSVファイル
        /// </summary>
        public class WorkerCsvInfo
        {
            public string strProcessType { get; set; }
            public string strWorkerID { get; set; }
            public string strWorkerNameSei { get; set; }
            public string strWorkerNameMei { get; set; }
            public string strWorkerKanaSei { get; set; }
            public string strWorkerKanaMei { get; set; }
        }
        #endregion

        #region イベント
        /// <summary>
        /// 初期表示
        /// </summary>
        public WokerMstImportCsv()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// CSVファイル選択ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectImportFile_Click(object sender, EventArgs e)
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

            txtImportFilePath.Text = ofDialog.FileName;

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
            WorkerCsvInfo uciWorkerData = new WorkerCsvInfo();
            bool bolTitleRow = true;
            int intRowCount = 0;
            int intImpCount = 0;
            int intImpOkCount = 0;
            int intImpNgCount = 0;
            int intCCount = 0;
            int intCOkCount = 0;
            int intCNgCount = 0;
            int intUCount = 0;
            int intUOkCount = 0;
            int intUNgCount = 0;
            int intDCount = 0;
            int intDOkCount = 0;
            int intDNgCount = 0;

            // 未入力チェック
            if (txtImportFilePath.Text == "")
            {
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0009, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSelectImportFile.Focus();
                return;
            }

            if (MessageBox.Show(g_clsMessageInfo.strMsgQ0004
                              , "確認"
                              , MessageBoxButtons.YesNo
                              , MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string strFileName = txtImportFilePath.Text;
                m_strInputFileName = strFileName;

                // 選択ファイル読み込み
                try
                {
                    using (StreamReader sr = new StreamReader(strFileName, Encoding.GetEncoding("Shift_JIS")))
                    {

                        // ストリームの末尾まで繰り返す
                        while (!sr.EndOfStream)
                        {
                            intRowCount++;

                            // マーカCSVファイルを１行読み込む
                            string strFileTextLine = sr.ReadLine();
                            if (strFileTextLine == "" || bolTitleRow == true)
                            {
                                // ヘッダ行(1行目)または空行（最終行）の場合読み飛ばす
                                bolTitleRow = false;
                                continue;
                            }

                            // 件数
                            intImpCount++;

                            // 件数(登録,更新,削除)
                            if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeCre)
                                intCCount++;
                            else if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeUpd)
                                intUCount++;
                            else if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeDel)
                                intDCount++;

                            // CSVファイル読み込み＆入力データチェックを行う
                            if (ReadCsvData(strFileTextLine, intRowCount, out uciWorkerData) == false)
                            {
                                // NG件数(登録,更新,削除)
                                if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeCre)
                                    intCNgCount++;
                                else if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeUpd)
                                    intUNgCount++;
                                else if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeDel)
                                    intDNgCount++;

                                intImpNgCount++;

                                continue;
                            }

                            // 登録処理実施
                            if (RegistrationWorker(uciWorkerData) == true)
                            {
                                // OK件数(登録,更新,削除)
                                if (uciWorkerData.strProcessType == g_clsSystemSettingInfo.strProcessTypeCre)
                                    intCOkCount++;
                                else if (uciWorkerData.strProcessType == g_clsSystemSettingInfo.strProcessTypeUpd)
                                    intUOkCount++;
                                else if (uciWorkerData.strProcessType == g_clsSystemSettingInfo.strProcessTypeDel)
                                    intUOkCount++;

                                intImpOkCount++;
                            }
                            else
                            {
                                // NG件数(登録,更新,削除)
                                if (uciWorkerData.strProcessType == g_clsSystemSettingInfo.strProcessTypeCre)
                                    intCNgCount++;
                                else if (uciWorkerData.strProcessType == g_clsSystemSettingInfo.strProcessTypeUpd)
                                    intUNgCount++;
                                else if (uciWorkerData.strProcessType == g_clsSystemSettingInfo.strProcessTypeDel)
                                    intDNgCount++;

                                intImpNgCount++;
                            }

                        }

                        // トランザクションコミット
                        g_clsConnectionNpgsql.DbCommit();
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0015 + "\r\n" + ex.Message);

                    OutPutImportLog("\"" + g_clsMessageInfo.strMsgE0015 + "\r\n" + ex.Message + "\"");

                    intImpNgCount++;
                }
                finally
                {
                    g_clsConnectionNpgsql.DbClose();
                }

                if (intImpNgCount > 0 || intCNgCount > 0 || intUNgCount > 0 || intDNgCount > 0)
                {
                    OutPutImportLog("\"" + string.Format(g_clsMessageInfo.strMsgW0001,
                                                         intImpCount, intImpOkCount, intImpNgCount,
                                                         intCCount, intCOkCount, intCNgCount,
                                                         intUCount, intUOkCount, intUNgCount,
                                                         intDCount, intDOkCount, intDNgCount) + "\"");
                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgW0001,
                                                  intImpCount, intImpOkCount, intImpNgCount,
                                                  intCCount, intCOkCount, intCNgCount,
                                                  intUCount, intUOkCount, intUNgCount,
                                                  intDCount, intDOkCount, intDNgCount) + "\n" +
                                    string.Format(g_clsMessageInfo.strMsgI0002, m_strOutPutFilePath),
                                    "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    OutPutImportLog("\"" + string.Format(g_clsMessageInfo.strMsgI0001,
                                                         intImpCount, intImpOkCount, intImpNgCount,
                                                         intCCount, intCOkCount, intCNgCount,
                                                         intUCount, intUOkCount, intUNgCount,
                                                         intDCount, intDOkCount, intDNgCount) + "\"");
                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgI0001,
                                                  intImpCount, intImpOkCount, intImpNgCount,
                                                  intCCount, intCOkCount, intCNgCount,
                                                  intUCount, intUOkCount, intUNgCount,
                                                  intDCount, intDOkCount, intDNgCount) + "\n" +
                                    string.Format(g_clsMessageInfo.strMsgI0002, m_strOutPutFilePath),
                                    "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

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
        /// <param name="lstWorkerData">CSVファイルデータ</param>
        /// <returns></returns>
        private Boolean ReadCsvData(string strFileTextLine
                                  , int intRowCount
                                  , out WorkerCsvInfo uciWorkerData)
        {
            uciWorkerData = new WorkerCsvInfo();
            String strData = string.Join(",",
                             uciWorkerData.strProcessType,
                             uciWorkerData.strWorkerID,
                             uciWorkerData.strWorkerNameSei,
                             uciWorkerData.strWorkerNameMei,
                             uciWorkerData.strWorkerKanaSei,
                             uciWorkerData.strWorkerKanaMei);

            // CSVを読み込む
            if (SetWorkerInfoCsv(strFileTextLine, out uciWorkerData) == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0010, intRowCount) + ",\"" + strFileTextLine + "\"");
                return false;
            }

            // 入力データチェックを行う
            if (InputDataCheck(uciWorkerData, intRowCount) == false)
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
        private static Boolean SetWorkerInfoCsv(string strFileReadLine
                                             , out WorkerCsvInfo uciData)
        {
            string[] stArrayData;

            uciData = new WorkerCsvInfo();

            // 半角スペース区切りで分割して配列に格納する
            stArrayData = strFileReadLine.Split(',');

            // 列数チェック
            if (stArrayData.Length - 1 != m_CON_COL_WORKER_INFO_KANA_MEI)
            {
                return false;
            }

            // CSVの各項目を構造体へ格納する
            uciData.strProcessType = stArrayData[m_CON_COL_WORKER_INFO_PROCESS_TYPE];
            uciData.strWorkerID = stArrayData[m_CON_COL_WORKER_INFO_NO];
            uciData.strWorkerNameSei = stArrayData[m_CON_COL_WORKER_INFO_NAME_SEI];
            uciData.strWorkerNameMei = stArrayData[m_CON_COL_WORKER_INFO_NAME_MEI];
            uciData.strWorkerKanaSei = stArrayData[m_CON_COL_WORKER_INFO_KANA_SEI];
            uciData.strWorkerKanaMei = stArrayData[m_CON_COL_WORKER_INFO_KANA_MEI];

            return true;
        }

        /// <summary>
        /// /入力チェック
        /// </summary>
        /// <param name="uciCheckData">読み込みユーザ情報リスト</param>
        /// <param name="intRowCount">対象行番号</param>
        /// <returns></returns>
        private Boolean InputDataCheck(WorkerCsvInfo uciCheckData
                                     , int intRowCount)
        {
            Int32 intWorkerNo = 0;
            String strData = string.Join(",",
                                         uciCheckData.strProcessType,
                                         uciCheckData.strWorkerID,
                                         uciCheckData.strWorkerNameSei,
                                         uciCheckData.strWorkerNameMei,
                                         uciCheckData.strWorkerKanaSei,
                                         uciCheckData.strWorkerKanaMei);

            // 必須入力チェック
            if (CheckRequiredInput(uciCheckData.strProcessType, "処理区分", intRowCount, strData, 1, 1 , "", "") == false ||
                CheckRequiredInput(uciCheckData.strWorkerID, "社員番号", intRowCount, strData, 4, 4, "0001", "0009") == false ||
                CheckRequiredInput(uciCheckData.strWorkerNameSei, "作業者名 性", intRowCount, strData, 1, 10, "", "") == false ||
                CheckRequiredInput(uciCheckData.strWorkerNameMei, "作業者名 名", intRowCount, strData, 1, 10, "", "") == false ||
                CheckRequiredInput(uciCheckData.strWorkerKanaSei, "読みカナ 性", intRowCount, strData, 1, 10, "", "") == false ||
                CheckRequiredInput(uciCheckData.strWorkerKanaMei, "読みカナ 名", intRowCount, strData, 1, 10, "", "") == false)
            {
                return false;
            }

            // 文字入力チェック
            if (uciCheckData.strProcessType != g_clsSystemSettingInfo.strProcessTypeCre &&
                uciCheckData.strProcessType != g_clsSystemSettingInfo.strProcessTypeUpd &&
                uciCheckData.strProcessType != g_clsSystemSettingInfo.strProcessTypeDel)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0014, "処理区分", "C/U/D") + ",\"" + strData + "\"");
                return false;
            }

            // 数値入力チェック
            if (Int32.TryParse(uciCheckData.strWorkerID, out intWorkerNo) == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0013, "社員番号", "0001", "0009") + ",\"" + strData + "\"");
                return false;
            }

            // カナ入力チェック
            if (Regex.IsMatch(uciCheckData.strWorkerKanaSei, "^[ァ-ヶー]*$") == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0014, "読みカナ 性", "全角カナ") + ",\"" + strData + "\"");
                return false;
            }
            if (Regex.IsMatch(uciCheckData.strWorkerKanaMei, "^[ァ-ヶー]*$") == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0014, "読みカナ 名", "全角カナ") + ",\"" + strData + "\"");
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
        /// <param name="strData">データ文字列</param>
        /// <param name="intMinLength">項目最小長</param>
        /// <param name="intMaxLength">項目最大長</param>
        /// <param name="strMinValue">項目最小値</param>
        /// <param name="strMaxValue">項目最大値</param>
        /// <returns></returns>
        private Boolean CheckRequiredInput(String strCheckData
                                         , String strItemName
                                         , Int32 intRowCount
                                         , String strData
                                         , Int32 intMinLength
                                         , Int32 intMaxLength
                                         , String strMinValue
                                         , String strMaxValue)
        {
            // 必須入力チェック
            if (strCheckData == "")
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0011 , strItemName) + ",\"" + strData + "\"");
                return false;
            }
            else if (strCheckData.Length < intMinLength || strCheckData.Length > intMaxLength)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0012, strItemName, intMinLength, intMaxLength) + ",\"" + strData + "\"");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="lstWorkerData">読み込みデータ一覧</param>
        /// <returns></returns>
        private Boolean RegistrationWorker(WorkerCsvInfo uciCheckData)
        {
            try
            {
                // 登録・更新
                if (uciCheckData.strProcessType == g_clsSystemSettingInfo.strProcessTypeCre ||
                    uciCheckData.strProcessType == g_clsSystemSettingInfo.strProcessTypeUpd)
                {
                    // SQL文を作成する
                    string strCreateSql = @"INSERT INTO mst_Worker (employee_num, worker_name_sei, worker_name_mei, worker_name_sei_kana, worker_name_mei_kana, del_flg)
                                                                    VALUES (:WorkerNo, :WorkerSurname, :WorkerName, :WorkerSurnameKana, :WorkerNameKana, 0)
                                                               ON CONFLICT (employee_num)
                                                             DO UPDATE SET worker_name_sei = :WorkerSurname
                                                                         , worker_name_mei = :WorkerName
                                                                         , worker_name_sei_kana = :WorkerSurnameKana
                                                                         , worker_name_mei_kana = :WorkerNameKana
                                                                     WHERE mst_Worker.del_flg = 0 ";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerNo", DbType = DbType.String, Value = String.Format("{0:D4}", Int32.Parse(uciCheckData.strWorkerID)) });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerSurname", DbType = DbType.String, Value = uciCheckData.strWorkerNameSei });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerName", DbType = DbType.String, Value = uciCheckData.strWorkerNameMei });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerSurnameKana", DbType = DbType.String, Value = uciCheckData.strWorkerKanaSei });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerNameKana", DbType = DbType.String, Value = uciCheckData.strWorkerKanaMei });

                    // sqlを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand);
                }
                // 削除
                else if (uciCheckData.strProcessType == g_clsSystemSettingInfo.strProcessTypeDel)
                {
                    string strSelWorkerNo = String.Format("{0:D4}", Int32.Parse(uciCheckData.strWorkerID));

                    // ユーザ削除
                    if (DelWorker(strSelWorkerNo) == false)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0017 + "\r\n" + ex.Message);

                return false;
            }
        }

        /// <summary>
        /// ユーザ削除処理
        /// </summary>
        /// <param name="NpgsqlCon">接続子</param>
        /// <param name="transaction">トランザクション</param>
        /// <returns></returns>
        private Boolean DelWorker(string strSelWorkerNo)
        {
            // SQL文を作成する
            string strUpdateSql = @"UPDATE mst_Worker
                                       SET del_flg = 1
                                     WHERE employee_num = :WorkerNo";

            // SQLコマンドに各パラメータを設定する
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
            lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerNo", DbType = DbType.String, Value = strSelWorkerNo });

            // sqlを実行する
            g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);

            return true;
        }

        /// <summary>
        /// インポートログ出力
        /// </summary>
        private static void OutPutImportLog(string strLogText)
        {
            string time = DateTime.Now.ToString("yyyy/M/d H:m:s");

            // 出力ファイル設定
            m_strOutPutFilePath = System.IO.Directory.GetCurrentDirectory() + @"\"
                                                                            + m_CON_OUTLOGFILE_NAME
                                                                            + "_"
                                                                            + DateTime.Now.ToString("yyyyMMdd")
                                                                            + ".csv";

            try
            {
                //Shift JISで書き込む
                //書き込むファイルが既に存在している場合は、上書きする
                using (StreamWriter sw = new StreamWriter(m_strOutPutFilePath
                                                        , true
                                                        , Encoding.GetEncoding("shift_jis")))
                {
                    // １行ずつ出力を行う
                    sw.WriteLine(time + "," + strLogText);
                }
                
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);

                return;
            }
        }
        #endregion    
    }
}
