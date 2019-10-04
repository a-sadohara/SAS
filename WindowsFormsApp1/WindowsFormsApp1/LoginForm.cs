using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        public static String parUserNo;
        public static String parUserNm;
        public static short parDispNum;

        public LoginForm()
        {
            InitializeComponent();

            rdbDispNum9.Checked = true;
        }
        
        private void Button1_Click(object sender, EventArgs e)
        {
            if (this.rdbDispNum2.Checked == true) { parDispNum = 2; }
            if (this.rdbDispNum4.Checked == true) { parDispNum = 4; }
            if (this.rdbDispNum9.Checked == true) { parDispNum = 9; }

            this.Visible = false;
            TargetSelection frmTargetSelection = new TargetSelection();

            frmTargetSelection.ShowDialog(this);
            this.Close();
            
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

            textBox2.Text = parUserNo + " " + parUserNm;
        }
    }
}
