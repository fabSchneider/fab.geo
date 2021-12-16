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

        [LuaHelpInfo("Shows the control panel")]
        public void show() => Value.Show();

        [LuaHelpInfo("Hides the control panel")]
        public void hide() => Value.Hide();

        [LuaHelpInfo("Adds a separator to the control panel")]
        public void add_separator() => Value.AddSeperator();

        [LuaHelpInfo("Starts a group in the control panel. Every following element will be added to this group.")]
        public void begin_group(string name) => Value.BeginGroup(name, true);
        
        [LuaHelpInfo("Ends the current group.")]
        public void end_group() => Value.EndGroup();

        [LuaHelpInfo("Adds a slider to the control panel")]
        public void add_slider(string name, float min, float max, float value) => Value.AddSlider(name, min, max, value);

        [LuaHelpInfo("Adds a slider to the control panel. You can pass in a function that will be called when the value changes")]
        public void add_slider(string name, float min, float max, float value, Closure on_value_change) => Value.AddSlider(name, min, max, value, v => on_value_change.Call(v));

        [LuaHelpInfo("Adds a range slider to the control panel.")]
        public void add_range_slider(string name, float min, float max, float minLimit, float maxLimit) => Value.AddRangeSlider(name, min, max, minLimit, maxLimit);

        [LuaHelpInfo("Adds a choice field to the control panel")]
        public void add_choice(string name, List<string> choices, string value) => Value.AddChoice(name, choices, value);

        [LuaHelpInfo("Adds a button to the control panel. You can pass in a function that will be called when the button was pressed")]
        public void add_button(string name, Closure on_press) => Value.AddButton(name, () => on_press.Call());
    }
}
