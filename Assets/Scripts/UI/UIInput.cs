using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ket.sketching
{
    public class UIInput : MonoBehaviour
    {
        private bool pointerOverUI;
        public bool PointerOverUI => pointerOverUI;

        [SerializeField]
        private UIDocument document;

        [SerializeField]
        private PlayerInput playerInput;

        void Start()
        {
            document.rootVisualElement.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            document.rootVisualElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            pointerOverUI = true;
            playerInput.DeactivateInput();
            //playerInput.SwitchCurrentActionMap("UI");
        }
        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            pointerOverUI = false;
            playerInput.ActivateInput();
            //playerInput.SwitchCurrentActionMap("Player");
        }

    }
}
