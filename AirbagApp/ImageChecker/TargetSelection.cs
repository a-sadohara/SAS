﻿using ImageChecker.DTO;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        // データ保持関連
        private DataTable m_dtData;

        // 選択行保持
        private int m_intSelRowIdx = -1;
        private int m_intFirstDisplayedScrollingRowIdx = -1;

        // スーパーユーザフラグ
        private bool m_bolIsSuperUser = false;

        // フォーム制御フラグ
        private bool m_bolFormControlFlag = true;

        // パラメータ
        private int m_intInspectionNum = -1;
        private string m_strInspectionDate = string.Empty;
        private string m_strFabricName = string.Empty;
        private string m_strUnitNum = string.Empty;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TargetSelection()
        {
            g_bolGridRepresentationFlg = true;

            InitializeComponent();
        }

        /// <summary>
        /// データグリッドビュー表示
        /// </summary>
        /// <param name="bolAllGetFlg">全取得フラグ</param>
        private void bolDispDataGridView(bool bolAllGetFlg)
        {
            string strSQL = string.Empty;
            StringBuilder sbFabricInfo = new StringBuilder();
            StringBuilder sbInspectionInfo = new StringBuilder();
            StringBuilder sbInspectionState = new StringBuilder();
            int intOverDetectionExceptStatus = -1;
            int intAcceptanceCheckStatus = -1;
            string strBefore48hourYmdhms = DateTime.Now.AddHours(-48).ToString("yyyy/MM/dd HH:mm:ss");
            ArrayList arrRow = new ArrayList();
            string strInspectionEndLine = string.Empty;
            DataTable dtTempData = new DataTable();
            DataTable dtCopyData = new DataTable();
            DataGridViewDisableButtonCell btnCellOverDetectionExcept = null;
            DataGridViewDisableButtonCell btnCellAcceptanceCheck = null;
            DataGridViewDisableButtonCell btnCellInspectionResult = null;
            int intAcceptanceCheckStatusGridData = -1;
            int intOverDetectionExceptStatusGridData = -1;
            int intIndex = -1;

            if (bolAllGetFlg)
            {
                // 洗替するため全クリアする
                dgvTargetSelection.Rows.Clear();
                m_dtData = new DataTable();
            }
            else
            {
                // 元のレコードを退避する
                dtCopyData = m_dtData.Copy();

                // 再取得対象のレコードをクリアする
                dgvTargetSelection.Rows.RemoveAt(m_intSelRowIdx);
                m_dtData.Rows.RemoveAt(m_intSelRowIdx);
            }

            if (dgvTargetSelection.Columns.Count != 7)
            {
                // グリッドビューにボタンを追加
                DataGridViewDisableButtonColumn btnOverDetectionExcept = new DataGridViewDisableButtonColumn();
                btnOverDetectionExcept.FlatStyle = FlatStyle.Flat;
                btnOverDetectionExcept.Width = 230;
                DataGridViewDisableButtonColumn btnAcceptanceCheck = new DataGridViewDisableButtonColumn();
                btnAcceptanceCheck.FlatStyle = FlatStyle.Flat;
                btnAcceptanceCheck.Width = 230;
                DataGridViewDisableButtonColumn btnInspectionResult = new DataGridViewDisableButtonColumn();
                btnInspectionResult.FlatStyle = FlatStyle.Flat;
                btnInspectionResult.Width = 230;
                this.dgvTargetSelection.Columns.Add(btnOverDetectionExcept);
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].Name = "OverDetectionExcept";
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].HeaderText = string.Empty;
                this.dgvTargetSelection.Columns.Add(btnAcceptanceCheck);
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].Name = "AcceptanceCheck";
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].HeaderText = string.Empty;
                this.dgvTargetSelection.Columns.Add(btnInspectionResult);
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].Name = "InspectionResult";
                this.dgvTargetSelection.Columns[this.dgvTargetSelection.Columns.Count - 1].HeaderText = string.Empty;
                btnOverDetectionExcept.Dispose();
                btnAcceptanceCheck.Dispose();
                btnInspectionResult.Dispose();
            }

            try
            {
                // SQL抽出
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
                                   AND   inspection_num = dr.inspection_num
                                   AND   unit_num = dr.unit_num) AS detection_image_count
                                , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                   WHERE over_detection_except_result = :over_detection_except_result_ok
                                   AND   fabric_name = dr.fabric_name
                                   AND   inspection_date = dr.inspection_date
                                   AND   inspection_num = dr.inspection_num
                                   AND   unit_num = dr.unit_num) AS over_detection_image_count
                                , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                  WHERE (over_detection_except_result = :over_detection_except_result_ng
                                  OR over_detection_except_result = :over_detection_except_result_ng_non_detect)
                                  AND   fabric_name = dr.fabric_name
                                  AND   inspection_date = dr.inspection_date
                                  AND   inspection_num = dr.inspection_num
                                  AND   unit_num = dr.unit_num) AS acceptance_check_image_count
                                , (SELECT COUNT(*) FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                   WHERE acceptance_check_result <> :acceptance_check_result_non
                                   AND   fabric_name = dr.fabric_name
                                   AND   inspection_date = dr.inspection_date
                                   AND   inspection_num = dr.inspection_num
                                   AND   unit_num = dr.unit_num) AS determine_count
                                , mpi.column_cnt
                                , mpi.airbag_imagepath
                           FROM
                               " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header iih
                           LEFT JOIN (
                                   SELECT fabric_name,inspection_date,inspection_num,unit_num
                                   FROM   " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                   GROUP BY fabric_name,inspection_date,inspection_num,unit_num
                               ) dr
                           ON  iih.fabric_name = dr.fabric_name
                           AND iih.inspection_date = dr.inspection_date
                           AND iih.inspection_num = dr.inspection_num
                           AND iih.unit_num = dr.unit_num
                           INNER JOIN mst_product_info mpi
                           ON  iih.product_name = mpi.product_name
                           WHERE (iih.result_datetime IS NULL OR
                                  iih.result_datetime >= TO_TIMESTAMP('" + strBefore48hourYmdhms + @"','YYYY/MM/DD HH24:MI:SS')) ";


                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ng", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNg });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ng_non_detect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNgNonDetect });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNon });

                if (bolAllGetFlg)
                {
                    strSQL += "ORDER BY iih.end_datetime DESC ";

                    g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);
                    dtTempData = m_dtData;
                }
                else
                {
                    strSQL += @"AND iih.inspection_num = :inspection_num
                                AND TO_CHAR(iih.inspection_date,'YYYY/MM/DD') = :inspection_date
                                AND iih.fabric_name = :fabric_name
                                AND iih.unit_num = :unit_num ";

                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    g_clsConnectionNpgsql.SelectSQL(ref dtTempData, strSQL, lstNpgsqlCommand);
                }

                // データグリッドに反映
                foreach (DataRow row in dtTempData.Rows)
                {
                    // 初期化
                    sbFabricInfo.Clear();
                    sbInspectionInfo.Clear();
                    sbInspectionState.Clear();
                    arrRow.Clear();
                    intOverDetectionExceptStatus = -1;
                    intAcceptanceCheckStatus = -1;
                    strInspectionEndLine = row["inspection_end_line"].ToString();
                    intAcceptanceCheckStatusGridData = int.Parse(row["acceptance_check_status"].ToString());
                    intOverDetectionExceptStatusGridData = int.Parse(row["over_detection_except_status"].ToString());

                    if (bolAllGetFlg)
                    {
                        // No
                        arrRow.Add(this.dgvTargetSelection.Rows.Count + 1);
                    }
                    else
                    {
                        // No
                        arrRow.Add(m_intSelRowIdx + 1);
                    }

                    // 反物情報
                    sbFabricInfo.AppendLine(string.Format(m_CON_FORMAT_UNIT_NUM, row["unit_num"]));
                    sbFabricInfo.AppendLine(string.Format(m_CON_FORMAT_ORDER_IMG, row["order_img"]));
                    sbFabricInfo.AppendLine(string.Format(m_CON_FORMAT_FABRIC_NAME, row["fabric_name"]));
                    sbFabricInfo.AppendLine(string.Format(m_CON_FORMAT_PRODUCT_NAME, row["product_name"]));
                    arrRow.Add(sbFabricInfo.ToString());

                    if (strInspectionEndLine.Equals(decimal.Zero.ToString()))
                    {
                        strInspectionEndLine = string.Empty;
                    }

                    // 外観検査情報
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_START_DATETIME, row["start_datetime"]));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_END_DATETIME, row["end_datetime"]));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_INSPECTION_LINE, row["inspection_start_line"], strInspectionEndLine));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_DECISION_START_DATETIME, row["decision_start_datetime"]));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_DECISION_END_DATETIME, row["decision_end_datetime"]));
                    sbInspectionInfo.AppendLine(string.Format(m_CON_FORMAT_INSPECTION_NUM, row["inspection_num"]));
                    arrRow.Add(sbInspectionInfo.ToString());

                    // 欠点検査状態 & ステータス
                    if (intAcceptanceCheckStatusGridData == g_clsSystemSettingInfo.intAcceptanceCheckStatusBef ||
                        intAcceptanceCheckStatusGridData == g_clsSystemSettingInfo.intAcceptanceCheckStatusExc)
                    {
                        //  過検知除外ステータス
                        if (intOverDetectionExceptStatusGridData == g_clsSystemSettingInfo.intOverDetectionExceptStatusBef)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameBef));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_DETECTION_IMAGE_COUNT_SCORES, row["detection_image_count"], "0", row["detection_image_count"]));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusBef;
                        }
                        else if (intOverDetectionExceptStatusGridData == g_clsSystemSettingInfo.intOverDetectionExceptStatusChk)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameChk));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_DETECTION_IMAGE_COUNT_SCORES, row["detection_image_count"], row["over_detection_image_count"], row["detection_image_count"]));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusChk;
                        }
                        else if (intOverDetectionExceptStatusGridData == g_clsSystemSettingInfo.intOverDetectionExceptStatusStp)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameStp));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_DETECTION_IMAGE_COUNT_SCORES, row["detection_image_count"], row["over_detection_image_count"], row["detection_image_count"]));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusStp;
                        }
                        else if (intOverDetectionExceptStatusGridData == g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameEnd));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_OVER_DETECTION_IMAGE_COUNT_SCORES, row["acceptance_check_image_count"], "0", row["acceptance_check_image_count"]));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd;
                        }
                        else if (intOverDetectionExceptStatusGridData == g_clsSystemSettingInfo.intOverDetectionExceptStatusExc)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strOverDetectionExceptStatusNameExc));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_DETECTION_IMAGE_COUNT, row["detection_image_count"].ToString()));
                            intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusExc;
                        }
                    }
                    else
                    {
                        // 合否確認ステータス
                        if (intAcceptanceCheckStatusGridData == g_clsSystemSettingInfo.intAcceptanceCheckStatusChk)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strAcceptanceCheckStatusNameChk));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_OVER_DETECTION_IMAGE_COUNT_SCORES, row["over_detection_image_count"], row["determine_count"], row["over_detection_image_count"]));
                            intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusChk;
                        }
                        else if (intAcceptanceCheckStatusGridData == g_clsSystemSettingInfo.intAcceptanceCheckStatusStp)
                        {
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_STATUS, g_clsSystemSettingInfo.strAcceptanceCheckStatusNameChk));
                            sbInspectionState.AppendLine(string.Format(m_CON_FORMAT_OVER_DETECTION_IMAGE_COUNT_SCORES, row["acceptance_check_image_count"], row["determine_count"], row["acceptance_check_image_count"]));
                            intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusStp;
                        }
                        else if (intAcceptanceCheckStatusGridData == g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd)
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

                    if (bolAllGetFlg)
                    {
                        this.dgvTargetSelection.Rows.Add(arrRow.ToArray());
                        intIndex = this.dgvTargetSelection.Rows.Count - 1;
                    }
                    else
                    {
                        // データを挿入する
                        this.dgvTargetSelection.Rows.Insert(m_intSelRowIdx, arrRow.ToArray());

                        m_dtData.Clear();

                        for (int rowCount = 0; rowCount < dtCopyData.Rows.Count; rowCount++)
                        {
                            if (m_intSelRowIdx == rowCount)
                            {
                                m_dtData.ImportRow(row);
                            }
                            else
                            {
                                m_dtData.ImportRow(dtCopyData.Rows[rowCount]);
                            }
                        }

                        intIndex = m_intSelRowIdx;
                    }

                    // ボタン設定
                    btnCellOverDetectionExcept = (DataGridViewDisableButtonCell)this.dgvTargetSelection.Rows[intIndex].Cells[m_CON_COL_IDX_OVERDETECTIONEXCEPT];
                    btnCellAcceptanceCheck = (DataGridViewDisableButtonCell)this.dgvTargetSelection.Rows[intIndex].Cells[m_CON_COL_IDX_ACCEPTANCECHECK];
                    btnCellInspectionResult = (DataGridViewDisableButtonCell)this.dgvTargetSelection.Rows[intIndex].Cells[m_CON_COL_IDX_RESULT];

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

                        // スーパーユーザの場合、押下可能にする
                        if (m_bolIsSuperUser)
                        {
                            btnCellAcceptanceCheck.Enabled = true;
                        }
                        else
                        {
                            btnCellAcceptanceCheck.Enabled = false;
                        }

                        btnCellInspectionResult.Enabled = false;
                    }
                    else if (intAcceptanceCheckStatus == g_clsSystemSettingInfo.intAcceptanceCheckStatusStp)
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
                    else if (intAcceptanceCheckStatus == g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd)
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
                    else if (intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusChk)
                    {
                        // 過検知除外：検査中
                        btnCellOverDetectionExcept.Style.BackColor = Color.Red;
                        btnCellOverDetectionExcept.Style.SelectionBackColor = Color.Red;
                        btnCellAcceptanceCheck.Style.BackColor = Color.DarkGray;
                        btnCellAcceptanceCheck.Style.SelectionBackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.BackColor = Color.DarkGray;
                        btnCellInspectionResult.Style.SelectionBackColor = Color.DarkGray;

                        // スーパーユーザの場合、押下可能にする
                        if (m_bolIsSuperUser)
                        {
                            btnCellOverDetectionExcept.Enabled = true;
                        }
                        else
                        {
                            btnCellOverDetectionExcept.Enabled = false;
                        }

                        btnCellAcceptanceCheck.Enabled = false;
                        btnCellInspectionResult.Enabled = false;
                    }
                    else if (intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusStp)
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
                    else if (intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd)
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
                    else if (intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusExc)
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

                if (bolAllGetFlg)
                {
                    m_intSelRowIdx = 0;
                    m_intFirstDisplayedScrollingRowIdx = 0;
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
                if (m_intFirstDisplayedScrollingRowIdx != -1)
                {
                    try
                    {
                        this.dgvTargetSelection.FirstDisplayedScrollingRowIndex = m_intFirstDisplayedScrollingRowIdx;
                    }
                    catch
                    {
                        // 一覧がリセットされる瞬間においては例外が発生するが、無視する
                    }
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

                m_intInspectionNum = -1;
                m_strInspectionDate = string.Empty;
                m_strFabricName = string.Empty;
                m_strUnitNum = string.Empty;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}処理ブロック:{2}{3}{4}",
                        g_clsMessageInfo.strMsgE0002,
                        Environment.NewLine,
                        "検査対象選択一覧情報取得・表示",
                        Environment.NewLine,
                        ex.Message));

                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0042, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 検査ステータス更新
        /// </summary>
        /// <param name="clsHeaderData">ヘッダ情報</param>
        /// <param name="btnTarget">選択ボタン</param>
        private void UpdateInspectionStatus(
            HeaderData clsHeaderData,
            DataGridViewDisableButtonCell btnTarget)
        {
            string strSQL = string.Empty;
            string strErrorMessage = string.Empty;
            string strLogMessage =
                    string.Format(
                        g_CON_LOG_MESSAGE_FOMAT,
                        clsHeaderData.strUnitNum,
                        clsHeaderData.strInspectionDate,
                        clsHeaderData.intInspectionNum,
                        clsHeaderData.strProductName,
                        clsHeaderData.strFabricName);

            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0010, "中断状態"), g_CON_MESSAGE_TITLE_QUESTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            try
            {
                // SQL文を作成する
                strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header ";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = clsHeaderData.strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = clsHeaderData.strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = clsHeaderData.intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = clsHeaderData.strUnitNum });

                switch (btnTarget.Value.ToString())
                {
                    case m_CON_OVER_DETECTION_EXCEPT_BUTTON_NAME:
                        // 過検知除外ステータスを更新する
                        strSQL += @"SET over_detection_except_status = :over_detection_except_status ";
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_status", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptStatusStp });
                        strErrorMessage = "過検知除外ステータス";
                        break;

                    case m_CON_ACCEPTANCE_CHECK_BUTTON_NAME:
                        // 合否確認ステータスを更新する
                        strSQL += @"SET acceptance_check_status = :acceptance_check_status ";
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_status", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckStatusStp });
                        strErrorMessage = "合否確認ステータス";
                        break;

                    default:
                        return;
                }

                strSQL += @"WHERE fabric_name = :fabric_name
                            AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date
                            AND inspection_num = :inspection_num
                            AND unit_num = :unit_num";

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);

                // DBコミット
                g_clsConnectionNpgsql.DbCommit();
            }
            catch (Exception ex)
            {
                // ロールバック
                g_clsConnectionNpgsql.DbRollback();

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}{3}処理ブロック:{4}{5}{6}{7}",
                        g_clsMessageInfo.strMsgE0002,
                        Environment.NewLine,
                        strLogMessage,
                        Environment.NewLine,
                        strErrorMessage,
                        "更新[作業中⇒中断]",
                        Environment.NewLine,
                        ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    g_clsMessageInfo.strMsgE0035,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }
            finally
            {
                // DBクローズ
                g_clsConnectionNpgsql.DbClose();
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
            bool? bolCheckNGRecordResult = true;
            bool? bolCheckImageResult = true;
            int intImageCount = 0;

            // 画像ディレクトリが存在しない場合、フォルダを作成する
            if (!Directory.Exists(strFaultImageFileDirectory))
            {
                Directory.CreateDirectory(strFaultImageFileDirectory);
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
                frmProgress.Dispose();
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

            // ログイン者がスーパーユーザかチェックを行う
            if (g_clsSystemSettingInfo.strSuperUser.Split(',').Contains(g_clsLoginInfo.strEmployeeNum))
            {
                m_bolIsSuperUser = true;
            }

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
        private async void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridView dgv = (DataGridView)sender;
                HeaderData clsHeaderData = new HeaderData();
                DecisionResult clsDecisionResult = new DecisionResult();
                string strFaultImageSubDirectory = string.Empty;
                string strLogMessage = string.Empty;

                // ボタン行以外はイベント終了
                if (e.ColumnIndex < m_CON_COL_IDX_OVERDETECTIONEXCEPT)
                {
                    return;
                }

                // ボタンが無効の場合は終了
                DataGridViewDisableButtonCell btnTarget = (DataGridViewDisableButtonCell)dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (!btnTarget.Enabled)
                {
                    return;
                }

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
                clsHeaderData.strAirbagImagepath = g_strMasterImageDirPath + Path.DirectorySeparatorChar +
                                                   Path.GetFileName(m_dtData.Rows[e.RowIndex]["airbag_imagepath"].ToString());
                clsHeaderData.strFaultImageDirectory = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, clsHeaderData.strUnitNum);
                m_intInspectionNum = clsHeaderData.intInspectionNum;
                m_strInspectionDate = clsHeaderData.strInspectionDate;
                m_strFabricName = clsHeaderData.strFabricName;
                m_strUnitNum = clsHeaderData.strUnitNum;

                strLogMessage =
                    string.Format(
                        g_CON_LOG_MESSAGE_FOMAT,
                        clsHeaderData.strUnitNum,
                        clsHeaderData.strInspectionDate,
                        clsHeaderData.intInspectionNum,
                        clsHeaderData.strProductName,
                        clsHeaderData.strFabricName);

                g_bolGridRepresentationFlg = false;

                // スーパーユーザが検査中ボタンを押下したかチェックする
                if (m_bolIsSuperUser &&
                    (btnTarget.Style.BackColor == Color.Red ||
                    btnTarget.Style.SelectionBackColor == Color.Red))
                {
                    UpdateInspectionStatus(
                        clsHeaderData,
                        btnTarget);

                    // 画面更新を行う
                    bolDispDataGridView(false);
                    return;
                }

                // 検査中かステータスをチェックする
                if (bolCheckWorkingStatus(clsHeaderData.intInspectionNum, clsHeaderData.strInspectionDate, clsHeaderData.strFabricName, clsHeaderData.strUnitNum, strLogMessage))
                {
                    // 画面更新を行う
                    bolDispDataGridView(false);
                    return;
                }

                strFaultImageSubDirectory = string.Join("_", clsHeaderData.strInspectionDate.Replace("/", string.Empty),
                                                             clsHeaderData.strProductName,
                                                             clsHeaderData.strFabricName,
                                                             clsHeaderData.intInspectionNum);

                // ディレクトリ存在チェック
                bool? tskRet =
                    await BolCheckFaultImage(
                        clsHeaderData.intInspectionNum,
                        clsHeaderData.strInspectionDate,
                        clsHeaderData.strUnitNum,
                        clsHeaderData.strFabricName,
                        strFaultImageSubDirectory,
                        strLogMessage);

                if (tskRet == null)
                {
                    return;
                }

                if (!tskRet.Equals(true))
                {
                    DirectoryInfo diFaultImage = new DirectoryInfo(Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, clsHeaderData.strUnitNum, strFaultImageSubDirectory));

                    if (!diFaultImage.Exists ||
                        diFaultImage.GetFiles().Where(x => string.Compare(x.Extension, ".jpg", true) == 0).Count() == 0)
                    {
                        MessageBox.Show(g_clsMessageInfo.strMsgE0068, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        MessageBox.Show(g_clsMessageInfo.strMsgW0006, g_CON_MESSAGE_TITLE_WARN, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                m_intInspectionNum = clsHeaderData.intInspectionNum;
                m_strInspectionDate = clsHeaderData.strInspectionDate;
                m_strFabricName = clsHeaderData.strFabricName;
                m_strUnitNum = clsHeaderData.strUnitNum;
                this.Visible = false;
                m_bolFormControlFlag = false;
                switch (e.ColumnIndex)
                {
                    case m_CON_COL_IDX_OVERDETECTIONEXCEPT:

                        // 過検知除外
                        using (Overdetectionexclusion frmOverDetectionExcept = new Overdetectionexclusion(ref clsHeaderData))
                        {
                            frmOverDetectionExcept.ShowDialog(this);

                            // 過検知除外結果から合否確認に遷移
                            if (frmOverDetectionExcept.intDestination == g_CON_APID_RESULT_CHECK)
                            {
                                // 変数を再初期化する
                                g_bolGridRepresentationFlg = false;

                                using (ResultCheck frmResultCheck = new ResultCheck(ref clsHeaderData, clsDecisionResult))
                                {
                                    frmResultCheck.ShowDialog(this);
                                }
                            }
                        }

                        m_bolFormControlFlag = true;

                        // 画面更新を行う
                        bolDispDataGridView(false);

                        break;

                    case m_CON_COL_IDX_ACCEPTANCECHECK:

                        // 合否確認・判定登録
                        using (ResultCheck frmResultCheck = new ResultCheck(ref clsHeaderData, clsDecisionResult))
                        {
                            frmResultCheck.ShowDialog(this);
                        }

                        m_bolFormControlFlag = true;

                        // 画面更新を行う
                        bolDispDataGridView(false);

                        break;

                    case m_CON_COL_IDX_RESULT:

                        // 検査結果確認
                        using (DisplayResults frmInspectionResult = new DisplayResults(ref clsHeaderData))
                        {
                            frmInspectionResult.ShowDialog(this);
                        }

                        m_bolFormControlFlag = true;

                        break;
                }
            }
            finally
            {
            }

            if (!g_clsLoginInfo.bolStatus)
            {
                this.Close();
                return;
            }

            // 連携処理をして画面表示
            this.Visible = true;
            this.Refresh();
            this.TargetSelection_Activated(null, null);
        }

        /// <summary>
        /// ログアウトボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
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

                // 変数を初期化する
                m_bolFormControlFlag = false;
                g_bolGridRepresentationFlg = false;

                using (DisplayResultsAgo frmResult = new DisplayResultsAgo())
                {
                    frmResult.ShowDialog(this);
                }

                m_bolFormControlFlag = true;
            }
            finally
            {
            }

            if (!g_clsLoginInfo.bolStatus)
            {
                this.Close();
                return;
            }

            // 連携処理をして画面表示
            this.Visible = true;
            this.Refresh();
            this.TargetSelection_Activated(null, null);
        }

        /// <summary>
        /// 検査対象外ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExceptTarget_Click(object sender, EventArgs e)
        {
            string strUnitNum = string.Empty;
            string strOrderImg = string.Empty;
            string strFabricName = string.Empty;
            string strProductName = string.Empty;
            string strInspectionDate = string.Empty;
            string strStartDatetime = string.Empty;
            string strEndDatetime = string.Empty;
            string strLogMessage = string.Empty;
            int intInspectionStartLine = -1;
            int intInspectionEndLine = -1;
            int intInspectionNum = 0;
            bool bolInspection = true;
            int intRow = -1;

            try
            {
                // 選択行インデックスの取得
                foreach (DataGridViewRow dgvRow in this.dgvTargetSelection.SelectedRows)
                {
                    intRow = dgvRow.Index;
                    break;
                }

                // 変数を初期化する
                g_bolGridRepresentationFlg = false;

                // 合否確認ステータス：検査終了の場合
                if (Convert.ToInt32(m_dtData.Rows[intRow]["acceptance_check_status"]) == g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd)
                {
                    // メッセージ出力
                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0062, btnExceptTarget.Text), g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // 既に検査対象外の場合
                if (int.Parse(m_dtData.Rows[intRow]["over_detection_except_status"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptStatusExc)
                {
                    bolInspection = false;
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
                m_intInspectionNum = intInspectionNum;
                m_strInspectionDate = strInspectionDate;
                m_strFabricName = strFabricName;
                m_strUnitNum = strUnitNum;

                strLogMessage =
                    string.Format(
                        g_CON_LOG_MESSAGE_FOMAT,
                            strUnitNum,
                            strInspectionDate,
                            intInspectionNum,
                            strProductName,
                            strFabricName);

                bool? bolRapidTableCheckResult = BolCheckRapidTableCount(intInspectionNum, strFabricName, strInspectionDate, strUnitNum, strLogMessage);

                // Rapidテーブルが存在する場合、NGレコード数のチェックを行う
                if (bolRapidTableCheckResult.Equals(true))
                {
                    bool? bolCheckResultInfo = BolCheckNGRecordCount(intInspectionNum, strFabricName, strInspectionDate, strUnitNum, strLogMessage, true);

                    if (bolCheckResultInfo == null)
                    {
                        return;
                    }
                    else if (bolCheckResultInfo.Equals(false))
                    {
                        // メッセージ出力
                        MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0070, btnExceptTarget.Text), g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }
                }

                // 検査対象外画面表示
                using (CheckExcept frmCheckExcept =
                    new CheckExcept(
                        strUnitNum,
                        strOrderImg,
                        strFabricName,
                        strProductName,
                        intInspectionNum,
                        strInspectionDate,
                        strStartDatetime,
                        strEndDatetime,
                        intInspectionStartLine,
                        intInspectionEndLine,
                        bolInspection))
                {
                    frmCheckExcept.ShowDialog(this);

                    if (frmCheckExcept.bolUpdateFlg)
                    {
                        // 画面更新を行う
                        bolDispDataGridView(false);
                    }
                }
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
            string strUnitNum = string.Empty;
            string strOrderImg = string.Empty;
            string strFabricName = string.Empty;
            string strProductName = string.Empty;
            string strInspectionDate = string.Empty;
            string strStartDatetime = string.Empty;
            string strEndDatetime = string.Empty;
            string strLogMessage = string.Empty;
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

                // 変数を初期化する
                g_bolGridRepresentationFlg = false;

                // 合否確認ステータス：検査終了の場合
                if (Convert.ToInt32(m_dtData.Rows[intRow]["acceptance_check_status"]) == g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd)
                {
                    // メッセージ出力
                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0062, btnReviseLine.Text), g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // 既に検査対象外の場合
                if (int.Parse(m_dtData.Rows[intRow]["over_detection_except_status"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptStatusExc)
                {
                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0064, "検査情報"), g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
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
                m_intInspectionNum = intInspectionNum;
                m_strInspectionDate = strInspectionDate;
                m_strFabricName = strFabricName;
                m_strUnitNum = strUnitNum;

                strLogMessage =
                    string.Format(
                        g_CON_LOG_MESSAGE_FOMAT,
                            strUnitNum,
                            strInspectionDate,
                            intInspectionNum,
                            strProductName,
                            strFabricName);

                bool? bolRapidTableCheckResult = BolCheckRapidTableCount(intInspectionNum, strFabricName, strInspectionDate, strUnitNum, strLogMessage);

                // Rapidテーブルが存在しない場合、処理を終了する
                if (!bolRapidTableCheckResult.Equals(true))
                {
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgW0003, g_CON_MESSAGE_TITLE_WARN, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                bool? bolCheckResultInfo = BolCheckNGRecordCount(intInspectionNum, strFabricName, strInspectionDate, strUnitNum, strLogMessage, true);

                if (bolCheckResultInfo == null)
                {
                    return;
                }
                else if (bolCheckResultInfo.Equals(false))
                {
                    // メッセージ出力
                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0070, btnReviseLine.Text), g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // 行補正画面表示
                using (LineCorrect frmLineCorrect =
                    new LineCorrect(
                        strUnitNum,
                        strOrderImg,
                        strFabricName,
                        strProductName,
                        intInspectionNum,
                        strInspectionDate,
                        strStartDatetime,
                        strEndDatetime,
                        intInspectionStartLine,
                        intInspectionEndLine))
                {
                    frmLineCorrect.ShowDialog(this);

                    if (frmLineCorrect.bolUpdateFlg)
                    {
                        // 画面更新を行う
                        bolDispDataGridView(false);
                    }
                }
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
            if (!g_bolGridRepresentationFlg)
            {
                g_bolGridRepresentationFlg = true;
                await Task.Delay(10);
                return;
            }

            // 連携基盤部との前回連携から5分以内の場合は、処理しない
            if (g_datetimePrevReplicate != DateTime.MinValue &&
                g_datetimePrevReplicate > DateTime.Now)
            {
                return;
            }

            await Task.Delay(100);

            if (!this.Visible)
            {
                return;
            }

            this.SuspendLayout();

            ImportImageZipProgressForm frmProgress = new ImportImageZipProgressForm();
            frmProgress.StartPosition = FormStartPosition.CenterScreen;
            frmProgress.Size = this.Size;

            // 連携基盤部連携ファイルの取込処理
            string[] strSplitFileName = new string[0];
            string strFileName = string.Empty;
            string strSQL = string.Empty;
            int intCnt = 0;
            DataTable dtPublicHeaderData = new DataTable();
            DataTable dtImagecheckerHeaderData = new DataTable();
            DataTable dtRapidDisabledData = new DataTable();

            // パラメータ
            string strInspectionDate = string.Empty;  // 検査日付(YYYY/MM/DD)
            string strProductName = string.Empty;     // 品名
            string strFabricName = string.Empty;      // 反番
            int intInspectionNum = 0;       // 検査番号
            string strNgImageCooperationDirectory = string.Empty;
            string strFaultImageFileName = string.Empty;
            string strFaultImageFileDirectory = string.Empty;
            string strRapidTableName = string.Empty;
            string strUnitNum = string.Empty;
            string strLogMessage = string.Empty;
            int intExecutionCount = 0;
            bool bolDisplayFlg = false;
            DateTime dateSyncTargetDate = DateTime.Now.Date.AddDays(g_clsSystemSettingInfo.intNgImageAcquisitionPeriod);

            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
            List<Task<Boolean>> lstTask = new List<Task<Boolean>>();

            if (g_datetimePrevReplicate == DateTime.MinValue)
            {
                bolDisplayFlg = true;
            }

            // 連携時間を更新
            g_datetimePrevReplicate = DateTime.Now.AddMinutes(5);

            // チェック対象の完了通知連携ディレクトリを設定する
            string[] strCompletionNoticeCooperationDirectoryArray =
            {
                g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN1,
                g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN2,
                g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN3,
                g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectoryN4
            };

            try
            {
                foreach (string strCompletionNoticeCooperationDirectory in strCompletionNoticeCooperationDirectoryArray)
                {
                    // 完了通知連携ディレクトリが未設定の場合、処理をスキップする
                    if (string.IsNullOrWhiteSpace(strCompletionNoticeCooperationDirectory))
                    {
                        continue;
                    }

                    // 完了通知ファイル(.TXT)の確認を実施
                    foreach (string strFilePath in Directory.GetFiles(strCompletionNoticeCooperationDirectory, "*", SearchOption.TopDirectoryOnly).Where(x => x.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)))
                    {
                        // 初期化
                        strInspectionDate = string.Empty;
                        strProductName = string.Empty;
                        strFabricName = string.Empty;
                        intInspectionNum = 0;
                        strNgImageCooperationDirectory = string.Empty;
                        strFaultImageFileName = string.Empty;
                        strFaultImageFileDirectory = string.Empty;
                        strRapidTableName = string.Empty;
                        strUnitNum = string.Empty;
                        lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                        intCnt = 0;
                        intExecutionCount = 0;

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

                            strFaultImageFileName = string.Join("_", strInspectionDate.Replace("/", string.Empty),
                                                                            strProductName,
                                                                            strFabricName,
                                                                            intInspectionNum);
                        }

                        // 取得失敗した場合は次のループ
                        if (string.IsNullOrEmpty(strInspectionDate) || string.IsNullOrEmpty(strProductName) || string.IsNullOrEmpty(strFabricName) || intInspectionNum == 0)
                        {
                            continue;
                        }

                        try
                        {
                            // 存在チェックを行う
                            dtPublicHeaderData = new DataTable();
                            strSQL = @"SELECT unit_num ";
                            strSQL += @"FROM  " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".inspection_info_header ";
                            strSQL += @"WHERE fabric_name = :fabric_name ";
                            strSQL += @"AND   inspection_num = :inspection_num ";
                            strSQL += @"AND   TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd ";

                            // SQLコマンドに各パラメータを設定する
                            lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                            lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                            lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                            lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });

                            g_clsConnectionNpgsql.SelectSQL(ref dtPublicHeaderData, strSQL, lstNpgsqlCommand);
                        }
                        catch (PostgresException pgex)
                        {
                            // ログ出力
                            WriteEventLog(
                                g_CON_LEVEL_ERROR,
                                string.Format(
                                    "{0}{1}検査日付:{2}, 検査番号:{3}, 品名:{4}, 反番:{5}, 取得対象テーブル:{6}, 処理ブロック:{7}{8}{9}",
                                    g_clsMessageInfo.strMsgE0001,
                                    Environment.NewLine,
                                    strInspectionDate,
                                    intInspectionNum,
                                    strProductName,
                                    strFabricName,
                                    g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".inspection_info_header",
                                    "号機情報取得",
                                    Environment.NewLine,
                                    pgex.Message));

                            // メッセージ出力
                            MessageBox.Show(g_clsMessageInfo.strMsgE0031, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                            continue;
                        }
                        catch (Exception ex)
                        {
                            // ログ出力
                            WriteEventLog(
                                g_CON_LEVEL_ERROR,
                                string.Format(
                                    "{0}{1}処理ブロック:{2}{3}{4}{5}",
                                    g_clsMessageInfo.strMsgE0001,
                                    Environment.NewLine,
                                    g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".inspection_info_header",
                                    "号機情報取得",
                                    Environment.NewLine,
                                    ex.Message));

                            // メッセージ出力
                            MessageBox.Show(g_clsMessageInfo.strMsgE0031, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                            continue;
                        }

                        for (int rowCount = 0; rowCount < dtPublicHeaderData.Rows.Count; rowCount++)
                        {
                            strUnitNum = dtPublicHeaderData.Rows[rowCount]["unit_num"].ToString();

                            // 号機情報に紐付くNG画像連携ディレクトリを設定する
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
                                    continue;
                            }

                            strLogMessage =
                                string.Format(
                                    g_CON_LOG_MESSAGE_FOMAT,
                                        strUnitNum,
                                        strInspectionDate,
                                        intInspectionNum,
                                        strProductName,
                                        strFabricName);

                            // 連携済みチェック
                            try
                            {
                                dtImagecheckerHeaderData = new DataTable();
                                strSQL = @"SELECT COUNT(inspection_num) AS cnt ";
                                strSQL += @"FROM  " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header ";
                                strSQL += @"WHERE fabric_name = :fabric_name ";
                                strSQL += @"AND   inspection_num = :inspection_num ";
                                strSQL += @"AND   TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd ";
                                strSQL += @"AND   unit_num = :unit_num ";

                                // SQLコマンドに各パラメータを設定する
                                lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });

                                g_clsConnectionNpgsql.SelectSQL(ref dtImagecheckerHeaderData, strSQL, lstNpgsqlCommand);

                                // 件数
                                if (dtImagecheckerHeaderData.Rows.Count > 0)
                                {
                                    intCnt = int.Parse(dtImagecheckerHeaderData.Rows[0]["cnt"].ToString());
                                }
                            }
                            catch (PostgresException pgex)
                            {
                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    string.Format(
                                        "{0}{1}検査日付:{2}, {3}号機, 検査番号:{4}, 品名:{5}, 反番:{6}, 取得対象テーブル:{7}, 処理ブロック:{8}{9}{10}",
                                        g_clsMessageInfo.strMsgE0001,
                                        Environment.NewLine,
                                        strInspectionDate,
                                        strUnitNum,
                                        intInspectionNum,
                                        strProductName,
                                        strFabricName,
                                        g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header",
                                        "検査情報取込済チェック",
                                        Environment.NewLine,
                                        pgex.Message));

                                // メッセージ出力
                                MessageBox.Show(g_clsMessageInfo.strMsgE0031, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                continue;
                            }
                            catch (Exception ex)
                            {
                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    string.Format(
                                        "{0}{1}{2}{3}処理ブロック:{4}{5}{6}{7}",
                                        g_clsMessageInfo.strMsgE0001,
                                        Environment.NewLine,
                                        strLogMessage,
                                        Environment.NewLine,
                                        g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header",
                                        "検査情報取込済チェック",
                                        Environment.NewLine,
                                        ex.Message));

                                // メッセージ出力
                                MessageBox.Show(g_clsMessageInfo.strMsgE0031, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                continue;
                            }

                            if (intCnt >= 1)
                            {
                                // 連携済みのため、次を処理
                                continue;
                            }

                            bolDisplayFlg = true;

                            if (!frmProgress.Visible)
                            {
                                frmProgress.Show(this);
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
                                    , ai_model_non_inspection_flg
                                    , ai_model_name
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
                                    , COALESCE(end_datetime, :end_datetime) AS end_datetime
                                    , inspection_direction
                                    , inspection_date
                                    , 0
                                    , 0
                                    , NULL
                                    , NULL
                                    , ai_model_non_inspection_flg
                                    , ai_model_name
                                FROM (
                                    SELECT ROW_NUMBER() OVER(PARTITION BY inspection_date, fabric_name, inspection_num ORDER BY branch_num DESC) AS SEQ
                                    , header.*
                                    FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".inspection_info_header AS header
                                    WHERE fabric_name = :fabric_name
                                        AND inspection_num = :inspection_num
                                        AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                                        AND unit_num = :unit_num
                                ) header
                                WHERE SEQ = 1
                                ON CONFLICT
                                DO NOTHING ";

                                // SQLコマンドに各パラメータを設定する
                                lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "end_datetime", DbType = DbType.DateTime2, Value = File.GetLastWriteTime(strFilePath) });

                                // sqlを実行する
                                g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
                            }
                            catch (PostgresException pgex)
                            {
                                g_clsConnectionNpgsql.DbRollback();

                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    string.Format(
                                        "{0}{1}検査日付:{2}, {3}号機, 検査番号:{4}, 品名:{5}, 反番:{6}, 更新対象テーブル:{7}, 処理ブロック:{8}{9}{10}",
                                        g_clsMessageInfo.strMsgE0002,
                                        Environment.NewLine,
                                        strInspectionDate,
                                        strUnitNum,
                                        intInspectionNum,
                                        strProductName,
                                        strFabricName,
                                        g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header",
                                        "検査情報ヘッダレコード取込",
                                        Environment.NewLine,
                                        pgex.Message));

                                // メッセージ出力
                                MessageBox.Show(g_clsMessageInfo.strMsgE0035, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                continue;
                            }
                            catch (Exception ex)
                            {
                                g_clsConnectionNpgsql.DbRollback();

                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    string.Format(
                                        "{0}{1}{2}{3}処理ブロック:{4}{5}{6}",
                                        g_clsMessageInfo.strMsgE0002,
                                        Environment.NewLine,
                                        strLogMessage,
                                        Environment.NewLine,
                                        "検査情報ヘッダレコード取込",
                                        Environment.NewLine,
                                        ex.Message));

                                // メッセージ出力
                                MessageBox.Show(g_clsMessageInfo.strMsgE0035, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                continue;
                            }

                            strRapidTableName = "rapid_" + strFabricName + "_" + intInspectionNum + "_" + strInspectionDate.Replace("/", string.Empty);

                            try
                            {
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
                                FROM
                                (
                                    SELECT
                                        ROW_NUMBER() OVER(PARTITION BY marking_image ORDER BY ABS(:decCoordinateVariable - coordinate_sum), coordinate_sum) AS SEQ
                                        , *
                                    FROM 
                                    (
                                        SELECT
                                            CAST((regexp_split_to_array(ng_point, ','))[1] AS INTEGER) + CAST((regexp_split_to_array(ng_point, ','))[2] AS INTEGER) AS coordinate_sum
                                            , *
                                        FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".""" + strRapidTableName + @""" 
                                        WHERE fabric_name = :fabric_name
                                        AND inspection_num = :inspection_num 
                                        AND unit_num = :unit_num 
                                        AND rapid_result = :rapid_result
                                        AND edge_result = :edge_result
                                        AND masking_result = :masking_result
                                    ) AS TempTable
                                ) rpd
                                WHERE SEQ = 1
                                ON CONFLICT
                                DO NOTHING ";

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
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });
                                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "decCoordinateVariable", DbType = DbType.Decimal, Value = g_clsSystemSettingInfo.decCoordinateVariable });

                                // SQLを実行する
                                intExecutionCount = g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
                            }
                            catch (PostgresException pgex)
                            {
                                g_clsConnectionNpgsql.DbRollback();

                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    string.Format(
                                        "{0}{1}検査日付:{2}, {3}号機, 検査番号:{4}, 品名:{5}, 反番:{6}, 移送元テーブル:{7}, 移送先テーブル:{8}, 処理ブロック:{9}{10}{11}",
                                        g_clsMessageInfo.strMsgE0002,
                                        Environment.NewLine,
                                        strInspectionDate,
                                        strUnitNum,
                                        intInspectionNum,
                                        strProductName,
                                        strFabricName,
                                        strRapidTableName,
                                        "decision_result",
                                        "RapidテーブルNGレコード取得、合否判定結果レコード移送",
                                        Environment.NewLine,
                                        pgex.Message));

                                // メッセージ出力
                                MessageBox.Show(g_clsMessageInfo.strMsgE0039, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                continue;
                            }
                            catch (Exception ex)
                            {
                                g_clsConnectionNpgsql.DbRollback();

                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    string.Format(
                                        "{0}{1}{2}{3}処理ブロック:{4}{5}{6}",
                                        g_clsMessageInfo.strMsgE0002,
                                        Environment.NewLine,
                                        strLogMessage,
                                        Environment.NewLine,
                                        "RapidテーブルNGレコード取得、合否判定結果レコード移送",
                                        Environment.NewLine,
                                        ex.Message));

                                // メッセージ出力
                                MessageBox.Show(g_clsMessageInfo.strMsgE0039, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                continue;
                            }

                            strFaultImageFileDirectory = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, strUnitNum, strFaultImageFileName);

                            // 非同期処理実行のため、必要な情報を別の変数に退避させる
                            int intInspectionNumInfo = intInspectionNum;
                            string strFabricNameInfo = strFabricName;
                            string strInspectionDateInfo = strInspectionDate;
                            string strUnitNumInfo = strUnitNum;
                            string strProductNameInfo = strProductName;
                            string strFaultImageFileNameInfo = strFaultImageFileName;
                            string strLogMessageInfo =
                                string.Format(
                                    g_CON_LOG_MESSAGE_FOMAT,
                                        strUnitNumInfo,
                                        strInspectionDateInfo,
                                        intInspectionNumInfo,
                                        strProductNameInfo,
                                        strFabricNameInfo);

                            // 直近で行われた検査情報の欠点画像を取得する
                            if (!Directory.Exists(strFaultImageFileDirectory) &&
                                dateSyncTargetDate < DateTime.Parse(strInspectionDate))
                            {
                                bool? bolCheckResultInfo = BolCheckNGRecordCount(intInspectionNumInfo, strFabricNameInfo, strInspectionDateInfo, strUnitNumInfo, strLogMessageInfo, true);

                                if (bolCheckResultInfo == null)
                                {
                                    continue;
                                }

                                // NGレコードが存在する場合、欠点画像取込を行う
                                if (bolCheckResultInfo.Equals(true))
                                {
                                    lstTask.Add(Task<Boolean>.Run(() => BolGetFaultImage(
                                        intInspectionNumInfo,
                                        strInspectionDateInfo,
                                        strUnitNumInfo,
                                        strFabricNameInfo,
                                        strFaultImageFileNameInfo,
                                        strLogMessageInfo,
                                        lstTask.Count != 0 ? true : false)));
                                }
                                else
                                {
                                    // ログ出力
                                    WriteEventLog(
                                        g_CON_LEVEL_INFO,
                                        string.Format(
                                            "{0}{1}{2}{3}パス:{4}",
                                            "▼NGレコードが存在しません。完了通知取込をスキップし、空フォルダを作成します。",
                                            Environment.NewLine,
                                            strLogMessageInfo,
                                            Environment.NewLine,
                                            strFaultImageFileDirectory));

                                    // NGレコードが存在しない場合、フォルダ作成のみ実施する
                                    Directory.CreateDirectory(strFaultImageFileDirectory);
                                }
                            }

                            if (intExecutionCount == 0)
                            {
                                dtRapidDisabledData = new DataTable();
                                DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                                bool? RapidRecordExistFlg = BolCheckNGRecordCount(intInspectionNum, strFabricName, strInspectionDate, strUnitNum, strLogMessage, false);

                                if (RapidRecordExistFlg == null)
                                {
                                    continue;
                                }

                                if (RapidRecordExistFlg.Equals(true))
                                {
                                    try
                                    {
                                        // NGデータが存在しない場合、検査無効データをチェックする
                                        strSQL = @"SELECT fabric_name
                                           FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".""" + strRapidTableName + @""" rpd
                                           WHERE fabric_name = :fabric_name
                                           AND inspection_num = :inspection_num 
                                           AND unit_num = :unit_num 
                                           AND rapid_result = :rapid_result
                                           AND edge_result = :edge_result
                                           AND masking_result = :masking_result";

                                        // SQLコマンドに各パラメータを設定する
                                        lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "rapid_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intRapidResultDis });
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "edge_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intEdgeResultDis });
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "masking_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intMaskingResultDis });

                                        // SQLを実行する
                                        g_clsConnectionNpgsql.SelectSQL(ref dtRapidDisabledData, strSQL, lstNpgsqlCommand);
                                    }
                                    catch (PostgresException pgex)
                                    {
                                        g_clsConnectionNpgsql.DbRollback();

                                        // ログ出力
                                        WriteEventLog(
                                            g_CON_LEVEL_ERROR,
                                            string.Format(
                                                "{0}{1}検査日付:{2}, {3}号機, 検査番号:{4}, 品名:{5}, 反番:{6}, 取得対象テーブル:{7}, 処理ブロック:{8}{9}{10}",
                                                g_clsMessageInfo.strMsgE0001,
                                                Environment.NewLine,
                                                strInspectionDate,
                                                strUnitNum,
                                                intInspectionNum,
                                                strProductName,
                                                strFabricName,
                                                strRapidTableName,
                                                "Rapidテーブル無効レコード取得",
                                                Environment.NewLine,
                                                pgex.Message));

                                        // メッセージ出力
                                        MessageBox.Show(g_clsMessageInfo.strMsgE0039, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                        continue;
                                    }
                                    catch (Exception ex)
                                    {
                                        // ログ出力
                                        WriteEventLog(
                                            g_CON_LEVEL_ERROR,
                                            string.Format(
                                                "{0}{1}{2}{3}処理ブロック:{4}{5}{6}",
                                                g_clsMessageInfo.strMsgE0001,
                                                Environment.NewLine,
                                                strLogMessage,
                                                Environment.NewLine,
                                                "Rapidテーブル無効レコード取得",
                                                Environment.NewLine,
                                                ex.Message));

                                        // メッセージ出力
                                        MessageBox.Show(g_clsMessageInfo.strMsgE0039, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                        continue;
                                    }
                                }

                                try
                                {
                                    // SQL文を作成する
                                    strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                                                   SET over_detection_except_status = :over_detection_except_status
                                                   , acceptance_check_status = :acceptance_check_status
                                                   , decision_start_datetime = :current_timestamp
                                                   , decision_end_datetime = :current_timestamp
                                                   , result_datetime = :current_timestamp
                                               WHERE fabric_name = :fabric_name
                                                   AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date
                                                   AND inspection_num = :inspection_num
                                                   AND unit_num = :unit_num";

                                    // SQLコマンドに各パラメータを設定する
                                    lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = strInspectionDate });
                                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = intInspectionNum });
                                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "current_timestamp", DbType = DbType.DateTime2, Value = date });
                                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });

                                    if (RapidRecordExistFlg.Equals(true) &&
                                        dtRapidDisabledData.Rows.Count == 0)
                                    {
                                        // 検査無効データが存在しない場合、検査情報ヘッダを「検査有効(NGなし)」の状態に更新する
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_status", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptStatusEnd });
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_status", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd });
                                    }
                                    else
                                    {
                                        // Rapidレコード件数が0件の場合、または 検査無効データが存在する場合、検査情報ヘッダを「検査対象外」として更新する
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_status", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptStatusExc });
                                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_status", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckStatusExc });
                                    }

                                    // SQLを実行する
                                    g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
                                }
                                catch (PostgresException pgex)
                                {
                                    g_clsConnectionNpgsql.DbRollback();

                                    // ログ出力
                                    WriteEventLog(
                                        g_CON_LEVEL_ERROR,
                                        string.Format(
                                            "{0}{1}検査日付:{2}, {3}号機, 検査番号:{4}, 品名:{5}, 反番:{6}, 更新対象テーブル:{7}, 処理ブロック:{8}{9}{10}",
                                            g_clsMessageInfo.strMsgE0002,
                                            Environment.NewLine,
                                            strInspectionDate,
                                            strUnitNum,
                                            intInspectionNum,
                                            strProductName,
                                            strFabricName,
                                            g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header",
                                            "検査情報対象外更新",
                                            Environment.NewLine,
                                            pgex.Message));

                                    // メッセージ出力
                                    MessageBox.Show(g_clsMessageInfo.strMsgE0035, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                    continue;
                                }
                                catch (Exception ex)
                                {
                                    g_clsConnectionNpgsql.DbRollback();

                                    // ログ出力
                                    WriteEventLog(
                                        g_CON_LEVEL_ERROR,
                                        string.Format(
                                            "{0}{1}{2}{3}処理ブロック:{4}{5}{6}",
                                            g_clsMessageInfo.strMsgE0002,
                                            Environment.NewLine,
                                            strLogMessage,
                                            Environment.NewLine,
                                            "検査情報対象外更新",
                                            Environment.NewLine,
                                            ex.Message));

                                    // メッセージ出力
                                    MessageBox.Show(g_clsMessageInfo.strMsgE0035, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                    continue;
                                }
                            }

                            // DBコミット
                            g_clsConnectionNpgsql.DbCommit();
                        }
                    }
                }

                // 欠点画像の取込み待ち
                await Task.WhenAll(lstTask.ToArray());
                foreach (Task<Boolean> tsk in lstTask)
                {
                    if (!tsk.Result)
                    {
                        // メッセージ出力
                        MessageBox.Show(g_clsMessageInfo.strMsgE0041, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                        break;
                    }
                }

                // DBクローズ
                g_clsConnectionNpgsql.DbClose();
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
                g_bolGridRepresentationFlg = false;
                dtPublicHeaderData.Dispose();
                dtImagecheckerHeaderData.Dispose();
                dtRapidDisabledData.Dispose();
                frmProgress.Close();
                frmProgress.Dispose();
            }

            if (bolDisplayFlg)
            {
                // データグリッドビュー表示
                bolDispDataGridView(true);
            }

            this.ResumeLayout();
        }

        /// <summary>
        /// ステータスチェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool bolCheckWorkingStatus(
            int intInspectionNum,
            string strInspectionDate,
            string strFabricName,
            string strUnitNum,
            string strLogMessage)
        {
            DataTable dtData = new DataTable();
            string strSQL = string.Empty;

            try
            {
                // 存在チェックを行う
                strSQL = @"
                            SELECT
                                over_detection_except_status,
                                acceptance_check_status
                            FROM  " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                            WHERE fabric_name = :fabric_name
                                AND inspection_num = :inspection_num
                                AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                                AND unit_num = :unit_num ";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = strUnitNum });

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                if (dtData.Rows.Count > 0 &&
                    (int.Parse(dtData.Rows[0]["over_detection_except_status"].ToString()) == g_clsSystemSettingInfo.intOverDetectionExceptStatusChk ||
                    int.Parse(dtData.Rows[0]["acceptance_check_status"].ToString()) == g_clsSystemSettingInfo.intAcceptanceCheckStatusChk))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}{3}処理ブロック:{4}{5}{6}",
                        g_clsMessageInfo.strMsgE0001,
                        Environment.NewLine,
                        strLogMessage,
                        Environment.NewLine,
                        "検査情報ヘッダ_ステータス状態チェック",
                        Environment.NewLine,
                        ex.Message));

                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0031, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            finally
            {
                dtData.Dispose();
            }
        }

        /// <summary>
        /// 一覧スクロール
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvTargetSelection_Scroll(object sender, ScrollEventArgs e)
        {
            m_intFirstDisplayedScrollingRowIdx = dgvTargetSelection.FirstDisplayedScrollingRowIndex;
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
            if (!m_bolFormControlFlag)
            {
                return;
            }

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
                        new Font("メイリオ", 9.75F),
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