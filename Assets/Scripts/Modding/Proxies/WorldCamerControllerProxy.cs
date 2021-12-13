using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class WorldCameraControllerProxy : ProxyBase<WorldCameraController>
    {
        public override string Name => "camera";
        public override string Description => "Module for controlling the world camera";

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
    }
}
