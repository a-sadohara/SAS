using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static UserMasterMaintenance.Common;

namespace UserMasterMaintenance
{
    public partial class UserEdit : Form
    {
        public int intEditMode;
        public string UserNo;
        public string parUserNm;
        public string parUserYomiGana;

        public UserEdit(int parEditMode, 
                        string parUserNo = "", 
                        string parUserNm = "", 
                        string parUserYomiGana = "")
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            intEditMode = parEditMode;

            if (intEditMode == CON_EDITMODE_UPD)
            {
                // 社員番号
                txtUserNo.Text = parUserNo;
                txtUserNo.ReadOnly = true;
                // 作業者名
                if (parUserNm.Split('　').Count() == 2)
                {
                    txtUserNm_Sei.Text = parUserNm.Split('　')[0];
                    txtUserNm_Mei.Text = parUserNm.Split('　')[1];
                }
                else
                {
                    if (txtUserNm_Sei.MaxLength < parUserNm.Length)
                    {
                        txtUserNm_Sei.MaxLength = parUserNm.Length;
                    }
                    txtUserNm_Sei.Text = parUserNm;
                }
                // 読み仮名
                if (parUserYomiGana.Split('　').Count() == 2)
                {
                    txtUserYomiGana_Sei.Text = parUserYomiGana.Split('　')[0];
                    txtUserYomiGana_Mei.Text = parUserYomiGana.Split('　')[1];
                }
                else
                {
                    if (txtUserYomiGana_Sei.MaxLength < parUserYomiGana.Length)
                    {
                        txtUserYomiGana_Sei.MaxLength = parUserYomiGana.Length;
                    }
                    txtUserYomiGana_Sei.Text = parUserYomiGana;
                }
            }
            else
            {
                txtUserNo.Clear();
                txtUserNm_Sei.Clear();
                txtUserNm_Mei.Clear();
                txtUserYomiGana_Sei .Clear();
            }
        }

        private void btnFix_Click(object sender, EventArgs e)
        {
            if (intEditMode == CON_EDITMODE_REG) { MessageBox.Show("登録しました"); }
            if (intEditMode == CON_EDITMODE_UPD) { MessageBox.Show("更新しました"); }
            this.Close();
        }
    }
}
