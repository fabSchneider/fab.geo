using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    [LuaHelpInfo("An image object")]
    public class TextureProxy : ProxyBase<Texture2D>
    {
        public override string Name => "image";

        [MoonSharpHidden]
        public TextureProxy(Texture2D source) : base(source) {}
        public string name => Value.name;
        public int width => Value.width;
        public int height => Value.height;

        [MoonSharpHidden]
        public Texture2D Texture => Value;

        public override string ToString()
        {
            if (IsNil())
                return "nil";

            return $"image {{ name: {name}, width: {width}, height: {height} }}";
        }
    }
}
