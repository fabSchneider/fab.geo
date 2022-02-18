using Fab.Geo.Lua.Core;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
	[LuaHelpInfo("Module for controlling the world camera")]
	public class Camera : LuaObject, ILuaObjectInitialize
	{
		private WorldCameraController cameraController;
		private Closure onAnimationFinished;
		public void Initialize()
		{
			cameraController = Object.FindObjectOfType<WorldCameraController>();

			if (!cameraController)
				throw new LuaObjectInitializationException("Could not find camera controller");
		}

		[LuaHelpInfo("Gets/Sets the camera's position in coordinates")]
		public Coordinate coord
		{
			get => cameraController.GetCoordinate();
			set => cameraController.SetCoordinate(value);
		}

		[LuaHelpInfo("Sets the camera's zoom level [0-1]")]
		public void set_zoom(float zoom)
		{
			cameraController.SetZoom(zoom);
		}

		[LuaHelpInfo("Enables the camera's input control")]
		public void enable_control() => cameraController.ControlEnabled = true;

		[LuaHelpInfo("Disables the camera's input control")]
		public void disable_control() => cameraController.ControlEnabled = false;

		[LuaHelpInfo("Moves the camera from one coordinate to the next in a list of coordinates")]
		public void animate(Coordinate[] coords, float speed, bool loop = false)
		{
			cameraController.Animate(coords, speed, loop);
		}

		[LuaHelpInfo("Called when a camera animation finished")]
		public void on_animation_finished(Closure evt)
		{
			onAnimationFinished = evt;
			cameraController.onAnimationFinished -= OnAnimationFinished;
			if (evt != null)
				cameraController.onAnimationFinished += OnAnimationFinished;
		}

		private void OnAnimationFinished()
		{
			if (onAnimationFinished != null)
				onAnimationFinished.Call();
		}
	}
}
