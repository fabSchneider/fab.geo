using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Fab.Geo
{
    public class WorldInputHandler : MonoBehaviour, IPointerClickHandler
    {
        public event Action<Coordinate> OnClick;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Vector3 localPos = transform.worldToLocalMatrix.MultiplyPoint(eventData.pointerCurrentRaycast.worldPosition);
                Coordinate coord = GeoUtils.PointToCoordinate(localPos.normalized);
                OnClick?.Invoke(coord);
            }
        }
    }
}
