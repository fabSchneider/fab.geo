using Fab.Geo.Lua.Core;
using Fab.Geo.UI;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
    [LuaHelpInfo("Module to show a popup on the screen")]
    public class Popup : LuaObject, ILuaObjectInitialize
    {
        private UI.Popup popup;
        public void Initialize()
        {
            UIManager manager = GameObject.FindObjectOfType<UIManager>();

            if(!manager)
                throw new LuaObjectInitializationException("UI Manager could not be found");

            popup = manager.Popup;
        }

        [LuaHelpInfo("Shows a popup with some text")]
        public void show(string title, string text = null)
        {
            popup.Show(title, text);
        }

        [LuaHelpInfo("Shows a popup with an image")]
        public void show(string title, ImageProxy image)
        {
            popup.Show(title, image.Target);
        }

        [LuaHelpInfo("Closes any open popup")]
        public void close()
        {
            popup.Close();
        }

        [LuaHelpInfo("Adds a button to the popup")]
        public Popup button(string text, Closure on_click)
        {
            popup.AddButton(text, () => on_click.Call());
            return this;
        }
    }
}
