using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using log4net;
using System.Data;
using Npgsql;

namespace ImageChecker
{
    static class Common
    {
        /// <summary>
        /// DB非接続モード
        /// </summary>
        /// <remarks>デバッグ用</remarks>
        public static bool bolModeNonDBCon = false;  //true:DB接続なし false:DB接続あり

        public static String g_parUserNo = "";
        public static String g_parUserNm = "";
        public static short g_parDispNum;
        public static short g_parStatus;  //0:ログアウト 1:ログイン

        // DB接続文字列
        public static string g_ConnectionString;

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
            string strUserNo = "";
            short shoDispNum = 0;

            // DB非接続
            if (args.Length >= 1)
            {
                if (int.Parse(args[0]) > 0) { bolModeNonDBCon = true; } else { bolModeNonDBCon = false; }
            }
            // 職員番号
            if (args.Length >= 2) strUserNo = args[1];
            // 表示数
            if (args.Length >= 3) shoDispNum = (short)int.Parse(args[2]);

            try
            {
                // 接続文字列をApp.configファイルから取得
                ConnectionStringSettings setConStr = null;
                setConStr = ConfigurationManager.ConnectionStrings["ConnectionString"];
                if (setConStr == null)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, "App.configの[ConnectionString]の設定値取得時にエラーが発生しました。");
                    // メッセージ出力
                    MessageBox.Show("App.configの[ConnectionString]の設定値取得に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                else
                {
                    // TODO ★接続確認ロジックを付与★
                }
                g_ConnectionString = setConStr.ConnectionString;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "画面起動時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("画面起動処理に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm(strUserNo, shoDispNum));
        }

        /// <summary>
        /// App.configファイルから設定値を取得
        /// </summary>
        /// <param name="strName">要素名</param>
        /// <returns>null:設定なし(取得失敗) それ以外:設定あり(取得成功)</returns>
        public static string strGetAppConfigValue(string strName)
        {
            string strValue = ConfigurationManager.AppSettings[strName];
            if (strValue == null)
                MessageBox.Show("App.configの[" + strName + "]の設定値取得に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return strValue;
        }

        /// <summary>
        /// システム設定情報から設定値を取得
        /// </summary>
        /// <param name="strId">ID</param>
        /// <param name="strValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        public static bool bolGetSystemSettingValue(string strId, out string strValue)
        {
            string strSQL = "";
            DataTable dtData;
            string strGetValue = "";

            try
            {
                // SQL抽出から情報を取得
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
                {
                    NpgsqlCon.Open();

                    NpgsqlCommand NpgsqlCom = null;
                    NpgsqlDataAdapter NpgsqlDtAd = null;
                    dtData = new DataTable();
                    strSQL = @"SELECT value FROM system_setting_info WHERE id = '" + strId + "'; ";
                    NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                    NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                    NpgsqlDtAd.Fill(dtData);

                    //  検査番号
                    strGetValue = dtData.Rows[0]["value"].ToString();
                }

                return true;
            }
            catch (NpgsqlException ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("システム設定情報の取得で例外が発生しました。", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            finally
            {
                strValue = strGetValue;
            }
        }

        /// <summary>
        /// ログイン処理
        /// </summary>
        public static void LogIn()
        {
            g_parStatus = 1;
        }

        /// <summary>
        /// ログアウト処理
        /// </summary>
        public static void LogOut()
        {
            g_parStatus = 0;

            // 開いている画面ループ
            foreach (Form frm in Application.OpenForms)
            {
                if (frm is LoginForm)
                {
                    // ログイン画面　⇒　再表示
                    frm.Visible = true;
                }
                else
                {
                    // ログイン画面以外　⇒　閉じる
                    frm.Close();
                }
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
