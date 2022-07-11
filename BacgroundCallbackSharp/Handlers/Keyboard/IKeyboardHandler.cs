using FVH.Background.Input;
using Linearstar.Windows.RawInput;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVH.Background.Input
{
    public interface IKeyboardHandler
    {
        public List<VKeys> PressedKeys { get; }
        public event EventHandler<IKeysNotificator>? KeyPressEvent;
        public event EventHandler<IKeysNotificator>? KeyUpPressEvent;
        internal void HandlerKeyboard(RawInputKeyboardData data);
    }
}
