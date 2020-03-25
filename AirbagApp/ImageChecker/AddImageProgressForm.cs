using ImageChecker.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class AddImageProgressForm : Form
    {
        public bool bolChgFile { get; set; }

        private string m_strChkFilePath = string.Empty;

        private string m_strCompletionNoticeCooperationDirectoryPath = string.Empty;

        private string m_strNgImageCooperationDirectoryPath = string.Empty;

        private string m_strNotDetectedImageCooperationDirectoryPath = string.Empty;

        private string m_strSafeFileName = string.Empty;

        private string m_strFileName = string.Empty;

        private string m_strFaultImageSubDirPath = string.Empty;

        private FileSystemWatcher m_fsWatcher;

        private delegate void Del();

        private HeaderData m_clsHeaderData;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clsHeaderData">ヘッダ情報</param>
        /// <param name="strFileName">ファイル名</param>
        /// <param name="strFaultImageSubDirPath">欠点画像サブディレクトリパス</param>
        public AddImageProgressForm(
            HeaderData clsHeaderData,
            string strFileName,
            string strFaultImageSubDirPath)
        {
            bolChgFile = false;

            m_clsHeaderData = clsHeaderData;

            m_strSafeFileName = strFileName;

            m_strFileName = Path.GetFileNameWithoutExtension(m_strSafeFileName);

            m_strFaultImageSubDirPath = strFaultImageSubDirPath;

            switch (m_clsHeaderData.strUnitNum)
            {
                case "N1":
                    m_strCompletionNoticeCooperationDirectoryPath = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN1;
                    m_strNgImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN1;
                    m_strNotDetectedImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNotDetectedImageCooperationDirectoryN1;
                    break;
                case "N2":
                    m_strCompletionNoticeCooperationDirectoryPath = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN2;
                    m_strNgImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN2;
                    m_strNotDetectedImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNotDetectedImageCooperationDirectoryN2;
                    break;
                case "N3":
                    m_strCompletionNoticeCooperationDirectoryPath = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN3;
                    m_strNgImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN3;
                    m_strNotDetectedImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNotDetectedImageCooperationDirectoryN3;
                    break;
                case "N4":
                    m_strCompletionNoticeCooperationDirectoryPath = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN4;
                    m_strNgImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN4;
                    m_strNotDetectedImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNotDetectedImageCooperationDirectoryN4;
                    break;
                default:
                    return;
            }

            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// ファイル監視中止
        /// </summary>
        private void StopSysFileWatcher()
        {
            m_fsWatcher.EnableRaisingEvents = false;
            m_fsWatcher.Dispose();
            m_fsWatcher = null;

            //this.Close();
            // ※InvalidOperationExceptionが発生
            new Thread(new ThreadStart(delegate
            {
                Invoke((Del)delegate
                {
                    this.Close();
                });
            })).Start();
        }

        /// <summary>
        /// 未検知画像チェック
        /// </summary>
        private bool CheckUndetectedImage()
        {
            string strOutPutFilePath = string.Empty;
            string strSQL = string.Empty;
            string[] strFileParam;
            int intParse = -1;
            DataTable dtData = null;
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

            // 未検知画像連携ファイルチェック
            try
            {
                // 出力ファイル設定
                strOutPutFilePath = Path.Combine(m_strNotDetectedImageCooperationDirectoryPath, m_strFileName + ".txt");

                // ファイル名妥当性チェック
                strFileParam = m_strFileName.Split('_');
                if ((strFileParam.Length < 2 || m_clsHeaderData.strFabricName != strFileParam[1]) ||
                    (strFileParam.Length < 3 || m_clsHeaderData.strInspectionDate.Replace("/", string.Empty) != strFileParam[2]) ||
                    (strFileParam.Length < 4 || int.TryParse(strFileParam[3], out intParse) == false || m_clsHeaderData.intInspectionNum != intParse))
                {
                    MessageBox.Show(
                        string.Format(
                            g_clsMessageInfo.strMsgW0005,
                            m_clsHeaderData.strInspectionDate,
                            m_clsHeaderData.strFabricName,
                            m_clsHeaderData.intInspectionNum),
                        g_CON_MESSAGE_TITLE_WARN,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return false;
                }

                // 未検知画像連携ディレクトリにテキストを出力する
                OutputNotDetectedImageCooperationText(strOutPutFilePath);
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0051, Environment.NewLine, ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    g_clsMessageInfo.strMsgE0052,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            // 連携済みチェック
            try
            {
                dtData = new DataTable();
                strSQL = @"SELECT COUNT(*) AS cnt
                           FROM  " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_num = :inspection_num
                           AND   TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                           AND   org_imagepath = :org_imagepath ";

                // SQLコマンドに各パラメータを設定する
                lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_clsHeaderData.strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_clsHeaderData.intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_clsHeaderData.strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "org_imagepath", DbType = DbType.String, Value = m_strSafeFileName });

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                // 件数
                if (dtData.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dtData.Rows[0]["cnt"]) > 0)
                    {
                        // メッセージ出力
                        MessageBox.Show(
                            g_clsMessageInfo.strMsgE0061,
                            g_CON_MESSAGE_TITLE_ERROR,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    g_clsMessageInfo.strMsgE0031,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            m_strChkFilePath = Path.Combine(m_strCompletionNoticeCooperationDirectoryPath, m_strFileName + ".txt");

            return true;
        }

        /// <summary>
        /// 未検知画像取込
        /// </summary>
        private bool ImportUndetectedImage()
        {
            string strSQL = string.Empty;
            string strZipFilePath = string.Empty;
            string strZipExtractToDirPath = string.Empty;
            string strZipArchiveEntryFilePath = string.Empty;
            string strExtractionDestinationPath = string.Empty;
            DirectoryInfo diThaw = null;
            DirectoryInfo diMigrationTarget = null;
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

            try
            {
                // 判定結果の取り込み処理
                strSQL = @"INSERT INTO " + g_clsSystemSettingInfo.strInstanceName + @".decision_result(
                               fabric_name
                             , inspection_num
                             , inspection_date
                             , branch_num
                             , line
                             , cloumns
                             , ng_face
                             , ng_reason
                             , org_imagepath
                             , marking_imagepath
                             , master_point
                             , ng_distance_x
                             , ng_distance_y
                             , camera_num
                             , worker_1
                             , worker_2
                             , over_detection_except_result          
                             , over_detection_except_datetime        
                             , over_detection_except_worker          
                             , before_over_detection_except_result   
                             , before_over_detection_except_datetime 
                             , before_over_detection_except_worker   
                             , acceptance_check_result
                             , acceptance_check_datetime
                             , acceptance_check_worker
                             , before_acceptance_check_result
                             , before_acceptance_check_upd_datetime
                             , before_acceptance_check_worker
                             , result_update_datetime
                             , result_update_worker
                             , before_ng_reason
                           )
                           SELECT
                               fabric_name
                             , inspection_num
                             , TO_DATE(:inspection_date_yyyymmdd,'YYYY/MM/DD')
                             , 1
                             , ng_line
                             , columns
                             , ng_face
                             , ng_reason
                             , ng_image
                             , marking_image
                             , master_point
                             , TO_NUMBER(ng_distance_x,'9999')
                             , TO_NUMBER(ng_distance_y,'9999')
                             , CASE
                                 WHEN ng_face = '#1' THEN camera_num_1
                                 WHEN ng_face = '#2' THEN camera_num_2
                                 ELSE NULL
                               END 
                             , worker_1
                             , worker_2
                             , :over_detection_except_result_ng_non_detect
                             , NULL
                             , NULL
                             , :over_detection_except_result_non
                             , NULL
                             , NULL
                             , :acceptance_check_result_non
                             , NULL
                             , NULL
                             , :acceptance_check_result_non
                             , NULL
                             , NULL
                             , NULL
                             , NULL
                             , NULL
                           FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".""rapid_" + m_clsHeaderData.strFabricName + "_" + m_clsHeaderData.intInspectionNum + "_" + m_clsHeaderData.strInspectionDate.Replace("/", string.Empty) + @"""
                           WHERE fabric_name = :fabric_name
                             AND inspection_num = :inspection_num 
                             AND ng_image = :ng_image 
                             AND rapid_result = :rapid_result
                             AND edge_result = :edge_result
                             AND masking_result = :masking_result";

                // SQLコマンドに各パラメータを設定する
                lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_clsHeaderData.strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_clsHeaderData.strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_clsHeaderData.intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_image", DbType = DbType.String, Value = m_strSafeFileName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "rapid_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intRapidResultNon });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "edge_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intEdgeResultNon });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "masking_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intMaskingResultNon });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNon });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ng_non_detect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNgNonDetect });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNon });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
            }
            catch (Exception ex)
            {
                g_clsConnectionNpgsql.DbRollback();

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    g_clsMessageInfo.strMsgE0057,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            // 欠点画像ディレクトリに格納する
            try
            {
                // 欠点画像格納ディレクトリに存在しない場合はフォルダを作成する
                if (Directory.Exists(m_strFaultImageSubDirPath) == false)
                {
                    Directory.CreateDirectory(m_strFaultImageSubDirPath);
                }

                // 一時ZIP解凍用フォルダ作成
                if (Directory.Exists(g_strZipExtractDirPath) == true)
                {
                    Directory.Delete(g_strZipExtractDirPath, true);
                }
                Directory.CreateDirectory(g_strZipExtractDirPath);

                strZipFilePath = Path.Combine(g_strZipExtractDirPath, m_strFileName + ".zip");

                // 欠点画像ZIPファイルを一時ZIP解凍用フォルダにコピーする
                File.Copy(Path.Combine(m_strNgImageCooperationDirectoryPath, m_strFileName + ".zip"), strZipFilePath, true);

                // 一時ディレクトリに欠点画像のフォルダが存在する場合は削除する
                using (ZipArchive za = ZipFile.Open(strZipFilePath, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry zae in za.Entries)
                    {
                        strZipExtractToDirPath =
                            Path.Combine(g_strZipExtractDirPath,
                            zae.FullName.Substring(0, (zae.FullName).IndexOf(Path.AltDirectorySeparatorChar)));

                        if (Directory.Exists(strZipExtractToDirPath) == true)
                        {
                            Directory.Delete(strZipExtractToDirPath, true);
                        }

                        break;
                    }
                }

                // 欠点画像ZIPを一時ディレクトリに解凍する
                ZipFile.ExtractToDirectory(strZipFilePath, g_strZipExtractDirPath);

                // 解凍先ディレクトリを取得
                diThaw = new DirectoryInfo(Path.Combine(g_strZipExtractDirPath, m_strFileName));

                // 移行先ディレクトリを作成
                diMigrationTarget = new DirectoryInfo(m_strFaultImageSubDirPath);

                // ファイルの取得
                foreach (FileInfo fInfo in diThaw.GetFiles().Where(
                    x => string.Compare(x.Extension, ".jpg", true) == 0))
                {
                    using (Bitmap bmpOriginalImage = new Bitmap(fInfo.FullName))
                    {
                        // カメラ位置を考慮し、180度回転させる
                        bmpOriginalImage.RotateFlip(RotateFlipType.Rotate180FlipNone);

                        // 画像を移行先ディレクトリに保存する
                        SaveNotDetectedImage(bmpOriginalImage, diMigrationTarget, fInfo);
                    }
                }

                // ZIPファイルの削除
                File.Delete(strZipFilePath);

                // 解凍先ディレクトリの削除
                diThaw.Delete(true);
            }
            catch (Exception ex)
            {
                // エラー発生時、中途半端に取り込まれた情報を削除する
                if (File.Exists(strZipFilePath))
                {
                    File.Delete(strZipFilePath);
                }

                if (diThaw.Exists)
                {
                    diThaw.Delete(true);
                }

                if (diMigrationTarget.Exists)
                {
                    diMigrationTarget.Delete(true);
                }

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0040, Environment.NewLine, ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    g_clsMessageInfo.strMsgE0041,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 未検知画像連携テキスト出力
        /// </summary>
        /// <param name="strOutPutFilePath">出力ファイルパス</param>
        private async Task<Boolean> OutputNotDetectedImageCooperationText(string strOutPutFilePath)
        {
            for (int intProcessingTimes = 1; intProcessingTimes <= g_clsSystemSettingInfo.intRetryTimes; intProcessingTimes++)
            {
                try
                {
                    // 書き込むファイルが既に存在している場合は、上書きする
                    using (StreamWriter sw = new StreamWriter(strOutPutFilePath, false, Encoding.GetEncoding("shift_jis")))
                    {
                        // 空ファイルの生成
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (intProcessingTimes == g_clsSystemSettingInfo.intRetryTimes)
                    {
                        // 試行後もエラーだった場合はリスローする
                        throw ex;
                    }

                    // 一時停止させ、処理をリトライする
                    await Task.Delay(g_clsSystemSettingInfo.intRetryWaitSeconds);
                }
            }

            return true;
        }

        /// <summary>
        /// 未検知画像保存
        /// </summary>
        /// <param name="bmpOriginalImage">未検知画像</param>
        /// <param name="diMigrationTarget">移行先ディレクトリ</param>
        /// <param name="fInfo">未検知画像ファイル情報</param>
        private async Task<Boolean> SaveNotDetectedImage(
            Bitmap bmpOriginalImage,
            DirectoryInfo diMigrationTarget,
            FileInfo fInfo)
        {
            for (int intProcessingTimes = 1; intProcessingTimes <= g_clsSystemSettingInfo.intRetryTimes; intProcessingTimes++)
            {
                try
                {
                    // 画像を移行先ディレクトリに保存する
                    bmpOriginalImage.Save(Path.Combine(diMigrationTarget.FullName, fInfo.Name), ImageFormat.Jpeg);
                    break;
                }
                catch (Exception ex)
                {
                    if (intProcessingTimes == g_clsSystemSettingInfo.intRetryTimes)
                    {
                        // 試行後もエラーだった場合はリスローする
                        throw ex;
                    }

                    // 一時停止させ、処理をリトライする
                    await Task.Delay(g_clsSystemSettingInfo.intRetryWaitSeconds);
                }
            }

            return true;
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressForm_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            bool bolProcOkNg = false;

            try
            {
                // 未検知画像チェックを行う
                if (!CheckUndetectedImage())
                {
                    return;
                }

                // 同期的に未検知画像連携ディレクトリの監視する
                m_fsWatcher = new FileSystemWatcher();
                m_fsWatcher.Path = m_strCompletionNoticeCooperationDirectoryPath;
                m_fsWatcher.Filter = string.Empty;
                m_fsWatcher.IncludeSubdirectories = false;

                // ファイル名とディレクトリ名と最終書き込む日時の変更を監視
                m_fsWatcher.NotifyFilter =
                    NotifyFilters.FileName
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.LastWrite;

                // イベントハンドラの追加
                m_fsWatcher.Changed += new FileSystemEventHandler(fsWatcher_Changed);
                m_fsWatcher.Created += new FileSystemEventHandler(fsWatcher_Changed);
                m_fsWatcher.Deleted += new FileSystemEventHandler(fsWatcher_Changed);
                m_fsWatcher.Renamed += new RenamedEventHandler(fsWatcher_Changed);

                // ボタンを有効にする
                DispatcherTimer dtVisiblebtnCancel = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
                dtVisiblebtnCancel.Start();
                dtVisiblebtnCancel.Tick += (s, args) =>
                {
                    dtVisiblebtnCancel.Stop();
                    btnCancel.Visible = true;
                };

                // 監視を開始する
                m_fsWatcher.EnableRaisingEvents = true;

                bolProcOkNg = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0058, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0059, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (bolProcOkNg == false)
                {
                    this.Close();
                }

                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ファイル監視(変更)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void fsWatcher_Changed(System.Object source, System.IO.FileSystemEventArgs e)
        {
            // ファイル存在チェック
            if (File.Exists(m_strChkFilePath) == false)
            {
                return;
            }

            // 未検知画像取込を行う
            if (ImportUndetectedImage())
            {
                bolChgFile = true;
            }

            StopSysFileWatcher();
        }

        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            StopSysFileWatcher();
        }
        #endregion
    }
}