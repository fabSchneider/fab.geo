using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class FeatureProxy
    {
        private Feature feature;

        private Closure clickEvent;

        [MoonSharpHidden]
        public FeatureProxy(Feature feature)
        {
            this.feature = feature;
        }

        public string name
        {
            get => feature.name;
            set => feature.SetName(name);
        }

        public float center_lat => Mathf.Rad2Deg * feature.Center.latitude;
        public float center_lon => Mathf.Rad2Deg * feature.Center.longitude;

        public int id => feature.GetInstanceID();

        private void OnClick()
        {
            clickEvent.Call(this);
        }

        public void addClickListener(Closure action)
        {
            clickEvent = action;
            feature.clicked -= OnClick;
            feature.clicked += OnClick;
        }

        public void removeClickListener()
        {
            clickEvent = null;
            feature.clicked -= OnClick;
        }
    }
}
