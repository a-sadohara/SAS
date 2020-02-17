using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class AddImageProgressForm : Form
    {
        public bool bolChgFile { get; set; }

        private FileSystemWatcher m_fsWatcher;

        private delegate void Del();

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AddImageProgressForm()
        {
            this.SuspendLayout();

            bolChgFile = false;

            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// ファイル監視中止
        /// </summary>
        private void StopSysFileWatcher()
        {
            m_fsWatcher.EnableRaisingEvents = false;
            m_fsWatcher.Dispose();
            m_fsWatcher = null;

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
            DispatcherTimer dtVisiblebtnCancel;

            // 同期的に未検知画像連携ディレクトリの監視する
            m_fsWatcher = new FileSystemWatcher();

            m_fsWatcher.Path = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectory;
            m_fsWatcher.Filter = "";
            m_fsWatcher.IncludeSubdirectories = false;
            //ファイル名とディレクトリ名と最終書き込む日時の変更を監視
            m_fsWatcher.NotifyFilter =
                NotifyFilters.FileName
                | NotifyFilters.DirectoryName
                | NotifyFilters.LastWrite;
             
            // イベントハンドラの追加
            m_fsWatcher.Changed += new FileSystemEventHandler(fsWatcher_Changed);
            m_fsWatcher.Created += new FileSystemEventHandler(fsWatcher_Changed);
            m_fsWatcher.Deleted += new FileSystemEventHandler(fsWatcher_Changed);
            m_fsWatcher.Renamed += new RenamedEventHandler(fsWatcher_Changed);

            // ボタンを有効にする
            dtVisiblebtnCancel = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            dtVisiblebtnCancel.Start();
            dtVisiblebtnCancel.Tick += (s, args) =>
            {
                dtVisiblebtnCancel.Stop();
                btnCancel.Visible = true;
            };

            // 監視を開始する
            m_fsWatcher.EnableRaisingEvents = true;

            this.ResumeLayout();
        }

        /// <summary>
        /// ファイル監視(ファイル変更)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void fsWatcher_Changed(System.Object source, System.IO.FileSystemEventArgs e)
        {
            bolChgFile = true;

            StopSysFileWatcher();
        }

        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            StopSysFileWatcher();
        }
        #endregion
    }
}
