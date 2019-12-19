using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static BeforeInspection.Common;

namespace BeforeInspection
{
    public partial class BeforeInspection : Form
    {
        // 変数
        private DataTable m_dtData;                                 // 抽出データ
        private string m_strImagingDeviceCooperationDirectory = ""; // 撮像装置部連携ディレクトリパス

        // パラメータ変数
        private int m_intInspectionNum = 0;             // 検査番号
        private int m_intBranchNum = 0;                 // 枝番
        private string m_strUnitNum = "";               // 号機
        private int m_intIlluminationInformation = -1;  // 照度情報
        private int m_intStartRegimarkCameraNum = 0;    // 開始レジマークカメラ番号
        private int m_intEndRegimarkCameraNum = 0;      // 終了レジマークカメラ番号
        private string m_strWorker1 = "";               // 作業者情報(社員番号)検反部No.1
        private string m_strWorker2 = "";               // 作業者情報(社員番号)検反部No.2
        private string m_strInspectionDirection;        // 検査方向

        // ヘッダ部関連
        private const string m_CON_FORMAT_UNIT_NUM = "{0}号機";
        private const string m_CON_FORMAT_INSPECTION_NUM = "検査番号:{0}";

        // 検査方向関連
        private const string m_CON_INSPECTION_DIRECTION_S = "S";
        private const string m_CON_INSPECTION_DIRECTION_X = "X";
        private const string m_CON_INSPECTION_DIRECTION_Y = "Y";
        private const string m_CON_INSPECTION_DIRECTION_R = "R";
        private Color m_clrInspectionDirectionActFore = System.Drawing.SystemColors.ActiveCaption;
        private Color m_clrInspectionDirectionActBack = SystemColors.Control;
        private const string m_CON_SEW_S = "ＳＷ";
        private const string m_CON_SEW_E = "ＥＷ";
        private const string m_CON_INSPECTION_IMAGE_S_PATH = @".\ABCDE_S.png";
        private const string m_CON_INSPECTION_IMAGE_X_PATH = @".\ABCDE_X.png";
        private const string m_CON_INSPECTION_IMAGE_Y_PATH = @".\ABCDE_Y.png";
        private const string m_CON_INSPECTION_IMAGE_R_PATH = @".\ABCDE_R.png";

