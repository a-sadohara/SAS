using ImageChecker.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

using static ImageChecker.Common;

namespace ImageChecker
{

    public partial class TargetSelection : Form
    {
        // 列インデックス関連
        private const int m_CON_COL_IDX_OVERDETECTIONEXCEPT = 4;
        private const int m_CON_COL_IDX_ACCEPTANCECHECK = 5;
        private const int m_CON_COL_IDX_RESULT = 6;

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
        private const string m_CON_FORMAT_STATUS = "状態：{0}";
        private const string m_CON_FORMAT_DETECTION_IMAGE_COUNT_SCORES = "検知撮像枚数：{0}枚（{1}/{2}）";
        private const string m_CON_FORMAT_DETECTION_IMAGE_COUNT = "検知撮像枚数：{0}枚";
        private const string m_CON_FORMAT_OVER_DETECTION_IMAGE_COUNT_SCORES = "合否確認撮像枚数：{0}枚（{1}/{2}）";
        private const string m_CON_OVER_DETECTION_EXCEPT_BUTTON_NAME = "過検知除外";
        private const string m_CON_ACCEPTANCE_CHECK_BUTTON_NAME = "合否確認・判定登録";
        private const string m_CON_INSPECTION_RESULT_BUTTON_NAME = "検査結果確認";
        private const string m_CON_FORMAT_WORKER_NAME = "作業者：{0}";
        private const string CON_ZIP_EXTRACT_DIR_PATH = "ZipExtractDirPath";

        // データ保持関連
        private DataTable m_dtData;

        // 前回の連携時間
        private DateTime datetimePrevReplicate;

        // 選択行保持
        private int m_intSelRowIdx = -1;
        private int m_intFirstDisplayedScrollingRowIndex = -1;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TargetSelection()
        {
            InitializeComponent();

            // 一時フォルダ作成
            if (Directory.Exists(g_clsSystemSettingInfo.strTemporaryDirectory) == false)
                Directory.CreateDirectory(g_clsSystemSettingInfo.strTemporaryDirectory);

            // 一時ZIP解凍用フォルダ作成
            if (Directory.Exists(g_clsSystemSettingInfo.strTemporaryDirectory + Path.DirectorySeparatorChar + CON_ZIP_EXTRACT_DIR_PATH) == true)
                Directory.Delete(g_clsSystemSettingInfo.strTemporaryDirectory + Path.DirectorySeparatorChar + CON_ZIP_EXTRACT_DIR_PATH, true);
            Directory.CreateDirectory(g_clsSystemSettingInfo.strTemporaryDirectory + Path.DirectorySeparatorChar + CON_ZIP_EXTRACT_DIR_PATH);
        }

        /// <summary>
        /// 欠点画像取り込み
        /// </summary>
        /// <param name="strFaultImage"></param>
        /// <returns></returns>
        private async Task<Boolean> BolImpFatalImage(string strFaultImage)
        {
            try
            {

                // ZIPファイルを一時ZIP解凍用フォルダにコピーする
                File.Copy(Path.Combine( g_clsSystemSettingInfo.strNgImageCooperationDirectory , strFaultImage + ".zip"),
                          Path.Combine(g_clsSystemSettingInfo.strTemporaryDirectory , CON_ZIP_EXTRACT_DIR_PATH ,strFaultImage + ".zip"),true);

                // 欠点画像Zipファイルの解凍
                ZipFile.ExtractToDirectory(Path.Combine(g_clsSystemSettingInfo.strTemporaryDirectory , CON_ZIP_EXTRACT_DIR_PATH , strFaultImage + ".zip"),
                           g_clsSystemSettingInfo.strFaultImageDirectory);

                // 一時ファイルの削除
                File.Delete(Path.Combine(g_clsSystemSettingInfo.strTemporaryDirectory, CON_ZIP_EXTRACT_DIR_PATH, strFaultImage + ".zip"));

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0040 , Environment.NewLine , ex.Message));

