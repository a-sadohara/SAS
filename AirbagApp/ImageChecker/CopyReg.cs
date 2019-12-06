using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageChecker.DTO;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class CopyReg : Form
    {
        TargetInfoDto objTargetInfoDto;
        int intRow;
        
        public int intRet { get; set; }

        public CopyReg(Image imgTarget, string strImgLocation, TargetInfoDto objTargetInfo,int intRowIndex, bool dummy = false)
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            pictureBox1.Image = System.Drawing.Image.FromFile(strImgLocation);
            pictureBox1.ImageLocation = System.IO.Path.GetFullPath(strImgLocation);

            comboBox1.SelectedIndex = 33;
            comboBox2.SelectedIndex = 2;
        }


        private void Button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("過検知で登録します。よろしいですか？"
                              , "確認"
                              , MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            intRet = 1;

            //this.Close();
        }

        private void BtnTanpatuWhite_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("NG理由：" + btnTanpatuWhite.Text.Replace("\r\n", "").Replace("　", "").Replace("□", "") + "で登録しますか？"
                                          , "確認"
                                          , MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            // ON
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("□","■");
            lblReason.Text = btnTanpatuWhite.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("■", "□");
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("■", "□");
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("■", "□");

            intRet = 1;

            //this.Close();
        }

        private void BtnTanpatuBlack_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("NG理由：" + btnTanpatuBlack.Text.Replace("\r\n", "").Replace("　", "").Replace("□", "") + "で登録しますか？"
                                          , "確認"
                                          , MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            // ON
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("□", "■");
            lblReason.Text = btnTanpatuBlack.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("■", "□");
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("■", "□");
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("■", "□");

            intRet = 1;

            //this.Close();
        }

        private void BtnRenzokuWhite_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("NG理由：" + btnRenzokuWhite.Text.Replace("\r\n", "").Replace("　", "").Replace("□", "") + "で登録しますか？"
                                          , "確認"
                                          , MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            // ON
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("□", "■");
            lblReason.Text = btnRenzokuWhite.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("■", "□");
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("■", "□");
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("■", "□");

            intRet = 1;

            //this.Close();
        }

        private void BtnRenzokuBlack_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("NG理由：" + btnRenzokuBlack.Text.Replace("\r\n", "").Replace("　", "").Replace("□", "") + "で登録しますか？"
                                          , "確認"
                                          , MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            // ON
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("□", "■");
            lblReason.Text = btnRenzokuBlack.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("■", "□");
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("■", "□");
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("■", "□");

            intRet = 1;

            //this.Close();
        }

        private void BtnOther_Click(object sender, EventArgs e)
        {
            ErrorReasonDTO errorReasonDto = new ErrorReasonDTO();
            SelectErrorReason frmErrorReason = new SelectErrorReason(errorReasonDto, true);
            frmErrorReason.ShowDialog(this);
            this.Visible = true;

            if (!string.IsNullOrEmpty(errorReasonDto.getStrErrorReason()))
            {
                // ON
                btnOther.Text = btnOther.Text.Replace("□", "■");
                lblReason.Text = string.Format("{0}({1})", btnOther.Text, errorReasonDto.getStrErrorReason());

                // OFF
                btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("■", "□");
                btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("■", "□");
                btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("■", "□");
                btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("■", "□");
                btnJudgeNG.Text = btnJudgeNG.Text.Replace("■", "□");

                intRet = 1;

                //this.Close();
            }
        }

        private void btnJudgeNG_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("NG理由：" + btnJudgeNG.Text.Replace("\r\n", "").Replace("　", "").Replace("□", "") + "で登録しますか？"
                                          , "確認"
                                          , MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            // ON
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("□", "■");
            lblReason.Text = btnJudgeNG.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("■", "□");
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("■", "□");
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("■", "□");
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");

            intRet = 1;

            //this.Close();
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogOut();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Int16 i;

            List<string> lstStr = new List<string>();

            for (i = 1; i < 100; ++i)
            {
                lstStr.Add(i.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<string> lstStr = new List<string>();

            lstStr.Add("A");
            lstStr.Add("B");
            lstStr.Add("C");
            lstStr.Add("D");

            ListWpf_WinForm frmListInWpf = new ListWpf_WinForm(lstStr);
            frmListInWpf.ShowDialog();
            this.Visible = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnNextReg_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
        }

        private void pictureBox1_Click(dynamic sender, EventArgs e)
        {
            ViewEnlargedimage frmViewImage = new ViewEnlargedimage(System.Drawing.Image.FromFile(sender.ImageLocation), sender.ImageLocation);
            this.Visible = true;
            frmViewImage.ShowDialog(this);
        }
    }
}
