using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Threading;
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

        private const string m_CON_OUTLOGFILE_NAME = "作業者CSVファイル取り込みログ";

        private static bool m_bolProcEnd = false;
        private static bool m_bolAppendFlag = false;

        private static string m_strOutPutFilePath = string.Empty;

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
            m_bolProcEnd = false;

            DispatcherTimer dtEnabletxtImportFilePath;
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

            // ダイアログ選択直後はCSVファイルパスを一時無効にする
            // ※ファイル選択ダイアログでファイルをダブルクリックで選択した時、
            // 　同じポイントに画面のCSVファイルパスがあった場合、
            // 　再度ファイル選択ダイアログを開いてしまう考慮
            txtImportFilePath.Enabled = false;
            dtEnabletxtImportFilePath = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            dtEnabletxtImportFilePath.Start();
            dtEnabletxtImportFilePath.Tick += (s, args) =>
            {
                dtEnabletxtImportFilePath.Stop();
                txtImportFilePath.Enabled = true;
            };

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
            m_bolProcEnd = false;

            // ログ出力を上書きモードに変更する
            m_bolAppendFlag = false;

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
            if (string.IsNullOrEmpty(txtImportFilePath.Text))
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
                            if (string.IsNullOrEmpty(strFileTextLine) || bolTitleRow == true)
                            {
                                // ヘッダ行(1行目)または空行（最終行）の場合読み飛ばす
                                bolTitleRow = false;
                                continue;
                            }

                            // 件数
                            intImpCount++;

                            // 件数(登録,更新,削除)
                            if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeCre)
                            {
                                intCCount++;
                            }
                            else if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeUpd)
                            {
                                intUCount++;
                            }
                            else if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeDel)
                            {
                                intDCount++;
                            }

                            // CSVファイル読み込み＆入力データチェックを行う
                            if (ReadCsvData(strFileTextLine, intRowCount, out uciWorkerData) == false)
                            {
                                // NG件数(登録,更新,削除)
                                if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeCre)
                                {
                                    intCNgCount++;
                                }
                                else if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeUpd)
                                {
                                    intUNgCount++;
                                }
                                else if (strFileTextLine.Split(',')[m_CON_COL_WORKER_INFO_PROCESS_TYPE] == g_clsSystemSettingInfo.strProcessTypeDel)
                                {
                                    intDNgCount++;
                                }

                                intImpNgCount++;

                                continue;
                            }

                            if (m_bolProcEnd == true)
                            {
                                return;
                            }

                            // 登録処理実施
                            if (RegistrationWorker(uciWorkerData) == true)
                            {
                                // OK件数(登録,更新,削除)
                                if (uciWorkerData.strProcessType == g_clsSystemSettingInfo.strProcessTypeCre)
                                {
                                    intCOkCount++;
                                }
                                else if (uciWorkerData.strProcessType == g_clsSystemSettingInfo.strProcessTypeUpd)
                                {
                                    intUOkCount++;
                                }
                                else if (uciWorkerData.strProcessType == g_clsSystemSettingInfo.strProcessTypeDel)
                                {
                                    intDOkCount++;
                                }

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

                            if (m_bolProcEnd == true)
                            {
                                return;
                            }

                        }

                        // トランザクションコミット
                        g_clsConnectionNpgsql.DbCommit();
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015 , Environment.NewLine, ex.Message));

                    OutPutImportLog(string.Format("{0}{1}{2}{3}{4}" , "\"",g_clsMessageInfo.strMsgE0015 , Environment.NewLine , ex.Message , "\""));
                    if (m_bolProcEnd == true)
                    {
                        return;
                    }

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
                    if (m_bolProcEnd == true)
                    {
                        return;
                    }

                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgW0001,
                                                  intImpCount, intImpOkCount, intImpNgCount,
                                                  intCCount, intCOkCount, intCNgCount,
                                                  intUCount, intUOkCount, intUNgCount,
                                                  intDCount, intDOkCount, intDNgCount) + Environment.NewLine +
                                    string.Format(g_clsMessageInfo.strMsgI0002, m_strOutPutFilePath),
                                    "取り込み結果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (m_bolProcEnd == true) 
                    {
                        return;
                    }
                }
                else
                {
                    OutPutImportLog("\"" + string.Format(g_clsMessageInfo.strMsgI0001,
                                                         intImpCount, intImpOkCount, intImpNgCount,
                                                         intCCount, intCOkCount, intCNgCount,
                                                         intUCount, intUOkCount, intUNgCount,
                                                         intDCount, intDOkCount, intDNgCount) + "\"");
                    if (m_bolProcEnd == true)
                    {
                        return;
                    }

                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgI0001,
                                                  intImpCount, intImpOkCount, intImpNgCount,
                                                  intCCount, intCOkCount, intCNgCount,
                                                  intUCount, intUOkCount, intUNgCount,
                                                  intDCount, intDOkCount, intDNgCount) + Environment.NewLine +
                                    string.Format(g_clsMessageInfo.strMsgI0002, m_strOutPutFilePath),
                                    "取り込み結果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (m_bolProcEnd == true)
                    {
                        return;
                    }
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
                if (m_bolProcEnd == true)
                {
                    return false;
                }

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

            //----------
            // 更新区分
            //----------
            // 必須チェック
            if (CheckRequiredInput(uciCheckData.strProcessType, "処理区分", intRowCount, strData) == false)
            {
                return false;
            }

            // 型チェック（アルファベット）
            if (!Regex.IsMatch(uciCheckData.strProcessType, @"[^a-zA-zａ-ｚＡ-Ｚ]") == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0014, "処理区分", "アルファベット") + ",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }

            // 桁数チェック
            if (CheckLength(uciCheckData.strProcessType, "処理区分", 1, 1, strData) == false)
            {
                return false;
            }

            // 設定可能値チェック
            if (uciCheckData.strProcessType != g_clsSystemSettingInfo.strProcessTypeCre &&
                uciCheckData.strProcessType != g_clsSystemSettingInfo.strProcessTypeUpd &&
                uciCheckData.strProcessType != g_clsSystemSettingInfo.strProcessTypeDel)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog("\"" + string.Format(g_clsMessageInfo.strMsgE0014, "処理区分", String.Join(",",
                                                                                                           g_clsSystemSettingInfo.strProcessTypeCre,
                                                                                                           g_clsSystemSettingInfo.strProcessTypeUpd,
                                                                                                           g_clsSystemSettingInfo.strProcessTypeDel)) +
                                                                                               "\",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }

            //----------
            // 社員番号
            //----------
            // 必須チェック
            if (CheckRequiredInput(uciCheckData.strWorkerID, "社員番号", intRowCount, strData) == false)
            {
                return false;
            }

            // 型チェック(数字)
            if (Int32.TryParse(uciCheckData.strWorkerID, out intWorkerNo) == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0014, "社員番号", "数値") + ",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }

            // 桁数チェック
            if (CheckLength(uciCheckData.strWorkerID, "社員番号", 4, 4, strData) == false)
            {
                return false;
            }

            // 範囲チェック
            if (CheckRange(uciCheckData.strWorkerID, "社員番号", "0001", "9999", strData) == false)
            {
                return false;
            }

            //--------------
            // 作業者名＿姓
            //--------------
            // 必須チェック
            if (CheckRequiredInput(uciCheckData.strWorkerNameSei, "作業者名＿姓", intRowCount, strData) == false)
            {
                return false;
            }

            // 全角文字チェック
            if (!(Encoding.GetEncoding("Shift-JIS").GetByteCount(uciCheckData.strWorkerNameSei) == uciCheckData.strWorkerNameSei.Length * 2))
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0014, "作業者名＿性", "全角文字") + ",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }

            // 桁数チェック
            if (CheckLength(uciCheckData.strWorkerNameSei, "作業者名＿姓", 1, 10, strData) == false)
            {
                return false;
            }

            //--------------
            // 作業者名＿名
            //--------------
            // 必須チェック
            if (CheckRequiredInput(uciCheckData.strWorkerNameMei, "作業者名＿名", intRowCount, strData) == false)
            {
                return false;
            }

            // 全角文字チェック
            if (!(Encoding.GetEncoding("Shift-JIS").GetByteCount(uciCheckData.strWorkerNameMei) == uciCheckData.strWorkerNameMei.Length * 2))
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0014, "作業者名＿名", "全角文字") + ",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }

            // 桁数チェック
            if (CheckLength(uciCheckData.strWorkerNameMei, "作業者名＿名", 1, 10, strData) == false)
            {
                return false;
            }

            //--------------
            // 読みカナ＿姓
            //--------------
            // 必須チェック
            if (CheckRequiredInput(uciCheckData.strWorkerKanaSei, "読みカナ＿姓", intRowCount, strData) == false)
            {
                return false;
            }

            // カナ入力チェック
            if (Regex.IsMatch(uciCheckData.strWorkerKanaSei, "^[ァ-ヶー]*$") == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0014, "読みカナ＿性", "全角カナ") + ",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }

            // 桁数チェック
            if (CheckLength(uciCheckData.strWorkerKanaSei, "読みカナ＿姓", 1, 30, strData) == false)
            {
                return false;
            }

            //--------------
            // 読みカナ＿名
            //--------------
            // 必須チェック
            if (CheckRequiredInput(uciCheckData.strWorkerKanaMei, "読みカナ＿名", intRowCount, strData) == false)
            {
                return false;
            }

            // カナ入力チェック
            if (Regex.IsMatch(uciCheckData.strWorkerKanaMei, "^[ァ-ヶー]*$") == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0014, "読みカナ＿名", "全角カナ") + ",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }

            // 桁数チェック
            if (CheckLength(uciCheckData.strWorkerKanaMei, "読みカナ＿名", 1, 30, strData) == false)
            {
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
        /// <returns></returns>
        private Boolean CheckRequiredInput(String strCheckData
                                         , String strItemName
                                         , Int32 intRowCount
                                         , String strData)
        {
            // 必須入力チェック
            if (string.IsNullOrEmpty(strCheckData))
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0011 , strItemName) + ",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 桁数チェック
        /// </summary>
        /// <param name="strCheckData">チェック対象テキスト</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <param name="intMinLength">項目最小桁</param>
        /// <param name="intMaxLength">項目最大桁</param>
        /// <param name="strData">データ文字列</param>
        /// <returns></returns>
        private Boolean CheckLength(String strCheckData
                                    , String strItemName
                                    , Int32 intMinLength
                                    , Int32 intMaxLength
                                    , String strData)
        {
            // 必須入力チェック
            if (strCheckData.Length < intMinLength || strCheckData.Length > intMaxLength)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0012, strItemName, intMinLength, intMaxLength) + ",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 範囲チェック
        /// </summary>
        /// <param name="strCheckData">チェック対象テキスト</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <param name="strMinValue">項目最小値</param>
        /// <param name="strMaxValue">項目最大値</param>
        /// <param name="strData">データ文字列</param>
        /// <returns></returns>
        private Boolean CheckRange(String strCheckData
                                   , String strItemName
                                   , String strMinValue
                                   , String strMaxValue
                                   , String strData)
        {
            // 必須入力チェック
            if (string.Compare(strCheckData, strMinValue) == -1 || string.Compare(strCheckData, strMaxValue) == 1)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0013, strItemName, strMinValue, strMaxValue) + ",\"" + strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

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
            string strData = string.Join(",", uciCheckData.strProcessType,
                                              uciCheckData.strWorkerID,
                                              uciCheckData.strWorkerNameSei,
                                              uciCheckData.strWorkerNameMei,
                                              uciCheckData.strWorkerKanaSei,
                                              uciCheckData.strWorkerKanaMei);

            try
            {
                // 登録
                if (uciCheckData.strProcessType == g_clsSystemSettingInfo.strProcessTypeCre)
                {
                    try
                    {
                        // SQL文を作成する
                        string strCreateSql = @"INSERT INTO mst_Worker (employee_num, worker_name_sei, worker_name_mei, worker_name_sei_kana, worker_name_mei_kana, del_flg)
                                                                VALUES (:WorkerNo, :WorkerSurname, :WorkerName, :WorkerSurnameKana, :WorkerNameKana, 0)";

                        // SQLコマンドに各パラメータを設定する
                        List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerNo", DbType = DbType.String, Value = String.Format("{0:D4}", Int32.Parse(uciCheckData.strWorkerID)) });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerSurname", DbType = DbType.String, Value = uciCheckData.strWorkerNameSei });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerName", DbType = DbType.String, Value = uciCheckData.strWorkerNameMei });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerSurnameKana", DbType = DbType.String, Value = uciCheckData.strWorkerKanaSei });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerNameKana", DbType = DbType.String, Value = uciCheckData.strWorkerKanaMei });

                        // sqlを実行する
                        g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand);

                        g_clsConnectionNpgsql.DbCommit();
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0017 , Environment.NewLine , ex.Message));

                        // ログファイル出力
                        OutPutImportLog("\"" + g_clsMessageInfo.strMsgE0017 + "\r\n" + ex.Message + "\",\"" + strData + "\"");
                        if (m_bolProcEnd == true)
                        {
                            return false;
                        }

                        return false;
                    }
                    finally
                    {
                        g_clsConnectionNpgsql.DbClose();
                    }
                }

                // 更新
                if (uciCheckData.strProcessType == g_clsSystemSettingInfo.strProcessTypeUpd)
                {
                    try
                    {
                        // SQL文を作成する
                        string strCreateSql = @"UPDATE mst_Worker
                                                   SET worker_name_sei = :WorkerSurname
                                                     , worker_name_mei = :WorkerName
                                                     , worker_name_sei_kana = :WorkerSurnameKana
                                                     , worker_name_mei_kana = :WorkerNameKana
                                                 WHERE employee_num = :WorkerNo
                                                   AND del_flg = 0 ";

                        // SQLコマンドに各パラメータを設定する
                        List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerNo", DbType = DbType.String, Value = String.Format("{0:D4}", Int32.Parse(uciCheckData.strWorkerID)) });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerSurname", DbType = DbType.String, Value = uciCheckData.strWorkerNameSei });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerName", DbType = DbType.String, Value = uciCheckData.strWorkerNameMei });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerSurnameKana", DbType = DbType.String, Value = uciCheckData.strWorkerKanaSei });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerNameKana", DbType = DbType.String, Value = uciCheckData.strWorkerKanaMei });

                        // sqlを実行する
                        g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand);

                        g_clsConnectionNpgsql.DbCommit();
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0018 , Environment.NewLine , ex.Message));

                        // ログファイル出力
                        OutPutImportLog("\"" + g_clsMessageInfo.strMsgE0018 + "\r\n" + ex.Message + "\",\"" + strData + "\"");
                        if (m_bolProcEnd == true)
                        {
                            return false;
                        }

                        return false;
                    }
                    finally
                    {
                        g_clsConnectionNpgsql.DbClose();
                    }
                }

                // 削除
                if (uciCheckData.strProcessType == g_clsSystemSettingInfo.strProcessTypeDel)
                {
                    string strSelWorkerNo = String.Format("{0:D4}", Int32.Parse(uciCheckData.strWorkerID));

                    // ユーザ削除
                    if (DelWorker(strSelWorkerNo, strData) == false)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0017 ,Environment.NewLine , ex.Message));

                return false;
            }
        }

        /// <summary>
        /// ユーザ削除処理
        /// </summary>
        /// <param name="strSelWorkerNo">社員番号</param>
        /// <param name="strData">データ文字列</param>
        /// <returns></returns>
        private Boolean DelWorker(string strSelWorkerNo, string strData)
        {
            try
            {
                // SQL文を作成する
                string strUpdateSql = @"UPDATE mst_Worker
                                       SET del_flg = 1
                                     WHERE employee_num = :WorkerNo
                                       AND del_flg = 0";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "WorkerNo", DbType = DbType.String, Value = strSelWorkerNo });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);

                g_clsConnectionNpgsql.DbCommit();

                return true;

            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0018 ,Environment.NewLine, ex.Message));

                // ログファイル出力
                OutPutImportLog("\"" + g_clsMessageInfo.strMsgE0018 + "\r\n" + ex.Message + "\",\""+ strData + "\"");
                if (m_bolProcEnd == true)
                {
                    return false;
                }

                return false;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// インポートログ出力
        /// </summary>
        private static void OutPutImportLog(string strLogText)
        {
            string time = DateTime.Now.ToString("yyyy/M/d H:m:s");

            // 出力ファイル設定
            m_strOutPutFilePath = g_clsSystemSettingInfo.strLogFileOutputDirectory + @"\"
                                                                                   + m_CON_OUTLOGFILE_NAME
                                                                                   + ".csv";

            try
            {
                //Shift JISで書き込む
                //書き込むファイルが既に存在している場合は、上書きする
                using (StreamWriter sw = new StreamWriter(m_strOutPutFilePath
                                                        , m_bolAppendFlag
                                                        , Encoding.GetEncoding("shift_jis")))
                {
                    // １行ずつ出力を行う
                    sw.WriteLine(time + "," + strLogText);
                }
                
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR,string.Format("{0}{1}{2}" ,g_clsMessageInfo.strMsgE0016 , Environment.NewLine , ex.Message));

                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0055, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 処理を中断
                // ※以下の対応が必要
                // 　・このメソッドを呼び出す可能性のある関数が対象であり、
                // 　　その関数の全ての呼出し直後でreturnを返す(メソッド単位の処理を終了する)ようにする。
                // 　　最終的に、大元のイベントで処理を終了できればOK。
                // 　・イベント開始直後にこの変数を初期化(false)する。
                m_bolProcEnd = true;
            }

            // ログ出力を追記モードに変更する
            m_bolAppendFlag = true;
        }
        #endregion    
    }
}
