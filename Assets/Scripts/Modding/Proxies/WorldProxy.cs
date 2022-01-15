using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class WorldProxy : ProxyBase<WorldInputHandler>
    {
        public override string Name => "world";

        public override string Description => "Module for interacting with the world";
       
        private Closure clickEvent;

        [MoonSharpHidden]
        public WorldProxy(WorldInputHandler value) : base(value)
        {
        }

        [LuaHelpInfo("Event function that is called when the world is clicked")]
        public void on_click(Closure action)
        {
            clickEvent = action;
            Value.clicked -= OnClick;
            if (action != null)
                Value.clicked += OnClick;
        }

        private void OnClick(Coordinate coord)
        {
            clickEvent.Call(coord);
        }
    }
}
