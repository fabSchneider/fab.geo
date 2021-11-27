using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fab.Geo.Modding
{
    public class FeaturePoint : MonoBehaviour
    {
        public event Action clicked;
        public TextMesh textMesh;

        private void Start()
        {
            if (textMesh)
                textMesh.text = name;
        }

        public void OnClick(BaseEventData data)
        {
            clicked?.Invoke();
        }
    }

    [MoonSharpUserData]
    public class FeaturePointProxy
    {
        private FeaturePoint point;

        private Closure clickEvent;

        [MoonSharpHidden]
        public FeaturePointProxy(FeaturePoint point)
        {
            this.point = point;
        }

        public string name
        {
            get => point.name;
            set
            {
                point.name = value;
                if (point.textMesh)
                    point.textMesh.text = value;
            }
        }

        private void OnClick()
        {
            clickEvent.Call(this);
        }

        public void addClickListener(Closure action)
        {
            clickEvent = action;
            point.clicked -= OnClick;
            point.clicked += OnClick;
        }

        public void removeClickListener()
        {
            clickEvent = null;
            point.clicked -= OnClick;
        }
    }
}
