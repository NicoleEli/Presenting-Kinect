using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Kinetique
{
    class Mouse_Simulator : MouseInput
    {
        protected class MouseDriver2
        {

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern Boolean SetCursorPos(int X, int Y);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
            public struct INPUT
            {
                public int type;
                public MOUSEINPUT mi;
            }
            public struct MOUSEINPUT
            {
                public int dx;
                public int dy;
                public uint mouseData;
                public uint dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            private MOUSEINPUT createMouseInput(int x, int y, uint data, uint t, uint flag)
            {
                MOUSEINPUT mi = new MOUSEINPUT();
                mi.dx = x;
                mi.dy = y;
                mi.mouseData = data;
                mi.time = t;
                //mi.dwFlags = MOUSEEVENTF_ABSOLUTE| MOUSEEVENTF_MOVE;
                mi.dwFlags = flag;
                return mi;
            }

            const uint MOUSEEVENTF_MOVE = 0x0001;
            const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
            const uint MOUSEEVENTF_LEFTUP = 0x0004;
            const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
            const uint MOUSEEVENTF_RIGHTUP = 0x0010;
            const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
            const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
            const uint MOUSEEVENTF_XDOWN = 0x0080;
            const uint MOUSEEVENTF_XUP = 0x0100;
            const uint MOUSEEVENTF_WHEEL = 0x0800;
            const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
            const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
            const int INPUT_MOUSE = 0;

            public void LeftMouseClick(int x, int y)
            {
                MoveCursorToPoint(x, y);
                INPUT[] inp = new INPUT[2];
                inp[0].type = INPUT_MOUSE;
                inp[0].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_LEFTDOWN);
                inp[1].type = INPUT_MOUSE;
                inp[1].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_LEFTUP);
                SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
            }
            public void LeftMouseDown(int x, int y)
            {
                MoveCursorToPoint(x, y);
                INPUT[] inp = new INPUT[1];
                inp[0].type = INPUT_MOUSE;
                inp[0].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_LEFTDOWN);
                SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
            }
            public void LeftMouseUp(int x, int y)
            {
                MoveCursorToPoint(x, y);
                INPUT[] inp = new INPUT[1];
                inp[0].type = INPUT_MOUSE;
                inp[0].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_LEFTUP);
                SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
            }

            public void RightMouseClick(int x, int y)
            {
                MoveCursorToPoint(x, y);
                INPUT[] inp = new INPUT[2];
                inp[0].type = INPUT_MOUSE;
                inp[0].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_RIGHTDOWN);
                inp[1].type = INPUT_MOUSE;
                inp[1].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_RIGHTUP);
                SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
            }
            public void RightMouseDown(int x, int y)
            {
                MoveCursorToPoint(x, y);
                INPUT[] inp = new INPUT[1];
                inp[0].type = INPUT_MOUSE;
                inp[0].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_RIGHTDOWN);
                SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
            }
            public void RightMouseUp(int x, int y)
            {
                MoveCursorToPoint(x, y);
                INPUT[] inp = new INPUT[1];
                inp[0].type = INPUT_MOUSE;
                inp[0].mi = createMouseInput(0, 0, 0, 0, MOUSEEVENTF_RIGHTUP);
                SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
            }

            public void MoveCursorToPoint(int x, int y)
            {
                SetCursorPos(x, y);
            }

        }

        private MouseDriver2 mouse = new MouseDriver2();
        public void MouseDown(float x1, float y1, int id)
        {
            //System.Drawing.Size size = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            int width = (int) System.Windows.SystemParameters.PrimaryScreenWidth;
            int height = (int)System.Windows.SystemParameters.PrimaryScreenHeight;

            int x = (int)(x1 * width);
            int y = (int)(y1 * height);

            mouse.LeftMouseDown(x, y);
        }
        public void MouseUp(float x1, float y1, int id)
        {
            //System.Drawing.Size size = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            int width = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
            int height = (int)System.Windows.SystemParameters.PrimaryScreenHeight;

            int x = (int)(x1 * width);
            int y = (int)(y1 * height);

            mouse.LeftMouseUp(x, y);
        }
        public void MouseUpdate(float x1, float y1, int id)
        {
            //System.Drawing.Size size = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            int width = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
            int height = (int)System.Windows.SystemParameters.PrimaryScreenHeight;

            int x = (int)(x1 * width);
            int y = (int)(y1 * height);

            mouse.MoveCursorToPoint(x, y);
        }

    }
}
