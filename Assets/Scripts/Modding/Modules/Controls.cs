using Fab.Geo.UI;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Fab.Geo.Modding
{
    [LuaHelpInfo("Module for adding controls to the control panel")]
    public class Controls : LuaObject, ILuaObjectInitialize
    {
        private ControlPanel panel;
        private List<Control> controlProxies;

        public void Initialize()
        {
            UIManager manager = UnityEngine.Object.FindObjectOfType<UIManager>();

            if (!manager)
                throw new LuaObjectInitializationException("UI Manager could not be found");

            panel = manager.ControlPanel;     
            controlProxies = new List<Control>();
        }

        [LuaHelpInfo("Shows the control panel")]
        public void show() => panel.Show();

        [LuaHelpInfo("Hides the control panel")]
        public void hide() => panel.Hide();

        [LuaHelpInfo("Gets the control at the given path from the panel")]
        public Control get(string path)
        {
            return GetControlProxy(path);
        }

        [LuaHelpInfo("Removes the control at the given path from the panel")]
        public void remove(Control control)
        {
            if (controlProxies.Remove(control))
            {
                panel.RemoveControl(control.path);
                control.Dispose();
            }

        }

        [LuaHelpInfo("Removes all controls from the panel")]
        public void remove_all()
        {
            panel.ClearAndHide();

            foreach (var control in controlProxies)
                control.Dispose();
            controlProxies.Clear();
        }

        [LuaHelpInfo("Adds a label to the control panel")]
        public Control label(string path, string text)
        {
            VisualElement l = panel.AddLabel(path, text);
            LabelProxy proxy = GetControlProxy<LabelProxy>(path);
            if (proxy == null)
            {
                proxy = new LabelProxy(l, this, path);
                controlProxies.Add(proxy);

            }
            return proxy;
        }

        [LuaHelpInfo("Adds a separator to the control panel")]
        public Control separator(string path)
        {
            VisualElement s = panel.AddSeparator(path);
            SeparatorProxy proxy = GetControlProxy<SeparatorProxy>(path);
            if (proxy == null)
            {
                proxy = new SeparatorProxy(s, this, path);
                controlProxies.Add(proxy);

            }
            return proxy;
        }

        [LuaHelpInfo("Adds a slider to the control panel")]
        public Control slider(string path, float min, float max, float value)
        {
            Slider s = panel.AddSlider(path, min, max, value);
            SliderProxy proxy = GetControlProxy<SliderProxy>(path);
            if (proxy == null)
            {
                proxy = new SliderProxy(s, this, path);
                controlProxies.Add(proxy);
            }
            return proxy;

        }

        [LuaHelpInfo("Adds a ranged slider to the control panel.")]
        public Control range(string path, float min, float max, float minLimit, float maxLimit)
        {
            MinMaxSlider s = panel.AddRangeSlider(path, min, max, minLimit, maxLimit);
            SliderProxy proxy = GetControlProxy<SliderProxy>(path);
            if (proxy == null)
            {
                proxy = new SliderProxy(s, this, path);
                controlProxies.Add(proxy);
            }
            return proxy;
        }

        [LuaHelpInfo("Adds a choice field to the control panel")]
        public Control choice(string path, List<string> choices, string value)
        {
            DropdownField d = panel.AddChoice(path, choices, value);
            ChoiceProxy proxy = GetControlProxy<ChoiceProxy>(path);
            if (proxy == null)
            {
                proxy = new ChoiceProxy(d, this, path);
                controlProxies.Add(proxy);
            }
            return proxy;
        }

        [LuaHelpInfo("Adds a button to the control panel. You can pass in a function that will be called when the button was pressed")]
        public Control button(string path, string text, Closure on_click)
        {
            Button b = panel.AddButton(path, null);
            ButtonProxy proxy = GetControlProxy<ButtonProxy>(path);
            if (proxy == null)
            {
                proxy = new ButtonProxy(b, this, path);
                if (text != null)
                    ((Button)proxy.Target).text = text;
                if(on_click != null)
                    proxy.on_click(on_click);
                controlProxies.Add(proxy);
            }
            return proxy;
        }

        private Control GetControlProxy(string path)
        {
            return controlProxies.FirstOrDefault(c => c.path == path);
        }

        private T GetControlProxy<T>(string path) where T : Control
        {
            return controlProxies.OfType<T>().FirstOrDefault(c => c.path == path);
        }
    }
}
