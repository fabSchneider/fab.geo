using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class UIManagerProxy
    {
        private UIManager manager;

        [MoonSharpHidden]
        public UIManagerProxy(UIManager manager)
        {
            this.manager = manager;
        }

        public void popup(string title, string text)
        {
            manager.Popup(title, text);
        }
    }
}
