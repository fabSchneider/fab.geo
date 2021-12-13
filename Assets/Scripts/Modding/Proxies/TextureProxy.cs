using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class TextureProxy : ProxyBase<Texture2D>
    {
        public override string Name => "image";
        public override string Description => "A image object";

        [MoonSharpHidden]
        public TextureProxy(Texture2D source) : base(source) {}
        public string name => Value.name;
        public int width => Value.width;
        public int height => Value.height;

        [MoonSharpHidden]
        public Texture2D Texture => Value;

        public override string ToString()
        {
            if (IsNull())
                return "nil";

            return $"image {{name: {name}, width: {width}, height: {height}}}";
        }
    }
}
