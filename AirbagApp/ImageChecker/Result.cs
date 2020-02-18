﻿using ImageChecker.DTO;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class Result : Form
    {
        public bool bolMod { get; set; }

        // 判定結果情報
        public DecisionResult clsDecisionResult { get; set; }

        // パラメータ関連
        private HeaderData m_clsHeaderData;             // ヘッダ情報
        private string m_strUnitNum = "";               // 号機
        private string m_strProductName = "";           // 品名
        private string m_strOrderImg = "";              // 指図
        private string m_strFabricName = "";            // 反番
        private string m_strInspectionDate = "";        // 検査日付
        private string m_strStartDatetime = "";         // 搬送開始日時
        private string m_strEndDatetime = "";           // 搬送終了日時
        private int m_intInspectionStartLine = -1;      // 検査開始行
        private int m_intInspectionEndLine = -1;        // 最終行数
        private int m_intInspectionTargetLine = -1;     // 検査対象数
        private string m_strDecisionStartTime = "";     // 判定開始日時
        private string m_strDecisionEndTime = "";       // 判定終了日時
        private string m_strInspectionDirection = "";   // 検査方向
        private int m_intInspectionNum = 0;             // 検査番号
        private int m_intAcceptanceCheckStatus = 0;     // 合否確認ステータス
        private int m_intColumnCnt = 0;                 // 列数
        private int m_intNgCushionCount = 0;            // NGクッション数
        private int m_intFromApId = 0;                  // 遷移元画面ID

        // 定数
        private const string m_CON_FORMAT_UNIT_NUM = "号機：{0}";
        private const string m_CON_FORMAT_PRODUCT_NAME = "品名：{0}";
        private const string m_CON_FORMAT_ORDER_IMG = "指図：{0}";
        private const string m_CON_FORMAT_FABRIC_NAME = "反番：{0}";
        private const string m_CON_FORMAT_START_DATETIME = "搬送開始日時：{0}";
        private const string m_CON_FORMAT_END_DATETIME = "搬送終了日時：{0}";
        private const string m_CON_FORMAT_INSPECTION_LINE = "検査範囲行：{0}～{1}";
        private const string m_CON_FORMAT_DECISION_START_DATETIME = "判定開始日時：{0}";
        private const string m_CON_FORMAT_DECISION_END_DATETIME = "判定終了日時：{0}";
        private const string m_CON_FORMAT_INSPECTION_NUM = "検査番号：{0}";
        private const string m_CON_FORMAT_WORKER_NAME = "作業者：{0}";
        private const string m_CON_FORMAT_NG_DISTANCE = "{0},{1}";
        private const string m_CON_FORMAT_IMAGE_INSPECTION_COUNT = "画像検査枚数：{0}(NG：{1}/OK：{2})";
        private const string m_CON_FORMAT_CUSHION_COUNT = "クッション数：{0}(NG：{1}/OK：{2})";
        private const string m_CON_FORMAT_KEN_TAN_CHECK_SHEET_CSV_NAME = "{0}_N.csv";

        // 欠点画像サブディレクトリパス
        private string m_strFaultImageSubDirectory = "";

        // 判定終了日時（仮）　※判定登録でDBに登録できるまでは未確定。印刷とDB登録に使用
        private string m_strDecisionEndTimeBeta = "";

        // データ保持関連
        private DataTable m_dtData;

        // [X]ボタン判定
        private bool m_bolXButton = true;

        // [X]ボタン無効
        private bool m_bolXButtonDisable = false;

        // PDF作成関連
        private IList<Stream> m_streams;
        private const int m_DETAIL_CNT = 7;
        private byte[] bytes;
        LocalReport lReport;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clsHeaderData">ヘッダ情報</param>
        /// <param name="clsDecisionResultBef">判定結果情報(更新前)</param>
        /// <param name="intFromApId">遷移元画面ID</param>
        public Result(ref HeaderData clsHeaderData, int intFromApId = 0)
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

            m_strFaultImageSubDirectory = string.Join("_", m_strInspectionDate.Replace("/", ""),
                                                           m_strProductName,
                                                           m_strFabricName,
                                                           m_intInspectionNum);

            bolMod = false;
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
        public static Boolean blnUpdAcceptanceCheckStatus(string strFabricName,
                                                          string strInspectionDate,
                                                          int intInspectionNum,
                                                          int intStatus,
                                                          string strEndDatetime = "")
        {
            string strSQL = "";
            try
            {
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // SQL文を作成する
                strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                              SET acceptance_check_status = :acceptance_check_status ";

                if (strEndDatetime != "")
                {
                    strSQL += @", decision_end_datetime = TO_TIMESTAMP(:decision_end_datetime_yyyymmdd_hhmmss, 'YYYY/MM/DD HH24:MI:SS') ";
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "decision_end_datetime_yyyymmdd_hhmmss", DbType = DbType.String, Value = strEndDatetime });
                }

                strSQL += @"WHERE fabric_name = :fabric_name
                              AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                              AND inspection_num = :inspection_num";

                // SQLコマンドに各パラメータを設定する
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_status", DbType = DbType.Int16, Value = intStatus });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0002 + "\r\n" + ex.Message);
                // メッセージ出力
                System.Windows.Forms.MessageBox.Show(g_clsMessageInfo.strMsgE0035, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
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

            string strSQL = "";
            DataTable dtData;
            ArrayList arrRow = new ArrayList();
            string stResultName = "";
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

                // 列のスタイル変更
                this.dgvDecisionResult.Columns[0].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;     //№

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

                dtData = new DataTable();
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
                                            AND   acceptance_check_result IN (:acceptance_check_result_ngdetect,
                                                                              :acceptance_check_result_ngnondetect)) AS image_inspection_count_ng
                                         , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                            WHERE fabric_name = :fabric_name
                                            AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                            AND   inspection_num = :inspection_num
                                            AND   acceptance_check_result = :acceptance_check_result_ok) AS image_inspection_count_ok
                                       FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                       WHERE fabric_name = :fabric_name
                                       AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                       AND   inspection_num = :inspection_num
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
                    WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
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
                    WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
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
                             , TO_CHAR(acceptance_check_datetime,'YYYY/MM/DD HH24:MI:SS') AS acceptance_check_datetime
                             , acceptance_check_worker
                             , org_imagepath
                             , marking_imagepath
                           FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                           AND   inspection_num = :inspection_num
                           AND   over_detection_except_result <> :over_detection_except_result_ok 
                           ORDER BY ";

                    if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS)
                        strSQL += "line ASC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                    else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
                        strSQL += "line ASC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                    else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY)
                        strSQL += "line ASC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                    else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
                        strSQL += "line DESC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });

                    g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);

                    // データグリッドに反映
                    foreach (DataRow row in m_dtData.Rows)
                    {
                        arrRow = new ArrayList();

                        // No
                        arrRow.Add(this.dgvDecisionResult.Rows.Count + 1);

                        // SQL抽出項目
                        arrRow.Add(row["line"]);
                        arrRow.Add(row["cloumns"]);
                        arrRow.Add(row["ng_face"]);
                        arrRow.Add(string.Format(m_CON_FORMAT_NG_DISTANCE, row["ng_distance_x"].ToString(), row["ng_distance_y"].ToString()));

                        // 過検知除外結果：名称を表示
                        if (int.Parse(row["over_detection_except_result"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptResultNon)
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameNon;
                        else if (int.Parse(row["over_detection_except_result"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptResultOk)
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameOk;
                        else if (int.Parse(row["over_detection_except_result"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptResultNg)
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameNg;
                        arrRow.Add(stResultName);

                        // 合否確認結果：名称を表示
                        if (int.Parse(row["acceptance_check_result"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckResultNon)
                            stResultName = g_clsSystemSettingInfo.strAcceptanceCheckResultNameNon;
                        else if (int.Parse(row["acceptance_check_result"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckResultOk)
                            stResultName = g_clsSystemSettingInfo.strAcceptanceCheckResultNameOk;
                        else if (int.Parse(row["acceptance_check_result"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckResultNgDetect)
                            stResultName = g_clsSystemSettingInfo.strAcceptanceCheckResultNameNgDetect;
                        else if (int.Parse(row["acceptance_check_result"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckResultNgNonDetect)
                            stResultName = g_clsSystemSettingInfo.strAcceptanceCheckResultNameNgNonDetect;
                        arrRow.Add(stResultName);

                        arrRow.Add(row["ng_reason"]);
                        arrRow.Add(row["over_detection_except_datetime"]);
                        arrRow.Add(row["over_detection_except_worker"]);
                        arrRow.Add(row["acceptance_check_datetime"]);
                        arrRow.Add(row["acceptance_check_worker"]);

                        this.dgvDecisionResult.Rows.Add(arrRow.ToArray());
                    }

                    bolProcOkNg = true;
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

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
            string strApName = "";

            // 遷移画面名の設定
            switch (intToApId)
            {
                case g_CON_APID_LOGIN:
                    strApName = "ログイン";
                    break;
                case g_CON_APID_DISPLAY_RESULTS:
                    strApName = "検査結果確認";
                    break;
                case g_CON_APID_DISPLAY_RESULTS_AGO:
                    strApName = "検査履歴照会";
                    break;
            }

            // 画面名チェック
            if (string.IsNullOrEmpty(strApName) == true)
            {
                return true;
            }

            // メッセージ表示
            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0013, strApName),
                                "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
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
            if (m_intFromApId != 0)
            {
                // 変更破棄
                if (bolDisposeUpd(m_intFromApId) == false)
                {
                    return;
                }
            }

            m_bolXButton = false;
            this.Close();
        }

        /// <summary>
        /// 判定登録クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRegstDecision_Click(object sender, EventArgs e)
        {
            bool bolProcOkNg = false;

            string strSQL = "";
            DataTable dtData;
            string strWriteLine = "";
            List<Control> lstctlEnable = null;

            if (MessageBox.Show(g_clsMessageInfo.strMsgQ0012, "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                // コントロール無効
                m_bolXButtonDisable = true;
                lstctlEnable = new List<Control>();
                lstctlEnable.Add(btnLogout);
                lstctlEnable.Add(btnTargetSelection);
                lstctlEnable.Add(btnAcceptanceCheck);
                lstctlEnable.Add(btnRegstDecision);
                lstctlEnable.Add(dgvDecisionResult);

                foreach (Control ctr in lstctlEnable)
                {
                    ctr.Enabled = false;
                }

                // 終了日時(仮)を保持
                m_strDecisionEndTimeBeta = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                // 合否確認ステータス更新(検査完了)
                if (blnUpdAcceptanceCheckStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd,
                                                m_strDecisionEndTimeBeta) == false)
                    return;

                // 帳票印刷
                try
                {
                    dtData = new DataTable();
                    strSQL = @"SELECT
                                   TO_CHAR(iih.start_datetime,'YYYY/MM/DD HH24:MI') AS start_datetime
                                 , TO_CHAR(iih.end_datetime,'YYYY/MM/DD HH24:MI') AS end_datetime
                                 , TO_CHAR(iih.decision_start_datetime,'YYYY/MM/DD HH24:MI') AS decision_start_datetime
                                 , TO_CHAR(iih.decision_end_datetime,'YYYY/MM/DD HH24:MI') AS decision_end_datetime
                                 , iih.order_img
                                 , iih.fabric_name
                                 , iih.product_name
                                 , iih.unit_num
                                 , iih.inspection_start_line
                                 , iih.inspection_end_line
                                 , iih.inspection_num
                                 , TO_CHAR(dr.acceptance_check_datetime,'HH24:MI:SS') AS acceptance_check_datetime
                                 , dr.line
                                 , dr.cloumns
                                 , dr.ng_face
                                 , dr.ng_reason
                                 , dr.ng_distance_x
                                 , dr.ng_distance_y
                                 , dr.acceptance_check_worker
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result dr
                               INNER JOIN " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header iih
                               ON  dr.fabric_name = iih.fabric_name
                               AND dr.inspection_date = iih.inspection_date
                               AND dr.inspection_num = iih.inspection_num
                               WHERE dr.fabric_name = :fabric_name
                               AND   dr.inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                               AND   dr.inspection_num = :inspection_num
                               AND   dr.ng_reason IS NOT NULL 
                               ORDER BY 
                                   dr.line ASC 
                                 , dr.cloumns ASC 
                                 , dr.ng_face ASC 
                                 , dr.over_detection_except_datetime ASC ";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    // LocalReport作成
                    lReport = new LocalReport();
                    lReport.ReportPath = ".\\KenTanChkSheet.rdlc";
                    m_streams = new List<Stream>();

                    KenTanChkSheet KTCSDs = new KenTanChkSheet();
                    foreach (DataRow dr in dtData.Rows)
                    {
                        KenTanChkSheet.KenTanChkSheetTableRow KTCSDr = KTCSDs.KenTanChkSheetTable.NewKenTanChkSheetTableRow();
                        KTCSDr.BeginEdit();

                        // ヘッダ情報
                        KTCSDr.OrderImg = dr["order_img"].ToString();
                        KTCSDr.ProductName = dr["product_name"].ToString();
                        KTCSDr.FabricName = dr["fabric_name"].ToString();
                        KTCSDr.UnitNum = dr["unit_num"].ToString();
                        KTCSDr.StartDatetime = dr["start_datetime"].ToString();
                        KTCSDr.EndDatetime = dr["end_datetime"].ToString();
                        KTCSDr.InspectionLine = dr["inspection_start_line"].ToString() + "～" + dr["inspection_end_line"].ToString();
                        KTCSDr.InspectionNum = dr["inspection_num"].ToString();
                        KTCSDr.DecisionStartDatetime = dr["decision_start_datetime"].ToString();
                        KTCSDr.DecisionEndDatetime = dr["decision_end_datetime"].ToString();
                        KTCSDr.NgCushionCnt = m_intNgCushionCount.ToString();
                        KTCSDr.NgImageCnt = m_dtData.Select("ng_reason <> ''").Length.ToString();

                        // 明細情報
                        KTCSDr.AcceptanceCheckDatetime = dr["acceptance_check_datetime"].ToString();
                        KTCSDr.Line = dr["line"].ToString();
                        KTCSDr.Cloumns = dr["cloumns"].ToString();
                        KTCSDr.NgFace = dr["ng_face"].ToString();
                        KTCSDr.NgDistanceXY = dr["ng_distance_x"].ToString() + "," + dr["ng_distance_y"].ToString();
                        KTCSDr.NgReason = dr["ng_reason"].ToString();
                        KTCSDr.AcceptanceCheckWorker = dr["acceptance_check_worker"].ToString();

                        KTCSDr.EndEdit();
                        KTCSDs.KenTanChkSheetTable.Rows.Add(KTCSDr);
                    }

                    // PDF作成
                    lReport.DataSources.Add(new ReportDataSource("KenTanChkSheet", (DataTable)KTCSDs.KenTanChkSheetTable));
                    CreatePDF();
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);
                    // メッセージ出力
                    System.Windows.Forms.MessageBox.Show(g_clsMessageInfo.strMsgE0054, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // 検査結果CSV作成
                try
                {
                    dtData = new DataTable();
                    strSQL = @"SELECT
                                   TO_CHAR(dr.acceptance_check_datetime,'YYYY/MM/DD HH24:MI:SS') AS acceptance_check_datetime
                                 , iih.order_img
                                 , iih.fabric_name
                                 , dr.line
                                 , dr.cloumns
                                 , dr.ng_face
                                 , dr.ng_reason
                                 , TO_CHAR(dr.inspection_num, 'FM00000000') AS inspection_num
                                 , dr.ng_distance_x
                                 , dr.ng_distance_y
                                 , dr.acceptance_check_worker
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result dr
                               INNER JOIN " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header iih
                               ON  dr.fabric_name = iih.fabric_name
                               WHERE dr.fabric_name = :fabric_name
                               AND   dr.ng_reason IS NOT NULL 
                               AND   iih.acceptance_check_status = :acceptance_check_status_end
                               ORDER BY 
                                   dr.inspection_num ASC 
                                 , dr.line ASC 
                                 , dr.cloumns ASC 
                                 , dr.ng_face ASC 
                                 , dr.over_detection_except_datetime ASC ";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_status_end", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    // 保存先ディレクトリに作成
                    // Shift JISで書き込む
                    // 書き込むファイルが既に存在している場合は、上書きする
                    using (StreamWriter sw = new StreamWriter(g_clsSystemSettingInfo.strInspectionResltCsvDirectory + @"\" +
                                                              string.Format(m_CON_FORMAT_KEN_TAN_CHECK_SHEET_CSV_NAME, m_strFabricName)
                                                            , false
                                                            , Encoding.GetEncoding("shift_jis")))
                    {
                        // １行目
                        strWriteLine = "";
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
                            strWriteLine = "";
                            strWriteLine += dr["acceptance_check_datetime"].ToString();
                            strWriteLine += "," + dr["order_img"].ToString();
                            strWriteLine += "," + dr["fabric_name"].ToString();
                            strWriteLine += "," + dr["line"].ToString();

                            if (dr["cloumns"].ToString() == "A")
                                strWriteLine += ",Ａ";
                            else if (dr["cloumns"].ToString() == "B")
                                strWriteLine += ",Ｂ";
                            else if (dr["cloumns"].ToString() == "C")
                                strWriteLine += ",Ｃ";
                            else if (dr["cloumns"].ToString() == "D")
                                strWriteLine += ",Ｄ";
                            else if (dr["cloumns"].ToString() == "E")
                                strWriteLine += ",Ｅ";
                            else 
                                strWriteLine += "," + dr["cloumns"].ToString();

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
                    File.Copy(g_clsSystemSettingInfo.strInspectionResltCsvDirectory + @"\" +
                              string.Format(m_CON_FORMAT_KEN_TAN_CHECK_SHEET_CSV_NAME, m_strFabricName),
                              g_clsSystemSettingInfo.strProductionManagementCooperationDirectory + @"\" +
                              string.Format(m_CON_FORMAT_KEN_TAN_CHECK_SHEET_CSV_NAME, m_strFabricName), true);
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);
                    // メッセージ出力
                    System.Windows.Forms.MessageBox.Show(g_clsMessageInfo.strMsgE0054, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
                g_clsConnectionNpgsql.DbClose();

                if (lstctlEnable != null)
                {
                    foreach (Control ctr in lstctlEnable)
                    {
                        ctr.Enabled = true;
                    }
                }
                m_bolXButtonDisable = false;

                if (bolProcOkNg == true)
                {
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
            int intSelIdx = -1;

            // 選択行インデックスの取得
            foreach (DataGridViewRow dgvRow in this.dgvDecisionResult.SelectedRows)
            {
                intSelIdx = dgvRow.Index;
                break;
            }

            if (intSelIdx == -1)
            {
                return;
            }

            clsDecisionResult = new DecisionResult();
            clsDecisionResult.intBranchNum = int.Parse(m_dtData.Rows[intSelIdx]["branch_num"].ToString());
            clsDecisionResult.intLine = int.Parse(m_dtData.Rows[intSelIdx]["line"].ToString());
            clsDecisionResult.strCloumns = m_dtData.Rows[intSelIdx]["cloumns"].ToString();
            clsDecisionResult.strNgReason = m_dtData.Rows[intSelIdx]["ng_reason"].ToString();
            clsDecisionResult.strMarkingImagepath = m_dtData.Rows[intSelIdx]["marking_imagepath"].ToString();
            clsDecisionResult.strOrgImagepath = m_dtData.Rows[intSelIdx]["org_imagepath"].ToString();

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
                return;

            ViewEnlargedimage frmViewEnlargedimage = new ViewEnlargedimage(g_clsSystemSettingInfo.strFaultImageDirectory + @"\" +
                                                                           m_strFaultImageSubDirectory + @"\" +
                                                                           m_dtData.Rows[e.RowIndex]["org_imagepath"].ToString(),
                                                                           g_clsSystemSettingInfo.strFaultImageDirectory + @"\" +
                                                                           m_strFaultImageSubDirectory + @"\" +
                                                                           m_dtData.Rows[e.RowIndex]["marking_imagepath"].ToString());
            frmViewEnlargedimage.ShowDialog(this);
            this.Visible = true;
        }

        /// <summary>
        ///ログアウトクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (m_intFromApId != 0)
            {
                // 変更破棄
                if (bolDisposeUpd(g_CON_APID_LOGIN) == false)
                {
                    return;
                }
            }
            
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

        #region PDF作成関連
        private void CreatePDF()
        {
            try
            {
                bytes = lReport.Render("PDF");

                AutoPrintCls autoprintme = new AutoPrintCls(lReport);
                autoprintme.Print();

                // 一時フォルダにPDFを作成
                string fileName = "Report" + DateTime.Now.ToString("yyyyMMddhhmmssfff") + ".pdf";
                using (FileStream fs = new FileStream(g_clsSystemSettingInfo.strTemporaryDirectory + @"\" + fileName, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                lReport.Dispose();
            }
        }
        #endregion

        #region LocalReport印刷用クラス
        /// <summary>
        /// The ReportPrintDocument will print all of the pages of a ServerReport or LocalReport.
        /// The pages are rendered when the print document is constructed.  Once constructed,
        /// call Print() on this class to begin printing.
        /// </summary>
        class AutoPrintCls : PrintDocument
        {
            private PageSettings m_pageSettings;
            private int m_currentPage;
            private List<Stream> m_pages = new List<Stream>();

            public AutoPrintCls(ServerReport serverReport)
                : this((Report)serverReport)
            {
                RenderAllServerReportPages(serverReport);
            }

            public AutoPrintCls(LocalReport localReport)
                : this((Report)localReport)
            {
                RenderAllLocalReportPages(localReport);
            }

            private AutoPrintCls(Report report)
            {
                // Set the page settings to the default defined in the report
                ReportPageSettings reportPageSettings = report.GetDefaultPageSettings();

                // The page settings object will use the default printer unless
                // PageSettings.PrinterSettings is changed.  This assumes there
                // is a default printer.
                m_pageSettings = new PageSettings();
                m_pageSettings.PaperSize = reportPageSettings.PaperSize;
                m_pageSettings.Margins = reportPageSettings.Margins;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    foreach (Stream s in m_pages)
                    {
                        s.Dispose();
                    }

                    m_pages.Clear();
                }
            }

            protected override void OnBeginPrint(PrintEventArgs e)
            {
                base.OnBeginPrint(e);

                m_currentPage = 0;
            }

            protected override void OnPrintPage(PrintPageEventArgs e)
            {
                base.OnPrintPage(e);

                Stream pageToPrint = m_pages[m_currentPage];
                pageToPrint.Position = 0;

                // Load each page into a Metafile to draw it.
                using (Metafile pageMetaFile = new Metafile(pageToPrint))
                {
                    Rectangle adjustedRect = new Rectangle(
                            e.PageBounds.Left - (int)e.PageSettings.HardMarginX,
                            e.PageBounds.Top - (int)e.PageSettings.HardMarginY,
                            e.PageBounds.Width,
                            e.PageBounds.Height);

                    // Draw a white background for the report
                    e.Graphics.FillRectangle(Brushes.White, adjustedRect);

                    // Draw the report content
                    e.Graphics.DrawImage(pageMetaFile, adjustedRect);

                    // Prepare for next page.  Make sure we haven't hit the end.
                    m_currentPage++;
                    e.HasMorePages = m_currentPage < m_pages.Count;
                }
            }

            protected override void OnQueryPageSettings(QueryPageSettingsEventArgs e)
            {
                e.PageSettings = (PageSettings)m_pageSettings.Clone();
            }

            private void RenderAllServerReportPages(ServerReport serverReport)
            {
                try
                {
                    string deviceInfo = CreateEMFDeviceInfo();

                    // Generating Image renderer pages one at a time can be expensive.  In order
                    // to generate page 2, the server would need to recalculate page 1 and throw it
                    // away.  Using PersistStreams causes the server to generate all the pages in
                    // the background but return as soon as page 1 is complete.
                    System.Collections.Specialized.NameValueCollection firstPageParameters = new NameValueCollection();
                    firstPageParameters.Add("rs:PersistStreams", "True");

                    // GetNextStream returns the next page in the sequence from the background process
                    // started by PersistStreams.
                    NameValueCollection nonFirstPageParameters = new NameValueCollection();
                    nonFirstPageParameters.Add("rs:GetNextStream", "True");

                    string mimeType;
                    string fileExtension;


                    Stream pageStream = serverReport.Render("IMAGE", deviceInfo, firstPageParameters, out mimeType, out fileExtension);



                    // The server returns an empty stream when moving beyond the last page.
                    while (pageStream.Length > 0)
                    {
                        m_pages.Add(pageStream);

                        pageStream = serverReport.Render("IMAGE", deviceInfo, nonFirstPageParameters, out mimeType, out fileExtension);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("possible missing information ::  " + e);
                }
            }

            private void RenderAllLocalReportPages(LocalReport localReport)
            {
                try
                {
                    string deviceInfo = CreateEMFDeviceInfo();

                    Warning[] warnings;

                    localReport.Render("IMAGE", deviceInfo, LocalReportCreateStreamCallback, out warnings);
                }
                catch (Exception e)
                {
                    MessageBox.Show("error :: " + e);
                }
            }

            private Stream LocalReportCreateStreamCallback(
                string name,
                string extension,
                Encoding encoding,
                string mimeType,
                bool willSeek)
            {
                MemoryStream stream = new MemoryStream();
                m_pages.Add(stream);

                return stream;
            }

            private string CreateEMFDeviceInfo()
            {
                PaperSize paperSize = m_pageSettings.PaperSize;
                Margins margins = m_pageSettings.Margins;

                // The device info string defines the page range to print as well as the size of the page.
                // A start and end page of 0 means generate all pages.
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "<DeviceInfo><OutputFormat>emf</OutputFormat><StartPage>0</StartPage><EndPage>0</EndPage><MarginTop>{0}</MarginTop><MarginLeft>{1}</MarginLeft><MarginRight>{2}</MarginRight><MarginBottom>{3}</MarginBottom><PageHeight>{4}</PageHeight><PageWidth>{5}</PageWidth></DeviceInfo>",
                    ToInches(margins.Top),
                    ToInches(margins.Left),
                    ToInches(margins.Right),
                    ToInches(margins.Bottom),
                    ToInches(paperSize.Height),
                    ToInches(paperSize.Width));
            }

            private static string ToInches(int hundrethsOfInch)
            {
                double inches = hundrethsOfInch / 100.0;
                return inches.ToString(CultureInfo.InvariantCulture) + "in";
            }
        }
        #endregion

        /// <summary>
        /// フォームクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Result_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_bolXButtonDisable == true)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (m_bolXButton == false)
            {
                return;
            }

            if (m_intFromApId != 0)
            {
                // 変更破棄
                if (bolDisposeUpd(m_intFromApId) == false)
                {
                    if (e.CloseReason == CloseReason.UserClosing)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }
    }
}
