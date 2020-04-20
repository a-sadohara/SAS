using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
                List<Button> lstStringButton = new List<Button>();
                lstStringButton.Add(btnA);
                lstStringButton.Add(btnB);
                lstStringButton.Add(btnC);
                lstStringButton.Add(btnD);
                lstStringButton.Add(btnE);
                lstStringButton.Add(btnHyphen);

                foreach (Button ctrButton in lstStringButton)
                {
                    // 文字ボタンを非表示にする
                    ctrButton.Visible = false;
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
            if (txtInput.TextLength == 0)
            {
                return;
            }

            // 1文字削除
            txtInput.Text = txtInput.Text.Remove(txtInput.TextLength - 1, 1);

            // カーソル位置を調整
            txtInput.SelectionStart = txtInput.TextLength;
            txtInput.Select();
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

        /// <summary>
        /// キーボード入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 現在有効な数値ボタン・文字ボタンを判定
            foreach (Button ctrButton in tableLayoutPanel1.Controls.OfType<Button>().Where(x => x.Visible && x.Text.Length == 1))
            {
                if (string.Compare(ctrButton.Text, e.KeyChar.ToString(), true) == 0)
                {
                    btnVal_Click(ctrButton, null);
                    return;
                }
            }

            // バックスペースキーの判定
            if (e.KeyChar == (char)Keys.Back)
            {
                btnBak_Click(null, null);
                return;
            }

            // エスケープキーの判定
            if (e.KeyChar == (char)Keys.Escape)
            {
                btnAllClear_Click(null, null);
                return;
            }

            // エンターキーの判定
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnEnter_Click(null, null);
                return;
            }
        }
    }
}