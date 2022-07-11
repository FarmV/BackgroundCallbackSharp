using FVH.Background.Input;

using Linearstar.Windows.RawInput;

using System.Threading.Tasks;
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

        internal class MyMouseH : IMouseHandler
        {
            public event EventHandler<RawInputMouseData>? MouseEvent;

            public void HandlerMouse(RawInputMouseData data)
            {
                MouseEvent?.Invoke(this, data);
            }
        }

        static async Task Main(string[] args)
        {
          
            AppDomain.CurrentDomain.SetData(ConstApp.Input, Task.Run(() => new Input(mouseHandler: new MyMouseH())));

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
                    Task taskHelperInit = await helperDispatcher.InvokeAsync(async () => await Task.Delay(1)).Task;
                    Task taskWinInit = await winDispatcher.InvokeAsync(async () => await Task.Delay(1)).Task;
                    Parallel.Invoke(() => taskHelperInit.Wait(), () => taskWinInit.Wait());
                    Timer.Dispose();
                    break;
                }
                catch (System.Threading.Tasks.TaskCanceledException) { }
            }

            AppDomain.CurrentDomain.SetData(ConstApp.Shutdown, new Task(() =>
            {
                Parallel.Invoke(async () => await winDispatcher.InvokeAsync(() => winDispatcher.InvokeShutdown()),
                                async () => await helperDispatcher.InvokeAsync(() => helperDispatcher.InvokeShutdown()));
                Environment.Exit(0);
            }));



            Task addCallBackFromHelepr = await helperDispatcher.InvokeAsync(async () =>
            {
                if (AppDomain.CurrentDomain.GetData(ConstApp.Input) is not Task<Input> input) throw new InvalidOperationException(nameof(input));

                await (await (await input).GetKeyboardCallbackFunction()).
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

                (await (await input).GetMouseHandler()).MouseEvent += async (s, e) => await window.Dispatcher.InvokeAsync(() => textBlock.Text = e.ToString());

            }).Task;

            Parallel.Invoke(() => addCallBackFromHelepr.Wait(), () => task2.Wait());



            //try
            //{
            //    Task rrr = await winDispatcher.InvokeAsync(async () => { await Task.Delay(5000); MessageBox.Show("???"); }).Task;
            //    await rrr.TimeOutDiscription(TimeSpan.FromSeconds(2), "Good job");

            //}
            //catch
            //{
            //    MessageBox.Show("CATH");
            //}
            //await await (await winDispatcher.InvokeAsync(async () => { await Task.Delay(15000); MessageBox.Show("???"); })).TimeOutDiscription(TimeSpan.FromSeconds(2), "Good job");
            //await Task.Delay(5000);




        }


    }
    internal static class HelpO
    {
        // не отпрвлять в метод where T is not Task<Task<T>> и т.д Где результат операции таск с типом таск
        internal static async Task<Task<T>> TimeOutDiscription<T>(this Task<T> workTask, TimeSpan timeout, string descriptionTimout)
        {
            try
            {
                await workTask.WaitAsync(timeout);
                return workTask;
            }
            catch (System.TimeoutException timeEx)
            {
                throw new TimeoutException(descriptionTimout, timeEx);
            }
            throw new NotImplementedException();
        }
        internal static async Task<Task> TimeOutDiscription(this Task workTask, TimeSpan timeout, string descriptionTimout)
        {
            try
            {
                await workTask.WaitAsync(timeout);
                return workTask;
            }
            catch (System.TimeoutException timeEx)
            {
                throw new TimeoutException(descriptionTimout, timeEx);
            }
            throw new NotImplementedException();
        }
    }

}