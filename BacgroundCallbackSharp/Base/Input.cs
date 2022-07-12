using FVH.Background.Input;

using Linearstar.Windows.RawInput;

using System;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Xml.Linq;

using static FVH.Background.Input.KeyboardHandler;

namespace FVH.Background.Input
{
    /// <summary>
    /// <br><see langword="En"/></br>
    ///<br/>The class creates a proxy <see cref="HwndSource"/>. Registers it to receive mouse and keyboard events. Creates classes to handle events.
    ///<br><see langword="Ru"/></br>
    ///<br>Класс создает прокси-источник HwndSource. Регистрирует его для получения событий мыши и клавиатуры. Создает классы для обработки событий.</br>
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
        private readonly IKeyboardHandler _keyboardHandler;
        private readonly IMouseHandler _mouseHandler;
        private LowLevlHook? _lowLevlHook;
        private IKeyboardCallBack? _callbackFunction;
        private Thread? winThread;



        ///<returns>
        /// <br><see langword="En"/></br>
        /// <br>Reference to the class that implements the <see cref="IKeyboardHandler"/>.</br>
        /// <br><see langword="Ru"/></br>
        /// <br>Cсылка на класс, реализующий интерфейс <see cref="IKeyboardHandler"/>.</br>
        ///</returns>
        public IKeyboardHandler GetKeyboardHandler() => _keyboardHandler;
        ///<returns>
        /// <br><see langword="En"/></br>
        /// <br>Reference to the class that implements the <see cref="IMouseHandler"/>.</br>
        /// <br><see langword="Ru"/></br>
        /// <br>Cсылка на класс, реализующий интерфейс <see cref="IMouseHandler"/>.</br>
        ///</returns>
        public IMouseHandler GetMouseHandler() => _mouseHandler;
        ///<returns>
        /// <br><see langword="En"/></br>
        /// <br>Reference to the class that implements the <see cref="IKeyboardCallBack"/>.</br>
        /// <br><see langword="Ru"/></br>
        /// <br>Cсылка на класс, реализующий интерфейс <see cref="IKeyboardCallBack"/>.</br>
        ///</returns>
        public IKeyboardCallBack GetKeyboardCallbackFunction() => _callbackFunction is IKeyboardCallBack CallBack ? CallBack : throw new NullReferenceException(nameof(_callbackFunction));


        public Input() : this(null) { }
        public Input(IMouseHandler? mouseHandler = null)
        {
            _keyboardHandler = new KeyboardHandler();
            _mouseHandler = mouseHandler is IMouseHandler handlerMouse ? handlerMouse : new MouseHandler();

            _callbackEventKeyboardData = new Action<RawInputKeyboardData>((x) => _keyboardHandler.HandlerKeyboard(x));
            _callbackEventMouseData = new Action<RawInputMouseData>((x) => _mouseHandler.HandlerMouse(x));

            Task waitForInitialization = Task.Run(async () => await Initialization());
            waitForInitialization.Wait();
        }


        private TimeSpan TimeoutInitialization => TimeSpan.FromSeconds(10);
        private Task Initialization()
        {
            if (isItialized is true) throw new InvalidOperationException($"The object({nameof(Input)}) cannot be re-initialized");

            Task InitThreadAndSetWindowsHanlder = Task.Run(() =>
            {
                winThread = new Thread(() =>
                {
                    HwndSourceParameters configInitWindow = new HwndSourceParameters($"InputHandler-{Path.GetRandomFileName}", 0, 0)
                    {
                        WindowStyle = 0x800000
                    };
                    ProxyInputHandlerWindow = new HwndSource(configInitWindow);
                    Dispatcher.Run();
                });
                winThread.SetApartmentState(ApartmentState.STA);
                winThread.Start();
            });

            Task waitforWidnowDispather = Task.Run(async () =>
            {
                Dispatcher? winDispatcher = Dispatcher.FromThread(winThread);

                while (winDispatcher is null)
                {
                    winDispatcher = Dispatcher.FromThread(winThread);
                }

                bool TimeoutInitDispathcer = false;
                System.Threading.Timer Timer = new System.Threading.Timer((_) => TimeoutInitDispathcer = true);
                Timer.Change(TimeoutInitialization, Timeout.InfiniteTimeSpan);
                while (true)
                {
                    try
                    {
                        if (TimeoutInitDispathcer is true) throw new InvalidOperationException(nameof(TimeoutInitDispathcer));
                        Task taskWinInit = await winDispatcher.InvokeAsync(async () => await Task.Delay(1)).Task;
                        Timer.Dispose();
                        break;
                    }
                    catch (System.Threading.Tasks.TaskCanceledException) { }
                }
            });

            Task subscribeWindowtoRawInput = new Task(() =>
            {
                if (ProxyInputHandlerWindow is null) throw new NullReferenceException("The window could not initialize");

                ProxyInputHandlerWindow.Dispatcher.Invoke(() => // синхронно?
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

                    _lowLevlHook = new LowLevlHook(); // он должен быть создан потоком владецем окна?
                    _lowLevlHook.InstallHook();
                    _callbackFunction = new CallbackFunctionKeyboard(_keyboardHandler, _lowLevlHook);
                }, DispatcherPriority.Render);

            });
            Task.WaitAll(InitThreadAndSetWindowsHanlder, waitforWidnowDispather);
            subscribeWindowtoRawInput.Start();
            subscribeWindowtoRawInput.Wait();
            isItialized = true;
            return Task.CompletedTask;
        }
    }
}



