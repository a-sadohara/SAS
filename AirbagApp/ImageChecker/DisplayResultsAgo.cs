using ImageChecker.DTO;
using Npgsql;
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
    public partial class DisplayResultsAgo : Form
    {
        // 定数
        private const string m_CON_FORMAT_WORKER_NAME = "作業者：{0}";
        private const string m_CON_FORMAT_NG_DISTANCE = "{0},{1}";
        private const string m_CON_FORMAT_SEARCH_COUNT = "{0} / {1}";
        private const string m_CON_FORMAT_INSPECTION_LINE = "{0}～{1}";

        // 件数
        private int m_intAllImageInspectionCount = 0;       // 全画像検査枚数
        private int m_intAllCushionspectionCount = 0;       // 全検査クッション数

        // データ保持関連
        private DataTable m_dtData;

        // 選択行情報関連
        private string m_strSelUnitNum = string.Empty;
        private string m_strSelProductName = string.Empty;
        private string m_strSelOrderImg = string.Empty;
        private string m_strSelFabricName = string.Empty;
        private string m_strSelInspectionDate = string.Empty;
        private int m_intSelInspectionNum = -1;
        private int m_intSelBranchNum = -1;
        private string m_strSelMarkingImagepath = string.Empty;

        // [X]ボタン無効
        private bool m_bolXButtonDisable = false;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DisplayResultsAgo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// データグリッドビュー表示
        /// </summary>
        /// <returns>true:正常終了 false:異常終了</returns>
        private bool bolDispDataGridView()
        {
            string strSQL = string.Empty;
            ArrayList arrRow = new ArrayList();
            string stResultName = string.Empty;
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
            List<String> lststrLineColumns = new List<String>();
            string stBefore48hourYmdhms = DateTime.Now.AddHours(-48).ToString("yyyy/MM/dd HH:mm:ss");

            // 一覧の表示
            dgvCheckInspectionHistory.Rows.Clear();
            try
            {
                m_dtData = new DataTable();
                strSQL = @"SELECT
                               iih.unit_num
                             , iih.product_name
                             , iih.order_img
                             , iih.fabric_name
                             , TO_CHAR(iih.start_datetime,'YYYY/MM/DD HH24:MI:SS') AS start_datetime
                             , TO_CHAR(iih.end_datetime,'YYYY/MM/DD HH24:MI:SS') AS end_datetime
                             , iih.inspection_start_line
                             , iih.inspection_end_line
                             , iih.inspection_target_line
                             , TO_CHAR(iih.decision_start_datetime,'YYYY/MM/DD HH24:MI:SS') AS decision_start_datetime
                             , TO_CHAR(iih.decision_end_datetime,'YYYY/MM/DD HH24:MI:SS') AS decision_end_datetime
                             , iih.inspection_direction
                             , iih.inspection_num
                             , TO_CHAR(iih.inspection_date,'YYYY/MM/DD') AS inspection_date
                             , iih.over_detection_except_status
                             , iih.acceptance_check_status
                             , dr.branch_num
                             , dr.line
                             , dr.cloumns
                             , dr.ng_face
                             , dr.ng_distance_x
                             , dr.ng_distance_y
                             , dr.over_detection_except_result
                             , dr.acceptance_check_result
                             , dr.ng_reason
                             , TO_CHAR(dr.over_detection_except_datetime,'YYYY/MM/DD HH24:MI:SS') AS over_detection_except_datetime
                             , dr.over_detection_except_worker
                             , TO_CHAR(dr.acceptance_check_datetime,'YYYY/MM/DD HH24:MI:SS') AS acceptance_check_datetime
                             , dr.acceptance_check_worker
                             , TO_CHAR(dr.result_update_datetime,'YYYY/MM/DD HH24:MI:SS') AS result_update_datetime
                             , dr.result_update_worker
                             , dr.before_ng_reason
                             , dr.org_imagepath
                             , dr.marking_imagepath
                             , mpi.column_cnt
                             , mpi.airbag_imagepath
                           FROM 
                               " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header iih
                           INNER JOIN 
                               " + g_clsSystemSettingInfo.strInstanceName + @".decision_result dr
                           ON  iih.fabric_name = dr.fabric_name
                           AND iih.inspection_date = dr.inspection_date
                           AND iih.inspection_num = dr.inspection_num
                           AND iih.unit_num = dr.unit_num
                           INNER JOIN mst_product_info mpi
                           ON  iih.product_name = mpi.product_name
                           WHERE iih.result_datetime < TO_TIMESTAMP('" + stBefore48hourYmdhms + @"','YYYY/MM/DD HH24:MI:SS') ";

                // 検索部
                // 号機
                if (string.IsNullOrEmpty(cmbUnitNum.Text) == false)
                {
                    strSQL += @"AND iih.unit_num = :unit_num ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = cmbUnitNum.Text });
                }
                // 品名
                if (string.IsNullOrEmpty(txtProductName.Text) == false)
                {
                    strSQL += @"AND iih.product_name = :product_name ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "product_name", DbType = DbType.String, Value = txtProductName.Text });
                }
                // 指図
                if (string.IsNullOrEmpty(txtOrderImg.Text) == false)
                {
                    strSQL += @"AND iih.order_img = :order_img ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "order_img", DbType = DbType.String, Value = txtOrderImg.Text });
                }
                // 反番
                if (string.IsNullOrEmpty(txtFabricName.Text) == false)
                {
                    strSQL += @"AND iih.fabric_name = :fabric_name ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = txtFabricName.Text });
                }
                // 搬送開始日時(From)
                if (string.IsNullOrEmpty(dtpStartDatetimeFrom.Text.Trim()) == false)
                {
                    strSQL += @"AND iih.start_datetime >= TO_TIMESTAMP(:start_datetime_from, 'YYYY/MM/DD HH24:MI:SS') ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "start_datetime_from", DbType = DbType.String, Value = dtpStartDatetimeFrom.Text });
                }
                // 搬送開始日時(To)
                if (string.IsNullOrEmpty(dtpStartDatetimeTo.Text.Trim()) == false)
                {
                    strSQL += @"AND iih.start_datetime <= TO_TIMESTAMP(:start_datetime_to, 'YYYY/MM/DD HH24:MI:SS') ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "start_datetime_to", DbType = DbType.String, Value = dtpStartDatetimeTo.Text });
                }
                // 搬送終了日時(From)
                if (string.IsNullOrEmpty(dtpEndDatetimeFrom.Text.Trim()) == false)
                {
                    strSQL += @"AND iih.end_datetime >= TO_TIMESTAMP(:end_datetime_from, 'YYYY/MM/DD HH24:MI:SS') ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "end_datetime_from", DbType = DbType.String, Value = dtpEndDatetimeFrom.Text });
                }
                // 搬送終了日時(To)
                if (string.IsNullOrEmpty(dtpEndDatetimeTo.Text.Trim()) == false)
                {
                    strSQL += @"AND iih.end_datetime <= TO_TIMESTAMP(:end_datetime_to, 'YYYY/MM/DD HH24:MI:SS') ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "end_datetime_to", DbType = DbType.String, Value = dtpEndDatetimeTo.Text });

                }
                // 検査範囲行(From)
                if (string.IsNullOrEmpty(txtSearchFrom.Text) == false)
                {
                    strSQL += @"AND dr.line >= :line_from ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "line_from", DbType = DbType.Int16, Value = int.Parse(txtSearchFrom.Text) });
                }
                // 検査範囲行(To)
                if (string.IsNullOrEmpty(txtSearchTo.Text) == false)
                {
                    strSQL += @"AND dr.line <= :line_to ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "line_to", DbType = DbType.Int16, Value = int.Parse(txtSearchTo.Text) });
                }
                // 判定開始日時(From)
                if (string.IsNullOrEmpty(dtpDecisionStartTimeFrom.Text.Trim()) == false)
                {
                    strSQL += @"AND iih.decision_start_datetime >= TO_TIMESTAMP(:decision_start_datetime_from, 'YYYY/MM/DD HH24:MI:SS') ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "decision_start_datetime_from", DbType = DbType.String, Value = dtpDecisionStartTimeFrom.Text });
                }
                // 判定開始日時(To)
                if (string.IsNullOrEmpty(dtpDecisionStartTimeTo.Text.Trim()) == false)
                {
                    strSQL += @"AND iih.decision_start_datetime <= TO_TIMESTAMP(:decision_start_datetime_to, 'YYYY/MM/DD HH24:MI:SS') ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "decision_start_datetime_to", DbType = DbType.String, Value = dtpDecisionStartTimeTo.Text });
                }
                // 判定終了日時(From)
                if (string.IsNullOrEmpty(dtpDecisionEndTimeFrom.Text.Trim()) == false)
                {
                    strSQL += @"AND iih.decision_end_datetime >= TO_TIMESTAMP(:decision_end_datetime_from, 'YYYY/MM/DD HH24:MI:SS') ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "decision_end_datetime_from", DbType = DbType.String, Value = dtpDecisionEndTimeFrom.Text });
                }
                // 判定終了日時(To)
                if (string.IsNullOrEmpty(dtpDecisionEndTimeTo.Text.Trim()) == false)
                {
                    strSQL += @"AND iih.decision_end_datetime <= TO_TIMESTAMP(:decision_end_datetime_to, 'YYYY/MM/DD HH24:MI:SS') ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "decision_end_datetime_to", DbType = DbType.String, Value = dtpDecisionEndTimeTo.Text });
                }
                // 検査番号
                if (string.IsNullOrEmpty(txtInspectionNum.Text) == false)
                {
                    strSQL += @"AND iih.inspection_num = :inspection_num ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = int.Parse(txtInspectionNum.Text) });
                }
                // 作業者
                if (string.IsNullOrEmpty(txtWorkerName.Text) == false)
                {
                    strSQL += @"AND (dr.acceptance_check_worker LIKE '%" + txtWorkerName.Text + "%' OR dr.result_update_worker LIKE '%" + txtWorkerName.Text + "%') ";
                }
                // 行
                if (string.IsNullOrEmpty(txtLine.Text) == false)
                {
                    strSQL += @"AND dr.line = :line ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "line", DbType = DbType.Int16, Value = int.Parse(txtLine.Text) });
                }
                // 列
                if (string.IsNullOrEmpty(cmbColumns.Text) == false)
                {
                    strSQL += @"AND dr.cloumns = :columns ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "columns", DbType = DbType.String, Value = cmbColumns.Text });
                }
                // NG面
                if (string.IsNullOrEmpty(cmbNgFace.Text) == false)
                {
                    strSQL += @"AND dr.ng_face = :ng_face ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_face", DbType = DbType.String, Value = cmbNgFace.Text });
                }
                // NG理由
                if (!string.IsNullOrEmpty(txtNgReason.Text))
                {
                    switch (txtNgReason.Text)
                    {
                        case g_CON_NG_REASON_OK:
                            strSQL += @"AND dr.ng_reason IS NULL ";
                            break;
                        case g_CON_ACCEPTANCE_CHECK_RESULT_NG_DETECT:
                            strSQL += string.Format("AND dr.acceptance_check_result = {0} ", g_clsSystemSettingInfo.intAcceptanceCheckResultNgDetect);
                            break;
                        case g_CON_ACCEPTANCE_CHECK_RESULT_NG_NON_DETECT:
                            strSQL += string.Format("AND dr.acceptance_check_result = {0} ", g_clsSystemSettingInfo.intAcceptanceCheckResultNgNonDetect);
                            break;
                        default:
                            strSQL += string.Format("AND dr.ng_reason LIKE '%{0}%' ", txtNgReason.Text);
                            break;
                    }
                }

                strSQL += @"ORDER BY iih.end_datetime ASC, iih.inspection_num ASC, dr.over_detection_except_datetime, dr.org_imagepath ASC, dr.branch_num";

                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);

                // データグリッドに反映
                for (int i = 0; i <= m_dtData.Rows.Count - 1; i++)
                {
                    arrRow = new ArrayList();

                    // No
                    arrRow.Add(this.dgvCheckInspectionHistory.Rows.Count + 1);

                    // SQL抽出項目
                    arrRow.Add(m_dtData.Rows[i]["unit_num"]);
                    arrRow.Add(m_dtData.Rows[i]["product_name"]);
                    arrRow.Add(m_dtData.Rows[i]["order_img"]);
                    arrRow.Add(m_dtData.Rows[i]["fabric_name"]);
                    arrRow.Add(m_dtData.Rows[i]["start_datetime"]);
                    arrRow.Add(m_dtData.Rows[i]["end_datetime"]);
                    arrRow.Add(string.Format(m_CON_FORMAT_INSPECTION_LINE, m_dtData.Rows[i]["inspection_start_line"], m_dtData.Rows[i]["inspection_end_line"]));
                    arrRow.Add(m_dtData.Rows[i]["decision_start_datetime"]);
                    arrRow.Add(m_dtData.Rows[i]["decision_end_datetime"]);
                    arrRow.Add(m_dtData.Rows[i]["inspection_num"]);
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
                    arrRow.Add(m_dtData.Rows[i]["result_update_datetime"]);
                    arrRow.Add(m_dtData.Rows[i]["result_update_worker"]);
                    arrRow.Add(m_dtData.Rows[i]["before_ng_reason"]);

                    this.dgvCheckInspectionHistory.Rows.Add(arrRow.ToArray());

                    // 行選択
                    if (!string.IsNullOrEmpty(m_strSelUnitNum) &&
                        m_dtData.Rows[i]["unit_num"].ToString() == m_strSelUnitNum &&
                        m_dtData.Rows[i]["product_name"].ToString() == m_strSelProductName &&
                        m_dtData.Rows[i]["order_Img"].ToString() == m_strSelOrderImg &&
                        m_dtData.Rows[i]["fabric_name"].ToString() == m_strSelFabricName &&
                        m_dtData.Rows[i]["inspection_date"].ToString() == m_strSelInspectionDate &&
                        Convert.ToInt32(m_dtData.Rows[i]["inspection_num"]) == m_intSelInspectionNum &&
                        Convert.ToInt32(m_dtData.Rows[i]["branch_num"]) == m_intSelBranchNum &&
                        m_dtData.Rows[i]["marking_imagepath"].ToString() == m_strSelMarkingImagepath)
                    {
                        dgvCheckInspectionHistory.Rows[i].Selected = true;
                        dgvCheckInspectionHistory.FirstDisplayedScrollingRowIndex = i;

                        // 以降の行は探さない
                        m_strSelUnitNum = string.Empty;
                    }

                    // 行列情報を保持
                    // 重複時はスキップ
                    if (lststrLineColumns.Contains(string.Join("|", m_dtData.Rows[i]["fabric_name"],
                                                                    m_dtData.Rows[i]["inspection_num"],
                                                                    m_dtData.Rows[i]["line"],
                                                                    m_dtData.Rows[i]["cloumns"])))
                    {
                        continue;
                    }

                    lststrLineColumns.Add(string.Join("|", m_dtData.Rows[i]["fabric_name"],
                                                           m_dtData.Rows[i]["inspection_num"],
                                                           m_dtData.Rows[i]["line"],
                                                           m_dtData.Rows[i]["cloumns"]));
                }

                if (dgvCheckInspectionHistory.Rows.Count == 0)
                {
                    btnResultUpdate.Enabled = false;
                    btnReprint.Enabled = false;
                }
                else
                {
                    btnResultUpdate.Enabled = true;
                    btnReprint.Enabled = true;
                }

                // 件数の表示
                lblImageSearchCount.Visible = true;
                lblCushionSearchCount.Visible = true;
                lblImageSearchCount.Text = string.Format(m_CON_FORMAT_SEARCH_COUNT, m_dtData.Rows.Count, m_intAllImageInspectionCount);
                lblCushionSearchCount.Text = string.Format(m_CON_FORMAT_SEARCH_COUNT, lststrLineColumns.Count, m_intAllCushionspectionCount);

                // 選択行情報初期化
                m_strSelUnitNum = string.Empty;
                m_strSelProductName = string.Empty;
                m_strSelOrderImg = string.Empty;
                m_strSelFabricName = string.Empty;
                m_strSelInspectionDate = string.Empty;
                m_intSelInspectionNum = -1;
                m_intSelBranchNum = -1;
                m_strSelMarkingImagepath = string.Empty;

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0047, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// クッション数取得
        /// </summary>
        /// <returns>true:正常終了 false:異常終了</returns>
        private bool bolGetCushionCnt()
        {
            string strSQL = string.Empty;
            DataTable dtData;
            string strBefore48hourYmdhms = DateTime.Now.AddHours(-48).ToString("yyyy/MM/dd HH:mm:ss");
            int intImageInspectionCount = 0;
            int intCushionspectionCount = 0;

            // 件数の取得
            try
            {
                dtData = new DataTable();
                strSQL = @"SELECT 
                               iih.fabric_name
                             , iih.inspection_date
                             , iih.inspection_num 
                             , iih.unit_num
                             , MAX(mpi.column_cnt) AS column_cnt
                             , MAX(iih.inspection_target_line) AS inspection_target_line
                             , COUNT(*) AS image_inspection_count
                           FROM 
                               " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header iih
                           INNER JOIN 
                               " + g_clsSystemSettingInfo.strInstanceName + @".decision_result dr
                           ON  iih.fabric_name = dr.fabric_name
                           AND iih.inspection_date = dr.inspection_date
                           AND iih.inspection_num = dr.inspection_num
                           AND iih.unit_num = dr.unit_num
                           INNER JOIN mst_product_info mpi
                           ON  iih.product_name = mpi.product_name
                           WHERE iih.result_datetime < TO_TIMESTAMP('" + strBefore48hourYmdhms + @"','YYYY/MM/DD HH24:MI:SS')
                           GROUP BY iih.fabric_name, iih.inspection_date, iih.inspection_num, iih.unit_num";

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL);

                foreach (DataRow row in dtData.Rows)
                {
                    intImageInspectionCount += int.Parse(row["image_inspection_count"].ToString());
                    intCushionspectionCount += int.Parse(row["column_cnt"].ToString()) * int.Parse(row["inspection_target_line"].ToString());
                }

                // 件数の表示
                m_intAllImageInspectionCount = intImageInspectionCount;
                m_intAllCushionspectionCount = intCushionspectionCount;

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0047, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// 欠点画像存在チェック
        /// </summary>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="strUnitNum">号機</param>
        /// <param name="strFabricName">反番</param>
        /// <param name="strFaultImageFileName">欠点画像ファイル名</param>
        /// <param name="strLogMessage">ログメッセージ</param>
        private async Task<bool?> BolCheckFaultImage(
            int intInspectionNum,
            string strInspectionDate,
            string strUnitNum,
            string strFabricName,
            string strFaultImageFileName,
            string strLogMessage)
        {
            string strFaultImageFileDirectory = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, strUnitNum, strFaultImageFileName);
            bool? bolRapidTableCheckResult = true;
            bool? bolCheckNGRecordResult = true;
            bool? bolCheckImageResult = true;
            int intImageCount = 0;

            // 画像ディレクトリが存在しない場合、フォルダを作成する
            if (!Directory.Exists(strFaultImageFileDirectory))
            {
                Directory.CreateDirectory(strFaultImageFileDirectory);
            }

            bolRapidTableCheckResult = BolCheckRapidTableCount(intInspectionNum, strFabricName, strInspectionDate, strUnitNum, strLogMessage);

            // Rapidテーブルが存在しない場合、処理を終了する
            if (!bolRapidTableCheckResult.Equals(true))
            {
                return false;
            }

            bolCheckNGRecordResult = BolCheckNGRecordCount(intInspectionNum, strFabricName, strInspectionDate, strUnitNum, strLogMessage, true);

            bolCheckImageResult =
                bolCheckImageCount(
                    ref intImageCount,
                    intInspectionNum,
                    strInspectionDate,
                    strUnitNum,
                    strFabricName,
                    strFaultImageFileName,
                    strLogMessage);

            // NGレコードが存在しない場合、処理を終了する
            if (!bolCheckNGRecordResult.Equals(true))
            {
                // NGレコード・欠点画像がともに存在しない場合、チェックOKとする
                if (bolCheckNGRecordResult.Equals(false) &&
                    intImageCount == 0)
                {
                    return true;
                }

                return bolCheckNGRecordResult;
            }

            // 欠点画像の不足が無い場合、チェック時にエラーが発生した場合、処理を終了する
            if (!bolCheckImageResult.Equals(false))
            {
                return bolCheckImageResult;
            }

            ImportImageZipProgressForm frmProgress = new ImportImageZipProgressForm();
            frmProgress.StartPosition = FormStartPosition.CenterScreen;
            frmProgress.Size = this.Size;
            frmProgress.Show(this);

            try
            {
                // 欠点画像数チェック・再取込を実施する
                return await BolReInputFaultImage(
                    intImageCount,
                    intInspectionNum,
                    strInspectionDate,
                    strUnitNum,
                    strFabricName,
                    strFaultImageFileName,
                    strLogMessage);
            }
            finally
            {
                frmProgress.Close();
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

            // 作業者の表示
            lblWorkerName.Text = string.Format(m_CON_FORMAT_WORKER_NAME, g_clsLoginInfo.strWorkerName);

            // 初期化
            cmbUnitNum.Text = "";
            txtProductName.Text = "";
            txtOrderImg.Text = "";
            txtFabricName.Text = "";
            txtInspectionNum.Text = "";
            dtpStartDatetimeFrom.Text = ""; dtpStartDatetimeFrom.CustomFormat = " ";
            dtpStartDatetimeTo.Text = ""; dtpStartDatetimeTo.CustomFormat = " ";
            txtSearchFrom.Text = "";
            txtSearchTo.Text = "";
            dtpEndDatetimeFrom.Text = ""; dtpEndDatetimeFrom.CustomFormat = " ";
            dtpEndDatetimeTo.Text = ""; dtpEndDatetimeTo.CustomFormat = " ";
            dtpDecisionStartTimeFrom.Text = ""; dtpDecisionStartTimeFrom.CustomFormat = " ";
            dtpDecisionStartTimeTo.Text = ""; dtpDecisionStartTimeTo.CustomFormat = " ";
            dtpDecisionEndTimeFrom.Text = ""; dtpDecisionEndTimeFrom.CustomFormat = " ";
            dtpDecisionEndTimeTo.Text = ""; dtpDecisionEndTimeTo.CustomFormat = " ";
            txtWorkerName.Text = "";
            txtLine.Text = "";
            cmbColumns.Text = "";
            cmbNgFace.Text = "";
            txtNgReason.Text = "";

            // 列のスタイル変更
            this.dgvCheckInspectionHistory.Columns["No"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dgvCheckInspectionHistory.Columns["InspectionNum"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            try
            {
                // 件数の非表示
                lblImageSearchCount.Visible = false;
                lblCushionSearchCount.Visible = false;

                btnResultUpdate.Enabled = false;
                btnReprint.Enabled = false;

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
        /// 検査結果を更新するクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnResultUpdate_Click(object sender, EventArgs e)
        {
            int intSelIdx = -1;
            string strFaultImageSubDirectory = string.Empty;
            string strLogMessage = string.Empty;

            // 選択行インデックスの取得
            foreach (DataGridViewRow dgvRow in this.dgvCheckInspectionHistory.SelectedRows)
            {
                intSelIdx = dgvRow.Index;
                break;
            }

            if (int.Parse(m_dtData.Rows[intSelIdx]["over_detection_except_status"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptStatusExc &&
                int.Parse(m_dtData.Rows[intSelIdx]["acceptance_check_status"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckStatusExc)
            {
                MessageBox.Show(g_clsMessageInfo.strMsgE0064, g_CON_MESSAGE_TITLE_WARN, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            strFaultImageSubDirectory = string.Join("_", m_dtData.Rows[intSelIdx]["inspection_date"].ToString().Replace("/", ""),
                                                         m_dtData.Rows[intSelIdx]["product_name"],
                                                         m_dtData.Rows[intSelIdx]["fabric_name"],
                                                         m_dtData.Rows[intSelIdx]["inspection_num"]);

            strLogMessage =
                string.Format(
                    g_CON_LOG_MESSAGE_FOMAT,
                    m_dtData.Rows[intSelIdx]["unit_num"].ToString(),
                    m_dtData.Rows[intSelIdx]["inspection_date"].ToString().Replace("/", ""),
                    m_dtData.Rows[intSelIdx]["inspection_num"].ToString(),
                    m_dtData.Rows[intSelIdx]["product_name"].ToString(),
                    m_dtData.Rows[intSelIdx]["fabric_name"].ToString());

            // ディレクトリ存在チェック
            bool? tskRet =
                await BolCheckFaultImage(
                    Convert.ToInt32(m_dtData.Rows[intSelIdx]["inspection_num"].ToString()),
                    m_dtData.Rows[intSelIdx]["inspection_date"].ToString(),
                    m_dtData.Rows[intSelIdx]["unit_num"].ToString(),
                    m_dtData.Rows[intSelIdx]["fabric_name"].ToString(),
                    strFaultImageSubDirectory,
                    strLogMessage);

            switch (tskRet)
            {
                case false:
                    MessageBox.Show(g_clsMessageInfo.strMsgW0003, g_CON_MESSAGE_TITLE_WARN, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                case null:
                    return;
            }

            // ヘッダ情報
            HeaderData clsHeaderData = new HeaderData();
            clsHeaderData.strUnitNum = m_dtData.Rows[intSelIdx]["unit_num"].ToString();
            clsHeaderData.strProductName = m_dtData.Rows[intSelIdx]["product_name"].ToString();
            clsHeaderData.strOrderImg = m_dtData.Rows[intSelIdx]["order_img"].ToString();
            clsHeaderData.strFabricName = m_dtData.Rows[intSelIdx]["fabric_name"].ToString();
            clsHeaderData.strInspectionDate = m_dtData.Rows[intSelIdx]["inspection_date"].ToString();
            clsHeaderData.strStartDatetime = m_dtData.Rows[intSelIdx]["start_datetime"].ToString();
            clsHeaderData.strEndDatetime = m_dtData.Rows[intSelIdx]["end_datetime"].ToString();
            clsHeaderData.intInspectionStartLine = int.Parse(m_dtData.Rows[intSelIdx]["inspection_start_line"].ToString());
            clsHeaderData.intInspectionEndLine = int.Parse(m_dtData.Rows[intSelIdx]["inspection_end_line"].ToString());
            clsHeaderData.intInspectionTargetLine = int.Parse(m_dtData.Rows[intSelIdx]["inspection_target_line"].ToString());
            clsHeaderData.strDecisionStartDatetime = m_dtData.Rows[intSelIdx]["decision_start_datetime"].ToString();
            clsHeaderData.strDecisionEndDatetime = m_dtData.Rows[intSelIdx]["decision_end_datetime"].ToString();
            clsHeaderData.strInspectionDirection = m_dtData.Rows[intSelIdx]["inspection_direction"].ToString();
            clsHeaderData.intInspectionNum = int.Parse(m_dtData.Rows[intSelIdx]["inspection_num"].ToString());
            clsHeaderData.intOverDetectionExceptStatus = int.Parse(m_dtData.Rows[intSelIdx]["over_detection_except_status"].ToString());
            clsHeaderData.intAcceptanceCheckStatus = int.Parse(m_dtData.Rows[intSelIdx]["acceptance_check_status"].ToString());
            clsHeaderData.intColumnCnt = int.Parse(m_dtData.Rows[intSelIdx]["column_cnt"].ToString());
            clsHeaderData.strAirbagImagepath = g_strMasterImageDirPath + Path.DirectorySeparatorChar +
                                               Path.GetFileName(m_dtData.Rows[intSelIdx]["airbag_imagepath"].ToString());
            clsHeaderData.strFaultImageDirectory = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, clsHeaderData.strUnitNum);

            // 判定結果情報
            DecisionResult clsDecisionResult = new DecisionResult();
            clsDecisionResult.intBranchNum = int.Parse(m_dtData.Rows[intSelIdx]["branch_num"].ToString());
            clsDecisionResult.intLine = int.Parse(m_dtData.Rows[intSelIdx]["line"].ToString());
            clsDecisionResult.strCloumns = m_dtData.Rows[intSelIdx]["cloumns"].ToString();
            clsDecisionResult.strNgReason = m_dtData.Rows[intSelIdx]["ng_reason"].ToString();
            clsDecisionResult.strMarkingImagepath = m_dtData.Rows[intSelIdx]["marking_imagepath"].ToString();
            clsDecisionResult.strOrgImagepath = m_dtData.Rows[intSelIdx]["org_imagepath"].ToString();
            clsDecisionResult.strNgFace = m_dtData.Rows[intSelIdx]["ng_face"].ToString();
            clsDecisionResult.intNgDistanceX = int.Parse(m_dtData.Rows[intSelIdx]["ng_distance_x"].ToString());
            clsDecisionResult.intNgDistanceY = int.Parse(m_dtData.Rows[intSelIdx]["ng_distance_y"].ToString());

            // 行選択情報
            m_strSelUnitNum = clsHeaderData.strUnitNum;
            m_strSelProductName = clsHeaderData.strProductName;
            m_strSelOrderImg = clsHeaderData.strOrderImg;
            m_strSelFabricName = clsHeaderData.strFabricName;
            m_strSelInspectionDate = clsHeaderData.strInspectionDate;
            m_intSelInspectionNum = clsHeaderData.intInspectionNum;
            m_intSelBranchNum = clsDecisionResult.intBranchNum;
            m_strSelMarkingImagepath = clsDecisionResult.strMarkingImagepath;

            this.Visible = false;

            ResultCheck frmResultCheck = new ResultCheck(ref clsHeaderData, clsDecisionResult, g_CON_APID_DISPLAY_RESULTS_AGO);
            frmResultCheck.ShowDialog(this);

            // 判定登録画面:検査開始選択へ戻るボタンで閉じる
            if (frmResultCheck.intDestination == g_CON_APID_TARGET_SELECTION)
            {
                this.Close();
                return;
            }

            this.Visible = true;

            // 件数取得
            if (bolGetCushionCnt() == false)
            {
                return;
            }

            // 一覧表示
            if (bolDispDataGridView() == false)
            {
                return;
            }
        }

        /// <summary>
        /// 再印刷ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnReprint_Click(object sender, EventArgs e)
        {
            int intSelIdx = -1;
            int intNgCushionCnt = 0;
            int intNgImageCnt = 0;
            int intInspectionNum = 0;
            string strFabricName = string.Empty;
            string strInspectionDate = string.Empty;
            string strUnitNum = string.Empty;
            string strSQL = string.Empty;
            DataTable dtData;

            // コントロール無効
            m_bolXButtonDisable = true;

            // 選択行インデックスの取得
            foreach (DataGridViewRow dgvRow in this.dgvCheckInspectionHistory.SelectedRows)
            {
                intSelIdx = dgvRow.Index;
                break;
            }

            if (intSelIdx != -1)
            {
                ImportImageZipProgressForm frmProgress = new ImportImageZipProgressForm(g_clsMessageInfo.strMsgI0012);
                frmProgress.StartPosition = FormStartPosition.CenterScreen;
                frmProgress.Size = this.Size;
                frmProgress.Show(this);

                try
                {
                    strFabricName = m_dtData.Rows[intSelIdx]["fabric_name"].ToString();
                    strInspectionDate = m_dtData.Rows[intSelIdx]["inspection_date"].ToString();
                    strUnitNum = m_dtData.Rows[intSelIdx]["unit_num"].ToString();
                    intInspectionNum = int.Parse(m_dtData.Rows[intSelIdx]["inspection_num"].ToString());

                    // NG画像の取得
                    dtData = new DataTable();
                    try
                    {
                        strSQL = @"SELECT
                                   image_inspection_count_ng
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
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = strInspectionDate });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = intInspectionNum });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ngdetect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNgDetect });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ngnondetect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNgNonDetect });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultOk });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });

                        g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                        intNgImageCnt = int.Parse(dtData.Rows[0]["image_inspection_count_ng"].ToString());
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));

                        // メッセージ出力
                        MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

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
                                      AND unit_num = :unit_num
                                      AND ng_reason IS NOT NULL
                                 GROUP BY line, cloumns
                                     ) imgcnt";

                        // SQLコマンドに各パラメータを設定する
                        List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = strInspectionDate });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = intInspectionNum });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultOk });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });

                        g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                        intNgCushionCnt = int.Parse(dtData.Rows[0]["cnt"].ToString());
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));

                        // メッセージ出力
                        MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }

                    // 帳票出力
                    await Task<Boolean>.Run(() =>
                        g_clsReportInfo.OutputReport(
                            strFabricName,
                            strInspectionDate,
                            strUnitNum,
                            intInspectionNum,
                            intNgCushionCnt,
                            intNgImageCnt));
                }
                finally
                {
                    frmProgress.Close();

                    m_bolXButtonDisable = false;
                }
            }
        }

        /// <summary>
        /// 一覧クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void dgvCheckInspectionHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            string strFaultImageSubDirectory = string.Join("_", m_dtData.Rows[e.RowIndex]["inspection_date"].ToString().Replace("/", ""),
                                                                m_dtData.Rows[e.RowIndex]["product_name"],
                                                                m_dtData.Rows[e.RowIndex]["fabric_name"],
                                                                m_dtData.Rows[e.RowIndex]["inspection_num"]);

            string strLogMessage =
                string.Format(
                    g_CON_LOG_MESSAGE_FOMAT,
                    m_dtData.Rows[e.RowIndex]["unit_num"].ToString(),
                    m_dtData.Rows[e.RowIndex]["inspection_date"].ToString().Replace("/", ""),
                    m_dtData.Rows[e.RowIndex]["inspection_num"].ToString(),
                    m_dtData.Rows[e.RowIndex]["product_name"].ToString(),
                    m_dtData.Rows[e.RowIndex]["fabric_name"].ToString());

            // ディレクトリ存在チェック
            bool? tskRet =
                await BolCheckFaultImage(
                    Convert.ToInt32(m_dtData.Rows[e.RowIndex]["inspection_num"].ToString()),
                    m_dtData.Rows[e.RowIndex]["inspection_date"].ToString(),
                    m_dtData.Rows[e.RowIndex]["unit_num"].ToString(),
                    m_dtData.Rows[e.RowIndex]["fabric_name"].ToString(),
                    strFaultImageSubDirectory,
                    strLogMessage);

            switch (tskRet)
            {
                case false:
                    MessageBox.Show(g_clsMessageInfo.strMsgW0003, g_CON_MESSAGE_TITLE_WARN, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                case null:
                    return;
            }

            ViewEnlargedimage frmViewEnlargedimage = new ViewEnlargedimage(Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory
                                                                           , m_dtData.Rows[e.RowIndex]["unit_num"].ToString()
                                                                           , strFaultImageSubDirectory
                                                                           , m_dtData.Rows[e.RowIndex]["org_imagepath"].ToString()),
                                                                           Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory
                                                                           , m_dtData.Rows[e.RowIndex]["unit_num"].ToString()
                                                                           , strFaultImageSubDirectory
                                                                           , m_dtData.Rows[e.RowIndex]["marking_imagepath"].ToString()));
            frmViewEnlargedimage.ShowDialog(this);
            this.Visible = true;
        }

        /// <summary>
        /// 検査対象選択へ戻る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTargetSelection_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// ログアウトボタン
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

        /// <summary>
        /// 品名ダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtProductName_DoubleClick(object sender, EventArgs e)
        {
            ProductNameSelection frmProductNameSelection = new ProductNameSelection();
            frmProductNameSelection.ShowDialog(this);
            txtProductName.Text = frmProductNameSelection.strProductName;
        }

        /// <summary>
        /// 日時ドロップダウン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtpDatetime_DropDown(object sender, EventArgs e)
        {
            DateTimePicker dtpCtrol = (DateTimePicker)sender;
            dtpCtrol.Text = string.Empty;
            dtpCtrol.CustomFormat = " ";
        }

        /// <summary>
        /// 日時値変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtpDatetime_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker dtpCtrol = (DateTimePicker)sender;
            dtpCtrol.CustomFormat = "yyyy/MM/dd HH:mm:ss";
        }

        /// <summary>
        /// 数値型キー入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                // 0～9と、バックスペース以外の時は、イベントをキャンセルする
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
            // 件数取得
            if (bolGetCushionCnt() == false)
            {
                return;
            }

            // 一覧表示
            if (bolDispDataGridView() == false)
            {
                return;
            }
        }

        #region 横スクロール対応
        Point mouseDownPosition;
        /// <summary>
        /// 一覧マウスオーバー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCheckInspectionHistory_MouseMove(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:

                    if (mouseDownPosition.X != e.Location.X)
                    {
                        try
                        {
                            int movePosi_X = dgvCheckInspectionHistory.HorizontalScrollingOffset - (e.Location.X - mouseDownPosition.X);

                            if (movePosi_X < 10)
                            {
                                movePosi_X = 0;
                            }

                            dgvCheckInspectionHistory.HorizontalScrollingOffset = movePosi_X;
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
        private void dgvCheckInspectionHistory_MouseDown(object sender, MouseEventArgs e)
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

        #region フォームクローズ
        /// <summary>
        /// フォームクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayResultsAgo_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 再印刷中は無効にする
            if (m_bolXButtonDisable == true)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        #endregion
    }
}