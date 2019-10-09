using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using WindowsFormsApp1.DTO;
using static WindowsFormsApp1.Common;

namespace WindowsFormsApp1
{
    public partial class Overdetectionexclusion : Form
    {

        private readonly SemaphoreSlim _clickSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _doubleClickSemaphore = new SemaphoreSlim(0);

        TargetInfoDto objTargetInfoDto;
        int intRow;

        public Overdetectionexclusion(TargetInfoDto objTartInfo,int intRowIndex)
        {
            InitializeComponent();

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            objTargetInfoDto = objTartInfo;
            intRow = intRowIndex;

            // パラメータ：表示数から表示数を調整
            switch (Common.parDispNum)
            {
                case 2:
                    tableLayoutPanel1.Controls.RemoveAt(6);
                    tableLayoutPanel1.Controls.RemoveAt(5);
                    tableLayoutPanel1.Controls.RemoveAt(4);
                    tableLayoutPanel1.Controls.RemoveAt(3);
                    tableLayoutPanel1.Controls.RemoveAt(2);
                    tableLayoutPanel1.Controls.RemoveAt(1);
                    tableLayoutPanel1.Controls.RemoveAt(0);

                    //tableLayoutPanel1.RowCount = 1;
                    //tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
                    //tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
                    //tableLayoutPanel1.RowCount = 2;

                    tableLayoutPanel1.RowStyles[0] = new RowStyle(SizeType.Percent, 50F);
                    tableLayoutPanel1.RowStyles[1] = new RowStyle(SizeType.Percent, 0F);
                    tableLayoutPanel1.RowStyles[2] = new RowStyle(SizeType.Percent, 0F);
                    tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50F);
                    tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50F);
                    tableLayoutPanel1.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, 0F);

                    break;
                case 4:
                    tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;

                    tableLayoutPanel1.Controls.RemoveAt(6);
                    tableLayoutPanel1.Controls.RemoveAt(3);
                    tableLayoutPanel1.Controls.RemoveAt(2);
                    tableLayoutPanel1.Controls.RemoveAt(1);
                    tableLayoutPanel1.Controls.RemoveAt(0);

