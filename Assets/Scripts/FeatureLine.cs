using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo
{
    [RequireComponent(typeof(LineRenderer))]
    public class FeatureLine : Feature
    {
        private LineRenderer lr;

        private Coordinate coordA, coordB;

        [SerializeField]
        private float zOffset = 0.005f;

        public override Coordinate[] Geometry => new Coordinate[] { coordA, coordB };

        void Awake()
        {
            lr = GetComponent<LineRenderer>();
        }

        public void SetEndpoints(Coordinate coordA, Coordinate coordB)
        {
            this.coordA = coordA;
            this.coordB = coordB;

            Vector3 pA = GeoUtils.CoordinateToPoint(coordA) * (1 + zOffset);
            Vector3 pB = GeoUtils.CoordinateToPoint(coordB) * (1 + zOffset);

            float distance = GeoUtils.Distance(coordA, coordB);
            int divs = (int)(distance / 200);

            Vector3[] pts = new Vector3[2 + divs];

            pts[0] = pA;

            Quaternion.FromToRotation(pA, pB).ToAngleAxis(out float angle, out Vector3 axis);

            float angleIncrement = angle / pts.Length;

            for (int i = 1; i < pts.Length - 1; i++)
            {
                pts[i] = Quaternion.AngleAxis(angleIncrement * i, axis) * pA;
            }

            pts[pts.Length - 1] = pB;

            lr.positionCount = pts.Length;
            lr.SetPositions(pts);
        }
    }
}
