using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
    public static class ClrConversion
    {
        private static readonly string COORD = "Coord";
        private static readonly string COORD_LON = "lon";
        private static readonly string COORD_LAT = "lat";
        private static readonly string COORD_ALT = "alt";

        private static readonly string VEC = "Vector";
        private static readonly string VEC_X = "x";
        private static readonly string VEC_Y = "y";
        private static readonly string VEC_Z = "z";

        private static readonly string COLOR = "Color";
        private static readonly string COLOR_R = "r";
        private static readonly string COLOR_G = "g";
        private static readonly string COLOR_B = "b";
        private static readonly string COLOR_A = "a";

        public static void RegisterConverters()
        {
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Coordinate>(CoordinateToScript);
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Coordinate), ScriptToCoordinate);

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector3>(VectorToScript);
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector3), ScriptToVector);

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Color>(ColorToScript);
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Color), ScriptToColor);
        }

        static DynValue CoordinateToScript(Script script, Coordinate coordinate)
        {
            DynValue lon = DynValue.NewNumber(Mathf.Rad2Deg * coordinate.longitude);
            DynValue lat = DynValue.NewNumber(Mathf.Rad2Deg * coordinate.latitude);
            return script.Call(script.Globals.Get(COORD), lon, lat);
        }

        static object ScriptToCoordinate(DynValue dynVal)
        {
            Table table = dynVal.Table;
            float lon = (float)table.Get(COORD_LON).CastToNumber();
            float lat = (float)table.Get(COORD_LAT).CastToNumber();
            float alt = (float)table.Get(COORD_ALT).CastToNumber();
            return new Coordinate(Mathf.Deg2Rad * lon, Mathf.Deg2Rad * lat);
        }

        static DynValue VectorToScript(Script script, Vector3 vector)
        {
            DynValue x = DynValue.NewNumber(vector.x);
            DynValue y = DynValue.NewNumber(vector.y);
            DynValue z = DynValue.NewNumber(vector.z);
            return script.Call(script.Globals.Get(VEC), x, y, z);
        }

        static object ScriptToVector(DynValue dynVal)
        {
            Table table = dynVal.Table;
            float x = (float)table.Get(VEC_X).CastToNumber();
            float y = (float)table.Get(VEC_Y).CastToNumber();
            float z = (float)table.Get(VEC_Z).CastToNumber();
            return new Vector3(x, y, z);
        }

        static DynValue ColorToScript(Script script, Color color)
        {
            DynValue r = DynValue.NewNumber(color.r);
            DynValue g = DynValue.NewNumber(color.g);
            DynValue b = DynValue.NewNumber(color.b);
            DynValue a = DynValue.NewNumber(color.a);
            return script.Call(script.Globals.Get(COLOR), r, g, b, a);
        }

        static object ScriptToColor(DynValue dynVal)
        {
            Table table = dynVal.Table;
            float r = (float)table.Get(COLOR_R).CastToNumber();
            float g = (float)table.Get(COLOR_G).CastToNumber();
            float b = (float)table.Get(COLOR_B).CastToNumber();
            float a = (float)table.Get(COLOR_A).CastToNumber();
            return new Color(r, g, b, a);
        }
    }
}
