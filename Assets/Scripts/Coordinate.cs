using Unity.Mathematics;

namespace Fab.Geo
{
    [System.Serializable]
    public struct Coordinate
    {
        public float longitude;
        public float latitude;

        public Coordinate(float longitude, float latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }

        public static implicit operator float2(Coordinate c)
        {
            return new float2(c.latitude, c.longitude);
        }

        public static implicit operator Coordinate(float2 f2)
        {
            return new Coordinate(f2.x, f2.y);
        }

        public override string ToString()
        {
            return $"(Lat: {math.degrees(latitude)} Lon: {math.degrees(longitude)})";
        }
    }
}
