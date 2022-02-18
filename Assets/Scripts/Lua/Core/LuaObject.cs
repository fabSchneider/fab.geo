namespace Fab.Geo.Lua.Core
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
}
