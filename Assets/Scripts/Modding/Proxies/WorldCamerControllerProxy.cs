using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Fab.Geo.Modding
{
    [MoonSharpUserData]
    public class WorldCameraControllerProxy
    {
        private WorldCameraController controller;

        [MoonSharpHidden]
        public WorldCameraControllerProxy(WorldCameraController controller)
        {
            this.controller = controller;
        }

        public void set_coord(float lat, float lon)
        {
            controller.SetCoordinate(new Coordinate(Mathf.Deg2Rad * lon, Mathf.Deg2Rad * lat));
        }

        public float[] get_coord()
        {
            Coordinate coord = controller.GetCoordinate();
            return new float[] { math.degrees(coord.longitude), math.degrees(coord.latitude) };
        }

        public void set_zoom(float zoom)
        {
            controller.SetZoom(zoom);
        }
    }
}
