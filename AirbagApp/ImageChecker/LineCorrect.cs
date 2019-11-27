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
        private int m_Correct = 0;
        private int m_CorrectInit_From = 0;
        private int m_CorrectInit_To = 0;

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
            lblCorrect.Text = "補正値：±0";

            m_Correct = 0;
            lblKensaHaniLine_Before.Text = KensaHaniLine;
            lblKensaHaniLine_After.Text = KensaHaniLine;

            string[] strCorrect = KensaHaniLine.Split('～');
            if (strCorrect.Length == 2)
            {
                m_CorrectInit_From = int.Parse(strCorrect[0]);
                m_CorrectInit_To = int.Parse(strCorrect[1]);
            } 
            else 
            {
                MessageBox.Show("検査範囲業が [開始～終了] の関係になっていません。値を確認してください");
                return;
            }

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("行補正をしますか？"
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

        private void btnMinus_Click(object sender, EventArgs e)
        {
            m_Correct -= 1;
            lblCorrect.Text = "補正値：";
            if (m_Correct > 0) { lblCorrect.Text = lblCorrect.Text + "+"; } else if (m_Correct == 0) { lblCorrect.Text = lblCorrect.Text + "±"; }
            lblCorrect.Text = lblCorrect.Text + m_Correct.ToString();

            lblKensaHaniLine_After.Text = m_CorrectInit_From.ToString() + "～" + (m_CorrectInit_To + m_Correct).ToString();
        }

        private void btnPlus_Click(object sender, EventArgs e)
        {
            m_Correct += 1;
            lblCorrect.Text = "補正値：";
            if (m_Correct > 0) { lblCorrect.Text = lblCorrect.Text + "+"; } else if (m_Correct == 0) { lblCorrect.Text = lblCorrect.Text + "±"; }
            lblCorrect.Text = lblCorrect.Text + m_Correct.ToString();

            lblKensaHaniLine_After.Text = m_CorrectInit_From.ToString() + "～" + (m_CorrectInit_To + m_Correct).ToString();
        }
    }
}
