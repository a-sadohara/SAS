using ImageChecker.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class Result : Form
    {
        public bool bolMod { get; set; }

        public bool bolReg { get; set; }

        // 判定結果情報
        public DecisionResult clsDecisionResult { get; set; }

        // パラメータ関連（可変）
        private HeaderData m_clsHeaderData;                                 // ヘッダ情報
        private int m_intAcceptanceCheckStatus = 0;                         // 合否確認ステータス
        private string m_strDecisionEndTime = string.Empty;                 // 判定終了日時
        private int m_intNgCushionCount = 0;                                // NGクッション数

        // パラメータ関連（不変）
        private readonly string m_strUnitNum = string.Empty;                // 号機
        private readonly string m_strProductName = string.Empty;            // 品名
        private readonly string m_strOrderImg = string.Empty;               // 指図
        private readonly string m_strFabricName = string.Empty;             // 反番
        private readonly string m_strInspectionDate = string.Empty;         // 検査日付
        private readonly string m_strStartDatetime = string.Empty;          // 搬送開始日時
        private readonly string m_strEndDatetime = string.Empty;            // 搬送終了日時
        private readonly int m_intInspectionStartLine = -1;                 // 検査開始行
        private readonly int m_intInspectionEndLine = -1;                   // 最終行数
        private readonly int m_intInspectionTargetLine = -1;                // 検査対象数
        private readonly string m_strDecisionStartTime = string.Empty;      // 判定開始日時
        private readonly string m_strInspectionDirection = string.Empty;    // 検査方向
        private readonly int m_intInspectionNum = 0;                        // 検査番号
        private readonly int m_intColumnCnt = 0;                            // 列数
        private readonly int m_intFromApId = 0;                             // 遷移元画面ID

        // 定数
        private const string m_CON_FORMAT_UNIT_NUM = "号機：{0}";
        private const string m_CON_FORMAT_PRODUCT_NAME = "品名：{0}";
        private const string m_CON_FORMAT_ORDER_IMG = "指図：{0}";
        private const string m_CON_FORMAT_FABRIC_NAME = "反番：{0}";
        private const string m_CON_FORMAT_START_DATETIME = "搬送開始日時：{0}";
        private const string m_CON_FORMAT_END_DATETIME = "搬送終了日時：{0}";
        private const string m_CON_FORMAT_INSPECTION_LINE = "検査範囲行　：{0}～{1}";
        private const string m_CON_FORMAT_DECISION_START_DATETIME = "判定開始日時：{0}";
        private const string m_CON_FORMAT_DECISION_END_DATETIME = "判定終了日時：{0}";
        private const string m_CON_FORMAT_INSPECTION_NUM = "検査番号：{0}";
        private const string m_CON_FORMAT_WORKER_NAME = "作業者：{0}";
        private const string m_CON_FORMAT_NG_DISTANCE = "{0},{1}";
        private const string m_CON_FORMAT_IMAGE_INSPECTION_COUNT = "画像検査枚数：{0}(NG：{1}/OK：{2})";
        private const string m_CON_FORMAT_CUSHION_COUNT = "クッション数：{0}(NG：{1}/OK：{2})";
        private const string m_CON_FORMAT_KEN_TAN_CHECK_SHEET_CSV_NAME = "{0}_N.csv";

        // 欠点画像サブディレクトリパス
        private readonly string m_strFaultImageSubDirectory = string.Empty;

        // 判定終了日時（仮）　※判定登録でDBに登録できるまでは未確定。印刷とDB登録に使用
        private string m_strDecisionEndTimeBeta = string.Empty;

        // データ保持関連
        private DataTable m_dtData;

        // 選択行情報関連
        private int m_intSelBranchNum = -1;
        private string m_strSelMarkingImagepath = string.Empty;

        // [X]ボタン判定
        private bool m_bolXButton = true;

        // [X]ボタン無効
        private bool m_bolXButtonDisable = false;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clsHeaderData">ヘッダ情報</param>
        /// <param name="clsDecisionResultBef">判定結果情報(更新前)</param>
        /// <param name="intFromApId">遷移元画面ID</param>
        public Result(ref HeaderData clsHeaderData, int intFromApId, int intSelBranchNum, string strSelMarkingImagePath)
        {
            m_clsHeaderData = clsHeaderData;

            m_strUnitNum = clsHeaderData.strUnitNum;
            m_strProductName = clsHeaderData.strProductName;
            m_strOrderImg = clsHeaderData.strOrderImg;
            m_strFabricName = clsHeaderData.strFabricName;
            m_strInspectionDate = clsHeaderData.strInspectionDate;
            m_strStartDatetime = clsHeaderData.strStartDatetime;
            m_strEndDatetime = clsHeaderData.strEndDatetime;
            m_intInspectionStartLine = clsHeaderData.intInspectionStartLine;
            m_intInspectionEndLine = clsHeaderData.intInspectionEndLine;
            m_intInspectionTargetLine = clsHeaderData.intInspectionTargetLine;
            m_strDecisionStartTime = clsHeaderData.strDecisionStartDatetime;
            m_strDecisionEndTime = clsHeaderData.strDecisionEndDatetime;
            m_strInspectionDirection = clsHeaderData.strInspectionDirection;
            m_intInspectionNum = clsHeaderData.intInspectionNum;
            m_intAcceptanceCheckStatus = clsHeaderData.intAcceptanceCheckStatus;
            m_intColumnCnt = clsHeaderData.intColumnCnt;
            m_intFromApId = intFromApId;

            m_strFaultImageSubDirectory = string.Join("_", m_strInspectionDate.Replace("/", string.Empty),
                                                           m_strProductName,
                                                           m_strFabricName,
                                                           m_intInspectionNum);

            m_intSelBranchNum = intSelBranchNum;
            m_strSelMarkingImagepath = strSelMarkingImagePath;

            bolMod = false;
            bolReg = false;
            clsDecisionResult = new DecisionResult();

            InitializeComponent();

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;
        }

        /// <summary>
        /// 過検知除外ステータス更新
        /// </summary>
        /// <param name="strFabricName">反番</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="intStatus">ステータス</param>
        /// <param name="strEndDatetime">判定終了日時(YYYY/MM//DD HH:MM:SS)</param>
        /// <returns></returns>
        private Boolean blnUpdAcceptanceCheckStatus(string strFabricName,
                                                          string strInspectionDate,
                                                          int intInspectionNum,
                                                          int intStatus,
                                                          string strEndDatetime = "")
        {
            string strSQL = string.Empty;
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            try
            {
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // SQL文を作成する
                strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                              SET acceptance_check_status = :acceptance_check_status ";

                if (intStatus == g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd)
                {
                    strSQL += @", result_datetime = CASE WHEN result_datetime IS NULL
                                                      THEN :current_timestamp
                                                      ELSE result_datetime
                                                    END ";
                }

                if (!string.IsNullOrEmpty(strEndDatetime))
                {
                    strSQL += @", decision_end_datetime = TO_TIMESTAMP(:decision_end_datetime_yyyymmdd_hhmmss, 'YYYY/MM/DD HH24:MI:SS') ";
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "decision_end_datetime_yyyymmdd_hhmmss", DbType = DbType.String, Value = strEndDatetime });
                }

                strSQL += @"WHERE fabric_name = :fabric_name
                              AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                              AND inspection_num = :inspection_num
                              AND unit_num = :unit_num";

                // SQLコマンドに各パラメータを設定する
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_status", DbType = DbType.Int16, Value = intStatus });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "current_timestamp", DbType = DbType.DateTime2, Value = date });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0035, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// 検査結果CSV出力
        /// </summary>
        /// <param name="dtData">データテーブル</param>
        private async Task<Boolean> OutputInspectionResltCsv(DataTable dtData)
        {
            string strWriteLine = string.Empty;

            for (int intProcessingTimes = 1; intProcessingTimes <= g_clsSystemSettingInfo.intRetryTimes; intProcessingTimes++)
            {
                try
                {
                    // 保存先ディレクトリに作成
                    // Shift JISで書き込む
                    // 書き込むファイルが既に存在している場合は、上書きする
                    using (StreamWriter sw = new StreamWriter(Path.Combine(g_clsSystemSettingInfo.strInspectionResltCsvDirectory,
                                                                           string.Format(m_CON_FORMAT_KEN_TAN_CHECK_SHEET_CSV_NAME, m_strFabricName)),
                                                                           false,
                                                                           Encoding.GetEncoding("shift_jis")))
                    {
                        // １行目
                        strWriteLine = string.Empty;
                        // カンマ区切りで1行文字列にする
                        strWriteLine += "#反番毎結果ファイル#,";
                        strWriteLine += ",";
                        strWriteLine += m_strProductName + ",";
                        strWriteLine += m_strUnitNum + "号機,";
                        strWriteLine += ",";
                        strWriteLine += ",";
                        strWriteLine += ",";
                        strWriteLine += ",";
                        strWriteLine += "X(cm),";
                        strWriteLine += "Y(cm),";
                        strWriteLine += "作業者名";
                        sw.WriteLine(strWriteLine);

                        // 明細行
                        foreach (DataRow dr in dtData.Rows)
                        {
                            // カンマ区切りで1行文字列にする
                            strWriteLine = string.Empty;
                            strWriteLine += dr["acceptance_check_datetime"].ToString();
                            strWriteLine += "," + dr["order_img"].ToString();
                            strWriteLine += "," + dr["fabric_name"].ToString();
                            strWriteLine += "," + dr["line"].ToString();

                            if (dr["cloumns"].ToString() == "A")
                            {
                                strWriteLine += ",Ａ";
                            }

                            if (dr["cloumns"].ToString() == "B")
                            {
                                strWriteLine += ",Ｂ";
                            }

                            if (dr["cloumns"].ToString() == "C")
                            {
                                strWriteLine += ",Ｃ";
                            }

                            if (dr["cloumns"].ToString() == "D")
                            {
                                strWriteLine += ",Ｄ";
                            }

                            if (dr["cloumns"].ToString() == "E")
                            {
                                strWriteLine += ",Ｅ";
                            }

                            strWriteLine += "," + dr["ng_face"].ToString();
                            strWriteLine += "," + dr["ng_reason"].ToString();
                            strWriteLine += "," + dr["inspection_num"].ToString();
                            strWriteLine += "," + dr["ng_distance_x"].ToString();
                            strWriteLine += "," + dr["ng_distance_y"].ToString();
                            strWriteLine += "," + dr["acceptance_check_worker"].ToString();

                            sw.WriteLine(strWriteLine);
                        }
                    }

                    // 検査結果CSVを生産管理システム連携ディレクトリにコピー
                    File.Copy(Path.Combine(g_clsSystemSettingInfo.strInspectionResltCsvDirectory
                              , string.Format(m_CON_FORMAT_KEN_TAN_CHECK_SHEET_CSV_NAME, m_strFabricName)),
                              Path.Combine(g_clsSystemSettingInfo.strProductionManagementCooperationDirectory
                              , string.Format(m_CON_FORMAT_KEN_TAN_CHECK_SHEET_CSV_NAME, m_strFabricName)), true);
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
        private void Result_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            bool bolProcOkNg = false;

            string strSQL = string.Empty;
            DataTable dtData = new DataTable();
            ArrayList arrRow = new ArrayList();
            string stResultName = string.Empty;
            int intImageInspectionCount = -1;
            int intImageInspectionCountOk = -1;
            int intImageInspectionCountNg = -1;
            int intCushionInspectionCount = -1;
            int intCushionInspectionCountOk = -1;
            int intCushionInspectionCountNg = -1;

            try
            {
                // 初期化
                bolMod = false;
                bolReg = false;

                // 列のスタイル変更
                this.dgvDecisionResult.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;     //№

                // 作業者の表示
                lblWorkerName.Text = string.Format(m_CON_FORMAT_WORKER_NAME, g_clsLoginInfo.strWorkerName);

                // ヘッダの表示
                lblUnitNum.Text = string.Format(m_CON_FORMAT_UNIT_NUM, m_strUnitNum);
                lblProductName.Text = string.Format(m_CON_FORMAT_PRODUCT_NAME, m_strProductName);
                lblOrderImg.Text = string.Format(m_CON_FORMAT_ORDER_IMG, m_strOrderImg);
                lblFabricName.Text = string.Format(m_CON_FORMAT_FABRIC_NAME, m_strFabricName);
                lblStartDatetime.Text = string.Format(m_CON_FORMAT_START_DATETIME, m_strStartDatetime);
                lblEndDatetime.Text = string.Format(m_CON_FORMAT_END_DATETIME, m_strEndDatetime);
                lblInspectionLine.Text = string.Format(m_CON_FORMAT_INSPECTION_LINE, m_intInspectionStartLine, m_intInspectionEndLine);
                lblDecisionStartTime.Text = string.Format(m_CON_FORMAT_DECISION_START_DATETIME, m_strDecisionStartTime);
                lblDecisionEndTime.Text = string.Format(m_CON_FORMAT_DECISION_END_DATETIME, m_strDecisionEndTime);
                lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum);

                dgvDecisionResult.Rows.Clear();

                try
                {
                    strSQL = @"SELECT
                                   image_inspection_count
                                 , image_inspection_count_ng
                                 , image_inspection_count_ok
                               FROM
                                   (        
                                       SELECT
                                           COUNT(*) AS image_inspection_count
                                         , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                            WHERE fabric_name = :fabric_name
                                            AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                            AND   inspection_num = :inspection_num
                                            AND   unit_num = :unit_num
                                            AND   acceptance_check_result IN (:acceptance_check_result_ngdetect,
                                                                              :acceptance_check_result_ngnondetect)) AS image_inspection_count_ng
                                         , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                            WHERE fabric_name = :fabric_name
                                            AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                            AND   inspection_num = :inspection_num
                                            AND   unit_num = :unit_num
                                            AND   acceptance_check_result = :acceptance_check_result_ok) AS image_inspection_count_ok
                                       FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                       WHERE fabric_name = :fabric_name
                                       AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                       AND   inspection_num = :inspection_num
                                       AND   unit_num = :unit_num
                                       AND   over_detection_except_result <> :over_detection_except_result_ok 
                                   ) imgcnt";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ngdetect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNgDetect });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ngnondetect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNgNonDetect });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultOk });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    intImageInspectionCount = int.Parse(dtData.Rows[0]["image_inspection_count"].ToString());
                    intImageInspectionCountNg = int.Parse(dtData.Rows[0]["image_inspection_count_ng"].ToString());
                    intImageInspectionCountOk = int.Parse(dtData.Rows[0]["image_inspection_count_ok"].ToString());

                    // ヘッダ表示
                    // 画像検査枚数
                    lblImageCount.Text = string.Format(m_CON_FORMAT_IMAGE_INSPECTION_COUNT,
                                                       intImageInspectionCount.ToString(),
                                                       intImageInspectionCountNg.ToString(),
                                                       intImageInspectionCountOk.ToString());
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1},{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                finally
                {
                    dtData.Dispose();
                }

                dtData = new DataTable();
                try
                {
                    // 行列単位のNG件数を抽出
                    strSQL = @"SELECT COUNT(*) AS cnt
                               FROM (
                                   SELECT
                                         line
                                       , cloumns
                                     FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                    WHERE fabric_name = :fabric_name
                                      AND inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                      AND inspection_num = :inspection_num
                                      AND unit_num = :unit_num
                                      AND ng_reason IS NOT NULL
                                 GROUP BY line, cloumns
                                     ) imgcnt";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultOk });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    // クッション数を算出
                    intCushionInspectionCount = m_intColumnCnt * m_intInspectionTargetLine;
                    intCushionInspectionCountNg = int.Parse(dtData.Rows[0]["cnt"].ToString());
                    intCushionInspectionCountOk = intCushionInspectionCount - intCushionInspectionCountNg;

                    // パラメータを設定
                    m_intNgCushionCount = intCushionInspectionCountNg;

                    // ヘッダ表示
                    // クッション数
                    lblCushionCount.Text = string.Format(m_CON_FORMAT_CUSHION_COUNT,
                                                         intCushionInspectionCount.ToString(),
                                                         intCushionInspectionCountNg.ToString(),
                                                         intCushionInspectionCountOk.ToString());
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                finally
                {
                    dtData.Dispose();
                }

                // 一覧の表示
                dgvDecisionResult.Rows.Clear();
                try
                {
                    m_dtData = new DataTable();
                    strSQL = @"SELECT
                               branch_num
                             , line
                             , cloumns
                             , ng_face
                             , ng_distance_x
                             , ng_distance_y
                             , over_detection_except_result
                             , acceptance_check_result
                             , ng_reason
                             , TO_CHAR(over_detection_except_datetime,'YYYY/MM/DD HH24:MI:SS') AS over_detection_except_datetime
                             , over_detection_except_worker
                             ,CASE WHEN result_update_datetime IS NOT NULL
                                   THEN TO_CHAR(result_update_datetime,'YYYY/MM/DD HH24:MI:SS')
                                   ELSE TO_CHAR(acceptance_check_datetime,'YYYY/MM/DD HH24:MI:SS')
                              END AS acceptance_check_datetime
                             ,CASE WHEN result_update_worker IS NOT NULL
                                   THEN result_update_worker
                                   ELSE acceptance_check_worker
                              END AS acceptance_check_worker
                             , org_imagepath
                             , marking_imagepath
                           FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                           AND   inspection_num = :inspection_num
                           AND   unit_num = :unit_num
                           AND   over_detection_except_result <> :over_detection_except_result_ok 
                           ORDER BY ";

                    if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS)
                    {
                        strSQL += "line ASC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                    }

                    if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
                    {
                        strSQL += "line ASC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                    }

                    if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY)
                    {
                        strSQL += "line DESC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                    }

                    if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
                    {
                        strSQL += "line DESC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                    }

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);

                    // データグリッドに反映
                    for (int i = 0; i <= m_dtData.Rows.Count - 1; i++)
                    {
                        arrRow = new ArrayList();

                        // No
                        arrRow.Add(this.dgvDecisionResult.Rows.Count + 1);

                        // SQL抽出項目
                        arrRow.Add(m_dtData.Rows[i]["line"]);
                        arrRow.Add(m_dtData.Rows[i]["cloumns"]);
                        arrRow.Add(m_dtData.Rows[i]["ng_face"]);
                        arrRow.Add(string.Format(m_CON_FORMAT_NG_DISTANCE, m_dtData.Rows[i]["ng_distance_x"].ToString(), m_dtData.Rows[i]["ng_distance_y"].ToString()));

                        // 過検知除外結果：名称を表示
                        if (int.Parse(m_dtData.Rows[i]["over_detection_except_result"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptResultNon)
                        {
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameNon;
                        }

                        if (int.Parse(m_dtData.Rows[i]["over_detection_except_result"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptResultOk)
                        {
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameOk;
                        }

                        if (int.Parse(m_dtData.Rows[i]["over_detection_except_result"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptResultNg)
                        {
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameNg;
                        }

                        if (int.Parse(m_dtData.Rows[i]["over_detection_except_result"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptResultNgNonDetect)
                        {
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameNgNonDetect;
                        }

                        arrRow.Add(stResultName);

                        // 合否確認結果：名称を表示
                        if (int.Parse(m_dtData.Rows[i]["acceptance_check_result"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckResultNon)
                        {
                            stResultName = g_clsSystemSettingInfo.strAcceptanceCheckResultNameNon;
                        }

                        if (int.Parse(m_dtData.Rows[i]["acceptance_check_result"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckResultOk)
                        {
                            stResultName = g_clsSystemSettingInfo.strAcceptanceCheckResultNameOk;
                        }

                        if (int.Parse(m_dtData.Rows[i]["acceptance_check_result"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckResultNgDetect)
                        {
                            stResultName = g_clsSystemSettingInfo.strAcceptanceCheckResultNameNgDetect;
                        }

                        if (int.Parse(m_dtData.Rows[i]["acceptance_check_result"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckResultNgNonDetect)
                        {
                            stResultName = g_clsSystemSettingInfo.strAcceptanceCheckResultNameNgNonDetect;
                        }

                        arrRow.Add(stResultName);

                        arrRow.Add(m_dtData.Rows[i]["ng_reason"]);
                        arrRow.Add(m_dtData.Rows[i]["over_detection_except_datetime"]);
                        arrRow.Add(m_dtData.Rows[i]["over_detection_except_worker"]);
                        arrRow.Add(m_dtData.Rows[i]["acceptance_check_datetime"]);
                        arrRow.Add(m_dtData.Rows[i]["acceptance_check_worker"]);

                        this.dgvDecisionResult.Rows.Add(arrRow.ToArray());

                        // 行選択
                        if (m_intSelBranchNum != -1 &&
                            Convert.ToInt32(m_dtData.Rows[i]["branch_num"]) == m_intSelBranchNum &&
                            m_dtData.Rows[i]["marking_imagepath"].ToString() == m_strSelMarkingImagepath)
                        {
                            dgvDecisionResult.Rows[i].Selected = true;
                            dgvDecisionResult.FirstDisplayedScrollingRowIndex = i;

                            // 以降の行は探さない
                            m_intSelBranchNum = -1;
                        }
                    }

                    bolProcOkNg = true;
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // 合否確認へ戻るボタン制御
                if (dgvDecisionResult.Rows.Count == 0)
                {
                    btnAcceptanceCheck.Enabled = false;
                }
                else
                {
                    btnAcceptanceCheck.Enabled = true;
                }

                bolProcOkNg = true;
            }
            finally
            {
                if (bolProcOkNg == false)
                {
                    m_bolXButton = false;
                    this.Close();
                }

                this.ResumeLayout();
            }
        }

        /// <summary>
        /// 変更破棄
        /// </summary>
        /// <param name="intToApId"></param>
        /// <returns>true:処理継続 false:処理中断</returns>
        private bool bolDisposeUpd(int intToApId)
        {
            string strApName = string.Empty;

            // 遷移画面名の設定
            switch (intToApId)
            {
                case g_CON_APID_LOGIN:
                    strApName = "ログイン";
                    break;
                case g_CON_APID_TARGET_SELECTION:
                    strApName = "検査対象選択";
                    break;
            }

            // 画面名チェック
            if (string.IsNullOrEmpty(strApName) == true)
            {
                return true;
            }

            // メッセージ表示
            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0013, strApName),
                                g_CON_MESSAGE_TITLE_QUESTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return false;
            }

            // 更新前の状態に戻す
            try
            {
                g_clsConnectionNpgsql.DbRollback();

                return true;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 検査対象選択へ戻る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTargetSelection_Click(object sender, EventArgs e)
        {
            if (m_intAcceptanceCheckStatus != g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd)
            {
                // 合否確認ステータス更新(中断)
                if (blnUpdAcceptanceCheckStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                g_clsSystemSettingInfo.intAcceptanceCheckStatusStp) == false)
                {
                    // エラー時
                    g_clsConnectionNpgsql.DbRollback();
                    g_clsConnectionNpgsql.DbClose();

                    return;
                }
            }

            if (m_intFromApId != 0)
            {
                // 変更破棄
                if (bolDisposeUpd(g_CON_APID_TARGET_SELECTION) == false)
                {
                    return;
                }
            }

            g_clsConnectionNpgsql.DbCommit();
            g_clsConnectionNpgsql.DbClose();

            m_bolXButton = false;
            this.Close();
        }

        /// <summary>
        /// 判定登録クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnRegstDecision_Click(object sender, EventArgs e)
        {
            bool bolProcOkNg = false;
            bool bolIsOutputSuccessful = false;

            string strSQL = string.Empty;
            DataTable dtData = new DataTable();
            string strWriteLine = string.Empty;

            if (MessageBox.Show(g_clsMessageInfo.strMsgQ0012, g_CON_MESSAGE_TITLE_QUESTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            ImportImageZipProgressForm frmProgress = new ImportImageZipProgressForm(g_clsMessageInfo.strMsgI0012);
            frmProgress.StartPosition = FormStartPosition.CenterScreen;
            frmProgress.Size = this.Size;
            frmProgress.Show(this);

            try
            {
                // 終了日時(仮)を保持
                m_strDecisionEndTimeBeta = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                // 合否確認ステータス更新(検査完了)
                if (blnUpdAcceptanceCheckStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd,
                                                m_strDecisionEndTimeBeta) == false)
                {
                    return;
                }

                // 帳票印刷
                bolIsOutputSuccessful =
                    await Task<Boolean>.Run(() =>
                        g_clsReportInfo.OutputReport(
                            m_strFabricName,
                            m_strInspectionDate,
                            m_strUnitNum,
                            m_intInspectionNum,
                            m_intNgCushionCount,
                            m_dtData.Select("ng_reason <> ''").Length));

                if (!bolIsOutputSuccessful)
                {
                    return;
                }

                // 検査結果CSV作成
                try
                {
                    strSQL = @"SELECT
                                   CASE WHEN dr.result_update_datetime IS NOT NULL
                                        THEN TO_CHAR(dr.result_update_datetime,'YYYY/MM/DD HH24:MI:SS')
                                        ELSE TO_CHAR(dr.acceptance_check_datetime,'YYYY/MM/DD HH24:MI:SS')
                                   END AS acceptance_check_datetime
                                 , iih.order_img
                                 , iih.fabric_name
                                 , dr.line
                                 , dr.cloumns
                                 , dr.ng_face
                                 , dr.ng_reason
                                 , TO_CHAR(dr.inspection_num, 'FM00000000') AS inspection_num
                                 , dr.ng_distance_x
                                 , dr.ng_distance_y
                                 , CASE WHEN dr.result_update_worker IS NOT NULL
                                        THEN dr.result_update_worker
                                        ELSE dr.acceptance_check_worker
                                   END AS acceptance_check_worker
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result dr
                               INNER JOIN " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header iih
                               ON  dr.fabric_name = iih.fabric_name
                               AND dr.inspection_date = iih.inspection_date
                               AND dr.inspection_num = iih.inspection_num
                               AND dr.unit_num = iih.unit_num
                               WHERE dr.fabric_name = :fabric_name
                               AND   dr.unit_num = :unit_num
                               AND   dr.ng_reason IS NOT NULL 
                               AND   iih.acceptance_check_status = :acceptance_check_status_end
                               ORDER BY 
                                   dr.inspection_num ASC 
                                 , dr.line ASC 
                                 , dr.cloumns ASC 
                                 , dr.ng_face ASC 
                                 , acceptance_check_datetime ASC ";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_status_end", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    await Task<Boolean>.Run(() => OutputInspectionResltCsv(dtData));
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0048, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0049, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // DBコミット
                g_clsConnectionNpgsql.DbCommit();

                // パラメータを更新
                m_clsHeaderData.intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd;
                m_intAcceptanceCheckStatus = m_clsHeaderData.intAcceptanceCheckStatus;
                m_clsHeaderData.strDecisionEndDatetime = m_strDecisionEndTimeBeta;
                m_strDecisionEndTime = m_clsHeaderData.strDecisionEndDatetime;

                m_bolXButton = false;
                bolProcOkNg = true;
            }
            finally
            {
                frmProgress.Close();
                frmProgress.Dispose();
                dtData.Dispose();

                m_bolXButtonDisable = false;

                if (bolProcOkNg == true)
                {
                    g_clsConnectionNpgsql.DbClose();

                    // 登録
                    bolReg = true;

                    this.Close();
                }
            }
        }

        /// <summary>
        /// 合否確認へ戻るボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAcceptanceCheck_Click(object sender, EventArgs e)
        {
            int intRowIdx = -1;

            // 選択行インデックスの取得
            foreach (DataGridViewRow dgvRow in this.dgvDecisionResult.SelectedRows)
            {
                intRowIdx = dgvRow.Index;
                break;
            }

            if (intRowIdx == -1)
            {
                return;
            }

            clsDecisionResult = new DecisionResult();
            clsDecisionResult.intBranchNum = int.Parse(m_dtData.Rows[intRowIdx]["branch_num"].ToString());
            clsDecisionResult.intLine = int.Parse(m_dtData.Rows[intRowIdx]["line"].ToString());
            clsDecisionResult.strCloumns = m_dtData.Rows[intRowIdx]["cloumns"].ToString();
            clsDecisionResult.strNgReason = m_dtData.Rows[intRowIdx]["ng_reason"].ToString();
            clsDecisionResult.strMarkingImagepath = m_dtData.Rows[intRowIdx]["marking_imagepath"].ToString();
            clsDecisionResult.strOrgImagepath = m_dtData.Rows[intRowIdx]["org_imagepath"].ToString();

            // 選択行情報を保持
            m_intSelBranchNum = clsDecisionResult.intBranchNum;
            m_strSelMarkingImagepath = clsDecisionResult.strOrgImagepath;

            // 修正する
            bolMod = true;

            m_bolXButton = false;
            this.Close();
        }

        /// <summary>
        /// 一覧クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDecisionResult_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            using (ViewEnlargedimage frmViewEnlargedimage =
                new ViewEnlargedimage(
                    Path.Combine(
                        m_clsHeaderData.strFaultImageDirectory,
                        m_strFaultImageSubDirectory,
                        m_dtData.Rows[e.RowIndex]["org_imagepath"].ToString()),
                    Path.Combine(
                        m_clsHeaderData.strFaultImageDirectory,
                        m_strFaultImageSubDirectory,
                        m_dtData.Rows[e.RowIndex]["marking_imagepath"].ToString())))
            {
                frmViewEnlargedimage.ShowDialog(this);
                this.Visible = true;
            }
        }

        /// <summary>
        ///ログアウトクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (m_intAcceptanceCheckStatus != g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd)
            {
                // 合否確認ステータス更新(中断)
                if (blnUpdAcceptanceCheckStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                            g_clsSystemSettingInfo.intAcceptanceCheckStatusStp) == false)
                {
                    // エラー時
                    g_clsConnectionNpgsql.DbRollback();
                    g_clsConnectionNpgsql.DbClose();

                    return;
                }
            }

            if (m_intFromApId != 0)
            {
                // 変更破棄
                if (bolDisposeUpd(g_CON_APID_LOGIN) == false)
                {
                    return;
                }
            }

            g_clsConnectionNpgsql.DbCommit();
            g_clsConnectionNpgsql.DbClose();

            m_bolXButton = false;
            g_clsLoginInfo.Logout();
        }

        #region 横スクロール対応
        Point mouseDownPosition;
        /// <summary>
        /// 一覧マウスオーバー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDecisionResult_MouseMove(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:

                    if (mouseDownPosition.X != e.Location.X)
                    {
                        try
                        {
                            int movePosi_X = dgvDecisionResult.HorizontalScrollingOffset - (e.Location.X - mouseDownPosition.X);

                            if (movePosi_X < 10)
                            {
                                movePosi_X = 0;
                            }

                            dgvDecisionResult.HorizontalScrollingOffset = movePosi_X;
                        }
                        catch (Exception)
                        {

                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 一覧マウスダウン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDecisionResult_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPosition = e.Location;
        }
        #endregion
        #endregion

        #region フォームクローズ
        /// <summary>
        /// フォームクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Result_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 判定登録処理中は無効にする
            if (m_bolXButtonDisable == true)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // [X]ボタンを押下していない場合は終了
            if (m_bolXButton == false)
            {
                return;
            }

            if (m_intFromApId != 0)
            {
                // 変更破棄
                if (bolDisposeUpd(g_CON_APID_TARGET_SELECTION) == false)
                {
                    if (e.CloseReason == CloseReason.UserClosing)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }
        #endregion

        #region 最大化画面制御
        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0x00A3;
            const int WM_SYSCOMMAND = 0x0112;
            const long SC_MOVE = 0xF010L;

            // ダブルクリック禁止
            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                return;
            }

            // フォーム移動禁止
            if (m.Msg == WM_SYSCOMMAND &&
                (m.WParam.ToInt64() & 0xFFF0L) == SC_MOVE)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }
        #endregion
    }
}