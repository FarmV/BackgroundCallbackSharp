using Linearstar.Windows.RawInput.Native;
using Linearstar.Windows.RawInput;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FVH.BackgroundInput
{
    internal class BaseKeybordInput
    {
        internal class DataKeysNotificator
        {
            internal DataKeysNotificator(VKeys[] keys)
            {
                Keys = keys;
            }

            internal VKeys[] Keys
            {
                get;
            }
        }
        internal class KeyBordBaseRawInput
        {
            internal static List<VKeys> IsPressedKeys => _isPressedKeys.ToList();

            internal static event EventHandler<DataKeysNotificator>? KeyPressEvent;

            private static readonly List<VKeys> _isPressedKeys = new List<VKeys>();

     
            private static readonly object _lockObject = new object();
            internal void RawInputHandler(RawInputData data)
            {
                if (data is not RawInputKeyboardData keyboardData) return;

                if (keyboardData.Keyboard.VirutalKey is 255) return; //todo Расмотреть возможность добавление обработки дополнительных клавиш (key 255)

                if (Enum.ToObject(typeof(VKeys), keyboardData.Keyboard.VirutalKey) is not VKeys FlagVkeys) throw new InvalidOperationException($"A virtual key is not an object {nameof(VKeys)}.");

                RawKeyboardFlags chekUPE0 = RawKeyboardFlags.Up | RawKeyboardFlags.KeyE0;

                lock (_lockObject)
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
                        //ChangeTheKeyPressure?.Invoke(null, new DataKeysNotificator(IsPressedKeys.ToArray()));
                        return;
                    }
                }
                throw new InvalidOperationException("Key Handler Error");
            }


        }
    }
}
