using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fab.Geo.Modding
{
    /// <summary>
    /// Registry for all <see cref="LuaObject"/>s
    /// </summary>
    public static class LuaObjectRegistry
    {
        private static List<StandardUserDataDescriptor> initializeObjectDescriptors;

        /// <summary>
        /// Registers all <see cref="LuaObject"/>s in an assembly
        /// </summary>
        /// <param name="assembly">The assembly to register. If null the calling assembly will be registered</param>
        public static void RegisterAssembly(Assembly asm = null)
        {
            if (asm == null)
            {
#if NETFX_CORE || DOTNET_CORE
					throw new NotSupportedException("Assembly.GetCallingAssembly is not supported on target framework.");
#else
                asm = Assembly.GetCallingAssembly();
#endif
            }

            RegisterLuaObjectTypes(asm);
            RegisterLuaProxyTypes(asm);
        }

        /// <summary>
        /// Initializes all registered LuaObjects implementing <see cref="ILuaObjectInitialize"/>
        /// </summary>
        public static Dictionary<object, object> InitalizeLuaObjects()
        {
            Dictionary<object, object> globals = new Dictionary<object, object>();

            foreach (StandardUserDataDescriptor descriptor in initializeObjectDescriptors)
            {
                LuaObject luaObject = (LuaObject)Activator.CreateInstance(descriptor.Type);
                try
                {           
                    ((ILuaObjectInitialize)luaObject).Initialize();
                    globals.Add(descriptor.FriendlyName, luaObject);
                }catch(LuaObjectInitializationException e)
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
        public static IEnumerable<Type> GetRegisteredTypes(bool excludeProxyTypes)
        {
            if (excludeProxyTypes)
                return UserData.GetRegisteredTypes()
                        .Where(t => t.IsSubclassOf(typeof(LuaObject)) && !t.IsSubclassOf(typeof(LuaProxy)));

            return UserData.GetRegisteredTypes()
                    .Where(t => t.IsSubclassOf(typeof(LuaObject)));

        }

        private static string GetLuaName(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(LuaNameAttribute), false);
            if (attr.Length > 0)
            {
                LuaNameAttribute nameAttr = (LuaNameAttribute)attr[0];
                return nameAttr.Name;
            }
            return type.Name.ToLower();
        }

        private static void RegisterLuaObjectTypes(Assembly asm)
        {
            initializeObjectDescriptors = new List<StandardUserDataDescriptor>();

            var luaObjectTypes = from t in asm.SafeGetTypes()
                                 where t.IsSubclassOf(typeof(LuaObject)) && !t.IsAbstract
                                 select t;

            foreach (var luaObjectType in luaObjectTypes)
            {
                string name = GetLuaName(luaObjectType);
                var descriptor = (StandardUserDataDescriptor)UserData.RegisterType(luaObjectType, friendlyName: name);
                if (luaObjectType.GetInterfaces().Any(i => i == typeof(ILuaObjectInitialize)))
                {
                    initializeObjectDescriptors.Add(descriptor);
                    descriptor.RemoveMember("Initialize");
                }
            }
        }

        private static void RegisterLuaProxyTypes(Assembly asm)
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