        // ステータス関連
        private int m_intStatus;
        private const int m_CON_STATUS_BEF = 1;     //検査開始前
        private const int m_CON_STATUS_CHK = 2;     //検査準備完了
        private const int m_CON_STATUS_STP = 3;     //検査中断
        private const int m_CON_STATUS_END = 4;     //検査終了
        private Color m_clrStatusActFore = System.Drawing.SystemColors.ControlText;
        private Color m_clrStatusActBack = System.Drawing.Color.White;
        private Color m_clrStatusNonActFore = System.Drawing.SystemColors.ControlLightLight;
        private Color m_clrStatusNonActBack = System.Drawing.Color.Transparent;
        private void MyForm_CloseOnStart(object sender, EventArgs e)
        {
            this.Visible = false;
            this.Close();
        }

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BeforeInspection()
        {
            string strSQL = "";
            NpgsqlCommand NpgsqlCom = null;
            NpgsqlDataAdapter NpgsqlDtAd = null;

            this.StartPosition = FormStartPosition.CenterScreen;
            this.Visible = false;

            try
            {
                DateTime datNow = DateTime.Now;

                // システム設定から情報を取得
                //  号機
                m_strUnitNum = strGetAppConfigValue("UnitNum");
                if (m_strUnitNum == null)
                {
                    this.Load += (s, e) => Close();
                    return;
                }
                //  撮像装置部連携ディレクトリパス
                if (bolGetSystemSettingValue("ImagingDeviceCooperationDirectory", out m_strImagingDeviceCooperationDirectory) == false)
                {
                    this.Load += (s, e) => Close();
                    return;
                }
                else
                {
                    // 存在チェック
                    if (Directory.Exists(m_strImagingDeviceCooperationDirectory) == false)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, "撮像装置部連携ディレクトリ[ " + m_strImagingDeviceCooperationDirectory + "]は存在しません。");
                        // メッセージ出力
                        MessageBox.Show("撮像装置部連携ディレクトリ[ " + m_strImagingDeviceCooperationDirectory + "]は存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // 画面生成前に閉じる
                        this.Load += (s, e) => Close();
                        return;
                    }
                }

                try
                {
                    // DBオープン
                    DbOpen();

                    NpgsqlCom = null;
                    NpgsqlDtAd = null;
                    m_dtData = new DataTable();
                    strSQL = @"SELECT COALESCE(MAX(inspection_num),0) AS inspection_num ";
                    strSQL += @"FROM  inspection_info_header ";
                    strSQL += @"WHERE (TO_CHAR(inspection_date,'YYYY/MM/DD') = '" + datNow.ToString("yyyy/MM/dd") + @"' OR ";
                    strSQL += @"       TO_CHAR(inspection_date,'YYYY/MM/DD') = '" + datNow.AddDays(-1).ToString("yyyy/MM/dd") + @"') ";
                    strSQL += @"AND   unit_num = '" + m_strUnitNum + @"' ";
                    strSQL += @"AND   end_datetime IS NULL ";
                    NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                    NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                    NpgsqlDtAd.Fill(m_dtData);

                    //  検査番号
                    m_intInspectionNum = int.Parse(m_dtData.Rows[0]["inspection_num"].ToString());
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                    // メッセージ出力
                    MessageBox.Show("検査情報ヘッダーの取得で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // 画面生成前に閉じる
                    this.Load += (s, e) => Close();
                    return;
                }

                if (m_intInspectionNum == 0)
                {
                    // 検査番号の取得
                    if (bolGetInspectionNum(out m_intInspectionNum, DateTime.Now.ToString("yyyy/MM/dd")) == false)
                    {
                        // 画面生成前に閉じる
                        this.Load += (s, e) => Close();
                        return;
                    }

                    InitializeComponent();

                    // 値の初期化
                    lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum.ToString());
                    lblUnitNum.Text = string.Format(m_CON_FORMAT_UNIT_NUM, m_strUnitNum);
                    txtProductName.Text = "";
                    txtOrderImg.Text = "";
                    txtFabricName.Text = "";
                    txtInspectionTargetLine.Text = "";
                    lblInspectionEndLine.Text = "";
                    txtInspectionStartLine.Text = "";
                    txtWorker1.Text = "";
                    txtWorker2.Text = "";
                    lblStartDatetime.Text = "";
                    lblEndDatetime.Text = "";
                    SetInspectionDirectionSetting(m_CON_INSPECTION_DIRECTION_S);

                    // 変数の設定
                    m_intBranchNum = 1;

                    // 品名にフォーカスを設定する
                    txtProductName.Focus();

                    // ステータスの表示設定(検査開始前)
                    SetStatusCtrSetting(m_CON_STATUS_BEF);
                }
                else
                {
                    try
                    {
                        // DBオープン
                        DbOpen();

                        NpgsqlCom = null;
                        NpgsqlDtAd = null;
                        m_dtData = new DataTable();
                        strSQL = @"SELECT ";
                        strSQL += @"    branch_num";
                        strSQL += @"  , product_name";
                        strSQL += @"  , order_img";
                        strSQL += @"  , fabric_name";
                        strSQL += @"  , inspection_target_line";
                        strSQL += @"  , inspection_end_line";
                        strSQL += @"  , inspection_start_line";
                        strSQL += @"  , worker_1";
                        strSQL += @"  , worker_2";
                        strSQL += @"  , TO_CHAR(start_datetime,'YYYY/MM/DD HH24:MI:SS') AS start_datetime";
                        strSQL += @"  , inspection_direction ";
                        strSQL += @"  , TO_CHAR(inspection_date,'YYYY/MM/DD') AS inspection_date ";
                        strSQL += @"FROM ";
                        strSQL += @"    inspection_info_header MAIN ";
                        strSQL += @"WHERE (TO_CHAR(inspection_date,'YYYY/MM/DD') = '" + datNow.ToString("yyyy/MM/dd") + @"' OR ";
                        strSQL += @"       TO_CHAR(inspection_date,'YYYY/MM/DD') = '" + datNow.AddDays(-1).ToString("yyyy/MM/dd") + @"') ";
                        strSQL += @"  AND unit_num = '" + m_strUnitNum + @"' ";
                        strSQL += @"  AND end_datetime IS NULL ";
                        strSQL += @"  AND inspection_num = " + m_intInspectionNum;
                        NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                        NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                        NpgsqlDtAd.Fill(m_dtData);

                        InitializeComponent();

                        // 値の初期化
                        lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum.ToString());
                        lblUnitNum.Text = string.Format(m_CON_FORMAT_UNIT_NUM, m_strUnitNum);
                        txtProductName.Text = m_dtData.Rows[0]["product_name"].ToString();
                        txtOrderImg.Text = m_dtData.Rows[0]["order_img"].ToString();
                        txtFabricName.Text = m_dtData.Rows[0]["fabric_name"].ToString();
                        txtInspectionTargetLine.Text = m_dtData.Rows[0]["inspection_target_line"].ToString();
                        lblInspectionEndLine.Text = m_dtData.Rows[0]["inspection_end_line"].ToString();
                        txtInspectionStartLine.Text = m_dtData.Rows[0]["inspection_start_line"].ToString();
                        //  作業者名の表示
                        string strWorkerName = "";
                        if (bolGetWorkerName(out strWorkerName, m_dtData.Rows[0]["worker_1"].ToString()) == false)
                        {
                            // 画面生成前に閉じる
                            this.Load += (s, e) => Close();
                            return;
                        }
                        txtWorker1.Text = strWorkerName;
                        if (bolGetWorkerName(out strWorkerName, m_dtData.Rows[0]["worker_2"].ToString()) == false)
                        {
                            // 画面生成前に閉じる
                            this.Load += (s, e) => Close();
                            return;
                        }
                        txtWorker2.Text = strWorkerName;
                        lblStartDatetime.Text = m_dtData.Rows[0]["start_datetime"].ToString();
                        lblEndDatetime.Text = "";
                        SetInspectionDirectionSetting(m_dtData.Rows[0]["inspection_direction"].ToString());

                        // 変数の設定
                        //  品番登録情報の取得(照度情報,開始レジマークカメラ番号,終了レジマークカメラ番号)
                        if (bolGetMstProductInfo(out m_intIlluminationInformation,
                                                 out m_intStartRegimarkCameraNum,
                                                 out m_intEndRegimarkCameraNum,
                                                 txtProductName.Text) == false)
                        {
                            // 画面生成前に閉じる
                            this.Load += (s, e) => Close();
                            return;
                        }
                        m_intBranchNum = int.Parse(m_dtData.Rows[0]["branch_num"].ToString());
                        m_strWorker1 = m_dtData.Rows[0]["worker_1"].ToString();
                        m_strWorker2 = m_dtData.Rows[0]["worker_2"].ToString();
                    }
                    catch (Exception ex)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                        // メッセージ出力
                        MessageBox.Show("検査情報ヘッダーの取得で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // 画面生成前に閉じる
                        this.Load += (s, e) => Close();
                        return;
                    }

                    // 検査対象数(行数)にフォーカスを設定する
                    txtInspectionTargetLine.Focus();

                    // ステータスの表示設定(検査準備完了)
                    SetStatusCtrSetting(m_CON_STATUS_CHK);
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "画面初期表示時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("画面初期表示処理で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 画面生成前に閉じる
                this.Load += (s, e) => Close();
                return;
            }
        }

