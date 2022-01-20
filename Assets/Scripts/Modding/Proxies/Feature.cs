using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [LuaHelpInfo("A feature object")]
    public class Feature : LuaProxy<Fab.Geo.Feature>
    {
        private Closure clickEvent;

        [LuaHelpInfo("The name of the feature")]
        public string name
        {
            get => Value.name;
            set => Value.SetName(value);
        }

        [LuaHelpInfo("The type of the feature (read only)")]
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
            get => Value.GetColor();
            set => Value.SetColor(value);
        }

        [LuaHelpInfo("The geometry of the feature as a list of coordinates (read only)")]
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

        [LuaHelpInfo("Resets this features style to its default state")]
        public void reset_style()
        {
            Value.ResetStyle();
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
