using Linearstar.Windows.RawInput.Native;
using Linearstar.Windows.RawInput;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using FVH.Background.Input;

namespace FVH.Background.Input
{


    public interface IKeysNotificator
    {
        VKeys[] Keys { get; }
    }

    public class DataKeysNotificator : IKeysNotificator
    {
        public DataKeysNotificator(VKeys[] keys) { Keys = keys; }

        public VKeys[] Keys { get; }
    }
    internal class KeyboardHandler : IKeyboardHandler
    {



        public List<VKeys> PressedKeys => _isPressedKeys.ToList();
        private readonly List<VKeys> _isPressedKeys = new List<VKeys>();

        public event EventHandler<IKeysNotificator>? KeyPressEvent;
        public event EventHandler<IKeysNotificator>? KeyUpPressEvent;



        private readonly object _lockObject = new object();     
        public void HandlerKeyboard(RawInputKeyboardData data)
        {

            if (data is not RawInputKeyboardData keyboardData) return; //E1 это правая версия клавиши нажатие event, E0 её отпускание event. но не совсем...

            if (keyboardData.Keyboard.VirutalKey is 255) return; //todo Расмотреть возможность добавление обработки дополнительных клавиш (key 255)

            if (Enum.ToObject(typeof(VKeys), keyboardData.Keyboard.VirutalKey) is not VKeys FlagVkeys) throw new InvalidOperationException($"A virtual key is not an object {nameof(VKeys)}.");

            RawKeyboardFlags chekUPE0 = RawKeyboardFlags.Up | RawKeyboardFlags.KeyE0;

            lock (_lockObject) // контрол + esacpe = ескейп только отжатый
            {
                if (keyboardData.Keyboard.Flags is RawKeyboardFlags.None | keyboardData.Keyboard.Flags is RawKeyboardFlags.KeyE0) // клавиша KeyDown
                {
                    if (_isPressedKeys.Contains(FlagVkeys)) return;
                    _isPressedKeys.Add(FlagVkeys);
                    KeyPressEvent?.Invoke(null, new DataKeysNotificator(_isPressedKeys.ToArray()));
                    return;
                }
                if (keyboardData.Keyboard.Flags is RawKeyboardFlags.Up | keyboardData.Keyboard.Flags == chekUPE0)  // клавиша KeyUp
                {
                    if (_isPressedKeys.Contains(FlagVkeys) is not true) return;
                    _isPressedKeys.Remove(FlagVkeys);
                    KeyUpPressEvent?.Invoke(null, new DataKeysNotificator(PressedKeys.ToArray()));
                    return;
                }
            }
            throw new InvalidOperationException("Key Handler Error");
        }
    }

}
