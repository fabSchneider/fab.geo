using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fab.Geo
{
    public class Feature : MonoBehaviour
    {
        public event Action clicked;

        public virtual void SetName(string name)
        {
            this.name = name;
        }


        public void OnClick(BaseEventData data)
        {
            clicked?.Invoke();
        }
    }
}