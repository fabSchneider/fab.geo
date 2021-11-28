using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fab.Geo
{
    public class FeaturePoint : Feature
    {
        public TextMesh textMesh;

        public override void SetName(string name)
        {
            base.SetName(name);
            if (textMesh)
                textMesh.text = name;
        }

        private void Start()
        {
            if (textMesh)
                textMesh.text = name;
        }
    }
}
