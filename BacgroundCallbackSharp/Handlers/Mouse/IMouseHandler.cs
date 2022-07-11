using Linearstar.Windows.RawInput;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVH.Background.Input
{

    public interface IMouseHandler
    {
        public event EventHandler<RawInputMouseData>? MouseEvent;
        public void HandlerMouse(RawInputMouseData data);
    }

}
