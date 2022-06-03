using FVH.Background.Input;
using FVH.Background.InputHandler;

using System.Diagnostics.CodeAnalysis;

namespace FVH.Background.Input
{
    internal class VKeysEqualityComparer : IEqualityComparer<VKeys[]>
    {
        public bool Equals(VKeys[]? x, VKeys[]? y) => x is not null && y is not null && x.Length is not 0 && y.Length is not 0 && x.SequenceEqual(y);
        public int GetHashCode([DisallowNull] VKeys[] obj) => 0;

    }
    internal class RegGroupFunction
    {
        public RegGroupFunction(int group, VKeys[] keyCombination, List<RegFunction> listOfRegisteredFunctions)
        {
            Group = group;
            ListOfRegisteredFunctions = listOfRegisteredFunctions;
            KeyCombination = keyCombination;
        }
        internal VKeys[] KeyCombination { get; }
        internal int Group { get; private set; }
        internal List<RegFunction> ListOfRegisteredFunctions { get; }
    }
    internal record RegFunction
    {
        public RegFunction(Func<Task> callBackTask, object? identifier = null)
        {
            CallBackTask = callBackTask;
            Identifier = identifier;
        }
        internal object? Identifier { get; }
        internal Func<Task> CallBackTask { get; }
    }
    internal class CallbackFunction : ICallBack
    {
        private readonly List<RegGroupFunction> GlobalList = new List<RegGroupFunction>();
        private readonly LowLevlHook _lowlevlhook;
        private readonly KeyboardHandler _keyboardHandler;
        private readonly Dictionary<VKeys[], Func<Task>> FunctionsCallback = new Dictionary<VKeys[], Func<Task>>(new VKeysEqualityComparer());

        public CallbackFunction(KeyboardHandler keyboardHandler, LowLevlHook lowLevlHook)
        {
            _keyboardHandler = keyboardHandler;
            _lowlevlhook = lowLevlHook;
            _keyboardHandler.KeyPressEvent += KeyboardHandler_KeyPressEvent;
        }
        private async void KeyboardHandler_KeyPressEvent(object? sender, DataKeysNotificator e)
        {
            VKeys[] pressedKeys = e.Keys;
            async Task<bool> InvokeOneKey(VKeys key)
            {
                //Func<Task>[] resultOneInvoke = FunctionsCallback.Keys.Where(x => x.Length == 1 & x[0] == key).Select(x => FunctionsCallback[x]).ToArray();
                IEnumerable<VKeys[]>? resultOneInvoke = FunctionsCallback.Keys.Where(x => x.Length == pressedKeys.Length).Where(x => x.Intersect(pressedKeys).Count() == x.Length);
                IEnumerable<KeyValuePair<VKeys[], Func<Task>>> onekey = FunctionsCallback.Where(x => x.Key == resultOneInvoke.ToArray()[0]);
                if (resultOneInvoke.Count() == 0) return false;
                await InvokFunctions(onekey);
                return true;
            }
            if (pressedKeys.Length is 0 || (pressedKeys.Length is 1 && await InvokeOneKey(e.Keys[0]) is true)) return;

            pressedKeys = new VKeys[]
            {
                 VKeys.VK_SPACE,
                 VKeys.VK_SHIFT,
                 VKeys.VK_CONTROL
            };


            //IEnumerable<VKeys[]>? hhh = FunctionsCallback.Keys.Where(x => x.Length == pressedKeys.Length).Where(x => x.Intersect(pressedKeys).Count() == x.Length);
            //IEnumerable<KeyValuePair<VKeys[], Func<Task>>> rnmn = FunctionsCallback.Where(x => x.Key == hhh.ToArray()[0]);


            // var abcdg = GetFunctionToInvokeIdentical(hhh);

            List<VKeys> listPreKeys = new List<VKeys>();

            //List<VKeys[]> fullV = new List<VKeys[]>();
            //List<VKeys[]> keys = FunctionsCallback.Keys.Where(itemKeyArray => 
            //{
            //    for (int i = 0; i < pressedKeys.Length; i++)
            //    {
            //        if (itemKeyArray.Any((xVkey) => xVkey == pressedKeys[i]) is not true) return false;
            //        else
            //        {
            //            VKeys[] keyPre = itemKeyArray.Except(pressedKeys).ToArray();
            //            if (keyPre.Length > 1) return false;
            //            else
            //            {
            //                if (keyPre.Length is 0)
            //                {
            //                    //  fullV.Add(itemKeyArray); // Вообще надо бы сделать более эфективную с логической точки срения обработку.
            //                    return false;
            //                };
            //                listPreKeys.Add(keyPre[0]);
            //            }
            //        }
            //    }
            //    return true;
            //}).Where(x => x.Length == pressedKeys.Length).ToList();

            //if (fullV.Count is 1 & listPreKeys.Count is 0)
            //{
            //    keys.Clear();
            //    keys.Add(fullV.ToArray()[0]);
            //}

            //var listPreKeys2 = listPreKeys.Distinct().ToList();
            //if (listPreKeys2.Count > 0)
            //{
            //    VKeys? callhook = await PreKeys(listPreKeys).ConfigureAwait(false);
            //    if (callhook.HasValue is true)
            //    {
            //        VKeys[] preseedKeys1 = pressedKeys.Append(callhook.Value).ToArray();

            //        keys = FunctionsCallback.Keys.Where(x =>
            //        {
            //            for (int i = 0; i < preseedKeys1.Length; i++)
            //            {
            //                if (x.Any((x) => x == preseedKeys1[i]) is not true)
            //                {
            //                    return false;
            //                }
            //            }
            //            return true;
            //        }).Where(x => x.Length == pressedKeys.Length + 1).ToList();
            //    }
            //}

            //if (keys.Count is 0) return;
            //if (keys.Count > 1) throw new InvalidOperationException();


            //await InvokFunctions(FunctionsCallback[keys[0]]);
        }

