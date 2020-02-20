﻿using ImageChecker.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class DisplayResults : Form
    {
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
        private int m_intColumnCnt = 0;                 // 列数
        private int m_intCountInit = -1;                // 初期表示件数

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
        private const string m_CON_FORMAT_SEARCH_COUNT = "{0} / {1}";

        // 欠点画像サブディレクトリパス
        private string m_strFaultImageSubDirectory = "";

        // データ保持関連
        private DataTable m_dtData;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clsHeaderData">ヘッダ情報</param>
        public DisplayResults(ref HeaderData clsHeaderData)
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
            m_intColumnCnt = clsHeaderData.intColumnCnt;

            m_strFaultImageSubDirectory = string.Join("_", m_strInspectionDate.Replace("/", ""),
                                                           m_strProductName,
                                                           m_strFabricName,
                                                           m_intInspectionNum);

            InitializeComponent();
        }

        /// <summary>
        /// データグリッドビュー表示
        /// </summary>
        /// <param name="strKanaSta">カナ（開始）</param>
        /// <param name="strKanaEnd">カナ（終了）</param>
        private bool bolDispDataGridView()
        {
            string strSQL = "";
            ArrayList arrRow = new ArrayList();
            string stResultName = "";
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
            List<String> lststrLineColumns = new List<String>();

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
                             , TO_CHAR(result_update_datetime,'YYYY/MM/DD HH24:MI:SS') AS result_update_datetime
                             , before_acceptance_check_result
                             , TO_CHAR(before_acceptance_check_upd_datetime,'YYYY/MM/DD HH24:MI:SS') AS before_acceptance_check_upd_datetime
                             , before_acceptance_check_worker
                             , result_update_worker
                             , before_ng_reason
                             , org_imagepath
                             , marking_imagepath
                           FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                           AND   inspection_num = :inspection_num
                           AND   over_detection_except_result <> :over_detection_except_result_ok ";

                // 検索部
                // 作業者
                if (txtWorkerName.Text != "")
                {
                    strSQL += @"AND (acceptance_check_worker LIKE '%" + txtWorkerName.Text + "%' OR result_update_worker LIKE '%" + txtWorkerName.Text + "%') ";
                }
                // 行
                if (txtLine.Text != "")
                {
                    strSQL += @"AND line = :line ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "line", DbType = DbType.Int16, Value = int.Parse(txtLine.Text) });
                }
                // 列
                if (cmbColumns.Text != "")
                {
                    strSQL += @"AND cloumns = :columns ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "columns", DbType = DbType.String, Value = cmbColumns.Text });
                }
                // NG面
                if (cmbNgFace.Text != "")
                {
                    strSQL += @"AND ng_face = :ng_face ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_face", DbType = DbType.String, Value = cmbNgFace.Text });
                }
                // NG理由
                if (txtNgReason.Text == g_CON_NG_REASON_OK)
                {
                    strSQL += @"AND ng_reason IS NULL ";
                }
                else if (txtNgReason.Text != "")
                {
                    strSQL += @"AND ng_reason LIKE '%" + txtNgReason.Text + "%' ";
                }

                strSQL += @"ORDER BY ";

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS)
                    strSQL += "line ASC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
                    strSQL += "line ASC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY)
                    strSQL += "line DESC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";
                else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
                    strSQL += "line DESC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC, branch_num ASC";

                // SQLコマンドに各パラメータを設定する
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
                    arrRow.Add(row["result_update_datetime"]);
                    arrRow.Add(row["result_update_worker"]);
                    arrRow.Add(row["before_ng_reason"]);

                    this.dgvDecisionResult.Rows.Add(arrRow.ToArray());

                    // 行列情報を保持
                    // 重複時はスキップ
                    if (lststrLineColumns.Contains(string.Join("|", row["line"], row["cloumns"])) == false)
                        lststrLineColumns.Add(string.Join("|", row["line"], row["cloumns"]));
                }

                // 初期表示件数を保持
                if (m_intCountInit == -1)
                    m_intCountInit = dgvDecisionResult.Rows.Count;

                // 件数の表示
                lblImageSearchCount.Text = string.Format(m_CON_FORMAT_SEARCH_COUNT, m_dtData.Rows.Count, m_intCountInit);
                lblCushionSearchCount.Text = string.Format(m_CON_FORMAT_SEARCH_COUNT, lststrLineColumns.Count, m_intColumnCnt * m_intInspectionTargetLine);
                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0050, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
            int intImageInspectionCount = -1;
            int intImageInspectionCountOk = -1;
            int intImageInspectionCountNg = -1;
            int intCushionInspectionCount = -1;
            int intCushionInspectionCountOk = -1;
            int intCushionInspectionCountNg = -1;

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

            // 初期化
            txtWorkerName.Text = "";
            txtLine.Text = "";
            cmbColumns.Text = "";
            cmbNgFace.Text = "";
            txtNgReason.Text = "";

            try
            {
                // カウント系のヘッダ表示
                // 列数取得
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

                // 一覧表示
                if (bolDispDataGridView() == false)
                    return;

                bolProcOkNg = true;
            }
            finally
            {
                if (bolProcOkNg == false)
                    this.Close();

                this.ResumeLayout();
            }
        }

        /// <summary>
        /// 行キー入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLine_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r' && e.KeyChar != '\b' && e.KeyChar < '0' || '9' < e.KeyChar)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 作業者キー入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtWorkerName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Encoding.GetEncoding("Shift-JIS").GetByteCount(e.KeyChar.ToString()) == e.KeyChar.ToString().Length * 2) && e.KeyChar != '\b')
            {
                // 全角文字以外の時は、イベントをキャンセルする
                e.Handled = true;
            }
        }

        /// <summary>
        /// 検索クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            bolDispDataGridView();
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
        /// 検査対象へ戻るクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTargetSelection_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 検査結果を更新するクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspectionUpdate_Click(object sender, EventArgs e)
        {
            int intSelIdx = -1;

            // 選択行インデックスの取得
            foreach (DataGridViewRow dgvRow in this.dgvDecisionResult.SelectedRows)
            {
                intSelIdx = dgvRow.Index;
                break;
            }

            DecisionResult clsDecisionResult = new DecisionResult();
            clsDecisionResult.intBranchNum = int.Parse(m_dtData.Rows[intSelIdx]["branch_num"].ToString());
            clsDecisionResult.intLine = int.Parse(m_dtData.Rows[intSelIdx]["line"].ToString());
            clsDecisionResult.strCloumns = m_dtData.Rows[intSelIdx]["cloumns"].ToString();
            clsDecisionResult.strNgReason = m_dtData.Rows[intSelIdx]["ng_reason"].ToString();
            clsDecisionResult.strNgFace = m_dtData.Rows[intSelIdx]["ng_face"].ToString();
            clsDecisionResult.strMarkingImagepath = m_dtData.Rows[intSelIdx]["marking_imagepath"].ToString();
            clsDecisionResult.strOrgImagepath = m_dtData.Rows[intSelIdx]["org_imagepath"].ToString();
            clsDecisionResult.intAcceptanceCheckResult = int.Parse(m_dtData.Rows[intSelIdx]["acceptance_check_result"].ToString());
            clsDecisionResult.strAcceptanceCheckDatetime = m_dtData.Rows[intSelIdx]["acceptance_check_datetime"].ToString();
            clsDecisionResult.strAcceptanceCheckWorker = m_dtData.Rows[intSelIdx]["acceptance_check_worker"].ToString();
            clsDecisionResult.intBeforeAcceptanceCheckResult = int.Parse(m_dtData.Rows[intSelIdx]["before_acceptance_check_result"].ToString());
            clsDecisionResult.strBeforeAcceptanceCheckUpdDatetime = m_dtData.Rows[intSelIdx]["before_acceptance_check_upd_datetime"].ToString();
            clsDecisionResult.strBeforeAcceptanceCheckWorker = m_dtData.Rows[intSelIdx]["before_acceptance_check_worker"].ToString();
            clsDecisionResult.strResultUpdateDatetime = m_dtData.Rows[intSelIdx]["result_update_datetime"].ToString();
            clsDecisionResult.strResultUpdateWorker = m_dtData.Rows[intSelIdx]["result_update_worker"].ToString();
            clsDecisionResult.strBeforeNgReason = m_dtData.Rows[intSelIdx]["before_ng_reason"].ToString();

            this.Visible = false;

            ResultCheck frmResultCheck = new ResultCheck(ref m_clsHeaderData, clsDecisionResult, g_CON_APID_DISPLAY_RESULTS);
            frmResultCheck.ShowDialog(this);

            // 判定登録画面:検査開始選択へ戻るボタンで閉じる
            if (frmResultCheck.intDestination == g_CON_APID_TARGET_SELECTION)
            {
                this.Close();
                return;
            }

            this.Visible = true;

            // 一覧表示
            if (bolDispDataGridView() == false)
                return;
        }

        /// <summary>
        /// ログアウトクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            g_clsLoginInfo.Logout();
        }

        /// <summary>
        /// 作業者ダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtWorkerName_DoubleClick(object sender, EventArgs e)
        {
            WorkerSelection frmWorkerSelection = new WorkerSelection();
            frmWorkerSelection.ShowDialog(this);
            txtWorkerName.Text = frmWorkerSelection.strWorkerName;
        }

        /// <summary>
        /// NG理由ダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtNgReason_DoubleClick(object sender, EventArgs e)
        {
            SelectErrorReason frmSelectErrorReason = new SelectErrorReason(true);
            frmSelectErrorReason.ShowDialog(this);
            txtNgReason.Text = frmSelectErrorReason.strDecisionReason;
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
