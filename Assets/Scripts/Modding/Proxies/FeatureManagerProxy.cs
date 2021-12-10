using MoonSharp.Interpreter;
using Unity.Mathematics;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class FeatureManagerProxy
    {
        private FeatureManager manager;

        [MoonSharpHidden]
        public FeatureManagerProxy(FeatureManager manager)
        {
            this.manager = manager;
        }

        public FeatureProxy add_point(string name, float lat = 0, float lon = 0)
        {
            FeaturePoint fp = manager.AddPoint(name, new Coordinate(math.radians(lon), math.radians(lat)));
            return new FeatureProxy(fp);
        }

        public FeatureProxy add_line(string name, float lat_1 = 0, float lon_1 = 0, float lat_2 = 0, float lon_2 = 0)
        {
            FeatureLine fl = manager.AddLine(
                name, 
                new Coordinate(math.radians(lon_1), math.radians(lat_1)),
                new Coordinate(math.radians(lon_2), math.radians(lat_2)));
            return new FeatureProxy(fl);
        }

        public FeatureProxy add_line(string name, FeatureProxy feature_1, FeatureProxy feature_2)
        {
            FeatureLine fl = manager.AddLine(
                name,
                new Coordinate(math.radians(feature_1.center_lon), math.radians(feature_1.center_lat)),
                new Coordinate(math.radians(feature_2.center_lon), math.radians(feature_2.center_lat)));
            return new FeatureProxy(fl);
        }

        public bool remove(FeatureProxy feature)
        {
            return manager.RemoveFeature(feature.Value);
        }

        public override string ToString()
        {
            return "features";
        }
    }
}
