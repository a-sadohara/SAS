using ImageChecker.DTO;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

        private string m_strMarkingImagepath = string.Empty;

        private int m_intBranchNum = 0;

        private int m_intNgDistanceX = 0;

        private int m_intNgDistanceY = 0;

        private string m_strLine = string.Empty;

        private string m_strCloumns = string.Empty;

        private bool m_bolLineCloumnsChangeFlg = false;

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
        /// <param name="strMarkingImagepath">マーキング画像パス</param>
        /// <param name="strLine">行</param>
        /// <param name="strCloumns">列</param>
        /// <param name="intBranchNum">枝番</param>
        /// <param name="intNgDistanceX">位置(±Xcm)</param>
        /// <param name="intNGDistanceY">位置(±Ycm)</param>
        /// <param name="bolLineCloumnsChangeFlg">行列変更フラグ</param>
        public AddImageProgressForm(
            HeaderData clsHeaderData,
            string strFileName,
            string strFaultImageSubDirPath,
            string strMarkingImagepath,
            string strLine,
            string strCloumns,
            int intBranchNum,
            int intNgDistanceX,
            int intNGDistanceY,
            bool bolLineCloumnsChangeFlg)
        {
            bolChgFile = false;

            m_clsHeaderData = clsHeaderData;

            m_strSafeFileName = strFileName;

            m_strFileName = Path.GetFileNameWithoutExtension(m_strSafeFileName);

            m_strFaultImageSubDirPath = strFaultImageSubDirPath;

            switch (m_clsHeaderData.strUnitNum)
            {
                case g_strUnitNumN1:
                    m_strCompletionNoticeCooperationDirectoryPath = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN1;
                    m_strNgImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN1;
                    m_strNotDetectedImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNotDetectedImageCooperationDirectoryN1;
                    break;
                case g_strUnitNumN2:
                    m_strCompletionNoticeCooperationDirectoryPath = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN2;
                    m_strNgImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN2;
                    m_strNotDetectedImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNotDetectedImageCooperationDirectoryN2;
                    break;
                case g_strUnitNumN3:
                    m_strCompletionNoticeCooperationDirectoryPath = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN3;
                    m_strNgImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN3;
                    m_strNotDetectedImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNotDetectedImageCooperationDirectoryN3;
                    break;
                case g_strUnitNumN4:
                    m_strCompletionNoticeCooperationDirectoryPath = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN4;
                    m_strNgImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNgImageCooperationDirectoryN4;
                    m_strNotDetectedImageCooperationDirectoryPath = g_clsSystemSettingInfo.strNotDetectedImageCooperationDirectoryN4;
                    break;
                default:
                    return;
            }

            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            m_bolLineCloumnsChangeFlg = bolLineCloumnsChangeFlg;

            if (m_bolLineCloumnsChangeFlg)
            {
                m_strMarkingImagepath = strMarkingImagepath;
                m_strLine = strLine;
                m_strCloumns = strCloumns;
                m_intBranchNum = intBranchNum;
                m_intNgDistanceX = intNgDistanceX;
                m_intNgDistanceY = intNGDistanceY;
                this.lblMessage.Text = g_clsMessageInfo.strMsgI0014;
            }
        }

        /// <summary>
        /// ファイル監視中止
        /// </summary>
        /// <param name="bolImportFlag">取込フラグ</param>
        private async void StopSysFileWatcher(bool bolImportFlag)
        {
            m_fsWatcher.EnableRaisingEvents = false;
            m_fsWatcher.Dispose();
            m_fsWatcher = null;

            if (bolImportFlag)
            {
                // 未検知画像の取込を行う
                bolChgFile = await ImportUndetectedImage();
            }

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
            DataTable dtData = new DataTable();
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

            // 未検知画像連携ファイルチェック
            try
            {
                // 出力ファイル設定
                strOutPutFilePath = Path.Combine(m_strNotDetectedImageCooperationDirectoryPath, m_strFileName + ".txt");

                // ファイル名妥当性チェック
                strFileParam = m_strFileName.Split('_');
                if (!m_bolLineCloumnsChangeFlg &&
                    ((strFileParam.Length < 2 || m_clsHeaderData.strFabricName != strFileParam[1]) ||
                    (strFileParam.Length < 3 || m_clsHeaderData.strInspectionDate.Replace("/", string.Empty) != strFileParam[2]) ||
                    (strFileParam.Length < 4 || int.TryParse(strFileParam[3], out intParse) == false || m_clsHeaderData.intInspectionNum != intParse)))
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
                for (int intProcessingTimes = 1; intProcessingTimes <= g_clsSystemSettingInfo.intRetryTimes; intProcessingTimes++)
                {
                    try
                    {
                        // 書き込むファイルが既に存在している場合は、上書きする
                        using (StreamWriter sw = new StreamWriter(strOutPutFilePath, false, Encoding.GetEncoding("shift_jis")))
                        {
                            // 行列変更時は、変更後の内容を書き込む
                            if (m_bolLineCloumnsChangeFlg)
                            {
                                sw.WriteLine(string.Format("{0},{1}", m_strLine, m_strCloumns));
                            }
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
                        Thread.Sleep(g_clsSystemSettingInfo.intRetryWaitSeconds);
                    }
                }
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

            if (!m_bolLineCloumnsChangeFlg)
            {
                // 連携済みチェック
                try
                {
                    strSQL = @"SELECT COUNT(*) AS cnt
                           FROM  " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_num = :inspection_num
                           AND   TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                           AND   unit_num = :unit_num
                           AND   org_imagepath = :org_imagepath ";

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_clsHeaderData.strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_clsHeaderData.intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_clsHeaderData.strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "org_imagepath", DbType = DbType.String, Value = m_strSafeFileName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_clsHeaderData.strUnitNum });

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
                finally
                {
                    dtData.Dispose();
                }
            }

            m_strChkFilePath = Path.Combine(m_strCompletionNoticeCooperationDirectoryPath, m_strFileName + ".txt");

            return true;
        }

        /// <summary>
        /// 未検知画像取込
        /// </summary>
        private async Task<Boolean> ImportUndetectedImage()
        {
            string strSQL = string.Empty;
            string strZipExtractDirPath = string.Empty;
            string strZipFilePath = string.Empty;
            string strZipExtractToDirPath = string.Empty;
            string strZipArchiveEntryFilePath = string.Empty;
            string strExtractionDestinationPath = string.Empty;
            string strRapidTableName = string.Empty;
            string strMsg = string.Empty;
            string strErrorMsg = string.Empty;
            int intExecutionCount = 0;
            DataTable dtData = new DataTable();
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

            try
            {
                strRapidTableName = "rapid_" + m_clsHeaderData.strFabricName + "_" + m_clsHeaderData.intInspectionNum + "_" + m_clsHeaderData.strInspectionDate.Replace("/", string.Empty);

                if (m_bolLineCloumnsChangeFlg)
                {
                    strErrorMsg = g_clsMessageInfo.strMsgE0072;

                    // 位置(±Xcm)・位置(±Ycm)の更新
                    strSQL = @"
                               UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".decision_result AS dr SET
                                   line = rpd.ng_line,
                                   cloumns = rpd.columns,
                                   master_point = rpd.master_point,
                                   ng_distance_x = TO_NUMBER(rpd.ng_distance_x,'9999'),
                                   ng_distance_y = TO_NUMBER(rpd.ng_distance_y,'9999')
                               FROM
                               (
                                   SELECT
                                       ng_line,
                                       columns,
                                       master_point,
                                       ng_distance_x,
                                       ng_distance_y
                                   FROM (
                                       SELECT ROW_NUMBER() OVER(PARTITION BY marking_image ORDER BY insert_datetime DESC) AS SEQ
                                           , rpd.*
                                       FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".""" + strRapidTableName + @""" rpd
                                       WHERE fabric_name = :fabric_name
                                       AND inspection_num = :inspection_num 
                                       AND ng_image = :ng_image 
                                       AND unit_num = :unit_num 
                                       AND rapid_result = :rapid_result
                                       AND edge_result = :edge_result
                                       AND masking_result = :masking_result
                                   ) temp
                                   WHERE SEQ = 1
                               ) AS rpd
                               WHERE dr.branch_num = :branch_num
                                   AND dr.unit_num = :unit_num
                                   AND dr.marking_imagepath = :marking_imagepath
                                   AND dr.ng_distance_x = :ng_distance_x
                                   AND dr.ng_distance_y = :ng_distance_y ";

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int32, Value = m_intBranchNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "marking_imagepath", DbType = DbType.String, Value = m_strMarkingImagepath });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_distance_x", DbType = DbType.Int32, Value = m_intNgDistanceX });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_distance_y", DbType = DbType.Int32, Value = m_intNgDistanceY });
                }
                else
                {
                    strErrorMsg = string.Format(g_clsMessageInfo.strMsgE0057, m_strSafeFileName);

                    // 判定結果の取り込み処理
                    strSQL = @"INSERT INTO " + g_clsSystemSettingInfo.strInstanceName + @".decision_result(
                               fabric_name
                             , inspection_num
                             , inspection_date
                             , branch_num
                             , unit_num
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
                             , unit_num
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
                           FROM (
                               SELECT ROW_NUMBER() OVER(PARTITION BY marking_image ORDER BY insert_datetime DESC) AS SEQ
                                   , rpd.*
                               FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".""" + strRapidTableName + @""" rpd
                               WHERE fabric_name = :fabric_name
                               AND inspection_num = :inspection_num 
                               AND ng_image = :ng_image 
                               AND unit_num = :unit_num 
                               AND rapid_result = :rapid_result
                               AND edge_result = :edge_result
                               AND masking_result = :masking_result
                           ) rpd
                           WHERE SEQ = 1
                           ON CONFLICT (branch_num, unit_num, marking_imagepath, ng_distance_x, ng_distance_y)
                           DO UPDATE SET
                               line = excluded.line
                             , cloumns = excluded.cloumns
                             , ng_distance_x = excluded.ng_distance_x
                             , ng_distance_y = excluded.ng_distance_y ";

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_clsHeaderData.strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNon });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ng_non_detect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNgNonDetect });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNon });
                }

                // SQLコマンドに各パラメータを設定する
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_clsHeaderData.strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_clsHeaderData.intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_image", DbType = DbType.String, Value = m_strSafeFileName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "rapid_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intRapidResultNon });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "edge_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intEdgeResultNon });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "masking_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intMaskingResultNon });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_clsHeaderData.strUnitNum });

                // sqlを実行する
                intExecutionCount = g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
            }
            catch (PostgresException pgex)
            {
                g_clsConnectionNpgsql.DbRollback();

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}検査日付:{2}, {3}号機, 検査番号:{4}, 品名:{5}, 反番:{6}, 画像ファイル:{7}, 移送元テーブル:{8}, 移送先テーブル:{9}, 処理ブロック:{10}{11}{12}",
                        g_clsMessageInfo.strMsgE0002,
                        Environment.NewLine,
                        m_clsHeaderData.strInspectionDate,
                        m_clsHeaderData.strUnitNum,
                        m_clsHeaderData.intInspectionNum,
                        m_clsHeaderData.strProductName,
                        m_clsHeaderData.strFabricName,
                        m_strSafeFileName,
                        strRapidTableName,
                        "decision_result",
                        "検査NG情報取得",
                        Environment.NewLine,
                        pgex.Message));

                // メッセージ出力
                MessageBox.Show(
                    strErrorMsg,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
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
                    strErrorMsg,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            // rapidテーブルに該当の未検知情報が存在しない場合、検査無効情報をチェックする
            if (intExecutionCount == 0)
            {
                try
                {
                    strSQL = @"SELECT
                                   fabric_name
                               FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".""" + strRapidTableName + @"""
                               WHERE fabric_name = :fabric_name
                                   AND inspection_num = :inspection_num 
                                   AND ng_image = :ng_image 
                                   AND unit_num = :unit_num 
                                   AND rapid_result = :rapid_result
                                   AND edge_result = :edge_result
                                   AND masking_result = :masking_result";

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_clsHeaderData.strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_clsHeaderData.intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_image", DbType = DbType.String, Value = m_strSafeFileName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_clsHeaderData.strUnitNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "rapid_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intRapidResultDis });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "edge_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intEdgeResultDis });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "masking_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intMaskingResultDis });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    if (dtData.Rows.Count > 0)
                    {
                        // 検査情報無効のエラーを表示する
                        strMsg = g_clsMessageInfo.strMsgE0063;
                    }
                    else
                    {
                        // 検査情報無しのエラーを表示する
                        strMsg = g_clsMessageInfo.strMsgE0057;
                    }

                    // ログ出力
                    WriteEventLog(
                        g_CON_LEVEL_WARN,
                        string.Format(
                            "{0}{1}検査日付:{2}, {3}号機, 検査番号:{4}, 品名:{5}, 反番:{6}, 画像ファイル:{7}, 取得対象テーブル:{8}",
                            g_clsMessageInfo.strMsgE0039,
                            Environment.NewLine,
                            m_clsHeaderData.strInspectionDate,
                            m_clsHeaderData.strUnitNum,
                            m_clsHeaderData.intInspectionNum,
                            m_clsHeaderData.strProductName,
                            m_clsHeaderData.strFabricName,
                            m_strSafeFileName,
                            strRapidTableName));

                    // メッセージ出力
                    MessageBox.Show(
                        string.Format(strMsg, m_strSafeFileName),
                        g_CON_MESSAGE_TITLE_ERROR,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (PostgresException pgex)
                {
                    g_clsConnectionNpgsql.DbRollback();

                    // ログ出力
                    WriteEventLog(
                        g_CON_LEVEL_ERROR,
                        string.Format(
                            "{0}{1}検査日付:{2}, {3}号機, 検査番号:{4}, 品名:{5}, 反番:{6}, 画像ファイル:{7}, 取得対象テーブル:{8}, 処理ブロック:{9}{10}{11}",
                            g_clsMessageInfo.strMsgE0001,
                            Environment.NewLine,
                            m_clsHeaderData.strInspectionDate,
                            m_clsHeaderData.strUnitNum,
                            m_clsHeaderData.intInspectionNum,
                            m_clsHeaderData.strProductName,
                            m_clsHeaderData.strFabricName,
                            m_strSafeFileName,
                            strRapidTableName,
                            "検査対象外更新",
                            Environment.NewLine,
                            pgex.Message));

                    // メッセージ出力
                    MessageBox.Show(
                        strErrorMsg,
                        g_CON_MESSAGE_TITLE_ERROR,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    g_clsConnectionNpgsql.DbRollback();

                    // ログ出力
                    WriteEventLog(
                        g_CON_LEVEL_ERROR,
                        string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));

                    // メッセージ出力
                    MessageBox.Show(
                        strErrorMsg,
                        g_CON_MESSAGE_TITLE_ERROR,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    dtData.Dispose();
                }

                return false;
            }

            if (!m_bolLineCloumnsChangeFlg)
            {
                DirectoryInfo diFaultImage = new DirectoryInfo(m_strFaultImageSubDirPath);

                string strLogMessage =
                    string.Format(
                        g_CON_LOG_MESSAGE_FOMAT,
                        m_clsHeaderData.strUnitNum,
                        m_clsHeaderData.strInspectionDate,
                        m_clsHeaderData.intInspectionNum,
                        m_clsHeaderData.strProductName,
                        m_clsHeaderData.strFabricName);

                // 未検知画像の取込を行う
                Task<Boolean> taskCheckFaultImage =
                    Task<Boolean>.Run(() => BolGetFaultImage(
                        m_clsHeaderData.intInspectionNum,
                        m_clsHeaderData.strInspectionDate,
                        m_clsHeaderData.strUnitNum,
                        m_clsHeaderData.strFabricName,
                        m_strFileName,
                        strLogMessage,
                        false,
                        true,
                        diFaultImage.Name));

                return await taskCheckFaultImage;
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
        private void fsWatcher_Changed(Object source, FileSystemEventArgs e)
        {
            // ファイル存在チェック
            if (File.Exists(m_strChkFilePath) == false)
            {
                return;
            }

            StopSysFileWatcher(true);
        }

        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            StopSysFileWatcher(false);
        }
        #endregion
    }
}