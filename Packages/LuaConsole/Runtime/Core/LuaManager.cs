using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Fab.Lua.Core
{
	/// <summary>
	/// Manager class responsible for loading, unloading and updating lua scripts
	/// </summary>
	[AddComponentMenu("FabGeo/Lua/Lua Manager")]
	public class LuaManager : MonoBehaviour
	{
		private static readonly string LuaFileSearchPattern = "*.lua";

		private static readonly string updateFuncKey = "update";
		private static readonly string deltaTimeKey = "deltaTime";

		private IScriptFactory scriptFactory;

		private List<Script> loadedScripts = new List<Script>();
		public IEnumerable<Script> LoadedScripts => loadedScripts;

		private Dictionary<Script, Closure> updateFunctions = new Dictionary<Script, Closure>();

		private event Action<Script> afterScriptLoaded;

		public event Action<Script> AfterScriptLoaded
		{
			add => afterScriptLoaded += value;
			remove => afterScriptLoaded -= value;
		}

		private event Action<Script> beforeScriptUnloaded;

		public event Action<Script> BeforeScriptUnloaded
		{
			add => beforeScriptUnloaded += value;
			remove => beforeScriptUnloaded -= value;
		}

		private void Start()
		{
			scriptFactory = new ScriptFileFactory();
			LoadScripts();
		}

		private void Update()
		{
			foreach (var func in updateFunctions.Values)
			{
				func.OwnerScript.Globals[deltaTimeKey] = Time.deltaTime;
				func.Call();
			}
		}

		/// <summary>
		/// Loads all scripts found in the scripts directory
		/// </summary>
		public void LoadScripts()
		{
			Directory.CreateDirectory(LuaEnvironment.ScriptsDirectory);
			Directory.CreateDirectory(LuaEnvironment.DataDirectory);
			string[] files = Directory.GetFiles(LuaEnvironment.ScriptsDirectory, LuaFileSearchPattern, SearchOption.AllDirectories);

			UnloadAllScripts();

			int loaded = 0;
			foreach (string scriptPath in files)
			{
				//ignore files starting with ~
				if (!Path.GetFileName(scriptPath).StartsWith('~'))
				{
					LoadScript(scriptPath);
					loaded++;
				}

			}

			int ignored = files.Length - loaded;

			if (loaded == 0 && ignored > 0)
				Debug.Log($"Loaded no scripts from user script directory (ignored {files.Length} script(s))");
			else
				Debug.Log($"Loaded {loaded} script(s) from user script directory" +
					(ignored > 0 ? $" (ignored {ignored}script(s))" : string.Empty));
		}

		/// <summary>
		/// Unloads the specified script
		/// </summary>
		/// <param name="script"></param>
		public void UnloadScript(Script script)
		{
			if (LoadedScripts.Contains(script))
			{
				beforeScriptUnloaded?.Invoke(script);

				loadedScripts.Remove(script);
				updateFunctions.Remove(script);
			}
		}

		/// <summary>
		/// Unloads all loaded scripts
		/// </summary>
		public void UnloadAllScripts()
		{
			for (int i = loadedScripts.Count - 1; i >= 0; i--)
				UnloadScript(loadedScripts[i]);
		}

		/// <summary>
		/// Lists all loaded scripts
		/// </summary>
		/// <returns></returns>
		public string[] GetAllLoadedScripts()
		{
			string[] list = new string[loadedScripts.Count];
			for (int i = 0; i < loadedScripts.Count; i++)
				list[i] = loadedScripts[i].Globals[LuaEnvironment.ScriptNameKey].ToString();

			return list;
		}

		private void LoadScript(string path)
		{
			if (!File.Exists(path))
				throw new FileNotFoundException("Could not load script. File not found", path);

			using Stream fileStream = new FileStream(path, FileMode.Open);

			string scriptName = Path.GetFileNameWithoutExtension(path);

			Script script = scriptFactory.CreateScript(scriptName);

			//execute script
			script.DoStream(fileStream);

			//add update function
			Closure updateFunc = script.Globals.Get(updateFuncKey).Function;
			if (updateFunc != null)
				updateFunctions.Add(script, updateFunc);

			//add script to loaded script list
			loadedScripts.Add(script);

			afterScriptLoaded?.Invoke(script);
		}
	}
}
