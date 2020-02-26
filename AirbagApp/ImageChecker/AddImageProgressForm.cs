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

        private string m_strChkFilePath = string.Empty;

        private FileSystemWatcher m_fsWatcher;

        private delegate void Del();

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="strChkFilePath">確認ファイルパス</param>
        public AddImageProgressForm(string strChkFilePath)
        {
            bolChgFile = false;

            m_strChkFilePath = strChkFilePath;

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
            this.SuspendLayout();

            bool bolProcOkNg = false;

            DispatcherTimer dtVisiblebtnCancel;

            try
            {
                // 同期的に未検知画像連携ディレクトリの監視する
                m_fsWatcher = new FileSystemWatcher();

                m_fsWatcher.Path = g_clsSystemSettingInfo.strCompletionNoticeCooperationDirectory;
                m_fsWatcher.Filter = string.Empty;
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

                bolProcOkNg = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0058, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0059, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (bolProcOkNg == false)
                    this.Close();

                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ファイル監視(変更)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void fsWatcher_Changed(System.Object source, System.IO.FileSystemEventArgs e)
        {
            // ファイル存在チェック
            if (File.Exists(m_strChkFilePath) == false)
            {
                return;
            }

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
