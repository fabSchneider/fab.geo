using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Fab.Geo
{
    [RequireComponent(typeof(Camera))]
    public class CameraDofController : MonoBehaviour
    {
        [SerializeField]
        private VolumeProfile volumeProfile;
        private DepthOfField dofComponent;

        private int worldLayerMask;
        private Camera cam;


        // Start is called before the first frame update
        void Start()
        {
            cam = GetComponent<Camera>();
            worldLayerMask = LayerMask.GetMask("World");

            if (volumeProfile)
            {
                volumeProfile.TryGet(out dofComponent);
                dofComponent.focusDistance.min = 0.05f;
            }

        }

        private void Update()
        {
            if (!dofComponent)
                return;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, cam.farClipPlane));

            if (Physics.Raycast(ray, out RaycastHit hit, cam.farClipPlane, worldLayerMask))
            {
                dofComponent.focusDistance.value = hit.distance;
            }
        }

        private void OnDrawGizmos()
        {
            if (!cam)
                return;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, cam.farClipPlane));
            Gizmos.DrawRay(ray);

            if (Physics.Raycast(ray, out RaycastHit hit, cam.farClipPlane, worldLayerMask))
            {
                Gizmos.DrawSphere(hit.point, 0.05f);
            }
        }
    }
}
