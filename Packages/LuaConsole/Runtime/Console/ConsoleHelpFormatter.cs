using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Fab.Lua.Core;

namespace Fab.Lua.Console
{
	public class ConsoleHelpFormatter : ILuaHelpFormatter
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
