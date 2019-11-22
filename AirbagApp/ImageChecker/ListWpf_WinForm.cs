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
    public partial class ListWpf_WinForm : Form
    {
        public string strVal { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parlstStr"></param>
        public ListWpf_WinForm(List<String> parlstStr)
        {
            InitializeComponent();

            elementHost1.Child = new ListWpf_UserCtrl(this, parlstStr);
        }

        private void ListInWpf_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ListInWpf_Load(object sender, EventArgs e)
        {
            int intLocX = (int)((this.ClientSize.Width / 2) - (elementHost1.ClientSize.Width / 2));
            int intLocY = 0;

            elementHost1.SetBounds(intLocX, intLocY, elementHost1.ClientSize.Width, this.ClientSize.Height);
        }
    }

}
