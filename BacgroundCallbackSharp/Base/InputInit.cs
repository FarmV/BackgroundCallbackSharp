using FVH.Background.Input;

using Linearstar.Windows.RawInput;

using System;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Xml.Linq;

using static FVH.Background.Input.DataHandler;

namespace FVH.Background.Input
{
    /// <summary>
    /// <br><see langword="En"/></br>
    ///<br/>The class creates a proxy <see cref="HwndSource"/>. Registers it to receive mouse and keyboard events. Creates classes to handle events.
    ///<br><see langword="Ru"/></br>
    ///<br>Класс создает прокси-источник HwndSource. Регистрирует его для получения событий мыши и клавиатуры. Создает классы для обработки событий.</br>
    ///<code Language ="cs">  
    ///<a>Example:</a>
    ///<br>
    ///<see langword="using"/> <see cref="Input"/> <paramref name="input"/> = <see langword="new"/>();
    ///</br>
    ///<br>
    ///<see cref="ICallBack"/> <paramref name="input"/> = <see langword="await"/> <paramref name="input"/>.Subscribe();
    ///</br>
    ///</code>    
    ///</summary>
    public class Input : IDisposable
    {
        private volatile HwndSource? ProxyInputHandlerWindow;

        private bool isDispose = false;
        public void Dispose()
        {
            if (isDispose is true) return;

            ProxyInputHandlerWindow?.Dispatcher?.InvokeShutdown();
            _lowLevlHook?.Dispose();
            isDispose = true;
            GC.SuppressFinalize(this);
        }

        ~Input()
        {
            if (isDispose is true) return;
            try
            {
                _lowLevlHook?.Dispose();
                ProxyInputHandlerWindow?.Dispose();
            }
            catch { }
        }


        private bool isItialized = false;
        private readonly Action<RawInputKeyboardData> _callbackEventKeyboardData;
        private readonly Action<RawInputMouseData> _callbackEventMouseData;
        private readonly IHandler _keyboardHandler;
        private LowLevlHook? _lowLevlHook;
        private CallbackFunction? _callbackFunction;

        public Input()
        {
            _keyboardHandler = new DataHandler();
            _callbackEventKeyboardData = new Action<RawInputKeyboardData>((x) => _keyboardHandler.HandlerKeyboard(x));
            _callbackEventMouseData = new Action<RawInputMouseData>((x) => _keyboardHandler.HandlerMouse(x));

        }

        public async Task<ICallBack> Subscribe()
        {

            if (isItialized is true) return _callbackFunction is not null ? _callbackFunction : throw new NullReferenceException($"{nameof(_callbackFunction)} cannot be null");


            EventWaitHandle WaitHandleStartWindow = new EventWaitHandle(false, EventResetMode.ManualReset);


            Thread winThread = new Thread(() =>
            {
                HwndSourceParameters configInitWindow = new HwndSourceParameters($"InputHandler-{Path.GetRandomFileName}", 0, 0)
                {
                    WindowStyle = 0x800000
                };
                ProxyInputHandlerWindow = new HwndSource(configInitWindow);
                WaitHandleStartWindow.Set();
                Dispatcher.Run();
            });
            winThread.SetApartmentState(ApartmentState.STA);
            winThread.Start();

            WaitHandleStartWindow.WaitOne();

            if (ProxyInputHandlerWindow is null) throw new NullReferenceException("The window could not initialize");
            await ProxyInputHandlerWindow.Dispatcher.InvokeAsync(() =>
            {
                RawInputDeviceRegistration[] devices =
                {
                         new RawInputDeviceRegistration(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, ProxyInputHandlerWindow.Handle),
                         new RawInputDeviceRegistration(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink, ProxyInputHandlerWindow.Handle)
                };

                RawInputDevice.RegisterDevice(devices);
                ProxyInputHandlerWindow.AddHook(WndProc);

                IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
                {
                    const int WM_INPUT = 0x00FF;
                    switch (msg)
                    {
                        case WM_INPUT:
                            {
                                if (RawInputData.FromHandle(lParam) is RawInputData data)
                                {
                                    switch (data)
                                    {
                                        case RawInputKeyboardData keyboardData:
                                            _callbackEventKeyboardData.Invoke(keyboardData);
                                            break;

                                        case RawInputMouseData mouseData:
                                            _callbackEventMouseData.Invoke(mouseData);
                                            break;
                                    }

                                }
                            }
                            break;
                    }
                    return hwnd;
                }

                _lowLevlHook = new LowLevlHook();
                _lowLevlHook.InstallHook();
                _callbackFunction = new CallbackFunction(_keyboardHandler, _lowLevlHook);
            }, DispatcherPriority.Render);

            isItialized = true;
            WaitHandleStartWindow.Dispose();
            return _callbackFunction is not null ? _callbackFunction : throw new NullReferenceException($"{nameof(_callbackFunction)} cannot be null");
        }

    }



