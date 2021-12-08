using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class PopupProxy : ProxyBase<Popup>
    {
        [MoonSharpHidden]
        public PopupProxy(Popup value) : base(value){}

        public void show(string title, string text = null)
        {
            Value.Show(title, text);
        }

        public void show(string title, TextureProxy image)
        {
            Value.Show(title, image.Texture);
        }
    }
}
