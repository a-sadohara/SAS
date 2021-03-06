﻿using BeforeInspection.DTO;
using log4net;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BeforeInspection
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

        // システム情報設定関連
        public static DataTable m_dtSystemSettingValue = new DataTable();

        // イベントログ出力関連
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int g_CON_LEVEL_FATAL = 1;
        public const int g_CON_LEVEL_ERROR = 2;
        public const int g_CON_LEVEL_WARN = 3;
        public const int g_CON_LEVEL_INFO = 4;
        public const int g_CON_LEVEL_DEBUG = 5;

        // コンフィグ情報
        public static string g_strConnectionPoint = string.Empty;
        public static string g_strConnectionUser = string.Empty;
        public static string g_strConnectionPassword = string.Empty;
        public static string[] g_strTaskName;
        public static string[] g_strExecutionFileName;
        public static int[] g_intFabricInfoExceptExtractionStatus;
        public static int g_intRetryTimes = 0;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Mutex名を設定する
            string mutexName = "BeforeInspection";

            // Mutexオブジェクトを作成する
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName);

            bool hasHandle = false;
            string strConnectionPoint = string.Empty;
            string strConnectionUser = string.Empty;
            string strConnectionPassword = string.Empty;
            string strFabricInfoExceptExtractionStatus = string.Empty;
            string strTaskName = string.Empty;
            string strExecutionFileName = string.Empty;

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
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("接続文字列取得時にエラーが発生しました。{0}{1}", Environment.NewLine, m_sbErrMessage.ToString()));
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

                // タッチキーボードのプロセスをキル
                System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process myProcess in myProcesses)
                {
                    if ("TabTip" == myProcess.ProcessName)
                    {
                        myProcess.Kill();
                    }
                }

                GetAppConfigValue("ConnectionPoint", ref strConnectionPoint);
                GetAppConfigValue("ConnectionUser", ref strConnectionUser);
                GetAppConfigValue("ConnectionPassword", ref strConnectionPassword);
                GetAppConfigValue("FabricInfoExceptExtractionStatus", ref strFabricInfoExceptExtractionStatus);
                GetAppConfigValue("TaskName", ref strTaskName);
                GetAppConfigValue("ExecutionFileName", ref strExecutionFileName);
                GetAppConfigValue("RetryTimes", ref g_intRetryTimes);

                // 連携基盤部接続接続用の情報を取得する。
                if (!string.IsNullOrWhiteSpace(strConnectionPoint))
                {
                    g_strConnectionPoint = string.Format("/S {0}", strConnectionPoint);
                }

                if (!string.IsNullOrWhiteSpace(strConnectionUser))
                {
                    g_strConnectionUser = string.Format("/U {0}", strConnectionUser);
                }

                if (!string.IsNullOrWhiteSpace(strConnectionPassword))
                {
                    g_strConnectionPassword = string.Format("/P {0}", strConnectionPassword);
                }

                // コンフィグ情報より「反物情報.ステータス_抽出除外条件」を取得する。
                g_intFabricInfoExceptExtractionStatus = strFabricInfoExceptExtractionStatus.Split(',').Select(x => int.Parse(x)).OrderBy(x => x).ToArray();

                // コンフィグ情報より「起動対象タスク名」を取得する。
                g_strTaskName = strTaskName.Split(',').OrderBy(x => x).ToArray();

                // コンフィグ情報より「起動対象タスク名」を取得する。
                g_strExecutionFileName = strExecutionFileName.Split(',').OrderBy(x => x).ToArray();

            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("初期起動時にエラーが発生しました。{0}{1}", Environment.NewLine, ex.Message));
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
            Application.Run(new BeforeInspection());
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
                m_sbErrMessage.AppendLine(string.Format("Key[{0}] AppConfigに存在しません。", strKey));
            }
        }

        /// <summary>
        /// App.configファイルから設定値を取得
        /// </summary>
        /// <param name="strKey">キー</param>
        /// <param name="intValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private static void GetAppConfigValue(string strKey, ref int intValue)
        {
            string strValue = string.Empty;

            try
            {
                GetAppConfigValue(strKey, ref strValue);
                intValue = int.Parse(strValue);
            }
            catch (Exception ex)
            {
                m_sbErrMessage.AppendLine(string.Format("Key[{0}] {1}", strKey, ex.Message));
            }
        }
    }
}