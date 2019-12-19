using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BeforeInspection
{
    public partial class InputForm : Form
    {
        // INSモードONOFF
        private bool bolIns = false; 

        // 入力値
        public string strInput { get; set; }

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="intMaxLength">最大桁数</param>
        public InputForm(int intMaxLength = 0)
        {
            InitializeComponent();

            txtInput.Select();

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

            // INSモード範囲選択
            if (bolIns == true)
            {
                // 値を挿入
                txtInput.Text = new StringBuilder(txtInput.Text).Replace(txtInput.Text.Substring(txtInput.SelectionStart, 1), btnObj.Text, txtInput.SelectionStart, 1).ToString();
                txtInput.SelectionStart = intSelIdx;
            }
            else
            {
                // 値を挿入
                txtInput.Text = txtInput.Text.Insert(txtInput.SelectionStart, btnObj.Text);
                txtInput.SelectionStart = intSelIdx + 1;
            }

            // カーソル位置を調整
            txtInput.Select();
        }

        /// <summary>
        /// 左ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLeft_Click(object sender, EventArgs e)
        {
            if (txtInput.SelectionStart > 0)
            {
                // マイナスがある場合はスルー
                if (txtInput.Text.Substring(txtInput.SelectionStart - 1, 1) != "-")
                {
                    txtInput.SelectionStart = txtInput.SelectionStart - 1;
                }
            }
            else
            {
                txtInput.SelectionStart = 0;
            }

            txtInput.Select();
        }

        /// <summary>
        /// 右ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRight_Click(object sender, EventArgs e)
        {
            txtInput.SelectionLength = 0;
            txtInput.Select();
            if (txtInput.SelectionStart <= txtInput.TextLength)
            {
                txtInput.SelectionStart = txtInput.SelectionStart + 1;
            }
            else
            {
                txtInput.SelectionStart = txtInput.TextLength - 1;
            }
        }

        /// <summary>
        /// プラス／マイナスボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlus_Click(object sender, EventArgs e)
        {
            if (txtInput.TextLength == 0)
            {
                return;
            }

            int intSelIdx = txtInput.SelectionStart;

            if (txtInput.Text.Substring(0, 1) == "-")
            {
                // 符号を削除
                txtInput.Text = txtInput.Text.Remove(0, 1);

                // 最大桁調整
                txtInput.MaxLength = txtInput.MaxLength - 1;

                // カーソル位置を調整
                txtInput.SelectionStart = intSelIdx - 1;
                txtInput.Select();
            }
        }

        /// <summary>
        /// プラス／マイナスボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMinus_Click(object sender, EventArgs e)
        {
            if (txtInput.TextLength == 0)
            {
                return;
            }

            int intSelIdx = txtInput.SelectionStart;

            if (txtInput.Text.Substring(0, 1) != "-")
            {
                // 最大桁調整
                txtInput.MaxLength = txtInput.MaxLength + 1;

                // 符号を挿入
                txtInput.Text = txtInput.Text.Insert(0, "-");

                // カーソル位置を調整
                txtInput.SelectionStart = intSelIdx + 1;
                txtInput.Select();
            }
        }

        /// <summary>
        /// ALLCLEARボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAllClear_Click(object sender, EventArgs e)
        {
            txtInput.Text = "";
            txtInput.Select();
        }

        /// <summary>
        /// DELボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, EventArgs e)
        {
            //右１文字を削除する

            int intSelIdx = txtInput.SelectionStart;

            // 後を削除
            if (txtInput.SelectionStart == txtInput.TextLength)
            {
                return;
            }
            else
            {
                txtInput.Text = txtInput.Text.Remove(txtInput.SelectionStart, 1);
            }

            // カーソル位置を調整
            txtInput.SelectionStart = intSelIdx;
            txtInput.Select();

            if (txtInput.Text == "-")
            {
                txtInput.Text = "";
            }
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
            else
            {
                if (txtInput.SelectionStart > 0)
                {
                    txtInput.Text = txtInput.Text.Remove(txtInput.SelectionStart - 1, 1);
                }
            }

            // カーソル位置を調整
            txtInput.SelectionStart = intSelIdx - 1;
            txtInput.Select();

            if (txtInput.Text == "-")
            {
                txtInput.Text = "";
            }
        }

        /// <summary>
        /// INSボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIns_Click(object sender, EventArgs e)
        {
            // INS切替
            if (bolIns == true)
            {
                bolIns = false;
            }
            else
            {
                bolIns = true;
            }
            txtInput.Select();
        }

        /// <summary>
        /// 入力テキストボックス選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInput_Enter(object sender, EventArgs e)
        {
            // マイナスより前は選択させない
            if (txtInput.SelectionStart == 0 && txtInput.TextLength > 0 && txtInput.Text.Substring(0, 1) == "-")
            {
                txtInput.SelectionStart = 1;
            }

            // INSモード範囲選択
            if (bolIns == true)
            {
                txtInput.SelectionLength = 1;
            }
            else
            {
                txtInput.SelectionLength = 0;
            }
        }

        /// <summary>
        /// 入力テキストボックスクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInput_Click(object sender, EventArgs e)
        {
            // マイナスより前は選択させない
            if (txtInput.SelectionStart == 0 && txtInput.TextLength > 0 && txtInput.Text.Substring(0, 1) == "-")
            {
                txtInput.SelectionStart = 1;
            }
        }
        #endregion
    }
}
