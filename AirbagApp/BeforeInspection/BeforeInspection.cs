using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BeforeInspection.Common;

namespace BeforeInspection
{
    public partial class BeforeInspection : Form
    {
        // パラメータ変数
        private int m_intInspectionNum = 0;             // 検査番号
        private int m_intBranchNum = 0;                 // 枝番
        private string m_strUnitNum = string.Empty;               // 号機
        private int m_intIlluminationInformation = 0;   // 照度情報
        private int m_intStartRegimarkCameraNum = 0;    // 開始レジマークカメラ番号
        private int m_intEndRegimarkCameraNum = 0;      // 終了レジマークカメラ番号
        private string m_strWorker1 = string.Empty;               // 作業者情報(社員番号)検反部No.1
        private string m_strWorker2 = string.Empty;               // 作業者情報(社員番号)検反部No.2
        private string m_strInspectionDirection;        // 検査方向
        private DateTime m_datInspectionDate;           // 検査日付
        private string m_strStartDatetime = string.Empty;         // 開始時刻

        // 定数
        private const string m_CON_SW = "ＳＷ";
        private const string m_CON_EW = "ＥＷ";
        private const string m_CON_INSPECTION_IMAGE_S_PATH = @".\image\ABCDE_S.png";
        private const string m_CON_INSPECTION_IMAGE_X_PATH = @".\image\ABCDE_X.png";
        private const string m_CON_INSPECTION_IMAGE_Y_PATH = @".\image\ABCDE_Y.png";
        private const string m_CON_INSPECTION_IMAGE_R_PATH = @".\image\ABCDE_R.png";
        private const string m_CON_FORMAT_UNIT_NUM = "{0}号機";
        private const string m_CON_FORMAT_INSPECTION_NUM = "検査番号:{0}";
        private const string m_CON_TEXTMODE_ORDERIMG = "1";
        private const string m_CON_TEXTMODE_FABRICNAME = "2";
        private const string m_CON_TEXTMODE_NUMBER = "3";
        private const string m_CON_EXTENSION_TEXT = ".txt";
        private const string m_CON_EXTENSION_BUSY = ".busy";
        private const string m_CON_EXTENSION_FINAL = ".final";
        private const int m_CON_MAXLENGTH_ORDERIMG = 7;
        private const int m_CON_MAXLENGTH_FABRICNAME = 10;

        // 検査方向背景色関連
        private Color m_clrInspectionDirectionActFore = System.Drawing.SystemColors.ActiveCaption;
        private Color m_clrInspectionDirectionActBack = SystemColors.Control;

        // ステータス背景色関連
        private int m_intStatus;
        private Color m_clrStatusActFore = System.Drawing.SystemColors.ControlText;
        private Color m_clrStatusActBack = System.Drawing.Color.White;
        private Color m_clrStatusNonActFore = System.Drawing.SystemColors.ControlLightLight;
        private Color m_clrStatusNonActBack = System.Drawing.Color.Transparent;

        // 前回のチェック時刻
        private DateTime datCheckTime;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BeforeInspection()
        {
            m_strUnitNum = g_clsSystemSettingInfo.strUnitNum;

            InitializeComponent();
        }

        /// <summary>
        /// 入力フォーム表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispTenKeyInputForm(object sender, EventArgs e)
        {
            // 撮像装置部が処理中か確認する。
            if (!bolCheckBusyFile() ||
                !bolCheckFinalFile())
            {
                return;
            }

            TextBox txtBox = (TextBox)sender;
            TenKeyInput frmTenKeyInput = null;

            // 入力用フォームの表示
            if (txtBox.Equals(txtOrderImg))
            {
                frmTenKeyInput =
                    new TenKeyInput(
                        m_CON_MAXLENGTH_ORDERIMG,
                        m_CON_TEXTMODE_ORDERIMG);
            }
            else if (txtBox.Equals(txtFabricName))
            {
                frmTenKeyInput =
                    new TenKeyInput(
                        m_CON_MAXLENGTH_FABRICNAME,
                        m_CON_TEXTMODE_FABRICNAME);
            }
            else
            {
                frmTenKeyInput =
                    new TenKeyInput(
                        txtBox.MaxLength,
                        m_CON_TEXTMODE_NUMBER);
            }

            frmTenKeyInput.ShowDialog(this);
            this.Visible = true;

            if (!string.IsNullOrEmpty(frmTenKeyInput.strInput))
            {
                txtBox.Text = frmTenKeyInput.strInput;
                txtBox.SelectAll();

                CalcMaxLine();
            }
        }

        /// <summary>
        /// 最終行の計算
        /// </summary>
        private void CalcMaxLine()
        {

            // 最終行番のラベルを初期化
            lblInspectionEndLine.Text = string.Empty;
            lblInspectionEndLine.ForeColor = System.Drawing.SystemColors.ControlText;

            // 最終行番の算出
            int intStartLine = 0;
            int intInspectionTargetLine = 0;
            int intEndLine = 0;

            if (string.IsNullOrEmpty(txtInspectionTargetLine.Text) || string.IsNullOrEmpty(txtInspectionStartLine.Text))
            {
                return;
            }


            // 最終行計算
            intStartLine = int.Parse(txtInspectionStartLine.Text);
            intInspectionTargetLine = int.Parse(txtInspectionTargetLine.Text);

            if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS ||
                m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
            {
                intEndLine = intStartLine + intInspectionTargetLine - 1;
            }

            if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY ||
                m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
            {
                intEndLine = intStartLine - intInspectionTargetLine + 1;
            }

            lblInspectionEndLine.Text = intEndLine.ToString();

            // 整合性チェック
            if (intEndLine <= 0)
            {
                // 不正の場合はメッセージを表示
                lblInspectionEndLine.ForeColor = Color.Red;
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0032, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // フォーカスセット
                txtInspectionTargetLine.Focus();
            }

        }