        //private Task InvokFunctions(Func<Task> function)
        //{
        //    try
        //    {
        //        function.Invoke().Start();}              catch (InvalidOperationException)
        //    {
        //        throw;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //    return Task.CompletedTask;
        //}
        private Task InvokFunctions(IEnumerable<KeyValuePair<VKeys[], Func<Task>>> toTaskInvoke)
        {
            Action[] r = toTaskInvoke.AsParallel().Select(x => new Action(() =>
            {
                try
                {
                    x.Value.Invoke().Start();
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception)
                {

                }

            })).ToArray();
            Parallel.Invoke(r);
            return Task.CompletedTask;
        }

        private async Task<VKeys?> PreKeys(IEnumerable<VKeys> keys)
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


        private int GoupCount = 0;
        private readonly object _lockMedthod = new object();
        public Task AddCallBackTask(VKeys[] keyCombo, Func<Task> callbackTask, object? identifier = null)
        {
            lock (_lockMedthod)
            {

                if (keyCombo.Length is 0) throw new ArgumentException($"The number of keys cannot be zero");

                FunctionsCallback.Add(keyCombo, callbackTask);

                RegGroupFunction? queryCotainGroup = GlobalList.SingleOrDefault(x => x.KeyCombination == keyCombo);
                if (queryCotainGroup is not null) queryCotainGroup.ListOfRegisteredFunctions.Add(new RegFunction(callbackTask, identifier));
                else
                {
                    RegGroupFunction newGroupF = new RegGroupFunction(++GoupCount, keyCombo, new List<RegFunction>());
                    newGroupF.ListOfRegisteredFunctions.Add(new RegFunction(callbackTask, identifier));                      
                    GlobalList.Add(new RegGroupFunction(++GoupCount, keyCombo, new List<RegFunction>())); 
                }

                return Task.CompletedTask;
            }
        }
        public Task<bool> RemoveCallBackTask(object? identifier = null)
        {
            if (identifier is null) return Task.FromResult(false);
            RegFunction? queryF = null;
            RegGroupFunction? queryResult = GlobalList.SingleOrDefault(x => 
            {
                queryF = x.ListOfRegisteredFunctions.Single(fun => fun.Identifier == identifier);
                return queryF is not null;
            });
            if(queryResult is null) return Task.FromResult(false);
            if (queryF is null) throw new InvalidOperationException();
            else 
            {
               if(queryResult.ListOfRegisteredFunctions.Remove(queryF) is not true) throw new InvalidOperationException();
               else return Task.FromResult(true);
            }      
        }

        public Task<IEnumerable<KeyValuePair<VKeys[], Func<Task>>>> ReturnRegisteredFunctions() => new Task<IEnumerable<KeyValuePair<VKeys[], Func<Task>>>>(() =>
        {
            IEnumerable<KeyValuePair<VKeys[], Func<Task>>> GetFunction()
            {
                foreach (var item in FunctionsCallback)
                {
                    yield return item;
                }
            }
            return GetFunction();
        });

        public Task<bool> ContainsKeyComibantion(VKeys[] keyCombo) => Task.FromResult(FunctionsCallback.ContainsKey(keyCombo));

    }

}
