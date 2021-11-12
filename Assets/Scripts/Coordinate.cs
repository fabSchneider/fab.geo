using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FabGeo
{
    public struct Coordinate
    {
        public float latitude;
        public float longitude;

        public Coordinate(float latitude, float longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public static implicit operator float2(Coordinate c)
        {
            return new float2(c.latitude, c.longitude);
        }

        public static implicit operator Coordinate(float2 f2)
        {
            return new Coordinate(f2.x, f2.y);
        }
    }
}
