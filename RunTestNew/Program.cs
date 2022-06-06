using FVH.Background.InputHandler;
using FVH.Background.Input;
using System.Windows;

namespace RunTestNew
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using Input input = new Input();
            ICallBack? callBack = await input.Subscribe();

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
                         MessageBox.Show("OK 1");
                     })
                   );
            await callBack.AddCallBackTask
                   (
                     new VKeys[]
                     {
                       VKeys.VK_SHIFT,
                       VKeys.VK_CONTROL,
                       VKeys.VK_BACK,
                     },
                     () => new Task(() =>
                     {
                         MessageBox.Show("OK 2");
                     })
                   );
            await callBack.AddCallBackTask
                   (
                     new VKeys[]
                     {
                       VKeys.VK_SHIFT,
                       VKeys.VK_CONTROL,
                       VKeys.VK_NUMPAD9
                     },
                     () => new Task(() =>
                     {
                         MessageBox.Show("OK 2");
                     })
                   );
                            
            await callBack.AddCallBackTask
                   (
                     new VKeys[]
                     {
                       VKeys.VK_KEY_G,

                     },
                     () => new Task(() =>
                     {
                         MessageBox.Show("OK 3");
                     })
                   );
              await callBack.AddCallBackTask
                   (
                     new VKeys[]
                     {
                       VKeys.VK_KEY_G,

                     },
                     () => new Task(() =>
                     {
                         MessageBox.Show("OK 4");
                     })
                   );
                             
             await callBack.AddCallBackTask
                  (
                    new VKeys[]
                    {
                         VKeys.VK_SHIFT,
                         VKeys.VK_CONTROL,
                         VKeys.VK_KEY_Q

                    },
                    () => new Task(() =>
                    {
                        MessageBox.Show("OK 4");
                    })
                  );
           
            await Task.Delay(TimeSpan.FromSeconds(250));

            Environment.Exit(0);

        }
    }
}