    public class ExtensionInput
    {
        private bool isItialized = false;
        public Task<HwndSource> GetOneInstanceProxyWindow()
        {
            if (isItialized is true) throw new InvalidOperationException("You cannot reinitialize the same class instance");
            EventWaitHandle WaitHandleStartWindow = new EventWaitHandle(false, EventResetMode.ManualReset);

            HwndSource? ProxyInputHandlerWindow = null;
            Thread winThread = new Thread(() =>
            {
                HwndSourceParameters configInitWindow = new HwndSourceParameters($"InputHandlerExtension-{Path.GetRandomFileName}", 0, 0)
                {
                    WindowStyle = 0x800000
                };
                HwndSource? ProxyInputHandlerWindow = new HwndSource(configInitWindow);
                WaitHandleStartWindow.Set();
                Dispatcher.Run();
            });
            winThread.SetApartmentState(ApartmentState.STA);
            winThread.Start();

            WaitHandleStartWindow.WaitOne();
            WaitHandleStartWindow.Dispose();
            isItialized = true;
            return Task.FromResult(ProxyInputHandlerWindow is not null ? ProxyInputHandlerWindow : throw new NullReferenceException());
        }
    }



    /// <summary>
    /// <br><see langword="En"/></br>
    ///<br/>This interface declares a contract for the keyboard input registration class. 
    ///<br><see langword="Ru"/></br>
    ///<br>Этот интерфейс объявляет контракт для класса регистрации ввода с клавиатуры.</br>
    ///<code Language ="cs">  
    ///<a>Example:</a>
    ///<br>
    ///<see cref="Input"/> <paramref name="input"/> = <see langword="new"/>();
    ///</br>
    ///<br>
    ///<see cref="ICallBack"/> <paramref name="input"/> = <see langword="await"/> <paramref name="input"/>.Subscribe();
    ///</br>
    ///</code>    
    ///</summary>

    public interface ICallBack
    {
        public Task AddCallBackTask(VKeys[] keyCombo, Func<Task> callbackTask, object? identifier = null);
        public Task<bool> DeleteATaskByAnIdentifier(object? identifier = null);
        public Task<bool> ContainsKeyComibantion(VKeys[] keyCombo);
        public List<RegGroupFunction> ReturnGroupRegFunctions();
        public Task<IHandler> GetHandler();
    }
    /// <summary>
    /// <br><see langword="En"/></br>
    ///<br/>This interface declares a contract for the keyboard and mouse event handler class. 
    ///<br><see langword="Ru"/></br>
    ///<br>Этот интерфейс объявляет контракт для класса обработчика событий клавиатуры и мыши.</br>
    ///<code Language ="cs">  
    ///<a>Example:</a>
    ///<br>
    ///<see langword="using"/> <see cref="Input"/> <paramref name="input"/> = <see langword="new"/>();
    ///</br>
    ///<br>
    ///<see cref="ICallBack"/> <paramref name="input"/> = <see langword="await"/> <paramref name="input"/>.Subscribe();
    ///</br>
    ///<br>
    ///<see cref="IHandler"/> <paramref name="handler"/> = <see langword="await"/> <paramref name="handler"/>.GetHandler(); 
    ///</br>
    ///</code>    
    ///</summary>
    public interface IHandler //todo добавить интерфейс для мыши этот оставить для клавиатуры
    {
        public List<VKeys> PressedKeys { get; }
        public event EventHandler<DataKeysNotificator>? KeyPressEvent;
        public event EventHandler<DataKeysNotificator>? KeyUpPressEvent;
        public event EventHandler<RawInputMouseData>? MouseEvent;
        internal void HandlerKeyboard(RawInputKeyboardData data);
        internal void HandlerMouse(RawInputMouseData data);
    }

    public interface IKeyboardHandlerBase
    {
        internal void HandlerKeyboard(RawInputKeyboardData data);
    }
    public interface IKeyboardHandler : IKeyboardHandlerBase
    {
        public List<VKeys> PressedKeys { get; }
        public event EventHandler<DataKeysNotificator>? KeyPressEvent;
        public event EventHandler<DataKeysNotificator>? KeyUpPressEvent;
    }



}



