using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo.Modding
{
    /// <summary>
    /// Use this attribute on a method or class to set a custom name for the lua representation of this class
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class LuaNameAttribute : System.Attribute
    {
        readonly string name;

        public LuaNameAttribute(string name)
        {
            this.name = name;

        }

        public string Name
        {
            get => name;
        }
    }
}
