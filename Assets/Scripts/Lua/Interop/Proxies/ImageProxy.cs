using Fab.Lua.Core;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
	public class ImageProxyFactory : LuaProxyFactory<ImageProxy, Texture2D> { }

	[LuaHelpInfo("An image object")]
	[LuaName("image")]
	public class ImageProxy : LuaProxy<Texture2D>
	{
		[LuaHelpInfo("The name of the image")]
		public string name => Target.name;

		[LuaHelpInfo("The width of the image")]
		public int width => Target.width;

		[LuaHelpInfo("The height of the image")]
		public int height => Target.height;

		public override string ToString()
		{
			if (IsNil())
				return "nil";

			return $"image {{ name: {name}, width: {width}, height: {height} }}";
		}
	}
}
