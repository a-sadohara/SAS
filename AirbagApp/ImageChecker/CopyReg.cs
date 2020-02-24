using ImageChecker.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class CopyReg : Form
    {
        /// <summary>
        /// 登録フラグ
        /// </summary>
        public bool bolRegister { get; set; }

        // パラメータ関連
        private string m_strProductName = string.Empty;         // 品名
        private string m_strFabricName = string.Empty;          // 反番
        private string m_strInspectionDate = string.Empty;      // 検査日付
        private int m_intInspectionNum = 0;                     // 検査番号
        private string m_strOrgImagepath = string.Empty;        // オリジナル画像ファイル名
        private string m_strMarkingImagepath = string.Empty;    // マーキング画像ファイル名
        private int m_intBranchNumGet = 0;                      // 枝番
        private int m_intBranchNumUpCnt = 0;                    // 枝番繰り上げ
        private int m_intFromApId = 0;                          // 遷移元画面ID
        private int m_intLine = -1;                             // 行
        private string m_strColumns = string.Empty;             // 列
        private string m_strNgReason = string.Empty;            // NG理由
        private ComboBox m_cmbBoxLine;                          // 行コンボボックス
        private ComboBox m_cmbBoxColumns;                       // 列コンボボックス
        private bool m_bolUpdMode = false;                      // 更新モード

        // 定数
        private const string m_CON_FORMAT_NG_FACE = "NG面：{0}";
        private const string m_CON_FORMAT_NG_DISTANCE = "位置(X,Y)cm：{0},{1}";
        private const string m_CON_FORMAT_NG_REASON_SELECT = "NG理由選択：{0}";
        private const string m_CON_FORMAT_NG_REASON = "NG理由：{0}";

        // 欠点画像サブディレクトリパス
        private string m_strFaultImageSubDirectory = string.Empty;

        // データ保持関連
        private DataTable m_dtData;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clsHeaderData">ヘッダ情報</param>
        /// <param name="cmbBoxLine">行コンボボックス</param>
        /// <param name="cmbBoxColumns">列コンボボックス</param>
        /// <param name="intLine">行</param>
        /// <param name="strColumns">列</param>
        /// <param name="strNgReason">NG理由</param>
        /// <param name="strMarkingImagepath">マーキング画像ファイル名</param>
        /// <param name="strOrgImagepath">オリジナル画像ファイル名</param>
        /// <param name="intBranchNum">枝番</param>
        /// <param name="intFromApId">遷移元画面ID</param>
        /// <param name="bolUpdMode">修正モード</param>
        public CopyReg(HeaderData clsHeaderData,
                       ComboBox cmbBoxLine,
                       ComboBox cmbBoxColumns,
                       int intLine,
                       string strColumns,
                       string strNgReason,
                       string strMarkingImagepath,
                       string strOrgImagepath,
                       int intBranchNum,
                       int intFromApId,
                       bool bolUpdMode)
        {
            m_strProductName = clsHeaderData.strProductName;
            m_strFabricName = clsHeaderData.strFabricName;
            m_strInspectionDate = clsHeaderData.strInspectionDate;
            m_intInspectionNum = clsHeaderData.intInspectionNum;

            m_intLine = intLine;
            m_strColumns = strColumns;
            m_strNgReason = strNgReason;
            m_strMarkingImagepath = strMarkingImagepath;
            m_strOrgImagepath = strOrgImagepath;
            m_intBranchNumGet = intBranchNum;
            m_intBranchNumUpCnt = 0;
            m_intFromApId = intFromApId;
            m_bolUpdMode = bolUpdMode;

            m_strFaultImageSubDirectory = string.Join("_", m_strInspectionDate.Replace("/", ""),
                                                           m_strProductName,
                                                           m_strFabricName,
                                                           m_intInspectionNum);

            m_cmbBoxLine = cmbBoxLine;
            m_cmbBoxColumns = cmbBoxColumns;

            bolRegister = false;

            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// 表示初期化
        /// </summary>
        private void dispInitialize()
        {
            try
            {
                // 行
                cmbBoxLine.SelectedItem = m_intLine.ToString();
                // 列
                cmbBoxColumns.SelectedItem = m_strColumns;
                // NG選択理由
                if (m_intBranchNumUpCnt > 0)
                {
                    lblNgReason.Text = string.Format(m_CON_FORMAT_NG_REASON_SELECT, string.Empty);
                }

                // 次の欠点を登録するの制御
                btnNextDefect.Enabled = false;

                // フォーカスの設定
                picMarkingImage.Focus();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 合否確認結果更新
        /// </summary>
        /// <param name="intResult">結果</param>
        /// <param name="strNgReason">NG理由</param>
        /// <param name="strDispResult">表示結果</param>
        private void UpdAcceptanceCheckResult(int intResult, string strNgReason, string strDispResult)
        {
            string strSQL = string.Empty;
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
            int intBranchNum = 0;
            string strNgFace = string.Empty;
            int intCopyRegistInfo = -1;
            string strDispResultMsg = "";

            // NG理由には「NG理由：」を付与する
            if (strDispResult == g_CON_NG_REASON_OK)
            {
                strDispResultMsg = strDispResult;
            }
            else
            {
                strDispResultMsg = string.Format(m_CON_FORMAT_NG_REASON, strDispResult);
            }

            // メッセージ表示
            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0011, strDispResultMsg), "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            try
            {
                intBranchNum = m_intBranchNumGet + m_intBranchNumUpCnt;
                strNgFace = m_dtData.Rows[0]["ng_face"].ToString();

                intCopyRegistInfo = intGetCopyRegistInfo();
                if (intCopyRegistInfo == -1)
                {
                    // エラー
                    return;
                }
                
                if (intCopyRegistInfo == 0)
                {
                    // 合否判定結果に複写情報が存在しない場合、複写する。
                    try
                    {
                        // SQL文を作成する
                        strSQL = @"INSERT INTO " + g_clsSystemSettingInfo.strInstanceName + @".decision_result(
                                   fabric_name
                                 , inspection_num
                                 , inspection_date
                                 , branch_num
                                 , line
                                 , cloumns
                                 , ng_face
                                 , ng_reason
                                 , input_datetime
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
                                 , inspection_date
                                 , :branch_num
                                 , line
                                 , cloumns
                                 , ng_face
                                 , ng_reason
                                 , input_datetime
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
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                               WHERE fabric_name = :fabric_name
                                 AND inspection_num = :inspection_num
                                 AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                                 AND branch_num = (:branch_num - 1)
                                 AND marking_imagepath = :marking_imagepath";

                        // SQLコマンドに各パラメータを設定する
                        lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = intBranchNum });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "marking_imagepath", DbType = DbType.String, Value = m_strMarkingImagepath });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNon });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNon });

                        // sqlを実行する
                        g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002 ,Environment.NewLine, ex.Message));
                        // メッセージ出力
                        MessageBox.Show(g_clsMessageInfo.strMsgE0050, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }
                }

                try
                {
                    // SQL文を作成する
                    lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                  SET ng_reason = :ng_reason
                                    , line = :line
                                    , cloumns = :cloumns
                                    , acceptance_check_result = :acceptance_check_result ";

                    if (m_intFromApId == 0 || m_intBranchNumUpCnt > 0)
                    {
                        // 新規で欠点を追加する場合
                        strSQL += @", acceptance_check_datetime = current_timestamp
                                    , acceptance_check_worker = :acceptance_check_worker
                                    , before_acceptance_check_result = acceptance_check_result
                                    , before_acceptance_check_upd_datetime = acceptance_check_datetime
                                    , before_acceptance_check_worker = acceptance_check_worker ";

                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_worker", DbType = DbType.String, Value = g_clsLoginInfo.strWorkerName });
                    }
                    else
                    {
                        // 結果更新
                        strSQL += @", before_ng_reason = ng_reason
                                    , result_update_datetime = current_timestamp
                                    , result_update_worker = :result_update_worker ";

                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "result_update_worker", DbType = DbType.String, Value = g_clsLoginInfo.strWorkerName });
                    }

                    strSQL += @"WHERE fabric_name = :fabric_name
                                  AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                                  AND inspection_num = :inspection_num
                                  AND branch_num = :branch_num
                                  AND ng_face = :ng_face
                                  AND marking_imagepath = :marking_imagepath";

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_reason", DbType = DbType.String, Value = strNgReason });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "line", DbType = DbType.Int16, Value = int.Parse(cmbBoxLine.SelectedItem.ToString()) });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "cloumns", DbType = DbType.String, Value = cmbBoxColumns.SelectedItem.ToString() });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result", DbType = DbType.Int16, Value = intResult });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_worker", DbType = DbType.String, Value = g_clsLoginInfo.strWorkerName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = intBranchNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_face", DbType = DbType.String, Value = strNgFace });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "marking_imagepath", DbType = DbType.String, Value = m_strMarkingImagepath });

                    // sqlを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);

                    if (m_intFromApId == 0)
                    {
                        g_clsConnectionNpgsql.DbCommit();
                    }

                    bolRegister = true;
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}" ,g_clsMessageInfo.strMsgE0002 , Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0043, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // NG選択理由
                lblNgReason.Text = string.Format(m_CON_FORMAT_NG_REASON_SELECT, strNgReason);

                // 次の欠点を登録するの制御
                btnNextDefect.Enabled = true;
                btnNextDefect.Focus();
            }
            finally
            {
                if (m_intFromApId == 0)
                {
                    g_clsConnectionNpgsql.DbClose();
                }
            }
        }

        /// <summary>
        /// 複写情報存在チェック
        /// </summary>
        /// <returns>0以上:件数 -1:異常終了</returns>
        private int intGetCopyRegistInfo()
        {
            string strSQL = string.Empty;
            DataTable dtData = new DataTable();
            int intCnt = 0;
            int intBranchNum = m_intBranchNumGet + m_intBranchNumUpCnt;

            try
            {
                // SQL文を作成する
                strSQL = @"SELECT COUNT(*) AS cnt
                             FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                            WHERE fabric_name = :fabric_name
                              AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                              AND inspection_num = :inspection_num
                              AND branch_num = :branch_num
                              AND marking_imagepath = :marking_imagepath";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = intBranchNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "marking_imagepath", DbType = DbType.String, Value = m_strMarkingImagepath });

                // sqlを実行する
                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                if (dtData.Rows.Count > 0)
                {
                    intCnt = int.Parse(dtData.Rows[0]["cnt"].ToString());
                }

                return intCnt;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}" ,g_clsMessageInfo.strMsgE0001 ,Environment.NewLine , ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0050, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return -1;
            }
        }

        /// <summary>
        /// NG区分判定取得
        /// </summary>
        /// <returns>検知と未検知で異なる</returns>
        private int intGetStatusNg()
        {
            return m_dtData.Rows[0]["over_detection_except_result"].ToString() ==
                   g_clsSystemSettingInfo.intOverDetectionExceptResultNon.ToString() ?
                   g_clsSystemSettingInfo.intAcceptanceCheckResultNgNonDetect :
                   g_clsSystemSettingInfo.intAcceptanceCheckResultNgDetect;
        }

        /// <summary>
        /// 必須入力チェック
        /// </summary>
        /// <param name="strItemName">項目名</param>
        /// <param name="strValue">値</param>
        /// <returns>true:OK false:NG</returns>
        private bool bolChkRequiredInput()
        {
            // 必須入力チェック
            if (bolChkRequiredInputByItem(cmbBoxLine, "行", cmbBoxLine.Text) == false ||
                bolChkRequiredInputByItem(cmbBoxColumns, "列", cmbBoxColumns.Text) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 必須入力チェック
        /// </summary>
        /// <param name="strItemName">項目名</param>
        /// <param name="strValue">値</param>
        /// <returns>true:OK false:NG</returns>
        private bool bolChkRequiredInputByItem(Control ctlItem, string strItemName, string strValue)
        {
            // 必須入力チェック
            if (string.IsNullOrEmpty(strValue) == true)
            {
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0011, strItemName), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ctlItem.Focus();
                return false;
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
        private void CopyReg_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            bool bolProcOkNg = false;

            string strSQL = string.Empty;

            try
            {
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
                                 , org_imagepath
                                 , marking_imagepath
                                 , over_detection_except_result
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                               WHERE fabric_name = :fabric_name
                               AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                               AND   inspection_num = :inspection_num
                               AND   branch_num = :branch_num
                               AND   marking_imagepath = :marking_imagepath
                               ORDER BY branch_num DESC";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = m_intBranchNumGet });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "marking_imagepath", DbType = DbType.String, Value = m_strMarkingImagepath });

                    g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001 ,Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // 画像イメージ表示
                FileStream fs = new FileStream(Path.Combine( g_clsSystemSettingInfo.strFaultImageDirectory 
                                               ,m_strFaultImageSubDirectory 
                                               ,m_strMarkingImagepath), FileMode.Open, FileAccess.Read);
                picMarkingImage.Image = Image.FromStream(fs);
                fs.Close();

                // その他情報を表示
                lblNgFace.Text = string.Format(m_CON_FORMAT_NG_FACE, m_dtData.Rows[0]["ng_face"].ToString());
                lblNgDistance.Text = string.Format(m_CON_FORMAT_NG_DISTANCE, m_dtData.Rows[0]["ng_distance_x"].ToString(), m_dtData.Rows[0]["ng_distance_y"].ToString());
                lblMarkingImagepath.Text = m_strMarkingImagepath;

                // コントロール初期化 
                // 行
                cmbBoxLine.Items.Clear();
                foreach (string value in m_cmbBoxLine.Items)
                {
                    cmbBoxLine.Items.Add(value);
                }
                // 高さを調整
                cmbBoxLine.DropDownHeight = this.Size.Height;
                // 列
                cmbBoxColumns.Items.Clear();
                foreach (string value in m_cmbBoxColumns.Items)
                {
                    cmbBoxColumns.Items.Add(value);
                }
                // NG選択理由
                lblNgReason.Text = string.Format(m_CON_FORMAT_NG_REASON_SELECT, m_strNgReason);

                dispInitialize();

                // 修正の場合、次の欠点を登録するを有効にする
                if (m_bolUpdMode == true)
                {
                    btnNextDefect.Enabled = true;
                }

                bolProcOkNg = true;
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
        /// フォーム表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyReg_Shown(object sender, EventArgs e)
        {
            // 初期表示時のフォーカス設定
            picMarkingImage.Focus();
        }

        /// <summary>
        /// 欠点画像クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picMarkingImage_Click(object sender, EventArgs e)
        {
            string strOrgImagepath = string.Empty;
            string strMarkingImagepath = string.Empty;

            // マーキング画像パスとオリジナル画像パスを取得
            strOrgImagepath = Path.Combine( g_clsSystemSettingInfo.strFaultImageDirectory 
                              ,m_strFaultImageSubDirectory 
                              ,m_strOrgImagepath);
            strMarkingImagepath = Path.Combine( g_clsSystemSettingInfo.strFaultImageDirectory 
                                  ,m_strFaultImageSubDirectory 
                                  ,m_strMarkingImagepath );

            // 画像拡大フォームに遷移
            ViewEnlargedimage frmViewImage = new ViewEnlargedimage(strOrgImagepath, strMarkingImagepath);
            frmViewImage.ShowDialog(this);
            this.Visible = true;
        }

        /// <summary>
        /// OKクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOKSelect_Click(object sender, EventArgs e)
        {
            // 必須入力チェック
            if (bolChkRequiredInput() == false)
            {
                return;
            }

            UpdAcceptanceCheckResult(g_clsSystemSettingInfo.intAcceptanceCheckResultOk,
                                     string.Empty,
                                     "過検知");
        }

        /// <summary>
        /// □結節有(白糸上単発)クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWhiteThreadOne_Click(object sender, EventArgs e)
        {
            // 必須入力チェック
            if (bolChkRequiredInput() == false)
            {
                return;
            }

            UpdAcceptanceCheckResult(intGetStatusNg(),
                                     g_CON_NG_REASON_WHITE_THREAD_ONE,
                                     g_CON_NG_REASON_WHITE_THREAD_ONE);
        }

        /// <summary>
        /// □結節有(白糸上連続)クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWhiteThreadMulti_Click(object sender, EventArgs e)
        {
            // 必須入力チェック
            if (bolChkRequiredInput() == false)
            {
                return;
            }

            UpdAcceptanceCheckResult(intGetStatusNg(),
                                     g_CON_NG_REASON_WHITE_THREAD_MULTI,
                                     g_CON_NG_REASON_WHITE_THREAD_MULTI);
        }

        /// <summary>
        /// □結節有(黒糸上単発)クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBlackThreadOne_Click(object sender, EventArgs e)
        {
            // 必須入力チェック
            if (bolChkRequiredInput() == false)
            {
                return;
            }

            UpdAcceptanceCheckResult(intGetStatusNg(),
                                     g_CON_NG_REASON_BLACK_THREAD_ONE,
                                     g_CON_NG_REASON_BLACK_THREAD_ONE);
        }

        /// <summary>
        /// □結節有(黒糸上連続)クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBlackThreadMulti_Click(object sender, EventArgs e)
        {
            // 必須入力チェック
            if (bolChkRequiredInput() == false)
            {
                return;
            }

            UpdAcceptanceCheckResult(intGetStatusNg(),
                                     g_CON_NG_REASON_BLACK_THREAD_MULTI,
                                     g_CON_NG_REASON_BLACK_THREAD_MULTI);
        }

        /// <summary>
        /// □他画像でＮＧ判定済みクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOtherNg_Click(object sender, EventArgs e)
        {
            // 必須入力チェック
            if (bolChkRequiredInput() == false)
            {
                return;
            }

            UpdAcceptanceCheckResult(intGetStatusNg(),
                                     g_CON_NG_REASON_OTHER_NG_JUDGEMENT,
                                     g_CON_NG_REASON_OTHER_NG_JUDGEMENT);
        }

        /// <summary>
        /// その他クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOther_Click(object sender, EventArgs e)
        {
            string strDecisionReason = string.Empty;

            SelectErrorReason frmErrorReason = new SelectErrorReason(false);
            frmErrorReason.ShowDialog(this);
            strDecisionReason = frmErrorReason.strDecisionReason;

            this.Visible = true;

            if (!string.IsNullOrEmpty(strDecisionReason))
            {
                UpdAcceptanceCheckResult(intGetStatusNg(),
                                         strDecisionReason,
                                         strDecisionReason);
            }
        }

        /// <summary>
        /// 次の欠点を登録するクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNextDefect_Click(object sender, EventArgs e)
        {
            btnNextDefect.Enabled = true;
            string strSQL = string.Empty;
            DataTable dtData = new DataTable();

            // 修正の場合、枝番を採番する。
            if (m_bolUpdMode == true && m_intBranchNumUpCnt == 0)
            {
                // 複写登録がある場合は子画面を表示する
                dtData = new DataTable();
                try
                {
                    strSQL = @"SELECT COALESCE(MAX(branch_num),0) AS branch_num_max
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                               WHERE fabric_name = :fabric_name
                               AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                               AND   inspection_num = :inspection_num
                               AND   marking_imagepath = :marking_imagepath";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "marking_imagepath", DbType = DbType.String, Value = m_strMarkingImagepath });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    if (dtData.Rows.Count > 0)
                    {
                        if (int.Parse(dtData.Rows[0]["branch_num_max"].ToString()) > 1)
                        {
                            m_intBranchNumGet = int.Parse(dtData.Rows[0]["branch_num_max"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }

            // 枝番のカウントアップ
            m_intBranchNumUpCnt++;

            dispInitialize();
        }

        /// <summary>
        /// 閉じるクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
