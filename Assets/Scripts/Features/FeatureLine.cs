using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Fab.Geo
{
	[RequireComponent(typeof(LineRenderer))]
	[RequireComponent(typeof(MeshCollider))]
	public class FeatureLine : Feature
	{
		// Line segment length in km
		private static readonly float SegmentLength = 100;

		private LineRenderer lr;
		private MeshCollider mc;
		private Mesh collisionMesh;

		private Coordinate coordA, coordB;

		[SerializeField]
		private float zOffset = 0.005f;

		public override Coordinate[] Geometry => new Coordinate[] { coordA, coordB };

		protected override void Awake()
		{
			base.Awake();
			lr = GetComponent<LineRenderer>();
			mc = GetComponent<MeshCollider>();
			collisionMesh = new Mesh();
			mc.sharedMesh = collisionMesh;
		}

		/// <summary>
		/// Sets the endpoints of the line
		/// </summary>
		/// <param name="coordA"></param>
		/// <param name="coordB"></param>
		public void SetEndpoints(Coordinate coordA, Coordinate coordB)
		{
			this.coordA = coordA;
			this.coordB = coordB;

			if (coordA.Equals(coordB))
			{
				lr.positionCount = 0;
				lr.SetPositions(new Vector3[0]);
				mc.sharedMesh = null;
				collisionMesh.Clear();
				return;
			}

			Vector3 pA = GeoUtils.CoordinateToPoint(coordA) * (1 + zOffset);
			Vector3 pB = GeoUtils.CoordinateToPoint(coordB) * (1 + zOffset);

			float distance = GeoUtils.Distance(coordA, coordB);
			int divs = (int)(distance / SegmentLength);

			Quaternion.FromToRotation(pA, pB).ToAngleAxis(out float angle, out Vector3 axis);

			//set rotation so that forward points away from the center of the line
			transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle / 2f, axis) * -pA, axis);

			Vector3[] pts = new Vector3[2 + divs];

			Vector3 startPoint = transform.worldToLocalMatrix.MultiplyVector(pA);
			float angleIncrement = angle / (pts.Length - 1);

			pts[0] = startPoint;
			for (int i = 1; i < pts.Length; i++)
				pts[i] = Quaternion.AngleAxis(angleIncrement * i, Vector3.up) * startPoint;

			lr.positionCount = pts.Length;
			lr.SetPositions(pts);

			lr.BakeMesh(collisionMesh);
			mc.sharedMesh = collisionMesh;
		}
	}
}
