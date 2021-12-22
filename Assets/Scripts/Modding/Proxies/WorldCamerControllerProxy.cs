using MoonSharp.Interpreter;

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

        [LuaHelpInfo("Enables the camera's input control")]
        public void enable_control() => Value.ControlEnabled = true;

        [LuaHelpInfo("Disables the camera's input control")]
        public void disable_control() => Value.ControlEnabled = false;
    }
}