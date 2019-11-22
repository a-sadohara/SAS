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
    public partial class CheckExcept : Form
    {
        public CheckExcept(string Goki,
                           string Sashizu,
                           string HanNo,
                           string HinNo,
                           string KensaNo,
                           string HansoStaDate,
                           string HansoEndDate,
                           string KensaHaniLine)
        {
            InitializeComponent();

            lblGoki.Text = Goki;
            lblSashizu.Text = Sashizu;
            lblHanNo.Text = HanNo;
            lblHinNo.Text = HinNo;
            lblKensaNo.Text = KensaNo;
            lblHansoStaDate.Text = HansoStaDate;
            lblHansoStaDate.Text = HansoEndDate;
            lblKensaHaniLine.Text = KensaHaniLine;

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("検査対象外にしますか？"
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
