using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProductMstMaintenance
{
    public partial class ProgressForm : Form
    {
        private delegate void Del();

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProgressForm()
        {
            InitializeComponent();
        }

        public ProgressForm(string strMsg)
        {
            InitializeComponent();
            // メッセージ登録
            lblMessage.Text = strMsg;
        }

        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressForm_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();
            this.TransparencyKey = this.BackColor;
            this.ResumeLayout();
        }
        #endregion

    }
}