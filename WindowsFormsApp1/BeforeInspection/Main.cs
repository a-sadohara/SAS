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
        public NpgsqlConnection NpgsqlCon;
        public DataTable dtData;
        public String m_strUserNo;
        public String m_strUserNm;

        public Main()
        {
            InitializeComponent();

            txtHinNo.Height = pnlHinNo.Height;
            txtHinNo.Font = new System.Drawing.Font("メイリオ", 10F);
            txtSashizu.Height = pnlSashizu.Height;
            txtSashizu.Font = new System.Drawing.Font("メイリオ", 10F);
            txtHanNo.Height = pnlHanNo.Height;
            txtHanNo.Font = new System.Drawing.Font("メイリオ", 10F);
            txtKensaTaishoNum.Height = pnlKensaTaishoNum_LastNum.Height;
            txtKensaTaishoNum.Font = new System.Drawing.Font("メイリオ", 10F);
            txtKensaTaishoNumMax.Height = pnlKensaTaishoNum_LastNum.Height;
            txtKensaTaishoNumMax.Font = new System.Drawing.Font("メイリオ", 10F);
            txtKensaStartRow.Height = pnlKensaStartRow_1.Height;
            txtKensaStartRow.Font = new System.Drawing.Font("メイリオ", 10F);
            txtSagyosyaInfo_1.Height = pnlSagyosyaInfo_1.Height;
            txtSagyosyaInfo_1.Font = new System.Drawing.Font("メイリオ", 10F);
            txtSagyosyaInfo_2.Height = pnlSagyosyaInfo_2.Height;
            txtSagyosyaInfo_2.Font = new System.Drawing.Font("メイリオ", 10F);
            txtStartDate_Year.Height = pnlStartDate.Height;
            txtStartDate_Year.Font = new System.Drawing.Font("メイリオ", 10F);
            txtStartDate_Month.Height = pnlStartDate.Height;
            txtStartDate_Month.Font = new System.Drawing.Font("メイリオ", 10F);
            txtStartDate_Day.Height = pnlStartDate.Height;
            txtStartDate_Day.Font = new System.Drawing.Font("メイリオ", 10F);
            txtStartDate_Hour.Height = pnlStartDate.Height;
            txtStartDate_Hour.Font = new System.Drawing.Font("メイリオ", 10F);
            txtStartDate_Minute.Height = pnlStartDate.Height;
            txtStartDate_Minute.Font = new System.Drawing.Font("メイリオ", 10F);
            txtStartDate_Second.Height = pnlStartDate.Height;
            txtStartDate_Second.Font = new System.Drawing.Font("メイリオ", 10F);
            txtEndDate_Year.Height = pnlEndDate.Height;
            txtEndDate_Year.Font = new System.Drawing.Font("メイリオ", 10F);
            txtEndDate_Month.Height = pnlEndDate.Height;
            txtEndDate_Month.Font = new System.Drawing.Font("メイリオ", 10F);
            txtEndDate_Day.Height = pnlEndDate.Height;
            txtEndDate_Day.Font = new System.Drawing.Font("メイリオ", 10F);
            txtEndDate_Hour.Height = pnlEndDate.Height;
            txtEndDate_Hour.Font = new System.Drawing.Font("メイリオ", 10F);
            txtEndDate_Minute.Height = pnlEndDate.Height;
            txtEndDate_Minute.Font = new System.Drawing.Font("メイリオ", 10F);
            txtEndDate_Second.Height = pnlEndDate.Height;
            txtEndDate_Second.Font = new System.Drawing.Font("メイリオ", 10F);

            btnKensaWay_S.Height = pnlKensaWay.Height;
            btnKensaWay_X.Height = pnlKensaWay.Height;
            btnKensaWay_Y.Height = pnlKensaWay.Height;
            btnKensaWay_R.Height = pnlKensaWay.Height;

            txtHinNo.Text = "";
            txtSashizu.Text = "";
            txtHanNo.Text = "";
            txtKensaTaishoNum.Text = "";
            txtKensaTaishoNumMax.Text = "";
            txtKensaStartRow.Text = "";
            txtSagyosyaInfo_1.Text = "";
            txtSagyosyaInfo_2.Text = "";
            txtStartDate_Year.Text = "";
            txtStartDate_Month.Text = "";
            txtStartDate_Day.Text = "";
            txtStartDate_Hour.Text = "";
            txtStartDate_Minute.Text = "";
            txtStartDate_Second.Text = "";
            txtEndDate_Year.Text = "";
            txtEndDate_Month.Text = "";
            txtEndDate_Day.Text = "";
            txtEndDate_Hour.Text = "";
            txtEndDate_Minute.Text = "";
            txtEndDate_Second.Text = "";

            btnKensaWay_S.BackColor = System.Drawing.SystemColors.Control;
            btnKensaWay_X.BackColor = System.Drawing.SystemColors.Control;
            btnKensaWay_Y.BackColor = System.Drawing.SystemColors.Control;
            btnKensaWay_R.BackColor = System.Drawing.SystemColors.Control;

        }

        private void btnStartDate_Click(object sender, EventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            txtStartDate_Year.Text = dtNow.Year.ToString();
            txtStartDate_Month.Text = dtNow.Month.ToString();
            txtStartDate_Day.Text = dtNow.Day.ToString();
            txtStartDate_Hour.Text = dtNow.Hour.ToString();
            txtStartDate_Minute.Text = dtNow.Minute.ToString();
            txtStartDate_Second.Text = dtNow.Second.ToString();
        }

        private void btnEndDate_Click(object sender, EventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            txtEndDate_Year.Text = dtNow.Year.ToString();
            txtEndDate_Month.Text = dtNow.Month.ToString();
            txtEndDate_Day.Text = dtNow.Day.ToString();
            txtEndDate_Hour.Text = dtNow.Hour.ToString();
            txtEndDate_Minute.Text = dtNow.Minute.ToString();
            txtEndDate_Second.Text = dtNow.Second.ToString();
        }

        private void txtHinNo_Click(object sender, EventArgs e)
        {
            HinNoSelection frmHinNoSelection = new HinNoSelection();
            frmHinNoSelection.ShowDialog(this);
            txtHinNo.Text = frmHinNoSelection.strHinNm;
            if (!string.IsNullOrEmpty(txtHinNo.Text))
            {
                DateTime dtNow = DateTime.Now;

                txtSashizu.Text = "1070205";
                //txtHanNo.Text = "180128-OAE";
                //txtKensaTaishoNum.Text = "356";
                //txtKensaStartRow_1.Text = "180";
                //txtKensaStartRow_2.Text = "180";
                //txtSagyosyaInfo_1.Text = "0001　エアバッグ　太郎";
                //txtSagyosyaInfo_2.Text = "0001　エアバッグ　太郎";
                //btnKensaWay_Click(btnKensaWay_S, e);

                //txtStartDate_Year.Text = dtNow.Year.ToString();
                //txtStartDate_Month.Text = dtNow.Month.ToString();
                //txtStartDate_Day.Text = dtNow.Day.ToString();
                //txtStartDate_Hour.Text = dtNow.Hour.ToString();
                //txtStartDate_Minute.Text = dtNow.Minute.ToString();
                //txtStartDate_Second.Text = dtNow.Second.ToString();

                //lblLastNum.Text = "535";
                //lblSlash.Text = "／";
            }

        }

        private void btnKensaWay_Click(dynamic sender, EventArgs e)
        {
            foreach (Button btn in new Button[] { btnKensaWay_S, btnKensaWay_X, btnKensaWay_Y, btnKensaWay_R })
            {
                if (sender == btn)
                {
                    btn.BackColor = System.Drawing.SystemColors.ActiveCaption;
                }
                else
                {
                    btn.BackColor = SystemColors.Control;
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            txtHinNo.Text = "";
            txtSashizu.Text = "";
            txtHanNo.Text = "";
            txtKensaTaishoNum.Text = "";
            txtKensaTaishoNumMax.Text = "";
            txtKensaStartRow.Text = "";
            txtSagyosyaInfo_1.Text = "";
            txtSagyosyaInfo_2.Text = "";
            txtStartDate_Year.Text = "";
            txtStartDate_Month.Text = "";
            txtStartDate_Day.Text = "";
            txtStartDate_Hour.Text = "";
            txtStartDate_Minute.Text = "";
            txtStartDate_Second.Text = "";
            txtEndDate_Year.Text = "";
            txtEndDate_Month.Text = "";
            txtEndDate_Day.Text = "";
            txtEndDate_Hour.Text = "";
            txtEndDate_Minute.Text = "";
            txtEndDate_Second.Text = "";
        }

        private void txtSagyosyaInfo_FocusIn(dynamic sender, EventArgs e)
        {
            // 選択情報を初期化
            sender.Text = "";

            // 入力制限
            sender.ReadOnly = false;
            sender.MaxLength = 4;
        }

        private void txtSagyosyaInfo_DoubleClick(object sender, EventArgs e)
        {
            SelectUser(sender, false);
        }

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

        private void txtSagyosyaInfo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r' && e.KeyChar != '\b' && e.KeyChar < '0' || '9' < e.KeyChar)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 職員選択
        /// </summary>
        /// <param name="parChk">チェック有無</param>
        private void SelectUser(dynamic txtSagyosyaInfo, bool parChk = true)
        {
            string strUserNo = "";
            string strUserNm = "";

            string strSQL = "";

            if (parChk)
            {
                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
                {
                    NpgsqlCon.Open();

                    // SQL抽出
                    NpgsqlCommand NpgsqlCom = null;
                    NpgsqlDataAdapter NpgsqlDtAd = null;
                    dtData = new DataTable();
                    strSQL += "SELECT USERNO,USERNAME FROM SAGYOSYA ";
                    strSQL += "WHERE USERNO = '" + txtSagyosyaInfo.Text + "'";
                    NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                    NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                    NpgsqlDtAd.Fill(dtData);

                    if (dtData.Rows.Count > 0)
                    {
                        strUserNo = dtData.Rows[0][0].ToString();
                        strUserNm = dtData.Rows[0][1].ToString();
                    }
                    else
                    {
                        MessageBox.Show("入力された職員番号は存在しません");

                        UserSelection frmTargetSelection = new UserSelection();
                        frmTargetSelection.ShowDialog(this);
                        this.Visible = true;

                        strUserNo = frmTargetSelection.strUserNo;
                        strUserNm = frmTargetSelection.strUserNm;
                    }
                }
            }
            else
            {
                UserSelection frmTargetSelection = new UserSelection();
                frmTargetSelection.ShowDialog(this);
                this.Visible = true;

                strUserNo = frmTargetSelection.strUserNo;
                strUserNm = frmTargetSelection.strUserNm;
            }

            if (!String.IsNullOrEmpty(strUserNo))
            {
                txtSagyosyaInfo.Text = strUserNo + "　" + strUserNm;

                // 入力不可にする
                txtSagyosyaInfo.ReadOnly = true;
                txtSagyosyaInfo.BackColor = SystemColors.Window;
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
    }
}
