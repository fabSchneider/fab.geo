using Fab.Lua.Core;
using System.Collections.Generic;

namespace Fab.Geo.Lua.Interop
{
	[LuaHelpInfo("Module for adding features (points, lines...) to the world")]
	public class Features : LuaObject, ILuaObjectInitialize
	{
		FeatureManager manager;

		public void Initialize()
		{
			manager = UnityEngine.Object.FindObjectOfType<FeatureManager>();

			if (!manager)
				throw new LuaObjectInitializationException("Features Manager could not be found");
		}

		[LuaHelpInfo("Adds a point at the given coordinates")]
		public Fab.Geo.Feature point(string name, Coordinate coord)
		{
			return manager.AddPoint(name, coord);
		}

		[LuaHelpInfo("Adds a line between two coordinates")]
		public Fab.Geo.Feature line(string name, Coordinate coord1, Coordinate coord2)
		{
			return manager.AddLine(name, coord1, coord2);
		}

		[LuaHelpInfo("Adds a line between two features")]
		public Fab.Geo.Feature line(string name, FeatureProxy feature_1, FeatureProxy feature_2)
		{
			return manager.AddLine(name, feature_1.geometry[0], feature_2.geometry[0]);
		}

		[LuaHelpInfo("Adds a polyline through a list of coordinates")]
		public Fab.Geo.Feature polyline(string name, ICollection<Coordinate> coords, bool closed = false)
		{
			return manager.AddPolyline(name, coords, closed);
		}

		[LuaHelpInfo("Gets the first feature with the given name")]
		public Fab.Geo.Feature get(string name)
		{
			return manager.GetFeature(name);
		}

		[LuaHelpInfo("Gets all feature with the given name")]
		public IEnumerable<Fab.Geo.Feature> get_all(string name)
		{
			return manager.GetFeatures(name);
		}

		[LuaHelpInfo("Removes a feature from the world")]
		public bool remove(FeatureProxy feature)
		{
			if (feature == null || feature.IsNil())
				return false;
			return manager.RemoveFeature(feature.Target);
		}

		[LuaHelpInfo("Removes all features from the world")]
		public void remove_all()
		{
			manager.RemoveAllFeatures();
		}
	}
}
