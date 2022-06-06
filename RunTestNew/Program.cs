using FVH.Background.InputHandler;
using FVH.Background.Input;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;

namespace RunTestNew
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using Input input = new Input();
            ICallBack callBack = await input.Subscribe();
            IHandler Handler = input.GetHandler();
            

            Window1? window1 = null;
            Thread thread = new Thread(() =>
            {
                window1 = new Window1
                {
                    Topmost = true
                };
                // window1.Show();
                Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Priority = ThreadPriority.AboveNormal;
            thread.Start();

            await callBack.AddCallBackTask
                   (
                     new VKeys[]
                     {
                       VKeys.VK_SHIFT,
                       VKeys.VK_CONTROL,
                       VKeys.VK_SPACE
                     },
                     () => new Task(() =>
                     {
                       
                             window1?.Dispatcher.InvokeAsync(() =>
                             {
                                   CancellationTokenSource source = new CancellationTokenSource();
                                 _ = RefreshWindowPositin.RefreshWindowPosCursor(window1, source.Token);
                                 window1.Show();
                                
                                 //Mouse.Capture(window1, CaptureMode.SubTree);

                                // Mouse.OverrideCursor = Cursors.None;
                                 window1.LottieAnimationView.PlayAnimation();
                                 //await Task.Delay((int)window1.LottieAnimationView.Composition.Duration);
                              //   source.Cancel();
                                // Mouse.Capture(null);
                                // Mouse.OverrideCursor = null;
                                // window1.Hide();
                                // window1.LottieAnimationView.PauseAnimation();
                                 //source.Dispose();
                             }, DispatcherPriority.Render);

 
                     })

                   );
            await callBack.AddCallBackTask
                  (
                    new VKeys[]
                    {
                       VKeys.VK_ESCAPE
                    },
                    () => new Task(() =>
                    {
                        window1?.Dispatcher.Invoke(() => window1.Close());
                        Environment.Exit(0);
                    })
                  );

            await Task.Delay(TimeSpan.FromSeconds(250));

            Environment.Exit(0);

        }


    }

    internal class RefreshWindowPositin
    {
        static Point pointOld = new Point(0, 0);
        internal static async Task RefreshWindowPosCursor(Window window, CancellationToken token)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);

            while (token.IsCancellationRequested is not true)
            {

               // Point point = GetCursorPosition();
                //if (pointOld == point) continue;
                await Task.Delay(1);   // узнать можно ли починитиь ошибку диспечера потоков окна?
               // pointOld = point;
                await window.Dispatcher.BeginInvoke(() =>
                {
                    //SetWindowPos(helper.Handle, -1, Convert.ToInt32(point.X - window.Width / 2), Convert.ToInt32(point.Y - window.Height / 2),
                    //Convert.ToInt32(window.Width), Convert.ToInt32(window.Height), 0x0400 | 0x0040);
                    ((Window1)window).DragMove();
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr handle, int handle2, int x, int y, int cx, int cy, int flag);


        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }
        internal static Point GetCursorPosition()
        {
            GetCursorPos(out POINT lpPoint);
            return lpPoint;
        }
    }
}