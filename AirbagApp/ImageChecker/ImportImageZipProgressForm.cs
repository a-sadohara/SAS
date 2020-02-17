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

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// ファイル監視中止
        /// </summary>
        private void StopSysFileWatcher()
        {
            //this.Close();
            // ※InvalidOperationExceptionが発生
            new Thread(new ThreadStart(delegate {
                Invoke((Del)delegate {
                    this.Close();
                });
            })).Start();
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

            this.ResumeLayout();
        }
        #endregion
    }
}
