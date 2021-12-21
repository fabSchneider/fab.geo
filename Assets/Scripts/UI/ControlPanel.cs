using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            root.Remove(controlPanelElement);
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
        /// Adds a separator to the panel
        /// </summary>
        /// <param name="path"></param>
        public void AddSeparator(string path)
        {
            VisualElement seperator = new VisualElement();
            seperator.AddToClassList(seperatorClassName);
            hierachyBuilder.AddToHierachy(seperator, path);

            Show();
        }

        /// <summary>
        /// Adds a slider to the panel
        /// </summary>
        /// <param name="path"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <param name="callback"></param>
        public void AddSlider(string path, float min, float max, float value, Action<float> callback = null)
        {
            string name = hierachyBuilder.GetName(path);
            Slider slider = new Slider(name, min, max);
            slider.value = value;
            if (callback != null)
                slider.RegisterCallback<ChangeEvent<float>>(evt => callback(evt.newValue));
            hierachyBuilder.AddToHierachy(slider, path);

            Show();
        }

        /// <summary>
        /// Adds a ranged slider to the panel
        /// </summary>
        /// <param name="path"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        public void AddRangeSlider(string path, float min, float max, float minLimit, float maxLimit)
        {
            string name = hierachyBuilder.GetName(path);
            MinMaxSlider rangeSlider = new MinMaxSlider(min, max, minLimit, maxLimit);
            hierachyBuilder.AddToHierachy(rangeSlider, path);

            Show();
        }

        /// <summary>
        /// Adds a choice dropdown to the panel
        /// </summary>
        /// <param name="path"></param>
        /// <param name="choices"></param>
        /// <param name="value"></param>
        public void AddChoice(string path, List<string> choices, string value)
        {
            string name = hierachyBuilder.GetName(path);
            DropdownField dropdown = new DropdownField(name, choices, value);
            hierachyBuilder.AddToHierachy(dropdown, path);

            Show();
        }

        /// <summary>
        /// Adds a button to the panel
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        public void AddButton(string path, Action callback)
        {
            string name = hierachyBuilder.GetName(path);
            Button button = new Button(callback);
            button.text = name;
            hierachyBuilder.AddToHierachy(button, path);

            Show();
        }
    }
}
