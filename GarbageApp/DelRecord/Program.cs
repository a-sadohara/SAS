using DelRecord.DTO;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;

namespace DelRecord
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
        public static string g_strDBRetentionPeriod = string.Empty;

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報クラス
        public static SystemSettingInfo g_clsSystemSettingInfo;

        // システム設定情報取得時のエラーメッセージ格納用
        private static StringBuilder m_sbErrMessage = new StringBuilder();

        // 削除時レコードの情報格納用
        private static StringBuilder m_sbDelRecordInfo = new StringBuilder();

        // イベントログ出力関連
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int g_CON_LEVEL_FATAL = 1;
        public const int g_CON_LEVEL_ERROR = 2;
        public const int g_CON_LEVEL_WARN = 3;
        public const int g_CON_LEVEL_INFO = 4;
        public const int g_CON_LEVEL_DEBUG = 5;

        // 抽出データ
        private static DataTable m_dtData;

        // 削除件数
        private static int m_intDeleteCont_PublicHeader = 0;
        private static int m_intDeleteCont_ImagecheckerHeader = 0;
        private static int m_intDeleteCont_ImagecheckerResult = 0;

        // パラメータ変数
        private static int m_intInspectionNum = 0;
        private static int m_intBranchNum = 0;
        private static string m_strUnitNum = string.Empty;
        private static string m_strInspectionDate = string.Empty;
        private static string m_strOrder_img = string.Empty;
        private static string m_strFabric_name = string.Empty;
        private static string m_strProduct_name = string.Empty;

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
                GetAppConfigValue("DBRetentionPeriod", ref g_strDBRetentionPeriod);

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

                string strRetentionPeriod = DateTime.Now.Date.AddMonths(-1 * int.Parse(g_strDBRetentionPeriod)).ToString("yyyy/MM/dd");
                m_dtData = new DataTable();

                SelImagecheckerHeader(strRetentionPeriod, ref m_dtData);

                foreach (DataRow row in m_dtData.Rows)
                {
                    m_intInspectionNum = int.Parse(row["inspection_num"].ToString());
                    m_intBranchNum = int.Parse(row["branch_num"].ToString());
                    m_strUnitNum = row["unit_num"].ToString();
                    m_strInspectionDate = DateTime.Parse(row["inspection_date"].ToString()).ToString("yyyy/MM/dd");
                    m_strOrder_img = row["order_img"].ToString();
                    m_strFabric_name = row["fabric_name"].ToString();
                    m_strProduct_name = row["product_name"].ToString();
                    DelImagecheckerHeader();
                    DelPublicHeader();
                    DelImagecheckerResult();

                    m_sbDelRecordInfo.AppendLine(string.Format(
                        "検査日付:{0}, {1}号機, 検査番号:{2}, 枝番:{3}, 品名:{4}, 指図:{5}, 反番:{6}",
                        m_strInspectionDate,
                        m_strUnitNum,
                        m_intInspectionNum,
                        m_intBranchNum,
                        m_strProduct_name,
                        m_strOrder_img,
                        m_strFabric_name));
                }

                if (m_intDeleteCont_PublicHeader > 0 ||
                    m_intDeleteCont_ImagecheckerHeader > 0 ||
                    m_intDeleteCont_ImagecheckerResult > 0)
                {
                    WriteEventLog(
                        g_CON_LEVEL_INFO,
                        string.Format(
                            "下記情報のレコードを削除しました。{0}・public.inspection_info_header:{1}件{2}・imagecheckerschema.inspection_info_header:{3}件{4}・imagecheckerschema.decision_result:{5}件{6}{7}{8}",
                            Environment.NewLine,
                            m_intDeleteCont_PublicHeader,
                            Environment.NewLine,
                            m_intDeleteCont_ImagecheckerHeader,
                            Environment.NewLine,
                            m_intDeleteCont_ImagecheckerResult,
                            Environment.NewLine,
                            Environment.NewLine,
                            m_sbDelRecordInfo.ToString()));
                }
                else
                {
                    WriteEventLog(g_CON_LEVEL_INFO, "削除対象のレコードはありません。");
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
                string strSelectSql = @"SELECT inspection_num, branch_num, unit_num, inspection_date, order_img, fabric_name, product_name FROM " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                                        WHERE decision_end_datetime IS NOT NULL
                                        AND TO_CHAR(decision_end_datetime,'YYYY/MM/DD') <= :decision_end_datetime_yyyymmdd
                                        ORDER BY inspection_date, unit_num, inspection_num, branch_num";

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

        /// <summary>
        /// 検査情報ヘッダ削除
        /// </summary>
        /// <returns></returns>
        private static void DelPublicHeader()
        {
            try
            {
                int intDeleteCont = 0;

                // SQL文を作成する
                string strDeleteSql = @"DELETE FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".inspection_info_header 
                                        WHERE inspection_num = :inspection_num 
                                        AND branch_num = :branch_num 
                                        AND unit_num = :unit_num 
                                        AND TO_CHAR(inspection_date, 'YYYY/MM/DD') = :inspection_date_yyyymmdd";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = m_intBranchNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });

                // sqlを実行する
                intDeleteCont = g_clsConnectionNpgsql.ExecTranSQL(strDeleteSql, lstNpgsqlCommand);

                // 削除件数を加算する
                if (intDeleteCont > 0)
                {
                    m_intDeleteCont_PublicHeader += intDeleteCont;
                }

                g_clsConnectionNpgsql.DbCommit();
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_WARN, string.Format("{0}{1}{2}", "検査情報ヘッダーの削除処理で例外が発生しました。", Environment.NewLine, ex.Message));
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 検査情報ヘッダ削除
        /// </summary>
        /// <returns></returns>
        private static void DelImagecheckerHeader()
        {
            try
            {
                int intDeleteCont = 0;

                // SQL文を作成する
                string strDeleteSql = @"DELETE FROM " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header 
                                        WHERE inspection_num = :inspection_num 
                                        AND branch_num = :branch_num 
                                        AND unit_num = :unit_num 
                                        AND TO_CHAR(inspection_date, 'YYYY/MM/DD') = :inspection_date_yyyymmdd";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = m_intBranchNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });

                // sqlを実行する
                intDeleteCont = g_clsConnectionNpgsql.ExecTranSQL(strDeleteSql, lstNpgsqlCommand);

                // 削除件数を加算する
                if (intDeleteCont > 0)
                {
                    m_intDeleteCont_ImagecheckerHeader += intDeleteCont;
                }

                g_clsConnectionNpgsql.DbCommit();
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_WARN, string.Format("{0}{1}{2}", "検査情報ヘッダーの削除処理で例外が発生しました。", Environment.NewLine, ex.Message));
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 合否判定結果削除
        /// </summary>
        /// <returns></returns>
        private static void DelImagecheckerResult()
        {
            try
            {
                int intDeleteCont = 0;

                // SQL文を作成する
                string strDeleteSql = @"DELETE FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result  
                                        WHERE inspection_num = :inspection_num 
                                        AND branch_num = :branch_num 
                                        AND TO_CHAR(inspection_date, 'YYYY/MM/DD') = :inspection_date_yyyymmdd";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = m_intBranchNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });

                // sqlを実行する
                intDeleteCont = g_clsConnectionNpgsql.ExecTranSQL(strDeleteSql, lstNpgsqlCommand);

                // 削除件数を加算する
                if (intDeleteCont > 0)
                {
                    m_intDeleteCont_ImagecheckerResult += intDeleteCont;
                }

                g_clsConnectionNpgsql.DbCommit();
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_WARN, string.Format("{0}{1}{2}", "合否判定結果の削除処理で例外が発生しました。", Environment.NewLine, ex.Message));
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }
    }
}