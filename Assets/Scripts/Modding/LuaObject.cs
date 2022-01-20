using MoonSharp.Interpreter;
using System;

namespace Fab.Geo.Modding
{ 
    /// <summary>
    /// Abstract base class for c# objects that should be exposed to lua
    /// </summary>
    public abstract class LuaObject
    {
        /// <summary>
        /// The name of the type of object in lua
        /// </summary>
        public static string GetLuaName(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(LuaNameAttribute), false);
            if(attr.Length > 0)
            {
                LuaNameAttribute nameAttr = (LuaNameAttribute)attr[0];
                return nameAttr.Name;
            }
            return type.Name.ToLower();
        }
    }

    public abstract class LuaProxy : LuaObject
    {
        public abstract bool IsNil();
    }


    /// <summary>
    /// Abstract base class for proxy objects that wrap a c# object and expose its functionality to lua  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LuaProxy<T> : LuaProxy where T : class
    {
        protected T value;

        [MoonSharpHidden]
        public T Value => value;

        [MoonSharpHidden]
        public void SetValue(T value)
        {
            this.value = value;
        }

        /// <summary>
        /// Returns true if the underlaying value of the proxy is null. 
        /// Use this instead of a simple null check to make it compatible with unity objects
        /// </summary>
        /// <returns></returns>
        [MoonSharpHidden]
        public override bool IsNil()
        {
            if (value is UnityEngine.Object obj)
                return !obj;

            return value == null;
        }

        /// <summary>
        /// Throws if the underlaying value of the proxy is null. 
        /// Use this instead of a simple null check to make it compatible with unity objects
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        protected void ThrowIfNil()
        {
            if (value is UnityEngine.Object obj && !obj)
                throw new NullReferenceException($"{GetLuaName(GetType())} is nil");

            if (value == null)
                throw new NullReferenceException($"{GetLuaName(GetType())} is nil");
        }
    }


    /// <summary>
    /// A <see cref="LuaObject"/> implementing this interface will be initialized in script registration process
    /// </summary>
    public interface ILuaObjectInitialize
    {
        void Initialize();
    }
}