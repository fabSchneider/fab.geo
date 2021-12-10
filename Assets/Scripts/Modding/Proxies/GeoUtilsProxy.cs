using MoonSharp.Interpreter;
using Unity.Mathematics;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class GeoProxy
    {
        public float distance(float lat_1, float lon_1, float lat_2, float lon_2)
        {
            return GeoUtils.Distance(
                new Coordinate(math.radians(lon_1), math.radians(lat_1)), 
                new Coordinate(math.radians(lon_2), math.radians(lat_2)));
        }

        public float distance(Coordinate coord1, Coordinate coord2)
        {
            return GeoUtils.Distance(coord1, coord2);
        }
    }
}
