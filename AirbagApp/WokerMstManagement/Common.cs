using log4net;
using System;
using System.Configuration;
using System.Windows.Forms;
using WokerMstManagement.DTO;

namespace WokerMstManagement
{
    static class Common
    {
        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報クラス
        public static SystemSettingInfo g_clsSystemSettingInfo;

        // メッセージ情報クラス
        public static MessageInfo g_clsMessageInfo;

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
        static void Main(string[] args)
        {
            try
            {
                // 接続文字列をApp.configファイルから取得
                ConnectionStringSettings setConStr = null;
                setConStr = ConfigurationManager.ConnectionStrings["ConnectionString"];
                if (setConStr == null)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, "接続文字列取得時にエラーが発生しました。\r\nConnectionStringが存在しません。");
                    // メッセージ出力
                    System.Windows.Forms.MessageBox.Show("接続文字列取得時に例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                else
                {
                    g_clsConnectionNpgsql = new ConnectionNpgsql(setConStr.ConnectionString);
                    g_clsConnectionNpgsql.DbOpen();
                    g_clsConnectionNpgsql.DbClose();
                }

                // システム設定情報取得
                g_clsSystemSettingInfo = new SystemSettingInfo();
                if (g_clsSystemSettingInfo.bolNormalEnd == false)
                    return;

                // メッセージ情報取得
                g_clsMessageInfo = new MessageInfo();
                if (g_clsMessageInfo.bolNormalEnd == false)
                    return;
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
                g_clsConnectionNpgsql.DbClose();
            }

            // フォーム画面を起動
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WokerMstManagement());
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
    }
}
