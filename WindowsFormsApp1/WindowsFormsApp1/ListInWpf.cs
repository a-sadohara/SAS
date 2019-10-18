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
    public partial class ListInWpf : Form
    {
        //WPFコントロールをホストするElementHostコントロール
        private System.Windows.Forms.Integration.ElementHost elementHost1;

        private List<String> lstStr;

        public ListInWpf(List<String> parlstStr)
        {
            InitializeComponent();

            lstStr = parlstStr;
        }

        private void ListInWpf_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ListInWpf_Load(object sender, EventArgs e)
        {
            // DataGridバインド
            System.Windows.Controls.DataGrid wpfDataGrid = new System.Windows.Controls.DataGrid();

            // 列定義
            System.Windows.Controls.DataGridTextColumn textColumn = new System.Windows.Controls.DataGridTextColumn();
            textColumn.Header = "Val";
            textColumn.Binding = new System.Windows.Data.Binding("Val");
            wpfDataGrid.Columns.Add(textColumn);
            wpfDataGrid.MinRowHeight = 30;
            wpfDataGrid.MinColumnWidth = pnlMain.ClientSize.Width;
            wpfDataGrid.Width = pnlMain.ClientSize.Width;

            for (int idx = 0; idx <= lstStr.Count - 1; idx++)
            {
                // 行追加
                wpfDataGrid.Items.Add(new Item(lstStr[idx]));
            }

            wpfDataGrid.FontSize = 13;
            //wpfDataGrid.FontFamily = MaterialDesignFont;
            //    = System.Windows.HorizontalAlignment.Center;
            wpfDataGrid.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;

            //ElementHostコントロールを作成する
            elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            //コントロールの位置と大きさを設定する
            elementHost1.SetBounds(0, 0, pnlMain.Width, this.ClientSize.Height);

            //ElementHostのChildプロパティにWPFコントロールを設定する
            elementHost1.Child = wpfDataGrid;

            pnlMain.Controls.Add(elementHost1);
        }
    }

    public class Item
    {
        public string Val { get; set; }

        public Item(string parVal)
        {
            Val = parVal;
        }
    }
}
