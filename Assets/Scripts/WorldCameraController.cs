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
        InputAction orbitAction;
        InputAction deltaAction;
        InputAction zoomAction;

        [SerializeField]
        private float panSpeed = 0.1f;

        [SerializeField]
        private float orbitSpeed = 0.1f;

        [SerializeField]
        private Vector2 zoomBounds;


        [SerializeField]
        private AnimationCurve zoomSpeed;

        [SerializeField]
        private AnimationCurve cameraPitch;

        [SerializeField]
        private World world;

        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            panAction = playerInput.actions.FindAction("Pan");
            orbitAction = playerInput.actions.FindAction("Orbit");
            deltaAction = playerInput.actions.FindAction("Delta");
            zoomAction = playerInput.actions.FindAction("Zoom");
            cam.farClipPlane = -cam.transform.localPosition.z;
        }

        private void Update()
        {
            if (panAction.ReadValue<float>() > 0f && deltaAction.triggered)
                Pan(deltaAction.ReadValue<Vector2>());

            if (orbitAction.ReadValue<float>() > 0f && deltaAction.triggered)
                Orbit(deltaAction.ReadValue<Vector2>());

            if (zoomAction.triggered)
                Zoom(zoomAction.ReadValue<Vector2>().y);

            if (world)
                CullWorld();
        }


        public Coordinate GetCoordinate()
        {
            return GeoUtils.PointToCoordinate(-transform.forward);
        }

        /// <summary>
        /// Sets the camera position to the given coordinate
        /// </summary>
        /// <param name="coordinate"></param>
        public void SetCoordinate(Coordinate coordinate)
        {
            Vector3 to = GeoUtils.CoordinateToPoint(coordinate);
            transform.rotation = Quaternion.LookRotation(-to);
            if (world)
                CullWorld();
        }

        /// <summary>
        /// Sets the zoom level [0-1]
        /// </summary>
        /// <param name="zoomLevel"></param>
        public void SetZoom(float zoomLevel)
        {
            float pitch = cameraPitch.Evaluate(zoomLevel);

            Vector3 camLocalPos = cam.transform.localPosition;

            cam.transform.localPosition = new Vector3(camLocalPos.x, camLocalPos.y, -Mathf.Lerp(zoomBounds.y, zoomBounds.x, zoomLevel));
            Vector3 euler = cam.transform.localRotation.eulerAngles;
            cam.transform.localEulerAngles = new Vector3(-pitch, euler.y, euler.z);
        }

        private void Pan(Vector2 delta)
        {

            Vector3 camLocalPos = cam.transform.localPosition;
            float lastZoomLevel = -camLocalPos.z;
            float currPanSpeed = Mathf.Lerp(panSpeed, panSpeed * 0.05f, Mathf.InverseLerp(zoomBounds.y, zoomBounds.x, lastZoomLevel));

            transform.localRotation = transform.localRotation *
            Quaternion.AngleAxis(delta.x * currPanSpeed, Vector3.up) *
            Quaternion.AngleAxis(-delta.y * currPanSpeed, Vector3.right);
        }

        private void Orbit(Vector2 delta)
        {
            transform.rotation *= Quaternion.AngleAxis(delta.x * orbitSpeed, cam.transform.localPosition.normalized);
        }

        private void Zoom(float delta)
        {
            Vector3 camLocalPos = cam.transform.localPosition;
            float lastZoomLevel = -camLocalPos.z;
            float lastZoomLevelNorm = Mathf.InverseLerp(zoomBounds.y, zoomBounds.x, lastZoomLevel);

            float currZoomSpeed = zoomSpeed.Evaluate(lastZoomLevelNorm) * 0.1f;

            float zoomLevel = Mathf.Clamp(lastZoomLevel + delta * currZoomSpeed, zoomBounds.x, zoomBounds.y);

            float zoomLevelNorm = Mathf.InverseLerp(zoomBounds.y, zoomBounds.x, zoomLevel);
            SetZoom(zoomLevelNorm);
        }

        private List<WorldChunk> regenerateChunks = new List<WorldChunk>();

        private void CullWorld()
        {
            float maxDistance = -cam.transform.localPosition.z - cam.nearClipPlane;
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
