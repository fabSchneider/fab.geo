using MoonSharp.Interpreter;

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
            FeaturePoint fp = manager.AddPoint(name, lat, lon);
            return new FeatureProxy(fp);
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
