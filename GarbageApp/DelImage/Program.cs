using DelImage.DTO;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;

namespace DelImage
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
        public static string g_strImageRetentionPeriod = string.Empty;

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報クラス
        public static SystemSettingInfo g_clsSystemSettingInfo;

        // システム設定情報取得時のエラーメッセージ格納用
        private static StringBuilder m_sbErrMessage = new StringBuilder();

        // 削除ディレクトリの情報格納用
        private static StringBuilder m_sbDelImageInfo = new StringBuilder();

        // イベントログ出力関連
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int g_CON_LEVEL_FATAL = 1;
        public const int g_CON_LEVEL_ERROR = 2;
        public const int g_CON_LEVEL_WARN = 3;
        public const int g_CON_LEVEL_INFO = 4;
        public const int g_CON_LEVEL_DEBUG = 5;

        // 抽出データ
        private static DataTable m_dtData;
        private static string m_strFolderPath = string.Empty;
        private static string m_strFolderName = string.Empty;

        // 削除件数
        private static int m_intDeleteCont = 0;

        // パラメータ変数
        private static string m_strInspectionDate = string.Empty;
        private static string m_strProductName = string.Empty;
        private static string m_strFabricName = string.Empty;
        private static int m_intInspectionNum = 0;

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
                GetAppConfigValue("ImageRetentionPeriod", ref g_strImageRetentionPeriod);

                if (m_sbErrMessage.Length > 0)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_WARN, string.Format("接続文字列取得時にエラーが発生しました。{0}{1}", Environment.NewLine, m_sbErrMessage.ToString()));

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

                // 保持期間の取得
                DateTime datRetentionPeriod = DateTime.Now.Date.AddMonths(-1 * int.Parse(g_strImageRetentionPeriod));

                m_dtData = new DataTable();

                SelImagecheckerHeader(datRetentionPeriod.ToString("yyyy/MM/dd"), ref m_dtData);

                foreach (DataRow row in m_dtData.Rows)
                {
                    m_strInspectionDate = DateTime.Parse(row["inspection_date"].ToString()).ToString("yyyyMMdd");
                    m_strProductName = row["product_name"].ToString();
                    m_strFabricName = row["fabric_name"].ToString();
                    m_intInspectionNum = int.Parse(row["inspection_num"].ToString());
                    m_strFolderName =
                        string.Join(
                            "_",
                            m_strInspectionDate,
                            m_strProductName,
                            m_strFabricName,
                            m_intInspectionNum);

                    // 検査画像が格納されているフォルダパスを設定
                    m_strFolderPath =
                        Path.Combine(
                            g_clsSystemSettingInfo.strFaultImageDirectory,
                            m_strFolderName);

                    // フォルダパスが存在する場合、削除
                    if (Directory.Exists(m_strFolderPath))
                    {
                        m_sbDelImageInfo.AppendLine(m_strFolderName);
                        Directory.Delete(m_strFolderPath, true);
                        m_intDeleteCont++;
                    }
                }

                if (m_intDeleteCont > 0)
                {
                    WriteEventLog(
                        g_CON_LEVEL_INFO,
                        string.Format(
                            "下記{0}個の検査画像フォルダーを削除しました。{1}{2}",
                            m_intDeleteCont,
                            Environment.NewLine,
                            m_sbDelImageInfo.ToString()));
                }
                else
                {
                    WriteEventLog(g_CON_LEVEL_INFO, "削除対象の検査画像フォルダーはありません。");
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_WARN, string.Format("初期起動時にエラーが発生しました。{0}{1}", Environment.NewLine, ex.Message));

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

        /// <summary>
        /// 検査情報ヘッダ取得
        /// </summary>
        /// <param name="strRetentionPeriod">保存期間</param>
        /// <returns></returns>
        private static void SelImagecheckerHeader(
            string strRetentionPeriod,
            ref DataTable m_dtData)
        {
            try
            {
                // SQL文を作成する
                string strSelectSql = @"SELECT inspection_date, product_name, fabric_name, inspection_num FROM " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                                        WHERE decision_end_datetime IS NOT NULL
                                        AND TO_CHAR(decision_end_datetime,'YYYY/MM/DD') <= :decision_end_datetime_yyyymmdd";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "decision_end_datetime_yyyymmdd", DbType = DbType.String, Value = strRetentionPeriod });

                // sqlを実行する
                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSelectSql, lstNpgsqlCommand);
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_WARN, string.Format("{0}{1}{2}", "検査情報ヘッダーから削除対象情報の取得時に例外が発生しました。", Environment.NewLine, ex.Message));
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }
    }
}