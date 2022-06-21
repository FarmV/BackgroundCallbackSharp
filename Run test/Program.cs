using FVH.Background.InputHandler;
using FVH.Background.Input;
using System.Windows;
using FVH.Background;
using System.Reflection;
using System.IO.Packaging;
using System.IO;

namespace Run_test
{
    internal class Program
    {

        static async Task Main()
        {
           // _ = System.IO.Packaging.PackUriHelper.UriSchemePack; // UriHelper.GetUriApp заглушка лечние
            //  if (UriParser.IsKnownScheme("pack") is not true) _ = new System.Windows.Application();
           
            using Input input = new Input();
            ICallBack iCallBack = await input.Subscribe();
            await iCallBack.AddCallBackTask(new VKeys[] { VKeys.VK_SHIFT, VKeys.VK_CONTROL, VKeys.VK_CONTROL },
                () => new Task(() =>
                {
                    Thread thread = new Thread(() =>
                    {
                        Test1 test = new Test1();
                        test.Notificator = new Notificator();

                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    System.Windows.MessageBox.Show("ok");
                }));

            //Thread thread = new Thread(() =>
            //{
            //    Window window = new Window();
            //    window.Show();
            //    Uri r1 = UriHelper.GetUriApp(@"Resources\Icon2Test.dll");
            //});
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();

            Console.WriteLine();

            await Task.Delay(255000);

            Environment.Exit(0);
        }
        internal class Test1
        {
            internal Notificator? Notificator;
        }
        internal static class UriHelper
        {
            internal static Uri GetUriApp(string resourcePath) => new Uri(string.Format("pack://application:,,,/{0};component/{1}", Assembly.GetExecutingAssembly().GetName().Name, resourcePath));
        }
    }
}