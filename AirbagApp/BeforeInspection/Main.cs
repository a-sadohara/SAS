using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BeforeInspection.Common;
using Npgsql;

namespace BeforeInspection
{
    public partial class Main : Form
    {
        public DataTable dtData;

        private string m_strKensaWay;
        private string m_CON_KENSA_WAY_S = "S";
        private string m_CON_KENSA_WAY_X = "X";
        private string m_CON_KENSA_WAY_Y = "Y";
        private string m_CON_KENSA_WAY_R = "R";

        private const string m_CON_SEW_S = "ＳＷ";
        private const string m_CON_SEW_E = "ＥＷ";

        private int m_intKensaNo; 
        private int m_intRowNumNow; 
        private int m_intRowCntByTan; //仮

        private int m_intStatus;
        private const int m_CON_BEF = 1;    //検査開始前
        private const int m_CON_CHK = 2;    //検査準備完了
        private const int m_CON_STP = 3;    //検査中断
        private const int m_CON_END = 4;    //検査終了
        private const string m_CON_BEF_NM = "検査開始前";
        private const string m_CON_CHK_NM = "検査準備完了";
        private const string m_CON_STP_NM = "検査中断";
        private const string m_CON_END_NM = "検査終了";

        private Color m_clrActFore = System.Drawing.SystemColors.ControlText;
        private Color m_clrActBack = System.Drawing.Color.White;
        private Color m_clrNonActFore = System.Drawing.SystemColors.ControlLightLight;
        private Color m_clrNonActBack = System.Drawing.Color.Transparent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Main()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            tableLayoutPanel1.RowStyles[7] = new RowStyle(SizeType.Percent, 0F);
            tableLayoutPanel1.RowStyles[8] = new RowStyle(SizeType.Percent, 0F);

            m_intKensaNo = 1;
            m_intRowNumNow = 1;

            // 値や背景色の初期化
            lblKensaNo.Text = "検査番号：" + m_intKensaNo;
            txtHinNo.Text = "";
            txtSashizu.Text = "";
            txtHanNo.Text = "";
            txtKensaTaishoNum.Text = "";
            lblKensaTaishoNumMax.Text = "";
            txtKensaStartRow.Text = "";
            txtSagyosyaInfo_1.Text = "";
            txtSagyosyaInfo_2.Text = "";
            lblStartDate.Text = "";
            lblEndDate.Text = "";
            btnKensaWay_S_2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            btnKensaWay_X_2.BackColor = SystemColors.Control;
            btnKensaWay_Y_2.BackColor = SystemColors.Control;
            btnKensaWay_R_2.BackColor = SystemColors.Control;
            m_strKensaWay = m_CON_KENSA_WAY_S;
            txtHinNo.BackColor = SystemColors.Window;

            // 検査方向ステータスの初期化
            picKensaWay_Front.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_S.png");
            lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
            lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
            picKensaWay_Back.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_X.png");
            lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
            lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";

            setStatus(m_CON_BEF);
            

            btnNext.Enabled = false;
        }

        /// <summary>
        /// ロードイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Load(object sender, EventArgs e)
        {
            // 座標系プロパティの設定　※Load後じゃないと反映されない
            txtHinNo.Location = new Point(txtHinNo.Location.X, (int)((double)(pnlHinNo.Size.Height / 2) - ((double)txtHinNo.Size.Height / 2)));
            txtSashizu.Location = new Point(txtSashizu.Location.X, (int)(((double)pnlSashizu.Size.Height / 2) - ((double)txtSashizu.Size.Height / 2)));
            txtHanNo.Location = new Point(txtHanNo.Location.X, (int)(((double)pnlHanNo.Size.Height / 2) - ((double)txtHanNo.Size.Height / 2)));
            txtKensaTaishoNum.Location = new Point(txtKensaTaishoNum.Location.X, (int)(((double)pnlKensaTaishoNum_LastNum.Size.Height / 2) - ((double)txtKensaTaishoNum.Size.Height / 2)));
            lblKensaTaishoNumMax.Location = new Point(lblKensaTaishoNumMax.Location.X, (int)(((double)pnlKensaTaishoNum_LastNum.Size.Height / 2) - ((double)lblKensaTaishoNumMax.Size.Height / 2)));
            txtKensaStartRow.Location = new Point(txtKensaStartRow.Location.X, (int)(((double)pnlKensaStartRow.Size.Height / 2) - ((double)txtKensaStartRow.Size.Height / 2)));
            txtSagyosyaInfo_1.Location = new Point(txtSagyosyaInfo_1.Location.X, (int)(((double)pnlSagyosyaInfo_1.Size.Height / 2) - ((double)txtSagyosyaInfo_1.Size.Height / 2)));
            txtSagyosyaInfo_2.Location = new Point(txtSagyosyaInfo_2.Location.X, (int)(((double)pnlSagyosyaInfo_2.Size.Height / 2) - ((double)txtSagyosyaInfo_2.Size.Height / 2)));
            btnKensaWay_S_2.Height = pnlKensaWay_Front.Height;
            btnKensaWay_X_2.Height = pnlKensaWay_Front.Height;
            btnKensaWay_Y_2.Height = pnlKensaWay_Back.Height;
            btnKensaWay_R_2.Height = pnlKensaWay_Back.Height;
        }

