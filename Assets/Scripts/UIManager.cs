using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo
{
    [RequireComponent(typeof(UIDocument))]
    public class UIManager : MonoBehaviour
    {
        private UIDocument document;

        [SerializeField]
        private VisualTreeAsset popupAsset;

        void Start()
        {
            document = GetComponent<UIDocument>();
        }
        
        public void Popup(string title, string text)
        {
            var popup = popupAsset.CloneTree();
            popup.name = "Popup";
            popup.Q<Label>(className: "popup__title").text = title;
            popup.Q<Label>(className: "popup__text").text = text;
            popup.Q<Button>(className: "popup__close-btn").clicked += () =>
            {
                popup.Blur();
                popup.RemoveFromHierarchy();
            };
            document.rootVisualElement.Add(popup);
            popup.Focus();
        }

    }
}
