using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetique
{
    public interface MouseInput
    {
        void MouseDown(float x, float y, int id);
        void MouseUpdate(float x, float y, int id);
        void MouseUp(float x, float y, int id);
    }
}
