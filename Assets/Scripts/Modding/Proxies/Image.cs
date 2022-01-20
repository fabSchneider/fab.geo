using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [LuaHelpInfo("An image object")]
    public class Image : LuaProxy<Texture2D>
    {
        [LuaHelpInfo("The name of the image")]
        public string name => Value.name;

        [LuaHelpInfo("The width of the image")]
        public int width => Value.width;

        [LuaHelpInfo("The height of the image")]
        public int height => Value.height;

        public override string ToString()
        {
            if (IsNil())
                return "nil";

            return $"image {{ name: {name}, width: {width}, height: {height} }}";
        }
    }
}
