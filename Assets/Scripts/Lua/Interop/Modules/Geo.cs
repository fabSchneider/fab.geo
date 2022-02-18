using Fab.Geo.Lua.Core;

namespace Fab.Geo.Lua.Interop
{
	[LuaHelpInfo("Module for geo operations")]
	public class Geo : LuaObject, ILuaObjectInitialize
	{
		public void Initialize() { }

		[LuaHelpInfo("Calculates the distance in kilometer between two coordinates")]
		public float distance(Coordinate coord1, Coordinate coord2)
		{
			return GeoUtils.Distance(coord1, coord2);
		}


	}
}
