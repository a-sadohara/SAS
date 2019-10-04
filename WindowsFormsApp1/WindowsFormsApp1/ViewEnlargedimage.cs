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
    public partial class ViewEnlargedimage : Form
    {
        public ViewEnlargedimage(Image imgTarget)
        {
            InitializeComponent();


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
    }
}
