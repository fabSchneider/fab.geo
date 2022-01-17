using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    [LuaHelpInfo("Module for adding features (points, lines...) to the world")]
    public class FeatureManagerProxy : ProxyBase<FeatureManager>
    {
        public override string Name => "features";

        [MoonSharpHidden]
        public FeatureManagerProxy(FeatureManager value) : base(value) { }

        [LuaHelpInfo("Adds a point at the given coordinates")]
        public FeatureProxy point(string name, Coordinate coord)
        {
            FeaturePoint fp = Value.AddPoint(name, coord);
            return new FeatureProxy(fp);
        }

        [LuaHelpInfo("Adds a line between two coordinates")]
        public FeatureProxy line(string name, Coordinate coord1, Coordinate coord2)
        {
            Feature fl = Value.AddLine(name, coord1, coord2);
            return new FeatureProxy(fl);
        }

        [LuaHelpInfo("Adds a line between two features")]
        public FeatureProxy line(string name, FeatureProxy feature_1, FeatureProxy feature_2)
        {
            Feature fl = Value.AddLine(
                name,
                feature_1.geometry[0], feature_2.geometry[0]);
            return new FeatureProxy(fl);
        }

        [LuaHelpInfo("Adds a polyline through a list of coordinates")]
        public FeatureProxy polyline(string name, ICollection<Coordinate> coords, bool closed = false)
        {
            Feature fl = Value.AddPolyline(name, coords, closed);
            return new FeatureProxy(fl);
        }

        [LuaHelpInfo("Gets the first feature with the given name")]
        public FeatureProxy get(string name)
        {
            Feature feature = Value.GetFeature(name);
            if (feature == null)
                throw new ArgumentException("No feature with that name found");

            return new FeatureProxy(feature);
        }

        [LuaHelpInfo("Gets all feature with the given name")]
        public FeatureProxy[] get_all(string name)
        {
            IEnumerable<Feature> features = Value.GetFeatures(name);
            if (features == null)
                throw new ArgumentException("No feature with that name found");

            return features.Select(f => new FeatureProxy(f)).ToArray();
        }

        [LuaHelpInfo("Removes a feature from the world")]
        public bool remove(FeatureProxy feature)
        {
            if (feature == null || feature.IsNil())
                return false;
            return Value.RemoveFeature(feature.Value);
        }

        [LuaHelpInfo("Removes all features from the world")]
        public void remove_all()
        {
            Value.RemoveAllFeatures();
        }
    }
}
