using System;
using System.Drawing;
using System.Windows.Forms;
using static BeforeInspection.Common;

namespace BeforeInspection
{
    public partial class ErrorMessageBox : Form
    {
        // パラメータ
        private string m_strDetail = string.Empty;    // 詳細

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="strDetail">詳細</param>
        /// <param name="bolWarningMessageFlag">警告メッセージフラグ</param>
        public ErrorMessageBox(string strDetail, bool bolWarningMessageFlag = false)
        {
            InitializeComponent();

            if (bolWarningMessageFlag)
            {
                lblMessage.Text = strDetail;
                lblMessage.Font = new Font(lblMessage.Font.OriginalFontName, 26);
            }
            else
            {
                m_strDetail = strDetail;
            }

            this.FormBorderStyle = FormBorderStyle.None;
        }

        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorMessageBox_Load(object sender, EventArgs e)
        {
            lblDetail.Text = m_strDetail;
        }

        /// <summary>
        /// フォームクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorMessage_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