        /// <summary>
        /// ステータス(画面)をセット
        /// </summary>
        /// <param name="intStatus">ステータス（検査開始前/検査準備完了/検査中断/検査終了）</param>
        private void SetStatusCtrSetting(int intStatus)
        {
            m_intStatus = intStatus;

            // 非アクティブ化
            foreach (Label lblStatus in new Label[] { lblStatusBef, lblStatusChk, lblStatusStp, lblStatusEnd })
            {
                if ((m_intStatus == g_clsSystemSettingInfo.intStatusBef && lblStatus == lblStatusBef) ||
                    (m_intStatus == g_clsSystemSettingInfo.intStatusChk && lblStatus == lblStatusChk) ||
                    (m_intStatus == g_clsSystemSettingInfo.intStatusStp && lblStatus == lblStatusStp) ||
                    (m_intStatus == g_clsSystemSettingInfo.intStatusEnd && lblStatus == lblStatusEnd))
                {
                    continue;
                }

                lblStatus.ForeColor = m_clrStatusNonActFore;
                lblStatus.BackColor = m_clrStatusNonActBack;
            }

            // アクティブ化
            if (m_intStatus == g_clsSystemSettingInfo.intStatusBef)
            {
                // 背景色変更(協調)
                lblStatusBef.ForeColor = m_clrStatusActFore;
                lblStatusBef.BackColor = m_clrStatusActBack;

                // 入力制御
                txtProductName.Enabled = true;
                txtOrderImg.Enabled = true;
                txtFabricName.Enabled = true;
                txtInspectionTargetLine.Enabled = true;
                txtInspectionStartLine.Enabled = true;
                txtWorker1.Enabled = true;
                txtWorker2.Enabled = true;
                btnStartDatetime.Enabled = true;
                btnEndDatetime.Enabled = false;
                btnInspectionDirectionS.Enabled = true;
                btnInspectionDirectionX.Enabled = true;
                btnInspectionDirectionY.Enabled = true;
                btnInspectionDirectionR.Enabled = true;
                btnInspectionStop.Enabled = false;
                btnSet.Enabled = true;
                btnNextFabric.Enabled = false;
            }

            if (m_intStatus == g_clsSystemSettingInfo.intStatusChk)
            {
                // 背景色変更(協調)
                lblStatusChk.ForeColor = m_clrStatusActFore;
                lblStatusChk.BackColor = m_clrStatusActBack;

                // 入力制御
                txtProductName.Enabled = false;
                txtOrderImg.Enabled = false;
                txtFabricName.Enabled = false;
                txtInspectionTargetLine.Enabled = true;
                txtInspectionStartLine.Enabled = false;
                txtWorker1.Enabled = true;
                txtWorker2.Enabled = true;
                btnStartDatetime.Enabled = false;
                btnEndDatetime.Enabled = true;
                btnInspectionDirectionS.Enabled = false;
                btnInspectionDirectionX.Enabled = false;
                btnInspectionDirectionY.Enabled = false;
                btnInspectionDirectionR.Enabled = false;
                btnInspectionStop.Enabled = true;
                btnSet.Enabled = true;
                btnNextFabric.Enabled = false;
            }

            if (m_intStatus == g_clsSystemSettingInfo.intStatusStp)
            {
                // 背景色変更(協調)
                lblStatusStp.ForeColor = m_clrStatusActFore;
                lblStatusStp.BackColor = m_clrStatusActBack;

                // 入力制御
                txtProductName.Enabled = false;
                txtOrderImg.Enabled = false;
                txtFabricName.Enabled = false;
                txtInspectionTargetLine.Enabled = true;
                txtInspectionStartLine.Enabled = true;
                txtWorker1.Enabled = true;
                txtWorker2.Enabled = true;
                btnStartDatetime.Enabled = true;
                btnEndDatetime.Enabled = false;
                btnInspectionDirectionS.Enabled = false;
                btnInspectionDirectionX.Enabled = false;
                btnInspectionDirectionY.Enabled = false;
                btnInspectionDirectionR.Enabled = false;
                btnInspectionStop.Enabled = false;
                btnSet.Enabled = true;
                btnNextFabric.Enabled = true;
            }

            if (m_intStatus == g_clsSystemSettingInfo.intStatusEnd)
            {
                // 背景色変更(協調)
                lblStatusEnd.ForeColor = m_clrStatusActFore;
                lblStatusEnd.BackColor = m_clrStatusActBack;

                // 入力制御
                txtProductName.Enabled = false;
                txtOrderImg.Enabled = false;
                txtFabricName.Enabled = false;
                txtInspectionTargetLine.Enabled = true;
                txtInspectionStartLine.Enabled = true;
                txtWorker1.Enabled = true;
                txtWorker2.Enabled = true;
                btnStartDatetime.Enabled = true;
                btnEndDatetime.Enabled = false;
                btnInspectionDirectionS.Enabled = false;
                btnInspectionDirectionX.Enabled = false;
                btnInspectionDirectionY.Enabled = false;
                btnInspectionDirectionR.Enabled = false;
                btnInspectionStop.Enabled = false;
                btnSet.Enabled = true;
                btnNextFabric.Enabled = true;
            }
        }

        /// <summary>
        /// 検査方向設定ボタン(画面)をセット
        /// </summary>
        /// <param name="strInspectionDirection">検査方向（S/X/Y/R）</param>
        private void SetInspectionDirectionSetting(string strInspectionDirection)
        {
            // 非アクティブ化
            foreach (Button btnInspectionDirection in new Button[] { btnInspectionDirectionS, btnInspectionDirectionX, btnInspectionDirectionY, btnInspectionDirectionR })
            {
                if ((strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS && btnInspectionDirection != btnInspectionDirectionS) ||
                    (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX && btnInspectionDirection != btnInspectionDirectionX) ||
                    (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY && btnInspectionDirection != btnInspectionDirectionY) ||
                    (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR && btnInspectionDirection != btnInspectionDirectionR))
                {
                    btnInspectionDirection.BackColor = m_clrInspectionDirectionActBack;
                }
            }

            lblSEWNo1Top.Text = string.Format(" ↓↓↓ {0} ↓↓↓ ", m_CON_EW);
            lblSEWNo1Bot.Text = string.Format(" ↓↓↓ {0} ↓↓↓ ", m_CON_SW);
            lblSEWNo2Top.Text = string.Format(" ↓↓↓ {0} ↓↓↓ ", m_CON_EW);
            lblSEWNo2Bot.Text = string.Format(" ↓↓↓ {0} ↓↓↓ ", m_CON_SW);


            // アクティブ化
            if (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS)
            {
                picInspectionDirectionNo1.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_S_PATH);
                picInspectionDirectionNo2.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_X_PATH);
                btnInspectionDirectionS.BackColor = m_clrInspectionDirectionActFore;
                m_strInspectionDirection = g_clsSystemSettingInfo.strInspectionDirectionS;
            }

