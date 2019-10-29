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
using WindowsFormsApp1.DTO;
using static WindowsFormsApp1.Common;

namespace WindowsFormsApp1
{
    public partial class ResultCheck : Form
    {
        TargetInfoDto objTargetInfoDto;
        int intRow;

        // 実際の座標（Input値）
        private int intPctWidthFact = 1000;
        private int intPctHeightFact = 1000;
        private int intPointWidthFact = 100;
        private int intPointHeightFact = 100;
        private int intPointXFact = 0;    // 取得
        private int intPointYFact = 0;    // 取得

        // 画面（モニタ）上の座標
        private int intPctWidthDisp = 0;
        private int intPctHeightDisp = 0;
        private int intPointWidthDisp = 0;
        private int intPointHeightDisp = 0;
        private int intReferencePointXDisp = 0;
        private int intReferencePointYDisp = 0;
        private int intPointXDisp = 0;
        private int intPointYDisp = 0;

        // 比率
        private Double dblPctWidthRatio = 0.0;
        private Double dblPctHeightRatio = 0.0;

        // 画像向き
        private const string m_CON_FLIP_S = "S";
        private const string m_CON_FLIP_X = "X";
        private const string m_CON_FLIP_Y = "Y";
        private const string m_CON_FLIP_R = "R";
        private string m_strFlip = m_CON_FLIP_S;
        private Bitmap m_bmpImgInit = null;

        public ResultCheck(TargetInfoDto objTargetInfo,int intRowIndex)
        {
            InitializeComponent();

            //// Buttonのフォント調整
            if ((int)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width >= 1920)
            {
                // OFF
                btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("　", "").Replace("\r\n", "");
                btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("　", "").Replace("\r\n", "");
                btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("　", "").Replace("\r\n", "");
                btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("　", "").Replace("\r\n", "");
                btnOther.Text = btnOther.Text.Replace("　", "").Replace("\r\n", "");
                btnJudgeNG.Text = btnJudgeNG.Text.Replace("　", "").Replace("\r\n", "");
                btnTanpatuWhite.TextAlign = ContentAlignment.MiddleCenter;
                btnRenzokuWhite.TextAlign = ContentAlignment.MiddleCenter;
                btnTanpatuBlack.TextAlign = ContentAlignment.MiddleCenter;
                btnRenzokuBlack.TextAlign = ContentAlignment.MiddleCenter;
                btnOther.TextAlign = ContentAlignment.MiddleCenter;
                btnJudgeNG.TextAlign = ContentAlignment.MiddleCenter;
            }

            objTargetInfoDto = objTargetInfo;
            intRow = intRowIndex;
            
            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            comboBox1.SelectedIndex = 33;
            comboBox2.SelectedIndex = 2;

            Bitmap bmpImgInit = new Bitmap(pictureBox5.Image);
            bmpImgInit.RotateFlip(RotateFlipType.Rotate90FlipX);
            m_bmpImgInit = new Bitmap(bmpImgInit);
            pictureBox5.Image = bmpImgInit;
        }


        private void Button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnTanpatuWhite_Click(object sender, EventArgs e)
        {
            // ON
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("□","■");
            lblReason.Text = btnTanpatuWhite.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("■", "□");
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("■", "□");
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("■", "□");

            button6.Enabled = true;
        }

        private void BtnTanpatuBlack_Click(object sender, EventArgs e)
        {
            // ON
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("□", "■");
            lblReason.Text = btnTanpatuBlack.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("■", "□");
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("■", "□");
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("■", "□");

            button6.Enabled = true;
        }

        private void BtnRenzokuWhite_Click(object sender, EventArgs e)
        {
            // ON
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("□", "■");
            lblReason.Text = btnRenzokuWhite.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("■", "□");
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("■", "□");
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("■", "□");

            button6.Enabled = true;
        }

