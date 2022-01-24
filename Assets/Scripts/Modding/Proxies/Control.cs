using MoonSharp.Interpreter;
using System;
using UnityEngine.UIElements;

namespace Fab.Geo.Modding
{
    public abstract class Control : LuaProxy<VisualElement>, IDisposable
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
            get => Target.enabledSelf;
            set => Target.SetEnabled(value);
        }

        private Controls panel;

        [MoonSharpHidden]
        public Control(VisualElement value, Controls panel, string path)
        {
            this.target = value;
            controlPath = path;
            this.panel = panel;
        }

        [LuaHelpInfo("Removes the control from the panel")]
        public void remove()
        {
            ThrowIfNil();
            panel.remove(this);
        }

        [LuaHelpInfo("Moves the control up in the hierarchy")]
        public void move_up()
        {
            int index = Target.parent.IndexOf(Target);
            if (index == 0)
                return;

            Target.PlaceBehind(Target.parent.ElementAt(index - 1));
        }

        [LuaHelpInfo("Moves the control down in the hierarchy")]
        public void move_down()
        {
            int index = Target.parent.IndexOf(Target);
            if (index == Target.parent.childCount - 1)
                return;

            Target.PlaceInFront(Target.parent.ElementAt(index + 1));
        }

        public virtual void Dispose()
        {
            target = null;
            controlPath = null;
        }

        public override string ToString()
        {
            if (IsNil())
                return "nil";

            return $"{UserData.GetDescriptorForObject(this).Name} {{ path: {path} }}";
        }
    }

    [LuaName("button")]
    [LuaHelpInfo("A button control")]
    public class ButtonProxy : Control
    {
        private Closure onClick;

        [MoonSharpHidden]
        public ButtonProxy(VisualElement value, Controls panel, string path) : base(value, panel, path) { }


        [LuaHelpInfo("The text of the button")]
        public string text
        {
            get => ((Button)Target).text;
            set => ((Button)Target).text = value;
        }

        [LuaHelpInfo("Add a function to be executed when the button is clicked")]
        public void on_click(Closure callback)
        {
            ThrowIfNil();

            if (Target is Button button)
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

    [LuaName("slider")]
    [LuaHelpInfo("A slider control")]
    public class SliderProxy : Control
    {
        private Closure onValueChange;

        [LuaHelpInfo("The currently value of the slider")]
        public float val => ((Slider)Target).value;

        [MoonSharpHidden]
        public SliderProxy(VisualElement value, Controls panel, string path) : base(value, panel, path)
        {
        }

        [LuaHelpInfo("Add a function to be executed when the value of this slider changes")]
        public void on_change(Closure callback)
        {
            ThrowIfNil();

            onValueChange = callback;

            Target.UnregisterCallback<ChangeEvent<float>>(OnValueChange);
            if (onValueChange != null)
                Target.RegisterCallback<ChangeEvent<float>>(OnValueChange);
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

    [LuaName("separator")]
    [LuaHelpInfo("A separator")]
    public class SeparatorProxy : Control
    {
        public SeparatorProxy(VisualElement value, Controls panel, string path) : base(value, panel, path)
        {
        }
    }

    [LuaName("label")]
    [LuaHelpInfo("A label")]
    public class LabelProxy : Control
    {
        [LuaHelpInfo("The text of the label")]
        public string text
        {
            get => ((Label)Target).text;
            set => ((Label)Target).text = value;
        }

        [LuaHelpInfo("The text size of the label")]
        public float size
        {
            get => ((Label)Target).style.fontSize.value.value;
            set => ((Label)Target).style.fontSize = new Length(value, LengthUnit.Pixel);
        }

        [LuaHelpInfo("Sets texts boldness of the label")]
        public bool bold
        {
            get => ((Label)Target).style.unityFontStyleAndWeight.value == UnityEngine.FontStyle.Bold;
            set => ((Label)Target).style.unityFontStyleAndWeight = value ? UnityEngine.FontStyle.Bold : UnityEngine.FontStyle.Normal;
        }

        [LuaHelpInfo("Sets the text alignment to centered")]
        public bool center
        {
            get => ((Label)Target).style.unityTextAlign.value == UnityEngine.TextAnchor.UpperCenter;
            set => ((Label)Target).style.unityTextAlign = value ? UnityEngine.TextAnchor.UpperCenter : UnityEngine.TextAnchor.UpperLeft;
        }

        [MoonSharpHidden]
        public LabelProxy(VisualElement value, Controls panel, string path) : base(value, panel, path)
        {
        }
    }

    [LuaName("choice")]
    [LuaHelpInfo("A choice control")]
    public class ChoiceProxy : Control
    {
        private Closure onValueChange;

        [LuaHelpInfo("The currently selected choice")]
        public string val => ((DropdownField)Target).value;

        [MoonSharpHidden]
        public ChoiceProxy(VisualElement value, Controls panel, string path) : base(value, panel, path)
        {
        }

        [LuaHelpInfo("Add a function to be executed when the selected choice changes")]
        public void on_change(Closure callback)
        {
            ThrowIfNil();

            onValueChange = callback;

            Target.UnregisterCallback<ChangeEvent<string>>(OnValueChange);
            if (onValueChange != null)
                Target.RegisterCallback<ChangeEvent<string>>(OnValueChange);
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
