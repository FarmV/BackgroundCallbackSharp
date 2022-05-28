//using Linearstar.Windows.RawInput.Native;
//using Linearstar.Windows.RawInput;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;



//namespace FVH.BackgroundInput
//{
//    internal class BaseKeybordInput
//    {
//        internal class DataKeysNotificator
//        {
//            internal DataKeysNotificator(VKeys[] keys)
//            {
//                Keys = keys;
//            }

//            internal VKeys[] Keys
//            {
//                get;
//            }
//        }
//        internal class KeyBordBaseRawInput
//        {
//            internal static List<VKeys> PresKeys => new List<VKeys>(IsPressedKeys);

//            internal static event EventHandler<DataKeysNotificator>? ChangeTheKeyPressure;

//            private static readonly List<VKeys> IsPressedKeys = IsPressedKeys = new List<VKeys>();

//            private readonly static KeyBordBaseRawInput Instance = new KeyBordBaseRawInput();

//            private KeyBordBaseRawInput()
//            {
//                App.Input += RawInputHandler;
//            }

//            internal static KeyBordBaseRawInput GetInstance() => Instance;


//            internal enum RawInputHandlerFail
//            {
//                None,
//                VirutalKeyNonVKeys
//            }

//            private static readonly object _lockMedthod = new object();
//            private static void RawInputHandler(object? sender, RawInputEvent e) => Task.Run(() =>
//            {
//                if (e.Data is not RawInputKeyboardData keyboardData) return;

//                if (keyboardData.Keyboard.VirutalKey is 255) return; //todo Расмотреть возможность добавление дополнитльных функций клваитуры (key 255)

//                if (Enum.ToObject(typeof(VKeys), keyboardData.Keyboard.VirutalKey) is not VKeys FlagVkeys) throw new InvalidOperationException($"Виртуальный ключ не явлется объектом {nameof(VKeys)}.");

//                RawKeyboardFlags chekUPE0 = RawKeyboardFlags.Up | RawKeyboardFlags.KeyE0;

//                lock (_lockMedthod)
//                {
//                    if (keyboardData.Keyboard.Flags is RawKeyboardFlags.None | keyboardData.Keyboard.Flags is RawKeyboardFlags.KeyE0) // клавиша KeyDown
//                    {
//                        if (IsPressedKeys.Contains(FlagVkeys)) return;
//                        IsPressedKeys.Add(FlagVkeys);
//                        ChangeTheKeyPressure?.Invoke(null, new DataKeysNotificator(IsPressedKeys.ToArray()));
//                        return;
//                    }
//                    if (keyboardData.Keyboard.Flags is RawKeyboardFlags.Up | keyboardData.Keyboard.Flags == chekUPE0)  // клавиша KeyUp
//                    {
//                        if (IsPressedKeys.Contains(FlagVkeys) is not true) return;
//                        IsPressedKeys.Remove(FlagVkeys);
//                        //ChangeTheKeyPressure?.Invoke(null, new DataKeysNotificator(IsPressedKeys.ToArray()));
//                        return;
//                    }
//                }
//            });


//        }
//    }
//}
