using FVH.Background.InputHandler;
using FVH.Background.Input;
using System.Windows;
using FVH.Background;
using System.Reflection;
using System.IO.Packaging;
using System.IO;
using static FVH.Background.Notificator;
using System.Windows.Threading;


namespace Run_test
{
    internal class Program
    {
        static Notificator? _notificator;
        class MyNotiWindow : WindowHideAltTab
        {
            public MyNotiWindow()
            {
                this.Content = new System.Windows.Controls.Button() { Name = "Exit", Content = "Exit", FontSize = 72 };
                this.Topmost = true;
                this.WindowStyle = WindowStyle.None;
                this.SizeToContent = SizeToContent.WidthAndHeight;
                this.ResizeMode = ResizeMode.NoResize;
                System.Windows.Controls.Button button = (System.Windows.Controls.Button)this.Content;
                button.Click += (x, y) =>
                {

                    _notificator?.Dispose();
                    //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    
                    //     System.Windows.Application.Current.Shutdown();
                        
                    //});

                    System.Windows.Forms.Application.Exit();
                    Environment.Exit(0);
                };
            }
        }

        static async Task Main()
        {
            //  if (UriParser.IsKnownScheme("pack") is not true);
            _ = System.IO.Packaging.PackUriHelper.UriSchemePack; // UriHelper.GetUriApp заглушка лечние

            using Input input = new Input();
            ICallBack iCallBack = await input.Subscribe();
            await iCallBack.AddCallBackTask(new VKeys[] { VKeys.VK_SHIFT,VKeys.VK_KEY_A, VKeys.VK_KEY_A },
                () => new Task(() => System.Windows.MessageBox.Show("Ok") ));
              



            Console.WriteLine();

            await Task.Delay(255000);

            Environment.Exit(0);
        }

        internal static class UriHelper
        {
            internal static Uri GetUriApp(string resourcePath) => new Uri(string.Format("pack://application:,,,/{0};component/{1}", Assembly.GetExecutingAssembly().GetName().Name, resourcePath));
        }
    }
}