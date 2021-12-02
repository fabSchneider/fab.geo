using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class TextureProxy : ProxyBase<Texture2D>
    {
        [MoonSharpHidden]
        public TextureProxy(Texture2D source) : base(source) {}
        public string name => Source.name;
        public int width => Source.width;
        public int height => Source.height;

        [MoonSharpHidden]
        public Texture2D Texture => Source;
    }
}
