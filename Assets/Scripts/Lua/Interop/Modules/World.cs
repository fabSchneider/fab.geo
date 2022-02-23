using Fab.Geo.Lua.Core;
using MoonSharp.Interpreter;

namespace Fab.Geo.Lua.Interop
{
	[LuaHelpInfo("Module for interacting with the world")]
	public class World : LuaObject, ILuaObjectInitialize
	{
		private WorldInputHandler worldInput;
		private Fab.Geo.World world;
		private Closure clickEvent;

		public void Initialize()
		{
			worldInput = UnityEngine.Object.FindObjectOfType<WorldInputHandler>();
			world = UnityEngine.Object.FindObjectOfType<Fab.Geo.World>();

			if (worldInput == null)
				throw new LuaObjectInitializationException("Could not find world input");
		}

		[LuaHelpInfo("Event function that is called when the world is clicked")]
		public void on_click(Closure action)
		{
			clickEvent = action;
			worldInput.clicked -= OnClick;
			if (action != null)
				worldInput.clicked += OnClick;
		}

		[LuaHelpInfo("Returns the radius of the earth in kilometers")]
		public int radius => GeoUtils.EARTH_RADIUS_KM;

		[LuaHelpInfo("Returns the world's altitude at a given longitude and latitude")]
		public float altitude(float lon, float lat)
		{
			return world.GetAltitude(lon, lat);
		}

		[LuaHelpInfo("Get/Set the scaling of the world terrain")]
		public float height_scale
		{
			get => world.HeightScale;
			set => world.HeightScale = value;
		}

		[LuaHelpInfo("Get/Set the oceans water level [0 to 1]")]
		public float water_level
		{
			get => world.WaterLevel;
			set => world.WaterLevel = value;
		}

		public float overlay_strength
		{
			get => world.OverlayStrength;
			set => world.OverlayStrength = value;
		}

		public float overlay_scale
		{
			get => world.OverlayScale;
			set => world.OverlayScale = value;
		}

		private void OnClick(Coordinate coord)
		{
			clickEvent.Call(coord);
		}
	}
}
