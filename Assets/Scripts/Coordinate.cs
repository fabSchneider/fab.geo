using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Fab.Geo
{
    [System.Serializable]
    public struct Coordinate : IEquatable<Coordinate>, IFormattable
    {
        public float longitude;
        public float latitude;

        public Coordinate(float longitude, float latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float2(Coordinate c) { return new float2(c.latitude, c.longitude); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Coordinate(float2 f2) { return new Coordinate(f2.x, f2.y); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool2 operator ==(Coordinate lhs, Coordinate rhs) { return new bool2(lhs.longitude == rhs.longitude, lhs.latitude == rhs.latitude); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool2 operator !=(Coordinate lhs, Coordinate rhs) { return new bool2(lhs.longitude != rhs.longitude, lhs.latitude != rhs.latitude); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Coordinate rhs) { return longitude == rhs.longitude && latitude == rhs.latitude; }

        public override bool Equals(object o) { return o is Coordinate converted && Equals(converted); }


        /// <summary>Returns a hash code for the float2.</summary>
        /// <returns>The computed hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() { return (int)math.hash(this); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return string.Format("(Lon: {0} Lat: {1})", math.degrees(longitude), math.degrees(latitude));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("(Lon: {0} Lat: {1})", longitude.ToString(format, formatProvider), latitude.ToString(format, formatProvider));
        }
    }
}
