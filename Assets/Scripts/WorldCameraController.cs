using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fab.Geo
{
    [RequireComponent(typeof(PlayerInput))]
    public class WorldCameraController : MonoBehaviour
    {
        private readonly Vector3[] camViewRays = new Vector3[]
        {
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1)
        };

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

        [SerializeField]
        private World world;


        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            panAction = playerInput.actions.FindAction("Pan");
            deltaAction = playerInput.actions.FindAction("Delta");
            zoomAction = playerInput.actions.FindAction("Zoom");

            zoomLevel = transform.localPosition.magnitude;
            cam.farClipPlane = zoomLevel;
        }

        private void Update()
        {
            if (panAction.ReadValue<float>() > 0f && deltaAction.triggered)
                Pan(deltaAction.ReadValue<Vector2>());

            if (zoomAction.triggered)
                Zoom(zoomAction.ReadValue<Vector2>().y);

            if (world)
                CullWorld();
        }

        private void Pan(Vector2 delta)
        {
            transform.RotateAround(Vector3.zero, transform.up, delta.x * panSpeed);
            transform.RotateAround(Vector3.zero, transform.right, -delta.y * panSpeed);
        }

        private void Zoom(float delta)
        {
            zoomLevel = Mathf.Clamp(zoomLevel + delta * zoomSpeed, zoomBounds.x, zoomBounds.y);
            transform.localPosition = (transform.localPosition).normalized * zoomLevel;

        }

        private List<WorldChunk> regenerateChunks = new List<WorldChunk>();

        private void CullWorld()
        {
            float maxDistance = zoomLevel - cam.nearClipPlane;
            float distance = 0f;

            Vector3 camVector = cam.transform.forward;

            for (int i = 0; i < camViewRays.Length; i++)
            {
                Ray ray = cam.ViewportPointToRay(camViewRays[i]);

                if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance))
                {
                    distance = maxDistance;
                    break;
                }

                distance = Mathf.Max(distance, Vector3.Dot(ray.direction * hit.distance, camVector));
            }
            cam.farClipPlane = cam.nearClipPlane + distance;

            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(cam);

            regenerateChunks.Clear();

            foreach (WorldChunk chunk in world.Chunks)
            {
                Bounds b = world.GetChunk(chunk.id).bounds;
                bool active = GeometryUtility.TestPlanesAABB(frustum, b);
                world.SetChunkActive(chunk.id, active);

                if (active)
                {
                    float d = (cam.transform.position - b.center).magnitude;
                    int lod = world.GetChunkLOD(d);
                    if (lod != chunk.lod)
                        regenerateChunks.Add(new WorldChunk(chunk.id, lod));
                }
            }

            if (regenerateChunks.Count > 0)
                world.RegenerateChunks(regenerateChunks);
        }
    }
}
