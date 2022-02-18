using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fab.Geo.Lua.Core
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
		private Dictionary<StandardUserDataDescriptor, LuaHelpInfo> cachedInfo;

		public LuaHelpInfoExtractor()
		{
			cachedInfo = new Dictionary<StandardUserDataDescriptor, LuaHelpInfo>();
		}

		/// <summary>
		/// Returns a formatted string of help information for the specified type
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public LuaHelpInfo GetHelpInfoForType(StandardUserDataDescriptor descriptor)
		{
			if (descriptor == null)
				throw new ArgumentNullException(nameof(descriptor));

			if (cachedInfo.TryGetValue(descriptor, out LuaHelpInfo info))
				return info;

			info = ExtractHelpInfoFromType(descriptor);
			cachedInfo.Add(descriptor, info);
			return info;
		}

		private LuaHelpInfo ExtractHelpInfoFromType(StandardUserDataDescriptor descriptor)
		{
			Type type = descriptor.Type;
			string name = descriptor.FriendlyName;
			string description = string.Empty;
			LuaHelpInfoAttribute classInfoAttr = type.GetCustomAttribute<LuaHelpInfoAttribute>();
			if (classInfoAttr != null)
				description = classInfoAttr.Info;

			var methods = from m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
						  where Attribute.IsDefined(m, typeof(LuaHelpInfoAttribute))
						  select (m, m.GetCustomAttribute<LuaHelpInfoAttribute>().Info);

			var properties = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
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
