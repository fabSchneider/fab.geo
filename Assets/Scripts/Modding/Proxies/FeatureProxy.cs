using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class FeatureProxy : ProxyBase<Feature>
    {
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
                    default:
                        return "undefined";
                }
            }
        }

        public float center_lat => Mathf.Rad2Deg * Value.Center.latitude;
        public float center_lon => Mathf.Rad2Deg * Value.Center.longitude;

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

            return $"feature {{type: {type}, name:{name}}}";
        }
    }
}
