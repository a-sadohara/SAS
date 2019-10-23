using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Common
    {
        public const string CON_DB_INFO = "Server=192.168.2.17;Port=5432;User ID=postgres;Database=postgres;Password=password;Enlist=true";

        public static String g_parUserNo = "";
        public static String g_parUserNm = "";
        public static short g_parDispNum;
        public static short g_parStatus;  //0:ログアウト 1:ログイン

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TargetSelection());
        }

        public static void LogIn()
        {
            g_parStatus = 1;
        }

        public static void LogOut()
        {
            g_parStatus = 0;

            foreach (Form frm in Application.OpenForms)
            {
                if (frm is LoginForm)
                {
                    frm.Visible = true;
                }
                else
                {
                    frm.Close();
                }
            }
        }

    }
}
