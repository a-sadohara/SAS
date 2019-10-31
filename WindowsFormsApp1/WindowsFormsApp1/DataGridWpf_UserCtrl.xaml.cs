using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WindowsFormsApp1
{
    /// <summary>
    /// UserControl2.xaml の相互作用ロジック
    /// </summary>
    public partial class DataGridWpf_UserCtrl : UserControl
    {
        public int intRowCount { get; set; }
        public int intColCount { get; set; }
        public ArrayList arrSelectData { get; set; }

        public enum COLUM_TYPE
        {
            USER = 1
        }

        public const string g_CON_USER_COL1_CD = "userno";
        public const string g_CON_USER_COL2_CD = "username";
        public const string g_CON_USER_COL1_NM = "社員番号";
        public const string g_CON_USER_COL2_NM = "作業者名";

        private dynamic m_dynParentForm;
        private System.Windows.Forms.Integration.ElementHost m_dynElementHost;

        public DataGridWpf_UserCtrl(dynamic parParentForm,
                                    System.Windows.Forms.Integration.ElementHost parElementHost,
                                    COLUM_TYPE enmColType,
                                    System.Data.DataTable parData = null)
        {
            InitializeComponent();

            m_dynParentForm = parParentForm;
            m_dynElementHost = parElementHost;

            // 初期化
            intRowCount = 0;
            intColCount = 0;
            int intRowCount_l = 0;
            int intColCount_l = 0;

            // Error回避
            

            // DataGrid調整
            dgData.Height = m_dynElementHost.Height;
            dgData.Width = m_dynElementHost.Width;

            // 列
            switch (enmColType)
            {
                case COLUM_TYPE.USER:
                    dgData.Columns.Add(new DataGridTextColumn() { Header = g_CON_USER_COL1_NM, IsReadOnly = true, FontSize = 16, Binding = new Binding(g_CON_USER_COL1_CD), Width = 100 });
                    dgData.Columns.Add(new DataGridTextColumn() { Header = g_CON_USER_COL2_NM, IsReadOnly = true, FontSize = 16, Binding = new Binding(g_CON_USER_COL2_CD), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                    break;
                default:
                    MessageBox.Show("Columnクラス内に存在するクラスを指定してください");
                    break;
            }
            intColCount_l = dgData.Columns.Count;

            // 行
            if (parData != null)
            {
                foreach (System.Data.DataRow row in parData.Rows)
                {
                    switch (enmColType)
                    {
                        case COLUM_TYPE.USER:
                            dgData.Items.Add(new Column.User
                            {
                                userno = row[0].ToString(),
                                username = row[1].ToString(),
                                useryomigana = row[2].ToString()
                            });
                            break;
                        default:
                            MessageBox.Show("Columnクラス内に存在するクラスを指定してください");
                            break;
                    }

                    intRowCount_l = intRowCount_l + 1;
                }
            }

            // イベントセット
            switch (enmColType)
            {
                case COLUM_TYPE.USER:
                    Style rowStyle = new Style(typeof(DataGridRow));
                    rowStyle.Setters.Add(new EventSetter(DataGridRow.MouseLeftButtonDownEvent,
                                                         new MouseButtonEventHandler(dgData_Click)));
                    dgData.RowStyle = rowStyle;
                    break;
                default:
                    MessageBox.Show("Columnクラス内に存在するクラスを指定してください");
                    break;
            }

            // プロパティセット
            intRowCount = intRowCount_l;
            intColCount = intColCount_l;
        }

        private void dgData_Click(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dtr = null;
            List<string> lststr = new List<string>();

            try
            {
                dtr = sender as DataGridRow;
                for (int c = 0; c <= dgData.Columns.Count - 1; c++)
                {
                    //lststr.Add(dtr);
                }

                DataGridRow row = sender as DataGridRow;
                //arrSelectData = (ArrayList)row;
            }
            catch 
            {
                System.Windows.MessageBox.Show("えら");
            }
            // arrSelectData = (ArrayList)dgData.SelectedCells[e.Y].Item;
        }
    }

    #region DataGrid項目クラス
    public class Column
    {
        public class User
        {
            public string userno { get; set; }
            public string username { get; set; }
            public string useryomigana { get; set; }
        }
    }
    #endregion
}
