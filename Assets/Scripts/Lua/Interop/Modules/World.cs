using Fab.Geo.Lua.Core;
using MoonSharp.Interpreter;

namespace Fab.Geo.Lua.Interop
{
    [LuaHelpInfo("Module for interacting with the world")]
    public class World : LuaObject, ILuaObjectInitialize
    {
        private WorldInputHandler worldInput;
        private Closure clickEvent;

        public void Initialize()
        {
            worldInput = UnityEngine.Object.FindObjectOfType<WorldInputHandler>();

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

        private void OnClick(Coordinate coord)
        {
            clickEvent.Call(coord);
        }
    }
}
