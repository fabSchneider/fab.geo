using Fab.Geo.Lua.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Fab.Geo.Lua.Editor
{
	public class LuaHelpMarkdownFormatter
	{
		private static string NL => Environment.NewLine;
		private static string CodeTag => "`";

		private static string MethodFormat = " {0}({1}) ";
		private static string PropertyFormat = " {0} ";

		public string Format(LuaHelpInfo helpInfo)
		{
			StringBuilder sb = new StringBuilder();

			WriteHeading(sb, helpInfo.name, 2);
			WriteText(sb, helpInfo.description);

			if (helpInfo.methodsHelp.Count() > 0)
			{
				WriteHeading(sb, "Methods:", 3);
				foreach (var methodHelp in helpInfo.methodsHelp)
				{
					WriteHeading(sb, MethodToString(methodHelp.method), 4);
					WriteText(sb, methodHelp.helpInfo);
				}
			}

			if (helpInfo.propertiesHelp.Count() > 0)
			{
				WriteHeading(sb, "Properties:", 3);
				foreach (var propertyHelp in helpInfo.propertiesHelp)
				{
					WriteHeading(sb, PropertyToString(propertyHelp.property), 4);
					WriteText(sb, propertyHelp.helpInfo);
				}
			}

			return sb.ToString();
		}

		private void WriteHeading(StringBuilder sb, string heading, int level)
		{
			sb.Append(string.Concat(Enumerable.Repeat('#', Mathf.Max(1, level))) + ' ' + heading + NL);
		}

		private void WriteText(StringBuilder sb, string text)
		{
			sb.Append(text + NL + NL);
		}

		private void WriteCode(StringBuilder sb, string text)
		{
			sb.Append(CodeTag + text + CodeTag + NL + NL);
		}

		private string MethodToString(MethodInfo methodInfo)
		{
			string param = string.Join(' ', methodInfo.GetParameters().Select(p => p.Name));
			return string.Format(MethodFormat, methodInfo.Name, param);
		}

		private string PropertyToString(PropertyInfo propertyInfo)
		{
			return string.Format(PropertyFormat, propertyInfo.Name);
		}
	}
}
