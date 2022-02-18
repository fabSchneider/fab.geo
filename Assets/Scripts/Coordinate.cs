using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Fab.Geo
{
	/// <summary>
	/// A coordinate with two components for longitude and latitude in radians.
	/// The last component represents altitude in meters where 0 is considered to be sea level 
	/// </summary>
	[Serializable]
	public readonly struct Coordinate : IEquatable<Coordinate>
	{
		public readonly float longitude;
		public readonly float latitude;
		public readonly float altitude;

		public Coordinate(float longitude, float latitude)
		{
			this.longitude = longitude;
			this.latitude = latitude;
			altitude = 0f;
		}

		public Coordinate(float longitude, float latitude, float altitude)
		{
			this.longitude = longitude;
			this.latitude = latitude;
			this.altitude = altitude;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator float2(Coordinate c) { return new float2(c.latitude, c.longitude); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Coordinate(float2 f2) { return new Coordinate(f2.x, f2.y); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator float3(Coordinate c) { return new float3(c.latitude, c.longitude, c.altitude); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Coordinate(float3 f3) { return new Coordinate(f3.x, f3.y, f3.z); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool3 operator ==(Coordinate lhs, Coordinate rhs) { return new bool3(lhs.longitude == rhs.longitude, lhs.latitude == rhs.latitude, lhs.altitude == rhs.altitude); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool3 operator !=(Coordinate lhs, Coordinate rhs) { return new bool3(lhs.longitude != rhs.longitude, lhs.latitude != rhs.latitude, lhs.altitude != rhs.altitude); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Coordinate rhs) { return longitude == rhs.longitude && latitude == rhs.latitude && altitude == rhs.altitude; }

		public override bool Equals(object o) { return o is Coordinate converted && Equals(converted); }


		/// <summary>Returns a hash code for the float2.</summary>
		/// <returns>The computed hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() { return (int)math.hash((float3)this); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
		{
			return string.Format("(lon: {0} lat: {1} alt: {2})", math.degrees(longitude), math.degrees(latitude), altitude);
		}
	}
}
