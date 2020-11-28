using log4net;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RecoveryTool
{
    static class Common
    {
        // コンフィグ情報
        public static string g_strConnectionString = string.Empty;
        private const string g_CON_CONNECTION_STRING = "Server={0};Port={1};User ID={2};Database={3};Password={4};Enlist=true";
        public static string g_strDBName = string.Empty;
        public static string g_strDBUser = string.Empty;
        public static string g_strDBUserPassword = string.Empty;
        public static string g_strDBServerName = string.Empty;
        public static string g_strDBPort = string.Empty;
        public static string g_strUnitNum = string.Empty;
        public static string g_strConnectionPoint = string.Empty;
        public static string g_strConnectionUser = string.Empty;
        public static string g_strConnectionPassword = string.Empty;
        public static string g_strErrorFileOutputPath = string.Empty;
        public static string g_strInputPath = string.Empty;
        public static string g_strInputScanInfomationPath = string.Empty;
        public static string g_strInputRegistrationMarkPath = string.Empty;
        public static string g_strTempPath = string.Empty;
        public static string g_strTempScanInfomationPath = string.Empty;
        public static string g_strTempRegistrationMarkPath = string.Empty;
        public static string g_strImagecheckPath = string.Empty;
        public static string g_strCheckedImagecheckPath = string.Empty;
        public static string g_strFolderNameScanInfo = "ScanInfomation";
        public static string g_strFolderNameRegiMark = "RegistrationMarkFiles";
        public static string g_strFolderNameCheck = "CHECKED";
        public static string[] g_strTaskName;
        public static string[] g_strExecutionFileName;
        public static string[] g_strRecoveryExemptErrorFileName;
        public static int g_intFabricInfoUpdateStatus = 0;
        public static int g_intProcessingStatusUpdateStatus = 0;
        public static int g_intRapidAnalysisRapidResultUpdateStatus = 0;
        public static int g_intRapidAnalysisEdgeResultUpdateStatus = 0;
        public static int g_intRapidAnalysisMaskingResultUpdateStatus = 0;
        public static int g_intRetryTimes = 0;
        public static int g_intRetryWaitSeconds = 0;
        public static int g_intRetryWaitMilliSeconds = 0;
        public static int[] g_intFabricInfoExceptExtractionStatus;

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報取得時のエラーメッセージ格納用
        private static StringBuilder m_sbErrMessage = new StringBuilder();

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

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Mutex名を設定する
            string mutexName = "RecoveryTool";

            // Mutexオブジェクトを作成する
            Mutex mutex = new Mutex(false, mutexName);

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

                string strConnectionPoint = string.Empty;
                string strConnectionUser = string.Empty;
                string strConnectionPassword = string.Empty;
                string strTaskName = string.Empty;
                string strRecoveryExemptErrorFileName = string.Empty;
                string strExecutionFileName = string.Empty;
                string strFabricInfoExceptExtractionStatus = string.Empty;

                // 接続文字列をApp.configファイルから取得
                GetAppConfigValue("DBName", ref g_strDBName);
                GetAppConfigValue("DBUser", ref g_strDBUser);
                GetAppConfigValue("DBUserPassword", ref g_strDBUserPassword);
                GetAppConfigValue("DBServerName", ref g_strDBServerName);
                GetAppConfigValue("DBPort", ref g_strDBPort);
                GetAppConfigValue("UnitNum", ref g_strUnitNum);
                GetAppConfigValue("ConnectionPoint", ref strConnectionPoint);
                GetAppConfigValue("ConnectionUser", ref strConnectionUser);
                GetAppConfigValue("ConnectionPassword", ref strConnectionPassword);
                GetAppConfigValue("TaskName", ref strTaskName);
                GetAppConfigValue("ErrorFileOutputPath", ref g_strErrorFileOutputPath);
                GetAppConfigValue("InputPath", ref g_strInputPath);
                GetAppConfigValue("TempPath", ref g_strTempPath);
                GetAppConfigValue("ImagecheckPath", ref g_strImagecheckPath);
                GetAppConfigValue("ScanInfoFolderName", ref g_strFolderNameScanInfo);
                GetAppConfigValue("RegiMarkFolderName", ref g_strFolderNameRegiMark);
                GetAppConfigValue("CheckFolderName", ref g_strFolderNameCheck);
                GetAppConfigValue("RecoveryExemptErrorFileName", ref strRecoveryExemptErrorFileName);
                GetAppConfigValue("ExecutionFileName", ref strExecutionFileName);
                GetAppConfigValue("FabricInfoUpdateStatus", ref g_intFabricInfoUpdateStatus);
                GetAppConfigValue("ProcessingStatusUpdateStatus", ref g_intProcessingStatusUpdateStatus);
                GetAppConfigValue("RapidAnalysisRapidResultUpdateStatus", ref g_intRapidAnalysisRapidResultUpdateStatus);
                GetAppConfigValue("RapidAnalysisEdgeResultUpdateStatus", ref g_intRapidAnalysisEdgeResultUpdateStatus);
                GetAppConfigValue("RapidAnalysisMaskingResultUpdateStatus", ref g_intRapidAnalysisMaskingResultUpdateStatus);
                GetAppConfigValue("RetryTimes", ref g_intRetryTimes);
                GetAppConfigValue("RetryWaitSeconds", ref g_intRetryWaitSeconds);
                GetAppConfigValue("FabricInfoExceptExtractionStatus", ref strFabricInfoExceptExtractionStatus);

                if (m_sbErrMessage.Length > 0)
                {
                    // ログ出力
                    WriteEventLog(
                        g_CON_LEVEL_ERROR,
                        string.Format(
                            "Config情報取得時にエラーが発生しました。{0}{1}",
                            Environment.NewLine,
                            m_sbErrMessage.ToString()));

                    // メッセージ出力
                    MessageBox.Show(
                        "Config情報取得時に例外が発生しました。",
                        g_CON_MESSAGE_TITLE_ERROR,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                if (!string.IsNullOrWhiteSpace(strConnectionPoint))
                {
                    g_strConnectionPoint = string.Format("/S {0}", strConnectionPoint);
                }

                if (!string.IsNullOrWhiteSpace(strConnectionUser))
                {
                    g_strConnectionUser = string.Format("/U {0}", strConnectionUser);
                }

                if (!string.IsNullOrWhiteSpace(strConnectionPassword))
                {
                    g_strConnectionPassword = string.Format("/P {0}", strConnectionPassword);
                }

                // 入力ファイル格納パスを生成する。
                if (!string.IsNullOrWhiteSpace(g_strInputPath))
                {
                    g_strInputScanInfomationPath = Path.Combine(g_strInputPath, g_strFolderNameScanInfo);
                    g_strInputRegistrationMarkPath = Path.Combine(g_strInputPath, g_strFolderNameRegiMark);
                }

                // 一時ファイル格納パスを生成する。
                if (!string.IsNullOrWhiteSpace(g_strTempPath))
                {
                    g_strTempScanInfomationPath = Path.Combine(g_strTempPath, g_strFolderNameScanInfo);
                    g_strTempRegistrationMarkPath = Path.Combine(g_strTempPath, g_strFolderNameRegiMark);
                }

                // 行間枚数チェック用ファイル格納パスを生成する。
                if (!string.IsNullOrWhiteSpace(g_strImagecheckPath))
                {
                    g_strCheckedImagecheckPath = Path.Combine(g_strImagecheckPath, g_strFolderNameCheck);
                }

                g_strTaskName = strTaskName.Split(',').OrderBy(x => x).ToArray();
                g_strRecoveryExemptErrorFileName = strRecoveryExemptErrorFileName.Split(',').OrderBy(x => x).ToArray();
                g_strExecutionFileName = strExecutionFileName.Split(',').OrderBy(x => x).ToArray();
                g_intFabricInfoExceptExtractionStatus = strFabricInfoExceptExtractionStatus.Split(',').Select(x => int.Parse(x)).OrderBy(x => x).ToArray();

                if (g_intRetryWaitSeconds == 0)
                {
                    g_intRetryWaitSeconds = 10;
                }

                g_intRetryWaitMilliSeconds = g_intRetryWaitSeconds * 1000;

                g_strConnectionString =
                    string.Format(
                        g_CON_CONNECTION_STRING,
                        g_strDBServerName,
                        g_strDBPort,
                        g_strDBUser,
                        g_strDBName,
                        g_strDBUserPassword);

                // 接続確認
                g_clsConnectionNpgsql = new ConnectionNpgsql(g_strConnectionString);
                g_clsConnectionNpgsql.DbOpen();
                g_clsConnectionNpgsql.DbClose();
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "初期起動時にエラーが発生しました。{0}{1}",
                        Environment.NewLine,
                        ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    "初期起動時に例外が発生しました。",
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }
            finally
            {
                if (g_clsConnectionNpgsql != null)
                {
                    g_clsConnectionNpgsql.DbClose();
                }
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RecoveryScreen());
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
                    ThreadContext.Properties["EventID"] = 99;
                    log.Error(strMessage);
                    break;
                case g_CON_LEVEL_WARN:
                    ThreadContext.Properties["EventID"] = 88;
                    log.Warn(strMessage);
                    break;
                case g_CON_LEVEL_INFO:
                    ThreadContext.Properties["EventID"] = 11;
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

        /// <summary>
        /// App.configファイルから設定値を取得
        /// </summary>
        /// <param name="strKey">キー</param>
        /// <param name="intValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private static void GetAppConfigValue(string strKey, ref int intValue)
        {
            string strValue = string.Empty;

            try
            {
                GetAppConfigValue(strKey, ref strValue);
                intValue = int.Parse(strValue);
            }
            catch (Exception ex)
            {
                m_sbErrMessage.AppendLine(string.Format("Key[{0}] {1}", strKey, ex.Message));
            }
        }
    }
}