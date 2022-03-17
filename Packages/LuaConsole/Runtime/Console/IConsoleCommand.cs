namespace Fab.Lua.Console
{
	/// <summary>
	/// Interface for registering commands for a console.
	/// </summary>
	public interface IConsoleCommand
	{
		public void Register(Console console);
	}
}
