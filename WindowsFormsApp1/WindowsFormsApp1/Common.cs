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
            Application.Run(new LoginForm());
        }

        /// <summary>
        /// ログイン処理
        /// </summary>
        public static void LogIn()
        {
            g_parStatus = 1;
        }

        /// <summary>
        /// ログアウト処理
        /// </summary>
        public static void LogOut()
        {
            g_parStatus = 0;

            // 開いている画面ループ
            foreach (Form frm in Application.OpenForms)
            {
                if (frm is LoginForm)
                {
                    // ログイン画面　⇒　再表示
                    frm.Visible = true;
                }
                else
                {
                    // ログイン画面以外　⇒　閉じる
                    frm.Close();
                }
            }
        }

    }
}
