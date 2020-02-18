using ImageChecker.DTO;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ImageChecker
{
    static class Common
    {
        // 接続情報情報
        public static string g_strConnectionString = "";
        private const string g_CON_CONNECTION_STRING = "Server={0};Port={1};User ID={2};Database={3};Password={4};Enlist=true";
        public static string g_strDBName = "";
        public static string g_strDBUser = "";
        public static string g_strDBUserPassword = "";
        public static string g_strDBServerName = "";
        public static string g_strDBPort = "";

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報クラス
        public static SystemSettingInfo g_clsSystemSettingInfo;

        // メッセージ情報クラス
        public static MessageInfo g_clsMessageInfo;

        // ログイン情報クラス
        public static LoginInfo g_clsLoginInfo;

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

        // NG理由（主要）
        public const string g_CON_NG_REASON_OK = "過検知";
        public const string g_CON_NG_REASON_WHITE_THREAD_ONE = "□結節有（白糸上単発）";
        public const string g_CON_NG_REASON_WHITE_THREAD_MULTI = "□結節有（白糸上連続）";
        public const string g_CON_NG_REASON_BLACK_THREAD_ONE = "□結節有（黒糸上単発）";
        public const string g_CON_NG_REASON_BLACK_THREAD_MULTI = "□結節有（黒糸上連続）";
        public const string g_CON_NG_REASON_OTHER_NG_JUDGEMENT = "●他画像でNG判定済み";

        // ログイン関連
        public static bool g_bolStatus = false; //0:ログアウト 1:ログイン

        // 一時サブディレクトリ
        public const string g_CON_DIR_MASTER_IMAGE = "MasterImage";                 // マスタ画像格納
        public const string g_CON_DIR_MASTER_IMAGE_MARKING = "MasterImageMarking";  // マーキングマスタ画像格納先

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Mutex名を設定する
            string mutexName = "ImageChecker";

            // Mutexオブジェクトを作成する
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName);

            bool hasHandle = false;

            try
            {
                try
                {
                    // Mutexの所有権を要求する
                    hasHandle = mutex.WaitOne(0, false);
                }
                catch (System.Threading.AbandonedMutexException)
                {
                    // 別のアプリケーションがMutexを解放せず終了した場合、変数を更新する
                    hasHandle = true;
                }

                if (!hasHandle)
                {
                    MessageBox.Show("多重起動はできません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 接続文字列をApp.configファイルから取得
                GetAppConfigValue("DBName", ref g_strDBName);
                GetAppConfigValue("DBUser", ref g_strDBUser);
                GetAppConfigValue("DBUserPassword", ref g_strDBUserPassword);
                GetAppConfigValue("DBServerName", ref g_strDBServerName);
                GetAppConfigValue("DBPort", ref g_strDBPort);

                if (m_sbErrMessage.Length > 0)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, "接続文字列取得時にエラーが発生しました。\r\n" + m_sbErrMessage.ToString());
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
                    return;

                // メッセージ情報取得
                g_clsMessageInfo = new MessageInfo();
                if (g_clsMessageInfo.bolNormalEnd == false)
                    return;

                // ログイン情報インスタンス生成
                g_clsLoginInfo = new LoginInfo();
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "初期起動時にエラーが発生しました。" + "\r\n" + ex.Message);
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
            Application.Run(new Login());
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

        ///// <summary>
        ///// 過検知除外ステータス更新
        ///// </summary>
        ///// <param name="strFabricName">反番</param>
        ///// <param name="strInspectionDate">検査日付</param>
        ///// <param name="intInspectionNum">検査番号</param>
        ///// <param name="intStatus">ステータス</param>
        ///// <returns></returns>
        //public static Boolean blnGetOverDetectionExceptStatus(string strFabricName,
        //                                                      string strInspectionDate,
        //                                                      int intInspectionNum,
        //                                                      ref int intStatus)
        //{
        //    string strSQL = "";
        //    try
        //    {
        //        // SQL文を作成する
        //        strSQL = @"SELECT over_detection_except_status 
        //                     FROM " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
        //                    WHERE fabric_name = :fabric_name
        //                      AND TO_CHAR(inspection_date,'YYYYMMDD') = :inspection_date_yyyymmdd
        //                      AND inspection_num = :inspection_num";

        //        // SQLコマンドに各パラメータを設定する
        //        List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
        //        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
        //        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate.Replace("/", "") });
        //        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
        //        // sqlを実行する
        //        g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (bolMessageInfoContent("E9901", ref m_strMessageInfoContent_Eve) == false ||
        //            bolMessageInfoContent("E0044", ref m_strMessageInfoContent_Msb) == false)
        //            return false;

        //        // ログ出力
        //        WriteEventLog(g_CON_LEVEL_ERROR, m_strMessageInfoContent_Eve + "\r\n" + ex.Message);
        //        // メッセージ出力
        //        System.Windows.Forms.MessageBox.Show(m_strMessageInfoContent_Msb, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

        //        return false;
        //    }
        //}

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
