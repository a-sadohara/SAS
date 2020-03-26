using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProductMstMaintenance
{
    public partial class ProgressForm : Form
    {
        private delegate void Del();

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProgressForm()
        {
            InitializeComponent();
        }

        public ProgressForm(string strMsg)
        {
            InitializeComponent();
            // メッセージ登録
            lblMessage.Text = strMsg;
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
            this.TransparencyKey = this.BackColor;
            this.ResumeLayout();
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Activate();
        }

        /// <summary>
        /// フォーム描写時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportImageZipProgressForm_Shown(object sender, EventArgs e)
        {
            // フォームを非活性にした場合、ラベルカラーが黒色固定となるため
            // 独自で生成したラベルコントロールに置き換える
            MyLabel lblMyLabel = new MyLabel();
            lblMyLabel.Name = "lblMyLabel";
            lblMyLabel.Text = lblMessage.Text;
            lblMyLabel.Anchor = lblMessage.Anchor;
            lblMyLabel.AutoSize = lblMessage.AutoSize;
            lblMyLabel.Dock = lblMessage.Dock;
            lblMyLabel.Location = lblMessage.Location;
            lblMyLabel.Margin = lblMessage.Margin;
            lblMyLabel.BackColor = lblMessage.BackColor;
            lblMyLabel.BorderStyle = lblMessage.BorderStyle;
            lblMyLabel.Font = lblMessage.Font;
            lblMyLabel.ForeColor = lblMessage.ForeColor;
            lblMyLabel.Size = lblMessage.Size;
            lblMyLabel.TabIndex = lblMessage.TabIndex;
            panel1.Controls.Remove(lblMessage);
            lblMessage.Dispose();
            panel1.Controls.Add(lblMyLabel);
            this.Enabled = false;
        }
    }

    class MyLabel : Label
    {
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (this.Enabled)
            {
                this.SetStyle(ControlStyles.UserPaint, false);
            }
            else
            {
                this.SetStyle(ControlStyles.UserPaint, true);
            }

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Brush b = new SolidBrush(this.ForeColor);

            e.Graphics.DrawString(this.Text, this.Font, b, -1, 1);
            b.Dispose();
        }
    }
}