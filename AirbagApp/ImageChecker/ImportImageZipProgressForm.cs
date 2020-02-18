using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class ImportImageZipProgressForm : Form
    {
        private delegate void Del();

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ImportImageZipProgressForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;
            //this.WindowState = FormWindowState.Minimized;
            this.TopMost = true;
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
            timer1.Start();
            this.ResumeLayout();
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Normal;
            this.Activate();
            timer1.Stop();
        }
    }
}