            if (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
            {
                picInspectionDirectionNo1.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_X_PATH);
                picInspectionDirectionNo2.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_S_PATH);
                btnInspectionDirectionX.BackColor = m_clrInspectionDirectionActFore;
                m_strInspectionDirection = g_clsSystemSettingInfo.strInspectionDirectionX;
            }

            if (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY)
            {
                picInspectionDirectionNo1.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_Y_PATH);
                picInspectionDirectionNo2.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_R_PATH);
                btnInspectionDirectionY.BackColor = m_clrInspectionDirectionActFore;
                m_strInspectionDirection = g_clsSystemSettingInfo.strInspectionDirectionY;
            }

            if (strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
            {
                picInspectionDirectionNo1.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_R_PATH);
                picInspectionDirectionNo2.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_Y_PATH);
                btnInspectionDirectionR.BackColor = m_clrInspectionDirectionActFore;
                m_strInspectionDirection = g_clsSystemSettingInfo.strInspectionDirectionR;
            }

            // 最終行の計算
            CalcMaxLine();
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <returns></returns>
        private Boolean InputDataCheck()
        {
            // 必須入力チェック
            if (CheckRequiredInput(txtProductName, "品名") == false ||
                CheckRequiredInput(txtOrderImg, "指図") == false ||
                CheckRequiredInput(txtFabricName, "反番") == false ||
                CheckRequiredInput(txtInspectionTargetLine, "検査対象数(行数)") == false ||
                CheckRequiredInput(txtInspectionStartLine, "検査開始行") == false ||
                CheckRequiredInput(txtWorker1, "作業者情報(社員番号)検反部No.1") == false ||
                CheckRequiredInput(txtWorker2, "作業者情報(社員番号)検反部No.2") == false)
            {
                return false;
            }

            // 指図入力チェック
            if (txtOrderImg.Text.Length > 7)
            {
                // メッセージ出力
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0012, "指図", 1, 7), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                txtOrderImg.Focus();
                return false;
            }

            // 反番入力チェック
            if (txtFabricName.Text.Length > 10)
            {
                // メッセージ出力
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0012, "反番", 1, 10), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                txtFabricName.Focus();
                return false;
            }

            // 検査対象数(行数)入力チェック
            if (int.Parse(txtInspectionTargetLine.Text) == 0)
            {
                // メッセージ出力
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0013, "検査対象数(行数)", 1, 9999), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                btnStartDatetime.Focus();
                return false;
            }

            // 検査開始行入力チェック
            if (int.Parse(txtInspectionStartLine.Text) == 0)
            {
                // メッセージ出力
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0013, "検査開始行", 1, 9999), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                btnStartDatetime.Focus();
                return false;
            }

            if (int.Parse(lblInspectionEndLine.Text) <= 0)
            {
                // 不正の場合はメッセージを表示
                lblInspectionEndLine.ForeColor = Color.Red;
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0032, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // フォーカスセット
                txtInspectionTargetLine.Focus();
                return false;
            }

            // 開始時刻入力チェック
            if (string.IsNullOrEmpty(lblStartDatetime.Text))
            {
                // メッセージ出力
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0007, "開始時刻"), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                btnStartDatetime.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 必須入力チェック
        /// </summary>
        /// <param name="tbxCheckData">チェック対象テキストボックス</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <returns></returns>
        private Boolean CheckRequiredInput(TextBox tbxCheckData, String strItemName)
        {
            // 必須入力チェック
            if (string.IsNullOrEmpty(tbxCheckData.Text))
            {
                // メッセージ出力
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0007, strItemName), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                tbxCheckData.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 検査情報ヘッダーの登録処理
        /// </summary>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="intBranchNum">枝番</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private Boolean RegStartInspectionInfoHeader(int intInspectionNum, int intBranchNum)
        {
            try
            {
                // SQL文を作成する
                string strSql = @"INSERT INTO inspection_info_header(
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
                                    --, end_datetime
                                    , inspection_direction
                                    , inspection_date
                                    , illumination_information
                                    , start_regimark_camera_num
                                    , end_regimark_camera_num
                                    )VALUES(
                                      :inspection_num
                                    , :branch_num
                                    , :product_name
                                    , :unit_num
                                    , :order_img
                                    , :fabric_name
                                    , :inspection_target_line
                                    , :inspection_end_line
                                    , :inspection_start_line
                                    , :worker_1
                                    , :worker_2
                                    , TO_TIMESTAMP(:start_datetime,'YYYY/MM/DD HH24:MI:SS')
                                    --, :end_datetime
                                    , :inspection_direction
                                    , TO_DATE(:inspection_date,'YYYY/MM/DD')
                                    , :illumination_information
                                    , :start_regimark_camera_num
                                    , :end_regimark_camera_num
                                    );";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = intBranchNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "product_name", DbType = DbType.String, Value = txtProductName.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "order_img", DbType = DbType.String, Value = txtOrderImg.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = txtFabricName.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_target_line", DbType = DbType.Int16, Value = int.Parse(txtInspectionTargetLine.Text) });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_end_line", DbType = DbType.Int16, Value = int.Parse(lblInspectionEndLine.Text) });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_start_line", DbType = DbType.Int16, Value = int.Parse(txtInspectionStartLine.Text) });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "worker_1", DbType = DbType.String, Value = m_strWorker1 });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "worker_2", DbType = DbType.String, Value = m_strWorker2 });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "start_datetime", DbType = DbType.String, Value = lblStartDatetime.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_direction", DbType = DbType.String, Value = m_strInspectionDirection });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_datInspectionDate.ToString("yyyy/MM/dd") });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "illumination_information", DbType = DbType.Int16, Value = m_intIlluminationInformation });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "start_regimark_camera_num", DbType = DbType.Int16, Value = m_intStartRegimarkCameraNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "end_regimark_camera_num", DbType = DbType.Int16, Value = m_intEndRegimarkCameraNum });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strSql, lstNpgsqlCommand);

                // 撮像装置部へ連携用ファイル出力
                if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum, g_clsConnectionNpgsql) == false)
                {
                    btnSet.Focus();
                    return false;
                }

                // DBコミット
                g_clsConnectionNpgsql.DbCommit();
                return true;

            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0033, Environment.NewLine, ex.Message));
                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0034)).ShowDialog(this);

                // フォーカスを設定
                btnSet.Focus();

                return false;
            }
            finally
            {
                // DBクローズ
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 検査情報ヘッダーの更新処理(検査終了)
        /// </summary>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="intBranchNum">枝番</param>
        /// <param name="strEndDateTime">終了時刻</param>
        /// <param name="bolFileOutputFlag">ファイル出力フラグ</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private Boolean UpdEndInspectionInfoHeader(
            int intInspectionNum,
            int intBranchNum,
            string strEndDateTime,
            bool bolFileOutputFlag = true)
        {
            string strSql = string.Empty;
            try
            {
                // SQL文を作成する
                strSql = @"UPDATE inspection_info_header
                              SET end_datetime = TO_TIMESTAMP(:end_datetime,'YYYY/MM/DD HH24:MI:SS')
                            WHERE inspection_num = :inspection_num
                              AND branch_num = :branch_num
                              AND unit_num = :unit_num
                              AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date ;";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = intBranchNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_datInspectionDate.ToString("yyyy/MM/dd") });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "end_datetime", DbType = DbType.String, Value = strEndDateTime });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strSql, lstNpgsqlCommand);


                // 撮像装置部へ連携用ファイル出力
                if (bolFileOutputFlag &&
                    bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum, g_clsConnectionNpgsql) == false)
                {
                    btnEndDatetime.Focus();
                    return false;
                }

                // DBコミット
                g_clsConnectionNpgsql.DbCommit();
                return true;

            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0033, Environment.NewLine, ex.Message));
                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0035)).ShowDialog(this);

                // フォーカスを設定
                btnEndDatetime.Focus();

                return false;
            }
            finally
            {
                // DBクローズ
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 検査番号の取得
        /// </summary>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private bool bolGetInspectionNum(out int intInspectionNum)
        {
            // 初期化
            intInspectionNum = 0;
            string strSQL = string.Empty;
            DataTable dtData;

            try
            {
                dtData = new DataTable();
                strSQL = @"SELECT COALESCE(MAX(inspection_num),0) AS inspection_num 
                             FROM inspection_info_header 
                            WHERE inspection_date = current_date
                              AND unit_num = :unit_num ";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                // SQL抽出
                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                //  検査番号
                if (dtData.Rows.Count > 0)
                {
                    intInspectionNum = int.Parse(dtData.Rows[0]["inspection_num"].ToString());
                }

                // 枝番のリセット
                m_intBranchNum = 1;

                // 検査日付の更新
                m_datInspectionDate = DateTime.Now.Date;

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0031)).ShowDialog(this);

                return false;
            }
        }

        /// <summary>
        /// 検査番号の採番
        /// </summary>
        /// <param name="intInspectionNum"></param>
        /// <param name="strInspectionDate"></param>
        /// <returns></returns>
        private bool bolNumberInspectionNum(out int intInspectionNum)
        {
            if (bolGetInspectionNum(out intInspectionNum) == true)
            {
                intInspectionNum++;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 枝番の取得
        /// </summary>
        /// <param name="intBranchNum">枝番</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="intInspectionNum">検査番号</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private bool bolGetBranchNum(out int intBranchNum, string strInspectionDate, int intInspectionNum)
        {
            // 初期化
            string strSQL = string.Empty;
            DataTable dtData;
            intBranchNum = 0;

            try
            {
                dtData = new DataTable();
                strSQL = @"SELECT COALESCE(MAX(branch_num),0) AS branch_num 
                             FROM inspection_info_header 
                            WHERE TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd 
                              AND unit_num = :unit_num 
                              AND inspection_num = :inspection_num";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_datInspectionDate.ToString("yyyy/MM/dd") });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = intInspectionNum });

                // SQL抽出
                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                //  枝番
                if (dtData.Rows.Count > 0)
                {
                    intBranchNum = int.Parse(dtData.Rows[0]["branch_num"].ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0031)).ShowDialog(this);

                return false;
            }
        }

        /// <summary>
        /// 品番登録情報の取得
        /// </summary>
        /// <param name="intIlluminationInformation">照度情報</param>
        /// <param name="intStartRegimarkCameraNum">開始レジマークカメラ番号</param>
        /// <param name="intEndRegimarkCameraNum">終了レジマークカメラ番号</param>
        /// <param name="strProductName">品名</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private bool bolGetMstProductInfo(out int intIlluminationInformation,
                                          out int intStartRegimarkCameraNum,
                                          out int intEndRegimarkCameraNum,
                                          string strProductName)
        {
            // 初期化
            intIlluminationInformation = 0;
            intStartRegimarkCameraNum = 0;
            intEndRegimarkCameraNum = 0;

            string strSQL = string.Empty;
            DataTable dtData;

            try
            {
                dtData = new DataTable();
                strSQL = @"SELECT ";
                strSQL += @"    illumination_information";
                strSQL += @"  , start_regimark_camera_num";
                strSQL += @"  , end_regimark_camera_num ";
                strSQL += @"FROM ";
                strSQL += @"    mst_product_info ";
                strSQL += @"WHERE product_name = :product_name ";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "product_name", DbType = DbType.String, Value = strProductName });

                // SQL抽出
                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                if (dtData.Rows.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(dtData.Rows[0]["illumination_information"].ToString()))
                    {
                        intIlluminationInformation = int.Parse(dtData.Rows[0]["illumination_information"].ToString());
                    }

                    if (!string.IsNullOrWhiteSpace(dtData.Rows[0]["start_regimark_camera_num"].ToString()))
                    {
                        intStartRegimarkCameraNum = int.Parse(dtData.Rows[0]["start_regimark_camera_num"].ToString());
                    }

                    if (!string.IsNullOrWhiteSpace(dtData.Rows[0]["end_regimark_camera_num"].ToString()))
                    {
                        intEndRegimarkCameraNum = int.Parse(dtData.Rows[0]["end_regimark_camera_num"].ToString());
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}{3}{4}",
                        g_clsMessageInfo.strMsgE0001,
                        Environment.NewLine,
                        ex.Message,
                        Environment.NewLine,
                        "[照度情報・開始レジマークカメラ番号・終了レジマークカメラ番号]"));

                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0021)).ShowDialog(this);

                return false;
            }
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="strFabricName"></param>
        /// <param name="intInspectionNum"></param>
        /// <param name="intBranchNum"></param>
        /// <returns></returns>
        private bool bolOutFile(string strFabricName, int intInspectionNum, int intBranchNum, ConnectionNpgsql objPgsqlConnect)
        {
            string strSQL = string.Empty;
            string strFilePath = string.Empty;
            string strFileNameBusy = string.Empty;
            string strFileNameText = string.Empty;
            DataTable dtData = new DataTable();

            try
            {
                // DBオープン
                strSQL = @"SELECT 
                               iih.product_name
                             , iih.order_img
                             , iih.inspection_target_line
                             , iih.inspection_end_line
                             , iih.inspection_start_line
                             , iih.worker_1
                             , iih.worker_2
                             , TO_CHAR(iih.start_datetime,'YYYY/MM/DD HH24:MI:SS') AS start_datetime
                             , TO_CHAR(iih.end_datetime,'YYYY/MM/DD HH24:MI:SS') AS end_datetime
                             , iih.inspection_direction 
                             , iih.illumination_information 
                             , iih.start_regimark_camera_num 
                             , iih.end_regimark_camera_num 
                             , COALESCE(mpi.line_length, 0) AS line_length 
                             , COALESCE(mpi.regimark_between_length, 0) AS regimark_between_length
                           FROM 
                               inspection_info_header AS iih 
                           LEFT JOIN 
                               mst_product_info AS mpi 
                           ON iih.product_name = mpi.product_name 
                           WHERE TO_CHAR(iih.inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd 
                             AND iih.unit_num = :unit_num 
                             AND iih.inspection_num = :inspection_num
                             AND iih.branch_num = :branch_num";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_datInspectionDate.ToString("yyyy/MM/dd") });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "branch_num", DbType = DbType.Int16, Value = intBranchNum });

                // SQL抽出
                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                if (dtData.Rows.Count > 0)
                {
                    strFilePath = g_clsSystemSettingInfo.strImagingDeviceCooperationDirectory + @"\" + strFabricName + "_" + intInspectionNum.ToString() + "_" + intBranchNum.ToString();
                    strFileNameBusy = strFilePath + m_CON_EXTENSION_BUSY;
                    strFileNameText = strFilePath + m_CON_EXTENSION_TEXT;

                    // busyファイルの作成
                    using (StreamWriter sw = File.CreateText(strFileNameBusy))
                    {
                        // 値の設定
                        sw.WriteLine(intInspectionNum.ToString());
                        sw.WriteLine(intBranchNum.ToString());
                        sw.WriteLine(dtData.Rows[0]["product_name"].ToString());
                        sw.WriteLine(m_strUnitNum.ToString());
                        sw.WriteLine(dtData.Rows[0]["order_img"].ToString());
                        sw.WriteLine(strFabricName);
                        sw.WriteLine(dtData.Rows[0]["inspection_target_line"].ToString());
                        sw.WriteLine(dtData.Rows[0]["inspection_end_line"].ToString());
                        sw.WriteLine(dtData.Rows[0]["inspection_start_line"].ToString());
                        sw.WriteLine(dtData.Rows[0]["worker_1"].ToString());
                        sw.WriteLine(dtData.Rows[0]["worker_2"].ToString());
                        sw.WriteLine(dtData.Rows[0]["start_datetime"].ToString());
                        sw.WriteLine(dtData.Rows[0]["end_datetime"].ToString());
                        sw.WriteLine(dtData.Rows[0]["inspection_direction"].ToString());
                        sw.WriteLine(dtData.Rows[0]["illumination_information"].ToString());
                        sw.WriteLine(dtData.Rows[0]["start_regimark_camera_num"].ToString());
                        sw.WriteLine(dtData.Rows[0]["end_regimark_camera_num"].ToString());
                        sw.WriteLine(dtData.Rows[0]["line_length"].ToString());
                        sw.WriteLine(dtData.Rows[0]["regimark_between_length"].ToString());
                    }

                    // 拡張子をtxtに変更
                    File.Copy(strFileNameBusy, strFileNameText, true);
                    File.Delete(strFileNameBusy);
                }
                return true;
            }
            catch (Exception ex)
            {
                // 作成したbusyファイルが残っている場合、削除する
                if (File.Exists(strFileNameBusy))
                {
                    File.Delete(strFileNameBusy);
                }

                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0036, Environment.NewLine, ex.Message));
                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0037)).ShowDialog(this);

                // DBロールバック
                objPgsqlConnect.DbRollback();

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
        private void BeforeInspection_Load(object sender, EventArgs e)
        {
            bool bolProcOkNg = false;

            string strSQL = string.Empty;
            DataTable dtData;

            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            try
            {
                this.SuspendLayout();

                m_datInspectionDate = DateTime.Now.Date;


                // 検査対象数/検査開始行を読み取り専用
                txtInspectionTargetLine.ReadOnly = true;
                txtInspectionTargetLine.BackColor = System.Drawing.Color.White;
                txtInspectionStartLine.ReadOnly = true;
                txtInspectionStartLine.BackColor = System.Drawing.Color.White;

                try
                {
                    dtData = new DataTable();
                    strSQL = @"SELECT COALESCE(MAX(inspection_num),0) AS inspection_num
                                 FROM inspection_info_header
                                WHERE (inspection_date = CURRENT_DATE OR
                                       inspection_date = CURRENT_DATE - 1)
                                  AND unit_num = :unit_num
                                  AND end_datetime IS NULL ";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    // SQL抽出
                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                    //  検査番号
                    m_intInspectionNum = int.Parse(dtData.Rows[0]["inspection_num"].ToString());
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                    // メッセージ出力
                    new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0031)).ShowDialog(this);

                    return;
                }

                if (m_intInspectionNum == 0)
                {
                    // 検査番号の取得
                    if (bolGetInspectionNum(out m_intInspectionNum) == false)
                    {
                        return;
                    }

                    // 値の初期化
                    lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum.ToString());
                    lblUnitNum.Text = string.Format(m_CON_FORMAT_UNIT_NUM, m_strUnitNum);
                    txtProductName.Text = string.Empty;
                    txtOrderImg.Text = string.Empty;
                    txtFabricName.Text = string.Empty;
                    txtInspectionTargetLine.Text = string.Empty;
                    lblInspectionEndLine.Text = string.Empty;
                    txtInspectionStartLine.Text = string.Empty;
                    txtWorker1.Text = string.Empty;
                    txtWorker2.Text = string.Empty;
                    lblStartDatetime.Text = string.Empty;
                    lblEndDatetime.Text = string.Empty;
                    SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionS);

                    // 変数の設定
                    m_intBranchNum = 1;

                    // 品名にフォーカスを設定する
                    txtProductName.Focus();

                    // ステータスの表示設定(検査開始前)
                    SetStatusCtrSetting(g_clsSystemSettingInfo.intStatusBef);
                }
                else
                {
                    try
                    {
                        dtData = new DataTable();
                        strSQL = @"SELECT 
                                       iih.branch_num
                                     , iih.product_name
                                     , iih.order_img
                                     , iih.fabric_name
                                     , iih.inspection_target_line
                                     , iih.inspection_end_line
                                     , iih.inspection_start_line
                                     , iih.worker_1
                                     , iih.worker_2
                                     , (SELECT worker_name_sei || worker_name_mei FROM mst_worker WHERE employee_num = iih.worker_1) AS worker_1_name
                                     , (SELECT worker_name_sei || worker_name_mei FROM mst_worker WHERE employee_num = iih.worker_2) AS worker_2_name
                                     , TO_CHAR(iih.start_datetime,'YYYY/MM/DD HH24:MI:SS') AS start_datetime
                                     , iih.inspection_direction 
                                     , TO_CHAR(iih.inspection_date,'YYYY/MM/DD') AS inspection_date 
                                     , mpi.illumination_information
                                     , mpi.start_regimark_camera_num
                                     , mpi.end_regimark_camera_num 
                                   FROM 
                                       inspection_info_header iih
                                   INNER JOIN
                                       mst_product_info mpi
                                   ON iih.product_name = mpi.product_name
                                   WHERE (iih.inspection_date = CURRENT_DATE OR 
                                          iih.inspection_date = CURRENT_DATE - 1) 
                                     AND iih.unit_num = :unit_num 
                                     AND iih.end_datetime IS NULL 
                                     AND iih.inspection_num = :inspection_num";

                        // SQLコマンドに各パラメータを設定する
                        List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });

                        // SQL抽出
                        g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                        // 値の初期化
                        lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum.ToString());
                        lblUnitNum.Text = string.Format(m_CON_FORMAT_UNIT_NUM, m_strUnitNum);
                        txtProductName.Text = dtData.Rows[0]["product_name"].ToString();
                        txtOrderImg.Text = dtData.Rows[0]["order_img"].ToString();
                        txtFabricName.Text = dtData.Rows[0]["fabric_name"].ToString();
                        txtInspectionTargetLine.Text = dtData.Rows[0]["inspection_target_line"].ToString();
                        lblInspectionEndLine.Text = dtData.Rows[0]["inspection_end_line"].ToString();
                        txtInspectionStartLine.Text = dtData.Rows[0]["inspection_start_line"].ToString();
                        txtWorker1.Text = dtData.Rows[0]["worker_1_name"].ToString();
                        txtWorker2.Text = dtData.Rows[0]["worker_2_name"].ToString();
                        lblStartDatetime.Text = dtData.Rows[0]["start_datetime"].ToString();
                        lblEndDatetime.Text = string.Empty;
                        SetInspectionDirectionSetting(dtData.Rows[0]["inspection_direction"].ToString());

                        // 変数の設定
                        m_intBranchNum = int.Parse(dtData.Rows[0]["branch_num"].ToString());
                        m_strWorker1 = dtData.Rows[0]["worker_1"].ToString();
                        m_strWorker2 = dtData.Rows[0]["worker_2"].ToString();

                        if (!string.IsNullOrWhiteSpace(dtData.Rows[0]["illumination_information"].ToString()))
                        {
                            m_intIlluminationInformation = int.Parse(dtData.Rows[0]["illumination_information"].ToString());
                        }

                        if (!string.IsNullOrWhiteSpace(dtData.Rows[0]["start_regimark_camera_num"].ToString()))
                        {
                            m_intStartRegimarkCameraNum = int.Parse(dtData.Rows[0]["start_regimark_camera_num"].ToString());
                        }

                        if (!string.IsNullOrWhiteSpace(dtData.Rows[0]["end_regimark_camera_num"].ToString()))
                        {
                            m_intEndRegimarkCameraNum = int.Parse(dtData.Rows[0]["end_regimark_camera_num"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                        // メッセージ出力
                        new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0031)).ShowDialog(this);

                        return;
                    }

                    // 検査対象数(行数)にフォーカスを設定する
                    txtInspectionTargetLine.Focus();

                    // ステータスの表示設定(検査準備完了)
                    SetStatusCtrSetting(g_clsSystemSettingInfo.intStatusChk);
                }

                // フォームの表示
                this.Visible = true;

                // 座標系プロパティの設定　※Load後じゃないと反映されない
                txtProductName.Location = new Point(txtProductName.Location.X, (int)((double)(pnlHinNo.Size.Height / 2) - ((double)txtProductName.Size.Height / 2)));
                txtOrderImg.Location = new Point(txtOrderImg.Location.X, (int)(((double)pnlSashizu.Size.Height / 2) - ((double)txtOrderImg.Size.Height / 2)));
                txtFabricName.Location = new Point(txtFabricName.Location.X, (int)(((double)pnlHanNo.Size.Height / 2) - ((double)txtFabricName.Size.Height / 2)));
                txtInspectionTargetLine.Location = new Point(txtInspectionTargetLine.Location.X, (int)(((double)pnlKensaTaishoNum_LastNum.Size.Height / 2) - ((double)txtInspectionTargetLine.Size.Height / 2)));
                lblInspectionEndLine.Location = new Point(lblInspectionEndLine.Location.X, (int)(((double)pnlKensaTaishoNum_LastNum.Size.Height / 2) - ((double)lblInspectionEndLine.Size.Height / 2)));
                txtInspectionStartLine.Location = new Point(txtInspectionStartLine.Location.X, (int)(((double)pnlKensaStartRow.Size.Height / 2) - ((double)txtInspectionStartLine.Size.Height / 2)));
                txtWorker1.Location = new Point(txtWorker1.Location.X, (int)(((double)pnlSagyosyaInfo_1.Size.Height / 2) - ((double)txtWorker1.Size.Height / 2)));
                txtWorker2.Location = new Point(txtWorker2.Location.X, (int)(((double)pnlSagyosyaInfo_2.Size.Height / 2) - ((double)txtWorker2.Size.Height / 2)));

                bolProcOkNg = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0031)).ShowDialog(this);

                // 品名にフォーカスを設定する
                txtProductName.Focus();

                return;
            }
            finally
            {
                this.ResumeLayout();

                if (bolProcOkNg == false)
                {
                    this.Close();
                }
            }
        }

        /// <summary>
        /// 品名テキストボックスクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtProductName_Click(object sender, EventArgs e)
        {
            // 撮像装置部が処理中か確認する。
            if (!bolCheckBusyFile() ||
                !bolCheckFinalFile())
            {
                return;
            }

            // 品番選択画面
            ProductNameSelection frmHinNoSelection = new ProductNameSelection();
            frmHinNoSelection.ShowDialog(this);
            if (string.IsNullOrEmpty(frmHinNoSelection.strProductName))
            {
                return;
            }


            // 品番登録情報の取得(照度情報,開始レジマークカメラ番号,終了レジマークカメラ番号)
            if (bolGetMstProductInfo(out m_intIlluminationInformation,
                                        out m_intStartRegimarkCameraNum,
                                        out m_intEndRegimarkCameraNum,
                                        frmHinNoSelection.strProductName) == false)
            {
                return;
            }

            // 品名の表示
            txtProductName.Text = frmHinNoSelection.strProductName;
            txtOrderImg.Focus();
        }

        /// <summary>
        /// 作業者情報(社員番号)テキストボックスクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtWorker_Click(object sender, EventArgs e)
        {
            // 撮像装置部が処理中か確認する。
            if (!bolCheckBusyFile())
            {
                return;
            }

            TextBox txtWorker = (TextBox)sender;

            // 作業者選択画面表示
            WorkerSelection frmWorkerSelection = new WorkerSelection();
            frmWorkerSelection.ShowDialog(this);
            this.Visible = true;

            if (!string.IsNullOrEmpty(frmWorkerSelection.strEmployeeNum) || !string.IsNullOrEmpty(frmWorkerSelection.strWorkerName))
            {
                // 社員番号を変数に保持
                if (txtWorker == txtWorker1)
                {
                    m_strWorker1 = frmWorkerSelection.strEmployeeNum;
                }

                if (txtWorker == txtWorker2)
                {

                    m_strWorker2 = frmWorkerSelection.strEmployeeNum;
                }

                // 作業者名の表示
                txtWorker.Text = frmWorkerSelection.strWorkerName;
                txtWorker.SelectAll();
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


        /// <summary>
        /// 半透明フォーム
        /// </summary>
        public partial class OpacityForm : Form
        {
            // 子フォーム
            public Form m_form;

            #region メソッド
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="form"></param>
            public OpacityForm(Form form)
            {
                m_form = form;

                this.FormBorderStyle = FormBorderStyle.None;
                this.Load += new System.EventHandler(this.OpacityForm_Load);
                this.WindowState = FormWindowState.Maximized;
                this.ShowInTaskbar = false;
            }
            #endregion

            #region イベント
            /// <summary>
            /// フォームロード
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OpacityForm_Load(object sender, EventArgs e)
            {
                this.Opacity = 0.8;

                m_form.ShowDialog(this);
                this.Close();
            }
            #endregion
        }

        private List<char> lstBarcode = new List<char>(1);
        private int intLastInputKey = DateTime.Now.Millisecond;

        private void BeforeInspection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.NumLock)
            {
                if (this.ActiveControl == this.txtOrderImg ||
                    this.ActiveControl == this.txtFabricName)
                {

                    // 指図または反番のため、スキップ
                    return;
                }

                if (!txtOrderImg.Enabled)
                {
                    // 指図が無効化されているため、処理しない
                    return;
                }

                // バーコード入力のため、フォーカスを設定
                txtOrderImg.Text = string.Empty;
                txtFabricName.Text = string.Empty;
                txtOrderImg.Focus();
                txtOrderImg.SelectAll();

                lstBarcode.Clear();
            }

            if (e.KeyCode != Keys.Return)
            {
                // バーコード入力値として登録
                lstBarcode.Add((char)e.KeyData);
            }


            if (e.KeyCode == Keys.Return && lstBarcode.Count > 0)
            {

                // 入力が確定されたため、データをセット
                TextBox txtActive = (TextBox)this.ActiveControl;

                if (txtActive.Text.Length > 13)
                {
                    // 検反情報チェックシートのバーコードが読み込まれたため、分割して登録する。
                    String strOrderImg = txtActive.Text.Substring(0, 7);
                    String strFabricName = string.Format("{0}-{1}",
                        txtActive.Text.Substring(7, 6), txtActive.Text.Substring(13));

                    txtOrderImg.Text = strOrderImg;
                    txtFabricName.Text = strFabricName;
                }

                txtActive.SelectAll();

                lstBarcode.Clear();
            }

            intLastInputKey = DateTime.Now.Millisecond;
        }

        private void txtOrderImg_Enter(object sender, EventArgs e)
        {
            this.txtOrderImg.SelectAll();
        }

        private void txtFabricName_Enter(object sender, EventArgs e)
        {
            this.txtFabricName.SelectAll();
        }

        private void txtInspectionTargetLine_Enter(object sender, EventArgs e)
        {
            this.txtInspectionTargetLine.SelectAll();
        }

        private void txtInspectionStartLine_Enter(object sender, EventArgs e)
        {
            this.txtInspectionStartLine.SelectAll();
        }

        private void txtWorker1_Enter(object sender, EventArgs e)
        {
            this.txtWorker1.SelectAll();
        }

        private void txtWorker2_Enter(object sender, EventArgs e)
        {
            this.txtWorker2.SelectAll();
        }

        /// <summary>
        /// 設定ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSet_MouseClick(object sender, MouseEventArgs e)
        {
            int intInspectionNum = 0;
            string strEndDatetime = lblEndDatetime.Text;

            if (m_intStatus == g_clsSystemSettingInfo.intStatusEnd &&
                m_strStartDatetime.Equals(lblStartDatetime.Text))
            {
                // メッセージ出力
                MessageBox.Show(
                    string.Format(
                        g_clsMessageInfo.strMsgE0071,
                        lblTitleStartDatetime.Text),
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            // 検査番号の採番
            if (m_intStatus == g_clsSystemSettingInfo.intStatusChk)
            {
                intInspectionNum = m_intInspectionNum;
            }
            else
            {
                if (bolNumberInspectionNum(out intInspectionNum) == false)
                {
                    return;
                }
            }

            // 撮像装置部が処理中か確認する。
            if (!bolCheckBusyFile() ||
                (intInspectionNum != m_intInspectionNum &&
                !bolCheckFinalFile()))
            {
                return;
            }

            if (InputDataCheck() == false)
            {
                return;
            }

            // 終了時刻入力時に設定ボタン押下で検査番号が繰り上がる場合、
            // 画面上の終了時刻をクリアする
            if (intInspectionNum != m_intInspectionNum &&
                !string.IsNullOrEmpty(lblEndDatetime.Text))
            {
                lblEndDatetime.Text = string.Empty;
            }

            // 確認メッセージ出力
            DialogResult result = System.Windows.Forms.MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0008,
                                                                                     intInspectionNum,
                                                                                     txtProductName.Text,
                                                                                     txtFabricName.Text,
                                                                                     txtInspectionTargetLine.Text,
                                                                                     lblInspectionEndLine.Text,
                                                                                     txtInspectionStartLine.Text,
                                                                                     txtWorker1.Text,
                                                                                     txtWorker2.Text,
                                                                                     m_strInspectionDirection),
                                                                       "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                // キャンセルする際は、退避していた終了時刻を画面上に復元する
                lblEndDatetime.Text = strEndDatetime;
                return;
            }

            // 終了時刻が入力されている場合は検査終了処理を行う
            if (m_intStatus == g_clsSystemSettingInfo.intStatusChk &&
                !string.IsNullOrEmpty(lblEndDatetime.Text))
            {
                // finalファイルを削除する
                DeleteFinalFile();

                // 枝番の取得
                if (bolGetBranchNum(out m_intBranchNum, lblStartDatetime.Text.Substring(0, 10), m_intInspectionNum) == false)
                {
                    return;
                }

                // 検査情報ヘッダーの更新
                if (UpdEndInspectionInfoHeader(m_intInspectionNum, m_intBranchNum, lblEndDatetime.Text) == false)
                {
                    return;
                }

                // 検査番号の取得
                if (bolGetInspectionNum(out m_intInspectionNum) == false)
                {
                    return;
                }

                // ステータスの表示設定(検査終了)
                SetStatusCtrSetting(g_clsSystemSettingInfo.intStatusEnd);

                // 開始時刻の退避
                m_strStartDatetime = lblStartDatetime.Text;

                // 次の反番情報を設定
                btnNextFabric.Focus();

                return;
            }

            if (m_intStatus == g_clsSystemSettingInfo.intStatusBef ||
                m_intStatus == g_clsSystemSettingInfo.intStatusEnd)
            {
                // ステータス：検査開始前 or 終了から

                // 終了時刻
                lblEndDatetime.Text = string.Empty;

                // 検査番号の移送
                m_intInspectionNum = intInspectionNum;

                // 検査情報ヘッダーの登録
                if (RegStartInspectionInfoHeader(m_intInspectionNum, 1) == false)
                {
                    return;
                }

                // 検査番号の再取得
                if (!bolGetInspectionNum(out m_intInspectionNum))
                {
                    return;
                }

                // 検査番号の表示
                lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum);

                // ステータスの表示設定(検査準備完了)
                SetStatusCtrSetting(g_clsSystemSettingInfo.intStatusChk);

                // 検査対象数(行数)にフォーカスを設定
                txtInspectionTargetLine.Focus();
                return;
            }

            if (m_intStatus == g_clsSystemSettingInfo.intStatusChk)
            {
                // ステータス：検査準備完了から

                // 枝番の取得
                if (bolGetBranchNum(out m_intBranchNum, lblStartDatetime.Text.Substring(0, 10), m_intInspectionNum) == false)
                {
                    return;
                }

                // 検査情報ヘッダーの更新
                if (UpdEndInspectionInfoHeader(m_intInspectionNum, m_intBranchNum, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), false) == false)
                {
                    return;
                }

                // 枝番の採番
                m_intBranchNum++;

                // 検査情報ヘッダーの登録
                if (RegStartInspectionInfoHeader(m_intInspectionNum, m_intBranchNum) == false)
                {
                    return;
                }

                // ステータスの表示設定(検査準備完了)
                SetStatusCtrSetting(g_clsSystemSettingInfo.intStatusChk);

                // 次の反番情報を設定のフォーカスセット
                btnNextFabric.Focus();

                // 終了時刻
                lblEndDatetime.Text = string.Empty;
                return;
            }

            if (m_intStatus == g_clsSystemSettingInfo.intStatusStp)
            {
                // ステータス：検査中断から

                // 枝番の取得
                if (bolGetBranchNum(out m_intBranchNum, lblStartDatetime.Text.Substring(0, 10), m_intInspectionNum) == false)
                {
                    return;
                }

                // 終了時刻
                lblEndDatetime.Text = string.Empty;

                // 検査番号の移送
                m_intInspectionNum = intInspectionNum;

                // 検査情報ヘッダーの登録
                if (RegStartInspectionInfoHeader(m_intInspectionNum, 1) == false)
                {
                    return;
                }

                // 検査番号の再取得
                if (!bolGetInspectionNum(out m_intInspectionNum))
                {
                    return;
                }

                // 検査番号の表示
                lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum);

                // ステータスの表示設定(検査準備完了)
                SetStatusCtrSetting(g_clsSystemSettingInfo.intStatusChk);

                // 検査対象数(行数)のフォーカスセット
                txtInspectionTargetLine.Focus();
                return;
            }
        }

        /// <summary>
        /// 検査中断ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspectionStop_MouseClick(object sender, MouseEventArgs e)
        {
            // 撮像装置部が処理中か確認する。
            if (!bolCheckBusyFile())
            {
                return;
            }

            // 確認メッセージ出力
            DialogResult result = MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0008,
                                                                m_intInspectionNum,
                                                                txtProductName.Text,
                                                                txtFabricName.Text,
                                                                txtInspectionTargetLine.Text,
                                                                lblInspectionEndLine.Text,
                                                                txtInspectionStartLine.Text,
                                                                txtWorker1.Text,
                                                                txtWorker2.Text,
                                                                m_strInspectionDirection),
                                                  "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            // finalファイルを削除する
            DeleteFinalFile();

            // 終了日付の表示
            lblEndDatetime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // 枝番の取得
            if (bolGetBranchNum(out m_intBranchNum, lblStartDatetime.Text.Substring(0, 10), m_intInspectionNum) == false)
            {
                return;
            }

            // 検査情報ヘッダーの更新
            if (UpdEndInspectionInfoHeader(m_intInspectionNum, m_intBranchNum, lblEndDatetime.Text) == false)
            {
                return;
            }

            // ステータスの表示設定(検査中断)
            SetStatusCtrSetting(g_clsSystemSettingInfo.intStatusStp);

            // フォーカスを検査対象に設定
            txtInspectionTargetLine.Focus();

        }

        /// <summary>
        /// 次の反番情報を設定ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNextFabric_MouseClick(object sender, MouseEventArgs e)
        {
            // 撮像装置部が処理中か確認する。
            if (!bolCheckBusyFile() ||
                !bolCheckFinalFile())
            {
                return;
            }

            // 検査番号の取得
            bolGetInspectionNum(out m_intInspectionNum);

            // 値の初期化
            lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum.ToString());
            lblUnitNum.Text = string.Format(m_CON_FORMAT_UNIT_NUM, m_strUnitNum);
            txtProductName.Text = string.Empty;
            txtOrderImg.Text = string.Empty;
            txtFabricName.Text = string.Empty;
            txtInspectionTargetLine.Text = string.Empty;
            lblInspectionEndLine.Text = string.Empty;
            txtInspectionStartLine.Text = string.Empty;
            txtWorker1.Text = string.Empty;
            txtWorker2.Text = string.Empty;
            lblStartDatetime.Text = string.Empty;
            lblEndDatetime.Text = string.Empty;
            SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionS);

            // 変数の設定
            m_intBranchNum = 1;
            m_datInspectionDate = DateTime.Now.Date;

            // 品名にフォーカスを設定する
            txtProductName.Focus();

            // ステータスの表示設定(検査開始前)
            SetStatusCtrSetting(g_clsSystemSettingInfo.intStatusBef);

        }

        /// <summary>
        /// 開始時刻(現在時刻選択)ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartDatetime_MouseClick(object sender, MouseEventArgs e)
        {
            // 撮像装置部が処理中か確認する。
            if (!bolCheckBusyFile() ||
                !bolCheckFinalFile())
            {
                return;
            }

            lblStartDatetime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// 終了時刻(現在時刻選択)ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEndDatetime_MouseClick(object sender, MouseEventArgs e)
        {
            // 撮像装置部が処理中か確認する。
            if (!bolCheckBusyFile())
            {
                return;
            }

            // 終了時刻の表示
            lblEndDatetime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // 入力制御
            txtProductName.Enabled = false;
            txtOrderImg.Enabled = false;
            txtFabricName.Enabled = false;
            txtInspectionTargetLine.Enabled = false;
            txtInspectionStartLine.Enabled = false;
            txtWorker1.Enabled = false;
            txtWorker2.Enabled = false;
            btnStartDatetime.Enabled = false;
            btnEndDatetime.Enabled = true;
            btnInspectionDirectionS.Enabled = false;
            btnInspectionDirectionX.Enabled = false;
            btnInspectionDirectionY.Enabled = false;
            btnInspectionDirectionR.Enabled = false;
            btnInspectionStop.Enabled = false;
            btnSet.Enabled = true;
            btnNextFabric.Enabled = false;
        }

        /// <summary>
        /// 検査方向ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspectionDirection_MouseClick(object sender, MouseEventArgs e)
        {
            // 撮像装置部が処理中か確認する。
            if (!bolCheckBusyFile() ||
                !bolCheckFinalFile())
            {
                return;
            }

            Button btn = (Button)sender;

            if (btn == btnInspectionDirectionS)
            {
                SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionS);
            }

            if (btn == btnInspectionDirectionX)
            {
                SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionX);
            }

            if (btn == btnInspectionDirectionY)
            {
                SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionY);
            }

            if (btn == btnInspectionDirectionR)
            {
                SetInspectionDirectionSetting(g_clsSystemSettingInfo.strInspectionDirectionR);
            }
        }

        /// <summary>
        /// 初期表示イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BeforeInspection_Shown(object sender, EventArgs e)
        {
            this.Activated += new EventHandler(this.BeforeInspection_Activated);
            this.BeforeInspection_Activated(null, null);
        }

        /// <summary>
        /// フォームアクティブイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BeforeInspection_Activated(object sender, EventArgs e)
        {
            if (datCheckTime < DateTime.Now.AddMinutes(-1))
            {
                await Task.Delay(10);

                // 撮像装置部が処理中か確認する。
                bolCheckBusyFile();
            }
        }

        /// <summary>
        /// 撮像装置部連携ディレクトリ情報取得
        /// </summary>
        /// <returns>撮像装置部連携ディレクトリ情報</returns>
        private DirectoryInfo GetImagingDeviceCooperationDirectoryInfo()
        {
            return new DirectoryInfo(g_clsSystemSettingInfo.strImagingDeviceCooperationDirectory);
        }

        /// <summary>
        /// Busyファイル存在チェック
        /// </summary>
        /// <returns>処理続行フラグ</returns>
        private bool bolCheckBusyFile()
        {
            datCheckTime = DateTime.Now;
            DirectoryInfo diImagingDevice = GetImagingDeviceCooperationDirectoryInfo();

            // 撮像装置部が処理中(*.busyファイルが存在する)の場合、本処理をストップする。
            if (diImagingDevice.Exists &&
                diImagingDevice.GetFiles().Where(x => string.Compare(x.Extension, m_CON_EXTENSION_BUSY, true) == 0).Count() != 0)
            {
                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0069, true)).Show(this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finalファイル存在チェック
        /// </summary>
        /// <returns>処理続行フラグ</returns>
        private bool bolCheckFinalFile()
        {
            DirectoryInfo diImagingDevice = GetImagingDeviceCooperationDirectoryInfo();

            // 撮像装置部が最終行の検査中(*.finalファイルが存在する)の場合、本処理をストップする。
            if (diImagingDevice.Exists &&
                diImagingDevice.GetFiles().Where(x => string.Compare(x.Extension, m_CON_EXTENSION_FINAL, true) == 0).Count() != 0)
            {
                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0065, true)).Show(this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finalファイル削除
        /// </summary>
        private void DeleteFinalFile()
        {
            DirectoryInfo diImagingDevice = GetImagingDeviceCooperationDirectoryInfo();

            // *.finalファイルが存在する場合、削除する
            if (diImagingDevice.Exists)
            {
                foreach (string strPath in diImagingDevice.GetFiles().Where(x => string.Compare(x.Extension, m_CON_EXTENSION_FINAL, true) == 0).Select(x => x.FullName))
                {
                    File.Delete(strPath);
                }
            }
        }
    }
}