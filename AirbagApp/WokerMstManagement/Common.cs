using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;
using log4net;
using System.Data;

namespace WokerMstManagement
{
    static class Common
    {
        public const int g_CON_EDITMODE_REG = 1;
        public const int g_CON_EDITMODE_UPD = 2;

        public const string g_CON_NAME_SEPARATE = "　";

        /// <summary>
        /// DB非接続モード
        /// </summary>
        /// <remarks>デバッグ用</remarks>
        public static bool g_bolModeNonDBCon = false;  //true:DB接続なし false:DB接続あり

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
        /// 登録・更新処理実行
        /// </summary>
        /// <param name="nscCommand">実行SQLコマンド</param>
        /// <param name="transaction">トランザクション情報</param>
        /// <returns></returns>
        public static Boolean ExecTranSQL(NpgsqlCommand nscCommand
                                        , NpgsqlTransaction transaction)
        {
            try
            {
                nscCommand.ExecuteNonQuery();
                return true;
            }
            catch (NpgsqlException ex)
            {
                transaction.Rollback();
                MessageBox.Show("DB更新時にエラーが発生しました。"
                              + Environment.NewLine
                              + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // DB非接続
            if (args.Length >= 1)
            {
                if (int.Parse(args[0]) > 0) { g_bolModeNonDBCon = true; } else { g_bolModeNonDBCon = false; }
            }

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
            Application.Run(new WokerMstManagement());
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
