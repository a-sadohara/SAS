using ImageChecker.DTO;
using log4net;
using Npgsql;
using SevenZipNET;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageChecker
{
    static class Common
    {
        // 接続情報情報
        public static string g_strConnectionString = string.Empty;
        private const string g_CON_CONNECTION_STRING = "Server={0};Port={1};User ID={2};Database={3};Password={4};Enlist=true";
        public static string g_strDBName = string.Empty;
        public static string g_strDBUser = string.Empty;
        public static string g_strDBUserPassword = string.Empty;
        public static string g_strDBServerName = string.Empty;
        public static string g_strDBPort = string.Empty;
        public static string g_strSharedFolderPath = string.Empty;
        public static string g_strSharedFolderUser = string.Empty;
        public static string g_strSharedFolderPassword = string.Empty;
        private const string g_CON_SHARED_FOLDER_CONNECTION_STRING = @" use {0} /user:{1} {2}";

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報クラス
        public static SystemSettingInfo g_clsSystemSettingInfo;

        // メッセージ情報クラス
        public static MessageInfo g_clsMessageInfo;

        // ログイン情報クラス
        public static LoginInfo g_clsLoginInfo;

        // 帳票情報クラス
        public static ReportInfo g_clsReportInfo;

        // システム設定情報取得時のエラーメッセージ格納用
        private static StringBuilder m_sbErrMessage = new StringBuilder();

        // 遷移先
        public const int g_CON_APID_LOGIN = 1;
        public const int g_CON_APID_TARGET_SELECTION = 3;
        public const int g_CON_APID_RESULT_CHECK = 6;
        public const int g_CON_APID_DISPLAY_RESULTS = 9;
        public const int g_CON_APID_DISPLAY_RESULTS_AGO = 10;

        // NoImage画像ファイルパス
        public const string g_CON_NO_IMAGE_FILE_PATH = @"image\NoImage.png";

        // イベントログ出力関連
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int g_CON_LEVEL_FATAL = 1;
        public const int g_CON_LEVEL_ERROR = 2;
        public const int g_CON_LEVEL_WARN = 3;
        public const int g_CON_LEVEL_INFO = 4;
        public const int g_CON_LEVEL_DEBUG = 5;

        // メッセージ関連
        public const string g_CON_MESSAGE_TITLE_ERROR = "エラー";
        public const string g_CON_MESSAGE_TITLE_WARN = "警告";
        public const string g_CON_MESSAGE_TITLE_QUESTION = "確認";

        // NG理由（主要）
        public const string g_CON_ACCEPTANCE_CHECK_RESULT_NG_DETECT = "AI検知";
        public const string g_CON_ACCEPTANCE_CHECK_RESULT_NG_NON_DETECT = "未検知";
        public const string g_CON_NG_REASON_OK = "過検知";
        public const string g_CON_NG_REASON_OTHER_NG_JUDGEMENT = "□他画像でNG判定済み";

        // 号機
        public const string g_strUnitNumN1 = "N1";
        public const string g_strUnitNumN2 = "N2";
        public const string g_strUnitNumN3 = "N3";
        public const string g_strUnitNumN4 = "N4";

        // プロセス優先度
        public const string g_strPriorityRealtime = "Realtime";
        public const string g_strPriorityHighPriority = "High Priority";
        public const string g_strPriorityAboveNormal = "Above Normal";
        public const string g_strPriorityNormal = "Normal";
        public const string g_strProcessNameImageChecker = "ImageChecker.exe";
        public const string g_strProcessName7zip = "7za.exe";
        private const string g_CON_CHANGE_PRIORITY = @"/c wmic process where name=""{0}"" CALL setpriority ""{1}""";

        // ログイン関連
        public static bool g_bolStatus = false; //0:ログアウト 1:ログイン

        // サブ一時ディレクトリ
        public const string g_CON_DIR_MASTER_IMAGE = "MasterImage";                         // マスタ画像格納先
        public const string g_CON_DIR_MASTER_IMAGE_MARKING = "MasterImageMarking";          // マーキングマスタ画像格納先
        public const string g_CON_ZIP_EXTRACT_DIR_PATH = "ZipExtractDirPath";               // ZIP解凍用格納先
        public static string g_strMasterImageDirPath = string.Empty;
        public static string g_strMasterImageDirMarking = string.Empty;
        public static string g_strZipExtractDirPath = string.Empty;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Mutex名を設定する
            string mutexName = "ImageChecker";

            // Mutexオブジェクトを作成する
            Mutex mutex = new Mutex(false, mutexName);

            bool hasConnection = false;
            bool hasHandle = false;

            try
            {
                try
                {
                    // Mutexの所有権を要求する
                    hasHandle = mutex.WaitOne(0, false);
                }
                catch (AbandonedMutexException)
                {
                    // 別のアプリケーションがMutexを解放せず終了した場合、変数を更新する
                    hasHandle = true;
                }

                if (!hasHandle)
                {
                    MessageBox.Show("多重起動はできません。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 接続文字列をApp.configファイルから取得
                GetAppConfigValue("DBName", ref g_strDBName);
                GetAppConfigValue("DBUser", ref g_strDBUser);
                GetAppConfigValue("DBUserPassword", ref g_strDBUserPassword);
                GetAppConfigValue("DBServerName", ref g_strDBServerName);
                GetAppConfigValue("DBPort", ref g_strDBPort);
                GetAppConfigValue("SharedFolderPath", ref g_strSharedFolderPath);
                GetAppConfigValue("SharedFolderUser", ref g_strSharedFolderUser);
                GetAppConfigValue("SharedFolderPassword", ref g_strSharedFolderPassword);

                if (m_sbErrMessage.Length > 0)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("接続文字列取得時にエラーが発生しました。{0}{1}", Environment.NewLine, m_sbErrMessage.ToString()));
                    // メッセージ出力
                    MessageBox.Show("接続文字列取得時に例外が発生しました。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                if (!string.IsNullOrWhiteSpace(g_strSharedFolderPath) &&
                    !Directory.Exists(g_strSharedFolderPath))
                {
                    try
                    {
                        // 共有フォルダ接続
                        using (Process prNet = new Process())
                        {
                            prNet.StartInfo.FileName = "net.exe";
                            prNet.StartInfo.Arguments =
                                string.Format(
                                    g_CON_SHARED_FOLDER_CONNECTION_STRING,
                                    g_strSharedFolderPath,
                                    g_strSharedFolderUser,
                                    g_strSharedFolderPassword);
                            prNet.StartInfo.CreateNoWindow = true;
                            prNet.StartInfo.UseShellExecute = false;
                            prNet.StartInfo.RedirectStandardOutput = true;
                            prNet.Start();
                            prNet.WaitForExit();
                        }

                        hasConnection = Directory.Exists(g_strSharedFolderPath);
                    }
                    catch (Exception)
                    {
                        hasConnection = false;
                    }

                    if (!hasConnection)
                    {
                        MessageBox.Show("共有フォルダに接続できません。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                g_strConnectionString = string.Format(g_CON_CONNECTION_STRING, g_strDBServerName,
                                                                               g_strDBPort,
                                                                               g_strDBUser,
                                                                               g_strDBName,
                                                                               g_strDBUserPassword);
                // 接続確認
                g_clsConnectionNpgsql = new ConnectionNpgsql(g_strConnectionString);
                g_clsConnectionNpgsql.DbOpen();
                g_clsConnectionNpgsql.DbClose();

                // システム設定情報取得
                g_clsSystemSettingInfo = new SystemSettingInfo();
                if (g_clsSystemSettingInfo.bolNormalEnd == false)
                {
                    return;
                }

                // メッセージ情報取得
                g_clsMessageInfo = new MessageInfo();
                if (g_clsMessageInfo.bolNormalEnd == false)
                {
                    return;
                }

                // ログイン情報インスタンス生成
                g_clsLoginInfo = new LoginInfo();

                // 帳票情報インスタンス生成
                g_clsReportInfo = new ReportInfo();

                // パス設定
                g_strMasterImageDirPath = Path.Combine(g_clsSystemSettingInfo.strTemporaryDirectory, g_CON_DIR_MASTER_IMAGE);
                g_strMasterImageDirMarking = Path.Combine(g_clsSystemSettingInfo.strTemporaryDirectory, g_CON_DIR_MASTER_IMAGE_MARKING);
                g_strZipExtractDirPath = Path.Combine(g_clsSystemSettingInfo.strTemporaryDirectory, g_CON_ZIP_EXTRACT_DIR_PATH);

                // プロセス優先度を変更する
                ChangeProcessingPriority(g_strProcessNameImageChecker);
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("初期起動時にエラーが発生しました。{0}{1}", Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show("初期起動時に例外が発生しました。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            finally
            {
                if (g_clsConnectionNpgsql != null)
                {
                    g_clsConnectionNpgsql.DbClose();
                }
            }

            // フォーム画面を起動
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Login());
        }

        /// <summary>
        /// 欠点画像取込
        /// </summary>
        /// <param name="strUnitNum">号機</param>
        /// <param name="strFaultImageFileName">欠点画像ファイル名</param>
        /// <param name="bolUndetectedImageFlag">未検知画像フラグ</param>
        /// <param name="strFaultImageFolderName">欠点画像フォルダ名</param>
        public static async Task<Boolean> BolInputFaultImage(
            string strUnitNum,
            string strFaultImageFileName,
            bool bolUndetectedImageFlag = false,
            string strFaultImageFolderName = null)
        {
            // 引数チェック
            if (bolUndetectedImageFlag &&
                string.IsNullOrWhiteSpace(strFaultImageFolderName))
            {
                return false;
            }

            // NG画像連携ディレクトリ
            string strNgImageCooperationDirectory = string.Empty;

            // 号機に紐付くディレクトリ情報を設定
            switch (strUnitNum)
            {
                case g_strUnitNumN1:
                    strNgImageCooperationDirectory = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN1;
                    break;
                case g_strUnitNumN2:
                    strNgImageCooperationDirectory = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN2;
                    break;
                case g_strUnitNumN3:
                    strNgImageCooperationDirectory = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN3;
                    break;
                case g_strUnitNumN4:
                    strNgImageCooperationDirectory = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN4;
                    break;
                default:
                    return false;
            }

            // 解凍ディレクトリ
            string strDecompressionDirectory = string.Empty;

            // 欠点画像格納ディレクトリ
            string strFaultImageDecompressionDirectory = string.Empty;

            if (bolUndetectedImageFlag)
            {
                // 一時ディレクトリを設定
                strDecompressionDirectory = Path.Combine(g_strZipExtractDirPath, strUnitNum);

                // 欠点画像格納ディレクトリを設定
                strFaultImageDecompressionDirectory = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, strUnitNum, strFaultImageFolderName);
            }
            else
            {
                // 欠点画像格納ディレクトリを設定
                strDecompressionDirectory = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, strUnitNum);
            }

            // 解凍サブディレクトリ
            string strDecompressionSubDirectory = Path.Combine(strDecompressionDirectory, strFaultImageFileName);

            // zipファイル名
            string strZipFileName = strFaultImageFileName + ".zip";

            // zipファイルパス
            string strZipFilePath = Path.Combine(strNgImageCooperationDirectory, strZipFileName);

            // zipファイルを解凍する
            Task<Boolean> taskExtractZipAll = Task<Boolean>.Run(() => ExtractZipAll(strZipFilePath, strDecompressionDirectory, strDecompressionSubDirectory));

            await Task.Delay(1000);

            // 解凍処理のプロセス優先度を変更する
            ChangeProcessingPriority(g_strProcessName7zip);

            await taskExtractZipAll;

            // 解凍有無をチェックする
            if (!taskExtractZipAll.Result)
            {
                return false;
            }

            if (bolUndetectedImageFlag)
            {
                // 解凍サブディレクトリの情報を取得する
                DirectoryInfo diDecompressionInfo = new DirectoryInfo(strDecompressionSubDirectory);

                try
                {
                    // 一時ディレクトリから欠点画像格納ディレクトリにファイルをコピーする(同名ファイルは上書きする)
                    foreach (FileInfo filePath in diDecompressionInfo.GetFiles().Where(x => string.Compare(x.Extension, ".jpg", true) == 0))
                    {
                        File.Copy(filePath.FullName, Path.Combine(strFaultImageDecompressionDirectory, filePath.Name), true);
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0040, Environment.NewLine, ex.Message));

                    // エラー発生時、中途半端に取り込まれた情報を削除する
                    diDecompressionInfo.Delete(true);

                    return false;
                }

                // 解凍サブディレクトリを削除する
                diDecompressionInfo.Delete(true);
            }

            return true;
        }

        /// <summary>
        /// 欠点画像再取込
        /// </summary>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="strUnitNum">号機</param>
        /// <param name="strFabricName">反番</param>
        /// <param name="strFaultImageFileName">欠点画像ファイル名</param>
        public static async Task<Boolean> BolReInputFaultImage(
            int intInspectionNum,
            string strInspectionDate,
            string strUnitNum,
            string strFabricName,
            string strFaultImageFileName)
        {
            DirectoryInfo diFaultImage = null;
            DataTable dtData = null;
            string strSQL = string.Empty;
            string strFaultImageDecompressionDirectory = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, strUnitNum, strFaultImageFileName);
            int intTotalCount = 0;

            try
            {
                dtData = new DataTable();

                // 合否判定結果テーブルに登録されているオリジナル画像・マーキング画像の総数を取得する
                strSQL = @"SELECT COUNT(DISTINCT(org_imagepath)) * 2 AS TotalCount
                           FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                           AND   inspection_num = :inspection_num
                           AND   unit_num = :unit_num";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                intTotalCount = Convert.ToInt32(dtData.Rows[0]["TotalCount"]);

                dtData = new DataTable();

                // 合否判定結果テーブルに登録されている未検知画像情報を取得する
                strSQL = @"SELECT DISTINCT(org_imagepath)
                           FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                           AND   inspection_num = :inspection_num
                           AND   unit_num = :unit_num
                           AND   over_detection_except_result = :over_detection_except_result_ng_non_detect
                           ORDER BY org_imagepath";

                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ng_non_detect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNgNonDetect });

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}",
                        g_clsMessageInfo.strMsgE0060,
                        Environment.NewLine, ex.Message));

                return false;
            }

            // 欠点画像の取込を行う
            Task<Boolean> taskInputFaultImage = Task<Boolean>.Run(() => BolInputFaultImage(strUnitNum, strFaultImageFileName));
            await taskInputFaultImage;

            if (!taskInputFaultImage.Result)
            {
                return false;
            }

            // 未検知画像の取込を行う
            foreach (DataRow row in dtData.Rows)
            {
                Task<Boolean> taskInputFaultImageUndetected = Task<Boolean>.Run(() => BolInputFaultImage(strUnitNum, row["org_imagepath"].ToString().Replace(".jpg", string.Empty), true, strFaultImageFileName));
                await taskInputFaultImageUndetected;

                if (!taskInputFaultImageUndetected.Result)
                {
                    return false;
                }
            }

            diFaultImage = new DirectoryInfo(strFaultImageDecompressionDirectory);

            try
            {
                // 解凍した画像数と比較する
                if (intTotalCount > diFaultImage.GetFiles().Where(x => string.Compare(x.Extension, ".jpg", true) == 0).Count())
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}",
                        g_clsMessageInfo.strMsgE0040,
                        Environment.NewLine,
                        ex.Message));

                return false;
            }

            return true;
        }

        /// <summary>
        /// zip解凍
        /// </summary>
        /// <param name="strZipFilePath">zipファイルパス</param>
        /// <param name="strDecompressionDirectory">解凍ディレクトリ</param>
        /// <param name="strDecompressionSubDirectory">解凍サブディレクトリ</param>
        private static async Task<Boolean> ExtractZipAll(
            string strZipFilePath,
            string strDecompressionDirectory,
            string strDecompressionSubDirectory)
        {
            SevenZipBase.Path7za = @".\7z-extra\x64\7za.exe";

            try
            {
                SevenZipExtractor extractor = new SevenZipExtractor(strZipFilePath);

                for (int intProcessingTimes = 0; intProcessingTimes < 2; intProcessingTimes++)
                {
                    // zipファイルを解凍する(同名ファイルは上書きする)
                    extractor.ExtractAll(strDecompressionDirectory, true);

                    // 解凍サブディレクトリが存在する場合、処理を抜ける
                    if (Directory.Exists(strDecompressionSubDirectory))
                    {
                        break;
                    }

                    // 解凍サブディレクトリを作成し、解凍処理をリトライする
                    Directory.CreateDirectory(strDecompressionSubDirectory);
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0040, Environment.NewLine, ex.Message));

                // エラー発生時、中途半端に取り込まれた情報を削除する
                if (Directory.Exists(strDecompressionSubDirectory))
                {
                    Directory.Delete(strDecompressionSubDirectory, true);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// プロセス優先度変更
        /// </summary>
        /// <param name="strProcessName">対象プロセス</param>
        private static void ChangeProcessingPriority(string strProcessName)
        {
            // プロセス優先度を変更する
            if (!string.IsNullOrWhiteSpace(g_clsSystemSettingInfo.strProcessingPriority) &&
                !g_clsSystemSettingInfo.strProcessingPriority.Equals(g_strPriorityNormal))
            {
                using (Process prCmd = new Process())
                {
                    prCmd.StartInfo.FileName = "cmd.exe";
                    prCmd.StartInfo.Arguments =
                        string.Format(
                            g_CON_CHANGE_PRIORITY,
                            strProcessName,
                            g_clsSystemSettingInfo.strProcessingPriority);
                    prCmd.StartInfo.CreateNoWindow = true;
                    prCmd.StartInfo.UseShellExecute = false;
                    prCmd.StartInfo.RedirectStandardOutput = true;
                    prCmd.Start();
                    prCmd.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Rapidレコード件数チェック
        /// </summary>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="strFabricName">反番</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="strUnitNum">号機</param>
        /// <returns>存在フラグ</returns>
        public static bool BolCheckRapidRecordCount(
            int intInspectionNum,
            string strFabricName,
            string strInspectionDate,
            string strUnitNum)
        {
            string strSQL = string.Empty;
            string strRapidTableName = "rapid_" + strFabricName + "_" + intInspectionNum + "_" + strInspectionDate.Replace("/", string.Empty);
            DataTable dtRapidData = new DataTable();

            try
            {
                // レコード件数をチェックする
                strSQL = @"SELECT COUNT(*) AS RecordCount
                           FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".""" + strRapidTableName + @"""
                           WHERE fabric_name = :fabric_name
                           AND inspection_num = :inspection_num
                           AND unit_num = :unit_num";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });

                // SQLを実行する
                g_clsConnectionNpgsql.SelectSQL(ref dtRapidData, strSQL, lstNpgsqlCommand);

                if (dtRapidData.Rows.Count == 0 ||
                    int.Parse(dtRapidData.Rows[0]["RecordCount"].ToString()) == 0)
                {
                    return false;
                }
            }
            catch (PostgresException pgex)
            {
                g_clsConnectionNpgsql.DbRollback();

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}検査日付:{2}, {3}号機, 検査番号:{4}, 反番:{5}, 取得対象テーブル:{6}, 処理ブロック:{7}{8}{9}",
                        g_clsMessageInfo.strMsgE0001,
                        Environment.NewLine,
                        strInspectionDate,
                        strUnitNum,
                        intInspectionNum,
                        strFabricName,
                        strRapidTableName,
                        "Rapidテーブル件数取得",
                        Environment.NewLine,
                        pgex.Message));

                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0039, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));

                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0039, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }

            return true;
        }

        /// <summary>
        /// イベントログ出力
        /// </summary>
        /// <param name="intLevel">レベル</param>
        /// <param name="strMessage">メッセージ</param>
        public static void WriteEventLog(int intLevel, string strMessage)
        {
            switch (intLevel)
            {
                case g_CON_LEVEL_FATAL:
                    log.Fatal(strMessage);
                    break;
                case g_CON_LEVEL_ERROR:
                    log4net.ThreadContext.Properties["EventID"] = 99;
                    log.Error(strMessage);
                    break;
                case g_CON_LEVEL_WARN:
                    log4net.ThreadContext.Properties["EventID"] = 88;
                    log.Warn(strMessage);
                    break;
                case g_CON_LEVEL_INFO:
                    log4net.ThreadContext.Properties["EventID"] = 11;
                    log.Info(strMessage);
                    break;
                case g_CON_LEVEL_DEBUG:
                    log.Debug(strMessage);
                    break;
            }
        }

        /// <summary>
        /// App.configファイルから設定値を取得
        /// </summary>
        /// <param name="strKey">キー</param>
        /// <param name="strValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private static void GetAppConfigValue(string strKey, ref string strValue)
        {
            strValue = ConfigurationManager.AppSettings[strKey];
            if (strValue == null)
            {
                m_sbErrMessage.AppendLine("Key[" + strKey + "] AppConfigに存在しません。");
            }
        }
    }
}