using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageChecker
{
    public partial class LineCorrect : Form
    {
        public LineCorrect(string Goki,
                           string Sashizu,
                           string HanNo,
                           string HinNo,
                           string KensaNo,
                           string HansoStaDate,
                           string HansoEndDate,
                           string KensaHaniLine)
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            lblGoki.Text = Goki;
            lblSashizu.Text = Sashizu;
            lblHanNo.Text = HanNo;
            lblHinNo.Text = HinNo;
            lblKensaNo.Text = KensaNo;
            lblHansoStaDate.Text = HansoStaDate;
            lblHansoStaDate.Text = HansoEndDate;
            lblKensaHaniLine.Text = KensaHaniLine;

            txtLine.Text = "";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string plusminus = "";
            if (txtLine.Text == "")
            {
                MessageBox.Show("行補正値を入力してください");
                return;
            }
            else
            {
                if(int.Parse(txtLine.Text) >= 0)
                {
                    plusminus = "+";
                }
                else
                {
                    plusminus = "-";
                }
            }

            if (MessageBox.Show("行補正値 [" + plusminus + txtLine.Text.ToString().Replace("+","").Replace("-","") + "] で登録しますか？"
                              , "確認"
                              , MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
