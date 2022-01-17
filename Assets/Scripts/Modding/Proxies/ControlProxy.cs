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

        [LuaHelpInfo("The enabled state of the control")]
        public bool enabled
        {
            get => Value.enabledSelf;
            set => Value.SetEnabled(value);
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

        [LuaHelpInfo("Moves the control up in the hierachy")]
        public void move_up()
        {
            int index = Value.parent.IndexOf(Value);
            if (index == 0)
                return;

            Value.PlaceBehind(Value.parent.ElementAt(index - 1));
        }

        [LuaHelpInfo("Moves the control down in the hierachy")]
        public void move_down()
        {
            int index = Value.parent.IndexOf(Value);
            if (index == Value.parent.childCount - 1)
                return;

            Value.PlaceInFront(Value.parent.ElementAt(index + 1));
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

    [MoonSharpUserData]
    [LuaHelpInfo("A button control")]
    public class ButtonProxy : ControlProxy
    {
        private Closure onClick;

        [MoonSharpHidden]
        public ButtonProxy(VisualElement value, ControlPanelProxy panel, string path) : base(value, panel, path) { }

        [MoonSharpHidden]
        public override string Name => "button";

        [LuaHelpInfo("The text of the button")]
        public string text
        {
            get => ((Button)Value).text;
            set => ((Button)Value).text = value;
        }

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

        [MoonSharpHidden]
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

        [MoonSharpHidden]
        public override string Name => "separator";
    }

    [MoonSharpUserData]
    [LuaHelpInfo("A label")]
    public class LabelProxy : ControlProxy
    {
        [LuaHelpInfo("The text of the label")]
        public string text
        {
            get => ((Label)Value).text;
            set => ((Label)Value).text = value;
        }

        [LuaHelpInfo("The text size of the label")]
        public float size
        {
            get => ((Label)Value).style.fontSize.value.value;
            set => ((Label)Value).style.fontSize = new Length(value, LengthUnit.Pixel);
        }

        [LuaHelpInfo("Sets texts boldness of the label")]
        public bool bold
        {
            get => ((Label)Value).style.unityFontStyleAndWeight.value == UnityEngine.FontStyle.Bold;
            set => ((Label)Value).style.unityFontStyleAndWeight = value ? UnityEngine.FontStyle.Bold : UnityEngine.FontStyle.Normal;
        }

        [LuaHelpInfo("Sets the text alignment to centered")]
        public bool center
        {
            get => ((Label)Value).style.unityTextAlign.value == UnityEngine.TextAnchor.UpperCenter;
            set => ((Label)Value).style.unityTextAlign = value ? UnityEngine.TextAnchor.UpperCenter : UnityEngine.TextAnchor.UpperLeft;
        }

        [MoonSharpHidden]
        public LabelProxy(VisualElement value, ControlPanelProxy panel, string path) : base(value, panel, path)
        {
        }

        [MoonSharpHidden]
        public override string Name => "label";
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

        [MoonSharpHidden]
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
