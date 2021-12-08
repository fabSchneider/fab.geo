using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class ProxyBase<T>
    {
        private T value;

        [MoonSharpHidden]
        public T Value => value;

        [MoonSharpHidden]
        public ProxyBase(T value)
        {
            this.value = value;
        }

        public bool IsNull()
        {
            if (value is Object obj)
                return !obj;

            return value == null;
        }
    }
}