        /// <summary>
        /// 開始時刻（現在時刻日時）ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartDate_Click(object sender, EventArgs e)
        {
            // キーボード非表示
            if (KeyboardApp.IsOpen())
                KeyboardApp.KillApp();

            DialogResult result = MessageBox.Show("以下の情報で、撮像装置へデータを転送します。よろしいですか？\r\n" +
                                                  lblTitleHinNo.Text + "：" + txtHinNo.Text + "\r\n" +
                                                  lblTitleSashizu.Text + "：" + txtSashizu.Text + "\r\n" +
                                                  lblTitleHanNo.Text + "：" + txtHanNo.Text + "\r\n" +
                                                  lblTitleKensaTaishoNum_LastNum.Text + "：" + txtKensaTaishoNum.Text + "／" + lblKensaTaishoNumMax.Text + "\r\n" +
                                                  lblTitleKensaStartRow.Text + "：" + txtKensaStartRow.Text + "\r\n" +
                                                  lblTitleSagyosyaInfo.Text + " " + lblTitleSagyosyaInfo_KenHanbuNo1.Text + "：" + txtSagyosyaInfo_1.Text + "\r\n" +
                                                  lblTitleSagyosyaInfo.Text + " " + lblTitleSagyosyaInfo_KenHanbuNo2.Text + "：" + txtSagyosyaInfo_2.Text + "\r\n" +
                                                  lblTitleKensaWay.Text + "：" + m_strKensaWay,
                                                  "確認", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                lblStartDate.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 終了時刻（現在時刻日時）ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEndDate_Click(object sender, EventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            lblEndDate.Text = dtNow.ToString("yyyy/MM/dd HH:mm:ss");
            btnNext.Enabled = true;

            setStatus(m_CON_END);
        }

        /// <summary>
        /// 品名クリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtHinNo_Click(object sender, EventArgs e)
        {
            // キーボード非表示
            if (KeyboardApp.IsOpen())
                KeyboardApp.KillApp();

            // 品番選択画面
            HinNoSelection frmHinNoSelection = new HinNoSelection();
            frmHinNoSelection.ShowDialog(this);
            txtHinNo.Text = frmHinNoSelection.strHinNm;
            if (!string.IsNullOrEmpty(txtHinNo.Text))
            {
                // TODO 指図反映
                txtSashizu.Text = "1070205";
            }

            // 反番毎の
            m_intRowCntByTan = 500;
        }

        /// <summary>
        /// 検査方向ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnKensaWay_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;


            ////検反部No1,2の表示
            //btn.BackColor = System.Drawing.SystemColors.ActiveCaption;
            //if (btn == btnKensaWay_S_2 || btn == btnKensaWay_X_2)
            //{
            //    picKensaWay_Front.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_S.png");
            //    lblSEW_1_Front.Text = m_CON_SEW_E;
            //    lblSEW_2_Front.Text = m_CON_SEW_S;
            //    picKensaWay_Back.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_X.png");
            //    lblSEW_1_Back.Text = m_CON_SEW_E;
            //    lblSEW_2_Back.Text = m_CON_SEW_S;

            //    //btnKensaWay_S_2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            //    //btnKensaWay_X_2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            //    //btnKensaWay_Y_2.BackColor = SystemColors.Control;
            //    //btnKensaWay_R_2.BackColor = SystemColors.Control;

            //    //m_strKensaWay = m_CON_KENSA_WAY_S;   
            //}
            //else if (btn == btnKensaWay_R_2 || btn == btnKensaWay_Y_2)
            //{
            //    picKensaWay_Front.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_R.png");
            //    lblSEW_1_Front.Text = m_CON_SEW_S;
            //    lblSEW_2_Front.Text = m_CON_SEW_E;
            //    picKensaWay_Back.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_Y.png");
            //    lblSEW_1_Back.Text = m_CON_SEW_S;
            //    lblSEW_2_Back.Text = m_CON_SEW_E;

            //    //btnKensaWay_S_2.BackColor = SystemColors.Control;
            //    //btnKensaWay_X_2.BackColor = SystemColors.Control;
            //    //btnKensaWay_Y_2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            //    //btnKensaWay_R_2.BackColor = System.Drawing.SystemColors.ActiveCaption;

            //    //m_strKensaWay = m_CON_KENSA_WAY_R;
            //}

            // 個別
            if (btn == btnKensaWay_S_2)
            {
                picKensaWay_Front.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_S.png");
                lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                picKensaWay_Back.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_X.png");
                lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                m_strKensaWay = m_CON_KENSA_WAY_S;   
            }
            else if (btn == btnKensaWay_X_2)
            {
                picKensaWay_Front.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_X.png");
                lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                picKensaWay_Back.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_S.png");
                lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                m_strKensaWay = m_CON_KENSA_WAY_X;
            }
            else if (btn == btnKensaWay_Y_2)
            {
                picKensaWay_Front.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_Y.png");
                lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                picKensaWay_Back.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_R.png");
                lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                m_strKensaWay = m_CON_KENSA_WAY_Y;
            }
            else if (btn == btnKensaWay_R_2)
            {
                picKensaWay_Front.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_R.png");
                lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                picKensaWay_Back.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_Y.png");
                lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
                lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
                m_strKensaWay = m_CON_KENSA_WAY_R;
            }

            //ボタンの背景色
            btn.BackColor = System.Drawing.SystemColors.ActiveCaption;
            foreach (Button btnKensaWay in new Button[] { btnKensaWay_S_2,btnKensaWay_X_2,btnKensaWay_Y_2,btnKensaWay_R_2 })
            {
                if (btnKensaWay != btn)
                    btnKensaWay.BackColor = SystemColors.Control;
            }

            calcMaxRow();

        }

        /// <summary>
        /// 次ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, EventArgs e)
        {
            // 値や背景色の初期化
            txtHinNo.Text = "";
            txtSashizu.Text = "";
            txtHanNo.Text = "";
            txtKensaTaishoNum.Text = "";
            lblKensaTaishoNumMax.Text = "";
            txtKensaStartRow.Text = "";
            txtSagyosyaInfo_1.Text = "";
            txtSagyosyaInfo_2.Text = "";
            lblStartDate.Text = "";
            lblEndDate.Text = "";
            btnKensaWay_S_2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            btnKensaWay_X_2.BackColor = SystemColors.Control;
            btnKensaWay_Y_2.BackColor = SystemColors.Control;
            btnKensaWay_R_2.BackColor = SystemColors.Control;
            m_strKensaWay = m_CON_KENSA_WAY_S;
            txtHinNo.BackColor = SystemColors.Window;

            // 検査方向ステータスの初期化
            picKensaWay_Front.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_S.png");
            lblSEW_1_Front.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
            lblSEW_2_Front.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";
            picKensaWay_Back.Image = System.Drawing.Image.FromFile(@"Image\ABCDE_X.png");
            lblSEW_1_Back.Text = " ↓↓↓ " + m_CON_SEW_E + " ↓↓↓ ";
            lblSEW_2_Back.Text = " ↓↓↓ " + m_CON_SEW_S + " ↓↓↓ ";

            btnNext.Enabled = false;

            setStatus(m_CON_BEF);
        }

        /// <summary>
        /// 作業者情報テキストボックスのフォーカスインイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>対象イベント：OnClick,Enter</remarks>
        private void txtSagyosyaInfo_FocusIn(dynamic sender, EventArgs e)
        {
            DispKeyboard(sender, e);

            // 選択情報を初期化
            sender.Text = "";

            // 入力制限
            sender.ReadOnly = false;
            sender.MaxLength = 4;
        }

        /// <summary>
        /// 作業者情報テキストボックスのフォーカスインイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSagyosyaInfo_DoubleClick(object sender, EventArgs e)
        {
            SelectUser(sender, false);
        }


        /// <summary>
        /// 作業者情報テキストボックスキーダウン処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSagyosyaInfo_KeyDown(dynamic sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (String.IsNullOrEmpty(sender.Text))
                {
                    // 入力されていない　⇒　チェックせずに選択画面に遷移
                    SelectUser(sender, false);
                }
                else
                {
                    // 入力されている　⇒　チェック
                    SelectUser(sender);
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (!String.IsNullOrEmpty(sender.Text))
                {
                    // フォーカスを当てた状態（初期化＆入力制限）にする
                    txtSagyosyaInfo_FocusIn(sender, e);
                }
            }
        }

