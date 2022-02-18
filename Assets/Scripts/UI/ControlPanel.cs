using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Fab.Geo.UI
{
	public class ControlPanel
	{
		private static readonly string className = "control-panel";
		private static readonly string contentClassName = className + "__content";
		private static readonly string seperatorClassName = className + "__seperator";

		private VisualElement root;
		private VisualElement controlPanelElement;
		private VisualElement controlPanelContent;

		private VisualElementHierachyBuilder hierachyBuilder;

		public ControlPanel(VisualElement root)
		{
			this.root = root;

			controlPanelElement = new VisualElement();
			controlPanelElement.AddToClassList(className);

			ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical);
			scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

			controlPanelContent = new VisualElement();
			controlPanelContent.AddToClassList(contentClassName);
			scrollView.Add(controlPanelContent);
			controlPanelElement.Add(scrollView);

			hierachyBuilder = new VisualElementHierachyBuilder(controlPanelContent, CreateGroup);
		}

		public string GetNameFromPath(string path)
		{
			return hierachyBuilder.GetName(path);
		}

		private VisualElement CreateGroup(string name)
		{
			Foldout foldout = new Foldout();
			foldout.text = name;
			foldout.value = true;
			return foldout;
		}

		/// <summary>
		/// Shows the control panel
		/// </summary>
		public void Show()
		{
			root.Add(controlPanelElement);
		}

		/// <summary>
		/// Hides the control panel
		/// </summary>
		public void Hide()
		{
			controlPanelElement.RemoveFromHierarchy();
		}

		/// <summary>
		/// Removes all controls and hides the panel
		/// </summary>
		public void ClearAndHide()
		{
			hierachyBuilder.RemoveAll();
			Hide();
		}

		/// <summary>
		/// Gets the control at the given path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public VisualElement GetControl(string path)
		{
			return hierachyBuilder.GetElementAtPath(path);
		}

		/// <summary>
		/// Removes a control at the given path
		/// </summary>
		/// <param name="path"></param>
		/// <returns>Returns true if the element was found and removed</returns>
		public bool RemoveControl(string path)
		{

			if (hierachyBuilder.RemoveElement(path))
			{
				//hide the control panel if no control left
				if (controlPanelContent.childCount == 0)
					Hide();

				return true;
			}

			return false;
		}

		/// <summary>
		/// Adds a label to the panel
		/// </summary>
		/// <param name="path"></param>
		/// <param name="text">The text of the label. If null the last item of the path is used instead</param>
		/// <returns></returns>
		public VisualElement AddLabel(string path, string text = null)
		{
			Label label = new Label();

			hierachyBuilder.AddToHierachy(label, path);

			if (text == null)
				label.text = hierachyBuilder.GetName(path);
			else
				label.text = text;
			Show();
			return label;
		}

		/// <summary>
		/// Adds a separator to the panel
		/// </summary>
		/// <param name="path"></param>
		public VisualElement AddSeparator(string path)
		{
			VisualElement separator = new VisualElement();
			separator.AddToClassList(seperatorClassName);
			hierachyBuilder.AddToHierachy(separator, path);

			Show();
			return separator;
		}

		/// <summary>
		/// Adds a slider to the panel
		/// </summary>
		/// <param name="path"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="value"></param>
		/// <param name="callback"></param>
		public Slider AddSlider(string path, float min, float max, float value)
		{
			string name = hierachyBuilder.GetName(path);
			Slider slider = new Slider(name, min, max);
			slider.value = value;
			hierachyBuilder.AddToHierachy(slider, path);

			Show();
			return slider;
		}

		/// <summary>
		/// Adds a ranged slider to the panel
		/// </summary>
		/// <param name="path"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="minLimit"></param>
		/// <param name="maxLimit"></param>
		public MinMaxSlider AddRangeSlider(string path, float min, float max, float minLimit, float maxLimit)
		{
			string name = hierachyBuilder.GetName(path);
			MinMaxSlider rangeSlider = new MinMaxSlider(min, max, minLimit, maxLimit);
			hierachyBuilder.AddToHierachy(rangeSlider, path);

			Show();
			return rangeSlider;
		}

		/// <summary>
		/// Adds a choice dropdown to the panel
		/// </summary>
		/// <param name="path"></param>
		/// <param name="choices"></param>
		/// <param name="value"></param>
		public DropdownField AddChoice(string path, List<string> choices, string value)
		{
			if (choices == null || choices.Count == 0)
			{
				choices = new List<string>() { string.Empty };
				value = string.Empty;
			}

			if (value == null || !choices.Contains(value))
				value = choices[0];

			string name = hierachyBuilder.GetName(path);
			DropdownField dropdown = new DropdownField(name, choices, value);
			hierachyBuilder.AddToHierachy(dropdown, path);

			Show();
			return dropdown;
		}

		/// <summary>
		/// Adds a button to the panel
		/// </summary>
		/// <param name="path"></param>
		/// <param name="callback"></param>
		public Button AddButton(string path, Action callback)
		{
			string name = hierachyBuilder.GetName(path);
			Button button = new Button(callback);
			button.text = name;
			hierachyBuilder.AddToHierachy(button, path);

			Show();
			return button;
		}
	}
}
