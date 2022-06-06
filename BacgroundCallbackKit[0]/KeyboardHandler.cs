using Linearstar.Windows.RawInput.Native;
using Linearstar.Windows.RawInput;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FVH.Background.InputHandler;
using System.Runtime.InteropServices;

namespace FVH.Background.Input
{
    public class DataKeysNotificator 
    {
        public DataKeysNotificator(VKeys[] keys)
        {
            Keys = keys;
        }

        public VKeys[] Keys
        {
            get;
        }
    }
    internal class KeyboardHandler : IHandler
    {

        //#region WinAPI
        //const int MAPVK_VK_TO_VSC_EX = 4;

        //[DllImport("user32.dll")]
        //private static extern int MapVirtualKey(int uCode, int uMapType);
        //#endregion
        public List<VKeys> IsPressedKeys => _isPressedKeys.ToList();

        public event EventHandler<DataKeysNotificator>? KeyPressEvent;
        public event EventHandler<DataKeysNotificator>? KeyUpPressEvent;

        private readonly List<VKeys> _isPressedKeys = new List<VKeys>();


        private readonly object _lockObject = new object();
      

        List<(VKeys, RawKeyboardFlags)> test = new List<(VKeys, RawKeyboardFlags)>();
        public void Handler(RawInputData data)
        {
            if (data is not RawInputKeyboardData keyboardData) return; //E1 это правая версия клавиши нажатие event, E0 её отпускание event

            if (keyboardData.Keyboard.VirutalKey is 255) return; //todo Расмотреть возможность добавление обработки дополнительных клавиш (key 255)

            if (Enum.ToObject(typeof(VKeys), keyboardData.Keyboard.VirutalKey) is not VKeys FlagVkeys) throw new InvalidOperationException($"A virtual key is not an object {nameof(VKeys)}.");

            //var VK_LCONTROL = 162;     /*Left CONTROL key*/
            //var VK_RCONTROL = 163;

            //var r1 = 42;
            //var r2 = 54;
            //if (keyboardData.Keyboard.Flags is RawKeyboardFlags.KeyE0 || keyboardData.Keyboard.Flags is RawKeyboardFlags.KeyE1)
            //{
            //    test.Add((FlagVkeys, keyboardData.Keyboard.Flags));
            //    var r = MapVirtualKey(17, 4);
            //}

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
                    KeyUpPressEvent?.Invoke(null, new DataKeysNotificator(IsPressedKeys.ToArray()));
                    return;
                }
            }
            throw new InvalidOperationException("Key Handler Error");
        }


    }

}
