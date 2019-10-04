using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.DTO;


namespace WindowsFormsApp1
{
    public partial class ResultCheck : Form
    {

        TargetInfoDto objTargetInfoDto;
        int intRow;

        public ResultCheck(TargetInfoDto objTargetInfo,int intRowIndex)
        {
            InitializeComponent();

            objTargetInfoDto = objTargetInfo;
            intRow = intRowIndex;
            
            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

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
    }
}
