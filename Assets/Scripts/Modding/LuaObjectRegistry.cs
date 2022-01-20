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

            var luaObjectTypes = from t in asm.SafeGetTypes()
                                 where t.IsSubclassOf(typeof(LuaObject)) && !t.IsAbstract
                                 select t;

            foreach (var luaObjectType in luaObjectTypes)
            {
                var descriptor = (StandardUserDataDescriptor)UserData.RegisterType(luaObjectType);
                if (luaObjectType.GetInterfaces().Any(i => i == typeof(ILuaObjectInitialize)))
                    descriptor.RemoveMember("Initialize");
            }
        }

        /// <summary>
        /// Initialized all registered LuaObjects implementing <see cref="ILuaObjectInitialize"/>
        /// </summary>
        /// <param name="globals"></param>
        public static void InitalizeLuaObjects(Dictionary<string, object> globals)
        {
            var luaIntializeTypes = from t in UserData.GetRegisteredTypes()
                                    where t.IsSubclassOf(typeof(LuaObject)) && t.GetInterfaces().Any(i => i == typeof(ILuaObjectInitialize))
                                    select t;

            foreach (Type type in luaIntializeTypes)
            {
                LuaObject luaObject = (LuaObject)Activator.CreateInstance(type);
                try
                {
                    ((ILuaObjectInitialize)luaObject).Initialize();
                    globals.Add(LuaObject.GetLuaName(type), luaObject);
                }catch(LuaObjectInitializationException e)
                {
                    Debug.LogError($"Error initializing lua {LuaObject.GetLuaName(type)} module: {e.Message}");
                }

            }
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
    }
}
