using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FVH.Background.Input
{
    /// <summary>
    /// <br><see langword="En"/></br>
    ///<br/>This interface declares a contract for the keyboard input registration class. 
    ///<br><see langword="Ru"/></br>
    ///<br>Этот интерфейс объявляет контракт для класса регистрации ввода с клавиатуры.</br>
    ///<code Language ="cs">  
    ///<a>Example:</a>
    ///<br>
    ///<see cref="Input"/> <paramref name="input"/> = <see langword="new"/>();
    ///</br>
    ///<br>
    ///<see cref="IKeyboardCallBack"/> <paramref name="input"/> = <see langword="await"/> <paramref name="input"/>.Subscribe();
    ///</br>
    ///</code>    
    ///</summary>

    public interface IKeyboardCallBack
    {
        public Task AddCallBackTask(VKeys[] keyCombo, Func<Task> callbackTask, object? identifier = null);
        public Task<bool> DeleteATaskByAnIdentifier(object? identifier = null);
        public Task<bool> ContainsKeyComibantion(VKeys[] keyCombo);
        public List<RegFunctionGroupKeyboard> ReturnGroupRegFunctions();
    }
}
