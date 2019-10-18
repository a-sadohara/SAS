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
using Npgsql;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        public String parUserNo;
        public String parUserNm;
        public NpgsqlConnection NpgsqlCon;
        public const string strConnect = "Server=192.168.2.17;Port=5432;User ID=postgres;Database=postgres;Password=password;Enlist=true";
        public DataTable dtData;

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

        private void txtUser_FocusIn(object sender, EventArgs e)
        {
            txtUser.ReadOnly = false;
            txtUser.Text = "";
            txtUser.MaxLength = 4;
        }

        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChkUserNo();
            }
        }

        private void txtUser_DoubleClick(object sender, EventArgs e)
        {
            ChkUserNo(false);
        }

        private void ChkUserNo(bool parChk = true)
        {
            string strSQL = "";

            if (parChk)
            {
                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(strConnect))
                {
                    NpgsqlCon.Open();

                    // SQL抽出
                    NpgsqlCommand NpgsqlCom = null;
                    NpgsqlDataAdapter NpgsqlDtAd = null;
                    dtData = new DataTable();
                    strSQL += "SELECT USERNAME FROM SAGYOSYA ";
                    strSQL += "WHERE USERNO = '" + txtUser.Text + "'";
                    NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                    NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                    NpgsqlDtAd.Fill(dtData);

                    if (dtData.Rows.Count > 0)
                    {
                        parUserNo = txtUser.Text;
                        parUserNm = dtData.Rows[0][0].ToString();
                    }
                    else
                    {
                        MessageBox.Show("入力された職員番号は存在しません");

                        UserSelection frmTargetSelection = new UserSelection();
                        frmTargetSelection.ShowDialog(this);

                        parUserNo = frmTargetSelection.parUserNo;
                        parUserNm = frmTargetSelection.parUserNm;
                    }

                    txtUser.Text = parUserNo + " " + parUserNm;
                }
            }
            else 
            {
                UserSelection frmTargetSelection = new UserSelection();
                frmTargetSelection.ShowDialog(this);

                parUserNo = frmTargetSelection.parUserNo;
                parUserNm = frmTargetSelection.parUserNm;

                txtUser.Text = parUserNo + " " + parUserNm;
            }

            // 値が入っている場合は入力不可にする
            if (!String.IsNullOrEmpty(txtUser.Text))
            {
                txtUser.ReadOnly = true;
            }

        }

        private void txtUser_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r' && e.KeyChar != '\b' && e.KeyChar < '0' || '9' < e.KeyChar)
            {
                e.Handled = true;
            }
        }

    }
}