        /// <summary>
        /// 作業者キープレスイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSagyosyaInfo_KeyPress(object sender, KeyPressEventArgs e)
        {
            // エンター,バックスペース,0～9のみ
            if (e.KeyChar != '\r' && e.KeyChar != '\b' && e.KeyChar < '0' || '9' < e.KeyChar)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 作業者選択
        /// </summary>
        /// <param name="parChk">チェック有無</param>
        private void SelectUser(dynamic txtSagyosyaInfo, bool parChk = true)
        {
            string strUserNo = "";
            string strUserNm = "";

            string strSQL = "";

            if (parChk)
            {
                try
                {
                    if (bolModeNonDBCon == true)
                        throw new Exception("DB非接続モードです");

                    // PostgreSQLへ接続
                    using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
                    {
                        NpgsqlCon.Open();

                        // SQL抽出
                        NpgsqlCommand NpgsqlCom = null;
                        NpgsqlDataAdapter NpgsqlDtAd = null;
                        dtData = new DataTable();

                        dtData = new DataTable();
                        strSQL += "SELECT USERNO,USERNAME FROM SAGYOSYA ";
                        strSQL += "WHERE USERNO = '" + txtSagyosyaInfo.Text + "'";
                        NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                        NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                        NpgsqlDtAd.Fill(dtData);
                    }
                }
                catch
                {
                    dtData = new DataTable();
                    dtData.Columns.Add("USERNO");
                    dtData.Columns.Add("USERNAME");
                    dtData.Columns.Add("USERYOMIGANA");

                    // 後々この処理は消す
                    foreach (string line in System.IO.File.ReadLines("作業者.tsv", Encoding.Default))
                    {
                        // 改行コードを変換
                        string strLine = line.Replace("\\rn", Environment.NewLine);

                        string[] csv = strLine.Split('\t');
                        string[] data = new string[csv.Length];
                        Array.Copy(csv, 0, data, 0, data.Length);

                        if (data[0].ToString() != txtSagyosyaInfo.Text)
                            continue;

                        dtData.Rows.Add(data);
                    }
                }

                if (dtData.Rows.Count > 0)
                {
                    strUserNo = dtData.Rows[0][0].ToString();
                    strUserNm = dtData.Rows[0][1].ToString();
                }
                else
                {
                    // キーボード非表示
                    if (KeyboardApp.IsOpen())
                        KeyboardApp.KillApp();

                    MessageBox.Show("入力された職員番号は存在しません");

                    // 作業者選択画面表示
                    UserSelection frmTargetSelection = new UserSelection();
                    frmTargetSelection.ShowDialog(this);
                    this.Visible = true;

                    strUserNo = frmTargetSelection.strUserNo;
                    strUserNm = frmTargetSelection.strUserNm;
                }
            }
            else
            {
                // キーボード非表示
                if (KeyboardApp.IsOpen())
                    KeyboardApp.KillApp();

                // 作業者選択画面表示
                UserSelection frmTargetSelection = new UserSelection();
                frmTargetSelection.ShowDialog(this);
                this.Visible = true;

                strUserNo = frmTargetSelection.strUserNo;
                strUserNm = frmTargetSelection.strUserNm;
            }

            if (!String.IsNullOrEmpty(strUserNo))
            {
                // ユーザ情報設定
                txtSagyosyaInfo.Text = strUserNm;

                // 入力不可にする
                txtSagyosyaInfo.ReadOnly = true;
                txtSagyosyaInfo.BackColor = SystemColors.Window;

                // 次のコントロールを選択する
                this.SelectNextControl((Control)txtSagyosyaInfo, true, true, true, true);
            }
            else
            {
                // 選択情報を初期化
                txtSagyosyaInfo.Text = "";

                // 入力制限
                txtSagyosyaInfo.ReadOnly = false;
                txtSagyosyaInfo.MaxLength = 4;
            }
        }

        /// <summary>
        /// キーボード表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispKeyboard(object sender, EventArgs e)
        {
            if (bolUseKeyboadApp == false)
                return;

            TextBox txtBox = (TextBox)sender;

            // 座標
            int intX = 0;
            int intY = 0;
            int intWidth = 0;
            int intHeight = 0;

            // DPI設定値　※通常(拡大率100%)が96
            float dpiDef = 96;
            float dpiX = 0;
            float dpiY = 0;

            // タイトルバー幅
            int intTitleWidth = 0;
            
            // DPI設定値を取得
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = graphics.DpiX;
                dpiY = graphics.DpiY;
            }

            // Windows枠幅を取得
            intTitleWidth = SystemInformation.CaptionHeight;

            //// NumLockを設定
            //SetNumLock(true);

            //// 座標・サイズ指定
            //intX = (int)((double)(PointToScreen(this.Location).X));
            //intY = (int)((double)(PointToScreen(pnlInfo.Location).Y + intTitleWidth));
            //intWidth = (int)((double)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - intX) / (dpiX / dpiDef));
            //intHeight = (int)((double)(Screen.PrimaryScreen.WorkingArea.Height - intY) / (dpiY / dpiDef));

            //KeyboardApp.OnApp(intX, intY, intWidth, intHeight);

            InpttForm frmInpttForm = new InpttForm(txtBox.MaxLength);
            frmInpttForm.ShowDialog(this);
            this.Visible = true;

            txtBox.Text = frmInpttForm.strInput;
        }

