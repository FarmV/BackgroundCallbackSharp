using FVH.Background.InputHandler;
using FVH.Background.Input;
using System.Windows;

namespace RunTestNew
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Input input = new Input();
            using ICallBack? callBack = await input.Subscribe();

            await callBack.AddCallBackTask
                   (
                     new VKeys[]
                     {
                       VKeys.VK_SHIFT,
                       VKeys.VK_CONTROL,
                       VKeys.VK_SPACE
                     },
                     () => new Task(() => { MessageBox.Show("OK =)"); })
                   );

            await Task.Delay(TimeSpan.FromSeconds(15));

            Environment.Exit(0);

        }
    }
}