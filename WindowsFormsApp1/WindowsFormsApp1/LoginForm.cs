using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp1.Common;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            if (!String.IsNullOrEmpty(parUserNo)) 
            {
                txtUser.Text = parUserNo + " " + parUserNm;
            }
            switch (parDispNum)
            {
                case 2:
                    rdbDispNum2.Checked = true;
                    break;
                case 4:
                    rdbDispNum4.Checked = true;
                    break;
                case 9:
                    rdbDispNum9.Checked = true;
                    break;
                default:
                    rdbDispNum9.Checked = true;
                    break;
            }
        }
        
        private void Button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(parUserNo))
            {
                MessageBox.Show("職員を選択してください。");
                return;
            }

            if (this.rdbDispNum2.Checked == true) { parDispNum = 2; }
            if (this.rdbDispNum4.Checked == true) { parDispNum = 4; }
            if (this.rdbDispNum9.Checked == true) { parDispNum = 9; }

            this.Visible = false;
            TargetSelection frmTargetSelection = new TargetSelection();

            frmTargetSelection.ShowDialog(this);
            
        }

        private void picDispNum2_Click(object sender, EventArgs e)
        {
            rdbDispNum2.Checked = true;
        }

        private void picDispNum4_Click(object sender, EventArgs e)
        {
            rdbDispNum4.Checked = true;
        }

        private void picDispNum9_Click(object sender, EventArgs e)
        {
            rdbDispNum9.Checked = true;
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            UserSelection frmTargetSelection = new UserSelection();
            frmTargetSelection.ShowDialog(this);

            parUserNo = frmTargetSelection.parUserNo;
            parUserNm = frmTargetSelection.parUserNm;

            txtUser.Text = parUserNo + " " + parUserNm;
        }
    }
}
