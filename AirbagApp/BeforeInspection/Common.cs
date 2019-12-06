using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace BeforeInspection
{
    static class Common
    {
        public static bool bolUseKeyboadApp = true;  //true:使用する false:使用しない

        /// <summary>
        /// DB非接続モード
        /// </summary>
        /// <remarks>デバッグ用</remarks>
        public static bool bolModeNonDBCon = true;  //true:DB接続なし false:DB接続あり

        public const string CON_DB_INFO = "Server=192.168.2.17;Port=5432;User ID=postgres;Database=postgres;Password=password;Enlist=true";

        private const byte VK_NUMLOCK = 0x90;
        private const uint KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 0x2;
        private const int KEYEVENTF_KEYDOWN = 0x0;

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern short GetKeyState(int keyCode);
        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // キーボード使用
            if (args.Length >= 1)
            {
                if (int.Parse(args[0]) > 0) { bolUseKeyboadApp = true; } else { bolUseKeyboadApp = false; }
            }
            // DB非接続
            if (args.Length >= 2)
            {
                if (int.Parse(args[1]) > 0) { bolModeNonDBCon = true; } else { bolModeNonDBCon = false; }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        public static string GetRegistryByLocalMachine(string KeyNm, string ValueName)
        {
            try
            {
                RegistryKey rKey = Registry.LocalMachine.OpenSubKey(KeyNm);
                string location = (string)rKey.GetValue(ValueName);
                rKey.Close();
                return location;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return "";
            }
        }

        public static string GetRegistryByCurrentUser(string KeyNm, string ValueName)
        {
            try
            {
                RegistryKey rKey = Registry.CurrentUser.OpenSubKey(KeyNm);
                string location = (string)rKey.GetValue(ValueName);
                rKey.Close();
                return location;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return "";
            }
        }

        public static void SetRegistryByLocalMachine(string KeyNm, string ValueName, string Value)
        {
            try
            {
                RegistryKey rKey = Registry.LocalMachine.CreateSubKey(KeyNm);
                rKey.SetValue(ValueName, Value);
                rKey.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static void SetRegistryByCurrentUser(string KeyNm, string ValueName, object Value)
        {
            try
            {
                RegistryKey rKey = Registry.CurrentUser.CreateSubKey(KeyNm);
                rKey.SetValue(ValueName, Value);
                rKey.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static bool GetNumLock()
        {
            return (((ushort)GetKeyState(0x90)) & 0xffff) != 0;
        }

        public static void SetNumLock(bool bState)
        {
            if (GetNumLock() != bState)
            {
                keybd_event(VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN, 0);
                keybd_event(VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            }
        }
    }

    /// <summary>
    /// マウスやキーボードによるペーストを無効にしたTextBox
    /// </summary>
    public class MyTextBox : TextBox
    {
        public MyTextBox()
            : base()
        {
            //コンテキストメニューを非表示にする
            this.ContextMenu = new ContextMenu();
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //Ctrl+VとShift+Insertを無効にする
            if (((keyData & Keys.Control) == Keys.Control &&
                (keyData & Keys.KeyCode) == Keys.V) ||
                ((keyData & Keys.Shift) == Keys.Shift &&
                (keyData & Keys.KeyCode) == Keys.Insert))
            {
                return true;
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }
    }
}
