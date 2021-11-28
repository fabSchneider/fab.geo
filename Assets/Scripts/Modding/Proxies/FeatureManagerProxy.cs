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

        public FeatureProxy addPoint(string name, float lat, float lon)
        {
            FeaturePoint fp = manager.AddPoint(name, lat, lon);
            return new FeatureProxy(fp);
        }

        public bool removeFeature(int id)
        {
            return manager.RemoveFeature(id);
        }
    }
}
