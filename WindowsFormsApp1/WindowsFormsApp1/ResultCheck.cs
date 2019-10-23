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
        private Int16 intPctWidthFact = 1000;
        private Int16 intPctHeightFact = 1000;
        private Int16 intPointWidthFact = 100;
        private Int16 intPointHeightFact = 100;
        private Int16 intPointXFact = 0;    // 取得
        private Int16 intPointYFact = 0;    // 取得

        // 画面（モニタ）上の座標
        private Int16 intPctWidthDisp = 0;
        private Int16 intPctHeightDisp = 0;
        private Int16 intPointWidthDisp = 0;
        private Int16 intPointHeightDisp = 0;
        private Int16 intPointXDisp = 0;
        private Int16 intPointYDisp = 0;

        // 比率
        private Double dblPctWidthRatio = 0.0;
        private Double dblPctHeightRatio = 0.0;

        public ResultCheck(TargetInfoDto objTargetInfo,int intRowIndex)
        {
            InitializeComponent();

            objTargetInfoDto = objTargetInfo;
            intRow = intRowIndex;
            
            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            comboBox1.SelectedIndex = 33;
            comboBox2.SelectedIndex = 2;
        }


        private void Button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnTanpatu_Click(object sender, EventArgs e)
        {
            btnTanpatu.Text = "■結節有（単発）";
            btnRenzoku.Text = "□結節有（連続）";
            btnOther.Text = "□その他";
            btnJudgeNG.Text = "□他画像でＮＧ判定済み";
            button6.Enabled = true;
            lblReason.Text = btnTanpatu.Text;
        }

        private void BtnRenzoku_Click(object sender, EventArgs e)
        {
            btnTanpatu.Text = "□結節有（単発）";
            btnRenzoku.Text = "■結節有（連続）";
            btnOther.Text = "□その他";
            btnJudgeNG.Text = "□他画像でＮＧ判定済み";
            button6.Enabled = true;
            lblReason.Text = btnRenzoku.Text;
        }

        private void BtnOther_Click(object sender, EventArgs e)
        {
            btnTanpatu.Text = "□結節有（単発）";
            btnRenzoku.Text = "□結節有（連続）";
            btnOther.Text = "■その他";
            btnJudgeNG.Text = "□他画像でＮＧ判定済み";
            button6.Enabled = true;

            ErrorReasonDTO errorReasonDto = new ErrorReasonDTO();
            SelectErrorReason frmErrorReason = new SelectErrorReason(errorReasonDto);
            frmErrorReason.ShowDialog(this);

            lblReason.Text = string.Format("{0}({1})", btnOther.Text,errorReasonDto.getStrErrorReason());
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
            tableLayoutPanel1.Controls[1].Controls.Add(new System.Windows.Forms.PictureBox());

            // 枠線用コントロールの設定変更
            var pctPointCtrl = (PictureBox)tableLayoutPanel1.Controls[1].Controls[0];
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

        private void Button6_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Result frmResult = new Result(objTargetInfoDto,intRow);
            frmResult.ShowDialog(this);
            this.Close();
        }

        private void btnJudgeNG_Click(object sender, EventArgs e)
        {
            btnTanpatu.Text = "□結節有（単発）";
            btnRenzoku.Text = "□結節有（連続）";
            btnOther.Text = "□その他";
            btnJudgeNG.Text = "■他画像でＮＧ判定済み";
            button6.Enabled = true;
            lblReason.Text = btnJudgeNG.Text;
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

            ListInWpf frmListInWpf = new ListInWpf(lstStr);
            frmListInWpf.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
