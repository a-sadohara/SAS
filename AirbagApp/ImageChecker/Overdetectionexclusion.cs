using ImageChecker.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class Overdetectionexclusion : Form
    {
        /// <summary>
        /// 遷移先
        /// </summary>
        public int intDestination { get; set; }

        // パラメータ関連（可変）
        private HeaderData g_clsHeaderData;                                 // ヘッダ情報

        // パラメータ関連（不変）
        private readonly string m_strUnitNum = string.Empty;                // 号機
        private readonly string m_strProductName = string.Empty;            // 品名
        private readonly string m_strOrderImg = string.Empty;               // 指図
        private readonly string m_strFabricName = string.Empty;             // 反番
        private readonly string m_strInspectionDate = string.Empty;         // 検査日付
        private readonly string m_strInspectionDirection = string.Empty;    // 検査方向
        private readonly int m_intInspectionNum = 0;                        // 検査番号
        private readonly int m_intOverDetectionExceptStatus = 0;            // 過検知除外ステータス

        // フラグ関連
        private bool m_bolRegFlg = false;       // 登録済み　　※true : 結果画面から修正以外で遷移
                                                // 　　　　　　　false: 初期表示,ログアウト,結果画面から修正で遷移
                                                // 　　　　　　　用途 : 本画面を閉じた時、ステータスを中断にするか否か制御する

        private bool m_bolUpdFlg = false;       // 修正モード　※true : 結果画面から修正で遷移
                                                // 　　　　　　　false: それ以外
                                                // 　　　　　　　用途 : 次ページを押下した時、結果画面に遷移するか否か制御する

        // ページ数関連
        private int m_intPageIdx = 0;
        private int m_intPageCnt = 1;

        // 欠点画像サブディレクトリパス
        private string m_strFaultImageSubDirectory = string.Empty;

        // データ保持関連
        private DataTable m_dtData;
        private Dictionary<int, string> m_dicKey;          // KEY:順番(画像表示数*ページIdx+画像Idx) VALUE:KEY(行_列_NG面_マーキング画像ファイル名)
        private Dictionary<string, string> m_dicState;     // KEY:行_列_NG面_マーキング画像ファイル名 VALUE:ステータス

        // クリックイベントとダブルクリックイベントの同時実装関連
        private readonly SemaphoreSlim _clickSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _doubleClickSemaphore = new SemaphoreSlim(0);

        // 選択行保持(結果画面用)
        private int m_intSelIdx = -1;
        private int m_intFirstDisplayedScrollingRowIdx = -1;

        // 定数
        private const string m_CON_FORMAT_UNIT_NUM = "号機：{0}";
        private const string m_CON_FORMAT_PRODUCT_NAME = "品名：{0}";
        private const string m_CON_FORMAT_ORDER_IMG = "指図：{0}";
        private const string m_CON_FORMAT_FABRIC_NAME = "反番：{0}";
        private const string m_CON_FORMAT_INSPECTION_NUM = "検査番号：{0}";
        private const string m_CON_FORMAT_WORKER_NAME = "作業者：{0}";
        private const string m_CON_FORMAT_COUNT_SCORES = "{0}/{1}";
        private const string m_CON_DELIMIT_KEY = "|";

        // 動的コントロール命名規則関連
        private const string m_CON_FORMAT_PICTUREBOX_NAME = "picImage{0}";
        private const string m_CON_FORMAT_LABEL_NAME = "lblImage{0}";

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clsHeaderData">ヘッダ情報</param>
        public Overdetectionexclusion(ref HeaderData clsHeaderData)
        {
            g_clsHeaderData = clsHeaderData;
            m_strUnitNum = clsHeaderData.strUnitNum;
            m_strProductName = clsHeaderData.strProductName;
            m_strOrderImg = clsHeaderData.strOrderImg;
            m_strFabricName = clsHeaderData.strFabricName;
            m_strInspectionDate = clsHeaderData.strInspectionDate;
            m_strInspectionDirection = clsHeaderData.strInspectionDirection;
            m_intInspectionNum = clsHeaderData.intInspectionNum;
            m_intOverDetectionExceptStatus = clsHeaderData.intOverDetectionExceptStatus;

            m_strFaultImageSubDirectory = string.Join("_", m_strInspectionDate.Replace("/", string.Empty),
                                                           m_strProductName,
                                                           m_strFabricName,
                                                           m_intInspectionNum);

            intDestination = -1;

            InitializeComponent();
        }

        /// <summary>
        /// 画像ファイル名取得
        /// </summary>
        /// <returns>true:正常終了 false:異常終了</returns>
        private bool bolGetImagePath()
        {
            string strSQL = string.Empty;
            int intIdx = 0;

            string strKey = string.Empty;
            int intOverDetectionExceptResult = g_clsSystemSettingInfo.intOverDetectionExceptResultNon;

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
                             , camera_num
                             , org_imagepath
                             , marking_imagepath
                             , over_detection_except_result
                           FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                           WHERE fabric_name = :fabric_name
                           AND   inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                           AND   inspection_num = :inspection_num
                           ORDER BY ";

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

                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);

                // ステータス保持
                m_dicKey = new Dictionary<int, string>();
                m_dicState = new Dictionary<string, string>();
                foreach (DataRow dr in m_dtData.Rows)
                {
                    strKey = string.Join(m_CON_DELIMIT_KEY,
                                         dr["line"].ToString(),
                                         dr["cloumns"].ToString(),
                                         dr["ng_face"].ToString(),
                                         dr["marking_imagepath"].ToString());

                    m_dicKey.Add(intIdx, strKey);

                    if (Convert.ToInt32(dr["over_detection_except_result"]) != g_clsSystemSettingInfo.intOverDetectionExceptResultNg)
                    {
                        // 過検知
                        m_dicState.Add(strKey, g_clsSystemSettingInfo.intOverDetectionExceptResultOk.ToString());
                    }
                    else
                    {
                        // AI検知
                        m_dicState.Add(strKey, g_clsSystemSettingInfo.intOverDetectionExceptResultNg.ToString());
                    }

                    intIdx++;
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0},{1},{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0050, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// 開始ページの取得
        /// </summary>
        /// <param name="intIdxStartPage">開始ページ</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private bool bolGetStartPageIdx(ref int intIdxStartPage)
        {
            intIdxStartPage = 0;

            string strSQL = string.Empty;
            DataTable dtData;
            int intMaxSortNum = 0;

            try
            {
                dtData = new DataTable();
                strSQL = @"SELECT COALESCE(MAX(sort_num),0) AS max_sort_num
                             FROM (
                                 SELECT ";

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionS)
                {
                    strSQL += "ROW_NUMBER() OVER(ORDER BY line ASC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC) AS sort_num";
                }

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionX)
                {
                    strSQL += "ROW_NUMBER() OVER(ORDER BY line ASC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC) AS sort_num";
                }

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionY)
                {
                    strSQL += "ROW_NUMBER() OVER(ORDER BY line DESC, cloumns ASC, ng_face ASC, camera_num ASC, org_imagepath ASC) AS sort_num";
                }

                if (m_strInspectionDirection == g_clsSystemSettingInfo.strInspectionDirectionR)
                {
                    strSQL += "ROW_NUMBER() OVER(ORDER BY line DESC, cloumns DESC, ng_face ASC, camera_num ASC, org_imagepath ASC) AS sort_num";
                }

                strSQL += @"       FROM " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                  WHERE fabric_name = :fabric_name
                                    AND inspection_date = TO_DATE(:inspection_date, 'YYYY/MM/DD')
                                    AND inspection_num = :inspection_num
                                    AND over_detection_except_datetime IS NOT NULL
                             ) AS MAIN";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });

                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                if (dtData.Rows.Count < 0)
                {
                    return true;
                }

                intMaxSortNum = Convert.ToInt32(dtData.Rows[0]["max_sort_num"]);

                if (intMaxSortNum == m_dtData.Rows.Count)
                {
                    intIdxStartPage = (intMaxSortNum - 1) / g_clsLoginInfo.intDispNum;
                }
                else
                {
                    intIdxStartPage = intMaxSortNum / g_clsLoginInfo.intDispNum;
                }

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
        /// ステータス切替
        /// </summary>
        /// <param name="picBox"></param>
        private void changePanelState(Label lblCtr)
        {
            int intIdxCtr = 0;
            string strKey = string.Empty;

            intIdxCtr = Convert.ToInt32(lblCtr.Name.Replace(string.Format(m_CON_FORMAT_LABEL_NAME, string.Empty), string.Empty));
            if (!m_dicKey.ContainsKey(intIdxCtr))
            {
                return;
            }

            strKey = m_dicKey[intGetStartDataIdxForPage(m_intPageIdx) + intIdxCtr];

            if (string.IsNullOrEmpty(lblCtr.Text))
            {
                lblCtr.Text = "NG";
                m_dicState[strKey] = g_clsSystemSettingInfo.intOverDetectionExceptResultNg.ToString();
            }
            else
            {
                lblCtr.Text = string.Empty;
                m_dicState[strKey] = g_clsSystemSettingInfo.intOverDetectionExceptResultOk.ToString();
            }
        }

        /// <summary>
        /// 画像表示
        /// </summary>
        /// <param name="intPageIdx"></param>
        private void dispImage(int intPageIdx)
        {
            int intImgIdxNum = 0;
            PictureBox pctImage = new PictureBox();
            Label lblImage = new Label();
            Control[] ctrImage;
            int intStartDataIdxForPage = -1;
            string strMarkingImagepath = "";

            // ページ数表示
            lblPageCount.Text = string.Format(m_CON_FORMAT_COUNT_SCORES, (intPageIdx + 1).ToString(), m_intPageCnt.ToString()); ;

            // 画像表示
            intImgIdxNum = 0;
            intStartDataIdxForPage = intGetStartDataIdxForPage(intPageIdx);
            for (int i = intStartDataIdxForPage; i < intStartDataIdxForPage + g_clsLoginInfo.intDispNum; i++)
            {
                ctrImage = this.Controls.Find(string.Format(m_CON_FORMAT_PICTUREBOX_NAME, intImgIdxNum), true);
                foreach (object ctr in ctrImage)
                {

                    pctImage = (PictureBox)ctr;

                    if (i >= m_dtData.Rows.Count)
                    {
                        // 画像イメージ非表示
                        pctImage.Image = null;
                        // ラベルステータス非表示
                        lblImage = (Label)pctImage.Controls[0];
                        lblImage.Text = string.Empty;

                        continue;
                    }

                    strMarkingImagepath = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory,
                                                       m_strFaultImageSubDirectory,
                                                       m_dtData.Rows[i]["marking_imagepath"].ToString());

                    // 画像イメージ表示
                    FileStream fs;
                    if (File.Exists(strMarkingImagepath) == false)
                    {
                        fs = new FileStream(g_CON_NO_IMAGE_FILE_PATH, FileMode.Open, FileAccess.Read);
                    }
                    else
                    {
                        fs = new FileStream(strMarkingImagepath, FileMode.Open, FileAccess.Read);
                    }

                    pctImage.Image = System.Drawing.Image.FromStream(fs);
                    fs.Close();

                    // ステータス設定
                    lblImage = (Label)pctImage.Controls[0];
                    if (m_dicState[string.Join(m_CON_DELIMIT_KEY,
                                                m_dtData.Rows[i]["line"].ToString(),
                                                m_dtData.Rows[i]["cloumns"].ToString(),
                                                m_dtData.Rows[i]["ng_face"].ToString(),
                                                m_dtData.Rows[i]["marking_imagepath"].ToString())] != g_clsSystemSettingInfo.intOverDetectionExceptResultNg.ToString())
                    {
                        lblImage.Text = string.Empty;
                    }
                    else
                    {
                        lblImage.Text = "NG";
                    }

                    break;
                }

                intImgIdxNum++;
            }

            // 左右ボタンを無効にする
            btnRight.Enabled = false;
            btnLeft.Enabled = false;

            // N秒後に左へボタンを有効にする
            if (intPageIdx > 0)
            {
                Task.Run(() =>
                {
                    // N秒後に右へボタンを有効にする
                    System.Threading.Thread.Sleep(g_clsSystemSettingInfo.intWaitingTimeProcessed * 1000);
                    this.Invoke(new Action(() =>
                    {
                        btnLeft.Enabled = true;
                    }));
                });
            }

            Task.Run(() =>
            {
                // N秒後に右へボタンを有効にする
                System.Threading.Thread.Sleep(g_clsSystemSettingInfo.intWaitingTimeProcessed * 1000);
                this.Invoke(new Action(() =>
                {
                    btnRight.Enabled = true;
                }));
            });


            // pictureboxの背景色を初期化
            foreach (Control ctrChild in tlpImage.Controls)
            {
                if (ctrChild.GetType() == typeof(PictureBox))
                {
                    PictureBox picWk = (PictureBox)ctrChild;
                    picWk.ClientSize = new Size(picWk.Size.Width, picWk.Size.Height);
                    picWk.BackColor = System.Drawing.Color.FromArgb(34, 67, 106);
                }

            }

            // 画像の1件目にフォーカスをセットする
            tlpImage.Controls[0].Focus();
            tlpImage.Controls[0].Select();

        }

        /// <summary>
        /// 画像の拡大縮小設定
        /// </summary>
        /// <param name="img"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Bitmap bmpImageScale(Image img, int width, int height)
        {
            return new Bitmap(img, width, height);
        }

        /// <summary>
        /// 過検知除外ステータス更新
        /// </summary>
        /// <param name="strFabricName">反番</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="intStatus">ステータス</param>
        /// <param name="strStartDatetime">判定開始日時(YYYY/MM//DD HH:MM:SS)</param>
        /// <returns></returns>
        private Boolean blnUpdOverDetectionExceptStatus(string strFabricName,
                                                        string strInspectionDate,
                                                        int intInspectionNum,
                                                        int intStatus,
                                                        string strStartDatetime = "")
        {
            string strSQL = string.Empty;
            try
            {
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // SQL文を作成する
                strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                              SET over_detection_except_status = :over_detection_except_status ";

                if (!string.IsNullOrEmpty(strStartDatetime))
                {
                    strSQL += @", decision_start_datetime = TO_TIMESTAMP(:decision_start_datetime_yyyymmdd_hhmmss, 'YYYY/MM/DD HH24:MI:SS') ";
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "decision_start_datetime_yyyymmdd_hhmmss", DbType = DbType.String, Value = strStartDatetime });
                }

                strSQL += @"WHERE fabric_name = :fabric_name
                              AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                              AND inspection_num = :inspection_num";

                // SQLコマンドに各パラメータを設定する
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_status", DbType = DbType.Int16, Value = intStatus });
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
                MessageBox.Show(g_clsMessageInfo.strMsgE0035, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// そのページ内の先頭画像のデータIdxを取得
        /// </summary>
        /// <param name="intCtrIdx"></param>
        /// <returns></returns>
        private int intGetStartDataIdxForPage(int intPageIdx)
        {
            return (intPageIdx * g_clsLoginInfo.intDispNum);
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Overdetectionexclusion_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            int intImgIdxNum = 0;
            PictureBox pctImage = new PictureBox();
            string strNowYmdhms = string.Empty;
            int intbtnLeftRightLocationY = -1;
            Single sngFontSize = 0;

            // 作業者の表示
            lblWorkerName.Text = string.Format(m_CON_FORMAT_WORKER_NAME, g_clsLoginInfo.strWorkerName);

            // ヘッダの表示
            lblUnitNum.Text = string.Format(m_CON_FORMAT_UNIT_NUM, m_strUnitNum);
            lblProductName.Text = string.Format(m_CON_FORMAT_PRODUCT_NAME, m_strProductName);
            lblOrderImg.Text = string.Format(m_CON_FORMAT_ORDER_IMG, m_strOrderImg);
            lblFabricName.Text = string.Format(m_CON_FORMAT_FABRIC_NAME, m_strFabricName);
            lblInspectionNum.Text = string.Format(m_CON_FORMAT_INSPECTION_NUM, m_intInspectionNum);

            // 前＆次ボタンのイメージ縮小対応
            btnLeft.Image = bmpImageScale(btnLeft.Image, btnLeft.ClientSize.Width, btnLeft.ClientSize.Height);
            btnRight.Image = bmpImageScale(btnRight.Image, btnRight.ClientSize.Width, btnRight.ClientSize.Height);

            // 前＆次ボタンの表示位置調整対応
            intbtnLeftRightLocationY = (int)(((double)pnlLeft.Size.Height / 2.0) - ((double)btnLeft.Size.Height / 2.0));
            btnLeft.Location = new Point(btnLeft.Location.X, intbtnLeftRightLocationY);
            btnRight.Location = new Point(btnRight.Location.X, intbtnLeftRightLocationY);

            // 表示枠の設定
            this.tlpImage.ColumnCount = 0;
            this.tlpImage.RowCount = 0;
            this.tlpImage.ColumnStyles.Clear();
            this.tlpImage.RowStyles.Clear();
            switch (g_clsLoginInfo.intDispNum)
            {
                case 2:
                    this.tlpImage.ColumnCount = 2;
                    this.tlpImage.RowCount = 1;
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    this.tlpImage.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                    break;
                case 4:
                    this.tlpImage.ColumnCount = 2;
                    this.tlpImage.RowCount = 2;
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    this.tlpImage.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                    this.tlpImage.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                    break;
                case 6:
                    this.tlpImage.ColumnCount = 3;
                    this.tlpImage.RowCount = 2;
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
                    this.tlpImage.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                    this.tlpImage.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                    break;
                case 9:
                    this.tlpImage.ColumnCount = 3;
                    this.tlpImage.RowCount = 3;
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
                    this.tlpImage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
                    this.tlpImage.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
                    this.tlpImage.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
                    this.tlpImage.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
                    break;
            }

            // 画像の追加
            for (int r = 0; r < tlpImage.RowCount; r++)
            {
                for (int c = 0; c < tlpImage.ColumnCount; c++)
                {
                    // 画像コントロール生成
                    tlpImage.Controls.Add(new PictureBox() { Name = string.Format(m_CON_FORMAT_PICTUREBOX_NAME, intImgIdxNum.ToString()) }, c, r);
                    pctImage = (PictureBox)tlpImage.Controls[tlpImage.Controls.Count - 1];
                    pctImage.Dock = DockStyle.Fill;
                    pctImage.SizeMode = PictureBoxSizeMode.Zoom;

                    Label lblState = new Label();

                    // NG表示用ラベル生成
                    lblState.Name = string.Format(m_CON_FORMAT_LABEL_NAME, intImgIdxNum);
                    lblState.AutoSize = false;
                    lblState.Dock = DockStyle.Fill;
                    lblState.ForeColor = Color.DodgerBlue;
                    lblState.TextAlign = ContentAlignment.MiddleCenter;

                    switch (g_clsLoginInfo.intDispNum)
                    {
                        case 2:
                            sngFontSize = 80;
                            break;
                        case 4:
                            sngFontSize = 60;
                            break;
                        case 6:
                            sngFontSize = 60;
                            break;
                        case 9:
                            sngFontSize = 50;
                            break;
                    }
                    lblState.Font = new System.Drawing.Font("メイリオ", sngFontSize);

                    // NG表示用ラベル追加
                    pctImage.Controls.Clear();
                    pctImage.Controls.Add(lblState);
                    lblState.BackColor = Color.Transparent;

                    // イベント追加
                    lblState.Click += lblState_Click;
                    lblState.DoubleClick += lblState_DoubleClick;

                    intImgIdxNum++;
                }
            }

            // 画像の1件目にフォーカスをセットする
            tlpImage.Controls[0].Focus();
            tlpImage.Controls[0].Select();

            // 画像パス一覧を取得
            if (bolGetImagePath() == false)
            {
                return;
            }

            // 総ページ数設定
            m_intPageCnt = m_dtData.Rows.Count / g_clsLoginInfo.intDispNum;
            if (m_dtData.Rows.Count % g_clsLoginInfo.intDispNum > 0)
            {
                m_intPageCnt++;
            }

            // ページIdx設定
            if (m_intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusBef)
            {
                // 過検知除外ステータス：作業前
                m_intPageIdx = 0;

                strNowYmdhms = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }

            if (m_intOverDetectionExceptStatus == g_clsSystemSettingInfo.intOverDetectionExceptStatusStp)
            {
                // 過検知除外ステータス：中断
                if (bolGetStartPageIdx(ref m_intPageIdx) == false)
                {
                    return;
                }
            }

            // 画像表示
            dispImage(m_intPageIdx);

            // 過検知除外ステータス更新(検査中)
            if (blnUpdOverDetectionExceptStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                g_clsSystemSettingInfo.intOverDetectionExceptStatusChk,
                                                strNowYmdhms) == false)
            {
                return;
            }

            g_clsConnectionNpgsql.DbCommit();
            g_clsConnectionNpgsql.DbClose();

            // パラメータの更新
            if (!string.IsNullOrEmpty(strNowYmdhms))
            {
                g_clsHeaderData.strDecisionStartDatetime = strNowYmdhms;
            }

            // 既に全レコードのチェックが済んでいる場合、過検知除外結果画面へ遷移する
            if (m_dtData.Select("over_detection_except_result = " + g_clsSystemSettingInfo.intOverDetectionExceptResultNon).Length == 0)
            {
                OpenSummaryForm();
            }

            this.ResumeLayout();
        }

        /// <summary>
        /// ステータスラベルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void lblState_Click(object sender, EventArgs e)
        {
            Label lblState = (Label)sender;
            PictureBox picState = (PictureBox)lblState.Parent;

            // 画像存在チェック
            if (picState.Image == null)
            {
                return;
            }

            changePanelState((Label)sender);
            // pictureboxの背景色を初期化
            foreach (Control ctrChild in tlpImage.Controls)
            {
                if (ctrChild.GetType() == typeof(PictureBox))
                {
                    PictureBox picWk = (PictureBox)ctrChild;
                    picWk.BackColor = System.Drawing.Color.FromArgb(34, 67, 106);
                }

            }

            // 背景色を設定
            picState.BackColor = Color.DodgerBlue;

            if (!_clickSemaphore.Wait(0))
            {
                return;
            }

            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                {
                    changePanelState((Label)sender);
                    return;
                }
            }
            finally
            {
                _clickSemaphore.Release();
            }
        }

        /// <summary>
        /// ステータスラベルダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblState_DoubleClick(object sender, EventArgs e)
        {
            string strOrgImagepath = string.Empty;
            string strMarkingImagepath = string.Empty;

            _doubleClickSemaphore.Release();

            // idxの取得　※lblImage1 + 選択したlblImage{0}
            int intIdx = intGetStartDataIdxForPage(m_intPageIdx) +
                         Convert.ToInt32(((Label)sender).Name.Replace(string.Format(m_CON_FORMAT_LABEL_NAME, string.Empty), string.Empty));

            // マーキング画像パスとオリジナル画像パスを取得
            strOrgImagepath = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory,
                                           m_strFaultImageSubDirectory,
                                           m_dtData.Rows[intIdx]["org_imagepath"].ToString());
            strMarkingImagepath = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory,
                                               m_strFaultImageSubDirectory,
                                               m_dtData.Rows[intIdx]["marking_imagepath"].ToString());

            // 画像拡大フォームに遷移
            ViewEnlargedimage frmViewImage = new ViewEnlargedimage(strOrgImagepath, strMarkingImagepath);
            frmViewImage.ShowDialog(this);
            this.Visible = true;
        }

        /// <summary>
        /// 左ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLeft_Click(object sender, EventArgs e)
        {
            // ページカウントアップ
            m_intPageIdx--;

            // 画像表示
            dispImage(m_intPageIdx);
        }

        /// <summary>
        /// 右ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRight_Click(object sender, EventArgs e)
        {
            string strSQL = string.Empty;
            string strKey = string.Empty;
            int intLine = -1;
            string strCloumns = string.Empty;
            string strNgFace = string.Empty;
            string strMarkingImagepath = string.Empty;
            int intStartDataIdxForPage = -1;
            int intDataIdxForPage = -1;

            try
            {
                for (int IdxCtr = 0; IdxCtr < g_clsLoginInfo.intDispNum; IdxCtr++)
                {
                    intStartDataIdxForPage = intGetStartDataIdxForPage(m_intPageIdx);
                    intDataIdxForPage = intStartDataIdxForPage + IdxCtr;

                    if (m_dicKey.ContainsKey(intDataIdxForPage) == false)
                    {
                        continue;
                    }

                    strKey = m_dicKey[intDataIdxForPage];
                    intLine = int.Parse(strKey.Split(m_CON_DELIMIT_KEY.ToCharArray(0, 1))[0]);
                    strCloumns = strKey.Split(m_CON_DELIMIT_KEY.ToCharArray(0, 1))[1];
                    strNgFace = strKey.Split(m_CON_DELIMIT_KEY.ToCharArray(0, 1))[2];
                    strMarkingImagepath = strKey.Split(m_CON_DELIMIT_KEY.ToCharArray(0, 1))[3];

                    // SQL文を作成する
                    strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                      SET over_detection_except_result = :over_detection_except_result
                                        , over_detection_except_datetime = current_timestamp
                                        , over_detection_except_worker = :over_detection_except_worker
                                        , before_over_detection_except_result = over_detection_except_result
                                        , before_over_detection_except_datetime = over_detection_except_datetime
                                        , before_over_detection_except_worker = over_detection_except_result
                                    WHERE fabric_name = :fabric_name
                                      AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date_yyyymmdd
                                      AND inspection_num = :inspection_num
                                      AND line = :line
                                      AND cloumns = :cloumns
                                      AND ng_face = :ng_face
                                      AND marking_imagepath = :marking_imagepath";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_result", DbType = DbType.Int16, Value = int.Parse(m_dicState[strKey]) });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_worker", DbType = DbType.String, Value = g_clsLoginInfo.strWorkerName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date_yyyymmdd", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "line", DbType = DbType.Int16, Value = intLine });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "cloumns", DbType = DbType.String, Value = strCloumns });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ng_face", DbType = DbType.String, Value = strNgFace });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "marking_imagepath", DbType = DbType.String, Value = strMarkingImagepath });
                    // sqlを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);

                    g_clsConnectionNpgsql.DbCommit();
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0043, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }

            // ページカウントアップ
            if (m_intPageIdx + 1 == m_intPageCnt || m_bolUpdFlg == true)
            {
                OpenSummaryForm();
            }
            else
            {
                // ページカウントアップ
                m_intPageIdx++;

                // 画像表示
                dispImage(m_intPageIdx);
            }

        }

        /// <summary>
        /// 検査対象選択へ戻る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// フォームクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Overdetectionexclusion_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_bolRegFlg == false)
            {
                // 過検知除外ステータス更新(中断)
                blnUpdOverDetectionExceptStatus(m_strFabricName, m_strInspectionDate, m_intInspectionNum,
                                                g_clsSystemSettingInfo.intOverDetectionExceptStatusStp);
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// ログアウトボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            m_bolRegFlg = false;

            g_clsLoginInfo.Logout();
        }

        /// <summary>
        /// 過検知除外結果フォームオープン
        /// </summary>
        private void OpenSummaryForm()
        {
            this.Visible = false;

            Summary frmSummary = new Summary(g_clsHeaderData, m_intSelIdx, m_intFirstDisplayedScrollingRowIdx);
            frmSummary.ShowDialog(this);

            if (frmSummary.intSelIdx != -1)
            {
                m_intSelIdx = frmSummary.intSelIdx;
                m_intFirstDisplayedScrollingRowIdx = frmSummary.intFirstDisplayedScrollingRowIdx;

                m_bolRegFlg = false;
                m_bolUpdFlg = true;

                m_intPageIdx = frmSummary.intSelIdx / g_clsLoginInfo.intDispNum;
                dispImage(m_intPageIdx);

                this.Visible = true;
            }
            else
            {
                m_bolRegFlg = true;
                intDestination = frmSummary.intDestination;

                this.Close();
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