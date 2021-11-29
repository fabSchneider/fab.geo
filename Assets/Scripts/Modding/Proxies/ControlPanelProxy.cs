using MoonSharp.Interpreter;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class ControlPanelProxy : ProxyBase<ControlPanel>
    {
        [MoonSharpHidden]
        public ControlPanelProxy(ControlPanel source) : base(source) { }

        public void add_seperator() => Source.AddSeperator();

        public void begin_group(string name, bool expanded = true) => Source.BeginGroup(name, expanded);

        public void end_group() => Source.EndGroup();

        public void add_slider(string name, float min, float max, float value) => Source.AddSlider(name, min, max, value);

        public void add_slider(string name, float min, float max, float value, Closure callback) => Source.AddSlider(name, min, max, value, v => callback.Call(v));

        public void add_range_slider(string name, float min, float max, float minLimit, float maxLimit) => Source.AddRangeSlider(name, min, max, minLimit, maxLimit);

        public void add_choice(string name, List<string> choices, string value) => Source.AddChoice(name, choices, value);
    }
}
