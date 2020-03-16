using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BeforeInspection
{
    public partial class TenKeyInput : Form
    {
        // 入力値
        public string strInput { get; set; }

        // テキストモード
        public string m_strTextMode { get; set; }

        // 定数
        private const string m_CON_TEXTMODE_ORDERIMG = "1";
        private const string m_CON_TEXTMODE_FABRICNAME = "2";
        private const string m_CON_TEXTMODE_NUMBER = "3";
        private const int m_CON_MAXLENGTH_ORDERIMG = 7;
        private const int m_CON_MAXLENGTH_FABRICNAME = 10;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="intMaxLength">最大桁数</param>
        /// <param name="bolOnly_number">数値フラグ</param>
        public TenKeyInput(
            int intMaxLength = 0,
            string strTextMode = "3")
        {
            InitializeComponent();

            m_strTextMode = strTextMode;

            // 最大桁を設定
            if (intMaxLength > 0)
            {
                txtInput.MaxLength = intMaxLength;
            }

            // 文字入力の可否をチェックする
            if (m_strTextMode.Equals(m_CON_TEXTMODE_ORDERIMG) ||
                m_strTextMode.Equals(m_CON_TEXTMODE_NUMBER))
            {
                List<Control> lstButtonInfo = new List<Control>();
                lstButtonInfo.Add(btnA);
                lstButtonInfo.Add(btnB);
                lstButtonInfo.Add(btnC);
                lstButtonInfo.Add(btnD);
                lstButtonInfo.Add(btnE);
                lstButtonInfo.Add(btnHyphen);

                foreach (Control ctr in lstButtonInfo)
                {
                    // 文字ボタンを非表示にする
                    ctr.Visible = false;
                }

                // 列の幅を0に設定する
                this.tableLayoutPanel1.ColumnStyles[3].Width = 0;
                this.tableLayoutPanel1.ColumnStyles[4].Width = 0;
                this.tableLayoutPanel1.ColumnStyles[5].Width = 0;

                // ウィンドウサイズを変更する
                this.Width = this.MinimumSize.Width;
                this.txtInput.Width = this.txtInput.MinimumSize.Width;
                this.tableLayoutPanel1.Width = this.tableLayoutPanel1.MinimumSize.Width;
            }
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputForm_Load(object sender, EventArgs e)
        {
            // 表示位置の調整
            int intHeight = (int)((double)(Screen.PrimaryScreen.WorkingArea.Height));
            int intY = (int)((double)(intHeight - this.Size.Height));

            this.Location = new Point(0, intY);

            // 入力テキストボックスを選択
            txtInput.Select();
        }

        /// <summary>
        /// 値ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVal_Click(object sender, EventArgs e)
        {
            if (txtInput.MaxLength == txtInput.TextLength)
            {
                return;
            }

            Button btnObj = (Button)sender;

            if (string.IsNullOrEmpty(txtInput.Text) &&
                !m_strTextMode.Equals(m_CON_TEXTMODE_FABRICNAME) &&
                btnObj == btn0)
            {
                return;
            }

            int intSelIdx = txtInput.SelectionStart;

            // 値を挿入
            txtInput.Text = txtInput.Text.Insert(txtInput.SelectionStart, btnObj.Text);
            txtInput.SelectionStart = intSelIdx + 1;

            // カーソル位置を調整
            txtInput.Select();
        }

        /// <summary>
        /// ALLCLEARボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAllClear_Click(object sender, EventArgs e)
        {
            txtInput.Text = string.Empty;
            txtInput.Select();
        }

        /// <summary>
        /// ENTERボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnter_Click(object sender, EventArgs e)
        {
            strInput = txtInput.Text;
            this.Close();
        }

        /// <summary>
        /// BAKボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBak_Click(object sender, EventArgs e)
        {
            //TODO
            int intSelIdx = txtInput.SelectionStart;

            // 前を削除
            if (txtInput.SelectionStart == 0 || txtInput.Text.Substring(txtInput.SelectionStart - 1, 1) == "-")
            {
                return;
            }

            if (txtInput.SelectionStart > 0)
            {
                txtInput.Text = txtInput.Text.Remove(txtInput.SelectionStart - 1, 1);
            }

            // カーソル位置を調整
            txtInput.SelectionStart = intSelIdx - 1;
            txtInput.Select();

            if (txtInput.Text == "-")
            {
                txtInput.Text = string.Empty;
            }
        }

        /// <summary>
        /// 入力テキストボックス選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInput_Enter(object sender, EventArgs e)
        {
            txtInput.SelectionStart = txtInput.TextLength;
            txtInput.SelectionLength = 0;
        }

        /// <summary>
        /// 入力テキストボックスクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInput_Click(object sender, EventArgs e)
        {
            txtInput.SelectionStart = txtInput.TextLength;
            txtInput.SelectionLength = 0;
        }
        #endregion
    }
}