using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static ProductMstMaintenance.Common;


namespace ProductMstMaintenance
{
    public partial class ProductMstMaintenance : Form
    {
        #region 定数・変数
        // 定数
        private const int m_CON_REALITY_SIZE_W = 640;
        private const int m_CON_REALITY_SIZE_H = 480;
        private const string m_CON_DISPLAY_POSITION_LABEL = "表示位置：{0},{1}";
        private const string m_CON_SHARED_FOLDER_CONNECTION_STRING = @" use {0} /user:{1} {2}";

        // マスタ列名
        private const string m_CON_COLNAME_PRODUCT_NAME = "product_name";
        private const string m_CON_COLNAME_AIRBAG_IMAGEPATH = "airbag_imagepath";
        private const string m_CON_COLNAME_LENGTH = "length";
        private const string m_CON_COLNAME_WIDTH = "width";
        private const string m_CON_COLNAME_LINE_LENGTH = "line_length";
        private const string m_CON_COLNAME_STRETCH_RATE_X = "stretch_rate_x";
        private const string m_CON_COLNAME_STRETCH_RATE_X_UPD = "stretch_rate_x_upd";
        private const string m_CON_COLNAME_STRETCH_RATE_Y = "stretch_rate_y";
        private const string m_CON_COLNAME_STRETCH_RATE_Y_UPD = "stretch_rate_y_upd";
        private const string m_CON_COLNAME_AI_MODEL_NON_INSPECTION_FLG = "ai_model_non_inspection_flg";
        private const string m_CON_COLNAME_AI_MODEL_NAME = "ai_model_name";
        private const string m_CON_COLNAME_REGIMARK_BETWEEN_LENGTH = "regimark_between_length";
        private const string m_CON_COLNAME_REGIMARK_1_POINT_X = "regimark_1_point_x";
        private const string m_CON_COLNAME_REGIMARK_1_POINT_Y = "regimark_1_point_y";
        private const string m_CON_COLNAME_REGIMARK_2_POINT_X = "regimark_2_point_x";
        private const string m_CON_COLNAME_REGIMARK_2_POINT_Y = "regimark_2_point_y";
        private const string m_CON_COLNAME_BASE_POINT_1_X = "base_point_1_x";
        private const string m_CON_COLNAME_BASE_POINT_1_Y = "base_point_1_y";
        private const string m_CON_COLNAME_POINT_1_PLUS_DIRECTION_X = "point_1_plus_direction_x";
        private const string m_CON_COLNAME_POINT_1_PLUS_DIRECTION_Y = "point_1_plus_direction_y";
        private const string m_CON_COLNAME_BASE_POINT_2_X = "base_point_2_x";
        private const string m_CON_COLNAME_BASE_POINT_2_Y = "base_point_2_y";
        private const string m_CON_COLNAME_POINT_2_PLUS_DIRECTION_X = "point_2_plus_direction_x";
        private const string m_CON_COLNAME_POINT_2_PLUS_DIRECTION_Y = "point_2_plus_direction_y";
        private const string m_CON_COLNAME_BASE_POINT_3_X = "base_point_3_x";
        private const string m_CON_COLNAME_BASE_POINT_3_Y = "base_point_3_y";
        private const string m_CON_COLNAME_POINT_3_PLUS_DIRECTION_X = "point_3_plus_direction_x";
        private const string m_CON_COLNAME_POINT_3_PLUS_DIRECTION_Y = "point_3_plus_direction_y";
        private const string m_CON_COLNAME_BASE_POINT_4_X = "base_point_4_x";
        private const string m_CON_COLNAME_BASE_POINT_4_Y = "base_point_4_y";
        private const string m_CON_COLNAME_POINT_4_PLUS_DIRECTION_X = "point_4_plus_direction_x";
        private const string m_CON_COLNAME_POINT_4_PLUS_DIRECTION_Y = "point_4_plus_direction_y";
        private const string m_CON_COLNAME_BASE_POINT_5_X = "base_point_5_x";
        private const string m_CON_COLNAME_BASE_POINT_5_Y = "base_point_5_y";
        private const string m_CON_COLNAME_POINT_5_PLUS_DIRECTION_X = "point_5_plus_direction_x";
        private const string m_CON_COLNAME_POINT_5_PLUS_DIRECTION_Y = "point_5_plus_direction_y";
        private const string m_CON_COLNAME_START_REGIMARK_CAMERA_NUM = "start_regimark_camera_num";
        private const string m_CON_COLNAME_END_REGIMARK_CAMERA_NUM = "end_regimark_camera_num";
        private const string m_CON_COLNAME_ILLUMINATION_INFORMATION = "illumination_information";
        private const string m_CON_COLNAME_COLUMN_THRESHOLD_01 = "column_threshold_01";
        private const string m_CON_COLNAME_COLUMN_THRESHOLD_02 = "column_threshold_02";
        private const string m_CON_COLNAME_COLUMN_THRESHOLD_03 = "column_threshold_03";
        private const string m_CON_COLNAME_COLUMN_THRESHOLD_04 = "column_threshold_04";
        private const string m_CON_COLNAME_LINE_THRESHOLD_A1 = "line_threshold_a1";
        private const string m_CON_COLNAME_LINE_THRESHOLD_A2 = "line_threshold_a2";
        private const string m_CON_COLNAME_LINE_THRESHOLD_B1 = "line_threshold_b1";
        private const string m_CON_COLNAME_LINE_THRESHOLD_B2 = "line_threshold_b2";
        private const string m_CON_COLNAME_LINE_THRESHOLD_C1 = "line_threshold_c1";
        private const string m_CON_COLNAME_LINE_THRESHOLD_C2 = "line_threshold_c2";
        private const string m_CON_COLNAME_LINE_THRESHOLD_D1 = "line_threshold_d1";
        private const string m_CON_COLNAME_LINE_THRESHOLD_D2 = "line_threshold_d2";
        private const string m_CON_COLNAME_LINE_THRESHOLD_E1 = "line_threshold_e1";
        private const string m_CON_COLNAME_LINE_THRESHOLD_E2 = "line_threshold_e2";
        private const string m_CON_COLNAME_COLUMN_CNT = "column_cnt";

        // 変数
        private double m_dblSizeRateW = 100.00;
        private double m_dblSizeRateH = 100.00;
        private double m_dblSizeRate = 100.00;
        private int m_intColumn_cnt = 0;
        private int m_intRegimark1PointX = 0;
        private int m_intRegimark1PointY = 0;
        private int m_intRegimark2PointX = 0;
        private int m_intRegimark2PointY = 0;
        private int m_intBasePoint1X = 0;
        private int m_intBasePoint1Y = 0;
        private int m_intBasePoint2X = 0;
        private int m_intBasePoint2Y = 0;
        private int m_intBasePoint3X = 0;
        private int m_intBasePoint3Y = 0;
        private int m_intBasePoint4X = 0;
        private int m_intBasePoint4Y = 0;
        private int m_intBasePoint5X = 0;
        private int m_intBasePoint5Y = 0;
        private DataTable m_dtData = new DataTable();
        private string m_strMstFilePath = string.Empty;

        // レジマーク間の長さ
        private int m_intRegiMarkLengthPx = 0;
        #endregion

