using MoonSharp.Interpreter;

namespace Fab.Lua.Core
{
	public interface IScriptFactory
	{
		Script CreateScript(string name);
	}
}
