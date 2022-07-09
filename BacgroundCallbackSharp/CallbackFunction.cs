using FVH.Background.Input;
using FVH.Background.InputHandler;

using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FVH.Background.Input
{
    internal class VKeysEqualityComparer : IEqualityComparer<VKeys[]>
    {
        public bool Equals(VKeys[]? x, VKeys[]? y)
        {
            if (x is null || y is null) return false;
            if (x.Length is 0 || y.Length is 0) return false;
            if (x.Length != y.Length) return false;
            if (x.SequenceEqual(y)) return true;
            if (x.Intersect(y).Count() == x.Length) return true;
            return false;
        }
        public int GetHashCode([DisallowNull] VKeys[] obj) => 0;

    }
    public class RegGroupFunction
    {
        internal RegGroupFunction(int group, VKeys[] keyCombination, List<RegFunction> listOfRegisteredFunctions)
        {
            Group = group;
            ListOfRegisteredFunctions = listOfRegisteredFunctions;
            KeyCombination = keyCombination;
        }
        public VKeys[] KeyCombination { get; }
        private int _group;
        public int Group
        {
            get { return _group; }
            private set { if (value < 0) throw new InvalidOperationException("The value for the group cannot be negative"); _group = value; }
        }
        public List<RegFunction> ListOfRegisteredFunctions { get; }
    }
    public record RegFunction
    {
        internal RegFunction(Func<Task> callBackTask, object? identifier = null)
        {
            CallBackTask = callBackTask;
            Identifier = identifier;
        }
        public object? Identifier { get; }
        public Func<Task> CallBackTask { get; }
    }
    internal class CallbackFunction : ICallBack
    {
        private readonly List<RegGroupFunction> GlobalList = new List<RegGroupFunction>();
        private readonly LowLevlHook _lowlevlhook;
        private readonly IHandler _keyboardHandler;
        private readonly Dictionary<VKeys[], Func<Task>> FunctionsCallback = new Dictionary<VKeys[], Func<Task>>(new VKeysEqualityComparer());

        public CallbackFunction(IHandler keyboardHandler, LowLevlHook lowLevlHook)
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
                RegGroupFunction? qR = GlobalList.SingleOrDefault(x => x.KeyCombination.Length == 1 & x.KeyCombination[0] == key);
                if (qR is null) return false;
                else
                {
                    await InvokeFunctions(qR.ListOfRegisteredFunctions);
                    return true;
                }
            }
            if (pressedKeys.Length is 0) return;
            if (pressedKeys.Length is 1)
            {
                await InvokeOneKey(e.Keys[0]);
                return;
            }
            static IEnumerable<VKeys> GetDifference(IEnumerable<VKeys> a, IEnumerable<VKeys> b)
            {
                List<VKeys> difColection = new List<VKeys>(a);
                b.ToList().ForEach(x => difColection.Remove(x));
                return difColection;
            }
            IEnumerable<RegGroupFunction> queryPrewievNotDuplicate = GlobalList.Where(x => x.KeyCombination.Length == pressedKeys.Length + 1).Where(x => x.KeyCombination.Except(pressedKeys).Count() == 1);
            IEnumerable<RegGroupFunction> queryPrewievDuplicate = GlobalList.Where(x => x.KeyCombination.Length == pressedKeys.Length + 1);           
            List<VKeys> myPreKeys = new List<VKeys>();
            if (queryPrewievNotDuplicate.Any() is false)
            {
                if (queryPrewievDuplicate.Any() is false) return;
                else
                {
                    foreach (RegGroupFunction x in queryPrewievDuplicate)
                    {
                        IEnumerable<VKeys> resultDifference = GetDifference(x.KeyCombination, pressedKeys);
                        if (resultDifference.Count() == 1) myPreKeys.Add(resultDifference.ToArray()[0]);
                    }
                }
            }
            else if (queryPrewievNotDuplicate.Any() is true)
            {
                IEnumerable<VKeys> preKeysGroup = queryPrewievNotDuplicate.Select(x => x.KeyCombination.Except(pressedKeys)).ToArray().Select(x => x.ToArray()[0]);

                VKeys? preKeyInput = await PreKeys(preKeysGroup);

                if (preKeyInput.HasValue is false) return;
                else
                {
                    RegGroupFunction invokeQuery = queryPrewievNotDuplicate.Single(x => x.KeyCombination.Intersect(new VKeys[] { preKeyInput.Value }).Count() == 1);
                    await InvokeFunctions(invokeQuery.ListOfRegisteredFunctions);
                }
            }
            if (myPreKeys.Count is 0) return;
            {
                VKeys? preKeyInput2 = await PreKeys(myPreKeys);

                if (preKeyInput2.HasValue is false) return;
                else
                {
                    RegGroupFunction invokeQuery = queryPrewievDuplicate.Single(x => x.KeyCombination.Intersect(new VKeys[] { preKeyInput2.Value }).Count() == 1);
                    await InvokeFunctions(invokeQuery.ListOfRegisteredFunctions);
                }
            }
        }

        private Task InvokeFunctions(IEnumerable<RegFunction> toTaskInvoke) //todo порверить и возможно перерабоать логику обработки исключений
        {
            if (toTaskInvoke.Any() is false) throw new InvalidOperationException("The collection cannot be empty");

            Parallel.Invoke(toTaskInvoke.Select(x => new Action(() =>
            {
                try
                {
                    x.CallBackTask.Invoke().Start();
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception)
                {

                }
            })).ToArray());

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
                VKeys? chekKey = null;
                if (key == VKeys.VK_LCONTROL || key == VKeys.VK_RCONTROL) // Заглушки из за разности между RawInput(не рапознает правый левый) и хуком, так как прдположительно нужно менять исходную библиотеку.
                {
                    chekKey = VKeys.VK_CONTROL;
                }
                else if (key == VKeys.VK_LMENU || key == VKeys.VK_RMENU)
                {
                    chekKey = VKeys.VK_MENU;
                }
                else if (key == VKeys.VK_LSHIFT || key == VKeys.VK_RSHIFT)
                {
                    chekKey = VKeys.VK_SHIFT;
                }
                else
                {
                    chekKey = key;
                }
                if (chekKey.HasValue is false) throw new InvalidOperationException();
                bool res1 = keys.Contains(chekKey.Value);
                if (keys.Contains(chekKey.Value))
                {
                    res = chekKey;
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

                RegGroupFunction? queryCotainGroup = GlobalList.SingleOrDefault(x => x.KeyCombination.SequenceEqual(keyCombo));
                if (queryCotainGroup is not null) queryCotainGroup.ListOfRegisteredFunctions.Add(new RegFunction(callbackTask, identifier));
                else
                {
                    RegGroupFunction newGroupF = new RegGroupFunction(++GoupCount, keyCombo, new List<RegFunction>());
                    newGroupF.ListOfRegisteredFunctions.Add(new RegFunction(callbackTask, identifier));
                    GlobalList.Add(newGroupF);
                }

                return Task.CompletedTask;
            }
        }
        public Task<bool> DeleteATaskByAnIdentifier(object? identifier = null)
        {
            lock (_lockMedthod)
            {
                if (identifier is null) return Task.FromResult(false);
                RegFunction? queryF = null;
                RegGroupFunction? queryResult = GlobalList.SingleOrDefault(x =>
                {
                    queryF = x.ListOfRegisteredFunctions.Single(fun => fun.Identifier == identifier);
                    return queryF is not null;
                });
                if (queryResult is null) return Task.FromResult(false);
                if (queryF is null) throw new InvalidOperationException();
                else
                {
                    if (queryResult.ListOfRegisteredFunctions.Remove(queryF) is not true) throw new InvalidOperationException();
                    else return Task.FromResult(true);
                }
            }
        }
        public Task<bool> DeleteAGroupByKeyСombination(VKeys[] keyCombo)
        {
            lock (_lockMedthod)
            {
                if (keyCombo.Length is 0) return Task.FromResult(false);
                RegGroupFunction? queyResult = GlobalList.SingleOrDefault(x => x.KeyCombination == keyCombo);
                if (queyResult is null) return Task.FromResult(false);
                if (GlobalList.Remove(queyResult) is not true) throw new InvalidOperationException();
                return Task.FromResult(true);
            }
        }

        public List<RegGroupFunction> ReturnGroupRegFunctions() => GlobalList.ToList();

        public Task<bool> ContainsKeyComibantion(VKeys[] keyCombo) => Task.FromResult(GlobalList.SingleOrDefault(x => x.KeyCombination == keyCombo) is not null);

        public Task<IHandler> GetHandler() => _keyboardHandler is not null ? Task.FromResult<IHandler>(_keyboardHandler) : Task.FromException<IHandler>(new NullReferenceException(nameof(_keyboardHandler)));
    }

}