        private void BtnRenzokuBlack_Click(object sender, EventArgs e)
        {
            // ON
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("□", "■");
            lblReason.Text = btnRenzokuBlack.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("■", "□");
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("■", "□");
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("■", "□");

            button6.Enabled = true;
        }

        private void BtnOther_Click(object sender, EventArgs e)
        {
            ErrorReasonDTO errorReasonDto = new ErrorReasonDTO();
            SelectErrorReason frmErrorReason = new SelectErrorReason(errorReasonDto);
            frmErrorReason.ShowDialog(this);

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

                button6.Enabled = true;
            }
        }

        private void btnJudgeNG_Click(object sender, EventArgs e)
        {
            // ON
            btnJudgeNG.Text = btnJudgeNG.Text.Replace("□", "■");
            lblReason.Text = btnJudgeNG.Text.Replace("\r\n", "").Replace("　", "");

            // OFF
            btnTanpatuWhite.Text = btnTanpatuWhite.Text.Replace("■", "□");
            btnRenzokuWhite.Text = btnRenzokuWhite.Text.Replace("■", "□");
            btnTanpatuBlack.Text = btnTanpatuBlack.Text.Replace("■", "□");
            btnRenzokuBlack.Text = btnRenzokuBlack.Text.Replace("■", "□");
            btnOther.Text = btnOther.Text.Replace("■", "□");

            button6.Enabled = true;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DataTable dtTargetInfo = objTargetInfoDto.getTargetInfoDTO();
            dtTargetInfo.Rows[intRow]["Status"] = "3";

            objTargetInfoDto.setTargetInfoDTO(dtTargetInfo);
            this.Close();
        }

        private void ResultCheck_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            button6.Enabled = false;
            lblReason.Text = "";

            lblUser.Text = "作業者名：" + g_parUserNm;

            // 検査方向のテーブルパネルの描画調整
            tableLayoutPanel2.Location = new Point((tableLayoutPanel1.Controls[1].ClientSize.Width / 2 - tableLayoutPanel2.Controls[1].ClientSize.Width ), tableLayoutPanel2.Location.Y);

            DrwFrame();
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Result frmResult = new Result(objTargetInfoDto,intRow);
            frmResult.ShowDialog(this);
            this.Close();
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

            ListWpf_WinForm frmListInWpf = new ListWpf_WinForm(lstStr);
            frmListInWpf.ShowDialog();

