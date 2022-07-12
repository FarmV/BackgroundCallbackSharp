# BackgroundCallbackSharp

## Example

```Csharp
using FVH.Background.Input;

***
Input input = new Input();
IKeyboardCallBack keyboardCallBack = input.GetKeyboardCallbackFunction();
await keyboardCallBack.AddCallBackTask(new VKeys[] { VKeys.VK_SHIFT, VKeys.VK_HOME, VKeys.VK_KEY_E, VKeys.VK_KEY_E }, () => new Task(() => { MessageBox.Show("OK"); }) );
***

// NotUse  VKeys.VK_LCONTROL, VKeys.VK_RCONTROL,VKeys.VK_LMENU,VKeys.VK_RMENU,VKeys.VK_LSHIFT,VKeys.VK_RSHIFT
// Use VKeys.VK_CONTROL, VKeys.VK_MENU, VKeys.VK_SHIFT;

```
* A + A = false
* B+A+A = true
* A+B+С = true

Intercepts the last keystrokes of the keyboard(Supposedly only for x64)
