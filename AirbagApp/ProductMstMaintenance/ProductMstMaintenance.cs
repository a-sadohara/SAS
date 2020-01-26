using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // 品番マスタから値の取得を行う
            if (GetHinMstInitial("") == false || 
                m_dtData.Rows.Count == 0)
            {
                txtProductName.Select();
                return;
            }

            // 取得結果反映処理
            CreateFormInfo();
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
        private void btnProMstImport_Click(object sender, EventArgs e)
        {
            string strMsg = "品番情報の取り込みを行います。" + Environment.NewLine;
            strMsg = strMsg + "画面の内容を更新している場合は、更新内容が破棄されます。" + Environment.NewLine;
            strMsg = strMsg + "処理を継続してよろしいでしょうか。";

            // 確認メッセージ
            if (MessageBox.Show(strMsg
                          , "確認"
                          , MessageBoxButtons.YesNo
                          , MessageBoxIcon.Information) == DialogResult.Yes) 
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
                    return;
                }

                // 取得結果反映処理
                CreateFormInfo();
            }
        }

        /// <summary>
        /// 更新ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string strMsg = "品名：" + txtProductName.Text + "の登録情報を更新します。" + Environment.NewLine;
            strMsg = strMsg + "処理を継続してよろしいでしょうか。";

            // 確認メッセージ
            if (MessageBox.Show(strMsg
                              , "確認"
                              , MessageBoxButtons.YesNo
                              , MessageBoxIcon.Information) == DialogResult.Yes) 
            {
                // 入力チェックを行う
                if (InputCheck() == false) 
                {
                    return;
                }
                // 更新処理を行う
                if (UPDMstProductInfoDisp() == true) 
                {
                    MessageBox.Show("更新が完了しました"
                                  , "確認"
                                  , MessageBoxButtons.OK
                                  , MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// 削除ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, EventArgs e)
        {
            string strMsg = "品名：" + txtProductName.Text + "の登録情報を削除します。" + Environment.NewLine;
            strMsg = strMsg + "処理を継続してよろしいでしょうか。";

            // 確認メッセージ
            if (MessageBox.Show(strMsg
                              , "確認"
                              , MessageBoxButtons.YesNo
                              , MessageBoxIcon.Information) == DialogResult.Yes)
            {

                // 削除処理を行う
                if (DELMstProductInfoDisp() == true) 
                {
                    MessageBox.Show("削除が完了しました"
                                  , "確認"
                                  , MessageBoxButtons.OK
                                  , MessageBoxIcon.Information);

                    // 画面初期表示
                    // 品番マスタから値の取得を行う
                    if (GetHinMstInitial("") == false ||
                        m_dtData.Rows.Count == 0)
                    {
                        txtProductName.Select();
                        return;
                    }

                    // 取得結果反映処理
                    CreateFormInfo();
                }
            }
        }

        /// <summary>
        /// 判定理由マスタボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnErrorReason_Click(object sender, EventArgs e)
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
            foreach (TextBox txtColumnThreshold in new TextBox[] { txtColumnThreshold01, txtColumnThreshold02,
                                                                   txtColumnThreshold03, txtColumnThreshold04 })
            {
                // 初期化
                intColumnThreshold = -1;

                if (m_intColumn_cnt > 1 && txtColumnThreshold == txtColumnThreshold01)
                {
                    intColumnThreshold = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 2 && txtColumnThreshold == txtColumnThreshold02)
                {
                    intColumnThreshold = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 3 && txtColumnThreshold == txtColumnThreshold03)
                {
                    intColumnThreshold = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 4 && txtColumnThreshold == txtColumnThreshold04)
                {
                    intColumnThreshold = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
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
            foreach (TextBox txtLineThreshold in new TextBox[] { txtLineThresholdA1, txtLineThresholdA2,
                                                                 txtLineThresholdB1, txtLineThresholdB2,
                                                                 txtLineThresholdC1, txtLineThresholdC2,
                                                                 txtLineThresholdD1, txtLineThresholdD2,
                                                                 txtLineThresholdE1, txtLineThresholdE2 })
            {
                // 初期化
                intLineThreshold = -1;
                intColumnThresholdTop = -1;
                intColumnThresholdBottom = -1;

                if (txtLineThreshold == txtLineThresholdA1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdA1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = 0;
                    if (m_intColumn_cnt > 1)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                }
                else if (txtLineThreshold == txtLineThresholdA2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdA2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = 0;
                    if (m_intColumn_cnt > 1)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 1 && txtLineThreshold == txtLineThresholdB1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdB1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 2)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 1 && txtLineThreshold == txtLineThresholdB2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdB2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold01.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 2)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 2 && txtLineThreshold == txtLineThresholdC1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdC1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 3)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 2 && txtLineThreshold == txtLineThresholdC2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdC2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold02.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 3)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 3 && txtLineThreshold == txtLineThresholdD1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdD1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 4)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 3 && txtLineThreshold == txtLineThresholdD2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdD2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold03.Text) * m_dblSizeRate);
                    if (m_intColumn_cnt > 4)
                        intColumnThresholdBottom = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
                }
                else if (m_intColumn_cnt > 4 && txtLineThreshold == txtLineThresholdE1)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdE1.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
                    intColumnThresholdBottom = picMasterImage.ClientSize.Height;
                }
                else if (m_intColumn_cnt > 4 && txtLineThreshold == txtLineThresholdE2)
                {
                    intLineThreshold = (int)((double)int.Parse(txtLineThresholdE2.Text) * m_dblSizeRate);
                    intColumnThresholdTop = (int)((double)int.Parse(txtColumnThreshold04.Text) * m_dblSizeRate);
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
                WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。" + ex.Message);
                MessageBox.Show("品番登録情報の取得で例外が発生しました。"
                              , "品番登録情報取得エラー"
                              , MessageBoxButtons.OK
                              , MessageBoxIcon.Warning);
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
                lblLength.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LENGTH]);

                // 幅
                lblWidth.Text = NulltoString(dtCurentRow[m_CON_COLNAME_WIDTH]);

                // レジマーク間引き
                lblRegiLength.Text = NulltoString(dtCurentRow[m_CON_COLNAME_REGIMARK_BETWEEN_LENGTH]);

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
                lblPointA.Text = strPoint;
                lblPlusDirectionA.Text = strArrow;
                if (strPoint != "") 
                {
                    m_intColumn_cnt = 1;
                }

                // 座標のラベルを作成 B
                CreatePointString(dtCurentRow
                                , m_CON_COLNAME_BASE_POINT_2_X
                                , m_CON_COLNAME_BASE_POINT_2_Y
                                , m_CON_COLNAME_POINT_2_PLUS_DIRECTION_X
                                , m_CON_COLNAME_POINT_2_PLUS_DIRECTION_Y
                                , out strPoint
                                , out strArrow);
                lblPointB.Text = strPoint;
                lblPlusDirectionB.Text = strArrow;

                if (strPoint != "")
                {
                    m_intColumn_cnt = 2;
                }

                // 座標のラベルを作成 C
                CreatePointString(dtCurentRow
                                , m_CON_COLNAME_BASE_POINT_3_X
                                , m_CON_COLNAME_BASE_POINT_3_Y
                                , m_CON_COLNAME_POINT_3_PLUS_DIRECTION_X
                                , m_CON_COLNAME_POINT_3_PLUS_DIRECTION_Y
                                , out strPoint
                                , out strArrow);
                lblPointC.Text = strPoint;
                lblPlusDirectionC.Text = strArrow;

                if (strPoint != "")
                {
                    m_intColumn_cnt = 3;
                }

                // 座標のラベルを作成 D
                CreatePointString(dtCurentRow
                                , m_CON_COLNAME_BASE_POINT_4_X
                                , m_CON_COLNAME_BASE_POINT_4_Y
                                , m_CON_COLNAME_POINT_4_PLUS_DIRECTION_X
                                , m_CON_COLNAME_POINT_4_PLUS_DIRECTION_Y
                                , out strPoint
                                , out strArrow);
                lblPointD.Text = strPoint;
                lblPlusDirectionD.Text = strArrow;

                if (strPoint != "")
                {
                    m_intColumn_cnt = 4;
                }

                // 座標のラベルを作成 E
                CreatePointString(dtCurentRow
                                , m_CON_COLNAME_BASE_POINT_5_X
                                , m_CON_COLNAME_BASE_POINT_5_Y
                                , m_CON_COLNAME_POINT_5_PLUS_DIRECTION_X
                                , m_CON_COLNAME_POINT_5_PLUS_DIRECTION_Y
                                , out strPoint
                                , out strArrow);
                lblPointE.Text = strPoint;
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
                txtColumnThreshold01.Text = NulltoString(dtCurentRow[m_CON_COLNAME_COLUMN_THRESHOLD_01]);
                // B-C列
                txtColumnThreshold02.Text = NulltoString(dtCurentRow[m_CON_COLNAME_COLUMN_THRESHOLD_02]);
                // C-D列
                txtColumnThreshold03.Text = NulltoString(dtCurentRow[m_CON_COLNAME_COLUMN_THRESHOLD_03]);
                // D-E列
                txtColumnThreshold04.Text = NulltoString(dtCurentRow[m_CON_COLNAME_COLUMN_THRESHOLD_04]);

                // 行判定用境界線設定表示部.A列
                txtLineThresholdA1.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_A1]);
                txtLineThresholdA2.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_A2]);

                // 行判定用境界線設定表示部.B列
                txtLineThresholdB1.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_B1]);
                txtLineThresholdB2.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_B2]);

                // 行判定用境界線設定表示部.C列
                txtLineThresholdC1.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_C1]);
                txtLineThresholdC2.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_C2]);

                // 行判定用境界線設定表示部.D列
                txtLineThresholdD1.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_D1]);
                txtLineThresholdD2.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_D2]);

                // 行判定用境界線設定表示部.E列
                txtLineThresholdE1.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_E1]);
                txtLineThresholdE2.Text = NulltoString(dtCurentRow[m_CON_COLNAME_LINE_THRESHOLD_E2]);

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
                txtColumnThreshold01.Visible = false;
                txtLineThresholdB1.Visible = false;
                txtLineThresholdB2.Visible = false;
                lblBDash.Visible = false;
            }
            if (m_intColumn_cnt < 3)
            {
                txtColumnThreshold02.Visible = false;
                txtLineThresholdC1.Visible = false;
                txtLineThresholdC2.Visible = false;
                lblCDash.Visible = false;
            }
            if (m_intColumn_cnt < 4)
            {
                txtColumnThreshold03.Visible = false;
                lblDDash.Visible = false;
            }
            if (m_intColumn_cnt < 5)
            {
                txtColumnThreshold04.Visible = false;
                txtLineThresholdE1.Visible = false;
                txtLineThresholdE2.Visible = false;
                lblEDash.Visible = false;
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
                CheckRequiredInput(txtLineThresholdA1, "A列(開始)") == false ||
                CheckRequiredInput(txtLineThresholdA2, "A列(終了)") == false)
            {
                return false;
            }

            // 範囲チェック
            if (CheckRangeInput(txtStretchRateX, "伸縮率（Xb）[%]", 1, 300) == false ||
                CheckRangeInput(txtStretchRateY, "伸縮率（Yb）[%]", 1, 300) == false ||
                CheckRangeInput(txtColumnThreshold01, "A-B列", 1, 480) == false ||
                CheckRangeInput(txtColumnThreshold02, "B-C列", 1, 480) == false ||
                CheckRangeInput(txtColumnThreshold03, "C-D列", 1, 480) == false ||
                CheckRangeInput(txtColumnThreshold04, "D-E列", 1, 480) == false ||
                CheckRangeInput(txtLineThresholdA1, "A列(開始)", 1, 640) == false ||
                CheckRangeInput(txtLineThresholdA2, "A列(終了)", 1, 640) == false ||
                CheckRangeInput(txtLineThresholdB1, "B列(開始)", 1, 640) == false ||
                CheckRangeInput(txtLineThresholdB2, "B列(終了)", 1, 640) == false ||
                CheckRangeInput(txtLineThresholdC1, "C列(開始)", 1, 640) == false ||
                CheckRangeInput(txtLineThresholdC2, "C列(終了)", 1, 640) == false ||
                CheckRangeInput(txtLineThresholdD1, "D列(開始)", 1, 640) == false ||
                CheckRangeInput(txtLineThresholdD2, "D列(終了)", 1, 640) == false ||
                CheckRangeInput(txtLineThresholdE1, "E列(開始)", 1, 640) == false ||
                CheckRangeInput(txtLineThresholdE2, "E列(終了)", 1, 640) == false)
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
            // 必須入力チェック
            if (txtCheckData.Text == "")
            {
                // ログファイルにエラー出力を行う
                MessageBox.Show(strItemName + "が未設定です。"
                              + strItemName + "を設定してください。"
                              , "確認"
                              , MessageBoxButtons.OK
                              , MessageBoxIcon.Warning);
                txtCheckData.Focus();
                return false;
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

            // 桁数チェック
            if (intCheckData < intMinRange || intCheckData > intMaxRange)
            {
                // ログファイルにエラー出力を行う
                // ログファイルにエラー出力を行う
                MessageBox.Show(strItemName + "の値が不正です。"
                                            + intMinRange.ToString() + "～" + intMaxRange.ToString()
                                            + "までを入力してください。 "
                                            , "確認"
                                            , MessageBoxButtons.OK
                                            , MessageBoxIcon.Warning);
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
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "stretch_rate_x_upd", DbType = DbType.Int32, Value = NulltoInt(txtStretchRateX.Text) });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "stretch_rate_y_upd", DbType = DbType.Int32, Value = NulltoInt(txtStretchRateY.Text) });
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThreshold01, "column_threshold_01"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThreshold02, "column_threshold_02"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThreshold03, "column_threshold_03"));
                lstNpgsqlCommand.Add(DBOrInt(txtColumnThreshold04, "column_threshold_04"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdA1, "line_threshold_a1"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdA2, "line_threshold_a2"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdB1, "line_threshold_b1"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdB2, "line_threshold_b2"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdC1, "line_threshold_c1"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdC2, "line_threshold_c2"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdD1, "line_threshold_d1"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdD2, "line_threshold_d2"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdE1, "line_threshold_e1"));
                lstNpgsqlCommand.Add(DBOrInt(txtLineThresholdE2, "line_threshold_e2"));

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
                MessageBox.Show("品番登録情報の更新で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtProductName.Focus();
                return false;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 品名削除処理
        /// </summary>
        /// <returns></returns>
        private Boolean DELMstProductInfoDisp()
        {
            try
            {
                // SQL文を作成する
                string strUpdateSql = g_CON_DELETE_MST_PRODUCT_INFO_DISP_INPUT;

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
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
                MessageBox.Show("品番登録情報の削除で例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
