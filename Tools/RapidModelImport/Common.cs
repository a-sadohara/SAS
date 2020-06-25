using log4net;
using RapidModelImport.DTO;
using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RapidModelImport
{
    static class Common
    {
        // DB接続文字列
        private const string g_CON_CONNECTION_STRING = "Server={0};Port={1};User ID={2};Database={3};Password={4};Enlist=true";
        public static string g_strConnectionString = string.Empty;
        public static string g_strDBName = string.Empty;
        public static string g_strDBUser = string.Empty;
        public static string g_strDBUserPassword = string.Empty;
        public static string g_strDBServerName = string.Empty;
        public static string g_strDBPort = string.Empty;
        public static string g_strBatchPath = string.Empty;
        public static string g_strINIFilePath = string.Empty;

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // メッセージ情報クラス
        public static MessageInfo g_clsMessageInfo;

        // システム設定情報取得時のエラーメッセージ格納用
        private static StringBuilder m_sbErrMessage = new StringBuilder();

        // イベントログ出力関連
        public static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int g_CON_LEVEL_FATAL = 1;
        public const int g_CON_LEVEL_ERROR = 2;
        public const int g_CON_LEVEL_WARN = 3;
        public const int g_CON_LEVEL_INFO = 4;
        public const int g_CON_LEVEL_DEBUG = 5;

        // メッセージ関連
        public const string g_CON_MESSAGE_TITLE_ERROR = "エラー";
        public const string g_CON_MESSAGE_TITLE_WARN = "警告";
        public const string g_CON_MESSAGE_TITLE_QUESTION = "確認";


        // INIファイル関連
        private const string m_CON_INI_SECTION_DBConnectionInfo = "DBConnectionInfo";
        private const string m_CON_INI_SECTION_Path = "Path";
        private const string m_CON_INI_ITEM_DBName = "DBName";
        private const string m_CON_INI_ITEM_DBUser = "DBUser";
        private const string m_CON_INI_ITEM_DBUserPassword = "DBUserPassword";
        private const string m_CON_INI_ITEM_DBServerName = "DBServerName";
        private const string m_CON_INI_ITEM_DBPort = "DBPort";
        private const string m_CON_INI_ITEM_BatchFilePath = "BatchFilePath";

        #region ラッパー
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
            string lpApplicationName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedstring,
            int nSize,
            string lpFileName);

        private static string GetIniValue(
            string path,
            string section,
            string key)
        {
            StringBuilder sb = new StringBuilder(256);

            GetPrivateProfileString(
                section,
                key,
                string.Empty,
                sb,
                sb.Capacity,
                path);

            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Mutex名を決める
            string mutexName = "RapidModelImport";

            // Mutexオブジェクトを作成する
            Mutex mutex = new Mutex(false, mutexName);

            bool hasHandle = false;

            try
            {
                try
                {
                    // ミューテックスの所有権を要求する
                    hasHandle = mutex.WaitOne(0, false);
                }
                catch (AbandonedMutexException)
                {
                    // 別のアプリケーションがミューテックスを解放しないで終了した時の考慮
                    hasHandle = true;
                }

                if (!hasHandle)
                {
                    // すでに起動していると判断して終了する
                    MessageBox.Show(
                        "多重起動はできません。",
                        g_CON_MESSAGE_TITLE_ERROR,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                try
                {
                    // Config情報を取得
                    GetAppConfigValue("INIFilePath", ref g_strINIFilePath);

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

                    try
                    {
                        // INI情報を取得
                        g_strDBName =
                            NulltoString(
                                GetIniValue(
                                    g_strINIFilePath,
                                    m_CON_INI_SECTION_DBConnectionInfo,
                                    m_CON_INI_ITEM_DBName));

                        g_strDBUser =
                            NulltoString(
                                GetIniValue(
                                    g_strINIFilePath,
                                    m_CON_INI_SECTION_DBConnectionInfo,
                                    m_CON_INI_ITEM_DBUser));

                        g_strDBUserPassword =
                            NulltoString(
                                GetIniValue(
                                    g_strINIFilePath,
                                    m_CON_INI_SECTION_DBConnectionInfo,
                                    m_CON_INI_ITEM_DBUserPassword));

                        g_strDBServerName =
                            NulltoString(
                                GetIniValue(
                                    g_strINIFilePath,
                                    m_CON_INI_SECTION_DBConnectionInfo,
                                    m_CON_INI_ITEM_DBServerName));

                        g_strDBPort =
                            NulltoString(
                                GetIniValue(
                                    g_strINIFilePath,
                                    m_CON_INI_SECTION_DBConnectionInfo,
                                    m_CON_INI_ITEM_DBPort));

                        g_strBatchPath =
                            NulltoString(
                                GetIniValue(
                                    g_strINIFilePath,
                                    m_CON_INI_SECTION_Path,
                                    m_CON_INI_ITEM_BatchFilePath));

                        g_strConnectionString =
                            string.Format(
                                g_CON_CONNECTION_STRING,
                                g_strDBServerName,
                                g_strDBPort,
                                g_strDBUser,
                                g_strDBName,
                                g_strDBUserPassword);
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(
                            g_CON_LEVEL_ERROR,
                            string.Format(
                                "INI情報取得時にエラーが発生しました。{0}{1}",
                                Environment.NewLine,
                                ex.Message));

                        // メッセージ出力
                        MessageBox.Show(
                            "INI情報取得時に例外が発生しました。",
                            g_CON_MESSAGE_TITLE_ERROR,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    // 接続確認
                    g_clsConnectionNpgsql = new ConnectionNpgsql(g_strConnectionString);
                    g_clsConnectionNpgsql.DbOpen();
                    g_clsConnectionNpgsql.DbClose();

                    // メッセージ情報取得
                    g_clsMessageInfo = new MessageInfo();

                    if (!g_clsMessageInfo.bolNormalEnd)
                    {
                        return;
                    }
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

                // フォーム画面を起動
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ImportScreen());
            }
            finally
            {
                if (hasHandle)
                {
                    // ミューテックスを解放する
                    mutex.ReleaseMutex();
                }

                mutex.Close();
            }
        }

        /// <summary>
        /// イベントログ出力
        /// </summary>
        /// <param name="intLevel">レベル</param>
        /// <param name="strMessage">メッセージ</param>
        public static void WriteEventLog(
            int intLevel,
            string strMessage)
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
        private static void GetAppConfigValue(
            string strKey,
            ref string strValue)
        {
            strValue = ConfigurationManager.AppSettings[strKey];

            if (string.IsNullOrWhiteSpace(strValue))
            {
                m_sbErrMessage.AppendLine(
                    string.Format(
                        "Key[{0}] AppConfigに存在しません。",
                        strKey));
            }
        }

        /// <summary>
        /// NULLを""に変換
        /// </summary>
        /// <param name="objNValue"></param>
        /// <returns></returns>
        public static string NulltoString(object objNValue)
        {
            if (objNValue == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(objNValue.ToString()))
            {
                return string.Empty;
            }

            return objNValue.ToString();
        }
    }
}