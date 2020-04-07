﻿using ImageChecker.DTO;
using log4net;
using SevenZipNET;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageChecker
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
        public static string g_strSharedFolderPath = string.Empty;
        public static string g_strSharedFolderUser = string.Empty;
        public static string g_strSharedFolderPassword = string.Empty;
        public static string g_CON_SHARED_FOLDER_CONNECTION_STRING = @" use {0} /user:{1} {2}";

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報クラス
        public static SystemSettingInfo g_clsSystemSettingInfo;

        // メッセージ情報クラス
        public static MessageInfo g_clsMessageInfo;

        // ログイン情報クラス
        public static LoginInfo g_clsLoginInfo;

        // 帳票情報クラス
        public static ReportInfo g_clsReportInfo;

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

        // メッセージ関連
        public const string g_CON_MESSAGE_TITLE_ERROR = "エラー";
        public const string g_CON_MESSAGE_TITLE_WARN = "警告";
        public const string g_CON_MESSAGE_TITLE_QUESTION = "確認";

        // NG理由（主要）
        public const string g_CON_NG_REASON_OK = "過検知";
        public const string g_CON_NG_REASON_WHITE_THREAD_ONE = "□結節有(白糸上単発)";
        public const string g_CON_NG_REASON_WHITE_THREAD_MULTI = "□結節有(白糸上連続)";
        public const string g_CON_NG_REASON_BLACK_THREAD_ONE = "□結節有(黒糸上単発)";
        public const string g_CON_NG_REASON_BLACK_THREAD_MULTI = "□結節有(黒糸上連続)";
        public const string g_CON_NG_REASON_OTHER_NG_JUDGEMENT = "□他画像でNG判定済み";

        // 号機
        public const string g_strUnitNumN1 = "N1";
        public const string g_strUnitNumN2 = "N2";
        public const string g_strUnitNumN3 = "N3";
        public const string g_strUnitNumN4 = "N4";

        // ログイン関連
        public static bool g_bolStatus = false; //0:ログアウト 1:ログイン

        // サブ一時ディレクトリ
        public const string g_CON_DIR_MASTER_IMAGE = "MasterImage";                         // マスタ画像格納先
        public const string g_CON_DIR_MASTER_IMAGE_MARKING = "MasterImageMarking";          // マーキングマスタ画像格納先
        public const string g_CON_ZIP_EXTRACT_DIR_PATH = "ZipExtractDirPath";               // ZIP解凍用格納先
        public static string g_strMasterImageDirPath = string.Empty;
        public static string g_strMasterImageDirMarking = string.Empty;
        public static string g_strZipExtractDirPath = string.Empty;

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

            bool hasConnection = false;
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
                    MessageBox.Show("多重起動はできません。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 接続文字列をApp.configファイルから取得
                GetAppConfigValue("DBName", ref g_strDBName);
                GetAppConfigValue("DBUser", ref g_strDBUser);
                GetAppConfigValue("DBUserPassword", ref g_strDBUserPassword);
                GetAppConfigValue("DBServerName", ref g_strDBServerName);
                GetAppConfigValue("DBPort", ref g_strDBPort);
                GetAppConfigValue("SharedFolderPath", ref g_strSharedFolderPath);
                GetAppConfigValue("SharedFolderUser", ref g_strSharedFolderUser);
                GetAppConfigValue("SharedFolderPassword", ref g_strSharedFolderPassword);

                if (m_sbErrMessage.Length > 0)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("接続文字列取得時にエラーが発生しました。{0}{1}", Environment.NewLine, m_sbErrMessage.ToString()));
                    // メッセージ出力
                    System.Windows.Forms.MessageBox.Show("接続文字列取得時に例外が発生しました。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                if (!string.IsNullOrWhiteSpace(g_strSharedFolderPath) &&
                    !Directory.Exists(g_strSharedFolderPath))
                {
                    try
                    {
                        // 共有フォルダ接続
                        using (Process prNet = new Process())
                        {
                            prNet.StartInfo.FileName = "net.exe";
                            prNet.StartInfo.Arguments =
                                string.Format(
                                    g_CON_SHARED_FOLDER_CONNECTION_STRING,
                                    g_strSharedFolderPath,
                                    g_strSharedFolderUser,
                                    g_strSharedFolderPassword);
                            prNet.StartInfo.CreateNoWindow = true;
                            prNet.StartInfo.UseShellExecute = false;
                            prNet.StartInfo.RedirectStandardOutput = true;
                            prNet.Start();
                            prNet.WaitForExit();
                        }

                        hasConnection = Directory.Exists(g_strSharedFolderPath);
                    }
                    catch (Exception)
                    {
                        hasConnection = false;
                    }

                    if (!hasConnection)
                    {
                        MessageBox.Show("共有フォルダに接続できません。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
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

                // ログイン情報インスタンス生成
                g_clsLoginInfo = new LoginInfo();

                // 帳票情報インスタンス生成
                g_clsReportInfo = new ReportInfo();

                // パス設定
                g_strMasterImageDirPath = Path.Combine(g_clsSystemSettingInfo.strTemporaryDirectory, g_CON_DIR_MASTER_IMAGE);
                g_strMasterImageDirMarking = Path.Combine(g_clsSystemSettingInfo.strTemporaryDirectory, g_CON_DIR_MASTER_IMAGE_MARKING);
                g_strZipExtractDirPath = Path.Combine(g_clsSystemSettingInfo.strTemporaryDirectory, g_CON_ZIP_EXTRACT_DIR_PATH);
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("初期起動時にエラーが発生しました。{0}{1}", Environment.NewLine, ex.Message));
                // メッセージ出力
                System.Windows.Forms.MessageBox.Show("初期起動時に例外が発生しました。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

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
        /// 欠点画像取込
        /// </summary>
        /// <param name="strUnitNum">号機</param>
        /// <param name="strFaultImageFileName">欠点画像ファイル名</param>
        public static async Task<Boolean> BolInputFaultImage(
            string strUnitNum,
            string strFaultImageFileName)
        {
            // NG画像連携ディレクトリ
            string strNgImageCooperationDirectory = string.Empty;

            // 号機に紐付くディレクトリ情報を設定
            switch (strUnitNum)
            {
                case g_strUnitNumN1:
                    strNgImageCooperationDirectory = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN1;
                    break;
                case g_strUnitNumN2:
                    strNgImageCooperationDirectory = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN2;
                    break;
                case g_strUnitNumN3:
                    strNgImageCooperationDirectory = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN3;
                    break;
                case g_strUnitNumN4:
                    strNgImageCooperationDirectory = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN4;
                    break;
                default:
                    return false;
            }

            // 欠点画像格納ディレクトリ
            string strFaultImageDirectory = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, strUnitNum);

            // 欠点画像解凍ディレクトリ
            string strFaultImageDecompressionDirectory = Path.Combine(strFaultImageDirectory, strFaultImageFileName);

            // zipファイル名
            string strZipFileName = strFaultImageFileName + ".zip";

            // zipファイルパス
            string strZipFilePath = Path.Combine(strNgImageCooperationDirectory, strZipFileName);

            // zipファイルコピーパス
            string strZipFileCopyPath = Path.Combine(g_strZipExtractDirPath, strUnitNum, strZipFileName);

            try
            {
                // zipファイルの存在チェックを行う
                if (!File.Exists(strZipFilePath))
                {
                    return false;
                }

                // zipファイルを一時フォルダにコピーする
                File.Copy(strZipFilePath, strZipFileCopyPath, true);

                // 欠点画像格納ディレクトリへ解凍する
                SevenZipBase.Path7za = @".\7z-extra\x64\7za.exe";
                SevenZipExtractor extractor = new SevenZipExtractor(strZipFileCopyPath);
                extractor.ExtractAll(strFaultImageDirectory, true);

                // 一時フォルダのzipファイルを削除する
                File.Delete(strZipFileCopyPath);

                if (!Directory.Exists(strFaultImageDecompressionDirectory))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0040, Environment.NewLine, ex.Message));

                // エラー発生時、中途半端に取り込まれた情報を削除する
                if (File.Exists(strZipFileCopyPath))
                {
                    File.Delete(strZipFileCopyPath);
                }

                if (Directory.Exists(strFaultImageDecompressionDirectory))
                {
                    Directory.Delete(strFaultImageDecompressionDirectory, true);
                }

                return false;
            }

            return true;
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
                m_sbErrMessage.AppendLine("Key[" + strKey + "] AppConfigに存在しません。");
            }
        }
    }
}
