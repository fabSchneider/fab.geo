namespace Fab.Lua.Core
{
	/// <summary>
	/// Abstract base class for c# objects that should be exposed to lua
	/// </summary>
	public abstract class LuaObject
	{
	}

	/// <summary>
	/// A <see cref="LuaObject"/> implementing this interface will be initialized in script registration process
	/// </summary>
	public interface ILuaObjectInitialize
	{
		void Initialize();
	}

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
