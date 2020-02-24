﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
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
        private int m_intIlluminationInformation = -1;  // 照度情報
        private int m_intStartRegimarkCameraNum = 0;    // 開始レジマークカメラ番号
        private int m_intEndRegimarkCameraNum = 0;      // 終了レジマークカメラ番号
        private string m_strWorker1 = string.Empty;               // 作業者情報(社員番号)検反部No.1
        private string m_strWorker2 = string.Empty;               // 作業者情報(社員番号)検反部No.2
        private string m_strInspectionDirection;        // 検査方向
        private DateTime m_datInspectionDate;           // 検査日付

        // 定数
        private const string m_CON_SW = "ＳＷ";
        private const string m_CON_EW = "ＥＷ";
        private const string m_CON_INSPECTION_IMAGE_S_PATH = @".\image\ABCDE_S.png";
        private const string m_CON_INSPECTION_IMAGE_X_PATH = @".\image\ABCDE_X.png";
        private const string m_CON_INSPECTION_IMAGE_Y_PATH = @".\image\ABCDE_Y.png";
        private const string m_CON_INSPECTION_IMAGE_R_PATH = @".\image\ABCDE_R.png";
        private const string m_CON_FORMAT_UNIT_NUM = "{0}号機";
        private const string m_CON_FORMAT_INSPECTION_NUM = "検査番号:{0}";

        // 検査方向背景色関連
        private Color m_clrInspectionDirectionActFore = System.Drawing.SystemColors.ActiveCaption;
        private Color m_clrInspectionDirectionActBack = SystemColors.Control;

        // ステータス背景色関連
        private int m_intStatus;
        private Color m_clrStatusActFore = System.Drawing.SystemColors.ControlText;
        private Color m_clrStatusActBack = System.Drawing.Color.White;
        private Color m_clrStatusNonActFore = System.Drawing.SystemColors.ControlLightLight;
        private Color m_clrStatusNonActBack = System.Drawing.Color.Transparent;

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
            TextBox txtBox = (TextBox)sender;

            // 入力用フォームの表示
            TenKeyInput frmTenKeyInput = new TenKeyInput(txtBox.MaxLength);
            frmTenKeyInput.ShowDialog(this);
            this.Visible = true;

            if (!string.IsNullOrEmpty(frmTenKeyInput.strInput))
            {
                txtBox.Text = frmTenKeyInput.strInput;
                txtBox.SelectAll();
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
                btnNextFabric.Enabled = false;
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
                txtInspectionStartLine.Enabled = false;
                txtWorker1.Enabled = true;
                txtWorker2.Enabled = true;
                btnStartDatetime.Enabled = false;
                btnEndDatetime.Enabled = true;
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

                // DBコミット
                g_clsConnectionNpgsql.DbCommit();

                // 撮像装置部へ連携用ファイル出力
                if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum) == false)
                {
                    btnSet.Focus();
                    return false;
                }

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
        /// <returns>true:正常終了 false:異常終了</returns>
        private Boolean UpdEndInspectionInfoHeader(int intInspectionNum, int intBranchNum, string strEndDateTime)
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

                // DBコミット
                g_clsConnectionNpgsql.DbCommit();

                // 撮像装置部へ連携用ファイル出力
                if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum) == false)
                {
                    // ロールバック
                    //g_clsConnectionNpgsql.DbRollback();
                    btnEndDatetime.Focus();
                    return false;
                }


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
            intIlluminationInformation = -1;
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
                    intIlluminationInformation = int.Parse(dtData.Rows[0]["illumination_information"].ToString());
                    intStartRegimarkCameraNum = int.Parse(dtData.Rows[0]["start_regimark_camera_num"].ToString());
                    intEndRegimarkCameraNum = int.Parse(dtData.Rows[0]["end_regimark_camera_num"].ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
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
        private bool bolOutFile(string strFabricName, int intInspectionNum, int intBranchNum)
        {
            StreamWriter sw = null;
            string strSQL = string.Empty;
            DataTable dtData = new DataTable();

            try
            {
                // txtファイルの作成
                sw = File.CreateText(g_clsSystemSettingInfo.strImagingDeviceCooperationDirectory + @"\" + strFabricName + "_" + intInspectionNum.ToString() + "_" + intBranchNum.ToString() + ".txt");

                // DBオープン
                strSQL = @"SELECT 
                               product_name
                             , order_img
                             , inspection_target_line
                             , inspection_end_line
                             , inspection_start_line
                             , worker_1
                             , worker_2
                             , TO_CHAR(start_datetime,'YYYY/MM/DD HH24:MI:SS') AS start_datetime
                             , TO_CHAR(end_datetime,'YYYY/MM/DD HH24:MI:SS') AS end_datetime
                             , inspection_direction 
                             , illumination_information 
                             , start_regimark_camera_num 
                             , end_regimark_camera_num 
                           FROM 
                               inspection_info_header 
                           WHERE TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd 
                             AND unit_num = :unit_num 
                             AND inspection_num = :inspection_num
                             AND branch_num = :branch_num";

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
                }
                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0036, Environment.NewLine, ex.Message));
                // メッセージ出力
                new OpacityForm(new ErrorMessageBox(g_clsMessageInfo.strMsgE0037)).ShowDialog(this);

                // DBロールバック
                g_clsConnectionNpgsql.DbRollback();

                return false;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
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
                        m_intIlluminationInformation = int.Parse(dtData.Rows[0]["illumination_information"].ToString());
                        m_intStartRegimarkCameraNum = int.Parse(dtData.Rows[0]["start_regimark_camera_num"].ToString());
                        m_intEndRegimarkCameraNum = int.Parse(dtData.Rows[0]["end_regimark_camera_num"].ToString());
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

        /// <summary>
        /// 検査対象数(行数)テキストボックスフォーカスアウト
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInspectionTargetLine_Leave(object sender, EventArgs e)
        {
            CalcMaxLine();
        }

        /// <summary>
        /// 検査開始行テキストボックスフォーカスアウト
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtKensaStartRow_Leave(object sender, EventArgs e)
        {
            CalcMaxLine();
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
            if (e.KeyCode == Keys.ControlKey)
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
            if (InputDataCheck() == false)
            {
                return;
            }

            // 確認メッセージ出力
            DialogResult result = System.Windows.Forms.MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0008,
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

            if (m_intStatus == g_clsSystemSettingInfo.intStatusBef ||
                m_intStatus == g_clsSystemSettingInfo.intStatusEnd)
            {
                // ステータス：検査開始前 or 終了から

                // 終了時刻
                lblEndDatetime.Text = string.Empty;

                // 検査番号の採番
                if (bolNumberInspectionNum(out m_intInspectionNum) == false)
                {
                    return;
                }


                // 検査情報ヘッダーの登録
                if (RegStartInspectionInfoHeader(m_intInspectionNum, 1) == false)
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
                if (UpdEndInspectionInfoHeader(m_intInspectionNum, m_intBranchNum, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")) == false)
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

                // 検査情報ヘッダーの更新
                if (UpdEndInspectionInfoHeader(m_intInspectionNum, m_intBranchNum, lblEndDatetime.Text) == false)
                {
                    return;
                }

                // 終了時刻
                lblEndDatetime.Text = string.Empty;

                // 検査番号の採番
                if (bolNumberInspectionNum(out m_intInspectionNum) == false)
                {
                    return;
                }

                // 検査情報ヘッダーの登録
                if (RegStartInspectionInfoHeader(m_intInspectionNum, m_intBranchNum) == false)
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
            lblStartDatetime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// 終了時刻(現在時刻選択)ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEndDatetime_MouseClick(object sender, MouseEventArgs e)
        {
            // 終了時刻の表示
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

            // 検査番号の取得
            if (bolGetInspectionNum(out m_intInspectionNum) == false)
            {
                return;
            }

            // 検査番号の表示
            lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum);

            // ステータスの表示設定(検査終了)
            SetStatusCtrSetting(g_clsSystemSettingInfo.intStatusEnd);

            // 次の反番情報を設定
            btnNextFabric.Focus();

        }

        /// <summary>
        /// 検査方向ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspectionDirection_MouseClick(object sender, MouseEventArgs e)
        {
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

    }
}
