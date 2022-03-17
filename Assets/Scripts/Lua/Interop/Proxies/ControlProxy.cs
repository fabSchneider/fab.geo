using Fab.Lua.Core;
using MoonSharp.Interpreter;
using System;
using UnityEngine.UIElements;

namespace Fab.Geo.Lua.Interop
{
	public abstract class ControlProxy : LuaProxy<VisualElement>, IDisposable
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

		protected string bindingName;

		public string name
		{
			get
			{
				return controls.Panel.GetNameFromPath(path);
			}
		}

		[LuaHelpInfo("The enabled state of the control")]
		public bool enabled
		{
			get => Target.enabledSelf;
			set => Target.SetEnabled(value);
		}

		private Controls controls;

		[MoonSharpHidden]
		public ControlProxy(VisualElement value, Controls controls, string path)
		{
			this.target = value;
			controlPath = path;
			this.controls = controls;
			bindingName = PathToBindingName(path);
		}

		private string PathToBindingName(string path)
		{
			return controls.Panel.GetNameFromPath(path).ToLower().Replace(' ', '_');
		}


		[LuaHelpInfo("Removes the control from the panel")]
		public void remove()
		{
			ThrowIfNil();
			controls.remove(this);
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
	public class ButtonProxy : ControlProxy
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
	public class SliderProxy : ControlProxy
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

		[LuaHelpInfo("Binds this control to a an objects value")]
		public void bind(DynValue obj)
		{
			if (obj.Type != DataType.UserData)
				throw new Exception("Controls can only bind to user data");

			object bObj = obj.UserData.Object;
			boundProperty = bObj.GetType().GetProperty(bindingName, typeof(float));
			if (boundProperty == null)
				throw new Exception($"Cannot bind the control {path}. A property named \"{bindingName}\" does not exist for the supplied object.");

			((Slider)Target).SetValueWithoutNotify((float)boundProperty.GetValue(bObj));

			boundObject = bObj;
			Target.RegisterCallback<ChangeEvent<float>>(OnValueChange);
		}

		[LuaHelpInfo("Unbinds this control")]
		public void unbind()
		{
			boundObject = null;
			boundProperty = null;
			Target.UnregisterCallback<ChangeEvent<float>>(OnValueChange);
		}

		private void SetBind(float val)
		{
			boundProperty.SetValue(boundObject, val);
		}

		private object boundObject;
		private System.Reflection.PropertyInfo boundProperty;

		private void OnValueChange(ChangeEvent<float> evt)
		{
			if (boundObject != null)
				SetBind(evt.newValue);
			// onValueChange?.Call(evt.newValue);
		}

		public override void Dispose()
		{
			base.Dispose();
			onValueChange = null;
		}
	}

	[LuaName("separator")]
	[LuaHelpInfo("A separator")]
	public class SeparatorProxy : ControlProxy
	{
		public SeparatorProxy(VisualElement value, Controls panel, string path) : base(value, panel, path)
		{
		}
	}

	[LuaName("label")]
	[LuaHelpInfo("A label")]
	public class LabelProxy : ControlProxy
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
	public class ChoiceProxy : ControlProxy
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
