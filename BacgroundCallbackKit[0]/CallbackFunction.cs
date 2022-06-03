using FVH.Background.InputHandler;

using System.Diagnostics.CodeAnalysis;

namespace FVH.Background.Input
{
    internal class VKeysEqualityComparer : IEqualityComparer<VKeys[]>
    {
        public bool Equals(VKeys[]? x, VKeys[]? y) => x is not null && y is not null && x.Length is not 0 && y.Length is not 0 && x.SequenceEqual(y);
        public int GetHashCode([DisallowNull] VKeys[] obj) => 0;

    }
    internal class CallbackFunction : IDisposable, ICallBack
    {

        public bool IsDispose { get; private set; }
      
        public void Dispose() 
        {
            if (IsDispose) return;
            _lowlevlhook?.Dispose();

            IsDispose = true;
            GC.SuppressFinalize(this);       
        }
   
        public CallbackFunction(KeyboardHandler keyboardHandler, LowLevlHook lowLevlHook)
        {
            _keyboardHandler = keyboardHandler;
            _lowlevlhook = lowLevlHook;
            _keyboardHandler.KeyPressEvent += KeyboardHandler_KeyPressEvent;
        }

        private async void KeyboardHandler_KeyPressEvent(object? sender, DataKeysNotificator e)
        {
            VKeys[] pressedKeys = e.Keys;
            if (pressedKeys.Length is 0) return;

            List<VKeys> listPreKeys = new List<VKeys>();

            List<VKeys[]> fullV = new List<VKeys[]>();
            List<VKeys[]> keys = Tasks.FunctionsCallback.Keys.Where(itemKeyArray =>  // Почему то метод пропускается при активации формы в хуке
            {
                for (int i = 0; i < pressedKeys.Length; i++)
                {
                    if (itemKeyArray.Any((xVkey) => xVkey == pressedKeys[i]) is not true) return false;
                    else
                    {
                        VKeys[] keyPre = itemKeyArray.Except(pressedKeys).ToArray();
                        if (keyPre.Length > 1) return false;
                        else
                        {
                            if (keyPre.Length is 0)
                            {
                                //  fullV.Add(itemKeyArray); // Вообще надо бы сделать более эфективную с логической точки срения обработку.
                                return false;
                            };
                            listPreKeys.Add(keyPre[0]);
                        }
                    }
                }
                return true;
            }).Where(x => x.Length == pressedKeys.Length).ToList();

            if (fullV.Count is 1 & listPreKeys.Count is 0)
            {
                keys.Clear();
                keys.Add(fullV.ToArray()[0]);
            }
     
            var listPreKeys2 = listPreKeys.Distinct().ToList();
            if (listPreKeys2.Count > 0)
            {
                VKeys? callhook = await PreKeys(listPreKeys);
                if (callhook.HasValue is true)
                {
                    VKeys[] preseedKeys1 = pressedKeys.Append(callhook.Value).ToArray();

                    keys = Tasks.FunctionsCallback.Keys.Where(x =>
                    {
                        for (int i = 0; i < preseedKeys1.Length; i++)
                        {
                            if (x.Any((x) => x == preseedKeys1[i]) is not true)
                            {
                                return false;
                            }
                        }
                        return true;
                    }).Where(x => x.Length == pressedKeys.Length + 1).ToList();
                }
            }

            if (keys.Count is 0) return;
            if (keys.Count > 1) throw new InvalidOperationException();
            _ = Task.Run(() =>
            {
                try
                {
                    Tasks.FunctionsCallback[keys[0]].Invoke().Start();
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception)
                {

                }
            }).ConfigureAwait(false);

        }

        private LowLevlHook _lowlevlhook;
        private KeyboardHandler _keyboardHandler;
        private async Task<VKeys?> PreKeys(List<VKeys> keys)
        {
            if (_lowlevlhook is null) throw new NullReferenceException(nameof(LowLevlHook));
            VKeys? res = null;

            bool ret = default;
            _lowlevlhook.KeyDown += CheckKey;

            void CheckKey(VKeys key, LowLevlHook.SettingHook setting)
            {
                if (_lowlevlhook is null) throw new NullReferenceException(nameof(LowLevlHook));
                if (keys.Contains(key))
                {
                    res = key;
                    setting.Break = true;
                    _lowlevlhook.KeyDown -= CheckKey;
                    ret = true;
                }
                else
                {
                    _lowlevlhook.KeyDown -= CheckKey;
                    ret = true;
                }
            }

            for (int i = 0; i < 20; i++)
            {
                if (ret is true) break;
                await Task.Delay(1);
            }
            return res;           
        }

        private class Tasks
        {
            internal static Dictionary<VKeys[], Func<Task>> FunctionsCallback = new Dictionary<VKeys[], Func<Task>>(new VKeysEqualityComparer());
        }


        private object _lockMedthod = new object();


        public Task AddCallBackTask(VKeys[] keyCombo, Func<Task> callbackTask, bool isOneKey = default)
        {
            lock (_lockMedthod)
            {
              
                if (keyCombo.Length is 0) throw new InvalidOperationException($"One key to register with the key was transferred {nameof(isOneKey)} false");

                if (Tasks.FunctionsCallback.ContainsKey(keyCombo) is true) throw new InvalidOperationException("The key combination(s) is already registered");

                if (keyCombo.Length is 1 & isOneKey is false) throw new InvalidOperationException("Unable to register an empty key combination");

                Tasks.FunctionsCallback.Add(keyCombo, callbackTask);

                return Task.CompletedTask;
            }
        }

        public Task<IEnumerable<KeyValuePair<VKeys[], Func<Task>>>> ReturnCollectionRegistrationFunction() => new Task<IEnumerable<KeyValuePair<VKeys[], Func<Task>>>>(() =>
        {
            static IEnumerable<KeyValuePair<VKeys[], Func<Task>>> GetFunction()
            {
                foreach (var item in Tasks.FunctionsCallback)
                {
                    yield return item;
                }
            }
            return GetFunction();
        });

        public Task<bool> ContainsKeyComibantion(VKeys[] keyCombo) => new Task<bool>(() =>
        {
            return Tasks.FunctionsCallback.ContainsKey(keyCombo) is true;
        });

    }

}
