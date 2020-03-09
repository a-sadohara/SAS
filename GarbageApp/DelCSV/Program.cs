using DelCSV.DTO;
using log4net;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DelCSV
{
    static class Program
    {
        // 接続情報
        public static string g_strConnectionString = string.Empty;
        private const string g_CON_CONNECTION_STRING = "Server={0};Port={1};User ID={2};Database={3};Password={4};Enlist=true";
        public static string g_strDBName = string.Empty;
        public static string g_strDBUser = string.Empty;
        public static string g_strDBUserPassword = string.Empty;
        public static string g_strDBServerName = string.Empty;
        public static string g_strDBPort = string.Empty;
        public static string g_strCSVRetentionPeriod = string.Empty;

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報クラス
        public static SystemSettingInfo g_clsSystemSettingInfo;

        // メッセージ情報クラス
        public static MessageInfo g_clsMessageInfo;

        // システム設定情報取得時のエラーメッセージ格納用
        private static StringBuilder m_sbErrMessage = new StringBuilder();

        // イベントログ出力関連
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int g_CON_LEVEL_FATAL = 1;
        public const int g_CON_LEVEL_ERROR = 2;
        public const int g_CON_LEVEL_WARN = 3;
        public const int g_CON_LEVEL_INFO = 4;
        public const int g_CON_LEVEL_DEBUG = 5;

        // 削除件数
        private static int m_intDeleteCont = 0;

        static void Main(string[] args)
        {
            try
            {
                // 接続文字列をApp.configファイルから取得する。
                GetAppConfigValue("DBName", ref g_strDBName);
                GetAppConfigValue("DBUser", ref g_strDBUser);
                GetAppConfigValue("DBUserPassword", ref g_strDBUserPassword);
                GetAppConfigValue("DBServerName", ref g_strDBServerName);
                GetAppConfigValue("DBPort", ref g_strDBPort);
                GetAppConfigValue("CSVRetentionPeriod", ref g_strCSVRetentionPeriod);

                if (m_sbErrMessage.Length > 0)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_WARN, string.Format("接続文字列取得時にエラーが発生しました。{0}{1}", Environment.NewLine, m_sbErrMessage.ToString()));

                    // メッセージ出力
                    Console.WriteLine("接続文字列取得時に例外が発生しました。");

                    return;
                }

                // 接続文字列を組み立てる。
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

                // 対象ディレクトリを取得
                DirectoryInfo dyInfo = new DirectoryInfo(g_clsSystemSettingInfo.strInspectionResltCsvDirectory);

                // 保持期間の取得
                DateTime datRetentionPeriod = DateTime.Now.Date.AddMonths(-1 * int.Parse(g_strCSVRetentionPeriod));

                // ファイルの取得
                foreach (FileInfo fInfo in dyInfo.GetFiles().Where(
                    x => string.Compare(x.Extension, ".csv", true) == 0 &&
                    x.LastWriteTime < datRetentionPeriod))
                {
                    fInfo.Delete();
                    m_intDeleteCont++;
                }

                if (m_intDeleteCont > 0)
                {
                    WriteEventLog(
                        g_CON_LEVEL_INFO,
                        string.Format(
                            "{0}個のCSVファイルを削除しました。",
                            m_intDeleteCont));
                }
                else
                {
                    WriteEventLog(g_CON_LEVEL_INFO, "削除対象のCSVファイルはありません。");
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_WARN, string.Format("初期起動時にエラーが発生しました。{0}{1}", Environment.NewLine, ex.Message));

                // メッセージ出力
                Console.WriteLine("初期起動時に例外が発生しました。");

                return;
            }
            finally
            {
                if (g_clsConnectionNpgsql != null)
                {
                    g_clsConnectionNpgsql.DbClose();
                }
            }
        }

        /// <summary>
        /// App.configファイルから設定値を取得
        /// </summary>
        /// <param name="strKey">キー</param>
        /// <param name="strValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        public static void GetAppConfigValue(string strKey, ref string strValue)
        {
            strValue = ConfigurationManager.AppSettings[strKey];

            if (strValue == null)
            {
                m_sbErrMessage.AppendLine("Key[" + strKey + "] AppConfigに存在しません。");
            }
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
    }
}