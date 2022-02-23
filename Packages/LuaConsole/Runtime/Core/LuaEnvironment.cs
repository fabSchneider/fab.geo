using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;
using System;
using System.IO;
using UnityEngine;

namespace Fab.Lua.Core
{
	/// <summary>
	/// Information on the lua environment and initialization 
	/// </summary>
	public static class LuaEnvironment
	{
		public static string DocumentsDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.productName);
		public static string ScriptsDirectory => Path.Combine(DocumentsDirectory, "Scripts");
		public static string DataDirectory => Path.Combine(DocumentsDirectory, "Data");

		public static readonly string ScriptNameKey = "SCRIPT_NAME";
		public static readonly string ScriptLoadDirKey = "SCRIPT_DIR";
		public static readonly string ScriptDataDirKey = "DATA_DIR";

		private static LuaObjectRegistry registry;

		/// <summary>
		/// The environments lua object registry 
		/// </summary>
		public static LuaObjectRegistry Registry
		{
			get
			{
				if (registry == null)
					registry = new LuaObjectRegistry();
				return registry;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		public static void Init()
		{
			Script.GlobalOptions.Platform = new StandardPlatformAccessor();
			Debug.Log("Lua Environment initialized");
		}

		/// <summary>
		/// Returns the name the script is registered with
		/// </summary>
		/// <param name="script"></param>
		/// <returns>Returns null if the script name cannot be resolved</returns>
		public static string GetScriptName(Script script)
		{
			return script.Globals[ScriptNameKey] as string;
		}

		/// <summary>
		/// Returns the path the script was loaded from
		/// </summary>
		/// <param name="script"></param>
		/// <returns>Returns null if the script load path cannot be resolved</returns>
		public static string GetScriptLoadPath(Script script)
		{
			string dir = script.Globals.Get(ScriptLoadDirKey).String;
			string name = script.Globals.Get(ScriptNameKey).String;
			if (dir == null || name == null)
				return null;

			return Path.Combine(dir, name + ".lua");
		}
	}
}
