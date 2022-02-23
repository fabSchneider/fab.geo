namespace Fab.Lua.Core
{
	/// <summary>
	/// Use this attribute on a method or class to set a custom name for the lua representation of this class
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class LuaNameAttribute : System.Attribute
	{
		readonly string name;

		public LuaNameAttribute(string name)
		{
			this.name = name;

		}

		public string Name
		{
			get => name;
		}
	}
}