        #region イベント
        /// <summary>
        /// 初期処理
        /// </summary>
        public ProductMstMaintenance()
        {
            InitializeComponent();
        }

        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProductMstMaintenance_Load(object sender, EventArgs e)
        {

            this.SuspendLayout();

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            bool bolProcOkNg = false;

            try
            {
                // 初期化
                txtProductName.Text = string.Empty;
                lblLength.Text = string.Empty;
                lblWidth.Text = string.Empty;
                lblLineLength.Text = string.Empty;
                lblRegimarkBetweenLength.Text = string.Empty;
                txtStretchRateX.Text = string.Empty;
                txtStretchRateY.Text = string.Empty;
                chkAiModelNonInspectionFlg.Checked = true;
                txtAiModelName.Text = string.Empty;
                picMasterImage.Image = null;
                lblStartRegimarkPointN.Text = string.Empty;
                lblStartRegimarkPointNPlus1Line.Text = string.Empty;
                lblEndRegimarkPointN.Text = string.Empty;
                lblEndRegimarkPointNMinus1Line.Text = string.Empty;
                lblBasePointA.Text = string.Empty;
                lblBasePointB.Text = string.Empty;
                lblBasePointC.Text = string.Empty;
                lblBasePointD.Text = string.Empty;
                lblBasePointE.Text = string.Empty;
                lblPlusDirectionA.Text = string.Empty;
                lblPlusDirectionB.Text = string.Empty;
                lblPlusDirectionC.Text = string.Empty;
                lblPlusDirectionD.Text = string.Empty;
                lblPlusDirectionE.Text = string.Empty;
                lblStartRegimarkCameraNum.Text = string.Empty;
                lblEndRegimarkCameraNum.Text = string.Empty;
                lblIlluminationInformation.Text = string.Empty;
                txtColumnThresholdAB.Text = string.Empty;
                txtColumnThresholdBC.Text = string.Empty;
                txtColumnThresholdCD.Text = string.Empty;
                txtColumnThresholdDE.Text = string.Empty;
                txtColumnThresholdAFrom.Text = string.Empty;
                txtColumnThresholdATo.Text = string.Empty;
                txtColumnThresholdBFrom.Text = string.Empty;
                txtColumnThresholdBTo.Text = string.Empty;
                txtColumnThresholdCFrom.Text = string.Empty;
                txtColumnThresholdCTo.Text = string.Empty;
                txtColumnThresholdDFrom.Text = string.Empty;
                txtColumnThresholdDTo.Text = string.Empty;
                txtColumnThresholdEFrom.Text = string.Empty;
                txtColumnThresholdETo.Text = string.Empty;

                // 品番マスタから値の取得を行う
                if (GetHinMstInitial("") == false)
                {
                    txtProductName.Select();
                    return;
                }

                if (m_dtData.Rows.Count == 0)
                {
                    if (bolRunProductMstImportCsv() == false)
                    {
                        return;
                    }
                }

                // 取得結果反映処理
                CreateFormInfo();

                bolProcOkNg = true;
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
            ProductMstSelection frmProductMstSelection = new ProductMstSelection();
            frmProductMstSelection.ShowDialog(this);
            if (!string.IsNullOrEmpty(frmProductMstSelection.strHinNm))
            {
                // 品番マスタから値の取得を行う
                if (GetHinMstInitial(frmProductMstSelection.strHinNm) == false ||
                    m_dtData.Rows.Count == 0)
                {
                    txtProductName.Select();
                    return;
                }

                // 品名を表示
                txtProductName.Text = frmProductMstSelection.strHinNm;

                // 取得結果反映処理
                CreateFormInfo();

                displayPositionLabel.Text =
                    string.Format(
                        m_CON_DISPLAY_POSITION_LABEL,
                        0,
                        0);
            }
        }

        /// <summary>
        /// 境界線テキストボックス値変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtThreshold_Leave(object sender, EventArgs e)
        {
            // 大小チェック
            if (CheckRangeInputData(txtColumnThresholdAB, txtColumnThresholdBC, "列判定用境界線設定", "A-B列", "B-C列", false) == false ||
                CheckRangeInputData(txtColumnThresholdBC, txtColumnThresholdCD, "列判定用境界線設定", "B-C列", "C-D列", false) == false ||
                CheckRangeInputData(txtColumnThresholdCD, txtColumnThresholdDE, "列判定用境界線設定", "C-D列", "D-E列", false) == false ||
                CheckRangeInputData(txtColumnThresholdAFrom, txtColumnThresholdATo, "行判定用境界線設定", "A列(開始)", "A列(終了)", false) == false ||
                CheckRangeInputData(txtColumnThresholdBFrom, txtColumnThresholdBTo, "行判定用境界線設定", "B列(開始)", "B列(終了)", false) == false ||
                CheckRangeInputData(txtColumnThresholdCFrom, txtColumnThresholdCTo, "行判定用境界線設定", "C列(開始)", "C列(終了)", false) == false ||
                CheckRangeInputData(txtColumnThresholdDFrom, txtColumnThresholdDTo, "行判定用境界線設定", "D列(開始)", "D列(終了)", false) == false ||
                CheckRangeInputData(txtColumnThresholdEFrom, txtColumnThresholdETo, "行判定用境界線設定", "E列(開始)", "E列(終了)", false) == false)
            {
                TextBox target = sender as TextBox;
                target.Focus();
                return;
            }

            DrawThreshold();
        }

        /// <summary>
        /// 境界線テキストボックス値変更確定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtThreshold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                // 大小チェック
                if (CheckRangeInputData(txtColumnThresholdAB, txtColumnThresholdBC, "列判定用境界線設定", "A-B列", "B-C列", false) == false ||
                    CheckRangeInputData(txtColumnThresholdBC, txtColumnThresholdCD, "列判定用境界線設定", "B-C列", "C-D列", false) == false ||
                    CheckRangeInputData(txtColumnThresholdCD, txtColumnThresholdDE, "列判定用境界線設定", "C-D列", "D-E列", false) == false ||
                    CheckRangeInputData(txtColumnThresholdAFrom, txtColumnThresholdATo, "行判定用境界線設定", "A列(開始)", "A列(終了)", false) == false ||
                    CheckRangeInputData(txtColumnThresholdBFrom, txtColumnThresholdBTo, "行判定用境界線設定", "B列(開始)", "B列(終了)", false) == false ||
                    CheckRangeInputData(txtColumnThresholdCFrom, txtColumnThresholdCTo, "行判定用境界線設定", "C列(開始)", "C列(終了)", false) == false ||
                    CheckRangeInputData(txtColumnThresholdDFrom, txtColumnThresholdDTo, "行判定用境界線設定", "D列(開始)", "D列(終了)", false) == false ||
                    CheckRangeInputData(txtColumnThresholdEFrom, txtColumnThresholdETo, "行判定用境界線設定", "E列(開始)", "E列(終了)", false) == false)
                {
                    TextBox target = sender as TextBox;
                    target.Focus();
                    return;
                }

                DrawThreshold();
            }
        }

        /// <summary>
        /// csv取り込みボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProductInfoImportCsv_Click(object sender, EventArgs e)
        {
            bolRunProductMstImportCsv();
        }

        /// <summary>
        /// 品番情報取り込み画面遷移
        /// </summary>
        /// <returns>
        ///     true :1件以上のデータが存在する 
        ///     false:それ以外
        /// </returns>
        private bool bolRunProductMstImportCsv()
        {
            if (m_dtData.Rows.Count == 0)
            {
                MessageBox.Show(g_clsMessageInfo.strMsgW0004, "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (MessageBox.Show(g_clsMessageInfo.strMsgQ0005, "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return false;
                }
            }

            // 取り込み表示の場合、初期表示する
            ProductMstImportCsv frmUserImportCsv = new ProductMstImportCsv();
            frmUserImportCsv.ShowDialog(this);
            frmUserImportCsv.Dispose();

            // 画面初期表示
            // 品番マスタから値の取得を行う
            if (GetHinMstInitial("") == false ||
                m_dtData.Rows.Count == 0)
            {
                txtProductName.Select();
                return false;
            }

            // 一時フォルダへマスタ画像を取り込む
            bolImpMasterImage();

            // 取得結果反映処理
            CreateFormInfo();

            return true;
        }

        /// <summary>
        /// 更新ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // 入力チェックを行う
            if (InputCheck() == false)
            {
                return;
            }

