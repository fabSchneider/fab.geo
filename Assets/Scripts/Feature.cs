using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fab.Geo
{
    public abstract class Feature : MonoBehaviour
    {

        public event Action clicked;

        public abstract Coordinate[] Geometry { get; }

        /// <summary>
        /// Sets the features name
        /// </summary>
        /// <param name="name"></param>
        public virtual void SetName(string name)
        {
            this.name = name;
        }


        /// <summary>
        /// Removes the feature
        /// </summary>
        public void Remove()
        {
            FeatureManager manager = GetComponentInParent<FeatureManager>();
            if (manager)
                manager.RemoveFeature(this);
            else
                Destroy(gameObject);
        }
    }
}