        /// <summary>
        /// 入力フォーム表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispInputForm(object sender, EventArgs e)
        {
            TextBox txtBox = (TextBox)sender;

            // 入力用フォームの表示
            InputForm frmInputForm = new InputForm(txtBox.MaxLength);
            frmInputForm.ShowDialog(this);
            this.Visible = true;

            if (frmInputForm.strInput != null)
                txtBox.Text = frmInputForm.strInput;
        }

        /// <summary>
        /// 最終行の計算
        /// </summary>
        private void CalcMaxLine()
        {
            //string strInspectionEndLineBk = txtInspectionTargetLine.Text;

            // 最終行計算
            if (txtInspectionTargetLine.Text != "" && txtInspectionStartLine.Text != "")
            {
                if (m_strInspectionDirection == m_CON_INSPECTION_DIRECTION_S || m_strInspectionDirection == m_CON_INSPECTION_DIRECTION_X)
                {
                    lblInspectionEndLine.Text = (int.Parse(txtInspectionStartLine.Text) + int.Parse(txtInspectionTargetLine.Text) - 1).ToString();
                }
                else if (m_strInspectionDirection == m_CON_INSPECTION_DIRECTION_Y || m_strInspectionDirection == m_CON_INSPECTION_DIRECTION_R)
                {
                    lblInspectionEndLine.Text = (int.Parse(txtInspectionStartLine.Text) - int.Parse(txtInspectionTargetLine.Text) + 1).ToString();
                }
            }
            else
            {
                lblInspectionEndLine.Text = "";
            }

            // 整合性チェック
            if (lblInspectionEndLine.Text != "" && int.Parse(lblInspectionEndLine.Text) <= 0)
            {
                // 不正の場合はメッセージを表示
                lblInspectionEndLine.ForeColor = Color.Red;
                MessageBox.Show("検査対象数が不正です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // フォーカスセット
                txtInspectionTargetLine.Focus();
            }
            else
            {
                lblInspectionEndLine.ForeColor = System.Drawing.SystemColors.ControlText;
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
                if ((m_intStatus == m_CON_STATUS_BEF && lblStatus == lblStatusBef) ||
                    (m_intStatus == m_CON_STATUS_CHK && lblStatus == lblStatusChk) ||
                    (m_intStatus == m_CON_STATUS_STP && lblStatus == lblStatusStp) ||
                    (m_intStatus == m_CON_STATUS_END && lblStatus == lblStatusEnd))
                    continue;

                lblStatus.ForeColor = m_clrStatusNonActFore;
                lblStatus.BackColor = m_clrStatusNonActBack;
            }

            // アクティブ化
            switch (m_intStatus)
            {
                case m_CON_STATUS_BEF:
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
                    btnStartDate.Enabled = true;
                    btnEndDate.Enabled = false;
                    btnInspectionDirection_S.Enabled = true;
                    btnInspectionDirection_X.Enabled = true;
                    btnInspectionDirection_Y.Enabled = true;
                    btnInspectionDirection_R.Enabled = true;
                    btnTyudan.Enabled = false;
                    btnSettei.Enabled = true;
                    btnTanSet.Enabled = false;
                    break;
                case m_CON_STATUS_CHK:
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
                    btnStartDate.Enabled = false;
                    btnEndDate.Enabled = true;
                    btnInspectionDirection_S.Enabled = false;
                    btnInspectionDirection_X.Enabled = false;
                    btnInspectionDirection_Y.Enabled = false;
                    btnInspectionDirection_R.Enabled = false;
                    btnTyudan.Enabled = true;
                    btnSettei.Enabled = true;
                    btnTanSet.Enabled = false;
                    break;
                case m_CON_STATUS_STP:
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
                    btnStartDate.Enabled = true;
                    btnEndDate.Enabled = false;
                    btnInspectionDirection_S.Enabled = false;
                    btnInspectionDirection_X.Enabled = false;
                    btnInspectionDirection_Y.Enabled = false;
                    btnInspectionDirection_R.Enabled = false;
                    btnTyudan.Enabled = false;
                    btnSettei.Enabled = true;
                    btnTanSet.Enabled = false;
                    break;
                case m_CON_STATUS_END:
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
                    btnStartDate.Enabled = false;
                    btnEndDate.Enabled = true;
                    btnInspectionDirection_S.Enabled = false;
                    btnInspectionDirection_X.Enabled = false;
                    btnInspectionDirection_Y.Enabled = false;
                    btnInspectionDirection_R.Enabled = false;
                    btnTyudan.Enabled = false;
                    btnSettei.Enabled = true;
                    btnTanSet.Enabled = true;
                    break;
            }

        }

        /// <summary>
        /// 検査方向設定ボタン(画面)をセット
        /// </summary>
        /// <param name="strInspectionDirection">検査方向（S/X/Y/R）</param>
        private void SetInspectionDirectionSetting(string strInspectionDirection)
        {
            // 非アクティブ化
            foreach (Button btnInspectionDirection in new Button[] { btnInspectionDirection_S, btnInspectionDirection_X, btnInspectionDirection_Y, btnInspectionDirection_R })
            {
                if ((strInspectionDirection == m_CON_INSPECTION_DIRECTION_S && btnInspectionDirection != btnInspectionDirection_S) ||
                    (strInspectionDirection == m_CON_INSPECTION_DIRECTION_X && btnInspectionDirection != btnInspectionDirection_X) ||
                    (strInspectionDirection == m_CON_INSPECTION_DIRECTION_Y && btnInspectionDirection != btnInspectionDirection_Y) ||
                    (strInspectionDirection == m_CON_INSPECTION_DIRECTION_R && btnInspectionDirection != btnInspectionDirection_R))
                    btnInspectionDirection.BackColor = m_clrInspectionDirectionActBack;
            }

            // アクティブ化
            switch (strInspectionDirection)
            {
                case m_CON_INSPECTION_DIRECTION_S:
                    // S
                    picInspectionDirection_Front.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_S_PATH);
                    lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                    lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                    picInspectionDirection_Back.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_X_PATH);
                    lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                    lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                    btnInspectionDirection_S.BackColor = m_clrInspectionDirectionActFore;

                    m_strInspectionDirection = m_CON_INSPECTION_DIRECTION_S;
                    
                    // 最終行の計算
                    CalcMaxLine();

                    break;
                case m_CON_INSPECTION_DIRECTION_X:
                    // X
                    picInspectionDirection_Front.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_X_PATH);
                    lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                    lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                    picInspectionDirection_Back.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_S_PATH);
                    lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                    lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                    btnInspectionDirection_X.BackColor = m_clrInspectionDirectionActFore;

                    m_strInspectionDirection = m_CON_INSPECTION_DIRECTION_X;

                    // 最終行の計算
                    CalcMaxLine();

                    break;
                case m_CON_INSPECTION_DIRECTION_Y:
                    // Y
                    picInspectionDirection_Front.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_Y_PATH);
                    lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                    lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                    picInspectionDirection_Back.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_R_PATH);
                    lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                    lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                    btnInspectionDirection_Y.BackColor = m_clrInspectionDirectionActFore;

                    m_strInspectionDirection = m_CON_INSPECTION_DIRECTION_Y;

                    // 最終行の計算
                    CalcMaxLine();

                    break;
                case m_CON_INSPECTION_DIRECTION_R:
                    // R
                    picInspectionDirection_Front.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_R_PATH);
                    lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                    lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                    picInspectionDirection_Back.Image = System.Drawing.Image.FromFile(m_CON_INSPECTION_IMAGE_Y_PATH);
                    lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                    lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                    btnInspectionDirection_R.BackColor = m_clrInspectionDirectionActFore;

                    m_strInspectionDirection = m_CON_INSPECTION_DIRECTION_R;

                    // 最終行の計算
                    CalcMaxLine();

                    break;
            }
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

            // 開始時刻入力チェック
            if (lblStartDatetime.Text == "")
            {
                MessageBox.Show("開始時刻は必須入力の項目です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStartDate.Focus();
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
            if (tbxCheckData.Text == "")
            {
                MessageBox.Show(strItemName + "は必須入力の項目です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // DBオープン
                DbOpen();
                // DBトランザクション開始
                DbBeginTran();

                // SQL文を作成する
                string strCreateSql = @"INSERT INTO inspection_info_header(
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
                NpgsqlCommand command = new NpgsqlCommand(strCreateSql, NpgsqlCon, NpgsqlTran);

                command.Parameters.Add(new NpgsqlParameter("inspection_num", DbType.Int32) { Value = intInspectionNum });
                command.Parameters.Add(new NpgsqlParameter("branch_num", DbType.Int16) { Value = intBranchNum });
                command.Parameters.Add(new NpgsqlParameter("product_name", DbType.String) { Value = txtProductName.Text });
                command.Parameters.Add(new NpgsqlParameter("unit_num", DbType.String) { Value = m_strUnitNum });
                command.Parameters.Add(new NpgsqlParameter("order_img", DbType.String) { Value = txtOrderImg.Text });
                command.Parameters.Add(new NpgsqlParameter("fabric_name", DbType.String) { Value = txtFabricName.Text });
                command.Parameters.Add(new NpgsqlParameter("inspection_target_line", DbType.Int16) { Value = int.Parse(txtInspectionTargetLine.Text) });
                command.Parameters.Add(new NpgsqlParameter("inspection_end_line", DbType.Int16) { Value = int.Parse(lblInspectionEndLine.Text) });
                command.Parameters.Add(new NpgsqlParameter("inspection_start_line", DbType.Int16) { Value = int.Parse(txtInspectionStartLine.Text) });
                command.Parameters.Add(new NpgsqlParameter("worker_1", DbType.String) { Value = m_strWorker1 });
                command.Parameters.Add(new NpgsqlParameter("worker_2", DbType.String) { Value = m_strWorker2 });
                command.Parameters.Add(new NpgsqlParameter("start_datetime", DbType.String) { Value = lblStartDatetime.Text });
                command.Parameters.Add(new NpgsqlParameter("inspection_direction", DbType.String) { Value = m_strInspectionDirection });
                command.Parameters.Add(new NpgsqlParameter("inspection_date", DbType.String) { Value = lblStartDatetime.Text.Substring(0, 10) });
                command.Parameters.Add(new NpgsqlParameter("illumination_information", DbType.Int16) { Value = m_intIlluminationInformation});
                command.Parameters.Add(new NpgsqlParameter("start_regimark_camera_num", DbType.Int16) { Value = m_intStartRegimarkCameraNum });
                command.Parameters.Add(new NpgsqlParameter("end_regimark_camera_num", DbType.Int16) { Value = m_intEndRegimarkCameraNum });

                // sqlを実行する
                if (ExecTranSQL(command) == false)
                    return false;

                return true;

            }
            catch (Exception ex)
            {
                // ロールバック
                DbRollback();

                MessageBox.Show("検査情報ヘッダーの登録時にエラーが発生しました。" + Environment.NewLine + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 設定を設定
                btnSettei.Focus();

                return false;
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
            try
            {
                // DBオープン
                DbOpen();
                // DBトランザクション開始
                DbBeginTran();

                // SQL文を作成する
                string strCreateSql = @"UPDATE inspection_info_header
                                           SET end_datetime = TO_TIMESTAMP(:end_datetime,'YYYY/MM/DD HH24:MI:SS')
                                         WHERE inspection_num = :inspection_num
                                           AND branch_num = :branch_num
                                           AND unit_num = :unit_num
                                           AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date ;";

                // SQLコマンドに各パラメータを設定する
                NpgsqlCommand command = new NpgsqlCommand(strCreateSql, NpgsqlCon, NpgsqlTran);

                command.Parameters.Add(new NpgsqlParameter("inspection_num", DbType.Int32) { Value = intInspectionNum });
                command.Parameters.Add(new NpgsqlParameter("branch_num", DbType.Int16) { Value = intBranchNum });
                command.Parameters.Add(new NpgsqlParameter("unit_num", DbType.String) { Value = m_strUnitNum });
                command.Parameters.Add(new NpgsqlParameter("inspection_date", DbType.String) { Value = lblStartDatetime.Text.Substring(0, 10) });
                command.Parameters.Add(new NpgsqlParameter("end_datetime", DbType.String) { Value = strEndDateTime });

                // sqlを実行する
                if (ExecTranSQL(command) == false)
                    return false;

                return true;

            }
            catch (Exception ex)
            {
                // ロールバック
                DbRollback();

                MessageBox.Show("検査情報ヘッダーの更新時にエラーが発生しました。" + Environment.NewLine + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                btnEndDate.Focus();

                return false;

            }
        }

        /// <summary>
        /// 検査番号の取得
        /// </summary>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private bool bolGetInspectionNum(out int intInspectionNum, string strInspectionDate)
        {
            // 初期化
            intInspectionNum = 0;
            string strSQL = "";
            DataTable dtData;

            try
            {
                // DBオープン
                DbOpen();

                NpgsqlCommand NpgsqlCom = null;
                NpgsqlDataAdapter NpgsqlDtAd = null;
                dtData = new DataTable();

                strSQL = @"SELECT COALESCE(MAX(inspection_num),0) AS inspection_num ";
                strSQL += @"FROM  inspection_info_header ";
                strSQL += @"WHERE TO_CHAR(inspection_date,'YYYY/MM/DD') = '" + strInspectionDate + @"' ";
                strSQL += @"AND   unit_num = '" + m_strUnitNum + @"' ";
                NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                NpgsqlDtAd.Fill(dtData);

                //  検査番号
                if (dtData.Rows.Count > 0)
                {
                    intInspectionNum = int.Parse(dtData.Rows[0]["inspection_num"].ToString());
                }

                // 枝番のリセット
                m_intBranchNum = 1;

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("検査情報ヘッダーの取得で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// 検査番号の採番
        /// </summary>
        /// <param name="intInspectionNum"></param>
        /// <param name="strInspectionDate"></param>
        /// <returns></returns>
        private bool bolNumberInspectionNum(out int intInspectionNum, string strInspectionDate)
        {
            if (bolGetInspectionNum(out intInspectionNum, strInspectionDate) == true)
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
            intBranchNum = 0;

            try
            {
                string strSQL = "";
                DataTable dtData;

                // DBオープン
                DbOpen();

                NpgsqlCommand NpgsqlCom = null;
                NpgsqlDataAdapter NpgsqlDtAd = null;
                dtData = new DataTable();
                strSQL = @"SELECT COALESCE(MAX(branch_num),0) AS branch_num ";
                strSQL += @"FROM  inspection_info_header ";
                strSQL += @"WHERE TO_CHAR(inspection_date,'YYYY/MM/DD') = '" + strInspectionDate + @"' ";
                strSQL += @"AND   unit_num = '" + m_strUnitNum + @"' ";
                strSQL += @"AND   inspection_num = " + intInspectionNum.ToString();
                NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                NpgsqlDtAd.Fill(dtData);

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
                WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("検査情報ヘッダーの取得で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// 作業者名の取得
        /// </summary>
        /// <param name="strWorkerName"></param>
        /// <param name="strEmployeeNum"></param>
        /// <returns></returns>
        private bool bolGetWorkerName(out string strWorkerName, string strEmployeeNum)
        {
            // 初期化
            strWorkerName = "";

            try
            {
                string strSQL = "";
                DataTable dtData;

                // DBオープン
                DbOpen();

                NpgsqlCommand NpgsqlCom = null;
                NpgsqlDataAdapter NpgsqlDtAd = null;
                dtData = new DataTable();
                strSQL = @"SELECT worker_name_sei || worker_name_mei AS worker_name ";
                strSQL += @"FROM  mst_worker ";
                strSQL += @"WHERE employee_num = '" + strEmployeeNum + @"' ";
                NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                NpgsqlDtAd.Fill(dtData);

                //  枝番
                if (dtData.Rows.Count > 0)
                {
                    strWorkerName = dtData.Rows[0]["worker_name"].ToString();
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("作業者マスタの取得で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

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

            try
            {
                string strSQL = "";
                DataTable dtData;

                // DBオープン
                DbOpen();

                NpgsqlCommand NpgsqlCom = null;
                NpgsqlDataAdapter NpgsqlDtAd = null;
                dtData = new DataTable();
                strSQL = @"SELECT ";
                strSQL += @"    illumination_information";
                strSQL += @"  , start_regimark_camera_num";
                strSQL += @"  , end_regimark_camera_num ";
                strSQL += @"FROM ";
                strSQL += @"    mst_product_info ";
                strSQL += @"WHERE product_name = '" + strProductName + @"' ";
                NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                NpgsqlDtAd.Fill(dtData);

                //  枝番
                if (dtData.Rows.Count > 0)
                {
                    intIlluminationInformation = int.Parse(dtData.Rows[0]["illumination_information"].ToString());
                    intStartRegimarkCameraNum = int.Parse(dtData.Rows[0]["start_regimark_camera_num"].ToString());
                    intIlluminationInformation = int.Parse(dtData.Rows[0]["end_regimark_camera_num"].ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("品種登録情報の取得で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
            string strSQL = "";
            DataTable dtData = new DataTable();

            try
            {
                // 撮像装置部連携ディレクトリの存在チェック
                if (Directory.Exists(m_strImagingDeviceCooperationDirectory) == false)
                {
                    // ロールバック
                    DbRollback();

                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, "撮像装置部連携ディレクトリ[ " + m_strImagingDeviceCooperationDirectory + "]は存在しません。");
                    // メッセージ出力
                    MessageBox.Show("撮像装置部連携ディレクトリ[ " + m_strImagingDeviceCooperationDirectory + "]は存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return false;
                }

                // txtファイルの作成
                sw = File.CreateText(m_strImagingDeviceCooperationDirectory + @"\" + strFabricName + "_" + intInspectionNum.ToString() + "_" + intBranchNum.ToString() + ".txt");

                // DBオープン
                DbOpen();

                NpgsqlCommand NpgsqlCom = null;
                NpgsqlDataAdapter NpgsqlDtAd = null;
                m_dtData = new DataTable();
                strSQL = @"SELECT ";
                strSQL += @"    product_name";
                strSQL += @"  , order_img";
                strSQL += @"  , inspection_target_line";
                strSQL += @"  , inspection_end_line";
                strSQL += @"  , inspection_start_line";
                strSQL += @"  , worker_1";
                strSQL += @"  , worker_2";
                strSQL += @"  , TO_CHAR(start_datetime,'YYYY/MM/DD HH24:MI:SS') AS start_datetime";
                strSQL += @"  , TO_CHAR(end_datetime,'YYYY/MM/DD HH24:MI:SS') AS end_datetime";
                strSQL += @"  , inspection_direction ";
                strSQL += @"  , TO_CHAR(inspection_date,'YYYY/MM/DD') AS inspection_date ";
                strSQL += @"  , illumination_information ";
                strSQL += @"  , start_regimark_camera_num ";
                strSQL += @"  , end_regimark_camera_num ";
                strSQL += @"FROM ";
                strSQL += @"    inspection_info_header ";
                strSQL += @"WHERE TO_CHAR(inspection_date,'YYYY/MM/DD') = '" + lblStartDatetime.Text.Substring(0, 10) + @"' ";
                strSQL += @"  AND unit_num = '" + m_strUnitNum + @"' ";
                strSQL += @"  AND inspection_num = " + intInspectionNum;
                strSQL += @"  AND branch_num = " + intBranchNum;
                NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                NpgsqlDtAd.Fill(dtData);

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
                    sw.WriteLine(dtData.Rows[0]["inspection_date"].ToString());
                    sw.WriteLine(dtData.Rows[0]["illumination_information"].ToString());
                    sw.WriteLine(dtData.Rows[0]["start_regimark_camera_num"].ToString());
                    sw.WriteLine(dtData.Rows[0]["end_regimark_camera_num"].ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                // ロールバック
                DbRollback();

                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "検査情報ヘッダファイルの出力でエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("検査情報ヘッダファイルの出力で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
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
        }

        /// <summary>
        /// 開始時刻(現在時刻選択)ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartDate_Click(object sender, EventArgs e)
        {
            lblStartDatetime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// 終了時刻(現在時刻選択)ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEndDate_Click(object sender, EventArgs e)
        {
            // 終了時刻の表示
            lblEndDatetime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // 枝番の取得
            if (bolGetBranchNum(out m_intBranchNum, lblStartDatetime.Text.Substring(0, 10), m_intInspectionNum) == false)
                return;

            // 検査情報ヘッダーの更新
            if (UpdEndInspectionInfoHeader(m_intInspectionNum, m_intBranchNum, lblEndDatetime.Text) == false)
                return;

            // 撮像装置部へ連携用ファイル出力
            if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum) == false)
                return;

            // DBコミット
            DbCommit();

            // 検査番号の取得
            if (bolGetInspectionNum(out m_intInspectionNum, DateTime.Now.ToString("yyyy/MM/dd")) == false)
                return;
            // 検査番号の表示
            lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum);

            // ステータスの表示設定(検査終了)
            SetStatusCtrSetting(m_CON_STATUS_END);

            // 次の反番情報を設定
            btnTanSet.Focus();
        }

        /// <summary>
        /// 品名テキストボックスクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtHinNo_Click(object sender, EventArgs e)
        {
            // 品番選択画面
            HinNoSelection frmHinNoSelection = new HinNoSelection();
            frmHinNoSelection.ShowDialog(this);
            if (frmHinNoSelection.strProductName != null)
            {
                // 品番登録情報の取得(照度情報,開始レジマークカメラ番号,終了レジマークカメラ番号)
                if (bolGetMstProductInfo(out m_intIlluminationInformation,
                                         out m_intStartRegimarkCameraNum,
                                         out m_intEndRegimarkCameraNum, 
                                         frmHinNoSelection.strProductName) == false)
                    return;

                // 品名の表示
                txtProductName.Text = frmHinNoSelection.strProductName;
            }
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
            UserSelection frmTargetSelection = new UserSelection();
            frmTargetSelection.ShowDialog(this);
            this.Visible = true;

            if (frmTargetSelection.strUserNo != null || frmTargetSelection.strUserNm != null)
            {
                // 社員番号を変数に保持
                if (txtWorker == txtWorker1)
                {
                    m_strWorker1 = frmTargetSelection.strUserNo;
                }
                else if (txtWorker == txtWorker2)
                {

                    m_strWorker2 = frmTargetSelection.strUserNo;
                }

                // 作業者名の表示
                txtWorker.Text = frmTargetSelection.strUserNm;
            }
        }

        /// <summary>
        /// 検査対象数(行数)テキストボックスフォーカスアウト
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtKensaTaishoNum_Leave(object sender, EventArgs e)
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

        /// <summary>
        /// 検査方向ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspectionDirection_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (btn == btnInspectionDirection_S)
            {
                SetInspectionDirectionSetting(m_CON_INSPECTION_DIRECTION_S);
            }
            else if (btn == btnInspectionDirection_X)
            {
                SetInspectionDirectionSetting(m_CON_INSPECTION_DIRECTION_X);
            }
            else if (btn == btnInspectionDirection_Y)
            {
                SetInspectionDirectionSetting(m_CON_INSPECTION_DIRECTION_Y);
            }
            else if (btn == btnInspectionDirection_R)
            {
                SetInspectionDirectionSetting(m_CON_INSPECTION_DIRECTION_R);
            }
        }

        /// <summary>
        /// 検査中断ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTyudan_Click(object sender, EventArgs e)
        {
            // 終了日付の表示
            lblEndDatetime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // 枝番の取得
            if (bolGetBranchNum(out m_intBranchNum, lblStartDatetime.Text.Substring(0, 10), m_intInspectionNum) == false)
                return;

            // 検査情報ヘッダーの更新
            if (UpdEndInspectionInfoHeader(m_intInspectionNum, m_intBranchNum, lblEndDatetime.Text) == false)
                return;

            // 撮像装置部へ連携用ファイル出力
            if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum) == false)
                return;

            // DBコミット
            DbCommit();

            // ステータスの表示設定(検査中断)
            SetStatusCtrSetting(m_CON_STATUS_STP);
        }

        /// <summary>
        /// 設定ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettei_Click(object sender, EventArgs e)
        {
            if (InputDataCheck() == false)
                return;

            // 確認メッセージ出力
            DialogResult result = MessageBox.Show("以下の情報で、撮像装置部へデータを転送します。\r\n処理を継続してよろしいでしょうか。\r\n" +
                                                  "　品名：" + txtProductName.Text + "\r\n" +
                                                  "　指図：" + txtOrderImg.Text + "\r\n" +
                                                  "　反番：" + txtFabricName.Text + "\r\n" +
                                                  "　検査対象数/最終行数：" + txtInspectionTargetLine.Text + "/" + lblInspectionEndLine.Text + "\r\n" +
                                                  "　検査開始行：" + txtInspectionStartLine.Text + "\r\n" +
                                                  "　作業者情報（社員番号）検反部No.1：" + txtWorker1.Text + "\r\n" +
                                                  "　作業者情報（社員番号）検反部No.2：" + txtWorker2.Text + "\r\n" +
                                                  "　検査方向：" + m_strInspectionDirection,
                                                  "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            switch (m_intStatus)
            {
                case m_CON_STATUS_BEF:
                case m_CON_STATUS_END:
                    // ステータス：検査開始前 or 終了から

                    // 終了時刻
                    lblEndDatetime.Text = "";

                    // 検査番号の採番
                    if (bolNumberInspectionNum(out m_intInspectionNum, DateTime.Now.ToString("yyyy/MM/dd")) == false)
                        return;
                    // 検査番号の表示
                    lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum);

                    // 検査情報ヘッダーの登録
                    if (RegStartInspectionInfoHeader(m_intInspectionNum, 1) == false)
                        return;

                    // 撮像装置部へ連携用ファイル出力
                    if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum) == false)
                        return;

                    // DBコミット
                    DbCommit();

                    // ステータスの表示設定(検査準備完了)
                    SetStatusCtrSetting(m_CON_STATUS_CHK);

                    // 検査対象数(行数)にフォーカスを設定
                    btnSettei.Focus();

                    break;
                case m_CON_STATUS_CHK:
                    // ステータス：検査準備完了から

                    // 枝番の取得
                    if (bolGetBranchNum(out m_intBranchNum, lblStartDatetime.Text.Substring(0, 10), m_intInspectionNum) == false)
                        return;

                    // 検査情報ヘッダーの更新
                    if (UpdEndInspectionInfoHeader(m_intInspectionNum, m_intBranchNum, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")) == false)
                        return;

                    // 撮像装置部へ連携用ファイル出力
                    if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum) == false)
                        return;

                    // 枝番の採番
                    m_intBranchNum++;

                    // 検査情報ヘッダーの登録
                    if (RegStartInspectionInfoHeader(m_intInspectionNum, m_intBranchNum) == false)
                        return;

                    // 撮像装置部へ連携用ファイル出力
                    if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum) == false)
                        return;

                    // DBコミット
                    DbCommit();

                    // ステータスの表示設定(検査準備完了)
                    SetStatusCtrSetting(m_CON_STATUS_CHK);

                    // 次の反番情報を設定のフォーカスセット
                    btnTanSet.Focus();

                    // 終了時刻
                    lblEndDatetime.Text = "";

                    break;
                case m_CON_STATUS_STP:
                    // ステータス：検査中断から

                    // 枝番の取得
                    if (bolGetBranchNum(out m_intBranchNum, lblStartDatetime.Text.Substring(0, 10), m_intInspectionNum) == false)
                        return;

                    // 検査情報ヘッダーの更新
                    if (UpdEndInspectionInfoHeader(m_intInspectionNum, m_intBranchNum, lblEndDatetime.Text) == false)
                        return;

                    // 撮像装置部へ連携用ファイル出力
                    if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum) == false)
                        return;

                    // 終了時刻
                    lblEndDatetime.Text = "";

                    // 検査番号の採番
                    if (bolNumberInspectionNum(out m_intInspectionNum, DateTime.Now.ToString("yyyy/MM/dd")) == false)
                        return;
                    // 検査番号の表示
                    lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum);

                    // 検査情報ヘッダーの登録
                    if (RegStartInspectionInfoHeader(m_intInspectionNum, m_intBranchNum) == false)
                        return;

                    // 撮像装置部へ連携用ファイル出力
                    if (bolOutFile(txtFabricName.Text, m_intInspectionNum, m_intBranchNum) == false)
                        return;

                    // ステータスの表示設定(検査準備完了)
                    SetStatusCtrSetting(m_CON_STATUS_CHK);

                    // 検査対象数(行数)のフォーカスセット
                    txtInspectionTargetLine.Focus();

                    break;
            }
        }

        /// <summary>
        /// 次の反番情報を設定ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTanSet_Click(object sender, EventArgs e)
        {
            // 検査番号の取得
            bolGetInspectionNum(out m_intInspectionNum, DateTime.Now.ToString("yyyy/MM/dd"));

            // 値の初期化
            lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum.ToString());
            lblUnitNum.Text = string.Format(m_CON_FORMAT_UNIT_NUM, m_strUnitNum);
            txtProductName.Text = "";
            txtOrderImg.Text = "";
            txtFabricName.Text = "";
            txtInspectionTargetLine.Text = "";
            lblInspectionEndLine.Text = "";
            txtInspectionStartLine.Text = "";
            txtWorker1.Text = "";
            txtWorker2.Text = "";
            lblStartDatetime.Text = "";
            lblEndDatetime.Text = "";
            SetInspectionDirectionSetting(m_CON_INSPECTION_DIRECTION_S);

            // 変数の設定
            m_intBranchNum = 1;

            // 品名にフォーカスを設定する
            txtProductName.Focus();

            // ステータスの表示設定(検査開始前)
            SetStatusCtrSetting(m_CON_STATUS_BEF);
        }
        #endregion
    }
}
