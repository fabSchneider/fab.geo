using MoonSharp.Interpreter;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Fab.Geo.Modding
{
    public abstract class ProxyBase 
    {
        public abstract string Name { get; }

        /// <summary>
        /// Returns true if the underlaying value of the proxy is null. 
        /// Use this instead of a simple null check to make it compatible with unity objects
        /// </summary>
        /// <returns></returns>
        public virtual bool IsNil()
        {
            //returns false by default as the base class does
            //not require to represent any object but the behaviour can be 
            //overwritten by classes inherting from this class
            return false;
        }
    }

    public abstract class ProxyBase<T> : ProxyBase where T : class
    {
        protected T value;

        [MoonSharpHidden]
        public T Value => value;

        [MoonSharpHidden]
        public ProxyBase(T value)
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
                throw new NullReferenceException($"{Name} is nil");

            if(value == null)
                throw new NullReferenceException($"{Name} is nil");
        }
    }
}
