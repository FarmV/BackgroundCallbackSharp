﻿

using Linearstar.Windows.RawInput;

using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace FVH.BackgroundInput
{

    public class Input : IDisposable
    {
        private volatile HwndSource? ProxyInputHandlerWindow;
      

        private bool isDispose = false;
        public void Dispose()
        {
            if (isDispose is true) return;
            ProxyInputHandlerWindow?.Dispatcher?.InvokeShutdown();
            ProxyInputHandlerWindow?.Dispose();
            isDispose = true;
            GC.SuppressFinalize(this);
        }
        ~Input() => Dispose();


        private bool isItialized = false;
        private static Action<string>? _callback = null;
        public async Task Subscribe(Action<string> callback)
        {
            if (isItialized is true) throw new InvalidOperationException("You cannot reinitialize the same class instance");
            EventWaitHandle WaitHandleStartWindow = new EventWaitHandle(false, EventResetMode.ManualReset);
            _callback = callback;

            Thread winThread = new Thread(() =>
            {
                HwndSourceParameters configInitWindow = new HwndSourceParameters("InputHandler", 0, 0)
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

                static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
                {
                    const int WM_INPUT = 0x00FF;
                    switch (msg)
                    {
                        case WM_INPUT:
                            {
                                RawInputData data = RawInputData.FromHandle(lParam);
                                _callback?.Invoke(data.ToString() ?? throw new NullReferenceException($"{nameof(RawInputData)} cannot be null"));
                            }
                            break;
                    }
                    return hwnd;
                }
            }, DispatcherPriority.Render);

            isItialized = true;
            WaitHandleStartWindow.Dispose();           
        }
    }



    public class ExtensionInput //: IDisposable
    {

        //private bool isDispose = false;
        //public void Dispose()
        //{
        //    if (isDispose is true) return;
     
        //    isDispose = true;
        //    GC.SuppressFinalize(this);
        //}
        //~ExtensionInput() => Dispose();
        public Task<HwndSource> GetProxyWindow()
        {
            EventWaitHandle WaitHandleStartWindow = new EventWaitHandle(false, EventResetMode.ManualReset);

            HwndSource? ProxyInputHandlerWindow = null;
            Thread winThread = new Thread(() =>
            {
                Console.WriteLine("Окно создано...");
                HwndSourceParameters configInitWindow = new HwndSourceParameters("InputHandlerExtension", 0, 0)
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
            return Task.FromResult(ProxyInputHandlerWindow is not null ? ProxyInputHandlerWindow : throw new NullReferenceException());
        }

    }

    public interface IBcakInput
    {

    }





}