            //button2.Text = frmListInWpf.strVal;
        }

        private void DrwFrame()
        {
            // 基準点
            switch (m_strFlip)
            {
                case m_CON_FLIP_S:
                    intReferencePointXDisp = 0;
                    intReferencePointYDisp = 0;
                    break;
                case m_CON_FLIP_X:
                    intReferencePointXDisp = pictureBox5.Width;
                    intReferencePointYDisp = 0;
                    break;
                case m_CON_FLIP_Y:
                    intReferencePointXDisp = pictureBox5.Width;
                    intReferencePointYDisp = pictureBox5.Height;
                    break;
                case m_CON_FLIP_R:
                    intReferencePointXDisp = 0;
                    intReferencePointYDisp = pictureBox5.Height;
                    break;
            }

            // 実際の座標位置を取得
            intPointXFact = 800;
            intPointYFact = 800;

            // 実際の座標とモニタに表示されるコントロールの長さから、比率を算出する
            dblPctWidthRatio = (double)pictureBox5.Width / (double)intPctWidthFact;
            dblPctHeightRatio = (double)pictureBox5.Height / (double)intPctHeightFact;

            // 画面（モニタ）上の座標の算出
            intPctWidthDisp = (short)((double)intPctWidthFact * dblPctWidthRatio);
            intPctHeightDisp = (short)((double)intPctHeightFact * dblPctHeightRatio);
            intPointWidthDisp = (short)((double)intPointWidthFact * dblPctWidthRatio);
            intPointHeightDisp = (short)((double)intPointHeightFact * dblPctHeightRatio);
            intPointXDisp = (short)(((double)intPointXFact * dblPctWidthRatio) - ((double)(intPointWidthDisp / 2)));
            intPointYDisp = (short)(((double)intPointYFact * dblPctHeightRatio) - ((double)(intPointHeightDisp / 2)));

            // 枠線用コントロールの追加
            pictureBox5.Controls.Add(new System.Windows.Forms.PictureBox());

            // 枠線用コントロールの設定変更
            //var pctPointCtrl = (PictureBox)tableLayoutPanel1.Controls[1].Controls[0];
            var pctPointCtrl = (PictureBox)pictureBox5.Controls[0];
            pctPointCtrl.Location = new Point(intPointXDisp, intPointYDisp);
            pctPointCtrl.Size = new Size(intPointWidthDisp, intPointHeightDisp);
            pctPointCtrl.BackColor = Color.Transparent;

            // 赤線を引いたGraphicsオブジェクトを生成
            Bitmap btmObj = new Bitmap(pctPointCtrl.Width, pctPointCtrl.Height);
            Graphics graObj = Graphics.FromImage(btmObj);
            graObj.DrawLine(Pens.Red, 1, 1, pctPointCtrl.Width, 1);                                                 // 上横
            graObj.DrawLine(Pens.Red, 1, 1, 1, pctPointCtrl.Height);                                                // 左縦
            graObj.DrawLine(Pens.Red, pctPointCtrl.Width - 1, 1, pctPointCtrl.Width - 1, pctPointCtrl.Height - 1);  // 右縦
            graObj.DrawLine(Pens.Red, 1, pctPointCtrl.Height - 1, pctPointCtrl.Width - 1, pctPointCtrl.Height - 1); // 下横

            // リソースを解放する
            graObj.Dispose();

            // 赤線を枠線コントロールに表示する
            pctPointCtrl.Image = btmObj;
        }

        private void btnRegMulti_Click(object sender, EventArgs e)
        {
            
        }

        private void btnFlipS_Click(object sender, EventArgs e)
        {
            Bitmap bmpImage = new Bitmap(m_bmpImgInit);
            pictureBox5.Image = bmpImage;
            // ON
            btnFlipS.BackColor = System.Drawing.SystemColors.ActiveCaption;
            // OFF
            btnFlipX.BackColor = SystemColors.Control;
            btnFlipY.BackColor = SystemColors.Control;
            btnFlipR.BackColor = SystemColors.Control;
        }

        private void btnFlipX_Click(object sender, EventArgs e)
        {
            Bitmap bmpImage = new Bitmap(m_bmpImgInit);
            bmpImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
            pictureBox5.Image = bmpImage;
            // ON
            btnFlipX.BackColor = System.Drawing.SystemColors.ActiveCaption;
            // OFF
            btnFlipS.BackColor = SystemColors.Control;
            btnFlipY.BackColor = SystemColors.Control;
            btnFlipR.BackColor = SystemColors.Control;
        }

        private void btnFlipY_Click(object sender, EventArgs e)
        {
            Bitmap bmpImage = new Bitmap(m_bmpImgInit);
            bmpImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
            pictureBox5.Image = bmpImage;
            // ON
            btnFlipY.BackColor = System.Drawing.SystemColors.ActiveCaption;
            // OFF
            btnFlipS.BackColor = SystemColors.Control;
            btnFlipX.BackColor = SystemColors.Control;
            btnFlipR.BackColor = SystemColors.Control;
        }

        private void btnFlipR_Click(object sender, EventArgs e)
        {
            Bitmap bmpImage = new Bitmap(m_bmpImgInit);
            bmpImage.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            pictureBox5.Image = bmpImage;
            // ON
            btnFlipR.BackColor = System.Drawing.SystemColors.ActiveCaption;
            // OFF
            btnFlipS.BackColor = SystemColors.Control;
            btnFlipX.BackColor = SystemColors.Control;
            btnFlipY.BackColor = SystemColors.Control;
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

            //button3.Text = frmListInWpf.strVal;
        }
    }
}
