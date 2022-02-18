using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Fab.Geo
{
	/// <summary>
	/// Utility functions for geography
	/// </summary>
	public static class GeoUtils
	{
		public const int EARTH_RADIUS_KM = 6371;
		public static readonly float EARTH_RADIUS_KM_INV = 1f / EARTH_RADIUS_KM;
		public const int EARTH_RADIUS_MILES = 3960;

		/// <summary>
		/// Calculates latitude and longitude (in radians) from a point on a unit sphere
		/// </summary>
		/// <param name="pointOnUnitSphere"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Coordinate PointToCoordinate(float3 pointOnUnitSphere)
		{
			float longitude = math.atan2(pointOnUnitSphere.x, -pointOnUnitSphere.z);
			float latitude = math.asin(pointOnUnitSphere.y);

			float len = math.length(pointOnUnitSphere);
			float altitude = (len - 1f) * EARTH_RADIUS_KM / 1000f;
			return new Coordinate(longitude, latitude, altitude);
		}

		/// <summary>
		/// Calculates a point on a unit sphere from a coordinate (in radians) offset by its altitude
		/// </summary>
		/// <param name="coordinate"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 CoordinateToPoint(Coordinate coordinate)
		{
			float y = math.sin(coordinate.latitude);
			float r = math.cos(coordinate.latitude);
			float x = math.sin(coordinate.longitude) * r;
			float z = -math.cos(coordinate.longitude) * r;

			float alt = 1f + EARTH_RADIUS_KM_INV * (coordinate.altitude / 1000f);
			return new float3(x * alt, y * alt, z * alt);
		}


		/// <summary>
		/// Calculates latitude and longitude (in radians) from a point on a unit sphere
		/// </summary>
		/// <param name="pointOnUnitSphere"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float2 PointToLonLat(float3 pointOnUnitSphere)
		{
			float longitude = math.atan2(pointOnUnitSphere.x, -pointOnUnitSphere.z);
			float latitude = math.asin(pointOnUnitSphere.y);
			return new float2(longitude, latitude);
		}

		/// <summary>
		/// Calculates a point on a unit sphere from latitude and longitude
		/// </summary>
		/// <param name="lat"></param>
		/// <param name="lon"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 LonLatToPoint(float lon, float lat)
		{
			float latRad = math.radians(lat);
			float latLon = math.radians(lon);
			float y = math.sin(latRad);
			float r = math.cos(latRad);
			float x = math.sin(latLon) * r;
			float z = -math.cos(latLon) * r;
			return new float3(x, y, z);
		}

		/// <summary>
		/// Calculates a point on a unit sphere from latitude and longitude
		/// </summary>
		/// <param name="lonlat"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 LonLatToPoint(float2 lonlat) => LonLatToPoint(lonlat.x, lonlat.y);


		/// <summary>
		/// Maps longitude and latitude into the interval [0, 1]
		/// </summary>
		/// <param name="coordinate"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 NormalizeCoordinate(Coordinate coordinate)
		{
			return new float3(
				(math.PI + coordinate.longitude) / (math.PI * 2),
				(math.PI / 2f + coordinate.latitude) / (math.PI),
				coordinate.altitude);
		}

		/// <summary>
		/// Maps longitude and latitude into the interval [0, 1]
		/// </summary>
		/// <param name="lat"></param>
		/// <param name="lon"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float2 NormalizeLonLat(float lon, float lat)
		{
			return new float2(
				(math.PI + lon) / (math.PI * 2),
				(math.PI / 2f + lat) / (math.PI));
		}

		/// <summary>
		/// Maps longitude and latitude into the interval [0, 1]
		/// </summary>
		/// <param name="lat"></param>
		/// <param name="lon"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float2 NormalizeLonLat(float2 lonlat) => NormalizeLonLat(lonlat.x, lonlat.y);

		/// <summary>
		/// Maps a normalized coordinate to longitude and latitude
		/// </summary>
		/// <param name="coordinate"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Coordinate NormalizedToCoordinate(float2 normalized, float altitude = 0f)
		{
			return new Coordinate(
				normalized.x * math.PI * 2 - math.PI,
				normalized.y * math.PI - math.PI / 2,
				altitude);
		}

		/// <summary>
		/// Calculates a point on a unit sphere from a point on a cube
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 PointOnCubeToPointOnSphere(float3 p)
		{
			float x2 = p.x * p.x;
			float y2 = p.y * p.y;
			float z2 = p.z * p.z;

			float x = p.x * math.sqrt(1f - (y2 + z2) / 2f + (y2 * z2) / 3f);
			float y = p.y * math.sqrt(1f - (z2 + x2) / 2f + (z2 * x2) / 3f);
			float z = p.z * math.sqrt(1f - (x2 + y2) / 2f + (x2 * y2) / 3f);

			return new float3(x, y, z);
		}

		/// <summary>
		/// Calculates the distance in km between two coordinates
		/// </summary>
		/// <param name="coord1"></param>
		/// <param name="coord2"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Distance(Coordinate coord1, Coordinate coord2)
		{
			float dLat = coord2.latitude - coord1.latitude;
			float dLon = coord2.longitude - coord1.longitude;

			float a = math.sin(dLat / 2f) * math.sin(dLat / 2f) +
				math.cos(coord1.latitude) * math.cos(coord2.latitude) *
				math.sin(dLon / 2) * math.sin(dLon / 2);
			float c = 2 * math.asin(math.min(1, math.sqrt(a)));
			return EARTH_RADIUS_KM * c;
		}
	}
}