                return false;
            }

        }

        /// <summary>
        /// データグリッドビュー表示
        /// </summary>
        /// <param name="strKanaSta">カナ（開始）</param>
        /// <param name="strKanaEnd">カナ（終了）</param>
        private bool bolDispDataGridView()
        {
            string strSQL = "";
            StringBuilder sbFabricInfo = new StringBuilder();
            StringBuilder sbInspectionInfo = new StringBuilder();
            StringBuilder sbInspectionState = new StringBuilder();
            int intOverDetectionExceptStatus = -1;
            int intAcceptanceCheckStatus = -1;
            string strBefore48hourYmdhms = DateTime.Now.AddHours(-48).ToString("yyyy/MM/dd HH:mm:ss");
            ArrayList arrRow = new ArrayList();

            dgvTargetSelection.Rows.Clear();

            if (dgvTargetSelection.Columns.Count != 7)
            {
                // グリッドビューにボタンを追加
                DataGridViewDisableButtonColumn btnOverDetectionExcept = new DataGridViewDisableButtonColumn();
                btnOverDetectionExcept.FlatStyle = FlatStyle.Flat;
                btnOverDetectionExcept.Width = 180;
                DataGridViewDisableButtonColumn btnAcceptanceCheck = new DataGridViewDisableButtonColumn();
                btnAcceptanceCheck.FlatStyle = FlatStyle.Flat;
                btnAcceptanceCheck.Width = 180;
                DataGridViewDisableButtonColumn btnInspectionResult = new DataGridViewDisableButtonColumn();
                btnInspectionResult.FlatStyle = FlatStyle.Flat;
                btnInspectionResult.Width = 180;
                this.dgvTargetSelection.Columns.Add(btnOverDetectionExcept);
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].Name = "OverDetectionExcept";
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].HeaderText = "";
                this.dgvTargetSelection.Columns.Add(btnAcceptanceCheck);
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].Name = "AcceptanceCheck";
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].HeaderText = "";
                this.dgvTargetSelection.Columns.Add(btnInspectionResult);
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].Name = "InspectionResult";
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].HeaderText = "";
            }
                       
            try
            {
                // SQL抽出
                m_dtData = new DataTable();
                strSQL = @"SELECT iih.unit_num
                                , iih.order_img
                                , iih.fabric_name
                                , iih.product_name
                                , TO_CHAR(iih.inspection_date,'YYYY/MM/DD') AS inspection_date
                                , iih.inspection_num
                                , TO_CHAR(iih.start_datetime,'YYYY/MM/DD HH24:MI:SS') AS start_datetime
                                , TO_CHAR(iih.end_datetime,'YYYY/MM/DD HH24:MI:SS') AS end_datetime
                                , iih.inspection_direction
                                , iih.inspection_start_line
                                , iih.inspection_end_line
                                , iih.inspection_target_line
                                , TO_CHAR(iih.decision_start_datetime,'YYYY/MM/DD HH24:MI:SS') AS decision_start_datetime
                                , TO_CHAR(iih.decision_end_datetime,'YYYY/MM/DD HH24:MI:SS') AS decision_end_datetime
                                , iih.over_detection_except_status
                                , iih.acceptance_check_status
                                , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                   WHERE fabric_name = dr.fabric_name
                                   AND   inspection_date = dr.inspection_date
                                   AND   inspection_num = dr.inspection_num) AS detection_image_count
                                , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                   WHERE over_detection_except_result = :over_detection_except_result_ok
                                   AND   fabric_name = dr.fabric_name
                                   AND   inspection_date = dr.inspection_date
                                   AND   inspection_num = dr.inspection_num) AS over_detection_image_count
                                , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                  WHERE over_detection_except_result = :over_detection_except_result_ng
                                  AND   fabric_name = dr.fabric_name
                                  AND   inspection_date = dr.inspection_date
                                  AND   inspection_num = dr.inspection_num) AS acceptance_check_image_count
                                , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                   WHERE acceptance_check_result <> :acceptance_check_result_non
                                   AND   fabric_name = dr.fabric_name
                                   AND   inspection_date = dr.inspection_date
                                   AND   inspection_num = dr.inspection_num) AS determine_count
                                , mpi.column_cnt
                                , mpi.airbag_imagepath
                           FROM
                               " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header iih
                           INNER JOIN (
                                   SELECT fabric_name,inspection_date,inspection_num
                                   FROM   " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                   GROUP BY fabric_name,inspection_date,inspection_num
                               ) dr
                           ON  iih.fabric_name = dr.fabric_name
                           AND iih.inspection_date = dr.inspection_date
                           AND iih.inspection_num = dr.inspection_num
                           INNER JOIN mst_product_info mpi
                           ON  iih.product_name = mpi.product_name
                           WHERE (iih.decision_end_datetime IS NULL OR
                                  iih.decision_end_datetime >= TO_TIMESTAMP('" + strBefore48hourYmdhms + @"','YYYY/MM/DD HH24:MI:SS'))
                           ORDER BY iih.inspection_date ASC, iih.inspection_num ASC";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ng", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNg });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNon });

                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);

                // データグリッドに反映
                foreach (DataRow row in m_dtData.Rows)
                {
                    // 初期化
                    sbFabricInfo = new StringBuilder();
                    sbInspectionInfo = new StringBuilder();
                    sbInspectionState = new StringBuilder();
                    arrRow = new ArrayList();
                    intOverDetectionExceptStatus = -1;
                    intAcceptanceCheckStatus = -1;

                    // No
                    arrRow.Add(this.dgvTargetSelection.Rows.Count + 1);

                    // 反物情報
                    sbFabricInfo.AppendLine(string.Format(m_CON_FORMAT_UNIT_NUM, row["unit_num"]));
                    sbFabricInfo.AppendLine(string.Format(m_CON_FORMAT_ORDER_IMG, row["order_img"]));
                    sbFabricInfo.AppendLine(string.Format(m_CON_FORMAT_FABRIC_NAME, row["fabric_name"]));
                    sbFabricInfo.AppendLine(string.Format(m_CON_FORMAT_PRODUCT_NAME, row["product_name"]));
                    arrRow.Add(sbFabricInfo.ToString());

                    // 外観検査情報
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_START_DATETIME, row["start_datetime"]));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_END_DATETIME, row["end_datetime"]));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_INSPECTION_LINE, row["inspection_start_line"], row["inspection_end_line"]));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_DECISION_START_DATETIME, row["decision_start_datetime"]));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_DECISION_END_DATETIME, row["decision_end_datetime"]));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_INSPECTION_NUM, row["inspection_num"]));
                    arrRow.Add(sbInspectionInfo.ToString());

                    // 欠点検査状態 & ステータス
                    if (int.Parse(row["acceptance_check_status"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckStatusBef ||
                        int.Parse(row["acceptance_check_status"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckStatusExc)
                    {
                        //  過検知除外ステータス
                        if (int.Parse(row["over_detection_except_status"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptStatusBef)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameBef));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_DETECTION_IMAGE_COUNT_SCORES, row["detection_image_count"], "0", row["detection_image_count"]));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusBef;
                        }
                        else if (int.Parse(row["over_detection_except_status"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptStatusChk)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameChk));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_DETECTION_IMAGE_COUNT_SCORES, row["detection_image_count"], row["over_detection_image_count"], row["detection_image_count"]));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusChk;
                        }
                        else if (int.Parse(row["over_detection_except_status"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptStatusStp)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameStp));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_DETECTION_IMAGE_COUNT_SCORES, row["detection_image_count"], row["over_detection_image_count"], row["detection_image_count"]));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusStp;
                        }
                        else if (int.Parse(row["over_detection_except_status"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameEnd));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_OVER_DETECTION_IMAGE_COUNT_SCORES, row["acceptance_check_image_count"], "0", row["acceptance_check_image_count"]));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd;
                        }
                        else if (int.Parse(row["over_detection_except_status"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptStatusExc)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameExc));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_DETECTION_IMAGE_COUNT, row["detection_image_count"].ToString()));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusExc;
                        }
                    }
                    else
                    {
                        // 合否確認ステータス
                        if (int.Parse(row["acceptance_check_status"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckStatusChk)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strAcceptanceCheckStatusNameChk));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_OVER_DETECTION_IMAGE_COUNT_SCORES, row["over_detection_image_count"], row["determine_count"], row["over_detection_image_count"]));
                            intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusChk;
                        }
                        else if (int.Parse(row["acceptance_check_status"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckStatusStp)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strAcceptanceCheckStatusNameChk));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_OVER_DETECTION_IMAGE_COUNT_SCORES, row["acceptance_check_image_count"], row["determine_count"], row["acceptance_check_image_count"]));
                            intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusStp;
                        }
                        else if (int.Parse(row["acceptance_check_status"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strAcceptanceCheckStatusNameEnd));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_OVER_DETECTION_IMAGE_COUNT_SCORES, row["acceptance_check_image_count"], row["determine_count"], row["acceptance_check_image_count"]));
                            intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd;
                        }
                    }
                    arrRow.Add(sbInspectionState.ToString());

                    // ボタン名
                    arrRow.Add(m_CON_OVER_DETECTION_EXCEPT_BUTTON_NAME);
                    arrRow.Add(m_CON_ACCEPTANCE_CHECK_BUTTON_NAME);
                    arrRow.Add(m_CON_INSPECTION_RESULT_BUTTON_NAME);

                    this.dgvTargetSelection.Rows.Add(arrRow.ToArray());


                    // ボタン設定
                    DataGridViewDisableButtonCell btnCellOverDetectionExcept = (DataGridViewDisableButtonCell)this.dgvTargetSelection.Rows[this.dgvTargetSelection.Rows.Count - 1].Cells[m_CON_COL_IDX_OVERDETECTIONEXCEPT];
                    DataGridViewDisableButtonCell btnCellAcceptanceCheck = (DataGridViewDisableButtonCell)this.dgvTargetSelection.Rows[this.dgvTargetSelection.Rows.Count - 1].Cells[m_CON_COL_IDX_ACCEPTANCECHECK];
                    DataGridViewDisableButtonCell btnCellInspectionResult = (DataGridViewDisableButtonCell)this.dgvTargetSelection.Rows[this.dgvTargetSelection.Rows.Count - 1].Cells[m_CON_COL_IDX_RESULT];

                    // ボタンレイアウト設定
                    //  背景色,有効無効
                    if (intAcceptanceCheckStatus == g_clsSystemSettingInfo.intAcceptanceCheckStatusChk)
                    {
                        // 合否確認：検査中
                        btnCellOverDetectionExcept.Style.BackColor = Color.WhiteSmoke;
                        btnCellOverDetectionExcept.Style.SelectionBackColor = Color.WhiteSmoke;
                        btnCellAcceptanceCheck.Style.BackColor = Color.Red;
                        btnCellAcceptanceCheck.Style.SelectionBackColor = Color.Red;
                        btnCellInspectionResult.Style.BackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.SelectionBackColor = Color.DarkGray;

                        btnCellOverDetectionExcept.Enabled = false;
                        btnCellAcceptanceCheck.Enabled = false;
                        btnCellInspectionResult.Enabled = false;
                    }

                    if (intAcceptanceCheckStatus == g_clsSystemSettingInfo.intAcceptanceCheckStatusStp)
                    {
                        // 合否確認：中断
                        btnCellOverDetectionExcept.Style.BackColor = Color.WhiteSmoke;
                        btnCellOverDetectionExcept.Style.SelectionBackColor = Color.WhiteSmoke;
                        btnCellAcceptanceCheck.Style.BackColor = Color.DarkOrange;
                        btnCellAcceptanceCheck.Style.SelectionBackColor = Color.DarkOrange;
                        btnCellInspectionResult.Style.BackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.SelectionBackColor = Color.DarkGray;

                        btnCellOverDetectionExcept.Enabled = false;
                        btnCellAcceptanceCheck.Enabled = true;
                        btnCellInspectionResult.Enabled = false;
                    }

                    if (intAcceptanceCheckStatus == g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd)
                    {
                        // 合否確認：検査完了
                        btnCellOverDetectionExcept.Style.BackColor = Color.WhiteSmoke;
                        btnCellOverDetectionExcept.Style.SelectionBackColor = Color.WhiteSmoke;
                        btnCellAcceptanceCheck.Style.BackColor = Color.WhiteSmoke;
                        btnCellAcceptanceCheck.Style.SelectionBackColor = Color.WhiteSmoke;
                        btnCellInspectionResult.Style.BackColor = Color.MediumSeaGreen;
                        btnCellInspectionResult.Style.SelectionBackColor = Color.MediumSeaGreen;

                        btnCellOverDetectionExcept.Enabled = false;
                        btnCellAcceptanceCheck.Enabled = false;
                        btnCellInspectionResult.Enabled = true;
                    }

                    if (intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusBef)
                    {
                        // 過検知除外：検査前
                        btnCellOverDetectionExcept.Style.BackColor = Color.MediumSeaGreen;
                        btnCellOverDetectionExcept.Style.SelectionBackColor = Color.MediumSeaGreen;
                        btnCellAcceptanceCheck.Style.BackColor = Color.DarkGray;
                        btnCellAcceptanceCheck.Style.SelectionBackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.BackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.SelectionBackColor = Color.DarkGray;

                        btnCellOverDetectionExcept.Enabled = true;
                        btnCellAcceptanceCheck.Enabled = false;
                        btnCellInspectionResult.Enabled = false;
                    }

                    if (intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusChk)
                    {
                        // 過検知除外：検査中
                        btnCellOverDetectionExcept.Style.BackColor = Color.Crimson;
                        btnCellOverDetectionExcept.Style.SelectionBackColor = Color.Crimson;
                        btnCellAcceptanceCheck.Style.BackColor = Color.DarkGray;
                        btnCellAcceptanceCheck.Style.SelectionBackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.BackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.SelectionBackColor = Color.DarkGray;

                        btnCellOverDetectionExcept.Enabled = false;
                        btnCellAcceptanceCheck.Enabled = false;
                        btnCellInspectionResult.Enabled = false;
                    }

                    if (intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusStp)
                    {
                        // 過検知除外：中断
                        btnCellOverDetectionExcept.Style.BackColor = Color.DarkOrange;
                        btnCellOverDetectionExcept.Style.SelectionBackColor = Color.DarkOrange;
                        btnCellAcceptanceCheck.Style.BackColor = Color.DarkGray;
                        btnCellAcceptanceCheck.Style.SelectionBackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.BackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.SelectionBackColor = Color.DarkGray;

                        btnCellOverDetectionExcept.Enabled = true;
                        btnCellAcceptanceCheck.Enabled = false;
                        btnCellInspectionResult.Enabled = false;
                    }

                    if (intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd)
                    {
                        // 過検知除外：検査完了
                        btnCellOverDetectionExcept.Style.BackColor = Color.WhiteSmoke;
                        btnCellOverDetectionExcept.Style.SelectionBackColor = Color.WhiteSmoke;
                        btnCellAcceptanceCheck.Style.BackColor = Color.MediumSeaGreen;
                        btnCellAcceptanceCheck.Style.SelectionBackColor = Color.MediumSeaGreen;
                        btnCellInspectionResult.Style.BackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.SelectionBackColor = Color.DarkGray;

                        btnCellOverDetectionExcept.Enabled = false;
                        btnCellAcceptanceCheck.Enabled = true;
                        btnCellInspectionResult.Enabled = false;
                    }

                    if (intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusExc)
                    {
                        // 対象外
                        btnCellOverDetectionExcept.Style.BackColor = Color.DarkGray;
                        btnCellOverDetectionExcept.Style.SelectionBackColor = Color.DarkGray;
                        btnCellAcceptanceCheck.Style.BackColor = Color.DarkGray;
                        btnCellAcceptanceCheck.Style.SelectionBackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.BackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.SelectionBackColor = Color.DarkGray;

                        btnCellOverDetectionExcept.Enabled = false;
                        btnCellAcceptanceCheck.Enabled = false;
                        btnCellInspectionResult.Enabled = false;
                    }
                }
                
                // 行選択
                if (m_intSelRowIdx != -1)
                {
                    try
                    {
                        this.dgvTargetSelection.Rows[m_intSelRowIdx].Selected = true;
                    }
                    catch
                    {
                        // 一覧がリセットされる瞬間においては例外が発生するが、無視する
                    }
                }

                // スクロールバー調整
                if (m_intFirstDisplayedScrollingRowIndex != -1)
                {
                    this.dgvTargetSelection.FirstDisplayedScrollingRowIndex = m_intFirstDisplayedScrollingRowIndex;
                }

                if (dgvTargetSelection.Rows.Count == 0)
                {
                    btnReviseLine.Enabled = false;
                    btnExceptTarget.Enabled = false;
                }
                else
                {
                    btnReviseLine.Enabled = true;
                    btnExceptTarget.Enabled = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0002 + "\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0042, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
        private void TargetSelection_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            bool bolProcOkNg = false;

            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            try
            {
                // 作業者名の表示
                lblWorkerName.Text = string.Format(m_CON_FORMAT_WORKER_NAME, g_clsLoginInfo.strWorkerName);
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
        /// 一覧セルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridView dgv = (DataGridView)sender;
                HeaderData clsHeaderData = new HeaderData();
                DecisionResult clsDecisionResult = new DecisionResult();
                string strFaultImageSubDirectory = "";

                // ボタン行以外はイベント終了
                if (e.ColumnIndex < m_CON_COL_IDX_OVERDETECTIONEXCEPT)
                    return;

                // ボタンが無効の場合は終了
                DataGridViewDisableButtonCell btnTarget = (DataGridViewDisableButtonCell)dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (!btnTarget.Enabled)
                    return;

                Overdetectionexclusion frmOverDetectionExcept;
                ResultCheck frmResultCheck;

                // ヘッダ情報の設定
                clsHeaderData.strUnitNum = m_dtData.Rows[e.RowIndex]["unit_num"].ToString();
                clsHeaderData.strProductName = m_dtData.Rows[e.RowIndex]["product_name"].ToString();
                clsHeaderData.strOrderImg = m_dtData.Rows[e.RowIndex]["order_img"].ToString();
                clsHeaderData.strFabricName = m_dtData.Rows[e.RowIndex]["fabric_name"].ToString();
                clsHeaderData.strInspectionDate = m_dtData.Rows[e.RowIndex]["inspection_date"].ToString();
                clsHeaderData.strStartDatetime = m_dtData.Rows[e.RowIndex]["start_datetime"].ToString();
                clsHeaderData.strEndDatetime = m_dtData.Rows[e.RowIndex]["end_datetime"].ToString();
                clsHeaderData.intInspectionStartLine = int.Parse(m_dtData.Rows[e.RowIndex]["inspection_start_line"].ToString());
                clsHeaderData.intInspectionEndLine = int.Parse(m_dtData.Rows[e.RowIndex]["inspection_end_line"].ToString());
                clsHeaderData.intInspectionTargetLine = int.Parse(m_dtData.Rows[e.RowIndex]["inspection_target_line"].ToString());
                clsHeaderData.strDecisionStartDatetime = m_dtData.Rows[e.RowIndex]["decision_start_datetime"].ToString();
                clsHeaderData.strDecisionEndDatetime = m_dtData.Rows[e.RowIndex]["decision_end_datetime"].ToString();
                clsHeaderData.strInspectionDirection = m_dtData.Rows[e.RowIndex]["inspection_direction"].ToString();
                clsHeaderData.intInspectionNum = int.Parse(m_dtData.Rows[e.RowIndex]["inspection_num"].ToString());
                clsHeaderData.intOverDetectionExceptStatus = int.Parse(m_dtData.Rows[e.RowIndex]["over_detection_except_status"].ToString());
                clsHeaderData.intAcceptanceCheckStatus = int.Parse(m_dtData.Rows[e.RowIndex]["acceptance_check_status"].ToString());
                clsHeaderData.intColumnCnt = int.Parse(m_dtData.Rows[e.RowIndex]["column_cnt"].ToString());
                clsHeaderData.strAirbagImagepath = g_clsSystemSettingInfo.strTemporaryDirectory + Path.DirectorySeparatorChar +
                                                   g_CON_DIR_MASTER_IMAGE + Path.DirectorySeparatorChar + 
                                                   Path.GetFileName(m_dtData.Rows[e.RowIndex]["airbag_imagepath"].ToString());

                strFaultImageSubDirectory = string.Join("_", m_dtData.Rows[e.RowIndex]["inspection_date"].ToString().Replace("/", ""),
                                                             m_dtData.Rows[e.RowIndex]["product_name"],
                                                             m_dtData.Rows[e.RowIndex]["fabric_name"],
                                                             m_dtData.Rows[e.RowIndex]["inspection_num"]);

                // ディレクトリ存在チェック
                if (Directory.Exists(g_clsSystemSettingInfo.strFaultImageDirectory + @"\" + strFaultImageSubDirectory) == false)
                {
                    MessageBox.Show(g_clsMessageInfo.strMsgW0006, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                switch (e.ColumnIndex)
                {
                    case m_CON_COL_IDX_OVERDETECTIONEXCEPT:

                        this.Visible = false;

                        // 過検知除外
                        frmOverDetectionExcept = new Overdetectionexclusion(ref clsHeaderData);
                        frmOverDetectionExcept.ShowDialog(this);

                        // 過検知除外結果から合否確認に遷移
                        if (frmOverDetectionExcept.intDestination == g_CON_APID_RESULT_CHECK)
                        {
                            frmResultCheck = new ResultCheck(ref clsHeaderData, clsDecisionResult);
                            frmResultCheck.ShowDialog(this);
                        }

                        break;

                    case m_CON_COL_IDX_ACCEPTANCECHECK:

                        this.Visible = false;

                        // 合否確認・判定登録
                        frmResultCheck = new ResultCheck(ref clsHeaderData, clsDecisionResult);
                        frmResultCheck.ShowDialog(this);

                        // 連携処理をして画面表示
                        datetimePrevReplicate = DateTime.MinValue;

                        break;
                    case m_CON_COL_IDX_RESULT:

                        this.Visible = false;

                        // 検査結果確認
                        DisplayResults frmInspectionResult = new DisplayResults(ref clsHeaderData);
                        frmInspectionResult.ShowDialog(this);

                        // 連携処理をして画面表示
                        datetimePrevReplicate = DateTime.MinValue;

                        break;
                }
                this.Visible = true;
            }
            finally
            {
            }
        }

        /// <summary>
        /// ログアウトボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            g_clsLoginInfo.Logout();
        }

        /// <summary>
        /// 検査履歴照会ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDisplayResultsAgo_Click(object sender, EventArgs e)
        {
            try
            {
                this.Visible = false;

                DisplayResultsAgo frmResult = new DisplayResultsAgo();
                frmResult.ShowDialog(this);

                // 連携処理をして画面表示
                datetimePrevReplicate = DateTime.MinValue;

                this.Visible = true;
            }
            finally
            {
            }
        }

        /// <summary>
        /// 検査対象外ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExceptTarget_Click(object sender, EventArgs e)
        {
            string strUnitNum = "";
            string strOrderImg = "";
            string strFabricName = "";
            string strProductName = "";
            string strInspectionDate = "";
            string strStartDatetime = "";
            string strEndDatetime = "";
            int intInspectionStartLine = -1;
            int intInspectionEndLine = -1;
            int intInspectionNum = 0;

            int intRow = -1;

            try
            {
                // 選択行インデックスの取得
                foreach (DataGridViewRow dgvRow in this.dgvTargetSelection.SelectedRows)
                {
                    intRow = dgvRow.Index;
                    break;
                }

                // パラメータの取得
                strUnitNum = m_dtData.Rows[intRow]["unit_num"].ToString();
                strOrderImg = m_dtData.Rows[intRow]["order_img"].ToString();
                strFabricName = m_dtData.Rows[intRow]["fabric_name"].ToString();
                strProductName = m_dtData.Rows[intRow]["product_name"].ToString();
                strInspectionDate = m_dtData.Rows[intRow]["inspection_date"].ToString();
                strStartDatetime = m_dtData.Rows[intRow]["start_datetime"].ToString();
                strEndDatetime = m_dtData.Rows[intRow]["end_datetime"].ToString();
                intInspectionStartLine = int.Parse(m_dtData.Rows[intRow]["inspection_start_line"].ToString());
                intInspectionEndLine = int.Parse(m_dtData.Rows[intRow]["inspection_end_line"].ToString());
                intInspectionNum = int.Parse(m_dtData.Rows[intRow]["inspection_num"].ToString());


                // 検査対象外画面表示
                CheckExcept frmCheckExcept = new CheckExcept(strUnitNum,
                                                             strOrderImg,
                                                             strFabricName,
                                                             strProductName,
                                                             intInspectionNum,
                                                             strInspectionDate,
                                                             strStartDatetime,
                                                             strEndDatetime,
                                                             intInspectionStartLine,
                                                             intInspectionEndLine);
                frmCheckExcept.ShowDialog(this);

                // 連携処理をして画面表示
                datetimePrevReplicate = DateTime.MinValue;

                this.Visible = true;
            }
            finally
            {
            }
        }

        /// <summary>
        /// 行補正ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReviseLine_Click(object sender, EventArgs e)
        {
            try
            {
                string strUnitNum = "";
                string strOrderImg = "";
                string strFabricName = "";
                string strProductName = "";
                string strInspectionDate = "";
                string strStartDatetime = "";
                string strEndDatetime = "";
                int intInspectionStartLine = -1;
                int intInspectionEndLine = -1;
                int intInspectionNum = 0;

                int intRow = -1;

                // 選択行インデックスの取得
                foreach (DataGridViewRow dgvRow in this.dgvTargetSelection.SelectedRows)
                {
                    intRow = dgvRow.Index;
                    break;
                }

                // パラメータの取得
                strUnitNum = m_dtData.Rows[intRow]["unit_num"].ToString();
                strOrderImg = m_dtData.Rows[intRow]["order_img"].ToString();
                strFabricName = m_dtData.Rows[intRow]["fabric_name"].ToString();
                strProductName = m_dtData.Rows[intRow]["product_name"].ToString();
                strInspectionDate = m_dtData.Rows[intRow]["inspection_date"].ToString();
                strStartDatetime = m_dtData.Rows[intRow]["start_datetime"].ToString();
                strEndDatetime = m_dtData.Rows[intRow]["end_datetime"].ToString();
                intInspectionStartLine = int.Parse(m_dtData.Rows[intRow]["inspection_start_line"].ToString());
                intInspectionEndLine = int.Parse(m_dtData.Rows[intRow]["inspection_end_line"].ToString());
                intInspectionNum = int.Parse(m_dtData.Rows[intRow]["inspection_num"].ToString());

                // 行補正画面表示
                LineCorrect frmLineCorrect = new LineCorrect(strUnitNum,
                                                             strOrderImg,
                                                             strFabricName,
                                                             strProductName,
                                                             intInspectionNum,
                                                             strInspectionDate,
                                                             strStartDatetime,
                                                             strEndDatetime,
                                                             intInspectionStartLine,
                                                             intInspectionEndLine);
                frmLineCorrect.ShowDialog(this);

                // 連携処理をして画面表示
                datetimePrevReplicate = DateTime.MinValue;
                this.Visible = true;
            }
            finally
            {
            }
        }
        #endregion


        /// <summary>
        /// フォームアクティブ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TargetSelection_Activated(object sender, EventArgs e)
        {
            this.SuspendLayout();
            if(datetimePrevReplicate != DateTime.MinValue &&
                datetimePrevReplicate > DateTime.Now)
            {
                // 連携基盤部との前回連携から5分以内の場合は、処理しない
                // データグリッドビュー表示
                bolDispDataGridView();
                this.ResumeLayout();
                return;
            }

            // 連携時間を更新
            datetimePrevReplicate = DateTime.Now.AddMinutes(5);

            ImportImageZipProgressForm frmProgress = new ImportImageZipProgressForm();
            frmProgress.StartPosition = FormStartPosition.CenterScreen;
            frmProgress.Size = this.Size;
            frmProgress.Show(this);
            
            // 連携基盤部連携ファイルの取込処理
            string[] strSplitFileName = new string[0];
            string strFileName = "";
            string strSQL = "";
            int intCnt = 0;
            DataTable dtData;

            // パラメータ
            string strInspectionDate = "";  // 検査日付(YYYY/MM/DD)
            string strProductName = "";     // 品名
            string strFabricName = "";      // 反番
            int intInspectionNum = 0;       // 検査番号
            string strFaultImageSubDirectory = "";

            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
            List<Task<Boolean>> lstTask = new List<Task<Boolean>>();

            try
            {
                // 完了通知ファイル(.TXT)の確認を実施
                foreach (string strFilePath in System.IO.Directory.GetFiles(g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectory, "*.txt",
                    System.IO.SearchOption.TopDirectoryOnly))
                {
                    // 初期化
                    strInspectionDate = "";
                    strProductName = "";
                    strFabricName = "";
                    intInspectionNum = 0;
                    strFaultImageSubDirectory = "";
                    lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    intCnt = 0;

                    // ファイル名の取得
                    strFileName = Path.GetFileNameWithoutExtension(strFilePath);

                    // ファイル名からパラメータを取得
                    strSplitFileName = strFileName.Split('_');

                    // パラメータを設定
                    if (strSplitFileName.Length == 4)
                    {
                        strInspectionDate = strSplitFileName[0].Insert(6, "/").Insert(4, "/");
                        strProductName = strSplitFileName[1];
                        strFabricName = strSplitFileName[2];
                        intInspectionNum = int.Parse(strSplitFileName[3]);

                        strFaultImageSubDirectory = string.Join("_", strInspectionDate.Replace("/", ""),
                                                                        strProductName,
                                                                        strFabricName,
                                                                        intInspectionNum);
                    }

                    // 取得失敗した場合は次のループ
                    if (string.IsNullOrEmpty(strInspectionDate) || string.IsNullOrEmpty(strProductName) || string.IsNullOrEmpty(strFabricName) || intInspectionNum == 0)
                    {
                        continue;
                    }

                    // 連携済みチェック
                    try
                    {
                        dtData = new DataTable();
                        strSQL = @"SELECT COUNT(inspection_num) AS cnt ";
                        strSQL += @"FROM  " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header ";
                        strSQL += @"WHERE fabric_name = :fabric_name ";
                        strSQL += @"AND   inspection_num = :inspection_num ";
                        strSQL += @"AND   TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd ";

                        // SQLコマンドに各パラメータを設定する
                        lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });

                        g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                        // 件数
                        if (dtData.Rows.Count > 0)
                        {
                            intCnt = int.Parse(dtData.Rows[0]["cnt"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001 , Environment.NewLine , ex.Message));
                        // メッセージ出力
                        MessageBox.Show(g_clsMessageInfo.strMsgE0031, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        frmProgress.Close();
                        return;
                    }

                    // 欠点画像の取込み処理
                    // フォルダの存在チェック
                    if (Directory.Exists(Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory,strFaultImageSubDirectory)) == false)
                    {
                        lstTask.Add(Task<Boolean>.Run(() => BolImpFatalImage(strFaultImageSubDirectory)));
                        System.Threading.Thread.Sleep(1000);
                    }

                    if (intCnt >= 1)
                    {
                        // 連携済みのため、次を処理
                        continue;
                    }

                    // 検査情報ヘッダの取り込み処理
                    try
                    {
                        strSQL = @"INSERT INTO " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header(
                                    inspection_num
                                    , branch_num
                                    , product_name
                                    , unit_num
                                    , order_img
                                    , fabric_name
                                    , inspection_target_line
                                    , inspection_end_line
                                    , inspection_start_line
                                    , worker_1
                                    , worker_2
                                    , start_datetime
                                    , end_datetime
                                    , inspection_direction
                                    , inspection_date
                                    , over_detection_except_status
                                    , acceptance_check_status
                                    , decision_start_datetime
                                    , decision_end_datetime
                                )
                                SELECT
                                    inspection_num
                                    , branch_num
                                    , product_name
                                    , unit_num
                                    , order_img
                                    , fabric_name
                                    , inspection_target_line
                                    , inspection_end_line
                                    , inspection_start_line
                                    , worker_1
                                    , worker_2
                                    , start_datetime
                                    , end_datetime
                                    , inspection_direction
                                    , inspection_date
                                    , 0
                                    , 0
                                    , NULL
                                    , NULL
                                FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".inspection_info_header
                                WHERE fabric_name = :fabric_name
                                    AND inspection_num = :inspection_num
                                    AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd";

                        // SQLコマンドに各パラメータを設定する
                        lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });

                        // sqlを実行する
                        g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
                    }
                    catch (Exception ex)
                    {
                        g_clsConnectionNpgsql.DbRollback();

                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR,string.Format("{0}{1}{2}" ,g_clsMessageInfo.strMsgE0002 , Environment.NewLine , ex.Message));
                        // メッセージ出力
                        MessageBox.Show(g_clsMessageInfo.strMsgE0035, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        frmProgress.Close();
                        return; 
                    }

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
                                    , camera_num_1
                                    , worker_1
                                    , worker_2
                                    , :over_detection_except_result_non
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
                                    SELECT ROW_NUMBER() OVER(PARTITION BY marking_image ORDER BY rapid_endtime DESC) AS SEQ
                                        , rpd.*
                                    FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".""rapid_" + strFabricName + "_" + intInspectionNum + @""" rpd
                                    WHERE fabric_name = :fabric_name
                                    AND inspection_num = :inspection_num 
                                    AND rapid_result = :rapid_result
                                    AND edge_result = :edge_result
                                    AND masking_result = :masking_result
                                ) rpd
                                WHERE SEQ = 1";

                        // SQLコマンドに各パラメータを設定する
                        lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "rapid_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intRapidResultNg });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "edge_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intEdgeResultNg });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "masking_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intMaskingResultNg });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNon });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNon });
                        // sqlを実行する
                        g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
                    }
                    catch (Exception ex)
                    {
                        g_clsConnectionNpgsql.DbRollback();

                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002 , Environment.NewLine , ex.Message));
                        // メッセージ出力
                        MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0039, strFabricName, intInspectionNum), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        frmProgress.Close();
                        return;
                    }
                }

                // 欠点画像の取込み待ち
                await Task.WhenAll(lstTask.ToArray());
                foreach (Task<Boolean> tsk in lstTask)
                {
                    if (!tsk.Result)
                    {
                        // メッセージ出力
                        MessageBox.Show( g_clsMessageInfo.strMsgE0041, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        frmProgress.Close();
                        return;
                    }
                }

                // DBコミット
                g_clsConnectionNpgsql.DbCommit();
                // DBクローズ
                g_clsConnectionNpgsql.DbClose();
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}" ,g_clsMessageInfo.strMsgE0058 , Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0059, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                frmProgress.Close();
              
            }

            // データグリッドビュー表示
            bolDispDataGridView();

            this.ResumeLayout();
        }

        /// <summary>
        /// 一覧スクロール
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvTargetSelection_Scroll(object sender, ScrollEventArgs e)
        {
            m_intFirstDisplayedScrollingRowIndex = dgvTargetSelection.FirstDisplayedScrollingRowIndex;
        }

        /// <summary>
        /// 一覧再描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvTargetSelection_Paint(object sender, PaintEventArgs e)
        {
            // 選択行インデックスの保持
            foreach (DataGridViewRow dgvRow in this.dgvTargetSelection.SelectedRows)
            {
                m_intSelRowIdx = dgvRow.Index;
                break;
            }
        }

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

    #region 独自クラス
    /// <summary>
    /// DataGridViewButtonColumnクラス
    /// </summary>
    public class DataGridViewDisableButtonColumn : DataGridViewButtonColumn
    {
        public DataGridViewDisableButtonColumn()
        {
            this.CellTemplate = new DataGridViewDisableButtonCell();
        }
    }

    /// <summary>
    /// DataGridViewButtonCellクラス
    /// </summary>
    public class DataGridViewDisableButtonCell : DataGridViewButtonCell
    {
        private bool enabledValue;
        public bool Enabled
        {
            get
            {
                return enabledValue;
            }
            set
            {
                enabledValue = value;
            }
        }

        // Cloneメソッドをオーバーライドして、
        // Enabledプロパティがコピーされるようにします。
        public override object Clone()
        {
            DataGridViewDisableButtonCell cell =
                (DataGridViewDisableButtonCell)base.Clone();
            cell.Enabled = this.Enabled;
            return cell;
        }

        // デフォルトでは、ボタンセルを有効にします。
        public DataGridViewDisableButtonCell()
        {
            this.enabledValue = true;
        }

        protected override void Paint(Graphics graphics,
            Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
            DataGridViewElementStates elementState, object value,
            object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            // ボタンセルは無効になっているため、
            // セルの境界線、背景、および無効なボタンをペイントします。
            if (!this.enabledValue)
            {
                // 指定されている場合、セルの背景を描画します。
                if ((paintParts & DataGridViewPaintParts.Background) ==
                    DataGridViewPaintParts.Background)
                {
                    SolidBrush cellBackground =
                        new SolidBrush(cellStyle.BackColor);
                    graphics.FillRectangle(cellBackground, cellBounds);
                    cellBackground.Dispose();
                }

                // 指定されている場合、セルの境界線を描画します。
                if ((paintParts & DataGridViewPaintParts.Border) ==
                    DataGridViewPaintParts.Border)
                {
                    PaintBorder(graphics, clipBounds, cellBounds, cellStyle,
                        advancedBorderStyle);
                }

                // ボタンを描画する領域を計算します。
                Rectangle buttonArea = cellBounds;
                Rectangle buttonAdjustment =
                    this.BorderWidths(advancedBorderStyle);
                buttonArea.X += buttonAdjustment.X;
                buttonArea.Y += buttonAdjustment.Y;
                buttonArea.Height -= buttonAdjustment.Height;
                buttonArea.Width -= buttonAdjustment.Width;

                // 無効なボタンのテキストを描画します。
                if (this.FormattedValue is String)
                {
                    TextRenderer.DrawText(graphics,
                        (string)this.FormattedValue,
                        new Font("メイリオ", 8.25F),
                        buttonArea, SystemColors.WindowText);
                    // 文字色を黒にする
                    // マウスオーバーした際の背景色を変更する
                }
            }
            else
            {
                // ボタンセルが有効になっているため、
                // 基本クラスにペイントを処理させます。
                base.Paint(graphics, clipBounds, cellBounds, rowIndex,
                    elementState, value, formattedValue, errorText,
                    cellStyle, advancedBorderStyle, paintParts);
            }
        }
    }
    #endregion
}
