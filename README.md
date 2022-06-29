# BackgroundCallbackSharp

## Example

```Csharp
using FVH.Background.InputHandler;
using FVH.Background.Input;

***
using Input input = new Input();
ICallBack iCallBack = await input.Subscribe();

 await iCallBack.AddCallBackTask(new VKeys[] { VKeys.VK_SHIFT, VKeys.VK_CONTROL, VKeys.VK_CONTROL }, 
        () => new Task(() => System.Windows.MessageBox.Show("Ok") ));
***

// NotUse  VKeys.VK_LCONTROL, VKeys.VK_RCONTROL,VKeys.VK_LMENU,VKeys.VK_RMENU,VKeys.VK_LSHIFT,VKeys.VK_RSHIFT
// Use VKeys.VK_CONTROL, VKeys.VK_MENU, VKeys.VK_SHIFT;

```
* A + A = false
* B+A+A = true
* A+B+С = true

Intercepts the last keystrokes of the keyboard(Supposedly only for x64)
