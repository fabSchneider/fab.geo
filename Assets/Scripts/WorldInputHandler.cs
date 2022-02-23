using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fab.Geo
{
	[RequireComponent(typeof(World))]
	public class WorldInputHandler : MonoBehaviour, IPointerClickHandler
	{
		private World world;

		private void Start()
		{
			world = GetComponent<World>();
		}

		public event Action<Coordinate> clicked;
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				Vector3 localPos = transform.worldToLocalMatrix.MultiplyPoint(eventData.pointerCurrentRaycast.worldPosition);
				float2 lonlat = GeoUtils.PointToLonLat(localPos.normalized);
				float altitude = world.GetAltitude(lonlat.x, lonlat.y);
				clicked?.Invoke(new Coordinate(lonlat.x, lonlat.y, altitude));
			}
		}
	}
}
