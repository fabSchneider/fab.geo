using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;

namespace Fab.Geo.Lua.Core
{
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
		protected T target;

		[MoonSharpHidden]
		public T Target => target;

		[MoonSharpHidden]
		public void SetTarget(T value)
		{
			this.target = value;
		}



		/// <summary>
		/// Returns true if the underlaying value of the proxy is null. 
		/// Use this instead of a simple null check to make it compatible with unity objects
		/// </summary>
		/// <returns></returns>
		[MoonSharpHidden]
		public override bool IsNil()
		{
			if (target is UnityEngine.Object obj)
				return !obj;

			return target == null;
		}

		/// <summary>
		/// Throws if the underlaying value of the proxy is null. 
		/// Use this instead of a simple null check to make it compatible with unity objects
		/// </summary>
		/// <exception cref="NullReferenceException"></exception>
		protected void ThrowIfNil()
		{
			if (target is UnityEngine.Object obj && !obj)
				throw new NullReferenceException($"{((StandardUserDataDescriptor)UserData.GetDescriptorForObject(this)).FriendlyName} is nil");

			if (target == null)
				throw new NullReferenceException($"{((StandardUserDataDescriptor)UserData.GetDescriptorForObject(this)).FriendlyName} is nil");
		}
	}

	/// <summary>
	/// Abstract base class for proxy factories. Inherit from this class if you want a proxy type conversion to be automatically registered
	/// </summary>
	/// <typeparam name="TProxy"></typeparam>
	/// <typeparam name="TTarget"></typeparam>
	public abstract class LuaProxyFactory<TProxy, TTarget> : IProxyFactory<TProxy, TTarget>
		where TProxy : LuaProxy<TTarget>
		where TTarget : class
	{
		public Type TargetType => typeof(TTarget);

		public Type ProxyType => typeof(TProxy);

		public TProxy CreateProxyObject(TTarget target)
		{
			TProxy proxy = Activator.CreateInstance<TProxy>();
			proxy.SetTarget(target);
			return proxy;
		}

		public object CreateProxyObject(object o)
		{
			return CreateProxyObject((TTarget)o);
		}
	}
}
