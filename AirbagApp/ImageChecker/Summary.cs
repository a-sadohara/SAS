﻿using ImageChecker.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class Summary : Form
    {
        // 選択行保持
        public int intSelIdx { get; set; }
        public int intFirstDisplayedScrollingRowIdx { get; set; }

        /// <summary>
        /// 遷移先
        /// </summary>
        public int intDestination { get; set; }

        // パラメータ関連（不変）
        private readonly string m_strUnitNum = string.Empty;                    // 号機
        private readonly string m_strProductName = string.Empty;                // 品名
        private readonly string m_strOrderImg = string.Empty;                   // 指図
        private readonly string m_strFabricName = string.Empty;                 // 反番
        private readonly string m_strInspectionDate = string.Empty;             // 検査日付
        private readonly string m_strStartDatetime = string.Empty;              // 搬送開始日時
        private readonly string m_strEndDatetime = string.Empty;                // 搬送終了日時
        private readonly int m_intInspectionStartLine = -1;                     // 検査開始行
        private readonly int m_intInspectionEndLine = -1;                       // 最終行数
        private readonly int m_intInspectionTargetLine = -1;                    // 検査対象数
        private readonly string m_strInspectionDirection = string.Empty;        // 検査方向
        private readonly int m_intInspectionNum = 0;                            // 検査番号
        private readonly int m_intColumnCnt = 0;                                // 列数

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

        // 欠点画像サブディレクトリパス
        private readonly string m_strFaultImageSubDirName = string.Empty;   // 欠点画像サブディレクトリ名
        private readonly string m_strFaultImageSubDirPath = string.Empty;   // 欠点画像サブディレクトリパス

        // フラグ関連
        private bool m_bolRoadFlg = false;       // ロード済み　※true : フォームロード処理が正常完了
                                                 // 　　　　　　　false: それ以外
                                                 // 　　　　　　　用途 : 過検知除外修正せずに本画面を閉じた時、ステータスを完了にするか否か制御する

        // データ保持関連
        private DataTable m_dtData;

        // クリックイベントとダブルクリックイベントの同時実装関連
        private readonly SemaphoreSlim _clickSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _doubleClickSemaphore = new SemaphoreSlim(0);

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clsHeaderData">ヘッダ情報</param>
        public Summary(HeaderData clsHeaderData, int intSelIndex, int intFirstDisplayedScrollingRowIndex)
        {
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
            m_strInspectionDirection = clsHeaderData.strInspectionDirection;
            m_intInspectionNum = clsHeaderData.intInspectionNum;
            m_intColumnCnt = clsHeaderData.intColumnCnt;

            intSelIdx = intSelIndex;
            intFirstDisplayedScrollingRowIdx = intFirstDisplayedScrollingRowIndex;
            intDestination = 0;

            m_strFaultImageSubDirName = string.Join("_", m_strInspectionDate.Replace("/", string.Empty),
                                                         m_strProductName,
                                                         m_strFabricName,
                                                         m_intInspectionNum);

            m_strFaultImageSubDirPath = Path.Combine(clsHeaderData.strFaultImageDirectory,
                                                     m_strFaultImageSubDirName);

            InitializeComponent();
        }

        /// <summary>
        /// 過検知除外ステータス更新
        /// </summary>
        /// <param name="strFabricName">反番</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="intStatus">ステータス</param>
        /// <param name="strStartDatetime">判定開始日時(YYYY/MM//DD HH:MM:SS)</param>
        /// <returns></returns>
        public Boolean blnUpdOverDetectionExceptStatus(string strFabricName,
                                                              string strInspectionDate,
                                                              int intInspectionNum,
                                                              int intStatus)
        {
            string strSQL = string.Empty;
            try
            {
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // SQL文を作成する
                strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                              SET over_detection_except_status = :over_detection_except_status
                            WHERE fabric_name = :fabric_name
                              AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                              AND inspection_num = :inspection_num
                              AND unit_num = :unit_num";

                // SQLコマンドに各パラメータを設定する
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_status", DbType = DbType.Int16, Value = intStatus });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
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
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Summary_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            bool bolProcOkNg = false;

            string strSQL = string.Empty;
            DataTable dtData;
            ArrayList arrRow = new ArrayList();
            string stResultName = string.Empty;
            int intImageInspectionCount = -1;
            int intImageInspectionCountOk = -1;
            int intImageInspectionCountNg = -1;
            int intCushionInspectionCount = -1;
            int intCushionInspectionCountOk = -1;
            int intCushionInspectionCountNg = -1;

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
            lblDecisionStartTime.Text = string.Format(m_CON_FORMAT_DECISION_START_DATETIME, string.Empty);
            lblDecisionEndTime.Text = string.Format(m_CON_FORMAT_DECISION_END_DATETIME, string.Empty);
            lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum);

            // 判定日時を取得
            dtData = new DataTable();
            try
            {
                strSQL = @"SELECT
                               TO_CHAR(decision_start_datetime,'YYYY/MM/DD HH24:MI:SS') AS decision_start_datetime
                             , TO_CHAR(decision_end_datetime,'YYYY/MM/DD HH24:MI:SS') AS decision_end_datetime
                             FROM " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                            WHERE fabric_name = :fabric_name
                              AND inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                              AND inspection_num = :inspection_num
                              AND unit_num = :unit_num";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                // ヘッダ表示
                lblDecisionStartTime.Text = string.Format(m_CON_FORMAT_DECISION_START_DATETIME,
                                                          dtData.Rows[0]["decision_start_datetime"].ToString());    // 判定開始日時
                lblDecisionEndTime.Text = string.Format(m_CON_FORMAT_DECISION_END_DATETIME,
                                                        dtData.Rows[0]["decision_end_datetime"].ToString());        // 判定終了日時
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

            try
            {
                // 列のスタイル変更
                this.dgvDecisionResult.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;     //№

                // カウント系のヘッダ表示
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
                                            AND   unit_num = :unit_num
                                            AND   over_detection_except_result = :over_detection_except_result_ng) AS image_inspection_count_ng
                                         , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                            WHERE fabric_name = :fabric_name
                                            AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                            AND   inspection_num = :inspection_num
                                            AND   unit_num = :unit_num
                                            AND   over_detection_except_result = :over_detection_except_result_ok) AS image_inspection_count_ok
                                       FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                       WHERE fabric_name = :fabric_name
                                       AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                       AND   inspection_num = :inspection_num
                                       AND   unit_num = :unit_num
                                   ) imgcnt";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ng", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNg });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    intImageInspectionCount = Convert.ToInt32(dtData.Rows[0]["image_inspection_count"]);
                    intImageInspectionCountNg = Convert.ToInt32(dtData.Rows[0]["image_inspection_count_ng"]);
                    intImageInspectionCountOk = Convert.ToInt32(dtData.Rows[0]["image_inspection_count_ok"]);

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
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
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
                                   SELECT DISTINCT
                                         line
                                       , cloumns
                                     FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                    WHERE fabric_name = :fabric_name
                                      AND inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                      AND inspection_num = :inspection_num
                                      AND unit_num = :unit_num
                                      AND over_detection_except_result = :over_detection_except_result_ng
                                     ) imgcnt";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ng", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNg });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    // クッション数を算出
                    intCushionInspectionCount = m_intColumnCnt * m_intInspectionTargetLine;
                    intCushionInspectionCountNg = Convert.ToInt32(dtData.Rows[0]["cnt"]);
                    intCushionInspectionCountOk = intCushionInspectionCount - intCushionInspectionCountNg;

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
                               line
                             , cloumns
                             , ng_face
                             , ng_distance_x
                             , ng_distance_y
                             , over_detection_except_result
                             , NULL AS ng_reason
                             , TO_CHAR(over_detection_except_datetime,'YYYY/MM/DD HH24:MI:SS') AS over_detection_except_datetime
                             , over_detection_except_worker
                             , org_imagepath
                             , marking_imagepath
                           FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                           AND   inspection_num = :inspection_num
                           AND   unit_num = :unit_num
                           ORDER BY ";

                    if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS)
                        strSQL += "line ASC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                    else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
                        strSQL += "line ASC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                    else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY)
                        strSQL += "line DESC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                    else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
                        strSQL += "line DESC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

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
                        if (Convert.ToInt32(row["over_detection_except_result"]) == g_clsSystemSettingInfo.intOverDetectionExceptResultNon)
                        {
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameNon;
                        }
                        if (Convert.ToInt32(row["over_detection_except_result"]) == g_clsSystemSettingInfo.intOverDetectionExceptResultOk)
                        {
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameOk;
                        }
                        if (Convert.ToInt32(row["over_detection_except_result"]) == g_clsSystemSettingInfo.intOverDetectionExceptResultNg)
                        {
                            stResultName = g_clsSystemSettingInfo.strOverDetectionExceptResultNameNg;
                        }

                        arrRow.Add(stResultName);

                        arrRow.Add(row["ng_reason"]);
                        arrRow.Add(row["over_detection_except_datetime"]);
                        arrRow.Add(row["over_detection_except_worker"]);

                        this.dgvDecisionResult.Rows.Add(arrRow.ToArray());
                    }

                    bolProcOkNg = true;

                    m_bolRoadFlg = true;
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // 行選択
                if (intSelIdx != -1)
                {
                    this.dgvDecisionResult.Rows[intSelIdx].Selected = true;
                }

                // スクロールバー調整
                if (intFirstDisplayedScrollingRowIdx != -1)
                {
                    this.dgvDecisionResult.FirstDisplayedScrollingRowIndex = intFirstDisplayedScrollingRowIdx;
                }

                // 初期化
                intSelIdx = -1;
                intFirstDisplayedScrollingRowIdx = -1;
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
        /// 検査対象選択へ戻る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTargetSelection_Click(object sender, EventArgs e)
        {
            // 過検知除外ステータス更新(中断)
            if (blnUpdOverDetectionExceptStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                g_clsSystemSettingInfo.intOverDetectionExceptStatusStp) == false)
            {
                // エラー時
                g_clsConnectionNpgsql.DbRollback();
                g_clsConnectionNpgsql.DbClose();

                return;
            }
            g_clsConnectionNpgsql.DbCommit();
            g_clsConnectionNpgsql.DbClose();

            intDestination = g_CON_APID_TARGET_SELECTION;
            this.Close();
        }

        /// <summary>
        /// 合否確認ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAcceptanceCheck_Click(object sender, EventArgs e)
        {
            // 過検知除外ステータス更新(検査終了)
            if (blnUpdOverDetectionExceptStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd) == false)
            {
                // エラー時
                g_clsConnectionNpgsql.DbRollback();
                g_clsConnectionNpgsql.DbClose();

                return;
            }
            g_clsConnectionNpgsql.DbCommit();
            g_clsConnectionNpgsql.DbClose();

            intDestination = g_CON_APID_RESULT_CHECK;
            this.Close();
        }

        /// <summary>
        /// ログアウトボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            // 過検知除外ステータス更新(中断)
            if (blnUpdOverDetectionExceptStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                g_clsSystemSettingInfo.intOverDetectionExceptStatusStp) == false)
            {
                g_clsConnectionNpgsql.DbRollback();
                g_clsConnectionNpgsql.DbClose();

                return;
            }
            g_clsConnectionNpgsql.DbCommit();
            g_clsConnectionNpgsql.DbClose();

            intDestination = g_CON_APID_LOGIN;

            g_clsLoginInfo.Logout();
        }

        /// <summary>
        /// 一覧クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void dgvDecisionResult_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (!_clickSemaphore.Wait(0))
            {
                return;
            }

            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                {
                    return;
                }
            }
            finally
            {
                _clickSemaphore.Release();
            }

            using (ViewEnlargedimage frmViewEnlargedimage =
                new ViewEnlargedimage(
                    Path.Combine(
                        m_strFaultImageSubDirPath,
                        m_dtData.Rows[e.RowIndex]["org_imagepath"].ToString()),
                    Path.Combine(
                        m_strFaultImageSubDirPath,
                        m_dtData.Rows[e.RowIndex]["marking_imagepath"].ToString())))
            {
                frmViewEnlargedimage.ShowDialog(this);
            }

            this.Visible = true;
        }

        /// <summary>
        /// 一覧ダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDecisionResult_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            _doubleClickSemaphore.Release();

            // 選択行情報を保持
            intSelIdx = e.RowIndex;
            intFirstDisplayedScrollingRowIdx = this.dgvDecisionResult.FirstDisplayedScrollingRowIndex;

            this.Close();
        }

        /// <summary>
        /// フォームクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Summary_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_bolRoadFlg == false)
            {
                // 過検知除外ステータス更新(中断)
                if (blnUpdOverDetectionExceptStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                    g_clsSystemSettingInfo.intOverDetectionExceptStatusStp) == false)
                {
                    // エラー時
                    g_clsConnectionNpgsql.DbRollback();
                    g_clsConnectionNpgsql.DbClose();

                    if (e.CloseReason == CloseReason.UserClosing)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
            }
            else if (m_bolRoadFlg == true && intDestination == 0 && intSelIdx == -1)
            {
                // [X]ボタン押下時

                intDestination = g_CON_APID_TARGET_SELECTION;

                // 過検知除外ステータス更新(検査完了)
                if (blnUpdOverDetectionExceptStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                    g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd) == false)
                {
                    // エラー時
                    intDestination = 0;

                    g_clsConnectionNpgsql.DbRollback();
                    g_clsConnectionNpgsql.DbClose();

                    if (e.CloseReason == CloseReason.UserClosing)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
            }
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