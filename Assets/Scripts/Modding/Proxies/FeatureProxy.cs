using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    [LuaHelpInfo("A feature object")]
    public class FeatureProxy : ProxyBase<Feature>
    {
        [MoonSharpHidden]
        public override string Name => "feature";

        private Closure clickEvent;

        [MoonSharpHidden]
        public FeatureProxy(Feature value) : base(value) { }

        [LuaHelpInfo("The name of the feature")]
        public string name
        {
            get => Value.name;
            set => Value.SetName(name);
        }

        [LuaHelpInfo("The type of the feature")]
        public string type
        {
            get
            {
                switch (Value)
                {
                    case FeaturePoint:
                        return "point";
                    case FeatureLine:
                        return "line";
                    default:
                        return "undefined";
                }
            }
        }

        [LuaHelpInfo("Returns the geometry of the feature as a list of coordinates")]
        public Coordinate[] geometry => Value.Geometry;

        [LuaHelpInfo("Event function that is called when the feature is clicked")]
        public void on_click(Closure action)
        {
            clickEvent = action;
            Value.clicked -= OnClick;
            if (action != null)
                Value.clicked += OnClick;
        }

        [LuaHelpInfo("Removes this feature")]
        public void remove()
        {
            Value.Remove();
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
