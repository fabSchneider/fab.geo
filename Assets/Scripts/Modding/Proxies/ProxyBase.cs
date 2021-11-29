using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class ProxyBase<T>
    {
        private T source;
        protected T Source => source;

        [MoonSharpHidden]
        public ProxyBase(T source)
        {
            this.source = source;
        }
    }
}
