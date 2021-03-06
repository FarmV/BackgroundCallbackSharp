using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Windows.Media.TextFormatting;

namespace FVH.Background.Input
{
    public class LowLevlHook : CriticalFinalizerObject, IDisposable
    {
        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed is true) return;
            UninstallHook();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private delegate IntPtr KeyboardHookHandler(int nCode, WMEvent wParam, TagKBDLLHOOKSTRUCT lParam);
        private KeyboardHookHandler? hookHandler;

        private IntPtr hookID = IntPtr.Zero;

        public void InstallHook()
        {
            CheckDisposed();
            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
        }

        ~LowLevlHook()
        {
            try
            {
                if (_disposed is true) return;
                UnhookWindowsHookEx(hookID);
                Marshal.FreeHGlobal(hookID);
            }
            catch { }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed() { if (_disposed is true) throw new ObjectDisposedException("You cannot use an instance of a class after it has been disposed of."); }


        public void UninstallHook() { CheckDisposed(); UnhookWindowsHookEx(hookID); }


        private readonly int WH_KEYBOARD_LL = 13;
        private IntPtr SetHook(KeyboardHookHandler proc) =>
                        SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                         GetModuleHandleW(Process.GetCurrentProcess().MainModule is not ProcessModule module2 ?
                          throw new NullReferenceException() : module2.ModuleName ?? throw new NullReferenceException()), 0);



        internal delegate void KeyboardHookCallback(VKeys key, SettingHook setting);
        internal event KeyboardHookCallback? KeyDown;


        internal SettingHook Settings = new SettingHook();

        private IntPtr HookFunc(int nCode, WMEvent wParam, TagKBDLLHOOKSTRUCT lParam)
        {
            if (nCode >= 0)
            {
                if (wParam is WMEvent.WM_KEYDOWN || wParam is WMEvent.WM_SYSKEYDOWN)
                {
                    KeyDown?.Invoke(lParam.Vkcode, Settings);
                    if (Settings.Break is true)
                    {
                        Settings.Break = false;
                        return (System.IntPtr)1;
                    }
                }

            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
        enum WMEvent
        {
            WM_KEYDOWN = 256,
            WM_SYSKEYDOWN = 260,
            WM_KEYUP = 257,
            WM_SYSKEYUP = 261
        }
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-kbdllhookstruct?redirectedfrom=MSDN
        [StructLayout(LayoutKind.Sequential)]
        private struct TagKBDLLHOOKSTRUCT
        {
            internal readonly VKeys Vkcode;
            internal readonly int ScanCode;
            internal readonly int Flags;
            internal readonly int Time; // Милисикунды между сообщениями. Обнуляются при переполнинии.
            internal readonly UIntPtr DwExtraInfo;   //??
        }
        internal struct KeyStats
        {
            internal bool Extendedkey;
            internal bool EventjectedisLow;
            internal bool EventIsInjected;
            internal bool ALTkeyIsPpressed;
            internal bool KeyNotIsPressed;

            public static implicit operator KeyStats(int flags) => new KeyStats
            {
                Extendedkey = Convert.ToBoolean(flags & 0b_0000_0000),
                EventjectedisLow = Convert.ToBoolean(flags & 0b_0000_0010),
                EventIsInjected = Convert.ToBoolean(flags & 0b_0001_0000),
                ALTkeyIsPpressed = Convert.ToBoolean(flags & 0b_0010_0000),
                KeyNotIsPressed = Convert.ToBoolean(flags & 0b_1000_0000),
            };

        }

        internal class SettingHook
        {
            internal bool Break { get; set; } = default;
        }

        #region WinAPI
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookHandler lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, WMEvent wParam, TagKBDLLHOOKSTRUCT lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandleW([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);
        #endregion
    }
}

