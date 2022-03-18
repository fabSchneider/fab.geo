using System.Collections.Generic;
using System.IO;
using Fab.Lua.Core;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace Fab.Lua.Console
{
	public class ConsoleScriptFactory : IScriptFactory
	{
		public Script CreateScript(string scriptName)
		{
			Script script = new Script(CoreModules.Preset_SoftSandbox | CoreModules.LoadMethods);

			//set script loader
			UnityAssetsScriptLoader scriptLoader = new UnityAssetsScriptLoader();

			//we need to set the module path to '?' for it tor load the resource correctly
			scriptLoader.ModulePaths = new string[] { "?" };
			script.Options.ScriptLoader = scriptLoader;

			Dictionary<object, object> globals = LuaEnvironment.Registry.CreateLuaObjects();

			//set globals
			foreach (var global in globals)
				script.Globals[global.Key] = global.Value;

			//add constants
			script.Globals[LuaEnvironment.ScriptNameKey] = Path.GetFileNameWithoutExtension(scriptName);
			script.Globals[LuaEnvironment.ScriptDataDirKey] = LuaEnvironment.DataDirectory + Path.DirectorySeparatorChar;

			foreach (var loaded in scriptLoader.GetLoadedScripts())
				script.DoString($"require \'{loaded}\'");

			return script;
		}
	}
}
