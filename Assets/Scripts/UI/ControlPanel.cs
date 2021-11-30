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

        public void AddSeperator()
        {
            if (controlPanelElement.parent == null)
                root.Add(controlPanelElement);

            VisualElement seperator = new VisualElement();
            seperator.AddToClassList(seperatorClassName);
            controlPanelContent.Add(seperator);
        }

        public void BeginGroup(string name, bool expanded = true)
        {
            if (controlPanelElement.parent == null)
                root.Add(controlPanelElement);

            Foldout foldout = new Foldout();
            foldout.text = name;
            foldout.value = expanded;
            currentParent.Add(foldout);
            currentParent = foldout;
        }

        public void EndGroup()
        {
            if (currentParent == controlPanelContent)
                return;

            currentParent = currentParent.parent;
        }

        public void AddSlider(string name, float min, float max, float value, Action<float> callback = null)
        {
            if (controlPanelElement.parent == null)
                root.Add(controlPanelElement);

            Slider slider = new Slider(name, min, max);
            slider.value = value;
            if(callback != null)
                slider.RegisterCallback<ChangeEvent<float>>(evt => callback(evt.newValue));
            currentParent.Add(slider);
        }

        public void AddRangeSlider(string name, float min, float max, float minLimit, float maxLimit)
        {
            if (controlPanelElement.parent == null)
                root.Add(controlPanelElement);

            MinMaxSlider rangeSlider = new MinMaxSlider(min, max, minLimit, maxLimit);
            currentParent.Add(rangeSlider);
        }

        public void AddChoice(string name, List<string> choices, string value)
        {
            if (controlPanelElement.parent == null)
                root.Add(controlPanelElement);

            DropdownField dropdown = new DropdownField(name, choices, value);
            currentParent.Add(dropdown);
        }

        public void AddButton(string name, Action callback)
        {
            if (controlPanelElement.parent == null)
                root.Add(controlPanelElement);

            Button button = new Button(callback);
            button.text = name;
            currentParent.Add(button);
        }
    }
}
