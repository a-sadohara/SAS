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
        private string m_strEmployeeNum;
        private string m_strWorkerNm;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Login()
        {
            InitializeComponent();

            // 一時フォルダ作成
            if (Directory.Exists(g_clsSystemSettingInfo.strTemporaryDirectory) == false)
            {
                Directory.CreateDirectory(g_clsSystemSettingInfo.strTemporaryDirectory);
            }

            // マスタ画面用フォルダ作成
            if (Directory.Exists(g_strMasterImageDirPath) == false)
            {
                Directory.CreateDirectory(g_strMasterImageDirPath);
            }

            // 一時ZIP解凍用フォルダ作成
            if (Directory.Exists(g_strZipExtractDirPath) == false)
            {
                Directory.CreateDirectory(g_strZipExtractDirPath);
            }

            this.StartPosition = FormStartPosition.CenterScreen;
        }


        /// <summary>
        /// 一時ディレクトリ初期化
        /// </summary>
        /// <returns></returns>
        private async Task<Boolean> bolInitializeTempDir()
        {
            string strZipExtractDirPath = string.Empty;
            string strFaultImageDirPath = string.Empty;

            try
            {
                // 存在しない一時ディレクトリ作成
                // マーキングマスタ画像格納先
                if (Directory.Exists(g_strMasterImageDirMarking) == false)
                {
                    Directory.CreateDirectory(g_strMasterImageDirMarking);
                }

                // 一時ディレクトリ内のサブディレクトリとファイルを削除
                // マーキングマスタ画像格納先
                foreach (string DirPath in Directory.GetDirectories(g_strMasterImageDirMarking, "*.*", SearchOption.TopDirectoryOnly))
                {
                    Directory.Delete(DirPath, true);
                }
                foreach (string FilePath in Directory.GetFiles(g_strMasterImageDirMarking, "*.*", SearchOption.TopDirectoryOnly))
                {
                    File.Delete(FilePath);
                }

                List<string> lstUnitNum = new List<string>
                {
                    g_strUnitNumN1,
                    g_strUnitNumN2,
                    g_strUnitNumN3,
                    g_strUnitNumN4
                };

                for (int rowIndex = 0; rowIndex < lstUnitNum.Count; rowIndex++)
                {
                    strZipExtractDirPath = Path.Combine(g_strZipExtractDirPath, lstUnitNum[rowIndex]);
                    strFaultImageDirPath = Path.Combine(g_clsSystemSettingInfo.strFaultImageDirectory, lstUnitNum[rowIndex]);

                    // ZIP解凍用の一時ディレクトリが存在するかチェックする
                    if (!Directory.Exists(strZipExtractDirPath))
                    {
                        Directory.CreateDirectory(strZipExtractDirPath);
                    }

                    // 欠点画像用の一時ディレクトリが存在するかチェックする
                    if (!Directory.Exists(strFaultImageDirPath))
                    {
                        Directory.CreateDirectory(strFaultImageDirPath);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteEventLog(g_CON_LEVEL_WARN, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// マスタ画像取り込み
        /// </summary>
        /// <returns>true:正常終了 false:異常終了</returns>
        private async Task<Boolean> bolImpMasterImage()
        {
            string strTempFilePath = string.Empty;

            try
            {
                // 一時ディレクトリ作成
                // ルート
                if (Directory.Exists(g_clsSystemSettingInfo.strTemporaryDirectory) == false)
                {
                    Directory.CreateDirectory(g_clsSystemSettingInfo.strTemporaryDirectory);
                }

                // マスタ画像格納先
                if (Directory.Exists(g_strMasterImageDirPath) == false)
                {
                    Directory.CreateDirectory(g_strMasterImageDirPath);
                }

                // 一時ディレクトリ初期化
                if (!bolInitializeTempDir().Result)
                {
                    return false;
                }

                // マスタ画像の更新
                // マスタ画像格納ディレクトリを列挙
                foreach (string FilePath in Directory.GetFiles(g_clsSystemSettingInfo.strMasterImageDirectory, "*", SearchOption.AllDirectories))
                {
                    strTempFilePath = Path.Combine(g_strMasterImageDirPath, Path.GetFileName(FilePath));

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
                              Path.Combine(g_strMasterImageDirPath, Path.GetFileName(FilePath)),
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
        private async void Login_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            Task<Boolean> tskCopyMstImg = Task.Run(() => bolImpMasterImage());

            // ユーザIDを初期選択
            txtUserId.Select();

            // 初期設定
            txtUserId.Text = string.Empty;
            rdbDispNum2.Checked = false;
            rdbDispNum4.Checked = false;
            rdbDispNum6.Checked = true;
            rdbDispNum9.Checked = false;

            this.ResumeLayout();

            await Task.WhenAll(tskCopyMstImg);
            if (!tskCopyMstImg.Result)
            {
                return;
            }

        }

        /// <summary>
        /// ユーザIDクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserId_Click(object sender, EventArgs e)
        {
            string strEmployeeNum = string.Empty;
            string strWorkerName = string.Empty;

            // 作業者検索画面を表示
            using (WorkerSelection frmTargetSelection = new WorkerSelection())
            {
                frmTargetSelection.ShowDialog(this);
                this.Visible = true;

                // 返却値をセット
                strEmployeeNum = frmTargetSelection.strEmployeeNum;
                strWorkerName = frmTargetSelection.strWorkerName;
            }

            if (!String.IsNullOrEmpty(strEmployeeNum))
            {
                // パラメータ反映
                m_strEmployeeNum = strEmployeeNum;
                m_strWorkerNm = strWorkerName;

                // ユーザIDに表示
                txtUserId.Text = string.Format("{0} {1}", m_strEmployeeNum, m_strWorkerNm);

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
        private void picDispNum2_Click(object sender, EventArgs e)
        {
            rdbDispNum2.Checked = true;
        }

        /// <summary>
        /// 表示数(画像)４クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum4_Click(object sender, EventArgs e)
        {
            rdbDispNum4.Checked = true;
        }

        /// <summary>
        /// 表示数(画像)６クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum6_Click(object sender, EventArgs e)
        {
            rdbDispNum6.Checked = true;
        }

        /// <summary>
        /// 表示数(画像)９クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picDispNum9_Click(object sender, EventArgs e)
        {
            rdbDispNum9.Checked = true;
        }

        /// <summary>
        /// ログインボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            int intDispNum = 0;

            // 入力チェック
            if (String.IsNullOrEmpty(m_strEmployeeNum))
            {
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgE0038, "作業者"), g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUserId_Click(null, null);
                return;
            }

            // 共通パラメータ設定
            if (this.rdbDispNum2.Checked == true) { intDispNum = 2; }
            if (this.rdbDispNum4.Checked == true) { intDispNum = 4; }
            if (this.rdbDispNum6.Checked == true) { intDispNum = 6; }
            if (this.rdbDispNum9.Checked == true) { intDispNum = 9; }

            // ログイン処理
            g_clsLoginInfo.Login(m_strEmployeeNum, m_strWorkerNm, intDispNum);
            g_datetimePrevReplicate = DateTime.MinValue;
            g_bolGridRepresentationFlg = true;

            // 検査対象選択画面に遷移
            this.Visible = false;
            frmTargetSelection.ShowDialog(this);

            this.Refresh();

            this.Visible = true;

            // 一時ディレクトリ初期化
            Task<Boolean> tskCopyMstImg = Task.Run(() => bolInitializeTempDir());

            await Task.WhenAll(tskCopyMstImg);
            if (!tskCopyMstImg.Result)
            {
                return;
            }
        }
        #endregion
    }
}