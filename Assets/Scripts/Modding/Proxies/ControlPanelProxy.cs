using Fab.Geo.UI;
using MoonSharp.Interpreter;
using System.Collections.Generic;

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

        [LuaHelpInfo("Removes all controls from the panel")]
        public void remove_all() => Value.ClearAndHide();

        [LuaHelpInfo("Removes a control from the panel")]
        public void remove(string path) => Value.RemoveControl(path);

        [LuaHelpInfo("Adds a separator to the control panel")]
        public void add_separator(string path) => Value.AddSeparator(path);

        [LuaHelpInfo("Adds a slider to the control panel")]
        public void add_slider(string path, float min, float max, float value) => Value.AddSlider(path, min, max, value);

        [LuaHelpInfo("Adds a slider to the control panel. You can pass in a function that will be called when the value changes")]
        public void add_slider(string path, float min, float max, float value, Closure on_value_change) => Value.AddSlider(path, min, max, value, v => on_value_change.Call(v));

        [LuaHelpInfo("Adds a ranged slider to the control panel.")]
        public void add_range_slider(string path, float min, float max, float minLimit, float maxLimit) => Value.AddRangeSlider(path, min, max, minLimit, maxLimit);

        [LuaHelpInfo("Adds a choice field to the control panel")]
        public void add_choice(string path, List<string> choices, string value) => Value.AddChoice(path, choices, value);

        [LuaHelpInfo("Adds a button to the control panel. You can pass in a function that will be called when the button was pressed")]
        public void add_button(string path, Closure on_press) => Value.AddButton(path, () => on_press.Call());
    }
}
