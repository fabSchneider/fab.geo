using MoonSharp.Interpreter.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fab.Lua.Core
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
	/// Class to extract and cache lua help information from classes with <see cref="LuaHelpInfoAttribute"/>
	/// </summary>
	public class LuaHelpInfoCache
	{
		private Dictionary<StandardUserDataDescriptor, LuaHelpInfo> cachedUserDataInfo;

		public LuaHelpInfoCache()
		{
			cachedUserDataInfo = new Dictionary<StandardUserDataDescriptor, LuaHelpInfo>();
		}

		/// <summary>
		/// Returns a formatted string of help information for some user data
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public LuaHelpInfo GetHelpInfoForUserData(StandardUserDataDescriptor descriptor)
		{
			if (descriptor == null)
				throw new ArgumentNullException(nameof(descriptor));

			if (cachedUserDataInfo.TryGetValue(descriptor, out LuaHelpInfo info))
				return info;

			info = ExtractHelpInfoFromUserData(descriptor);
			cachedUserDataInfo.Add(descriptor, info);
			return info;
		}

		private LuaHelpInfo ExtractHelpInfoFromUserData(StandardUserDataDescriptor descriptor)
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
