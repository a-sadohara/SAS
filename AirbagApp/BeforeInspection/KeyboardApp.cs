using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static BeforeInspection.Common;

namespace BeforeInspection
{
    public static class KeyboardApp
    {
        public static int intProsessId { get; set; }

        //参考文献
        //Unable to launch onscreen keyboard (osk.exe) from a 32-bit process on Win7 x64
        //http://stackoverflow.com/questions/2929255/unable-to-launch-onscreen-keyboard-osk-exe-from-a-32-bit-process-on-win7-x64

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        private const UInt32 WM_SYSCOMMAND = 0x112;
        private const UInt32 SC_RESTORE = 0xf120;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //private static extern int MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, int bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        private const string OnScreenKeyboadApplication = "osk.exe";

        static KeyboardApp()
        {
            intProsessId = -1;
        }

        public static void OnApp(int x, int y, int nWidth, int nHeight)
        {
            // Get the name of the On screen keyboard
            string processName = System.IO.Path.GetFileNameWithoutExtension(OnScreenKeyboadApplication);

            // Check whether the application is not running 
            var query = from process in Process.GetProcesses()
                        where process.ProcessName == processName
                        select process;

            var keyboardProcess = query.FirstOrDefault();

            // launch it if it doesn’t exist
            if (keyboardProcess == null)
            {
                IntPtr ptr = new IntPtr(); ;
                bool sucessfullyDisabledWow64Redirect = false;

                // Disable x64 directory virtualization if we’re on x64,
                // otherwise keyboard launch will fail.
                if (System.Environment.Is64BitOperatingSystem)
                {
                    sucessfullyDisabledWow64Redirect = Wow64DisableWow64FsRedirection(ref ptr);
                }

                // osk.exe is in windows/system folder. So we can directky call it without path
                using (Process osk = new Process())
                {
                    osk.StartInfo.FileName = OnScreenKeyboadApplication;
                    osk.Start();
                    intProsessId = osk.Id;

                    osk.WaitForInputIdle();//起動してすぐはWaitForInputIdleで異常が発生するため、コメントアウト (異常内容:WaitForInputIdle に失敗しました。プロセスがグラフィック インターフェイスを含んでいない可能性があります。)

                    // Re-enable directory virtualisation if it was disabled.
                    if (System.Environment.Is64BitOperatingSystem)
                        if (sucessfullyDisabledWow64Redirect)
                            Wow64RevertWow64FsRedirection(ptr);

                    //var startTime = DateTime.Now;
                    //var timeSpan = new TimeSpan(0, 0, 10);

                    //while (0 >= (int)osk.MainWindowHandle)
                    //{
                    //    if (IsTimeout(startTime, timeSpan))
                    //    {
                    //        Console.WriteLine("Timeout");
                    //        Environment.Exit(0);
                    //    }

                    //    System.Threading.Thread.Sleep(1000);
                    //    Console.WriteLine("...");
                    //    osk.Refresh();
                    //}
                }
            }
            else
            {
                intProsessId = keyboardProcess.Id;

                // Bring keyboard to the front if it’s already running
                var windowHandle = keyboardProcess.MainWindowHandle;
                SendMessage(windowHandle, WM_SYSCOMMAND, new IntPtr(SC_RESTORE), new IntPtr(0));

                keyboardProcess.WaitForInputIdle();    
            }

            if (IsOpen())
            {
                DispSetting(x, y, nWidth, nHeight);
            }
        }

        public static bool IsOpen()
        {
            try
            {
                Process procs = Process.GetProcessById(intProsessId);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public static void KillApp()
        {
            try
            {
                Process procs = Process.GetProcessById(intProsessId);
                procs.Kill();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message.ToString());
            }
        }

        public static void DispSetting(int parX, int parY, int parWidth, int parHeight)
        {
            string strRegistryKey = @"Software\Microsoft\Osk";
            // レジストリを設定して画面の表示位置位置・サイズを変更する
            SetRegistryByCurrentUser(strRegistryKey, "WindowLeft", parX);
            SetRegistryByCurrentUser(strRegistryKey, "WindowTop", parY);
            SetRegistryByCurrentUser(strRegistryKey, "WindowWidth", parWidth);
            SetRegistryByCurrentUser(strRegistryKey, "WindowHeight", parHeight);
            SetRegistryByCurrentUser(strRegistryKey, "ShowNumPad", 1);
        }
    }
}
