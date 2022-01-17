using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo.Modding
{
    [RequireComponent(typeof(UIDocument))]
    public class ReloadUI : MonoBehaviour
    {
        private UIDocument document;

        void Start()
        {
            document = GetComponent<UIDocument>();
            Button reloadBtn = document.rootVisualElement.Q<Button>(name: "reload-btn");
            reloadBtn.clicked += App.Reload;      
        }
    }
}
