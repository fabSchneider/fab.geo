using Fab.Geo.UI;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    [LuaHelpInfo("Module for adding controls to the control panel")]
    public class ControlPanelProxy : ProxyBase<ControlPanel>
    {
        public override string Name => "controls";

        private List<ControlProxy> controlProxies;

        [MoonSharpHidden]
        public ControlPanelProxy(ControlPanel source) : base(source)
        {
            controlProxies = new List<ControlProxy>();
        }

        [LuaHelpInfo("Shows the control panel")]
        public void show() => Value.Show();

        [LuaHelpInfo("Hides the control panel")]
        public void hide() => Value.Hide();

        [LuaHelpInfo("Gets the control at the given path from the panel")]
        public ControlProxy get(string path)
        {
            return GetControlProxy(path);
        }

        [LuaHelpInfo("Removes the control at the given path from the panel")]
        public void remove(ControlProxy control)
        {
            if (controlProxies.Remove(control))
            {
                Value.RemoveControl(control.path);
                control.Dispose();
            }

        }

        [LuaHelpInfo("Removes all controls from the panel")]
        public void remove_all()
        {
            Value.ClearAndHide();

            foreach (var control in controlProxies)
                control.Dispose();
            controlProxies.Clear();
        }

        [LuaHelpInfo("Adds a separator to the control panel")]
        public ControlProxy add_separator(string path)
        {
            VisualElement s = Value.AddSeparator(path);
            SeparatorProxy proxy = GetControlProxy<SeparatorProxy>(path);
            if (proxy == null)
            {
                proxy = new SeparatorProxy(s, this, path);
                controlProxies.Add(proxy);

            }
            return proxy;
        }

        [LuaHelpInfo("Adds a slider to the control panel")]
        public ControlProxy add_slider(string path, float min, float max, float value)
        {
            Slider s = Value.AddSlider(path, min, max, value);
            SliderProxy proxy = GetControlProxy<SliderProxy>(path);
            if (proxy == null)
            {
                proxy = new SliderProxy(s, this, path);
                controlProxies.Add(proxy);
            }
            return proxy;

        }

        [LuaHelpInfo("Adds a ranged slider to the control panel.")]
        public ControlProxy add_range_slider(string path, float min, float max, float minLimit, float maxLimit)
        {
            MinMaxSlider s = Value.AddRangeSlider(path, min, max, minLimit, maxLimit);
            SliderProxy proxy = GetControlProxy<SliderProxy>(path);
            if (proxy == null)
            {
                proxy = new SliderProxy(s, this, path);
                controlProxies.Add(proxy);
            }
            return proxy;
        }

        [LuaHelpInfo("Adds a choice field to the control panel")]
        public ControlProxy add_choice(string path, List<string> choices, string value)
        {
            DropdownField d = Value.AddChoice(path, choices, value);
            ChoiceProxy proxy = GetControlProxy<ChoiceProxy>(path);
            if (proxy == null)
            {
                proxy = new ChoiceProxy(d, this, path);
                controlProxies.Add(proxy);
            }
            return proxy;
        }

        [LuaHelpInfo("Adds a button to the control panel. You can pass in a function that will be called when the button was pressed")]
        public ControlProxy add_button(string path)
        {
            Button b = Value.AddButton(path, null);
            ButtonProxy proxy = GetControlProxy<ButtonProxy>(path);
            if (proxy == null)
            {
                proxy = new ButtonProxy(b, this, path);
                controlProxies.Add(proxy);
            }
            return proxy;
        }

        private ControlProxy GetControlProxy(string path)
        {
            return controlProxies.FirstOrDefault(c => c.path == path);
        }

        private T GetControlProxy<T>(string path) where T : ControlProxy
        {
            return controlProxies.OfType<T>().FirstOrDefault(c => c.path == path);
        }
    }
}
