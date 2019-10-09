using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Common
    {
        public static String parUserNo;
        public static String parUserNm;
        public static short parDispNum;
        public static short parStatus;  //0:ログアウト 1:ログイン

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }

        public static void LogIn()
        {
            parStatus = 1;
        }

        public static void LogOut()
        {
            parStatus = 0;

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
