using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class Login : Form
    {
        // 作業者情報選関連
        public string m_strEmployeeNum;
        public string m_strWorkerNm;

        // 平行処理
        List<Task<Boolean>> m_lstTask = new List<Task<Boolean>>();

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Login()
        {
            InitializeComponent();

            // 一時フォルダ作成
            if (Directory.Exists(g_clsSystemSettingInfo.strTemporaryDirectory) == false)
                Directory.CreateDirectory(g_clsSystemSettingInfo.strTemporaryDirectory);

            // 一時ZIP解凍用フォルダ作成
            if (Directory.Exists(g_clsSystemSettingInfo.strTemporaryDirectory + Path.DirectorySeparatorChar + g_CON_DIR_MASTER_IMAGE) == false)
                Directory.CreateDirectory(g_clsSystemSettingInfo.strTemporaryDirectory + Path.DirectorySeparatorChar + g_CON_DIR_MASTER_IMAGE);

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// 欠点画像取り込み
        /// </summary>
        /// <returns></returns>
        private Boolean bolImpMasterImage()
        {
            string strTempFilePath = "";

            // マスタ画像の更新
            try
            {
                // マスタ画像格納ディレクトリを列挙
                foreach (string FilePath in Directory.GetFiles(g_clsSystemSettingInfo.strMasterImageDirectory, "*", SearchOption.AllDirectories))
                {
                    strTempFilePath = g_clsSystemSettingInfo.strTemporaryDirectory + Path.DirectorySeparatorChar +
                                      g_CON_DIR_MASTER_IMAGE + Path.DirectorySeparatorChar + Path.GetFileName(FilePath);

                    // 無い場合処理継続
                    if (File.Exists(strTempFilePath) == true)
                    {
                        // タイムスタンプ比較
                        if (File.GetLastWriteTime(FilePath).CompareTo(File.GetLastWriteTime(strTempFilePath)) <= 0)
                        {
                            continue;
                        }
                    }

                    // マスタ画像を一時フォルダにコピーする
                    File.Copy(FilePath,
                              g_clsSystemSettingInfo.strTemporaryDirectory + Path.DirectorySeparatorChar +
                              g_CON_DIR_MASTER_IMAGE + Path.DirectorySeparatorChar + Path.GetFileName(FilePath),
                              true);
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteEventLog(g_CON_LEVEL_WARN, ex.Message);
                return false;
            }
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Login_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            m_lstTask.Add(Task<Boolean>.Run(() => bolImpMasterImage()));
            System.Threading.Thread.Sleep(1000);

            Task.WaitAll(m_lstTask.ToArray());

            foreach (Task<Boolean> tsk in m_lstTask)
            {
                if (!tsk.Result)
                {
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0041, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // ユーザIDを初期選択
            txtUserId.Select();

            // 初期設定
            txtUserId.Text = "";
            rdbDispNum2.Checked = false;
            rdbDispNum4.Checked = false;
            rdbDispNum6.Checked = true;
            rdbDispNum9.Checked = false;

            this.ResumeLayout();
        }

        /// <summary>
        /// ユーザIDクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserId_Click(object sender, EventArgs e)
        {
            string strEmployeeNum = "";
            string strWorkerName = "";

            // 作業者検索画面を表示
            WorkerSelection frmTargetSelection = new WorkerSelection();
            frmTargetSelection.ShowDialog(this);
            this.Visible = true;

            // 返却値をセット
            strEmployeeNum = frmTargetSelection.strEmployeeNum;
            strWorkerName = frmTargetSelection.strWorkerName;

            if (!String.IsNullOrEmpty(strEmployeeNum))
            {
                // パラメータ反映
                m_strEmployeeNum = strEmployeeNum;
                m_strWorkerNm = strWorkerName;

                // ユーザIDに表示
                txtUserId.Text = m_strEmployeeNum + " " + m_strWorkerNm;

                // 入力不可にする
                txtUserId.ReadOnly = true;
                txtUserId.BackColor = SystemColors.Window;
            }
        }

        /// <summary>
        /// 表示数(画像)２クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum2_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum2.Checked = true;
        }

        /// <summary>
        /// 表示数(画像)４クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum4_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum4.Checked = true;
        }

        /// <summary>
        /// 表示数(画像)６クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum6_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum6.Checked = true;
        }

        /// <summary>
        /// 表示数(画像)９クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum9_Click(dynamic sender, EventArgs e)
        {
            rdbDispNum9.Checked = true;
        }

        /// <summary>
        /// ログインボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            int intDispNum = 0;

            // 入力チェック
            if (String.IsNullOrEmpty(m_strEmployeeNum))
            {
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0038, "作業者"), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            // 共通パラメータ設定
            if (this.rdbDispNum2.Checked == true) { intDispNum = 2; }
            else if (this.rdbDispNum4.Checked == true) { intDispNum = 4; }
            else if (this.rdbDispNum6.Checked == true) { intDispNum = 6; }
            else if (this.rdbDispNum9.Checked == true) { intDispNum = 9; }

            // ログイン処理
            g_clsLoginInfo.Login(m_strEmployeeNum, m_strWorkerNm, intDispNum);

            // 検査対象選択画面に遷移
            this.Visible = false;
            TargetSelection frmTargetSelection = new TargetSelection();
            frmTargetSelection.ShowDialog(this);
            this.Visible = true;
        }
        #endregion
    }
}