            // 確認メッセージ
            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0006, txtProductName.Text), "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // 更新処理を行う
                if (UPDMstProductInfoDisp() == false)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 判定理由マスタボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMstDecisionReason_Click(object sender, EventArgs e)
        {
            SelectErrorReason frmSelectErrorReason = new SelectErrorReason();
            frmSelectErrorReason.ShowDialog(this);
        }

        /// <summary>
        /// マスタ画像クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picMasterImage_MouseDown(object sender, MouseEventArgs e)
        {
            // マウス座標を取得
            var x = e.Location.X;
            var y = e.Location.Y;

            // 基準のピクチャボックスより、高さ・幅を取得
            var picHeight = picMasterImage.ClientSize.Height;
            var picWidth = picMasterImage.ClientSize.Width;
            var imgHeight = picMasterImage.Image.Height;
            var imgWidth = picMasterImage.Image.Width;

            int intXcoordinate = 0;
            int intYcoordinate = 0;

            // ピクチャボックス上の座標を特定する
            if (picWidth / (float)picHeight > imgWidth / (float)imgHeight)
            {
                var scaledW = imgWidth * picHeight / (float)imgHeight;
                var dx = (picWidth - scaledW) / 2;
                intXcoordinate = (int)((x - dx) * imgHeight / picHeight);

                intYcoordinate = (int)(imgHeight * y / (float)picHeight);
            }
            else
            {
                intXcoordinate = (int)(imgWidth * x / (float)picWidth);

                var scaledH = imgHeight * picWidth / (float)imgWidth;
                var dy = (picHeight - scaledH) / 2;
                intYcoordinate = (int)((y - dy) * imgWidth / picWidth);
            }

            // 算出結果が不正な場合、処理を終了する
            if (intXcoordinate < 0 || imgWidth < intXcoordinate || intYcoordinate < 0 || imgHeight < intYcoordinate)
            {
                return;
            }

            displayPositionLabel.Text =
                string.Format(
                    m_CON_DISPLAY_POSITION_LABEL,
                    intXcoordinate,
                    intYcoordinate);
        }

        /// <summary>
        /// 数値入力チェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckInputNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                // 0～9と、バックスペース以外の時は、イベントをキャンセルする
                e.Handled = true;
            }
        }

        /// <summary>
        /// 小数入力チェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckInputDouble_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.')
            {
                TextBox target = sender as TextBox;

                if (target.Text.IndexOf('.') >= 0)
                {
                    // 複数のピリオドが入力された時は、イベントをキャンセルする
                    e.Handled = true;
                }

                return;
            }

            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                // 0～9と、バックスペース以外の時は、イベントをキャンセルする
                e.Handled = true;
            }
        }
        #endregion

        #region メソッド
        /// <summary>
        /// 境界線描画処理
        /// </summary>
        private void DrawThreshold()
        {
            string strPicBoxDispLineCtrName = "picBoxDspLine";

            // 変数
            int intColumnThreshold = -1;                    // 列判定用境界線
            int intLineThreshold = -1;                      // 行判定用境界線
            int intColumnThresholdTop = -1;                 // 列判定用境界線の天辺
            int intColumnThresholdBottom = -1;              // 列判定用境界線の底辺
            int intRegimark1PointX = -1;                    // 開始レジマークX座標
            int intRegimark1PointY = -1;                    // 開始レジマークY座標
            int intRegimark2PointX = -1;                    // 終了レジマークX座標
            int intRegimark2PointY = -1;                    // 終了レジマークY座標
            int intBasePoint1X = -1;                        // 基準点1X座標
            int intBasePoint1Y = -1;                        // 基準点1Y座標
            int intBasePoint2X = -1;                        // 基準点2X座標
            int intBasePoint2Y = -1;                        // 基準点2Y座標
            int intBasePoint3X = -1;                        // 基準点3X座標
            int intBasePoint3Y = -1;                        // 基準点3Y座標
            int intBasePoint4X = -1;                        // 基準点4X座標
            int intBasePoint4Y = -1;                        // 基準点4Y座標
            int intBasePoint5X = -1;                        // 基準点5X座標
            int intBasePoint5Y = -1;                        // 基準点5Y座標

            // 描画の前準備
            // 描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(picMasterImage.Width, picMasterImage.Height);
            // ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);
            PictureBox pctLineCtrl = null;
            // Penオブジェクト
            Pen p = null;

            // 既存の行判定用境界線コントロール存在チェック
            if (this.Controls.Find(strPicBoxDispLineCtrName, true).Length == 0)
            {
                // 枠線用コントロールの追加
                picMasterImage.Controls.Add(new System.Windows.Forms.PictureBox());
                pctLineCtrl = (PictureBox)picMasterImage.Controls[picMasterImage.Controls.Count - 1];
                pctLineCtrl.Name = strPicBoxDispLineCtrName;
                pctLineCtrl.ClientSize = new Size(picMasterImage.ClientSize.Width, picMasterImage.ClientSize.Height);
                pctLineCtrl.MouseDown += new MouseEventHandler(this.picMasterImage_MouseDown);
            }
            else
            {
                foreach (Control ctr in this.Controls.Find(strPicBoxDispLineCtrName, true))
                {
                    pctLineCtrl = (PictureBox)ctr;
                    break;
                }
            }

            // 列判定用境界線の描画
            foreach (TextBox txtColumnThreshold in new TextBox[] { txtColumnThresholdAB, txtColumnThresholdBC,
                                                                   txtColumnThresholdCD, txtColumnThresholdDE })
            {
                // 初期化
                intColumnThreshold = -1;

                if (m_intColumn_cnt > 1 && txtColumnThreshold == txtColumnThresholdAB)
                {
                    intColumnThreshold = (int)((double)NulltoInt(txtColumnThresholdAB.Text) * m_dblSizeRate);
                }

                if (m_intColumn_cnt > 2 && txtColumnThreshold == txtColumnThresholdBC)
                {
                    intColumnThreshold = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                }

                if (m_intColumn_cnt > 3 && txtColumnThreshold == txtColumnThresholdCD)
                {
                    intColumnThreshold = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                }

                if (m_intColumn_cnt > 4 && txtColumnThreshold == txtColumnThresholdDE)
                {
                    intColumnThreshold = (int)((double)NulltoInt(txtColumnThresholdDE.Text) * m_dblSizeRate);
                }

                // 行判定用境界線が未設定（列数の設定等）の場合はスルー
                if (intColumnThreshold > -1)
                {
                    // 線を描画
                    p = new Pen(Color.Red, 2);
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    g.DrawLine(p, 0, intColumnThreshold,
                                  picMasterImage.ClientSize.Width, intColumnThreshold);
                }
            }

            // 行判定用境界線の描画
            foreach (TextBox txtLineThreshold in new TextBox[] { txtColumnThresholdAFrom, txtColumnThresholdATo,
                                                                 txtColumnThresholdBFrom, txtColumnThresholdBTo,
                                                                 txtColumnThresholdCFrom, txtColumnThresholdCTo,
                                                                 txtColumnThresholdDFrom, txtColumnThresholdDTo,
                                                                 txtColumnThresholdEFrom, txtColumnThresholdETo })
            {
                // 初期化
                intLineThreshold = -1;
                intColumnThresholdTop = -1;
                intColumnThresholdBottom = -1;

                if (txtLineThreshold == txtColumnThresholdAFrom)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdAFrom.Text) * m_dblSizeRate);
                    intColumnThresholdTop = 0;
                    if (m_intColumn_cnt > 1)
                    {
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdAB.Text) * m_dblSizeRate);
                    }
                }

                if (txtLineThreshold == txtColumnThresholdATo)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdATo.Text) * m_dblSizeRate);
                    intColumnThresholdTop = 0;
                    if (m_intColumn_cnt > 1)
                    {
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdAB.Text) * m_dblSizeRate);
                    }
                }

                if (m_intColumn_cnt > 1 && txtLineThreshold == txtColumnThresholdBFrom)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdBFrom.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdAB.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 2)
                    {
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                    }
                }

                if (m_intColumn_cnt > 1 && txtLineThreshold == txtColumnThresholdBTo)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdBTo.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdAB.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 2)
                    {
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                    }
                }

                if (m_intColumn_cnt > 2 && txtLineThreshold == txtColumnThresholdCFrom)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdCFrom.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 3)
                    {
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                    }
                }

                if (m_intColumn_cnt > 2 && txtLineThreshold == txtColumnThresholdCTo)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdCTo.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 3)
                    {
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                    }
                }

                if (m_intColumn_cnt > 3 && txtLineThreshold == txtColumnThresholdDFrom)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdDFrom.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 4)
                    {
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdDE.Text) * m_dblSizeRate);
                    }
                }

                if (m_intColumn_cnt > 3 && txtLineThreshold == txtColumnThresholdDTo)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdDTo.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 4)
                    {
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdDE.Text) * m_dblSizeRate);
                    }
                }

                if (m_intColumn_cnt > 4 && txtLineThreshold == txtColumnThresholdEFrom)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdEFrom.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdDE.Text) * m_dblSizeRate);
                    intColumnThresholdBottom = picMasterImage.ClientSize.Height;
                }

                if (m_intColumn_cnt > 4 && txtLineThreshold == txtColumnThresholdETo)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdETo.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdDE.Text) * m_dblSizeRate);
                    intColumnThresholdBottom = picMasterImage.ClientSize.Height;
                }

                // 行判定用境界線が未設定（列数の設定等）の場合はスルー
                if (intLineThreshold > -1)
                {
                    // 列判定用境界線の底辺Y座標を設定する
                    if (intColumnThresholdBottom == -1)
                    {
                        intColumnThresholdBottom = picMasterImage.ClientSize.Height;
                    }

                    // 線を描画
                    p = new Pen(Color.Black, 2);
                    p.DashPattern = new float[] { 1.0F, 3.0F, 1.0F, 3.0F };
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    g.DrawLine(p, intLineThreshold, intColumnThresholdTop,
                                  intLineThreshold, intColumnThresholdBottom);
                }
            }

            intRegimark1PointX = (int)((double)m_intRegimark1PointX * m_dblSizeRate);
            intRegimark1PointY = (int)((double)m_intRegimark1PointY * m_dblSizeRate);
            intRegimark2PointX = (int)((double)m_intRegimark2PointX * m_dblSizeRate);
            intRegimark2PointY = (int)((double)m_intRegimark2PointY * m_dblSizeRate);

            // 開始レジマーク箇所に「Ｘ」を描画
            p = new Pen(Color.DeepSkyBlue, 1.5f);
            g.DrawLine(p, intRegimark1PointX - 8, intRegimark1PointY - 8, intRegimark1PointX + 8, intRegimark1PointY + 8);
            g.DrawLine(p, intRegimark1PointX + 8, intRegimark1PointY - 8, intRegimark1PointX - 8, intRegimark1PointY + 8);

            // 終了レジマーク箇所に「Ｘ」を描画
            p = new Pen(Color.MediumVioletRed, 1.5f);
            g.DrawLine(p, intRegimark2PointX - 8, intRegimark2PointY - 8, intRegimark2PointX + 8, intRegimark2PointY + 8);
            g.DrawLine(p, intRegimark2PointX + 8, intRegimark2PointY - 8, intRegimark2PointX - 8, intRegimark2PointY + 8);

            // 基準点描画用に生成
            p = new Pen(Color.Orange, 1.5f);

            if (m_intBasePoint1X != 0 ||
                m_intBasePoint1Y != 0)
            {
                intBasePoint1X = (int)((double)m_intBasePoint1X * m_dblSizeRate);
                intBasePoint1Y = (int)((double)m_intBasePoint1Y * m_dblSizeRate);

                // A列座標箇所に「Ｘ」を描画
                g.DrawLine(p, intBasePoint1X - 8, intBasePoint1Y - 8, intBasePoint1X + 8, intBasePoint1Y + 8);
                g.DrawLine(p, intBasePoint1X + 8, intBasePoint1Y - 8, intBasePoint1X - 8, intBasePoint1Y + 8);
            }

            if (m_intColumn_cnt > 1 &&
                (m_intBasePoint2X != 0 ||
                m_intBasePoint2Y != 0))
            {
                intBasePoint2X = (int)((double)m_intBasePoint2X * m_dblSizeRate);
                intBasePoint2Y = (int)((double)m_intBasePoint2Y * m_dblSizeRate);

                // B列座標箇所に「Ｘ」を描画
                g.DrawLine(p, intBasePoint2X - 8, intBasePoint2Y - 8, intBasePoint2X + 8, intBasePoint2Y + 8);
                g.DrawLine(p, intBasePoint2X + 8, intBasePoint2Y - 8, intBasePoint2X - 8, intBasePoint2Y + 8);
            }

            if (m_intColumn_cnt > 2 &&
                (m_intBasePoint3X != 0 ||
                m_intBasePoint3Y != 0))
            {
                intBasePoint3X = (int)((double)m_intBasePoint3X * m_dblSizeRate);
                intBasePoint3Y = (int)((double)m_intBasePoint3Y * m_dblSizeRate);

                // C列座標箇所に「Ｘ」を描画
                g.DrawLine(p, intBasePoint3X - 8, intBasePoint3Y - 8, intBasePoint3X + 8, intBasePoint3Y + 8);
                g.DrawLine(p, intBasePoint3X + 8, intBasePoint3Y - 8, intBasePoint3X - 8, intBasePoint3Y + 8);
            }

            if (m_intColumn_cnt > 3 &&
                (m_intBasePoint4X != 0 ||
                m_intBasePoint4Y != 0))
            {
                intBasePoint4X = (int)((double)m_intBasePoint4X * m_dblSizeRate);
                intBasePoint4Y = (int)((double)m_intBasePoint4Y * m_dblSizeRate);

                // D列座標箇所に「Ｘ」を描画
                g.DrawLine(p, intBasePoint4X - 8, intBasePoint4Y - 8, intBasePoint4X + 8, intBasePoint4Y + 8);
                g.DrawLine(p, intBasePoint4X + 8, intBasePoint4Y - 8, intBasePoint4X - 8, intBasePoint4Y + 8);
            }

            if (m_intColumn_cnt > 4 &&
                (m_intBasePoint5X != 0 ||
                m_intBasePoint5Y != 0))
            {
                intBasePoint5X = (int)((double)m_intBasePoint5X * m_dblSizeRate);
                intBasePoint5Y = (int)((double)m_intBasePoint5Y * m_dblSizeRate);

                // E列座標箇所に「Ｘ」を描画
                g.DrawLine(p, intBasePoint5X - 8, intBasePoint5Y - 8, intBasePoint5X + 8, intBasePoint5Y + 8);
                g.DrawLine(p, intBasePoint5X + 8, intBasePoint5Y - 8, intBasePoint5X - 8, intBasePoint5Y + 8);
            }

            //リソースを解放する
            p.Dispose();
            g.Dispose();

            //PictureBox1に表示する
            pctLineCtrl.Image = canvas;
        }

        /// <summary>
        /// 品種登録情報取得（画面初期表示）
        /// </summary>
        private Boolean GetHinMstInitial(string strProductName)
        {
            try
            {
                // 条件が指定されていない場合は抽出しない
                // SQL抽出
                m_dtData = new DataTable();
                string strSQL = string.Empty;

                if (string.IsNullOrEmpty(strProductName))
                {
                    // 画面初期表示
                    strSQL = g_CON_SELECT_MST_PRODUCT_INFO_TOP;
                    g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL);
                }
                else
                {
                    // 品名選択情報表示
                    strSQL = g_CON_SELECT_MST_PRODUCT_INFO_PRN;
                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter
                    {
                        ParameterName = "product_name"
                                                                              ,
                        DbType = DbType.String
                                                                              ,
                        Value = strProductName
                    });
                    g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + Environment.NewLine + ex.Message);
                MessageBox.Show(g_clsMessageInfo.strMsgE0021, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }

        /// <summary>
        /// 画面反映処理
        /// </summary>
        private void CreateFormInfo()
        {
            if (!string.IsNullOrWhiteSpace(g_clsSystemSettingInfo.strSharedFolderUser) &&
                !string.IsNullOrWhiteSpace(g_clsSystemSettingInfo.strSharedFolderPassword))
            {
                // 共有フォルダ接続
                using (Process prNet = new Process())
                {
                    prNet.StartInfo.FileName = "net.exe";
                    prNet.StartInfo.Arguments =
                        string.Format(
                            m_CON_SHARED_FOLDER_CONNECTION_STRING,
                            g_clsSystemSettingInfo.strMasterImageDirectory,
                            g_clsSystemSettingInfo.strSharedFolderUser,
                            g_clsSystemSettingInfo.strSharedFolderPassword);
                    prNet.StartInfo.CreateNoWindow = true;
                    prNet.StartInfo.UseShellExecute = false;
                    prNet.StartInfo.RedirectStandardOutput = true;
                    prNet.Start();
                    prNet.WaitForExit();
                }
            }

            // 取得結果描画
            MappingDtToForm(m_dtData);

            // ファイルが存在するか確認
            if (File.Exists(m_strMstFilePath) == true)
            {
                // マスタ画像イメージ反映
                // ※ファイルをロックしてしまい、取り込み処理に失敗する考慮
                FileStream fs = new FileStream(m_strMstFilePath, FileMode.Open, FileAccess.Read);
                picMasterImage.Image = Image.FromStream(fs);
                fs.Close();
            }
            else
            {
                // マスタ画像イメージ削除
                picMasterImage.Image = null;
            }

            // ピクチャボックスの設定を行う
            SettingPicBox();

            // 境界線の描画
            DrawThreshold();

            // カメラ位置設定
            lblCamPosi1.Text = "2";
            lblCamPosi2.Text = "8";
            lblCamPosi3.Text = "14";
            lblCamPosi4.Text = "20";
            lblCamPosi5.Text = "26";

            // 座標ラベルの表示非表示を変更する
            DispPointChange();

            // 品名にフォーカスを合わせる
            txtProductName.Focus();
        }

        /// <summary>
        /// 取得結果画面表示
        /// </summary>
        /// <param name="dtData">読み取り対象データテーブル</param>
        private void MappingDtToForm(DataTable dtData)
        {
            DataRow dtCurentRow = dtData.Rows[0];
            m_intColumn_cnt = 0;

            // 品名
            txtProductName.Text = NulltoString(dtCurentRow[m_CON_COLNAME_PRODUCT_NAME]);

            //エアバック画像ファイルパス
            m_strMstFilePath = Path.Combine(g_strMasterImageDirPath, Path.GetFileName(NulltoString(dtCurentRow[m_CON_COLNAME_AIRBAG_IMAGEPATH])));

            // 長さ
            if (!string.IsNullOrEmpty(NulltoString(dtCurentRow[m_CON_COLNAME_LENGTH])))
            {
                lblLength.Text = String.Format("{0:#,0}", int.Parse(NulltoString(dtCurentRow[m_CON_COLNAME_LENGTH])));
            }
            else
            {
                lblLength.Text = string.Empty;
            }

            // 幅
            if (!string.IsNullOrEmpty(NulltoString(dtCurentRow[m_CON_COLNAME_WIDTH])))
            {
                lblWidth.Text = String.Format("{0:#,0}", int.Parse(NulltoString(dtCurentRow[m_CON_COLNAME_WIDTH])));
            }
            else
            {
                lblWidth.Text = string.Empty;
            }

            // 行長さ
            if (!string.IsNullOrEmpty(NulltoString(dtCurentRow[m_CON_COLNAME_LINE_LENGTH])))
            {
                lblLineLength.Text = String.Format("{0:#,0}", int.Parse(NulltoString(dtCurentRow[m_CON_COLNAME_LINE_LENGTH])));
            }
            else
            {
                lblLineLength.Text = string.Empty;
            }

            // レジマーク間引き
            if (!string.IsNullOrEmpty(NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_BETWEEN_LENGTH])))
            {
                lblRegimarkBetweenLength.Text = String.Format("{0:#,0}", int.Parse(NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_BETWEEN_LENGTH])));
            }
            else
            {
                lblRegimarkBetweenLength.Text = string.Empty;
            }

            // 伸縮率X
            string strStretchRateX = NulltoString(dtCurentRow[m_CON_COLNAME_STRETCH_RATE_X]);
            if (!string.IsNullOrEmpty(strStretchRateX))
            {
                txtStretchRateX.Text = string.Format("{0:f2}", double.Parse(strStretchRateX));
            }
            else
            {
                txtStretchRateX.Text = string.Empty;
            }

            // 伸縮率Y
            string strStretchRateY = NulltoString(dtCurentRow[m_CON_COLNAME_STRETCH_RATE_Y]);
            if (!string.IsNullOrEmpty(strStretchRateY))
            {
                txtStretchRateY.Text = string.Format("{0:f2}", double.Parse(strStretchRateY));
            }
            else
            {
                txtStretchRateY.Text = string.Empty;
            }

            // AIモデル未検査フラグ
            if (NulltoInt(dtCurentRow[m_CON_COLNAME_AI_MODEL_NON_INSPECTION_FLG]) == 1)
            {
                chkAiModelNonInspectionFlg.Checked = true;
            }
            else
            {
                chkAiModelNonInspectionFlg.Checked = false;
            }

            // AIモデル名
            txtAiModelName.Text = Path.GetFileNameWithoutExtension(NulltoString(dtCurentRow[m_CON_COLNAME_AI_MODEL_NAME]));

            // レジマーク間の長さ（pixel）
            decimal dc = (640 * (NulltoDcm(dtCurentRow[m_CON_COLNAME_REGIMARK_BETWEEN_LENGTH]) /
                                NulltoDcm(dtCurentRow[m_CON_COLNAME_LENGTH])));
            m_intRegiMarkLengthPx = NulltoInt(Math.Floor(dc));

            // レジマーク表示部.開始レジマーク座標.N行
            lblStartRegimarkPointN.Text = string.Format("({0},{1})", NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_1_POINT_X])
                                                , NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_1_POINT_Y]));

            m_intRegimark1PointX = NulltoInt(dtCurentRow[m_CON_COLNAME_REGIMARK_1_POINT_X]);
            m_intRegimark1PointY = NulltoInt(dtCurentRow[m_CON_COLNAME_REGIMARK_1_POINT_Y]);

            // レジマーク表示部.終了レジマーク座標.N行
            lblEndRegimarkPointN.Text = string.Format("({0},{1})", NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_2_POINT_X])
                                              , NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_2_POINT_Y]));

            m_intRegimark2PointX = NulltoInt(dtCurentRow[m_CON_COLNAME_REGIMARK_2_POINT_X]);
            m_intRegimark2PointY = NulltoInt(dtCurentRow[m_CON_COLNAME_REGIMARK_2_POINT_Y]);

            string strPoint;
            string strArrow;

            // 座標のラベルを作成 A
            CreatePointString(dtCurentRow
                            , m_CON_COLNAME_BASE_POINT_1_X
                            , m_CON_COLNAME_BASE_POINT_1_Y
                            , m_CON_COLNAME_POINT_1_PLUS_DIRECTION_X
                            , m_CON_COLNAME_POINT_1_PLUS_DIRECTION_Y
                            , out strPoint
                            , out strArrow);
            lblBasePointA.Text = strPoint;
            lblPlusDirectionA.Text = strArrow;

            m_intBasePoint1X = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_1_X]);
            m_intBasePoint1Y = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_1_Y]);

            // 座標のラベルを作成 B
            CreatePointString(dtCurentRow
                            , m_CON_COLNAME_BASE_POINT_2_X
                            , m_CON_COLNAME_BASE_POINT_2_Y
                            , m_CON_COLNAME_POINT_2_PLUS_DIRECTION_X
                            , m_CON_COLNAME_POINT_2_PLUS_DIRECTION_Y
                            , out strPoint
                            , out strArrow);
            lblBasePointB.Text = strPoint;
            lblPlusDirectionB.Text = strArrow;

            m_intBasePoint2X = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_2_X]);
            m_intBasePoint2Y = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_2_Y]);

            // 座標のラベルを作成 C
            CreatePointString(dtCurentRow
                            , m_CON_COLNAME_BASE_POINT_3_X
                            , m_CON_COLNAME_BASE_POINT_3_Y
                            , m_CON_COLNAME_POINT_3_PLUS_DIRECTION_X
                            , m_CON_COLNAME_POINT_3_PLUS_DIRECTION_Y
                            , out strPoint
                            , out strArrow);
            lblBasePointC.Text = strPoint;
            lblPlusDirectionC.Text = strArrow;

            m_intBasePoint3X = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_3_X]);
            m_intBasePoint3Y = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_3_Y]);

            // 座標のラベルを作成 D
            CreatePointString(dtCurentRow
                            , m_CON_COLNAME_BASE_POINT_4_X
                            , m_CON_COLNAME_BASE_POINT_4_Y
                            , m_CON_COLNAME_POINT_4_PLUS_DIRECTION_X
                            , m_CON_COLNAME_POINT_4_PLUS_DIRECTION_Y
                            , out strPoint
                            , out strArrow);
            lblBasePointD.Text = strPoint;
            lblPlusDirectionD.Text = strArrow;

            m_intBasePoint4X = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_4_X]);
            m_intBasePoint4Y = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_4_Y]);

            // 座標のラベルを作成 E
            CreatePointString(dtCurentRow
                            , m_CON_COLNAME_BASE_POINT_5_X
                            , m_CON_COLNAME_BASE_POINT_5_Y
                            , m_CON_COLNAME_POINT_5_PLUS_DIRECTION_X
                            , m_CON_COLNAME_POINT_5_PLUS_DIRECTION_Y
                            , out strPoint
                            , out strArrow);
            lblBasePointE.Text = strPoint;
            lblPlusDirectionE.Text = strArrow;

            m_intBasePoint5X = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_5_X]);
            m_intBasePoint5Y = NulltoInt(dtCurentRow[m_CON_COLNAME_BASE_POINT_5_Y]);

            // 開始レジマークカメラ番号
            lblStartRegimarkCameraNum.Text = NulltoString(dtCurentRow[m_CON_COLNAME_START_REGIMARK_CAMERA_NUM]);
            // 終了レジマークカメラ番号
            lblEndRegimarkCameraNum.Text = NulltoString(dtCurentRow[m_CON_COLNAME_END_REGIMARK_CAMERA_NUM]);
            // 照度情報
            lblIlluminationInformation.Text = NulltoString(dtCurentRow[m_CON_COLNAME_ILLUMINATION_INFORMATION]);

            // A-B列
            txtColumnThresholdAB.Text = NulltoString(dtCurentRow[m_CON_COLNAME_COLUMN_THRESHOLD_01]);
            // B-C列
            txtColumnThresholdBC.Text = NulltoString(dtCurentRow[m_CON_COLNAME_COLUMN_THRESHOLD_02]);
            // C-D列
            txtColumnThresholdCD.Text = NulltoString(dtCurentRow[m_CON_COLNAME_COLUMN_THRESHOLD_03]);
            // D-E列
            txtColumnThresholdDE.Text = NulltoString(dtCurentRow[m_CON_COLNAME_COLUMN_THRESHOLD_04]);

            // 行判定用境界線設定表示部.A列
            txtColumnThresholdAFrom.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_A1]);
            txtColumnThresholdATo.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_A2]);

            // 行判定用境界線設定表示部.B列
            txtColumnThresholdBFrom.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_B1]);
            txtColumnThresholdBTo.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_B2]);

            // 行判定用境界線設定表示部.C列
            txtColumnThresholdCFrom.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_C1]);
            txtColumnThresholdCTo.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_C2]);

            // 行判定用境界線設定表示部.D列
            txtColumnThresholdDFrom.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_D1]);
            txtColumnThresholdDTo.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_D2]);

            // 行判定用境界線設定表示部.E列
            txtColumnThresholdEFrom.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_E1]);
            txtColumnThresholdETo.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_E2]);

            // 開始レジマーク座標.N行+1行
            string strPointNPlus1 = string.Empty;
            strPointNPlus1 = "(";
            strPointNPlus1 = strPointNPlus1 + NulltoString(NulltoInt(dtCurentRow[m_CON_COLNAME_REGIMARK_1_POINT_X])
                                                         + m_intRegiMarkLengthPx);
            strPointNPlus1 = strPointNPlus1 + ",";
            strPointNPlus1 = strPointNPlus1 + NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_1_POINT_Y]) + ")";
            lblStartRegimarkPointNPlus1Line.Text = strPointNPlus1;

            // 終了レジマーク座標.N行+1行
            strPointNPlus1 = "(";
            strPointNPlus1 = strPointNPlus1 + NulltoString(NulltoInt(dtCurentRow[m_CON_COLNAME_REGIMARK_2_POINT_X])
                                                         - m_intRegiMarkLengthPx);
            strPointNPlus1 = strPointNPlus1 + ",";
            strPointNPlus1 = strPointNPlus1 + NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_2_POINT_Y]) + ")";
            lblEndRegimarkPointNMinus1Line.Text = strPointNPlus1;

            // 列数
            m_intColumn_cnt = NulltoInt(dtCurentRow[m_CON_COLNAME_COLUMN_CNT]);
        }

        /// <summary>
        /// 座標軸作成
        /// </summary>
        /// <param name="dtCurentRow">抽出マスタ値</param>
        /// <param name="strPointXBase">座標Xの項目名</param>
        /// <param name="strPointYBase">座標Yの項目名</param>
        /// <param name="strPointXPlus">座標Xの方向</param>
        /// <param name="strPointYPlus">座標Yの方向</param>
        /// <param name="strPoint">表示する座標軸の文字列</param>
        /// <param name="strArrow">表示する座標軸の方向の文字列</param>
        private void CreatePointString(DataRow dtCurentRow
                                     , string strPointXBase
                                     , string strPointYBase
                                     , string strPointXPlus
                                     , string strPointYPlus
                                     , out string strPoint
                                     , out string strArrow)
        {
            // 基準点表示部.A列座標.(X,Y)
            strPoint = string.Empty;
            strArrow = string.Empty;

            // 未設定の場合は空文字
            if (string.IsNullOrEmpty(NulltoString(dtCurentRow[strPointXBase])) &&
                string.IsNullOrEmpty(NulltoString(dtCurentRow[strPointYBase])))
            {
                return;
            }

            strPoint = "(";
            if (NulltoInt(dtCurentRow[strPointXPlus]) == 0)
            {
                strArrow = strArrow + "←";
            }
            else
            {
                strArrow = strArrow + "→";
            }

            strPoint = strPoint + NulltoString(dtCurentRow[strPointXBase]) + ",";
            if (NulltoInt(dtCurentRow[strPointYPlus]) == 0)
            {
                strArrow = strArrow + "↑";
            }
            else
            {
                strArrow = strArrow + "↓";
            }
            strPoint = strPoint + NulltoString(dtCurentRow[strPointYBase]) + ")";

        }

        /// <summary>
        /// 座標ラベル表示非表示切り替え
        /// </summary>
        private void DispPointChange()
        {
            // 初期化
            bool bolVsbColumn1Flg = false;
            bool bolVsbColumn2Flg = false;
            bool bolVsbColumn3Flg = false;
            bool bolVsbColumn4Flg = false;
            bool bolVsbColumn5Flg = false;

            bool bolVsbColumnThresholdAB = false;
            bool bolVsbColumnThresholdBC = false;
            bool bolVsbColumnThresholdCD = false;
            bool bolVsbColumnThresholdDE = false;

            bool bolVsbLineThresholdA = false;
            bool bolVsbLineThresholdB = false;
            bool bolVsbLineThresholdC = false;
            bool bolVsbLineThresholdD = false;
            bool bolVsbLineThresholdE = false;

            // 列数でコントロール表示フラグを設定する
            if (m_intColumn_cnt > 0)
            {
                bolVsbColumn1Flg = true;
            }
            if (m_intColumn_cnt > 1)
            {
                bolVsbColumn2Flg = true;
            }
            if (m_intColumn_cnt > 2)
            {
                bolVsbColumn3Flg = true;
            }
            if (m_intColumn_cnt > 3)
            {
                bolVsbColumn4Flg = true;
            }
            if (m_intColumn_cnt > 4)
            {
                bolVsbColumn5Flg = true;
            }

            if (bolVsbColumn2Flg &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdAB.Text))
            {
                bolVsbColumnThresholdAB = true;
            }

            if (bolVsbColumn3Flg &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdBC.Text))
            {
                bolVsbColumnThresholdBC = true;
            }

            if (bolVsbColumn4Flg &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdCD.Text))
            {
                bolVsbColumnThresholdCD = true;
            }

            if (bolVsbColumn5Flg &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdDE.Text))
            {
                bolVsbColumnThresholdDE = true;
            }

            if (bolVsbColumn1Flg &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdAFrom.Text) &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdATo.Text))
            {
                bolVsbLineThresholdA = true;
            }

            if (bolVsbColumn2Flg &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdBFrom.Text) &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdBTo.Text))
            {
                bolVsbLineThresholdB = true;
            }

            if (bolVsbColumn3Flg &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdCFrom.Text) &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdCTo.Text))
            {
                bolVsbLineThresholdC = true;
            }

            if (bolVsbColumn4Flg &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdDFrom.Text) &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdDTo.Text))
            {
                bolVsbLineThresholdD = true;
            }

            if (bolVsbColumn5Flg &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdEFrom.Text) &&
                !string.IsNullOrWhiteSpace(txtColumnThresholdETo.Text))
            {
                bolVsbLineThresholdE = true;
            }

            // フラグからコントロールの表示非表示を設定する
            // 基準点
            lblBasePointA.Visible = bolVsbColumn1Flg;
            lblTitlePlusDirectionA.Visible = bolVsbColumn1Flg;
            lblPlusDirectionA.Visible = bolVsbColumn1Flg;
            lblBasePointB.Visible = bolVsbColumn2Flg;
            lblTitlePlusDirectionB.Visible = bolVsbColumn2Flg;
            lblPlusDirectionB.Visible = bolVsbColumn2Flg;
            lblBasePointC.Visible = bolVsbColumn3Flg;
            lblTitlePlusDirectionC.Visible = bolVsbColumn3Flg;
            lblPlusDirectionC.Visible = bolVsbColumn3Flg;
            lblBasePointD.Visible = bolVsbColumn4Flg;
            lblTitlePlusDirectionD.Visible = bolVsbColumn4Flg;
            lblPlusDirectionD.Visible = bolVsbColumn4Flg;
            lblBasePointE.Visible = bolVsbColumn5Flg;
            lblTitlePlusDirectionE.Visible = bolVsbColumn5Flg;
            lblPlusDirectionE.Visible = bolVsbColumn5Flg;

            // 列判定用境界線設定
            txtColumnThresholdAB.Visible = bolVsbColumnThresholdAB;
            txtColumnThresholdBC.Visible = bolVsbColumnThresholdBC;
            txtColumnThresholdCD.Visible = bolVsbColumnThresholdCD;
            txtColumnThresholdDE.Visible = bolVsbColumnThresholdDE;

            // 行判定用境界線設定
            txtColumnThresholdAFrom.Visible = bolVsbLineThresholdA;
            txtColumnThresholdATo.Visible = bolVsbLineThresholdA;
            lblADash.Visible = bolVsbLineThresholdA;
            txtColumnThresholdBFrom.Visible = bolVsbLineThresholdB;
            txtColumnThresholdBTo.Visible = bolVsbLineThresholdB;
            lblBDash.Visible = bolVsbLineThresholdB;
            txtColumnThresholdCFrom.Visible = bolVsbLineThresholdC;
            txtColumnThresholdCTo.Visible = bolVsbLineThresholdC;
            lblCDash.Visible = bolVsbLineThresholdC;
            txtColumnThresholdDFrom.Visible = bolVsbLineThresholdD;
            txtColumnThresholdDTo.Visible = bolVsbLineThresholdD;
            lblDDash.Visible = bolVsbLineThresholdD;
            txtColumnThresholdEFrom.Visible = bolVsbLineThresholdE;
            txtColumnThresholdETo.Visible = bolVsbLineThresholdE;
            lblEDash.Visible = bolVsbLineThresholdE;
        }

        /// <summary>
        /// ピクチャボックス設定
        /// </summary>
        private void SettingPicBox()
        {
            // 表示領域とマスタ画像実寸との比率を算出
            // ※実寸：640x480
            picMasterImage.Location = new Point(0, 0);
            picMasterImage.Size = new Size(m_CON_REALITY_SIZE_W, m_CON_REALITY_SIZE_H);
            m_dblSizeRateW = (double)pnlMasterImage.ClientSize.Width / (double)picMasterImage.ClientSize.Width;
            m_dblSizeRateH = (double)pnlMasterImage.ClientSize.Height / (double)picMasterImage.ClientSize.Height;

            // 縦横の小さい方を比率にする
            if (m_dblSizeRateW < m_dblSizeRateH)
            {
                m_dblSizeRate = m_dblSizeRateW;
            }
            else
            {
                m_dblSizeRate = m_dblSizeRateH;
            }

            // 比率に合わせて、マスタ画像サイズを調整する
            picMasterImage.Size = new Size((int)((double)picMasterImage.ClientSize.Width * m_dblSizeRate), (int)((double)picMasterImage.ClientSize.Height * m_dblSizeRate));

            // 表示位置を調整する(中心)
            picMasterImage.Location = new Point((int)((double)pnlMasterImage.ClientSize.Width - (double)picMasterImage.ClientSize.Width) / 2,
                                                (int)((double)pnlMasterImage.ClientSize.Height - (double)picMasterImage.ClientSize.Height) / 2);

        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <returns></returns>
        private Boolean InputCheck()
        {
            // 必須入力チェック
            if (CheckRequiredInput(txtProductName, "品名") == false ||
                CheckRequiredInput(txtStretchRateX, "伸縮率（Xb）[%]") == false ||
                CheckRequiredInput(txtStretchRateY, "伸縮率（Yb）[%]") == false ||
                CheckRequiredInput(txtColumnThresholdAB, "A-B列") == false ||
                CheckRequiredInput(txtColumnThresholdBC, "B-C列") == false ||
                CheckRequiredInput(txtColumnThresholdCD, "C-D列") == false ||
                CheckRequiredInput(txtColumnThresholdDE, "D-E列") == false ||
                CheckRequiredInput(txtColumnThresholdAFrom, "A列(開始)") == false ||
                CheckRequiredInput(txtColumnThresholdATo, "A列(終了)") == false ||
                CheckRequiredInput(txtColumnThresholdBFrom, "B列(開始)") == false ||
                CheckRequiredInput(txtColumnThresholdBTo, "B列(終了)") == false ||
                CheckRequiredInput(txtColumnThresholdCFrom, "C列(開始)") == false ||
                CheckRequiredInput(txtColumnThresholdCTo, "C列(終了)") == false ||
                CheckRequiredInput(txtColumnThresholdDFrom, "D列(開始)") == false ||
                CheckRequiredInput(txtColumnThresholdDTo, "D列(終了)") == false ||
                CheckRequiredInput(txtColumnThresholdEFrom, "E列(開始)") == false ||
                CheckRequiredInput(txtColumnThresholdETo, "E列(終了)") == false)
            {
                return false;
            }

            // 範囲チェック
            if (CheckRangeInputDouble(txtStretchRateX, "伸縮率（Xb）[%]", 1.00, 300.00) == false ||
                CheckRangeInputDouble(txtStretchRateY, "伸縮率（Yb）[%]", 1.00, 300.00) == false ||
                CheckRangeInput(txtColumnThresholdAB, "A-B列", 1, 480) == false ||
                CheckRangeInput(txtColumnThresholdBC, "B-C列", 1, 480) == false ||
                CheckRangeInput(txtColumnThresholdCD, "C-D列", 1, 480) == false ||
                CheckRangeInput(txtColumnThresholdDE, "D-E列", 1, 480) == false ||
                CheckRangeInput(txtColumnThresholdAFrom, "A列(開始)", 1, 640) == false ||
                CheckRangeInput(txtColumnThresholdATo, "A列(終了)", 1, 640) == false ||
                CheckRangeInput(txtColumnThresholdBFrom, "B列(開始)", 1, 640) == false ||
                CheckRangeInput(txtColumnThresholdBTo, "B列(終了)", 1, 640) == false ||
                CheckRangeInput(txtColumnThresholdCFrom, "C列(開始)", 1, 640) == false ||
                CheckRangeInput(txtColumnThresholdCTo, "C列(終了)", 1, 640) == false ||
                CheckRangeInput(txtColumnThresholdDFrom, "D列(開始)", 1, 640) == false ||
                CheckRangeInput(txtColumnThresholdDTo, "D列(終了)", 1, 640) == false ||
                CheckRangeInput(txtColumnThresholdEFrom, "E列(開始)", 1, 640) == false ||
                CheckRangeInput(txtColumnThresholdETo, "E列(終了)", 1, 640) == false)
            {
                return false;
            }

            // 大小チェック
            if (CheckRangeInputData(txtColumnThresholdAB, txtColumnThresholdBC, "列判定用境界線設定", "A-B列", "B-C列", true) == false ||
                CheckRangeInputData(txtColumnThresholdBC, txtColumnThresholdCD, "列判定用境界線設定", "B-C列", "C-D列", true) == false ||
                CheckRangeInputData(txtColumnThresholdCD, txtColumnThresholdDE, "列判定用境界線設定", "C-D列", "D-E列", true) == false ||
                CheckRangeInputData(txtColumnThresholdAFrom, txtColumnThresholdATo, "行判定用境界線設定", "A列(開始)", "A列(終了)", true) == false ||
                CheckRangeInputData(txtColumnThresholdBFrom, txtColumnThresholdBTo, "行判定用境界線設定", "B列(開始)", "B列(終了)", true) == false ||
                CheckRangeInputData(txtColumnThresholdCFrom, txtColumnThresholdCTo, "行判定用境界線設定", "C列(開始)", "C列(終了)", true) == false ||
                CheckRangeInputData(txtColumnThresholdDFrom, txtColumnThresholdDTo, "行判定用境界線設定", "D列(開始)", "D列(終了)", true) == false ||
                CheckRangeInputData(txtColumnThresholdEFrom, txtColumnThresholdETo, "行判定用境界線設定", "E列(開始)", "E列(終了)", true) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 必須入力チェック
        /// </summary>
        /// <param name="strCheckData">チェック対象テキスト</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <param name="intRowCount">チェック対象行番号</param>
        /// <param name="intMaxLength">項目最大長</param>
        /// <returns></returns>
        private static Boolean CheckRequiredInput(TextBox txtCheckData
                                                , String strItemName)
        {
            // 境界線項目は無効になっている可能性があるため、有効な場合のみチェックする
            if (txtCheckData.Visible == true)
            {
                // 必須入力チェック
                if (string.IsNullOrEmpty(txtCheckData.Text))
                {
                    // エラーメッセージ出力を行う
                    MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0007, strItemName), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtCheckData.Focus();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 範囲チェック
        /// </summary>
        /// <param name="strCheckData">チェック対象テキスト</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <param name="intMinRange">項目最小範囲</param>
        /// <param name="intMaxRange">項目最大範囲</param>
        /// <returns></returns>
        private static Boolean CheckRangeInput(TextBox txtCheckData
                                             , String strItemName
                                             , Int32 intMinRange
                                             , Int32 intMaxRange)
        {
            // 未入力データの場合はチェックしない
            // ※未入力データは必須入力チェックではじく
            if (string.IsNullOrEmpty(txtCheckData.Text))
            {
                return true;
            }

            int intCheckData = NulltoInt(txtCheckData.Text);

            // 範囲チェック
            if (intCheckData < intMinRange || intCheckData > intMaxRange)
            {
                // ログファイルにエラー出力を行う
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0023, strItemName, intMinRange, intMaxRange),
                                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCheckData.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 範囲チェック（少数あり）
        /// </summary>
        /// <param name="strCheckData">チェック対象テキスト</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <param name="intMinRange">項目最小範囲</param>
        /// <param name="intMaxRange">項目最大範囲</param>
        /// <returns></returns>
        private static Boolean CheckRangeInputDouble(TextBox txtCheckData
                                                   , String strItemName
                                                   , double dblMinRange
                                                   , double dblMaxRange)
        {
            // 未入力データの場合はチェックしない
            // ※未入力データは必須入力チェックではじく
            if (string.IsNullOrEmpty(txtCheckData.Text))
            {
                return true;
            }

            double dblCheckData = NulltoDbl(txtCheckData.Text);

            // 桁数チェック
            if (dblCheckData < dblMinRange || dblCheckData > dblMaxRange)
            {
                // エラーメッセージ出力を行う
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0023, strItemName, dblMinRange, dblMaxRange),
                                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCheckData.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 大小チェック
        /// </summary>
        /// <param name="txtCheckDataFrom">チェック対象テキスト(From)</param>
        /// <param name="txtCheckDataTo">チェック対象テキスト(To)</param>
        /// <param name="strErrorLocation">エラー箇所</param>
        /// <param name="strItemNameFrom">チェック対象項目名From</param>
        /// <param name="strItemNameTo">チェック対象項目名To</param>
        /// <param name="bolIsFocusSetting">フォーカス設定フラグ</param>
        /// <returns>チェック結果</returns>
        private static Boolean CheckRangeInputData(TextBox txtCheckDataFrom
                                                   , TextBox txtCheckDataTo
                                                   , string strErrorLocation
                                                   , string strItemNameFrom
                                                   , string strItemNameTo
                                                   , bool bolIsFocusSetting)
        {
            // 未入力データの場合はチェックしない
            // ※未入力データは必須入力チェックではじく
            if (string.IsNullOrEmpty(txtCheckDataFrom.Text) ||
                string.IsNullOrEmpty(txtCheckDataTo.Text))
            {
                return true;
            }

            int intCheckDataFrom = NulltoInt(txtCheckDataFrom.Text);
            int intCheckDataTo = NulltoInt(txtCheckDataTo.Text);

            // 範囲チェック
            if (intCheckDataFrom >= intCheckDataTo)
            {
                // ログファイルにエラー出力を行う
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0056, strErrorLocation, strItemNameFrom, strItemNameTo),
                                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (bolIsFocusSetting)
                {
                    txtCheckDataFrom.Focus();
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 品名更新処理
        /// </summary>
        /// <returns></returns>
        private Boolean UPDMstProductInfoDisp()
        {
            try
            {
                // SQL文を作成する
                string strUpdateSql = g_CON_UPDATE_MST_PRODUCT_INFO_DISP_INPUT;

                int intChkFlg = 0;
                if (chkAiModelNonInspectionFlg.Checked == true)
                {
                    intChkFlg = 1;
                }

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ai_model_non_inspection_flg", DbType = DbType.Int32, Value = intChkFlg });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ai_model_name", DbType = DbType.String, Value = txtAiModelName.Text });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "stretch_rate_x", DbType = DbType.Double, Value = NulltoDbl(txtStretchRateX.Text) });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "stretch_rate_y", DbType = DbType.Double, Value = NulltoDbl(txtStretchRateY.Text) });
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdAB, "column_threshold_01"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdBC, "column_threshold_02"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdCD, "column_threshold_03"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdDE, "column_threshold_04"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdAFrom, "line_threshold_a1"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdATo, "line_threshold_a2"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdBFrom, "line_threshold_b1"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdBTo, "line_threshold_b2"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdCFrom, "line_threshold_c1"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdCTo, "line_threshold_c2"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdDFrom, "line_threshold_d1"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdDTo, "line_threshold_d2"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdEFrom, "line_threshold_e1"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThresholdETo, "line_threshold_e2"));

                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "product_name", DbType = DbType.String, Value = txtProductName.Text });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);

                g_clsConnectionNpgsql.DbCommit();

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0022, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtProductName.Focus();
                return false;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// NULL値変換
        /// </summary>
        /// <param name="txtValue"></param>
        /// <param name="strParamName"></param>
        /// <returns></returns>
        private ConnectionNpgsql.structParameter DBOrInt(TextBox txtValue
                                                       , string strParamName)
        {
            ConnectionNpgsql.structParameter cnsStructParameter;

            // 値がある場合設定
            if (string.IsNullOrEmpty(txtValue.Text))
            {
                cnsStructParameter = new ConnectionNpgsql.structParameter { ParameterName = strParamName, DbType = DbType.Int32, Value = DBNull.Value };
            }
            else
            {
                cnsStructParameter = new ConnectionNpgsql.structParameter { ParameterName = strParamName, DbType = DbType.Int32, Value = NulltoInt(txtValue.Text) };
            }

            return cnsStructParameter;
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
