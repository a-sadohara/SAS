using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                lblMessage.TextAlign = ContentAlignment.MiddleLeft;
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
        /// 初期表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ErrorMessageBox_Shown(object sender, EventArgs e)
        {
            await Task.Delay(10);
            this.lblMessage.Click += new EventHandler(this.ErrorMessage_Click);
            this.tlpMain.Click += new EventHandler(this.ErrorMessage_Click);
            this.lblDetail.Click += new EventHandler(this.ErrorMessage_Click);
            this.picIcon.Click += new EventHandler(this.ErrorMessage_Click);
            this.tableLayoutPanel1.Click += new EventHandler(this.ErrorMessage_Click);
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