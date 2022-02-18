using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo
{
	public class FeaturePolyline : Feature
	{
		// Line segement length in km
		private static readonly float SegmentLength = 100;

		private LineRenderer lr;
		private MeshCollider mc;
		private Mesh collisionMesh;

		public bool Closed
		{
			get => lr.loop;
			set
			{
				if (lr.loop == value)
					return;
				lr.loop = value;
				UpdateCollisionMesh();
			}
		}

		[SerializeField]
		private float zOffset = 0.005f;

		private Coordinate[] coordinates;

		public override Coordinate[] Geometry
		{
			get
			{
				Coordinate[] coords = new Coordinate[coordinates.Length];
				coordinates.CopyTo(coords, 0);
				return coords;
			}

		}

		protected override void Awake()
		{
			base.Awake();
			lr = GetComponent<LineRenderer>();
			mc = GetComponent<MeshCollider>();
			collisionMesh = new Mesh();
			mc.sharedMesh = collisionMesh;
		}

		/// <summary>
		/// Sets the points of the polyline
		/// </summary>
		/// <param name="coords">The coordinates of the polyline</param>
		/// <param name="closed"></param>
		/// <exception cref="ArgumentException"></exception>
		public void SetPoints(ICollection<Coordinate> coords, bool closed)
		{
			if (coords == null || coords.Count < 2)
				throw new ArgumentException("A polyline needs to consist of atleast 2 points", nameof(coords));


			coordinates = new Coordinate[coords.Count];
			coords.CopyTo(coordinates, 0);
			lr.loop = closed;
			CalculatePositions();
		}

		private void CalculatePositions()
		{
			List<Vector3> pts = new List<Vector3>(coordinates.Length + 1);

			for (int i = 0; i < coordinates.Length - 1; i++)
			{
				Coordinate a = coordinates[i];
				Coordinate b = coordinates[i + 1];
				GenerateSegment(pts, a, b);
			}

			lr.positionCount = pts.Count;
			lr.SetPositions(pts.ToArray());

			UpdateCollisionMesh();
		}

		private void GenerateSegment(List<Vector3> pts, Coordinate a, Coordinate b)
		{
			Vector3 pA = GeoUtils.CoordinateToPoint(a) * (1 + zOffset);
			Vector3 pB = GeoUtils.CoordinateToPoint(b) * (1 + zOffset);

			float distance = GeoUtils.Distance(a, b);
			int divs = (int)(distance / SegmentLength);

			Quaternion.FromToRotation(pA, pB).ToAngleAxis(out float angle, out Vector3 axis);

			float angleIncrement = angle / (divs + 1);

			pts.Add(pA);
			for (int j = 1; j < divs + 2; j++)
				pts.Add(Quaternion.AngleAxis(angleIncrement * j, axis) * pA);
		}

		private void UpdateCollisionMesh()
		{
			lr.BakeMesh(collisionMesh);
			mc.sharedMesh = collisionMesh;
		}
	}
}
