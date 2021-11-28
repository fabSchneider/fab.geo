using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
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

        public void setPosition(float lat, float lon)
        {
            controller.SetPosition(new Coordinate(Mathf.Deg2Rad * lon, Mathf.Deg2Rad * lat));
        }

        public void setZoom(float zoom)
        {
            controller.SetZoom(zoom);
        }
    }
}
