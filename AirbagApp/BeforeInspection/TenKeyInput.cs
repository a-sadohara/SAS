using System;
using System.Drawing;
using System.Windows.Forms;

namespace BeforeInspection
{
    public partial class TenKeyInput : Form
    {
        // 入力値
        public string strInput { get; set; }

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="intMaxLength">最大桁数</param>
        public TenKeyInput(int intMaxLength = 0)
        {
            InitializeComponent();

            // 最大桁を設定
            if (intMaxLength > 0)
            {
                txtInput.MaxLength = intMaxLength;
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