        /// <summary>
        /// テキストボックス（一般）キープレスイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtNormalType_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                // エンター：次のコントロールを選択する
                this.SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        /// <summary>
        /// テキストボックス（数値のみ）キープレスイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtNumType_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                // エンター：次のコントロールを選択する
                this.SelectNextControl((Control)sender, true, true, true, true);
            }
            else if (e.KeyChar != '\b' && e.KeyChar < '0' || '9' < e.KeyChar)
            {
                // バックスペース,0～9のみ
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 値や背景色の初期化
            txtKensaTaishoNum.Text = "";
            lblKensaTaishoNumMax.Text = "";
            txtKensaStartRow.Text = "";
            txtSagyosyaInfo_1.Text = "";
            txtSagyosyaInfo_2.Text = "";
            lblStartDate.Text = "";
            lblEndDate.Text = "";
            txtHinNo.BackColor = SystemColors.Window;

            btnNext.Enabled = false;
        }

        private void txtHanNo_Leave(object sender, EventArgs e)
        {
            ////TODO 反物毎の枚数を格納
            //m_intRowCntByTan = 500;
        }

        private void txtKensaTaishoNum_Leave(object sender, EventArgs e)
        {
            calcMaxRow();
        }

        private void txtKensaStartRow_Leave(object sender, EventArgs e)
        {
            calcMaxRow();
        }

        private void calcMaxRow()
        {
            // 最終行計算
            if (txtKensaTaishoNum.Text != "" && txtKensaStartRow.Text != "")
            {
                if (m_strKensaWay == m_CON_KENSA_WAY_S || m_strKensaWay == m_CON_KENSA_WAY_X)
                {
                    lblKensaTaishoNumMax.Text = (int.Parse(txtKensaStartRow.Text) + int.Parse(txtKensaTaishoNum.Text) - 1).ToString();
                }
                else if (m_strKensaWay == m_CON_KENSA_WAY_Y || m_strKensaWay == m_CON_KENSA_WAY_R)
                {
                    lblKensaTaishoNumMax.Text = (int.Parse(txtKensaStartRow.Text) - int.Parse(txtKensaTaishoNum.Text) + 1).ToString();
                }
            }
            else
            {
                lblKensaTaishoNumMax.Text = "";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            setStatus(m_CON_CHK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            setStatus(m_CON_STP);
        }

        private void setStatus(int intStatus)
        {
            m_intStatus = intStatus;

            // 非アクティブ化
            foreach (Label lblStatus in new Label[] { lblStatusBef, lblStatusChk, lblStatusStp, lblStatusEnd })
            {
                if ((m_intStatus == m_CON_BEF && lblStatus == lblStatusBef) ||
                    (m_intStatus == m_CON_CHK && lblStatus == lblStatusChk) ||
                    (m_intStatus == m_CON_STP && lblStatus == lblStatusStp) ||
                    (m_intStatus == m_CON_END && lblStatus == lblStatusEnd))
                    continue;

                lblStatus.ForeColor = m_clrNonActFore;
                lblStatus.BackColor = m_clrNonActBack;
            }

            // アクティブ化
            switch (m_intStatus)
            {
                case m_CON_BEF:
                    lblStatusBef.ForeColor = m_clrActFore;
                    lblStatusBef.BackColor = m_clrActBack;
                    break;
                case m_CON_CHK:
                    lblStatusChk.ForeColor = m_clrActFore;
                    lblStatusChk.BackColor = m_clrActBack;
                    break;
                case m_CON_STP:
                    lblStatusStp.ForeColor = m_clrActFore;
                    lblStatusStp.BackColor = m_clrActBack;
                    break;
                case m_CON_END:
                    lblStatusEnd.ForeColor = m_clrActFore;
                    lblStatusEnd.BackColor = m_clrActBack;
                    break;
            }
            
        }
    }
}
