using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo
{
    public class ControlPanel
    {
        private static readonly string className = "control-panel";
        private static readonly string contentClassName = className + "__content";
        private static readonly string seperatorClassName = className + "__seperator";

        private VisualElement root;
        private VisualElement controlPanelElement;
        private VisualElement controlPanelContent;

        private VisualElement currentParent;

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

            currentParent = controlPanelContent;
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
            root.Remove(controlPanelElement);
        }

        public void AddSeperator()
        {
            VisualElement seperator = new VisualElement();
            seperator.AddToClassList(seperatorClassName);
            controlPanelContent.Add(seperator);
            Show();
        }

        public void BeginGroup(string name, bool expanded = true)
        {
            Foldout foldout = new Foldout();
            foldout.text = name;
            foldout.value = expanded;
            currentParent.Add(foldout);
            currentParent = foldout;

            Show();
        }

        public void EndGroup()
        {
            if (currentParent == controlPanelContent)
                return;

            currentParent = currentParent.parent;
        }

        public void AddSlider(string name, float min, float max, float value, Action<float> callback = null)
        {
            Slider slider = new Slider(name, min, max);
            slider.value = value;
            if (callback != null)
                slider.RegisterCallback<ChangeEvent<float>>(evt => callback(evt.newValue));
            currentParent.Add(slider);

            Show();
        }

        public void AddRangeSlider(string name, float min, float max, float minLimit, float maxLimit)
        {
            MinMaxSlider rangeSlider = new MinMaxSlider(min, max, minLimit, maxLimit);
            currentParent.Add(rangeSlider);

            Show();
        }

        public void AddChoice(string name, List<string> choices, string value)
        {
            DropdownField dropdown = new DropdownField(name, choices, value);
            currentParent.Add(dropdown);

            Show();
        }

        public void AddButton(string name, Action callback)
        {
            Button button = new Button(callback);
            button.text = name;
            currentParent.Add(button);

            Show();
        }
    }
}
