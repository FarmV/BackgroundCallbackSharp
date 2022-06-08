using FVH.Background.InputHandler;
using FVH.Background.Input;
using System.Windows;

namespace Run_test
{
    internal class Program
    {
        static async Task Main()
        {
            using Input input = new Input();
            ICallBack iCallBack = await input.Subscribe();
            await iCallBack.AddCallBackTask(new VKeys[] { VKeys.VK_SHIFT, VKeys.VK_CONTROL, VKeys.VK_CONTROL }, 
                () => new Task(()=> System.Windows.MessageBox.Show("test")));
            await Task.Delay(255000);
            Environment.Exit(0);
        }
    }
}