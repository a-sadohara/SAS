using System;
using System.Collections.Generic;
using System.Data;
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

        // マスタ列名
        private const string m_CON_COLNAME_PRODUCT_NAME = "product_name";
        private const string m_CON_COLNAME_AIRBAG_IMAGEPATH = "airbag_imagepath";
        private const string m_CON_COLNAME_LENGTH = "length";
        private const string m_CON_COLNAME_WIDTH = "width";
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

        // 変数
        private double m_dblSizeRateW = 100.00;
        private double m_dblSizeRateH = 100.00;
        private double m_dblSizeRate = 100.00;
        private int m_intColumn_cnt = 5;
        private DataTable m_dtData = new DataTable();
        private string m_strMstFilePath = "";

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
            bool bolProcOkNg = false;

            try
            {
                // 初期化
                txtProductName.Text = "";
                lblLength.Text = "";
                lblWidth.Text = "";
                lblLineLength.Text = "";
                lblRegimarkBetweenLength.Text = "";
                txtStretchRateX.Text = "";
                txtStretchRateY.Text = "";
                chkAiModelNonInspectionFlg.Checked = true;
                txtAiModelName.Text = "";
                picMasterImage.Image = null;
                lblStartRegimarkPointN.Text = "";
                lblStartRegimarkPointNPlus1Line.Text = "";
                lblEndRegimarkPointN.Text = "";
                lblEndRegimarkPointNMinus1Line.Text = "";
                lblBasePointA.Text = "";
                lblBasePointB.Text = "";
                lblBasePointC.Text = "";
                lblBasePointD.Text = "";
                lblBasePointE.Text = "";
                lblPlusDirectionA.Text = "";
                lblPlusDirectionB.Text = "";
                lblPlusDirectionC.Text = "";
                lblPlusDirectionD.Text = "";
                lblPlusDirectionE.Text = "";
                lblStartRegimarkCameraNum.Text = "";
                lblEndRegimarkCameraNum.Text = "";
                lblIlluminationInformation.Text = "";
                txtColumnThresholdAB.Text = "";
                txtColumnThresholdBC.Text = "";
                txtColumnThresholdCD.Text = "";
                txtColumnThresholdDE.Text = "";
                txtColumnThresholdAFrom.Text = "";
                txtColumnThresholdATo.Text = "";
                txtColumnThresholdBFrom.Text = "";
                txtColumnThresholdBTo.Text = "";
                txtColumnThresholdCFrom.Text = "";
                txtColumnThresholdCTo.Text = "";
                txtColumnThresholdDFrom.Text = "";
                txtColumnThresholdDTo.Text = "";
                txtColumnThresholdEFrom.Text = "";
                txtColumnThresholdETo.Text = "";

                // 品番マスタから値の取得を行う
                if (GetHinMstInitial("") == false)
                {
                    txtProductName.Select();
                    return;
                }
                if (m_dtData.Rows.Count == 0)
                    if (bolRunProductMstImportCsv() == false)
                        return;

                // 取得結果反映処理
                CreateFormInfo();

                bolProcOkNg = true;
            }
            finally
            {
                if (bolProcOkNg == false)
                    this.Close();
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
            if (frmProductMstSelection.strHinNm != "")
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
            }
        }

        /// <summary>
        /// 境界線テキストボックス値変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtThreshold_Leave(object sender, EventArgs e)
        {
            DrawThreshold();
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
            // 確認メッセージ
            if (MessageBox.Show(g_clsMessageInfo.strMsgQ0005, "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
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

                // 取得結果反映処理
                CreateFormInfo();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 更新ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // 確認メッセージ
            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0006, txtProductName.Text), "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) 
            {
                // 入力チェックを行う
                if (InputCheck() == false) 
                {
                    return;
                }

                // 更新処理を行う
                if (UPDMstProductInfoDisp() == false)
                    return;
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
            }
            else
            {
                foreach (dynamic ctr in this.Controls.Find(strPicBoxDispLineCtrName, true))
                {
                    pctLineCtrl = ctr;
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
                else if (m_intColumn_cnt > 2 && txtColumnThreshold == txtColumnThresholdBC)
                {
                    intColumnThreshold = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 3 && txtColumnThreshold == txtColumnThresholdCD)
                {
                    intColumnThreshold = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 4 && txtColumnThreshold == txtColumnThresholdDE)
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
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdAB.Text) * m_dblSizeRate);
                }
                else if (txtLineThreshold == txtColumnThresholdATo)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdATo.Text) * m_dblSizeRate);
                    intColumnThresholdTop = 0;
                    if (m_intColumn_cnt > 1)
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdAB.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 1 && txtLineThreshold == txtColumnThresholdBFrom)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdBFrom.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdAB.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 2)
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 1 && txtLineThreshold == txtColumnThresholdBTo)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdBTo.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdAB.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 2)
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 2 && txtLineThreshold == txtColumnThresholdCFrom)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdCFrom.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 3)
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 2 && txtLineThreshold == txtColumnThresholdCTo)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdCTo.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdBC.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 3)
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 3 && txtLineThreshold == txtColumnThresholdDFrom)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdDFrom.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 4)
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdDE.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 3 && txtLineThreshold == txtColumnThresholdDTo)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdDTo.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdCD.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 4)
                        intColumnThresholdBottom = (int)((double)NulltoInt(txtColumnThresholdDE.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 4 && txtLineThreshold == txtColumnThresholdEFrom)
                {
                    intLineThreshold = (int)((double)NulltoInt(txtColumnThresholdEFrom.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)NulltoInt(txtColumnThresholdDE.Text) * m_dblSizeRate);
                    intColumnThresholdBottom = picMasterImage.ClientSize.Height;
                }
                else if (m_intColumn_cnt > 4 && txtLineThreshold == txtColumnThresholdETo)
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
                string strSQL = "";

                if (strProductName == "")
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
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "product_name"
                                                                              , DbType = DbType.String
                                                                              , Value = strProductName });
                    g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0001 + "\r\n" + ex.Message);
                MessageBox.Show(g_clsMessageInfo.strMsgE0021, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }

        /// <summary>
        /// 画面反映処理
        /// </summary>
        private void CreateFormInfo() 
        {
            // 取得結果描画
            MappingDtToForm(m_dtData);

            // ファイルが存在するか確認
            if (File.Exists(m_strMstFilePath) == true)
            {
                // マスタ画像イメージ反映
                picMasterImage.Image = System.Drawing.Image.FromFile(m_strMstFilePath);
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
            // 行数分ループする
            foreach (DataRow dtCurentRow in dtData.Rows)
            {
                // 品名
                txtProductName.Text = NulltoString(dtCurentRow[m_CON_COLNAME_PRODUCT_NAME]);

                //エアバック画像ファイルパス
                m_strMstFilePath = NulltoString(dtCurentRow[m_CON_COLNAME_AIRBAG_IMAGEPATH]);

                // 長さ
                if (NulltoString(dtCurentRow[m_CON_COLNAME_LENGTH]) != "")
                    lblLength.Text = String.Format("{0:#,0}", int.Parse(NulltoString(dtCurentRow[m_CON_COLNAME_LENGTH])));
                else
                    lblLength.Text = "";

                // 幅
                if (NulltoString(dtCurentRow[m_CON_COLNAME_WIDTH]) != "")
                    lblWidth.Text = String.Format("{0:#,0}", int.Parse(NulltoString(dtCurentRow[m_CON_COLNAME_WIDTH])));
                else
                    lblWidth.Text = "";

                // レジマーク間引き
                if (NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_BETWEEN_LENGTH]) != "")
                    lblRegimarkBetweenLength.Text = String.Format("{0:#,0}", int.Parse(NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_BETWEEN_LENGTH])));
                else
                    lblRegimarkBetweenLength.Text = "";

                // 伸縮率X
                string strStretchRateX = NulltoString(dtCurentRow[m_CON_COLNAME_STRETCH_RATE_X_UPD]);
                if (strStretchRateX == "")
                {
                    strStretchRateX = NulltoString(dtCurentRow[m_CON_COLNAME_STRETCH_RATE_X]);
                }
                txtStretchRateX.Text = strStretchRateX;

                // 伸縮率Y
                string strStretchRateY = NulltoString(dtCurentRow[m_CON_COLNAME_STRETCH_RATE_Y_UPD]);
                if (strStretchRateY == "")
                {
                    strStretchRateY = NulltoString(dtCurentRow[m_CON_COLNAME_STRETCH_RATE_Y]);
                }
                txtStretchRateY.Text = strStretchRateY;

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
                txtAiModelName.Text = NulltoString(dtCurentRow[m_CON_COLNAME_AI_MODEL_NAME]);

                // レジマーク間の長さ（pixel）
                decimal dc = (640 * NulltoInt(dtCurentRow[m_CON_COLNAME_REGIMARK_BETWEEN_LENGTH]) /
                                    NulltoInt(dtCurentRow[m_CON_COLNAME_LENGTH]));
                m_intRegiMarkLengthPx = NulltoInt(Math.Floor(dc));

                // レジマーク表示部.開始レジマーク座標.N行
                lblStartRegimarkPointN.Text = "(" + NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_1_POINT_X]) + "," +
                                                    NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_1_POINT_Y]) + ")";

                // レジマーク表示部.終了レジマーク座標.N行
                lblEndRegimarkPointN.Text = "(" + NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_2_POINT_X]) + "," +
                                                  NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_2_POINT_Y]) + ")";

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

                if (strPoint != "")
                {
                    m_intColumn_cnt = 5;
                }

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

                if (txtColumnThresholdAFrom.Text != "" && txtColumnThresholdATo.Text != "")
                {
                    m_intColumn_cnt = 1;
                }

                // 行判定用境界線設定表示部.B列
                txtColumnThresholdBFrom.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_B1]);
                txtColumnThresholdBTo.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_B2]);

                if (txtColumnThresholdBFrom.Text != "" && txtColumnThresholdBTo.Text != "")
                {
                    m_intColumn_cnt = 2;
                }

                // 行判定用境界線設定表示部.C列
                txtColumnThresholdCFrom.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_C1]);
                txtColumnThresholdCTo.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_C2]);

                if (txtColumnThresholdCFrom.Text != "" && txtColumnThresholdCTo.Text != "")
                {
                    m_intColumn_cnt = 3;
                }

                // 行判定用境界線設定表示部.D列
                txtColumnThresholdDFrom.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_D1]);
                txtColumnThresholdDTo.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_D2]);

                if (txtColumnThresholdDFrom.Text != "" && txtColumnThresholdDTo.Text != "")
                {
                    m_intColumn_cnt = 4;
                }

                // 行判定用境界線設定表示部.E列
                txtColumnThresholdEFrom.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_E1]);
                txtColumnThresholdETo.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_E2]);

                if (txtColumnThresholdEFrom.Text != "" && txtColumnThresholdETo.Text != "")
                {
                    m_intColumn_cnt = 5;
                }

                // 開始レジマーク座標.N行+1行
                string strPointNPlus1 = "";
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
            }
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
            strPoint = "";
            strArrow = "";

            // 未設定の場合は空文字
            if (NulltoString(dtCurentRow[strPointXBase]) == "" &&
                NulltoString(dtCurentRow[strPointYBase]) == "")
            {
                return;
            }

            strPoint = "(";
            if (NulltoInt(dtCurentRow[strPointXPlus]) == 1)
            {
                strPoint = strPoint + "-";
                strArrow = strArrow + "←";
            }
            else
            {
                strArrow = strArrow + "→";
            }

            strPoint = strPoint + NulltoString(dtCurentRow[strPointXBase]) + ",";
            if (NulltoInt(dtCurentRow[strPointYPlus]) == 0)
            {
                strPoint = strPoint + "-";
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
            // 列数でコントロールを非表示にする
            if (m_intColumn_cnt < 2)
            {
                txtColumnThresholdAB.Visible = false;
                txtColumnThresholdBFrom.Visible = false;
                txtColumnThresholdBTo.Visible = false;
                lblBDash.Visible = false;
            }
            else 
            {
                txtColumnThresholdAB.Visible = true;
                txtColumnThresholdBFrom.Visible = true;
                txtColumnThresholdBTo.Visible = true;
                lblBDash.Visible = true;
            }

            if (m_intColumn_cnt < 3)
            {
                txtColumnThresholdBC.Visible = false;
                txtColumnThresholdCFrom.Visible = false;
                txtColumnThresholdCTo.Visible = false;
                lblCDash.Visible = false;
            }
            else
            {
                txtColumnThresholdBC.Visible = true;
                txtColumnThresholdCFrom.Visible = true;
                txtColumnThresholdCTo.Visible = true;
                lblCDash.Visible = true;
            }

            if (m_intColumn_cnt < 4)
            {
                txtColumnThresholdCD.Visible = false;
                txtColumnThresholdDFrom.Visible = false;
                txtColumnThresholdDTo.Visible = false;
                lblDDash.Visible = false;
            }
            else 
            {
                txtColumnThresholdCD.Visible = true;
                txtColumnThresholdDFrom.Visible = true;
                txtColumnThresholdDTo.Visible = true;
                lblDDash.Visible = true;
            }

            if (m_intColumn_cnt < 5)
            {
                txtColumnThresholdDE.Visible = false;
                txtColumnThresholdEFrom.Visible = false;
                txtColumnThresholdETo.Visible = false;
                lblEDash.Visible = false;
            }
            else 
            {
                txtColumnThresholdDE.Visible = true;
                txtColumnThresholdEFrom.Visible = true;
                txtColumnThresholdETo.Visible = true;
                lblEDash.Visible = true;
            }
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
                if (txtCheckData.Text == "")
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
            if (txtCheckData.Text == "")
            {
                return true;
            }

            int intCheckData = NulltoInt(txtCheckData.Text);

            // 範囲チェック
            if (intCheckData < intMinRange || intCheckData > intMaxRange)
            {
                // ログファイルにエラー出力を行う
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0023, intMinRange, intMaxRange), 
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
            if (txtCheckData.Text == "")
            {
                return true;
            }

            double dblCheckData = NulltoDbl(txtCheckData.Text);

            // 桁数チェック
            if (dblCheckData < dblMinRange || dblCheckData > dblMaxRange)
            {
                // エラーメッセージ出力を行う
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0023, dblMinRange, dblMaxRange), 
                                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCheckData.Focus();
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
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "stretch_rate_x_upd", DbType = DbType.Double, Value = NulltoDbl(txtStretchRateX.Text) });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "stretch_rate_y_upd", DbType = DbType.Double, Value = NulltoDbl(txtStretchRateY.Text) });
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
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0002 + "\r\n" + ex.Message);
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
            if (txtValue.Text == "")
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

    }
}
