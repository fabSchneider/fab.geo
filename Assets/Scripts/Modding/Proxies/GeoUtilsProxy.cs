using MoonSharp.Interpreter;
using Unity.Mathematics;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class GeoProxy : ProxyBase
    {
        public override string Name => "geo";
        public override string Description => "Module for geo operations";

        [LuaHelpInfo("Calculates the distance in kilometer between two coordinates")]
        public float distance(Coordinate coord1, Coordinate coord2)
        {
            return GeoUtils.Distance(coord1, coord2);
        }
    }
}
