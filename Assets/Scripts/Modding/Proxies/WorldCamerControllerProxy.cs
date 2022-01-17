using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    [LuaHelpInfo("Module for controlling the world camera")]
    public class WorldCameraControllerProxy : ProxyBase<WorldCameraController>
    {
        public override string Name => "camera";

        private Closure onAnimationFinished;

        [MoonSharpHidden]
        public WorldCameraControllerProxy(WorldCameraController value) : base(value) { }

        [LuaHelpInfo("Gets/Sets the camera's position in coordinates")]
        public Coordinate coord
        {
            get => Value.GetCoordinate();
            set => Value.SetCoordinate(value);
        }

        [LuaHelpInfo("Sets the camera's zoom level [0-1]")]
        public void set_zoom(float zoom)
        {
            Value.SetZoom(zoom);
        }

        [LuaHelpInfo("Enables the camera's input control")]
        public void enable_control() => Value.ControlEnabled = true;

        [LuaHelpInfo("Disables the camera's input control")]
        public void disable_control() => Value.ControlEnabled = false;
        
        [LuaHelpInfo("Moves the camera from one coordinate to the next in a list of coordinates")]
        public void animate(Coordinate[] coords, float speed, bool loop = false)
        {
            Value.Animate(coords, speed, loop);
        }

        [LuaHelpInfo("Called when a camera animation finished")]
        public void on_animation_finished(Closure evt)
        {
            onAnimationFinished = evt;
            Value.onAnimationFinished -= OnAnimationFinished;
            if(evt != null)
                Value.onAnimationFinished += OnAnimationFinished;
        }

        private void OnAnimationFinished()
        {
            if (onAnimationFinished != null)
                onAnimationFinished.Call();
        }
    }
}
