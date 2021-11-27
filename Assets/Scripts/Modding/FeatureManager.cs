using MoonSharp.Interpreter;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    public class FeatureManager : MonoBehaviour
    {
        [SerializeField]
        private FeaturePoint pointPrefab;

        public FeaturePoint AddPoint(string name, float lat, float lon)
        {
            FeaturePoint inst = Instantiate(pointPrefab, transform);
            inst.transform.localPosition = GeoUtils.CoordinateToPoint(lat, lon);
            inst.name = name;
            return inst;
        }
    }

    [MoonSharpUserData]
    public class FeatureManagerProxy
    {
        private FeatureManager manager;

        [MoonSharpHidden]
        public FeatureManagerProxy(FeatureManager manager)
        {
            this.manager = manager;
        }

        public FeaturePointProxy addPoint(string name, float lat, float lon)
        {
            FeaturePoint fp = manager.AddPoint(name, lat, lon);
            return new FeaturePointProxy(fp);
        }
    }
}
