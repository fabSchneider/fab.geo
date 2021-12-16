using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class PopupProxy : ProxyBase<Popup>
    {
        public override string Name => "popup";
        public override string Description => "Module to show popups on the screen";

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
    }
}
