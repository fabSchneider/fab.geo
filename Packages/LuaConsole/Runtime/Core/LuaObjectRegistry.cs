using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fab.Lua.Core
{

	/// <summary>
	/// Registry for all <see cref="LuaObject"/>s
	/// </summary>
	public class LuaObjectRegistry
	{
		private class LuaObjectDescriptorCollection
		{
			public List<StandardUserDataDescriptor> objectDescriptors;
			public List<StandardUserDataDescriptor> proxyDescriptors;
			public List<StandardUserDataDescriptor> initializeDescriptors;
		}

		private Dictionary<string, LuaObjectDescriptorCollection> assemblyDescriptorCollections = new Dictionary<string, LuaObjectDescriptorCollection>();

		/// <summary>
		/// Registers all <see cref="LuaObject"/>s in an assembly
		/// </summary>
		/// <param name="assembly">The assembly to register. If null the calling assembly will be registered</param>
		public void RegisterAssembly(Assembly asm = null)
		{
			if (asm == null)
			{
#if NETFX_CORE || DOTNET_CORE
					throw new NotSupportedException("Assembly.GetCallingAssembly is not supported on target framework.");
#else
				asm = Assembly.GetCallingAssembly();
#endif
			}

			if (assemblyDescriptorCollections.ContainsKey(asm.FullName))
				throw new InvalidOperationException("You are trying to register an assembly that has already been registered");


			assemblyDescriptorCollections[asm.FullName] = new LuaObjectDescriptorCollection();

			RegisterLuaObjectTypes(asm);
			RegisterLuaProxyTypes(asm);
		}

		/// <summary>
		/// Creates instances of all registered LuaObjects implementing <see cref="ILuaObjectInitialize"/>
		/// </summary>
		public Dictionary<object, object> CreateLuaObjects()
		{
			Dictionary<object, object> globals = new Dictionary<object, object>();

			var descriptors = assemblyDescriptorCollections.Values.Select(c => c.initializeDescriptors).SelectMany(d => d);
			foreach (StandardUserDataDescriptor descriptor in descriptors)
			{
				LuaObject luaObject = (LuaObject)Activator.CreateInstance(descriptor.Type);
				try
				{
					((ILuaObjectInitialize)luaObject).Initialize();
					globals.Add(descriptor.FriendlyName, luaObject);
				}
				catch (LuaObjectInitializationException e)
				{
					Debug.LogError($"Error initializing lua {descriptor.FriendlyName} module: {e.Message}");
				}
			}

			return globals;
		}

		/// <summary>
		/// Returns all registered LuaObjects
		/// </summary>
		/// <param name="excludeProxyTypes"></param>
		/// <returns></returns>
		public IEnumerable<StandardUserDataDescriptor> GetRegisteredTypes(bool excludeProxyTypes)
		{
			if (excludeProxyTypes)
				return assemblyDescriptorCollections.Values
					.Select(c => c.objectDescriptors)
					.SelectMany(d => d);

			return assemblyDescriptorCollections.Values
					.Select(c => c.objectDescriptors)
					.Concat(assemblyDescriptorCollections.Values.Select(c => c.proxyDescriptors))
					.SelectMany(d => d);
		}

		private string GetLuaName(Type type)
		{
			var attr = type.GetCustomAttributes(typeof(LuaNameAttribute), false);
			if (attr.Length > 0)
			{
				LuaNameAttribute nameAttr = (LuaNameAttribute)attr[0];
				return nameAttr.Name;
			}
			return type.Name.ToLower();
		}

		private void RegisterLuaObjectTypes(Assembly asm)
		{
			List<StandardUserDataDescriptor> objectDescriptors = new List<StandardUserDataDescriptor>();
			List<StandardUserDataDescriptor> proxyDescriptors = new List<StandardUserDataDescriptor>();
			List<StandardUserDataDescriptor> initializeDescriptors = new List<StandardUserDataDescriptor>();

			var luaObjectTypes = from t in asm.SafeGetTypes()
								 where t.IsSubclassOf(typeof(LuaObject)) && !t.IsAbstract
								 select t;

			foreach (var luaObjectType in luaObjectTypes)
			{
				string name = GetLuaName(luaObjectType);
				var descriptor = (StandardUserDataDescriptor)UserData.RegisterType(luaObjectType, friendlyName: name);

				if (typeof(LuaProxy).IsAssignableFrom(luaObjectType))
					proxyDescriptors.Add(descriptor);
				else
					objectDescriptors.Add(descriptor);

				if (luaObjectType.GetInterfaces().Any(i => i == typeof(ILuaObjectInitialize)))
				{
					initializeDescriptors.Add(descriptor);
					descriptor.RemoveMember("Initialize");
				}
			}

			string asmName = asm.FullName;
			assemblyDescriptorCollections[asmName].objectDescriptors = objectDescriptors;
			assemblyDescriptorCollections[asmName].proxyDescriptors = proxyDescriptors;
			assemblyDescriptorCollections[asmName].initializeDescriptors = initializeDescriptors;
		}

		private void RegisterLuaProxyTypes(Assembly asm)
		{
			var luaProxyFactoryTypes = from t in asm.SafeGetTypes()
									   where typeof(IProxyFactory).IsAssignableFrom(t) && !t.IsAbstract
									   select t;

			foreach (var luaProxyFactoryType in luaProxyFactoryTypes)
			{
				IProxyFactory factory = (IProxyFactory)Activator.CreateInstance(luaProxyFactoryType);
				UserData.RegisterProxyType(factory);
			}
		}
	}
}
