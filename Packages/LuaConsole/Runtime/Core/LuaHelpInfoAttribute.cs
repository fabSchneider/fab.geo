using System;

namespace Fab.Lua.Core
{
	/// <summary>
	/// Use this attribute on a method or class to add help information when calling help(...) in lua
	/// </summary>
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public sealed class LuaHelpInfoAttribute : Attribute
	{
		readonly string info;
		public string Info => info;

		public LuaHelpInfoAttribute(string info)
		{
			this.info = info;
		}
	}
}
