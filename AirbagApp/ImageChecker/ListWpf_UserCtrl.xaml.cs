using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageChecker
{
    /// <summary>
    /// ListWpf_UserCtrl.xaml の相互作用ロジック
    /// </summary>
    public partial class ListWpf_UserCtrl : System.Windows.Controls.UserControl
    {
        private dynamic m_frmParent;
        private List<String> m_lstStr;

        public string strVal { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parlstStr"></param>
        public ListWpf_UserCtrl(dynamic parParent, List<String> parlstStr)
        {
            InitializeComponent();

            m_frmParent = parParent;
            m_lstStr = parlstStr;

            // 行追加
            for (int idx = 0; idx <= m_lstStr.Count - 1; idx++)
            {
                dgData.Items.Add(new Item(m_lstStr[idx]));
            }
        }

        private void dgData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            strVal = "";

            var elem = e.MouseDevice.DirectlyOver as FrameworkElement;
            if (elem != null)
            {
                dynamic cell = elem.Parent as DataGridCell;
                if (cell == null)
                {
                    // ParentでDataGridCellが拾えなかった時はTemplatedParentを参照
                    // （Borderをダブルクリックした時）
                    cell = elem.TemplatedParent as DataGridCell;
                }
                if (cell != null)
                {
                    // ここでcellの内容を処理
                    // （cell.DataContextにバインドされたものが入っているかと思います）
                    m_frmParent.strVal = cell.Content.Text.ToString();
                    m_frmParent.Close();
                }
            }
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
