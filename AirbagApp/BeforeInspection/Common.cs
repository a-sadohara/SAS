using log4net;
using Npgsql;
using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;

namespace BeforeInspection
{
    static class Common
    {
        // DB接続文字列
        public static string g_ConnectionString;
        // DBコネクションオブジェクト
        public static NpgsqlConnection NpgsqlCon;
        // DBトランザクションオブジェクト
        public static NpgsqlTransaction NpgsqlTran;

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
            // 接続文字列をApp.configファイルから取得
            ConnectionStringSettings setConStr = null;
            setConStr = ConfigurationManager.ConnectionStrings["ConnectionString"];
            if (setConStr == null)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "App.configの[ConnectionString]の設定値取得時にエラーが発生しました。");
                // メッセージ出力
                // 接続文字列取得エラー
                MessageBox.Show("App.configの[ConnectionString]の設定値取得に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                NpgsqlCon = new NpgsqlConnection(setConStr.ConnectionString);

                DbOpen();
            }
            g_ConnectionString = setConStr.ConnectionString;

            // フォーム画面を起動
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BeforeInspection() { Visible = false });
        }

        /// <summary>
        /// DBオープン
        /// </summary>
        /// <example>既存のDBオープンが存在しない場合、DBオープンを実施する</example>
        public static void DbOpen()
        {
            try
            {
                if (NpgsqlCon.FullState != System.Data.ConnectionState.Open)
                    NpgsqlCon.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DBトランザクション開始
        /// </summary>
        /// <example>既存のDBトランザクションがない場合、DBトランザクション開始を実施する</example>
        public static void DbBeginTran()
        {
            try
            {
                if (NpgsqlTran == null || NpgsqlTran.IsCompleted == true)
                    NpgsqlTran = NpgsqlCon.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DBコミット
        /// </summary>
        /// <example>DBトランザクション開始されている場合、DBコミットを実施する</example>
        public static void DbCommit()
        {
            try
            {
                if (NpgsqlTran != null && NpgsqlTran.IsCompleted == false)
                    NpgsqlTran.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DBロールバック
        /// </summary>
        /// <example>DBトランザクション開始されている場合、DBロールバックを実施する</example>
        public static void DbRollback()
        {
            try
            {
                if (NpgsqlTran != null && NpgsqlTran.IsCompleted == false)
                    NpgsqlTran.Rollback();
            }
            catch (Exception ex)
            {
                throw ex;
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
        /// 登録・更新処理実行
        /// </summary>
        /// <param name="nscCommand">実行SQLコマンド</param>
        /// <param name="transaction">トランザクション情報</param>
        /// <returns></returns>
        public static Boolean ExecTranSQL(NpgsqlCommand nscCommand)
        {
            try
            {
                nscCommand.ExecuteNonQuery();
                return true;
            }
            catch (NpgsqlException ex)
            {
                DbRollback();
                MessageBox.Show("DB更新時にエラーが発生しました。"
                              + Environment.NewLine
                              + ex.Message);
                return false;
            }
        }

        public static bool bolGetSystemSettingValue(string strId, out string strValue)
        {
            string strSQL = "";
            DataTable dtData;
            string strGetValue = "";

            try
            {
                // SQL抽出から情報を取得
                DbOpen();

                NpgsqlCommand NpgsqlCom = null;
                NpgsqlDataAdapter NpgsqlDtAd = null;
                dtData = new DataTable();
                strSQL = @"SELECT value FROM system_setting_info WHERE id = '" + strId + "'; ";
                NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                NpgsqlDtAd.Fill(dtData);

                //  検査番号
                strGetValue = dtData.Rows[0]["value"].ToString();

                return true;
            }
            catch (NpgsqlException ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("システム設定の取得で例外が発生しました。", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
