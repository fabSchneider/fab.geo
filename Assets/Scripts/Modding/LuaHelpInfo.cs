using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fab.Geo.Modding
{
    /// <summary>
    /// Struct holding all the help information about a lua object
    /// </summary>
    public struct LuaHelpInfo
    {
        public string name;
        public string description;
        public IEnumerable<(MethodInfo method, string helpInfo)> methodsHelp;
        public IEnumerable<(PropertyInfo property, string helpInfo)> propertiesHelp;
    }

    /// <summary>
    /// Utility class to extract lua help information from classes with <see cref="LuaHelpInfoAttribute"/>
    /// </summary>
    public class LuaHelpInfoExtractor
    {
        private Dictionary<Type, LuaHelpInfo> cachedInfo;

        public LuaHelpInfoExtractor()
        {
            cachedInfo = new Dictionary<Type, LuaHelpInfo>();
        }

        /// <summary>
        /// Returns a formatted string of help information for the specified type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public LuaHelpInfo GetHelpInfoForType(Type t)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            if (cachedInfo.TryGetValue(t, out LuaHelpInfo info))
                return info;

            info = ExtractHelpInfoFromType(t);
            cachedInfo.Add(t, info);
            return info;
        }

        private LuaHelpInfo ExtractHelpInfoFromType(Type t)
        {
            string name = ((StandardUserDataDescriptor)UserData.GetDescriptorForType(t, false)).FriendlyName;
            string description = string.Empty;
            LuaHelpInfoAttribute classInfoAttr = t.GetCustomAttribute<LuaHelpInfoAttribute>();
            if (classInfoAttr != null)
                description = classInfoAttr.Info;

            var methods = from m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                          where Attribute.IsDefined(m, typeof(LuaHelpInfoAttribute))
                          select (m, m.GetCustomAttribute<LuaHelpInfoAttribute>().Info);

            var properties = from p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             where Attribute.IsDefined(p, typeof(LuaHelpInfoAttribute))
                             select (p, p.GetCustomAttribute<LuaHelpInfoAttribute>().Info);

            return new LuaHelpInfo()
            {
                name = name,
                description = description,
                methodsHelp = methods.ToArray(),
                propertiesHelp = properties.ToArray()
            };
        }

    }
}
