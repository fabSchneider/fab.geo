using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class PopupProxy : ProxyBase<Popup>
    {
        [MoonSharpHidden]
        public PopupProxy(Popup source) : base(source){}

        public void show(string title, string text)
        {
            Source.Show(title, text);
        }

        public void show(string title, TextureProxy image)
        {
            Source.Show(title, image.Texture);
        }
    }
}
