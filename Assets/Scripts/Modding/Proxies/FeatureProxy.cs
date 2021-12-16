using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class FeatureProxy : ProxyBase<Feature>
    {
        public override string Name => "feature";
        public override string Description => "A feature object";

        private Closure clickEvent;

        [MoonSharpHidden]
        public FeatureProxy(Feature value) : base(value) { }

        public string name
        {
            get => Value.name;
            set => Value.SetName(name);
        }

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

        public Coordinate center => Value.Geometry[0];

        public void on_click(Closure action)
        {
            clickEvent = action;
            Value.clicked -= OnClick;
            if (action != null)
                Value.clicked += OnClick;
        }

        private void OnClick()
        {
            clickEvent.Call(this);
        }

        public override string ToString()
        {
            if (IsNull())
                return "nil";

            return $"feature {{ type: {type}, name: {name} }}";
        }
    }
}
