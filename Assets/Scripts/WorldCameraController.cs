using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fab.Geo
{
    [RequireComponent(typeof(PlayerInput))]
    public class WorldCameraController : MonoBehaviour
    {
        [SerializeField]
        private Camera cam;

        PlayerInput playerInput;
        InputAction panAction;
        InputAction deltaAction;
        InputAction zoomAction;

        [SerializeField]
        private float panSpeed = 0.1f;

        [SerializeField]
        private float zoomSpeed = 0.1f;

        [SerializeField]
        private Vector2 zoomBounds;

        private float zoomLevel;

        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            panAction = playerInput.actions.FindAction("Pan");
            deltaAction = playerInput.actions.FindAction("Delta");
            zoomAction = playerInput.actions.FindAction("Zoom");

            zoomLevel = transform.position.magnitude;
        }

        private void Update()
        {
            if (panAction.ReadValue<float>() > 0f && deltaAction.triggered)
                Pan(deltaAction.ReadValue<Vector2>());

            if (zoomAction.triggered)
                Zoom(zoomAction.ReadValue<Vector2>().y);
        }

        public void Pan(Vector2 delta)
        {
            transform.RotateAround(Vector3.zero, transform.up, delta.x * panSpeed);
            transform.RotateAround(Vector3.zero, transform.right, -delta.y * panSpeed);
        }

        public void Zoom(float delta)
        {
            zoomLevel = Mathf.Clamp(zoomLevel + delta * zoomSpeed, zoomBounds.x, zoomBounds.y);
            Debug.Log(zoomLevel);
            transform.position =  (transform.position).normalized * zoomLevel;

        }
    }
}
