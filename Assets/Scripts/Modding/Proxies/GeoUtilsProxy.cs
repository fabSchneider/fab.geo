using MoonSharp.Interpreter;
using Unity.Mathematics;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    [LuaHelpInfo("Module for geo operations")]
    public class GeoProxy : ProxyBase
    {
        public override string Name => "geo";

        [LuaHelpInfo("Calculates the distance in kilometer between two coordinates")]
        public float distance(Coordinate coord1, Coordinate coord2)
        {
            return GeoUtils.Distance(coord1, coord2);
        }
    }
}
