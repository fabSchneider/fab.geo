using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo
{
	public class FeatureManager : MonoBehaviour
	{
		[SerializeField]
		private FeaturePoint pointPrefab;
		[SerializeField]
		private FeatureLine linePrefab;
		[SerializeField]
		private FeaturePolyline polylinePrefab;

		private Dictionary<string, List<Feature>> features;

		private void Awake()
		{
			features = new Dictionary<string, List<Feature>>();
		}

		/// <summary>
		/// Gets the first feature with given name or null if no feature with that name exists
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Feature GetFeature(string name)
		{
			if (features.TryGetValue(name, out List<Feature> list))
				return list[0];
			return null;
		}

		/// <summary>
		/// Gets all features with the given name or null if no feature with that name exists
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IEnumerable<Feature> GetFeatures(string name)
		{
			if (features.TryGetValue(name, out List<Feature> list))
				return list;
			return null;
		}

		public FeaturePoint AddPoint(string name, Coordinate coord)
		{
			FeaturePoint inst = Instantiate(pointPrefab, transform);
			inst.transform.localPosition = GeoUtils.CoordinateToPoint(coord);
			inst.name = name;
			AddFeature(inst);
			return inst;
		}

		public FeatureLine AddLine(string name, Coordinate a, Coordinate b)
		{
			FeatureLine inst = Instantiate(linePrefab, transform);
			inst.name = name;
			inst.SetEndpoints(a, b);
			AddFeature(inst);
			return inst;
		}

		public FeaturePolyline AddPolyline(string name, ICollection<Coordinate> coords, bool closed)
		{
			FeaturePolyline inst = Instantiate(polylinePrefab, transform);
			inst.name = name;
			inst.SetPoints(coords, closed);
			AddFeature(inst);
			return inst;
		}

		public bool RemoveFeature(Feature feature)
		{
			if (features.TryGetValue(feature.name, out List<Feature> list))
			{
				if (list.Remove(feature))
				{
					Destroy(feature.gameObject);
					return true;
				}
			}
			return false;
		}

		public void RemoveAllFeatures()
		{
			foreach (var featureList in features.Values)
				foreach (var feature in featureList)
					Destroy(feature.gameObject);

			features.Clear();
		}

		private void AddFeature(Feature feature)
		{
			if (features.TryGetValue(feature.name, out List<Feature> list))
			{
				if (!list.Contains(feature))
					list.Add(feature);
			}
			else
			{
				features.Add(feature.name, new List<Feature>() { feature });
			}
		}
	}
}
