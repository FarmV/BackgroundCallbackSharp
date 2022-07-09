using FVH.Background.Input;
using FVH.Background.InputHandler;

using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;



namespace Run_test
{
    internal static class ConstApp
    {
        internal const string Input = "App.Input";
        internal const string Shutdown = "App.Shutdown";
    }
    internal class Program
    {
      
        static async Task Main(string[] args)
        {
       
            AppDomain.CurrentDomain.SetData(ConstApp.Input, Task.Run(() => new Input()));

            Thread helperThread = new Thread(() =>
            {
                Thread.CurrentThread.Name = "HelperThread";
               
                Dispatcher.Run();
            });
            helperThread.SetApartmentState(ApartmentState.STA);

            Thread winThread = new Thread(() =>
            {
                Thread.CurrentThread.Name = "WinThread";
               
                Dispatcher.Run();
            });

            winThread.SetApartmentState(ApartmentState.STA);
            Parallel.Invoke(() => winThread.Start(), () => helperThread.Start());

            Dispatcher? winDispatcher = Dispatcher.FromThread(winThread);
            Dispatcher? helperDispatcher = Dispatcher.FromThread(helperThread);

            while (winDispatcher is null || helperDispatcher is null)
            {
                winDispatcher = Dispatcher.FromThread(winThread);
                helperDispatcher = Dispatcher.FromThread(helperThread);                
            }
            
            bool TimeoutInitDispathcer = false;
            System.Threading.Timer Timer = new System.Threading.Timer((_) => TimeoutInitDispathcer = true);
            Timer.Change(TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);

            while (true)
            {
                try
                {
                    if (TimeoutInitDispathcer is true) throw new InvalidOperationException(nameof(TimeoutInitDispathcer));
                    Task taskHelperInit = await helperDispatcher.InvokeAsync(async() => await Task.Delay(1)).Task;
                    Task taskWinInit = await winDispatcher.InvokeAsync(async () => await Task.Delay(1)).Task;
                    Parallel.Invoke(() => taskHelperInit.Wait(),()=> taskWinInit.Wait());
                    Timer.Dispose();
                    break;
                }
                catch (System.Threading.Tasks.TaskCanceledException) { }
            }

            //await Task.Delay(TimeSpan.FromSeconds(8));




            AppDomain.CurrentDomain.SetData(ConstApp.Shutdown, new Task(() => 
            {
                Parallel.Invoke(async () => await winDispatcher.InvokeAsync(() => winDispatcher.InvokeShutdown()),
                                async () => await helperDispatcher.InvokeAsync(() => helperDispatcher.InvokeShutdown()));
                Environment.Exit(0);
            }));

           

            Task addCallBackFromHelepr = await helperDispatcher.InvokeAsync(async () => 
            {               
                 if (AppDomain.CurrentDomain.GetData(ConstApp.Input) is not Task<Input> input) throw new InvalidOperationException(nameof(input));

                 await (await (await input).Subscribe()).
                 AddCallBackTask(new VKeys[] { VKeys.VK_SHIFT, VKeys.VK_CONTROL, VKeys.VK_CONTROL }, () => new Task(() =>
                 {
                     MessageBox.Show("Test");
                 }));
            }).Task;
                
            Task task2 = await winDispatcher.InvokeAsync(async () =>
            {               
                System.Windows.Window window = new System.Windows.Window()
                {
                    Height = 200,
                    Width = 1800,
                    Topmost = true,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                    Content = new TextBlock() { Name = "Text", Text = "Hello", FontSize = 30 }
                };

                window.Show();
                window.Closing += (s, e) =>
                {
                    if (AppDomain.CurrentDomain.GetData(ConstApp.Shutdown) is not Task shutdown) throw new InvalidOperationException($"App.Shutdown {nameof(shutdown)}");
                    shutdown.Start();
                    shutdown.Wait();
                };
                TextBlock textBlock = (TextBlock)window.Content;
                if (AppDomain.CurrentDomain.GetData(ConstApp.Input) is not Task<Input> input) throw new InvalidOperationException(nameof(input));

                (await (await (await input).Subscribe()).GetHandler()).MouseEvent += async (s, e) => await window.Dispatcher.InvokeAsync(() => textBlock.Text = e.ToString());

            }).Task;
          
            Parallel.Invoke(() => addCallBackFromHelepr.Wait(), () => task2.Wait());
        }
    }
}