                    tableLayoutPanel1.RowStyles[0] = new RowStyle(SizeType.Percent, 50F);
                    tableLayoutPanel1.RowStyles[1] = new RowStyle(SizeType.Percent, 50F);
                    tableLayoutPanel1.RowStyles[2] = new RowStyle(SizeType.Percent, 0F);
                    tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50F);
                    tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50F);
                    tableLayoutPanel1.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, 0F);

                    break;
                case 9:
                    break;
            }



        }

        private async void PictureBox1_Click_1(object sender, EventArgs e)
        {
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }

            changePanelState(pictureBox1);

        }

        private void PictureBox1_DoubleClick_1(object sender, EventArgs e)
        {
            _doubleClickSemaphore.Release();

            viewImage(pictureBox1);

        }

        private async void PictureBox8_Click(object sender, EventArgs e)
        {
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }

            changePanelState(pictureBox8);

        }

        private void PictureBox8_DoubleClick(object sender, EventArgs e)
        {
            _doubleClickSemaphore.Release();

            viewImage(pictureBox8);

        }

        private async void PictureBox5_Click(object sender, EventArgs e)
        {
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }

            changePanelState(pictureBox5);


        }

        private void PictureBox5_DoubleClick(object sender, EventArgs e)
        {
            _doubleClickSemaphore.Release();

            viewImage(pictureBox5);

        }


        private async void PictureBox9_Click(object sender, EventArgs e)
        {
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }

            changePanelState(pictureBox9);


        }

        private void PictureBox9_DoubleClick(object sender, EventArgs e)
        {
            _doubleClickSemaphore.Release();

            viewImage(pictureBox9);

        }

        private async void PictureBox12_Click(object sender, EventArgs e)
        {
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }

            changePanelState(pictureBox12);


        }

        private void PictureBox12_DoubleClick(object sender, EventArgs e)
        {
            _doubleClickSemaphore.Release();

            viewImage(pictureBox12);

        }

        private async void PictureBox11_Click(object sender, EventArgs e)
        {
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }

            changePanelState(pictureBox11);

        }

        private void PictureBox11_DoubleClick(object sender, EventArgs e)
        {
            _doubleClickSemaphore.Release();

            viewImage(pictureBox11);

        }

        private async void PictureBox6_Click(object sender, EventArgs e)
        {
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }

            changePanelState(pictureBox6);

        }

        private void PictureBox6_DoubleClick(object sender, EventArgs e)
        {
            _doubleClickSemaphore.Release();

            viewImage(pictureBox6);

        }


        private async void PictureBox7_Click(object sender, EventArgs e)
        {
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }

            changePanelState(pictureBox7);
            
        }

        private void PictureBox7_DoubleClick(object sender, EventArgs e)
        {
            _doubleClickSemaphore.Release();

            viewImage(pictureBox7);

        }


        private void Button6_Click(object sender, EventArgs e)
        {

            DataTable dtTargetInfo = objTargetInfoDto.getTargetInfoDTO();
            dtTargetInfo.Rows[intRow]["Status"] = "1";
            dtTargetInfo.Rows[intRow]["Process"] = "合否確認・判定登録";

            objTargetInfoDto.setTargetInfoDTO(dtTargetInfo);

            this.Close();
        }

        private async void PictureBox4_Click(object sender, EventArgs e)
        {
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }

            changePanelState(pictureBox4);

        }

        private void PictureBox4_DoubleClick(object sender, EventArgs e)
        {
            _doubleClickSemaphore.Release();

            viewImage(pictureBox4);
        }


        private void Button1_Click(object sender, EventArgs e)
        {

            DataTable dtTargetInfo = objTargetInfoDto.getTargetInfoDTO();
            dtTargetInfo.Rows[intRow]["Status"] = "3";

            objTargetInfoDto.setTargetInfoDTO(dtTargetInfo);
            this.Close();
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void changePanelState(PictureBox picBox)
        {
            Graphics g = Graphics.FromImage(picBox.Image);

            if (picBox.BorderStyle == BorderStyle.Fixed3D)
            {
                picBox.BorderStyle = BorderStyle.None;
                picBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(67)))), ((int)(((byte)(106))))); 

                picBox.Image = System.Drawing.Image.FromFile(picBox.ImageLocation);

                return;
            }

            picBox.BorderStyle = BorderStyle.Fixed3D;
            picBox.BackColor = System.Drawing.Color.DodgerBlue;

            g.DrawString("NG", new Font("Arial", 200), Brushes.DodgerBlue, new Point((int)(picBox.Width * 0.2), (int)(picBox.Height * 0.4)));
                      
        }

        private void viewImage(PictureBox picBox)
        {
            ViewEnlargedimage frmViewImage = new ViewEnlargedimage(System.Drawing.Image.FromFile(picBox.ImageLocation));
            frmViewImage.ShowDialog(this);
        }


        private void Overdetectionexclusion_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            lblUser.Text = "作業者名：" + Common.parUserNm;

            List<PictureBox> lstPicBox = new List<PictureBox>();
            lstPicBox.Add(pictureBox1);
            lstPicBox.Add(pictureBox11);
            lstPicBox.Add(pictureBox12);
            lstPicBox.Add(pictureBox4);
            lstPicBox.Add(pictureBox5);
            lstPicBox.Add(pictureBox6);
            lstPicBox.Add(pictureBox7);
            lstPicBox.Add(pictureBox8);
            lstPicBox.Add(pictureBox9);

            int intCnt = 0;
            foreach(string strFilePath in System.IO.Directory.GetFiles(@".\Image", "*", System.IO.SearchOption.AllDirectories))
            {
                lstPicBox[intCnt].Image = System.Drawing.Image.FromFile(strFilePath);
                lstPicBox[intCnt].ImageLocation = strFilePath;

                intCnt++;
            }

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogOut();
        }
    }
}
