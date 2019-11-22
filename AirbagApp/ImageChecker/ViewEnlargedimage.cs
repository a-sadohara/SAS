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
    public partial class ViewEnlargedimage : Form
    {
        private Image m_ImgInit = null;
        private string m_strImgLocationIni;
        private string m_strImgLocation;

        public ViewEnlargedimage(Image imgTarget, string strImgLocation)
        {
            InitializeComponent();

            m_ImgInit = imgTarget;
            m_strImgLocationIni = strImgLocation;
            m_strImgLocation = strImgLocation;

            //現在フォームが存在しているディスプレイを取得
            System.Windows.Forms.Screen s =
                System.Windows.Forms.Screen.FromControl(this);
            //ディスプレイの高さと幅を取得
            int h = s.Bounds.Height;
            int w = s.Bounds.Width;
            // ディスプレイサイズを指定
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Width = (int)(w * 0.7);
            this.Height = (int)(h * 0.7);

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            // 画像を設定する
            pictureBox1.Image = imgTarget;

        }

        private void PictureBox1_DoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox1_Click(dynamic sender, EventArgs e)
        {
            if (m_strImgLocation.IndexOf("1_04_01_S334_T1") < 0)
            {
                //現在フォームが存在しているディスプレイを取得
                System.Windows.Forms.Screen s =
                    System.Windows.Forms.Screen.FromControl(this);
                //ディスプレイの高さと幅を取得
                int h = s.Bounds.Height;
                int w = s.Bounds.Width;
                // ディスプレイサイズを指定
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.Width = (int)(w * 0.7);
                this.Height = (int)(h * 0.7);

                // フォームの表示位置調整
                this.StartPosition = FormStartPosition.CenterParent;

                pictureBox1.Image = System.Drawing.Image.FromFile(@".\Image\1_04_01_S334_T1.jpg");
                m_strImgLocation = @".\Image\1_04_01_S334_T1.jpg";
            }
            else
            {
                //現在フォームが存在しているディスプレイを取得
                System.Windows.Forms.Screen s =
                    System.Windows.Forms.Screen.FromControl(this);
                //ディスプレイの高さと幅を取得
                int h = s.Bounds.Height;
                int w = s.Bounds.Width;
                // ディスプレイサイズを指定
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.Width = (int)(w * 0.7);
                this.Height = (int)(h * 0.7);

                // フォームの表示位置調整
                this.StartPosition = FormStartPosition.CenterParent;

                // 画像を設定する
                pictureBox1.Image = m_ImgInit;
                m_strImgLocation = m_strImgLocationIni;
            }
        }
    }
}
