//using Unity.Mathematics;
//using UnityEngine;

//namespace Fab.Geo.Samples
//{
//    [RequireComponent(typeof(MeshFilter))]
//    public class PlaneSample : MonoBehaviour
//    {
//        public float offset = 1f;
//        public float2 extent = new float2(0.5f, 0.5f);
//        public int2 resolution = new int2(4, 4);

//        private MeshFilter mf;

//        private void Start()
//        {
//            if (mf == null)
//            {
//                Mesh mesh = new Mesh();
//                mesh.name = "Plane";
//                mf = GetComponent<MeshFilter>();
//                mf.mesh = mesh;
//            }
//        }

//        private void Update()
//        {
//            if (mf)
//            {
//                var mesh = mf.mesh;
//                MeshUtils.CreatePlane(mesh, Vector3.forward, offset, extent, resolution);
//            }
//        }
//    }
//}