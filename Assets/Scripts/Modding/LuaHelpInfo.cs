using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fab.Geo.Modding
{
    /// <summary>
    /// Use this attribute on a method or class to add help information when calling help(...) in lua
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class LuaHelpInfoAttribute : Attribute
    {
        readonly string info;
        public string Info => info;

        public LuaHelpInfoAttribute(string info)
        {
            this.info = info;
        }
    }

    /// <summary>
    /// Struct holding all the help information about a lua object
    /// </summary>
    public struct LuaHelpInfo
    {
        public string description;
        public IEnumerable<(MethodInfo, string helpInfo)> methodsHelp;
        public IEnumerable<(PropertyInfo, string helpInfo)> propertiesHelp;
    }

    /// <summary>
    /// Utillity class to extract lua help information from classes with <see cref="LuaHelpInfoAttribute"/>
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

        public LuaHelpInfo GetHelpInfoForMethod(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            LuaHelpInfoAttribute helpAttr = methodInfo.GetCustomAttribute<LuaHelpInfoAttribute>();
            if (helpAttr != null)
                return new LuaHelpInfo()
                {
                    description = helpAttr.Info
                };

            return default;
        }

        private LuaHelpInfo ExtractHelpInfoFromType(Type t)
        {
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
                description = description,
                methodsHelp = methods.ToArray(),
                propertiesHelp = properties.ToArray()
            };
        }
    }

    public class LuaHelpInfoFormatter
    {
        private static readonly string NL = Environment.NewLine;
        private static readonly string MethodsHeading = NL + "<b>Methods:</b>" + NL;
        private static readonly string MethodNameFormat = "<b>{0}</b> ( {1} )";

        private static readonly string PropertiesHeading = NL + "<b>Properties:</b>" + NL;
        private static readonly string PropertyNameFormat = "<b>{0}</b>";

        private static readonly string HelpTextFormat = "  <i>{0}</i>" + NL;

        public string Format(LuaHelpInfo helpInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(helpInfo.description);

            if (helpInfo.methodsHelp.Count() > 0)
            {
                sb.AppendLine(MethodsHeading);
                foreach ((MethodInfo method, string help) mh in helpInfo.methodsHelp)
                {
                    sb.AppendLine(string.Format(MethodNameFormat, mh.method.Name,
                        string.Join(" , ", mh.method.GetParameters().Select(p => p.Name))).PadRight(32, ' '));
                    sb.AppendLine(string.Format(HelpTextFormat, mh.help));
                }
            }

            if (helpInfo.propertiesHelp.Count() > 0)
            {
                sb.AppendLine(PropertiesHeading);
                foreach ((PropertyInfo property, string help) ph in helpInfo.propertiesHelp)
                {
                    sb.AppendLine(string.Format(PropertyNameFormat, ph.property.Name));
                    sb.AppendLine(string.Format(HelpTextFormat, ph.help));
                }
            }

            return sb.ToString();
        }
    }
}
