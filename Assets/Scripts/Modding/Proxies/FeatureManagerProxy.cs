using MoonSharp.Interpreter;
using Unity.Mathematics;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class FeatureManagerProxy : ProxyBase<FeatureManager>
    {
        public override string Name => "features";
        public override string Description => "Module for adding features (points, lines...) to the world";

        [MoonSharpHidden]
        public FeatureManagerProxy(FeatureManager value) : base(value) { }

        [LuaHelpInfo("Adds a point at the given coordinates")]
        public FeatureProxy add_point(string name, Coordinate coord)
        {
            FeaturePoint fp = Value.AddPoint(name, coord);
            return new FeatureProxy(fp);
        }

        [LuaHelpInfo("Adds a line between two coordinates")]
        public FeatureProxy add_line(string name, Coordinate coord1, Coordinate coord2)
        {
            FeatureLine fl = Value.AddLine(name, coord1, coord2);
            return new FeatureProxy(fl);
        }

        [LuaHelpInfo("Adds a line between two features")]
        public FeatureProxy add_line(string name, FeatureProxy feature_1, FeatureProxy feature_2)
        {
            FeatureLine fl = Value.AddLine(
                name,
                new Coordinate(math.radians(feature_1.center_lon), math.radians(feature_1.center_lat)),
                new Coordinate(math.radians(feature_2.center_lon), math.radians(feature_2.center_lat)));
            return new FeatureProxy(fl);
        }

        [LuaHelpInfo("Removes a feature from the world")]
        public bool remove(FeatureProxy feature)
        {
            return Value.RemoveFeature(feature.Value);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
