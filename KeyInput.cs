using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WindowsInput;


namespace Recognition
{
    public interface GestureEvent { }

    public class LeftGesture : GestureEvent { }

    public class RightGesture : GestureEvent { }

    public class UpGesture : GestureEvent { }

    public class DownGesture : GestureEvent { }

    public class AGesture : GestureEvent { }
}

namespace AppSpecific
{
    using Recognition;

    class Testing
    {
        static void Main(string[] args)
        {
            GestureListener listener = new DefaultListener();

            for (int i = 0; i < 5; i++)
            {
                listener.gestureOccurs(new AGesture());
            }


        }
    }

    public interface GestureListener
    {
        void gestureOccurs(GestureEvent e);
    }

    public class DefaultListener : GestureListener
    {
        private static Dictionary<Type, WindowsInput.VirtualKeyCode> lookup;
        private static LeftGesture LEFT = new LeftGesture();
        private static RightGesture RIGHT = new RightGesture();
        private static UpGesture UP = new UpGesture();
        private static AGesture A = new AGesture();

        static DefaultListener()
        {
            lookup = new Dictionary<Type, WindowsInput.VirtualKeyCode>();
            lookup.Add(LEFT.GetType(), VirtualKeyCode.LEFT);
            lookup.Add(RIGHT.GetType(), VirtualKeyCode.RIGHT);
            lookup.Add(UP.GetType(), VirtualKeyCode.UP);
            lookup.Add(A.GetType(), VirtualKeyCode.VK_A);
        }

        public void gestureOccurs(GestureEvent e)
        {
            if (lookup.ContainsKey(e.GetType()))
            {
                WindowsInput.VirtualKeyCode ekey = lookup[e.GetType()];
                InputSimulator.SimulateKeyPress(ekey);
            }

        }
    }
}
