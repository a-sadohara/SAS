using log4net;
using System;
using System.Configuration;
using System.Text;
using System.Windows.Forms;
using WokerMstManagement.DTO;

namespace WokerMstManagement
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

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報クラス
        public static SystemSettingInfo g_clsSystemSettingInfo;

        // メッセージ情報クラス
        public static MessageInfo g_clsMessageInfo;

        // システム設定情報取得時のエラーメッセージ格納用
        private static StringBuilder m_sbErrMessage = new StringBuilder();

        // 空白
        public const string g_CON_NAME_SEPARATE = "　";

        // 登録/削除
        public const int g_CON_EDITMODE_REG = 1;
        public const int g_CON_EDITMODE_UPD = 2;

        // イベントログ出力関連
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int g_CON_LEVEL_FATAL = 1;
        public const int g_CON_LEVEL_ERROR = 2;
        public const int g_CON_LEVEL_WARN = 3;
        public const int g_CON_LEVEL_INFO = 4;
        public const int g_CON_LEVEL_DEBUG = 5;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Mutex名を決める
            string mutexName = "WokerMstManagement";
            // Mutexオブジェクトを作成する
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName);

            bool hasHandle = false;
            try
            {
                try
                {
                    // ミューテックスの所有権を要求する
                    hasHandle = mutex.WaitOne(0, false);
                }
                // .NET Framework 2.0以降の場合
                catch (System.Threading.AbandonedMutexException)
                {
                    // 別のアプリケーションがミューテックスを解放しないで終了した時
                    hasHandle = true;
                }
                // ミューテックスを得られたか調べる
                if (hasHandle == false)
                {
                    //得られなかった場合は、すでに起動していると判断して終了
                    MessageBox.Show("多重起動はできません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    // 接続文字列をApp.configファイルから取得
                    GetAppConfigValue("DBName", ref g_strDBName);
                    GetAppConfigValue("DBUser", ref g_strDBUser);
                    GetAppConfigValue("DBUserPassword", ref g_strDBUserPassword);
                    GetAppConfigValue("DBServerName", ref g_strDBServerName);
                    GetAppConfigValue("DBPort", ref g_strDBPort);

                    if (m_sbErrMessage.Length > 0)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("接続文字列取得時にエラーが発生しました。{0}{1}" ,Environment.NewLine, m_sbErrMessage.ToString()));
                        // メッセージ出力
                        System.Windows.Forms.MessageBox.Show("接続文字列取得時に例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
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
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format( "初期起動時にエラーが発生しました。{0}{1}" ,Environment.NewLine, ex.Message));
                    // メッセージ出力
                    System.Windows.Forms.MessageBox.Show("初期起動時に例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
                Application.Run(new WokerMstManagement());
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
        /// NULLを""に変換
        /// </summary>
        /// <param name="objNValue"></param>
        /// <returns></returns>
        public static string NulltoString(object objNValue)
        {
            if (objNValue == null)
            {
                return "";
            }
            else if (objNValue.ToString() == "")
            {
                return "";
            }
            else
            {
                return objNValue.ToString();
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
                m_sbErrMessage.AppendLine(string.Format("Key[{0}] AppConfigに存在しません。" , strKey ));
            }
        }
    }
}
