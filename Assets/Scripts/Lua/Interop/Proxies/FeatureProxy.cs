using Fab.Geo.Lua.Core;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
	public class FeatureProxyFactory : LuaProxyFactory<FeatureProxy, Feature> { }

	[LuaHelpInfo("A feature object")]
	[LuaName("feature")]
	public class FeatureProxy : LuaProxy<Feature>
	{
		private Closure clickEvent;

		[LuaHelpInfo("The name of the feature")]
		public string name
		{
			get => Target.name;
			set => Target.SetName(value);
		}

		[LuaHelpInfo("The type of the feature (read only)")]
		public string type
		{
			get
			{
				switch (Target)
				{
					case FeaturePoint:
						return "point";
					case FeatureLine:
						return "line";
					case FeaturePolyline:
						return "polyline";
					default:
						return "undefined";
				}
			}
		}

		[LuaHelpInfo("The main color of this feature")]
		public Color color
		{
			get => Target.GetColor();
			set => Target.SetColor(value);
		}

		[LuaHelpInfo("The geometry of the feature as a list of coordinates (read only)")]
		public Coordinate[] geometry => Target.Geometry;

		[LuaHelpInfo("Event function that is called when the feature is clicked")]
		public void on_click(Closure action)
		{
			clickEvent = action;
			Target.clicked -= OnClick;
			if (action != null)
				Target.clicked += OnClick;
		}

		[LuaHelpInfo("Removes this feature")]
		public void remove()
		{
			Target.Remove();
		}

		[LuaHelpInfo("Resets this features style to its default state")]
		public void reset_style()
		{
			Target.ResetStyle();
		}

		[MoonSharpHidden]
		public override string ToString()
		{
			if (IsNil())
				return "nil";

			return $"feature {{ type: {type}, name: {name} }}";
		}

		private void OnClick()
		{
			clickEvent.Call(this);
		}
	}
}
