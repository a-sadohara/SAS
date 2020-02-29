using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class ViewEnlargedimage : Form
    {
        // パラメータ関連（不変）
        private readonly string m_strOrgImagepath = string.Empty;
        private readonly string m_strMarkingImagepath = string.Empty;

        // 表示モード関連
        private int m_intDispMode = 1;      
        private const int m_CON_DISP_MODE_MRK = 1;      // 1:マーキング
        private const int m_CON_DISP_MODE_ORG = 2;      // 2:オリジナル

        private readonly SemaphoreSlim _clickSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _doubleClickSemaphore = new SemaphoreSlim(0);

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="strOrgImagepath">オリジナル画像パス</param>
        /// <param name="strMarkingImagepath">マーキング画像パス</param>
        public ViewEnlargedimage(string strOrgImagepath, string strMarkingImagepath)
        {
            m_strOrgImagepath = strOrgImagepath;
            m_strMarkingImagepath = strMarkingImagepath;

            InitializeComponent();

            // フォーム画面サイズ計算
            int intHeight = -1;
            int intWidth = -1;
            double dblRate = 1.0;

            // ディスプレイ高さいっぱい表示する。
            // 補正なしの縦サイズとディスプレイ高さから比率を算出
            dblRate = (double)(Screen.PrimaryScreen.WorkingArea.Height - 5) / (double)this.Size.Height;

            intHeight = (int)((double)this.Size.Height * dblRate);
            intWidth = (int)((double)this.Size.Width * dblRate);

            this.Size = new Size(intWidth, intHeight);
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewEnlargedimage_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            // 画像を設定する
            FileStream fs;
            if (File.Exists(m_strMarkingImagepath) == false)
            {
                fs = new FileStream(g_CON_NO_IMAGE_FILE_PATH, FileMode.Open, FileAccess.Read);
            }
            else
            {
                fs = new FileStream(m_strMarkingImagepath, FileMode.Open, FileAccess.Read);
            }
            
            picImage.Image = Image.FromStream(fs);
            fs.Close();

            m_intDispMode = m_CON_DISP_MODE_MRK;

            this.ResumeLayout();
        }

        /// <summary>
        /// ダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picImage_DoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// イメージ画像クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void picImage_Click(object sender, EventArgs e)
        {
            PictureBox picImage = (PictureBox)sender;

            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                {
                    return;
                }
            }
            finally
            {
                _clickSemaphore.Release();
            }

            // オリジナル/マーキング切替
            FileStream fs;
            if (m_intDispMode == m_CON_DISP_MODE_ORG)
            {
                // オリジナル
                if (File.Exists(m_strMarkingImagepath) == false)
                {
                    fs = new FileStream(g_CON_NO_IMAGE_FILE_PATH, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    fs = new FileStream(m_strMarkingImagepath, FileMode.Open, FileAccess.Read);
                    this.picImage.ImageLocation = m_strMarkingImagepath;
                }
                
                Image.FromStream(fs);

                m_intDispMode = m_CON_DISP_MODE_MRK;
            }    
            else
            {
                // マーキング
                if (File.Exists(m_strOrgImagepath) == false)
                {
                    fs = new FileStream(g_CON_NO_IMAGE_FILE_PATH, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    fs = new FileStream(m_strOrgImagepath, FileMode.Open, FileAccess.Read);
                    this.picImage.ImageLocation = m_strOrgImagepath;
                }
                    
                Image.FromStream(fs);

                m_intDispMode = m_CON_DISP_MODE_ORG;
            }
            fs.Close();
        }
        #endregion
    }
}
