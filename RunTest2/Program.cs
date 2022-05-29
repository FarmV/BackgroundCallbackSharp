using FVH.Background.Input;
using FVH.Background.InputHandler;

namespace RunTest2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Input input = new Input();
            ICallBack callBack = await input.Subscribe();
            await callBack.AddCallBackTask
                (
                 new VKeys[]
                 {
                    VKeys.VK_LWIN,
                    //VKeys.VK_SHIFT,
                    VKeys.VK_KEY_Q,

                 },
                 () => new Task(() => MessageBox.Show("Test"))
                );





            await Task.Delay(TimeSpan.FromSeconds(15));
            input.Dispose();
            Environment.Exit(0);
        }
    }
}