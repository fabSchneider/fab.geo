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

        public FeatureProxy add_line(string name, float lat1 = 0, float lon1 = 0, float lat2 = 0, float lon2 = 0)
        {
            FeatureLine fl = manager.AddLine(
                name, 
                new Coordinate(math.radians(lon1), math.radians(lat1)),
                new Coordinate(math.radians(lon2), math.radians(lat2)));
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
