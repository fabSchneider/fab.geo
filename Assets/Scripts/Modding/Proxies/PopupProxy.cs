using Fab.Geo.UI;
using MoonSharp.Interpreter;
using System.Collections.Generic;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    [LuaHelpInfo("Module to show popups on the screen")]
    public class PopupProxy : ProxyBase<Popup>
    {
        public override string Name => "popup";

        [MoonSharpHidden]
        public PopupProxy(Popup value) : base(value){}

        [LuaHelpInfo("Shows a popup with some text")]
        public void show(string title, string text = null)
        {
            Value.Show(title, text);
        }

        [LuaHelpInfo("Shows a popup with an image")]
        public void show(string title, TextureProxy image)
        {
            Value.Show(title, image.Texture);
        }

        [LuaHelpInfo("Closes any open popup")]
        public void close()
        {
            Value.Close();
        }

        [LuaHelpInfo("Adds a button to the popup")]
        public PopupProxy button(string text, Closure on_click)
        { 
            Value.AddButton(text, () => on_click.Call());
            return this;
        }
    }
}
