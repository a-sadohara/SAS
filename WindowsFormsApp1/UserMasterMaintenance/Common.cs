using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserMasterMaintenance
{
    static class Common
    {
        public const int CON_EDITMODE_REG = 1;
        public const int CON_EDITMODE_UPD = 2;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UserMasterMaintenance());
        }
    }
}
