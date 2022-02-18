namespace Fab.Geo.Lua.Core
{

	[System.Serializable]
	public class LuaObjectInitializationException : System.Exception
	{
		public LuaObjectInitializationException() { }
		public LuaObjectInitializationException(string message) : base(message) { }
		public LuaObjectInitializationException(string message, System.Exception inner) : base(message, inner) { }
		protected LuaObjectInitializationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
