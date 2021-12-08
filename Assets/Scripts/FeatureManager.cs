using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    public class FeatureManager : MonoBehaviour
    {
        private List<Feature> features;

        [SerializeField]
        private FeaturePoint pointPrefab;

        private void Awake()
        {
            features = new List<Feature>();
        }

        public FeaturePoint AddPoint(string name, float lat, float lon)
        {
            FeaturePoint inst = Instantiate(pointPrefab, transform);
            Vector3 pos = GeoUtils.CoordinateToPoint(lat, lon);
            //add some altitude so the point is not sunken into the ground
            inst.transform.localPosition = pos + pos * 0.005f;
            inst.name = name;
            features.Add(inst);
            return inst;
        }

        public bool RemoveFeature(Feature feature)
        {
            if (features.Remove(feature))
            {
                Destroy(feature.gameObject);
                return true;
            }
            return false;
        }
    }
}
