using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    public static class ClrConversion
    {
        public static void RegisterConverters()
        {
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Coordinate>(
                (script, coordinate) =>
                {
                    DynValue lon = DynValue.NewNumber(Mathf.Rad2Deg * coordinate.longitude);
                    DynValue lat = DynValue.NewNumber(Mathf.Rad2Deg * coordinate.latitude);
                    return script.Call(script.Globals.Get("Coord"), lon, lat);
                }
            );

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Coordinate),
                dynVal =>
                {
                    Table table = dynVal.Table;
                    float lon = (float)table.Get("lon").CastToNumber();
                    float lat = (float)table.Get("lat").CastToNumber();
                    return new Coordinate(Mathf.Deg2Rad * lon, Mathf.Deg2Rad * lat);
                }
             );

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector3),
                dynVal =>
                {
                    Table table = dynVal.Table;
                    float x = (float)table.Get("x").CastToNumber();
                    float y = (float)table.Get("y").CastToNumber();
                    float z = (float)table.Get("z").CastToNumber();
                    return new Vector3(x, y, z);
                }
            );
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector3>(
                (script, vector) =>
                {
                    DynValue x = DynValue.NewNumber(vector.x);
                    DynValue y = DynValue.NewNumber(vector.y);
                    DynValue z = DynValue.NewNumber(vector.z);
                    return script.Call(script.Globals.Get("Vector"), x, y, z);
                }
            );
        }
    }
}
