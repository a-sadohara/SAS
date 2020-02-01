using System;
using System.Windows.Forms;

namespace BeforeInspection
{
    class MyDataGridView : DataGridView
    {
        private const int WM_VSCROLL = 0x0115;
        private const int SB_THUMBPOSITION = 0x0004;
        private const int SB_THUMBTRACK = 0x0005;
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        // メッセージを処理します。
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_VSCROLL)
            {
                if (LoWord((long)m.WParam) == LoWord((long)SB_THUMBPOSITION))
                {
                    BeginInvoke((Action<IntPtr, IntPtr>)((WParam, LParam) =>
                    {
                        // SB_THUMBPOSITION を SB_THUMBTRACK に変更します。
                        IntPtr testWParam = new IntPtr(SB_THUMBTRACK);
                        // WM_VSCROLL メッセージを再送します。
                        PostMessage(this.Handle, WM_VSCROLL, testWParam, LParam);
                    }), m.WParam, m.LParam);
                }
            }
        }

        protected short LoWord(long input)
        {
            return (short)((int)input & 0xFFFF);
        }
    }
}
