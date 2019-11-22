using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageChecker
{
    static class Common
    {
        /// <summary>
        /// DB非接続モード
        /// </summary>
        /// <remarks>デバッグ用</remarks>
        public static bool bolModeNonDBCon = true;  //true:DB接続なし false:DB接続あり

        public const string CON_DB_INFO = "Server=192.168.2.17;Port=5432;User ID=postgres;Database=postgres;Password=password;Enlist=true";

        public static String g_parUserNo = "";
        public static String g_parUserNm = "";
        public static short g_parDispNum;
        public static short g_parStatus;  //0:ログアウト 1:ログイン

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string strUserNo = "";
            short shoDispNum = 0;

            // DB非接続
            if (args.Length >= 1)
            {
                if (int.Parse(args[0]) > 0) { bolModeNonDBCon = true; } else { bolModeNonDBCon = false; }
            }
            // 職員番号
            if (args.Length >= 2) strUserNo = args[1];
            // 表示数
            if (args.Length >= 3) shoDispNum = (short)int.Parse(args[2]);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm(strUserNo, shoDispNum));
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
