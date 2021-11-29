using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    public class FeatureManager : MonoBehaviour
    {
        private Dictionary<int, Feature> features;

        [SerializeField]
        private FeaturePoint pointPrefab;

        private void Awake()
        {
            features = new Dictionary<int, Feature>();
        }

        public FeaturePoint AddPoint(string name, float lat, float lon)
        {
            FeaturePoint inst = Instantiate(pointPrefab, transform);
            Vector3 pos = GeoUtils.CoordinateToPoint(lat, lon);
            //add some altitude so the point is not sunken into the ground
            inst.transform.localPosition = pos + pos * 0.005f;
            inst.name = name;
            features.Add(inst.GetInstanceID(), inst);
            return inst;
        }

        public bool RemoveFeature(int id)
        {
            if (features.TryGetValue(id, out Feature feature))
            {
                Destroy(feature.gameObject);
                features.Remove(id);
                return true;
            }
            return false;
        }
    }
}
