using ImageChecker.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class ResultCheck : Form
    {
        /// <summary>
        /// 遷移先
        /// </summary>
        public int intDestination { get; set; }

        // パラメータ関連（可変）
        private HeaderData m_clsHeaderData;                                 // ヘッダ情報
        private DecisionResult m_clsDecisionResultCorrection;               // 検査結果情報(修正)

        // パラメータ関連（不変）
        private readonly string  m_strUnitNum = string.Empty;               // 号機
        private readonly string m_strProductName = string.Empty;            // 品名
        private readonly string m_strOrderImg = string.Empty;               // 指図
        private readonly string m_strFabricName = string.Empty;             // 反番
        private readonly string m_strInspectionDate = string.Empty;         // 検査日付
        private readonly string m_strStartDatetime = string.Empty;          // 搬送開始日時
        private readonly string m_strEndDatetime = string.Empty;            // 搬送終了日時
        private readonly int m_intInspectionStartLine = -1;                 // 検査開始行
        private readonly int m_intInspectionEndLine = -1;                   // 最終行数
        private readonly string m_strDecisionStartTime = string.Empty;      // 判定開始日時
        private readonly string m_strInspectionDirection = string.Empty;    // 検査方向
        private readonly int m_intInspectionNum = 0;                        // 検査番号
        private readonly string m_strAirbagImagepath = string.Empty;        // エアバック画像ファイルパス
        private readonly int m_intColumnCnt = 0;                            // 列数
        private readonly int m_intFromApId = 0;                             // 遷移元画面ID

        // 合否判定ステータス関連
        private string m_strDecisionEndTime = string.Empty;                 // 判定終了日時
        private int m_intAcceptanceCheckStatus = 0;                         // 合否確認ステータス

        // ディレクトリ関連
        private readonly string m_strFaultImageSubDirName = string.Empty;   // 欠点画像サブディレクトリ名
        private readonly string m_strFaultImageSubDirPath = string.Empty;   // 欠点画像サブディレクトリパス
        
        // 検査方向背景色関連
        private readonly Color m_clrInspectionDirectionActFore = System.Drawing.SystemColors.ActiveCaption;
        private readonly Color m_clrInspectionDirectionActBack = SystemColors.Control;
        
        // ページIdx
        private int m_intPageIdx = -1;

        // データ保持関連
        private DataTable m_dtData;

        // 画像関連
        private Bitmap m_bmpMasterImageInit = null;
        private Bitmap m_bmpMasterImageMarking = null;

        // 選択行情報関連(結果画面用)
        private int m_intSelBranchNum = -1;
        private string m_strSelMarkingImagepath = string.Empty;

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
        private const string m_CON_FORMAT_COUNT_SCORES = "{0}/{1}";
        private const string m_CON_FORMAT_INSPECTION_DIRECTION_NO1 = "検反部No.1：{0}";
        private const string m_CON_FORMAT_INSPECTION_DIRECTION_NO2 = "検反部No.2：{0}";
        private const string m_CON_FORMAT_NG_FACE = "NG面：{0}";
        private const string m_CON_FORMAT_NG_DISTANCE = "位置(X,Y)cm：{0},{1}";
        private const string m_CON_FORMAT_NG_REASON_SELECT = "NG理由選択：{0}";
        private const string m_CON_FORMAT_NG_REASON = "NG理由：{0}";
        private const string m_CON_CAMERA_NO_2 = "2";
        private const string m_CON_CAMERA_NO_8 = "8";
        private const string m_CON_CAMERA_NO_14 = "14";
        private const string m_CON_CAMERA_NO_20 = "20";
        private const string m_CON_CAMERA_NO_26 = "26";
        private const int m_CON_PAGE_WAY_L = -1;
        private const int m_CON_PAGE_WAY_R = 1;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clsHeaderData">ヘッダ情報</param>
        /// <param name="clsDecisionResultCorrection">判定結果情報(修正用)</param>
        /// <param name="intFromApId">遷移元画面ID</param>
        public ResultCheck(ref HeaderData clsHeaderData, DecisionResult clsDecisionResultCorrection, int intFromApId = 0)
        {
            intDestination = 0;

            m_clsHeaderData = clsHeaderData;
            m_clsDecisionResultCorrection = clsDecisionResultCorrection;

            m_strUnitNum = clsHeaderData.strUnitNum;
            m_strProductName = clsHeaderData.strProductName;
            m_strOrderImg = clsHeaderData.strOrderImg;
            m_strFabricName = clsHeaderData.strFabricName;
            m_strInspectionDate = clsHeaderData.strInspectionDate;
            m_strStartDatetime = clsHeaderData.strStartDatetime;
            m_strEndDatetime = clsHeaderData.strEndDatetime;
            m_intInspectionStartLine = clsHeaderData.intInspectionStartLine;
            m_intInspectionEndLine = clsHeaderData.intInspectionEndLine;
            m_strDecisionStartTime = clsHeaderData.strDecisionStartDatetime;
            m_strDecisionEndTime = clsHeaderData.strDecisionEndDatetime;
            m_strInspectionDirection = clsHeaderData.strInspectionDirection;
            m_intInspectionNum = clsHeaderData.intInspectionNum;
            m_intAcceptanceCheckStatus = clsHeaderData.intAcceptanceCheckStatus;
            m_strAirbagImagepath = clsHeaderData.strAirbagImagepath;
            m_intColumnCnt = clsHeaderData.intColumnCnt;
            m_intFromApId = intFromApId;

            m_strFaultImageSubDirName = string.Join("_", m_strInspectionDate.Replace("/", string.Empty),
                                                           m_strProductName,
                                                           m_strFabricName,
                                                           m_intInspectionNum);

            m_strFaultImageSubDirPath = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory,
                                                     m_strFaultImageSubDirName);

            InitializeComponent();
        }

        /// <summary>
        /// 画像ファイル名取得
        /// </summary>
        /// <returns>true:正常終了 false:異常終了</returns>
        private bool bolGetImagePath()
        {
            string strSQL = string.Empty;

            try
            {
                m_dtData = new DataTable();
                strSQL = @"SELECT
                               branch_num
                             , line
                             , cloumns
                             , ng_face
                             , ng_reason
                             , master_point
                             , ng_distance_x
                             , ng_distance_y
                             , camera_num
                             , org_imagepath
                             , marking_imagepath
                             , over_detection_except_result
                             , acceptance_check_result
                           FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                           AND   inspection_num = :inspection_num
                           AND   branch_num = 1
                           AND   over_detection_except_result <> :over_detection_except_result_ok
                           ORDER BY CASE
                                      WHEN acceptance_check_datetime IS NULL THEN 2
                                      ELSE 1
                                    END ASC, ";
                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS)
                {
                    strSQL += "line ASC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                }

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
                {
                    strSQL += "line ASC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                }

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY)
                {
                    strSQL += "line DESC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                }

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
                {
                    strSQL += "line DESC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                }

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });

                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// 画像表示
        /// </summary>
        /// <param name="intPageIdx"></param>
        /// <param name="intPageWay">遷移方向(-1:左 1:右)</param>
        private bool bolDispImageInfo(int intPageIdx, int intPageWay)
        {
            string strMarkingImagepath = string.Empty;
            string strImagePath = string.Empty;
            string strCloumns = string.Empty;
            string[] strMasterPoint;
            int intMasterPointX = -1;
            int intMasterPointY = -1;
            string strNgFace = string.Empty;
            int intNgFace = 0;
            Graphics gra;
            Pen pen;
            string strMarkingFilePath;
            bool bolExistsImageFile = true;
            string strSQL = string.Empty;
            DataTable dtData;
            bool bolMovecoercive = false;
            string strMsg = string.Empty;

            try
            {
                // 画像パス
                strMarkingImagepath = m_dtData.Rows[intPageIdx]["marking_imagepath"].ToString();
                strImagePath = strGetMarkingImagePathForPage(intPageIdx);

                // マスタ位置
                strMasterPoint = m_dtData.Rows[intPageIdx]["master_point"].ToString().Split(',');
                intMasterPointX = Convert.ToInt32(strMasterPoint[0]);
                intMasterPointY = Convert.ToInt32(strMasterPoint[1]);

                // NG面
                strNgFace = m_dtData.Rows[intPageIdx]["ng_face"].ToString();
                if (strNgFace.IndexOf("1") > 0)
                {
                    intNgFace = 1;  // 表
                }
                if (strNgFace.IndexOf("2") > 0)
                {
                    intNgFace = 2;  // 裏
                }

                // 列
                strCloumns = m_dtData.Rows[intPageIdx]["cloumns"].ToString();

                // 欠点画像にフォーカスセット
                picMarkingImage.Focus();
                
                // ページ数表示
                lblPageCount.Text = string.Format(m_CON_FORMAT_COUNT_SCORES, (intPageIdx + 1).ToString(), m_dtData.Rows.Count.ToString());

                // 画像イメージ表示
                FileStream fs;
                if (File.Exists(strImagePath) == false)
                {
                    fs = new FileStream(g_CON_NO_IMAGE_FILE_PATH, FileMode.Open, FileAccess.Read);
                    bolExistsImageFile = false;
                }
                else
                {
                    fs = new FileStream(strImagePath, FileMode.Open, FileAccess.Read);
                }
                picMarkingImage.Image = System.Drawing.Image.FromStream(fs);
                fs.Close();

                // マスタ画像にNG位置をマーキングした画像を一時ファイル保存する
                strMarkingFilePath = Path.Combine(g_strMasterImageDirMarking, intPageIdx.ToString() + ".bmp");
                m_bmpMasterImageMarking = new Bitmap(m_bmpMasterImageInit);
                gra = Graphics.FromImage(m_bmpMasterImageMarking);

                // NG面 表:青 裏:赤
                pen = new Pen(Color.Black, 2);
                if (intNgFace == 1)
                {
                    pen.Color = Color.Blue;
                }
                if (intNgFace == 2)
                {
                    pen.Color = Color.Red;
                }

                // 「Ｘ」を描画
                gra.DrawLine(pen, intMasterPointX - 8, intMasterPointY - 8, intMasterPointX + 8, intMasterPointY + 8);
                gra.DrawLine(pen, intMasterPointX + 8, intMasterPointY - 8, intMasterPointX - 8, intMasterPointY + 8);

                // 既に画像ファイルがある場合は削除
                if (File.Exists(strMarkingFilePath))
                {
                    File.Delete(strMarkingFilePath);
                }

                // 保存
                m_bmpMasterImageMarking.Save(strMarkingFilePath, System.Drawing.Imaging.ImageFormat.Bmp);
                gra.Dispose();

                // 検査方向によって画像を回転させて表示する。
                SetInspectionDirectionSetting(m_strInspectionDirection);

                // その他情報を表示
                lblNgFace.Text = string.Format(m_CON_FORMAT_NG_FACE, strNgFace);
                lblNgDistance.Text = string.Format(m_CON_FORMAT_NG_DISTANCE, m_dtData.Rows[intPageIdx]["ng_distance_x"].ToString(), m_dtData.Rows[intPageIdx]["ng_distance_y"].ToString());
                lblMarkingImagepath.Text = strMarkingImagepath;
                lblNgReason.Text = string.Format(m_CON_FORMAT_NG_REASON_SELECT, m_dtData.Rows[intPageIdx]["ng_reason"].ToString());
                cmbBoxLine.SelectedItem = m_dtData.Rows[intPageIdx]["line"].ToString();
                cmbBoxColumns.SelectedItem = strCloumns;

                // ひとつ前へ戻るの制御
                if (m_intPageIdx == 0)
                {
                    btnBackPage.Enabled = false;
                }
                else
                {
                    btnBackPage.Enabled = true;
                }

                // 画像イメージが存在しない場合はメッセージを表示し、スキップする
                if (bolExistsImageFile == false)
                {
                    switch (intPageWay)
                    {
                        case m_CON_PAGE_WAY_R:
                            strMsg = g_clsMessageInfo.strMsgW0007;
                            break;
                        case m_CON_PAGE_WAY_L:
                            strMsg = g_clsMessageInfo.strMsgW0008;
                            break;
                    }

                    // メッセージ出力
                    MessageBox.Show(strMsg, g_CON_MESSAGE_TITLE_WARN, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    bolMovecoercive = true;
                    return true;
                }

                // 複写登録がある場合は子画面を表示する
                dtData = new DataTable();
                try
                {
                    strSQL = @"SELECT COALESCE(MAX(branch_num),0) AS branch_num_max
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                               WHERE fabric_name = :fabric_name
                               AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                               AND   inspection_num = :inspection_num
                               AND   marking_imagepath = :marking_imagepath
                               AND   acceptance_check_result IN(:acceptance_check_result_ng_detect, 
                                                                :acceptance_check_result_ng_nondetect)";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "marking_imagepath", DbType = DbType.String, Value = strMarkingImagepath });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ng_detect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNgDetect });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ng_nondetect", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNgNonDetect });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    if (dtData.Rows.Count > 0)
                    {
                        if (Convert.ToInt32(dtData.Rows[0]["branch_num_max"]) > 1)
                        {
                            dispCopyRegist();
                        }
                    }

                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0060, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return false;
                }

                // 同一行列がNGに登録済みになっている場合、他画面でNG登録済みにする
                dtData = new DataTable();
                try
                {
                    strSQL = @"SELECT COUNT(*) AS cnt
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                               WHERE fabric_name = :fabric_name
                               AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                               AND   inspection_num = :inspection_num
                               AND   line = :line
                               AND   cloumns = :cloumns
                               AND   acceptance_check_result IN(:acceptance_check_result_ng_detect, 
                                                                :acceptance_check_result_ng_nondetect)";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "line", DbType = DbType.Int16, Value = Convert.ToInt32(m_dtData.Rows[intPageIdx]["line"]) });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "cloumns", DbType = DbType.String, Value = strCloumns });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ng_detect", 
                                                                                DbType = DbType.Int16,
                                                                                Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNgDetect });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_ng_nondetect",
                                                                                DbType = DbType.Int16,
                                                                                Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNgNonDetect });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    if (dtData.Rows.Count > 0)
                    {
                        // 他画像でＮＧ判定済みの表示
                        if (Convert.ToInt32(dtData.Rows[0]["cnt"]) > 0)
                        {
                            lblNgReason.Text = string.Format(m_CON_FORMAT_NG_REASON_SELECT, g_CON_NG_REASON_OTHER_NG_JUDGEMENT);
                            btnOtherNgJudgement.Focus();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0060, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0021, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            finally
            {
                // 強制移動
                if (bolMovecoercive == true)
                {
                    bolNextPage();
                }
            }
        }

        /// <summary>
        /// 検査方向設定ボタン(画面)をセット
        /// </summary>
        /// <param name="strInspectionDirection">検査方向（S/X/Y/R）</param>
        private void SetInspectionDirectionSetting(string strInspectionDirection)
        {
            Bitmap bmpImageNo1 = new Bitmap(m_bmpMasterImageMarking);
            Bitmap bmpImageNo2 = new Bitmap(m_bmpMasterImageMarking);

            // 非アクティブ化
            foreach (Button btnInspectionDirection in new Button[] { btnInspectionDirectionS, btnInspectionDirectionX, btnInspectionDirectionY, btnInspectionDirectionR })
            {
                if ((strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS && btnInspectionDirection != btnInspectionDirectionS) ||
                    (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX && btnInspectionDirection != btnInspectionDirectionX) ||
                    (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY && btnInspectionDirection != btnInspectionDirectionY) ||
                    (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR && btnInspectionDirection != btnInspectionDirectionR))
                    btnInspectionDirection.BackColor = m_clrInspectionDirectionActBack;
            }

            // アクティブ化
            if (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS)
            {
                // 背景色(アクティブ)
                btnInspectionDirectionS.BackColor = m_clrInspectionDirectionActFore;

                // 検反部ラベル
                lblInspectionDirectionNo1.Text = string.Format(m_CON_FORMAT_INSPECTION_DIRECTION_NO1, g_clsSystemSettingInfo.strInspectionDirectionS);
                lblInspectionDirectionNo2.Text = string.Format(m_CON_FORMAT_INSPECTION_DIRECTION_NO2, g_clsSystemSettingInfo.strInspectionDirectionX);

                // 回転
                bmpImageNo1.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bmpImageNo2.RotateFlip(RotateFlipType.Rotate270FlipX);

                // 画像表示
                picMasterImageNo1.Image = bmpImageNo1;
                picMasterImageNo2.Image = bmpImageNo2;

                // カメラ位置
                lblCameraNo11.Text = m_CON_CAMERA_NO_2;
                lblCameraNo12.Text = m_CON_CAMERA_NO_8;
                lblCameraNo13.Text = m_CON_CAMERA_NO_14;
                lblCameraNo14.Text = m_CON_CAMERA_NO_20;
                lblCameraNo15.Text = m_CON_CAMERA_NO_26;
                lblCameraNo21.Text = m_CON_CAMERA_NO_26;
                lblCameraNo22.Text = m_CON_CAMERA_NO_20;
                lblCameraNo23.Text = m_CON_CAMERA_NO_14;
                lblCameraNo24.Text = m_CON_CAMERA_NO_8;
                lblCameraNo25.Text = m_CON_CAMERA_NO_2;
            }

            if (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
            {
                // 背景色(アクティブ)
                btnInspectionDirectionX.BackColor = m_clrInspectionDirectionActFore;

                // 検反部ラベル
                lblInspectionDirectionNo1.Text = string.Format(m_CON_FORMAT_INSPECTION_DIRECTION_NO1, g_clsSystemSettingInfo.strInspectionDirectionX);
                lblInspectionDirectionNo2.Text = string.Format(m_CON_FORMAT_INSPECTION_DIRECTION_NO2, g_clsSystemSettingInfo.strInspectionDirectionS);

                // 回転
                bmpImageNo1.RotateFlip(RotateFlipType.Rotate270FlipX);
                bmpImageNo2.RotateFlip(RotateFlipType.Rotate270FlipNone);

                // 画像表示
                picMasterImageNo1.Image = bmpImageNo1;
                picMasterImageNo2.Image = bmpImageNo2;

                // カメラ位置
                lblCameraNo11.Text = m_CON_CAMERA_NO_26;
                lblCameraNo12.Text = m_CON_CAMERA_NO_20;
                lblCameraNo13.Text = m_CON_CAMERA_NO_14;
                lblCameraNo14.Text = m_CON_CAMERA_NO_8;
                lblCameraNo15.Text = m_CON_CAMERA_NO_2;
                lblCameraNo21.Text = m_CON_CAMERA_NO_2;
                lblCameraNo22.Text = m_CON_CAMERA_NO_8;
                lblCameraNo23.Text = m_CON_CAMERA_NO_14;
                lblCameraNo24.Text = m_CON_CAMERA_NO_20;
                lblCameraNo25.Text = m_CON_CAMERA_NO_26;
            }

            if (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY)
            {
                // 背景色(アクティブ)
                btnInspectionDirectionY.BackColor = m_clrInspectionDirectionActFore;

                // 検反部ラベル
                lblInspectionDirectionNo1.Text = string.Format(m_CON_FORMAT_INSPECTION_DIRECTION_NO1, g_clsSystemSettingInfo.strInspectionDirectionY);
                lblInspectionDirectionNo2.Text = string.Format(m_CON_FORMAT_INSPECTION_DIRECTION_NO2, g_clsSystemSettingInfo.strInspectionDirectionR);

                // 回転
                bmpImageNo1.RotateFlip(RotateFlipType.Rotate270FlipY);
                bmpImageNo2.RotateFlip(RotateFlipType.Rotate270FlipXY);

                // 画像表示
                picMasterImageNo1.Image = bmpImageNo1;
                picMasterImageNo2.Image = bmpImageNo2;

                // カメラ位置
                lblCameraNo11.Text = m_CON_CAMERA_NO_2;
                lblCameraNo12.Text = m_CON_CAMERA_NO_8;
                lblCameraNo13.Text = m_CON_CAMERA_NO_14;
                lblCameraNo14.Text = m_CON_CAMERA_NO_20;
                lblCameraNo15.Text = m_CON_CAMERA_NO_26;
                lblCameraNo21.Text = m_CON_CAMERA_NO_26;
                lblCameraNo22.Text = m_CON_CAMERA_NO_20;
                lblCameraNo23.Text = m_CON_CAMERA_NO_14;
                lblCameraNo24.Text = m_CON_CAMERA_NO_8;
                lblCameraNo25.Text = m_CON_CAMERA_NO_2;
            }

            if (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
            {
                // 背景色(アクティブ)
                btnInspectionDirectionR.BackColor = m_clrInspectionDirectionActFore;

                // 検反部ラベル
                lblInspectionDirectionNo1.Text = string.Format(m_CON_FORMAT_INSPECTION_DIRECTION_NO1, g_clsSystemSettingInfo.strInspectionDirectionR);
                lblInspectionDirectionNo2.Text = string.Format(m_CON_FORMAT_INSPECTION_DIRECTION_NO2, g_clsSystemSettingInfo.strInspectionDirectionY);

                // 回転
                bmpImageNo1.RotateFlip(RotateFlipType.Rotate270FlipXY);
                bmpImageNo2.RotateFlip(RotateFlipType.Rotate270FlipY);

                // 画像表示
                picMasterImageNo1.Image = bmpImageNo1;
                picMasterImageNo2.Image = bmpImageNo2;

                // カメラ位置
                lblCameraNo11.Text = m_CON_CAMERA_NO_26;
                lblCameraNo12.Text = m_CON_CAMERA_NO_20;
                lblCameraNo13.Text = m_CON_CAMERA_NO_14;
                lblCameraNo14.Text = m_CON_CAMERA_NO_8;
                lblCameraNo15.Text = m_CON_CAMERA_NO_2;
                lblCameraNo21.Text = m_CON_CAMERA_NO_2;
                lblCameraNo22.Text = m_CON_CAMERA_NO_8;
                lblCameraNo23.Text = m_CON_CAMERA_NO_14;
                lblCameraNo24.Text = m_CON_CAMERA_NO_20;
                lblCameraNo25.Text = m_CON_CAMERA_NO_26;
            }
        }

        /// <summary>
        /// 過検知除外ステータス更新
        /// </summary>
        /// <param name="strFabricName">反番</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="intStatus">ステータス</param>
        /// <returns></returns>
        private Boolean blnUpdAcceptanceCheckStatus(string strFabricName,
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
                              SET acceptance_check_status = :acceptance_check_status
                            WHERE fabric_name = :fabric_name
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
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));
                // メッセージ出力
                System.Windows.Forms.MessageBox.Show(g_clsMessageInfo.strMsgE0035, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
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
            int intBranchNum = 0;
            string strNgFace = string.Empty;
            string strMarkingImagepath = string.Empty;
            string strDbConKey = string.Empty;
            string strDispResultMsg = string.Empty;

            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

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
            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0011, strDispResultMsg), g_CON_MESSAGE_TITLE_QUESTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            try
            {
                intBranchNum = Convert.ToInt32(m_dtData.Rows[m_intPageIdx]["branch_num"]);
                strNgFace = m_dtData.Rows[m_intPageIdx]["ng_face"].ToString();
                strMarkingImagepath = m_dtData.Rows[m_intPageIdx]["marking_imagepath"].ToString();

                strDbConKey = string.Join("|", m_dtData.Rows[m_intPageIdx]["branch_num"].ToString(),
                                               strNgFace,
                                               strMarkingImagepath);

                // SQL文を作成する
                strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                              SET ng_reason = :ng_reason
                                , line = :line
                                , cloumns = :cloumns
                                , acceptance_check_result = :acceptance_check_result ";

                if (m_intFromApId == 0 ||
                    Convert.ToInt32(m_dtData.Rows[m_intPageIdx]["acceptance_check_result"]) == g_clsSystemSettingInfo.intAcceptanceCheckResultNon)
                {
                    // 新規登録　もしくは　未検知画像追加分
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

                // NG理由
                if (string.IsNullOrEmpty(strNgReason))
                {
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_reason", DbType = DbType.String, Value = DBNull.Value });
                }
                else
                {
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_reason", DbType = DbType.String, Value = strNgReason });
                }

                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "line", DbType = DbType.Int16, Value = Convert.ToInt32(cmbBoxLine.SelectedItem) });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "cloumns", DbType = DbType.String, Value = cmbBoxColumns.SelectedItem.ToString() });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result", DbType = DbType.Int16, Value = intResult });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = intBranchNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_face", DbType = DbType.String, Value = strNgFace });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "marking_imagepath", DbType = DbType.String, Value = strMarkingImagepath });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);

                // データテーブルの更新
                m_dtData.Rows[m_intPageIdx]["ng_reason"] = strNgReason;
                m_dtData.Rows[m_intPageIdx]["line"] = Convert.ToInt32(cmbBoxLine.SelectedItem);
                m_dtData.Rows[m_intPageIdx]["cloumns"] = cmbBoxColumns.SelectedItem.ToString();

                if (m_intFromApId == 0)
                {
                    g_clsConnectionNpgsql.DbCommit();
                }
            }
            catch (Exception ex)
            {
                g_clsConnectionNpgsql.DbRollback();

                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0043, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            finally
            {
                if (m_intFromApId == 0)
                {
                    g_clsConnectionNpgsql.DbClose();
                }
            }

            // 次ページ
            bolNextPage();
        }

        /// <summary>
        /// NG区分判定取得
        /// </summary>
        /// <returns>検知と未検知で異なる</returns>
        private int intGetStatusNg()
        {
            return m_dtData.Rows[m_intPageIdx]["over_detection_except_result"].ToString() ==
                   g_clsSystemSettingInfo.intOverDetectionExceptResultNon.ToString() ?
                   g_clsSystemSettingInfo.intAcceptanceCheckResultNgNonDetect :
                   g_clsSystemSettingInfo.intAcceptanceCheckResultNgDetect;
        }

        /// <summary>
        /// 次ページ
        /// </summary>
        /// <returns>true:以降も表示する false:以降は表示しない</returns>
        private bool bolNextPage()
        {

            if (m_intPageIdx + 1 < m_dtData.Rows.Count && m_clsDecisionResultCorrection.intBranchNum <= 0)
            {
                // ページカウントアップ
                m_intPageIdx++;
                // 画像表示
                bolDispImageInfo(m_intPageIdx, m_CON_PAGE_WAY_R);

                return true;
            }

            // 結果確認画面に遷移
            this.Visible = false;
            Result frmResult = new Result(ref m_clsHeaderData, m_intFromApId, m_intSelBranchNum, m_strSelMarkingImagepath);
            frmResult.ShowDialog(this);

            // パラメータ更新
            m_intAcceptanceCheckStatus = m_clsHeaderData.intAcceptanceCheckStatus;
            m_strDecisionEndTime = m_clsHeaderData.strDecisionEndDatetime;

            if (frmResult.bolMod == true)
            {
                // 修正ありの場合

                m_clsDecisionResultCorrection = frmResult.clsDecisionResult;

                m_intSelBranchNum = m_clsDecisionResultCorrection.intBranchNum;
                m_strSelMarkingImagepath = m_clsDecisionResultCorrection.strMarkingImagepath;

                // ページIdxを検索
                for (int idx = 0; idx < m_dtData.Rows.Count; idx++)
                {
                    if (m_dtData.Rows[idx]["marking_imagepath"].ToString() == m_clsDecisionResultCorrection.strMarkingImagepath)
                    {
                        m_intPageIdx = idx;
                        break;
                    }
                }
            }
            else
            {
                // 修正なしの場合

                // 遷移先：検査対象選択画面
                if (frmResult.bolReg == false)
                {
                    intDestination = g_CON_APID_TARGET_SELECTION;
                }

                // 画面を閉じる
                this.Close();
                return false;
            }

            this.Visible = true;  

            // 画像表示
            bolDispImageInfo(m_intPageIdx, m_CON_PAGE_WAY_R);

            return true;
        }

        /// <summary>
        /// 次ページ
        /// </summary>
        /// <returns>true:以降も表示する false:以降は表示しない</returns>
        private bool bolBackPage()
        {

            // ページカウントダウン
            m_intPageIdx--;

            // 画像表示
            if (bolDispImageInfo(m_intPageIdx, m_CON_PAGE_WAY_L) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 開始ページの取得
        /// </summary>
        /// <param name="intIdxStartPage">開始ページ</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private void GetStartPageIdx(ref int intIdxStartPage)
        {
            string strSQL = string.Empty;
            DataTable dtData;

            try
            {
                dtData = new DataTable();
                strSQL = @"SELECT COALESCE(MAX(sort_num),0) AS max_sort_num
                             FROM (
                                 SELECT ROW_NUMBER() OVER(ORDER BY CASE
                                                            WHEN acceptance_check_datetime IS NULL THEN 2
                                                            ELSE 1
                                                          END ASC, ";

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS)
                {
                    strSQL += "line ASC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                }

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
                {
                    strSQL += "line ASC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                }

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY)
                {
                    strSQL += "line DESC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                }

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
                {
                    strSQL += "line DESC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC";
                }

                strSQL += @" ) AS sort_num
                                   FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                  WHERE fabric_name = :fabric_name
                                    AND inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                    AND inspection_num = :inspection_num
                                    AND branch_num = 1
                                    AND over_detection_except_result <> :over_detection_except_result_ok
                                    AND acceptance_check_datetime IS NOT NULL
                             ) AS MAIN";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_ok", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultOk });

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                if (dtData.Rows.Count > 0)
                {
                    intIdxStartPage = Convert.ToInt32(dtData.Rows[0]["max_sort_num"]);
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}" ,g_clsMessageInfo.strMsgE0044 ,Environment.NewLine, ex.Message));
            }
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
        private bool bolChkRequiredInputByItem(Control ctlItem , string strItemName, string strValue)
        {
            // 必須入力チェック
            if (string.IsNullOrEmpty(strValue) == true)
            {
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0011, strItemName), g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ctlItem.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 指定ページに追加されているマーキング画像パスを取得
        /// </summary>
        /// <returns></returns>
        private string strGetMarkingImagePathForPage(int intPageIdx)
        {
            return Path.Combine(m_strFaultImageSubDirPath,
                                m_dtData.Rows[intPageIdx]["marking_imagepath"].ToString());
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultCheck_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            // ボタン名設定
            btnWhiteThreadOne.Text = g_CON_NG_REASON_WHITE_THREAD_ONE;
            btnWhiteThreadMulti.Text = g_CON_NG_REASON_WHITE_THREAD_MULTI;
            btnBlackThreadOne.Text = g_CON_NG_REASON_BLACK_THREAD_ONE;
            btnBlackThreadMulti.Text = g_CON_NG_REASON_BLACK_THREAD_MULTI;

            bool bolDispForm = false;

            List<string> lstCmbItem = new List<string>();
            int intMinLine = 0;
            int intMaxLine = 0;

            try
            {
                this.WindowState = FormWindowState.Maximized;

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

                // コンボボックスのFrom-To
                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS || m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
                {
                    intMinLine = m_intInspectionStartLine;
                    intMaxLine = m_intInspectionEndLine;
                }
                else if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY || m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
                {
                    intMinLine = m_intInspectionEndLine;
                    intMaxLine = m_intInspectionStartLine;
                }

                // コンボボックスの設定
                // 行
                cmbBoxLine.Items.Clear();
                for (int i = intMinLine; i <= intMaxLine; i++)
                {
                    lstCmbItem.Add(i.ToString());
                }
                cmbBoxLine.Items.AddRange(lstCmbItem.ToArray());
                // 行逆さを調整
                cmbBoxLine.DropDownHeight = this.Size.Height;

                // 列
                cmbBoxColumns.Items.Clear();
                for (int i = 1; i <= m_intColumnCnt; i++)
                {
                    if (i == 1) cmbBoxColumns.Items.Add("A");
                    if (i == 2) cmbBoxColumns.Items.Add("B");
                    if (i == 3) cmbBoxColumns.Items.Add("C");
                    if (i == 4) cmbBoxColumns.Items.Add("D");
                    if (i == 5) cmbBoxColumns.Items.Add("E");
                }

                // 画像パス一覧を取得
                if (bolGetImagePath() == false)
                {
                    return;
                }

                // マスタ画像取り込み
                try
                {
                    // 初期状態を描画
                    FileStream fs = new FileStream(m_strAirbagImagepath, FileMode.Open, FileAccess.Read);
                    m_bmpMasterImageInit = new Bitmap(Image.FromStream(fs));
                    fs.Close();

                    // 枠線表示用に調整
                    picMasterImageNo1.Controls.Add(new System.Windows.Forms.PictureBox());
                    picMasterImageNo2.Controls.Add(new System.Windows.Forms.PictureBox());
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001 ,Environment.NewLine , ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0021, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // ページIdx設定
                if (m_clsDecisionResultCorrection.intBranchNum > 0)
                {
                    // 判定結果情報が譲渡された（履歴系の画面から遷移した）場合

                    // ページIdxを検索
                    for (int idx = 0; idx < m_dtData.Rows.Count; idx++)
                    {
                        if (m_dtData.Rows[idx]["marking_imagepath"].ToString() == m_clsDecisionResultCorrection.strMarkingImagepath)
                        {
                            m_intPageIdx = idx;
                            break;
                        }
                    }
                }
                else
                {
                    // 新規登録の場合

                    m_intPageIdx = 0;

                    if (m_intAcceptanceCheckStatus == g_clsSystemSettingInfo.intAcceptanceCheckStatusStp)
                    {
                        // 合否確認ステータス：中断であれば、途中からのページIdxを探す
                        GetStartPageIdx(ref m_intPageIdx);
                    }
                }

                if (m_intAcceptanceCheckStatus == g_clsSystemSettingInfo.intAcceptanceCheckStatusBef ||
                    m_intAcceptanceCheckStatus == g_clsSystemSettingInfo.intAcceptanceCheckStatusStp)
                {
                    // 合否確認ステータス更新(検査中)
                    if (blnUpdAcceptanceCheckStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                    g_clsSystemSettingInfo.intAcceptanceCheckStatusChk) == false)
                    {
                        // エラー時
                        g_clsConnectionNpgsql.DbRollback();
                        g_clsConnectionNpgsql.DbClose();

                        return;
                    }

                    g_clsConnectionNpgsql.DbCommit();
                    g_clsConnectionNpgsql.DbClose();

                    // パラメータ更新
                    m_clsHeaderData.intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusChk;
                    m_intAcceptanceCheckStatus = m_clsHeaderData.intAcceptanceCheckStatus;
                }

                if (m_intPageIdx <= m_dtData.Rows.Count - 1)
                {
                	// 全ての合否確認が済んでいなければ画面を表示
                    if (bolDispImageInfo(m_intPageIdx, m_CON_PAGE_WAY_R) == false)
                    {
                        return;
                    }
                }
                else
                {
                    // 全ての合否確認が済んでいれば判定登録画面に遷移
                    if (bolNextPage() == false)
                    {
                        return;
                    }
                }

                // 欠点画像にフォーカスセット
                picMarkingImage.Focus();

                bolDispForm = true;
            }
            finally
            {
                if (bolDispForm == false)
                {
                    this.Close();
                }

                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ひとつ前へ戻るクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBackPage_Click(object sender, EventArgs e)
        {
            bolBackPage();
        }

        /// <summary>
        /// 検査方向Sクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspectionDirectionS_Click(object sender, EventArgs e)
        {
            SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionS);
        }

        /// <summary>
        /// 検査方向Xクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspectionDirectionX_Click(object sender, EventArgs e)
        {
            SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionX);
        }

        /// <summary>
        /// 検査方向Yクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspectionDirectionY_Click(object sender, EventArgs e)
        {
            SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionY);
        }

        /// <summary>
        /// 検査方向Rクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspectionDirectionR_Click(object sender, EventArgs e)
        {
            SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionR);
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
            strOrgImagepath = Path.Combine(m_strFaultImageSubDirPath,
                                           m_dtData.Rows[m_intPageIdx]["org_imagepath"].ToString());
            strMarkingImagepath = strGetMarkingImagePathForPage(m_intPageIdx);

            // 画像拡大フォームを開く
            ViewEnlargedimage frmViewImage = new ViewEnlargedimage(strOrgImagepath, strMarkingImagepath);
            frmViewImage.ShowDialog(this);
            this.Visible = true;
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
        /// フォームクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultCheck_FormClosing(object sender, FormClosingEventArgs e)
        {

            string strMasterDirPath = string.Empty;

            if (m_intAcceptanceCheckStatus != g_clsSystemSettingInfo.intAcceptanceCheckStatusEnd)
            {
                // 合否確認ステータス更新(中断)
                if (blnUpdAcceptanceCheckStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                g_clsSystemSettingInfo.intAcceptanceCheckStatusStp) == false)
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

                // パラメータ更新
                m_clsHeaderData.intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusStp;
                m_intAcceptanceCheckStatus = m_clsHeaderData.intAcceptanceCheckStatus;
            }
        }

        /// <summary>
        /// OKクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, EventArgs e)
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
        private void btnOtherNgJudgement_Click(object sender, EventArgs e)
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
            // 必須入力チェック
            if (bolChkRequiredInput() == false)
            {
                return;
            }

            string strDecisionReason = string.Empty;

            // 理由選択画面を開く
            SelectErrorReason frmErrorReason = new SelectErrorReason(false);
            frmErrorReason.ShowDialog(this);
            strDecisionReason = frmErrorReason.strDecisionReason;

            this.Visible = true;

            // 未選択の場合は終了
            if (string.IsNullOrEmpty(strDecisionReason))
            {
                return;
            }

            UpdAcceptanceCheckResult(intGetStatusNg(),
                                     strDecisionReason,
                                     strDecisionReason);
        }

        /// <summary>
        /// 検査対象選択へ戻るクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTargetSelection_Click(object sender, EventArgs e)
        {
            intDestination = g_CON_APID_TARGET_SELECTION;

            this.Close();
        }

        /// <summary>
        /// 複写指定画面表示
        /// </summary>
        private void dispCopyRegist()
        {
            bool bolRegister = false;
            int intBranch = 0;
            int intLine = -1;
            string strColumns = string.Empty;
            string strNgReason = string.Empty;
            string strOrgImagepath = string.Empty;
            string strMarkingImagepath = string.Empty;
            bool bolUpdMode = false;

            // 新規
            intBranch = 1;
            intLine = Convert.ToInt32(cmbBoxLine.SelectedItem);
            strColumns = cmbBoxColumns.SelectedItem.ToString();
            strNgReason = m_dtData.Rows[m_intPageIdx]["ng_reason"].ToString();
            strMarkingImagepath = m_dtData.Rows[m_intPageIdx]["marking_imagepath"].ToString();
            strOrgImagepath = m_dtData.Rows[m_intPageIdx]["org_imagepath"].ToString();

            // 修正
            if (m_clsDecisionResultCorrection.intBranchNum > 0)
            {
                intBranch = m_clsDecisionResultCorrection.intBranchNum;
                intLine = m_clsDecisionResultCorrection.intLine;
                strColumns = m_clsDecisionResultCorrection.strCloumns;
                strNgReason = m_clsDecisionResultCorrection.strNgReason;
                strMarkingImagepath = m_clsDecisionResultCorrection.strMarkingImagepath;
                strOrgImagepath = m_clsDecisionResultCorrection.strOrgImagepath;
                bolUpdMode = true;
            }

            // 複写指定画面を開く
            CopyReg frmCopyReg = new CopyReg(m_clsHeaderData,
                                             cmbBoxLine,
                                             cmbBoxColumns,
                                             intLine,
                                             strColumns,
                                             strNgReason,
                                             strMarkingImagepath,
                                             strOrgImagepath,
                                             intBranch,
                                             m_intFromApId,
                                             bolUpdMode);
            frmCopyReg.ShowDialog(this);
            bolRegister = frmCopyReg.bolRegister;

            this.Visible = true;

            // 登録済みor修正であれば、次ページを表示する。
            if (bolRegister == true || m_clsDecisionResultCorrection.intBranchNum > 0)
            {
                bolNextPage();
            }
        }

        /// <summary>
        /// 複写して登録
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopyRegist_Click(object sender, EventArgs e)
        {
            dispCopyRegist();
        }

        /// <summary>
        /// 未検知画像の追加クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddImage_Click(object sender, EventArgs e)
        {
            bool bolProcOkNg = false;

            string strSQL = string.Empty;
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
            string strFileNameWithExtension = string.Empty;
            string strFileName = string.Empty;
            string strOutPutFilePath = string.Empty;
            string strChkExistsFilePath = string.Empty;
            OpenFileDialog ofDialog = null;
            string[] strFileParam;
            AddImageProgressForm frmProgressForm = null;
            string strFaultImageSubDirectory = string.Empty;
            string strZipExtractToDirPath = string.Empty;
            DataTable dtData = null;
            int intParse = -1;

            string strZipFilePath = string.Empty;
            string strZipArchiveEntryFilePath = string.Empty;

            try
            {
                // 画像ファイルZIPを選択する
                try
                {
                    ofDialog = new OpenFileDialog();

                    // デフォルトのフォルダを指定する
                    ofDialog.InitialDirectory = @"C:";

                    //ダイアログのタイトルを指定する
                    ofDialog.Title = "ファイル選択ダイアログ";

                    //ダイアログを表示する
                    if (ofDialog.ShowDialog() == DialogResult.OK)
                    {
                        strFileNameWithExtension = ofDialog.SafeFileName;
                        strFileName = Path.GetFileNameWithoutExtension(strFileNameWithExtension);
                    }
                    else
                    {
                        return;
                    }

                    // 出力ファイル設定
                    strOutPutFilePath = Path.Combine( g_clsSystemSettingInfo.strNotDetectedImageCooperationDirectory 
                                                                                                       , strFileName + ".txt");

                    // オブジェクトを破棄する
                    ofDialog.Dispose();

                    // ファイル名妥当性チェック
                    strFileParam = strFileName.Split('_');
                    if ((strFileParam.Length < 2 || m_strFabricName != strFileParam[1]) ||
                        (strFileParam.Length < 3 || m_strInspectionDate.Replace("/", string.Empty) != strFileParam[2]) ||
                        (strFileParam.Length < 4 || int.TryParse(strFileParam[3], out intParse) == false || m_intInspectionNum != intParse))
                    {
                        MessageBox.Show(string.Format(g_clsMessageInfo.strMsgW0005,
                                                      m_strInspectionDate,
                                                      m_strFabricName,
                                                      m_intInspectionNum), g_CON_MESSAGE_TITLE_WARN, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //書き込むファイルが既に存在している場合は、上書きする
                    using (StreamWriter sw = new StreamWriter(strOutPutFilePath
                                                            , false
                                                            , Encoding.GetEncoding("shift_jis")))
                    {
                        // 空ファイルの生成
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}" ,g_clsMessageInfo.strMsgE0051 ,Environment.NewLine , ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0052, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
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
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "org_imagepath", DbType = DbType.String, Value = strFileNameWithExtension });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    // 件数
                    if (dtData.Rows.Count > 0)
                    {
                        if (Convert.ToInt32(dtData.Rows[0]["cnt"]) > 0)
                        {
                            // メッセージ出力
                            MessageBox.Show(g_clsMessageInfo.strMsgE0061, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0031, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                strChkExistsFilePath = Path.Combine( g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectory 
                                                                                                      , strFileName + ".txt");
                // プログレスフォーム表示
                frmProgressForm = new AddImageProgressForm(strChkExistsFilePath);
                frmProgressForm.ShowDialog(this);

                this.Visible = true;

                if (!frmProgressForm.bolChgFile)
                {
                    // キャンセル
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
                               FROM " + g_clsSystemSettingInfo.strCooperationBaseInstanceName + @".""rapid_" + m_strFabricName + "_" + m_intInspectionNum + @"""
                               WHERE fabric_name = :fabric_name
                                 AND inspection_num = :inspection_num 
                                 AND ng_image = :ng_image 
                                 AND rapid_result = :rapid_result
                                 AND edge_result = :edge_result
                                 AND masking_result = :masking_result";

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_image", DbType = DbType.String, Value = strFileNameWithExtension });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "rapid_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intRapidResultNon });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "edge_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intEdgeResultNon });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "masking_result", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intMaskingResultNon });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intOverDetectionExceptResultNon });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_result_non", DbType = DbType.Int16, Value = g_clsSystemSettingInfo.intAcceptanceCheckResultNon });

                    // sqlを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
                }
                catch (Exception ex)
                {
                    g_clsConnectionNpgsql.DbRollback();

                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0057, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
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

                    strZipFilePath = Path.Combine(g_strZipExtractDirPath, strFileName + ".zip");

                    // 欠点画像ZIPファイルを一時ZIP解凍用フォルダにコピーする
                    File.Copy(Path.Combine(g_clsSystemSettingInfo.strNgImageCooperationDirectory, strFileName + ".zip"),
                                           strZipFilePath, true);

                    // 一時ディレクトリに欠点画像のフォルダが存在する場合は削除する
                    using (ZipArchive za = ZipFile.Open(strZipFilePath, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry zae in za.Entries)
                        {
                            strZipExtractToDirPath = Path.Combine(g_strZipExtractDirPath,
                                                                  zae.FullName.Substring(0, (zae.FullName).IndexOf(Path.AltDirectorySeparatorChar)));

                            if (Directory.Exists(strZipExtractToDirPath) == true)
                            {
                                Directory.Delete(strZipExtractToDirPath, true);
                            }

                            break;
                        }
                    }

                    // 欠点画像ZIPを一時ディレクトリに解凍する
                    ZipFile.ExtractToDirectory(strZipFilePath,
                                               g_strZipExtractDirPath);

                    // 一時ディレクトリから欠点画像格納ディレクトリにファイルをコピーする。
                    foreach (string FilePath in Directory.GetFiles(Path.Combine(strZipExtractToDirPath), "*", SearchOption.AllDirectories))
                    {
                        strZipArchiveEntryFilePath = Path.Combine(m_strFaultImageSubDirPath,
                                                                  Path.GetFileName(FilePath));

                        // 存在する場合は削除する
                        if (File.Exists(strZipArchiveEntryFilePath))
                        {
                            File.Delete(Path.Combine(strZipArchiveEntryFilePath));
                        }

                        File.Copy(FilePath,
                                  Path.Combine(strZipArchiveEntryFilePath));

                    }

                    // 一時ディレクトリから欠点画像ZIPと解凍済み欠点画像ディレクトリを削除する
                    Directory.Delete(strZipExtractToDirPath, true);
                    File.Delete(strZipFilePath);

                    // 取り込み完了
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR,string.Format("{0}{1}{2}",  g_clsMessageInfo.strMsgE0040 ,Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0041, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // 追加分を画面表示
                try
                {
                    dtData = new DataTable();
                    strSQL = @"SELECT
                                   line
                                 , cloumns
                                 , ng_face
                                 , ng_reason
                                 , master_point
                                 , ng_distance_x
                                 , ng_distance_y
                                 , camera_num
                                 , org_imagepath
                                 , marking_imagepath
                                 , over_detection_except_result
                                 , acceptance_check_result
                               FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                               WHERE fabric_name = :fabric_name
                               AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                               AND   inspection_num = :inspection_num
                               AND   org_imagepath = :org_imagepath";

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "org_imagepath", DbType = DbType.String, Value = strFileNameWithExtension });

                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    // データに挿入する
                    DataRow drNew = m_dtData.NewRow();
                    drNew["branch_num"] = 1;
                    drNew["line"] = dtData.Rows[0]["line"];
                    drNew["cloumns"] = dtData.Rows[0]["cloumns"];
                    drNew["ng_face"] = dtData.Rows[0]["ng_face"];
                    drNew["ng_reason"] = dtData.Rows[0]["ng_reason"];
                    drNew["master_point"] = dtData.Rows[0]["master_point"];
                    drNew["ng_distance_x"] = dtData.Rows[0]["ng_distance_x"];
                    drNew["ng_distance_y"] = dtData.Rows[0]["ng_distance_y"];
                    drNew["camera_num"] = dtData.Rows[0]["camera_num"];
                    drNew["org_imagepath"] = dtData.Rows[0]["org_imagepath"];
                    drNew["marking_imagepath"] = dtData.Rows[0]["marking_imagepath"];
                    drNew["over_detection_except_result"] = dtData.Rows[0]["over_detection_except_result"];
                    drNew["acceptance_check_result"] = dtData.Rows[0]["acceptance_check_result"];
                     
                    // データを差し込み
                    m_dtData.Rows.InsertAt(drNew, m_intPageIdx);

                    // 画面表示
                    if (bolDispImageInfo(m_intPageIdx, m_CON_PAGE_WAY_R) == false)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR,string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001 ,Environment.NewLine, ex.Message));
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0039, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                bolProcOkNg = true;
            }
            finally
            {
                // オブジェクトの破棄
                if (ofDialog != null)
                {
                    ofDialog.Dispose();
                }

                if (frmProgressForm != null)
                {
                    frmProgressForm.Dispose();
                }

                // DB処理
                if (bolProcOkNg == true)
                {
                    if (m_intFromApId == 0)
                    {
                        g_clsConnectionNpgsql.DbCommit();
                    }
                }
                else
                {
                    g_clsConnectionNpgsql.DbRollback();
                }

                if (m_intFromApId == 0)
                {
                    g_clsConnectionNpgsql.DbClose();
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
