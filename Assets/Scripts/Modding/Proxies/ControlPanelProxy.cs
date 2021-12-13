using MoonSharp.Interpreter;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class ControlPanelProxy : ProxyBase<ControlPanel>
    {
        public override string Name => "controls";
        public override string Description => "Module for adding controls to the control panel";

        [MoonSharpHidden]
        public ControlPanelProxy(ControlPanel source) : base(source) { }

        public void add_seperator() => Value.AddSeperator();

        public void begin_group(string name, bool expanded = true) => Value.BeginGroup(name, expanded);

        public void end_group() => Value.EndGroup();

        public void add_slider(string name, float min, float max, float value) => Value.AddSlider(name, min, max, value);

        public void add_slider(string name, float min, float max, float value, Closure callback) => Value.AddSlider(name, min, max, value, v => callback.Call(v));

        public void add_range_slider(string name, float min, float max, float minLimit, float maxLimit) => Value.AddRangeSlider(name, min, max, minLimit, maxLimit);

        public void add_choice(string name, List<string> choices, string value) => Value.AddChoice(name, choices, value);

        public void add_button(string name, Closure callback) => Value.AddButton(name, () => callback.Call());
    }
}
