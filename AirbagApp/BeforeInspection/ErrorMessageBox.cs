using System;
using System.Windows.Forms;
using static BeforeInspection.Common;

namespace BeforeInspection
{
    public partial class ErrorMessageBox : Form
    {
        // パラメータ
        private string m_strDetail = "";    // 詳細

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="strTitle">タイトル</param>
        /// <param name="strDetail">詳細</param>
        public ErrorMessageBox(string strDetail)
        {           
            m_strDetail = strDetail;

            InitializeComponent();

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
