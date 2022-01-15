using MoonSharp.Interpreter;
using System;
using UnityEngine.UIElements;

namespace Fab.Geo.Modding
{
    public abstract class ControlProxy : ProxyBase<VisualElement>, IDisposable
    {
        protected string controlPath;

        [LuaHelpInfo("The path of the control in the panel")]
        public string path
        {
            get
            {
                ThrowIfNil();
                return controlPath;
            }
        }

        private ControlPanelProxy panel;

        [MoonSharpHidden]
        public ControlProxy(VisualElement value, ControlPanelProxy panel, string path) : base(value)
        {
            controlPath = path;
            this.panel = panel;
        }

        [LuaHelpInfo("Removes the control from the panel")]
        public void remove()
        {
            ThrowIfNil();
            panel.remove(this);
        }

        public virtual void Dispose()
        {
            value = null;
            controlPath = null;
        }

        public override string ToString()
        {
            if (IsNil())
                return "nil";

            return $"{Name} {{ path: {path} }}";
        }
    }

    [LuaHelpInfo("A button control")]
    public class ButtonProxy : ControlProxy
    {
        private Closure onClick;

        [MoonSharpHidden]
        public ButtonProxy(VisualElement value, ControlPanelProxy panel, string path) : base(value, panel, path) { }

        public override string Name => "button";

        [LuaHelpInfo("Add a function to be executed when the button is clicked")]
        public void on_click(Closure callback)
        {
            ThrowIfNil();

            if (Value is Button button)
            {
                onClick = callback;
                button.clickable.clicked -= OnClick;
                if (onClick != null)
                    button.clickable.clicked += OnClick;
            }
        }

        private void OnClick()
        {
            onClick.Call();
        }

        public override void Dispose()
        {
            base.Dispose();
            onClick = null;
        }
    }

    [MoonSharpUserData]
    [LuaHelpInfo("A slider control")]
    public class SliderProxy : ControlProxy
    {
        private Closure onValueChange;

        [LuaHelpInfo("The currently value of the slider")]
        public float val => ((Slider)Value).value;

        [MoonSharpHidden]
        public SliderProxy(VisualElement value, ControlPanelProxy panel, string path) : base(value, panel, path)
        {
        }

        public override string Name => "slider";

        [LuaHelpInfo("Add a function to be executed when the value of this slider changes")]
        public void on_change(Closure callback)
        {
            ThrowIfNil();

            onValueChange = callback;

            Value.UnregisterCallback<ChangeEvent<float>>(OnValueChange);
            if (onValueChange != null)
                Value.RegisterCallback<ChangeEvent<float>>(OnValueChange);
        }

        private void OnValueChange(ChangeEvent<float> evt)
        {
            onValueChange.Call(evt.newValue);
        }

        public override void Dispose()
        {
            base.Dispose();
            onValueChange = null;
        }
    }

    [MoonSharpUserData]
    [LuaHelpInfo("A seperator")]
    public class SeparatorProxy : ControlProxy
    {
        public SeparatorProxy(VisualElement value, ControlPanelProxy panel, string path) : base(value, panel, path)
        {
        }

        public override string Name => "separator";
    }

    [MoonSharpUserData]
    [LuaHelpInfo("A choice control")]
    public class ChoiceProxy : ControlProxy
    {
        private Closure onValueChange;

        [LuaHelpInfo("The currently selected choice")]
        public string val => ((DropdownField)Value).value;

        [MoonSharpHidden]
        public ChoiceProxy(VisualElement value, ControlPanelProxy panel, string path) : base(value, panel, path)
        {
        }

        public override string Name => "choice";

        [LuaHelpInfo("Add a function to be executed when the selected choice changes")]
        public void on_change(Closure callback)
        {
            ThrowIfNil();

            onValueChange = callback;

            Value.UnregisterCallback<ChangeEvent<string>>(OnValueChange);
            if (onValueChange != null)
                Value.RegisterCallback<ChangeEvent<string>>(OnValueChange);
        }

        private void OnValueChange(ChangeEvent<string> evt)
        {
            onValueChange.Call(evt.newValue);
        }

        public override void Dispose()
        {
            base.Dispose();
            onValueChange = null;
        }
    }
}
