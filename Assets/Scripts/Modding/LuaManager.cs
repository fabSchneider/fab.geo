using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NaughtyAttributes;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;
using MoonSharp.VsCodeDebugger;
using CommandTerminal;
using System.Linq;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Loaders;
using Fab.Geo.UI;

namespace Fab.Geo.Modding
{
    public class LuaManager : MonoBehaviour
    {
        private static readonly string LuaFileSearchPattern = "*.lua";

        private static readonly string scriptNameKey = "SCRIPT_NAME";
        private static readonly string scriptDirKey = "SCRIPT_DIR";
        private static readonly string dataDirKey = "DATA_DIR";

        private static readonly string updateFuncKey = "update";
        private static readonly string deltaTimeKey = "deltaTime";

        private List<Script> loadedScripts = new List<Script>();

        public IEnumerable<Script> LoadedScripts => loadedScripts;

        public TextAsset[] luaModules;

        private Dictionary<Script, Closure> updateFunctions = new Dictionary<Script, Closure>();

        private Dictionary<string, object> globals;

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
            UserData.RegisterAssembly();
            LuaObjectRegistry.RegisterAssembly();
            ClrConversion.RegisterConverters();
            Script.GlobalOptions.Platform = new StandardPlatformAccessor();

            GetGlobals();
            LoadScripts();

            Debug.Log("Loaded Lua Objects: " + Environment.NewLine + string.Join(", ", UserData.GetRegisteredTypes().Select(t => t.Name).ToArray()));
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
        /// Returns the name the script is registered with
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public string GetScriptName(Script script)
        {
            if (loadedScripts.Contains(script))
                return (string)script.Globals[scriptNameKey];

            Debug.LogError("The script is not registered");
            return null;
        }

        /// <summary>
        /// Returns the path the script was loaded from
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public string GetScriptLoadPath(Script script)
        {
            return Path.Combine((string)script.Globals[scriptDirKey], (string)script.Globals[scriptNameKey] + ".lua");
        }

        /// <summary>
        /// (Re)loads all scripts found in the scripts directory
        /// </summary>
        [Button("Reload Scripts")]
        public void LoadScripts()
        {
            string scriptsDir = LuaEnvironment.ScriptsDirectory;
            string dataDir = LuaEnvironment.DataDirectory;
            Directory.CreateDirectory(scriptsDir);
            Directory.CreateDirectory(dataDir);

            Debug.Log("Loading scripts from " + scriptsDir);
            string[] files = Directory.GetFiles(scriptsDir, LuaFileSearchPattern, SearchOption.AllDirectories);

            UnloadAllScripts();

            foreach (string scriptPath in files)
                LoadScript(scriptPath);

            Debug.Log($"Loaded {loadedScripts.Count} scripts");
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
        /// Unloads the specified script
        /// </summary>
        /// <param name="script"></param>
        public void UnloadScript(Script script)
        {
            beforeScriptUnloaded?.Invoke(script);

            loadedScripts.Remove(script);
            updateFunctions.Remove(script);
        }

        /// <summary>
        /// Lists all loaded scripts
        /// </summary>
        /// <returns></returns>
        public string[] GetAllLoadedScripts()
        {
            string[] list = new string[loadedScripts.Count];
            for (int i = 0; i < loadedScripts.Count; i++)
                list[i] = loadedScripts[i].Globals[scriptNameKey].ToString();

            return list;
        }

        /// <summary>
        /// Creates a script with the given script name
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public Script CreateScript(string scriptName)
        {
            Script script = new Script(CoreModules.Preset_SoftSandbox | CoreModules.LoadMethods);

            //set script loader
            UnityAssetsScriptLoader scriptLoader = new UnityAssetsScriptLoader();

            //we need to set the module path to '?' for it tor load the resource correctly
            scriptLoader.ModulePaths = new string[] { "?" };
            script.Options.ScriptLoader = scriptLoader;

            //set globals
            foreach (var global in globals)
                script.Globals[global.Key] = global.Value;

            script.Options.DebugPrint = s => Debug.Log(s);

            //add constants
            script.Globals[scriptNameKey] = Path.GetFileNameWithoutExtension(scriptName);
            script.Globals[scriptDirKey] = LuaEnvironment.ScriptsDirectory + Path.DirectorySeparatorChar;
            script.Globals[dataDirKey] = LuaEnvironment.DataDirectory + Path.DirectorySeparatorChar;


            LoadLuaModulesForScript(script);

            return script;
        }

        private void LoadScript(string path)
        {
            string scriptName = Path.GetFileNameWithoutExtension(path);
            Script script = CreateScript(scriptName);

            using Stream fileStream = new FileStream(path, FileMode.Open);

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

        private void GetGlobals()
        {
            globals = new Dictionary<string, object>();
            //UserData.RegisterProxyType<Image, Texture2D>(v =>
            //{
            //    var proxy = new Image();
            //    proxy.SetTarget(v);
            //    return proxy;
            //});

            //UserData.RegisterProxyType<Feature, Fab.Geo.Feature>(v =>
            //{
            //    var proxy = new Feature();
            //    proxy.SetTarget(v);
            //    return proxy;
            //});

            LuaObjectRegistry.InitalizeLuaObjects(globals);
        }

        private void LoadLuaModulesForScript(Script script)
        {
            //load all modules
            for (int i = 0; i < luaModules.Length; i++)
            {
                script.DoString($"require \'{luaModules[i].name}\'");
            }
        }

        #region Commands

        [RegisterCommand(command_name: "list_scripts", Help = "Lists all loaded lua scripts")]
        private static void Command_List_Scripts(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            LuaManager manager = FindObjectOfType<LuaManager>();
            if (manager)
                foreach (string script in manager.GetAllLoadedScripts())
                    Terminal.Log(TerminalLogType.ShellMessage, script);
        }

        [RegisterCommand(command_name: "unload_scripts", Help = "Unloads all lua scripts")]
        private static void Command_Unload_Scripts(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            LuaManager manager = FindObjectOfType<LuaManager>();
            if (manager)
                manager.UnloadAllScripts();
        }

        [RegisterCommand(command_name: "unload_script", Help = "Unloads a lua scripts", MaxArgCount = 1, MinArgCount = 1)]
        private static void Command_Unload_Script(CommandArg[] args)
        {
            string scriptName = args[0].String;

            if (Terminal.IssuedError) return;

            LuaManager manager = FindObjectOfType<LuaManager>();
            if (manager)
            {
                Script script = manager.loadedScripts.FirstOrDefault(s => (string)s.Globals[scriptNameKey] == scriptName);
                if (script != null)
                    manager.UnloadScript(script);
            }
        }

        #endregion
    }
}
